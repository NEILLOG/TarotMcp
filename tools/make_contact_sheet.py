#!/usr/bin/env python3
"""把 assets/cards/*.webp 拼成一張總覽圖，方便一眼檢查（有沒有出現文字、邊框）。"""
from pathlib import Path

from PIL import Image

ROOT = Path(__file__).resolve().parent.parent
CARDS = sorted((ROOT / "assets" / "cards").glob("*.webp"))
OUT = ROOT / "assets" / "contact-sheet.jpg"

COLS, TW, TH = 13, 140, 240

if not CARDS:
    raise SystemExit("沒有圖片可拼")

rows = -(-len(CARDS) // COLS)
sheet = Image.new("RGB", (COLS * TW, rows * TH), (16, 16, 24))
for i, p in enumerate(CARDS):
    im = Image.open(p).convert("RGB").resize((TW, TH), Image.LANCZOS)
    sheet.paste(im, ((i % COLS) * TW, (i // COLS) * TH))
sheet.save(OUT, "JPEG", quality=85)
print(f"{len(CARDS)} 張 -> {OUT}")
