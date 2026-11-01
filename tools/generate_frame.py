#!/usr/bin/env python3
"""依 plan/image-prompts.md「一、邊框範本」產生共用邊框的候選圖，並把洋紅窗口去背、量出窗口座標。

流程：解析提示詞 -> 呼叫 images/generations -> 裁成 7:12 -> 洋紅窗口去背 -> 存成候選。
候選放在 assets/frame/candidates/frame_NN.png ＋ frame_NN.json（窗口座標），人工挑一張後用 FRAME_PICK 轉正。
已存在的候選自動略過（可續跑）；API 呼叫沿用 generate_cards.py 的 Generator（重試、尺寸候選、7:12 裁切）。

環境變數（API 相關同 generate_cards.py）：
  IMAGE_API_KEY      產生候選時必填
  FRAME_COUNT        候選張數，預設 3
  FRAME_PICK         選填，例如 2：把 candidates/frame_02 複製成 assets/frame/frame.png / frame.json
"""
from __future__ import annotations

import json
import os
import re
import shutil
import sys
from pathlib import Path

from PIL import Image, ImageChops, ImageFilter

from generate_cards import Generator, PROMPTS_MD, ROOT

FRAME_DIR = ROOT / "assets" / "frame"
CAND_DIR = FRAME_DIR / "candidates"
FENCE_RE = re.compile(r"```text\n(.*?)\n```", re.S)


def parse_frame_prompt(md: str) -> str:
    start = md.index("## 一、邊框範本")
    end = md.index("## 二、插畫")
    m = FENCE_RE.search(md[start:end])
    if not m:
        raise SystemExit("找不到邊框範本提示詞區塊")
    return m.group(1).strip()


def key_out_magenta(im: Image.Image) -> tuple[Image.Image, dict, list[str]]:
    """洋紅窗口去背成透明，回傳 (RGBA, window, 警告)。"""
    im = im.convert("RGB")
    r, g, b = im.split()
    high = lambda band: band.point(lambda v: 255 if v > 180 else 0)  # noqa: E731
    low = lambda band: band.point(lambda v: 255 if v < 100 else 0)  # noqa: E731
    mask = ImageChops.darker(ImageChops.darker(high(r), high(b)), low(g))
    bbox = mask.getbbox()
    if bbox is None:
        raise RuntimeError("找不到洋紅窗口")

    warnings: list[str] = []
    area = (bbox[2] - bbox[0]) * (bbox[3] - bbox[1])
    coverage = mask.crop(bbox).histogram()[255] / area
    if coverage < 0.97:
        warnings.append(f"窗口洋紅只佔外框 {coverage:.0%}（有漸層、雜點或窗口外的洋紅），建議重畫")

    # 邊緣的洋紅反鋸齒殘邊：外擴 2px 再去背，露出的底色填黑
    grown = mask.filter(ImageFilter.MaxFilter(5))
    im.paste((0, 0, 0), mask=grown)
    rgba = im.convert("RGBA")
    rgba.putalpha(ImageChops.invert(grown))

    x0, y0, x1, y1 = grown.getbbox()
    window = {"x": x0, "y": y0, "width": x1 - x0, "height": y1 - y0}
    w, h = im.size
    if not 0.75 <= window["width"] / w <= 0.97:
        warnings.append(f"窗口寬度佔 {window['width'] / w:.0%}，與預期的約 90% 差很多")
    if not 0.65 <= window["height"] / h <= 0.85:
        warnings.append(f"窗口高度佔 {window['height'] / h:.0%}，與預期的約 78% 差很多")
    return rgba, window, warnings


def main() -> int:
    count = int(os.environ.get("FRAME_COUNT", "3") or 0)
    pick = os.environ.get("FRAME_PICK", "").strip()
    failed = 0

    CAND_DIR.mkdir(parents=True, exist_ok=True)
    todo = [n for n in range(1, count + 1) if not (CAND_DIR / f"frame_{n:02d}.png").exists()]
    print(f"邊框候選：要 {count} 張，本次待生成 {len(todo)} 張", flush=True)

    if todo:
        prompt = parse_frame_prompt(PROMPTS_MD.read_text(encoding="utf-8"))
        gen = Generator()
        for n in todo:
            name = f"frame_{n:02d}"
            try:
                card = gen.to_card(gen.generate(prompt))
                rgba, window, warnings = key_out_magenta(card)
                rgba.save(CAND_DIR / f"{name}.png", "PNG", optimize=True)
                (CAND_DIR / f"{name}.json").write_text(
                    json.dumps({"size": {"width": rgba.width, "height": rgba.height},
                                "window": window}, ensure_ascii=False, indent=2) + "\n",
                    encoding="utf-8",
                )
                print(f"[ok]   {name}  window={window}", flush=True)
                for w in warnings:
                    print(f"[warn] {name}: {w}", flush=True)
            except Exception as e:  # noqa: BLE001
                failed += 1
                print(f"[fail] {name}: {e}", flush=True)

    if pick:
        name = f"frame_{int(pick):02d}"
        src_png, src_json = CAND_DIR / f"{name}.png", CAND_DIR / f"{name}.json"
        if not (src_png.exists() and src_json.exists()):
            print(f"[fail] FRAME_PICK={pick}：找不到候選 {name}", flush=True)
            return 1
        shutil.copyfile(src_png, FRAME_DIR / "frame.png")
        shutil.copyfile(src_json, FRAME_DIR / "frame.json")
        print(f"[ok]   {name} 已複製為 assets/frame/frame.png / frame.json", flush=True)

    return 1 if failed else 0


if __name__ == "__main__":
    sys.exit(main())
