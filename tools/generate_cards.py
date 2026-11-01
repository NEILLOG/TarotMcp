#!/usr/bin/env python3
"""依 plan/image-prompts.md 批次產生 78 張塔羅牌圖片。

流程：解析提示詞 -> 呼叫 images/generations -> 裁成 7:12 -> 存成 webp。
特色：已存在的檔案自動略過（可續跑）、429/5xx 自動重試、
可用環境變數限制張數與併發。

環境變數：
  IMAGE_API_KEY      (必填) API token
  IMAGE_API_URL      預設 https://api.openai.com/v1/images/generations
  IMAGE_MODEL        預設 gpt-image-2.5-flare
  IMAGE_SIZES        逗號分隔的候選尺寸，依序嘗試，遇到「尺寸不支援」就換下一個
                     預設 1008x1728,1024x1792,1024x1536
  IMAGE_QUALITY      選填，原樣傳給 API（例如 low / medium / high）
  CARD_ONLY          選填，只跑指定的卡（逗號分隔，例如 major_02,cups_01）
  CARD_SKIP          選填，額外略過的卡（逗號分隔）
  CARD_LIMIT         選填，最多本次實際呼叫 API 的張數
  CARD_CONCURRENCY   預設 3
  OUT_W / OUT_H      輸出尺寸，預設 1400x2400（7:12）
"""
from __future__ import annotations

import argparse
import base64
import io
import json
import os
import re
import sys
import threading
import time
from concurrent.futures import ThreadPoolExecutor, as_completed
from pathlib import Path

import requests
from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
PROMPTS_MD = ROOT / "plan" / "image-prompts.md"
OUT_DIR = ROOT / "assets" / "cards"
LOG_PATH = ROOT / "assets" / "generation-log.jsonl"

HEADING_RE = re.compile(r"^###\s+([a-z]+_\d{2})\s")
FENCE_RE = re.compile(r"```text\n(.*?)\n```", re.S)


def parse_prompts(md: str) -> tuple[str, list[tuple[str, str]]]:
    """回傳 (風格前綴, [(card_id, scene), ...])。"""
    prefix_pos = md.index("## 風格前綴")
    scenes_pos = md.index("## 各牌場景描述")
    prefix_m = FENCE_RE.search(md[prefix_pos:scenes_pos])
    if not prefix_m:
        raise SystemExit("找不到風格前綴區塊")
    prefix = prefix_m.group(1).strip()

    cards: list[tuple[str, str]] = []
    body = md[scenes_pos:]
    # 以標題切段，每段取第一個 text 區塊
    parts = re.split(r"(?m)^(?=### )", body)
    for part in parts:
        first_line = part.splitlines()[0] if part.strip() else ""
        m = HEADING_RE.match(first_line)
        if not m:
            continue
        f = FENCE_RE.search(part)
        if not f:
            raise SystemExit(f"{m.group(1)} 缺少場景描述區塊")
        cards.append((m.group(1), f.group(1).strip()))
    return prefix, cards


def csv_env(name: str) -> set[str]:
    return {x.strip() for x in os.environ.get(name, "").split(",") if x.strip()}


