#!/usr/bin/env bash
# 發佈腳本：產出 artifacts/release/ 下的發佈檔（每個平台一個 zip）。
# 用法：scripts/publish-release.sh [--version x.y.z] [--only win|mac]
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
cd "$ROOT"

PROJECT="src/TarotMcp.Server"

VERSION=""
ONLY=""
while [[ $# -gt 0 ]]; do
  case "$1" in
    --version) VERSION="${2:?--version 需要參數}"; shift 2 ;;
    --only) ONLY="${2:?--only 需要參數}"; shift 2 ;;
    -h|--help) sed -n '2,3p' "$0"; exit 0 ;;
    *) echo "未知參數：$1" >&2; exit 2 ;;
  esac
done

if [[ -n "$ONLY" && "$ONLY" != "win" && "$ONLY" != "mac" ]]; then
  echo "--only 只能是 win 或 mac" >&2
  exit 2
fi

if [[ -z "$VERSION" ]]; then
  TAG="$(git describe --tags --abbrev=0 2>/dev/null || true)"
  if [[ -n "$TAG" ]]; then
    VERSION="$TAG"
  else
    VERSION="$(sed -n 's:.*<Version>\(.*\)</Version>.*:\1:p' "$PROJECT/TarotMcp.Server.csproj" | head -n 1)"
  fi
fi
VERSION="${VERSION#v}"
[[ -n "$VERSION" ]] || { echo "無法取得版本號" >&2; exit 1; }

OUT="$ROOT/artifacts/release"
rm -rf "$OUT"
mkdir -p "$OUT"

echo "發佈版本：$VERSION"

# 單一檔案、自包含、不裁剪（MCP 與 JSON 用到反射，裁剪不安全）。
# 不開單檔壓縮：內嵌的牌面插畫約 45 MB，壓縮會讓每次啟動都要解壓，MCP 客戶端啟動 server 會變慢。
publish_rid() {
  local rid="$1"
  dotnet publish "$PROJECT" -c Release -r "$rid" --self-contained \
    -p:PublishSingleFile=true \
    -p:IncludeNativeLibrariesForSelfExtract=true \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=false \
    -p:DebugType=none -p:DebugSymbols=false \
    -p:Version="$VERSION" \
    -o "artifacts/publish/$rid" --nologo
}

if [[ -z "$ONLY" || "$ONLY" == "win" ]]; then
  echo "==> 發佈 Windows x64"
  publish_rid win-x64
  # zip 檔名帶版本號，裡面的執行檔固定叫 TarotMcp.exe（更新後路徑不變，MCP 客戶端設定不用改）
  STAGE="$(mktemp -d)"
  cp "artifacts/publish/win-x64/TarotMcp.exe" "$STAGE/TarotMcp.exe"
  (cd "$STAGE" && zip -q -X "$OUT/TarotMcp-$VERSION-win-x64.zip" TarotMcp.exe)
  rm -rf "$STAGE"
fi

if [[ -z "$ONLY" || "$ONLY" == "mac" ]]; then
  echo "==> 發佈 macOS Apple Silicon"
  publish_rid osx-arm64
  STAGE="$(mktemp -d)"
  cp "artifacts/publish/osx-arm64/TarotMcp" "$STAGE/TarotMcp"
  chmod +x "$STAGE/TarotMcp"
  # Apple Silicon 不接受完全沒簽章的執行檔；ad-hoc 簽章只能在 macOS 上做
  if [[ "$(uname)" == "Darwin" ]]; then
    codesign --force --sign - "$STAGE/TarotMcp"
  else
    echo "警告：目前不是 macOS（$(uname)），macOS 版沒有簽章，在 Apple Silicon 上可能打不開" >&2
  fi
  (cd "$STAGE" && zip -q -X "$OUT/TarotMcp-$VERSION-osx-arm64.zip" TarotMcp)
  rm -rf "$STAGE"
fi

echo "==> artifacts/release/ 內容："
ls -lh "$OUT"