class Generator:
    def __init__(self) -> None:
        self.key = os.environ["IMAGE_API_KEY"]
        self.url = os.environ.get(
            "IMAGE_API_URL", "https://api.openai.com/v1/images/generations"
        )
        self.model = os.environ.get("IMAGE_MODEL", "gpt-image-2.5-flare")
        self.sizes = [
            s.strip()
            for s in os.environ.get(
                "IMAGE_SIZES", "1008x1728,1024x1792,1024x1536"
            ).split(",")
            if s.strip()
        ]
        self.quality = os.environ.get("IMAGE_QUALITY", "").strip()
        w, h = os.environ.get("OUT_W", "1400"), os.environ.get("OUT_H", "2400")
        self.out_w, self.out_h = int(w), int(h)
        self.size_lock = threading.Lock()
        self.size_idx = 0  # 找到可用尺寸後所有執行緒共用

    def _request(self, prompt: str, size: str) -> requests.Response:
        payload = {"model": self.model, "prompt": prompt, "size": size, "n": 1}
        if self.quality:
            payload["quality"] = self.quality
        return requests.post(
            self.url,
            headers={"Authorization": f"Bearer {self.key}"},
            json=payload,
            timeout=300,
        )

    def generate(self, prompt: str) -> bytes:
        last_err = ""
        for attempt in range(6):
            with self.size_lock:
                size = self.sizes[self.size_idx]
            try:
                r = self._request(prompt, size)
            except requests.RequestException as e:
                last_err = f"network: {e}"
                time.sleep(min(60, 2 ** attempt * 3))
                continue

            if r.status_code == 200:
                item = r.json()["data"][0]
                if item.get("b64_json"):
                    return base64.b64decode(item["b64_json"])
                if item.get("url"):
                    return requests.get(item["url"], timeout=120).content
                raise RuntimeError(f"回應中沒有圖片資料: {str(item)[:200]}")

            last_err = f"HTTP {r.status_code}: {r.text[:300]}"
            low = r.text.lower()
            if r.status_code == 400 and "size" in low:
                with self.size_lock:
                    if self.sizes[self.size_idx] == size:
                        if self.size_idx + 1 >= len(self.sizes):
                            raise RuntimeError(f"所有候選尺寸都不被接受: {last_err}")
                        self.size_idx += 1
                        print(f"[size] {size} 不支援，改用 {self.sizes[self.size_idx]}", flush=True)
                continue
            if r.status_code in (401, 403, 404):
                raise RuntimeError(f"致命錯誤，停止: {last_err}")
            if r.status_code in (429, 500, 502, 503, 504):
                wait = float(r.headers.get("retry-after", 0)) or min(90, 2 ** attempt * 5)
                time.sleep(wait)
                continue
            raise RuntimeError(last_err)  # 其他 4xx（例如內容審查）不重試
        raise RuntimeError(f"重試次數用盡: {last_err}")

    def to_card(self, raw: bytes) -> Image.Image:
        """置中裁切成 7:12 再縮放到輸出尺寸。"""
        im = Image.open(io.BytesIO(raw)).convert("RGB")
        target = self.out_w / self.out_h
        w, h = im.size
        if w / h > target:  # 太寬 -> 裁左右
            nw = round(h * target)
            left = (w - nw) // 2
            im = im.crop((left, 0, left + nw, h))
        else:  # 太高 -> 裁上下
            nh = round(w / target)
            top = (h - nh) // 2
            im = im.crop((0, top, w, top + nh))
        return im.resize((self.out_w, self.out_h), Image.LANCZOS)


def main() -> int:
    ap = argparse.ArgumentParser()
    ap.add_argument("--dry-run", action="store_true", help="只解析並列出，不呼叫 API")
    args = ap.parse_args()

    prefix, cards = parse_prompts(PROMPTS_MD.read_text(encoding="utf-8"))
    ids = [c for c, _ in cards]
    if len(ids) != len(set(ids)):
        raise SystemExit("卡片 id 有重複")

    only, skip = csv_env("CARD_ONLY"), csv_env("CARD_SKIP")
    unknown = (only | skip) - set(ids)
    if unknown:
        raise SystemExit(f"未知的卡片 id: {sorted(unknown)}")

    OUT_DIR.mkdir(parents=True, exist_ok=True)
    todo = []
    for cid, scene in cards:
        if only and cid not in only:
            continue
        if cid in skip or (OUT_DIR / f"{cid}.webp").exists():
            continue
        todo.append((cid, f"{prefix}\n\n{scene}"))

    limit = int(os.environ.get("CARD_LIMIT", "0") or 0)
    if limit > 0:
        todo = todo[:limit]

    print(f"解析到 {len(cards)} 張；本次待生成 {len(todo)} 張", flush=True)
    if args.dry_run:
        for cid, p in todo:
            print(f"  {cid}  ({len(p)} 字元)")
        return 0
    if not todo:
        return 0

    gen = Generator()
    conc = max(1, int(os.environ.get("CARD_CONCURRENCY", "3")))
    log_lock = threading.Lock()
    failed: list[str] = []

    def work(cid: str, prompt: str) -> str:
        t0 = time.time()
        raw = gen.generate(prompt)
        gen.to_card(raw).save(OUT_DIR / f"{cid}.webp", "WEBP", quality=88, method=6)
        with log_lock, LOG_PATH.open("a", encoding="utf-8") as f:
            f.write(json.dumps({
                "card": cid, "model": gen.model,
                "size": gen.sizes[gen.size_idx],
                "seconds": round(time.time() - t0, 1),
                "ts": time.strftime("%Y-%m-%dT%H:%M:%SZ", time.gmtime()),
            }, ensure_ascii=False) + "\n")
        return cid

    with ThreadPoolExecutor(max_workers=conc) as ex:
        futs = {ex.submit(work, c, p): c for c, p in todo}
        for fut in as_completed(futs):
            cid = futs[fut]
            try:
                fut.result()
                print(f"[ok]   {cid}", flush=True)
            except Exception as e:  # noqa: BLE001
                failed.append(cid)
                print(f"[fail] {cid}: {e}", flush=True)

    if failed:
        print(f"失敗 {len(failed)} 張: {','.join(sorted(failed))}", flush=True)
        return 1
    return 0


if __name__ == "__main__":
    sys.exit(main())
