#!/bin/bash
# ═══════════════════════════════════════════
#  تبدیل icon.png به icon.icns برای مک‌او‌اس
# ═══════════════════════════════════════════

INPUT="icon.png"
ICONSET="icon.iconset"

if [ ! -f "$INPUT" ]; then
    echo "❌ فایل $INPUT پیدا نشد!"
    exit 1
fi

echo "📐 بررسی سایز آیکون..."
WIDTH=$(sips -g pixelWidth "$INPUT" | grep -oE '[0-9]+$')
HEIGHT=$(sips -g pixelHeight "$INPUT" | grep -oE '[0-9]+$')
echo "   سایز فعلی: ${WIDTH}x${HEIGHT}"

if [ "$WIDTH" -lt 1024 ] || [ "$HEIGHT" -lt 1024 ]; then
    echo "⚠️  آیکون کوچکتر از 1024x1024 است."
    echo "   آیکون به 1024x1024 تغییر سایز می‌دهد (ممکن است کیفیت کم شود)."
    echo ""
    read -p "   ادامه می‌دهید؟ (y/n): " -n 1 -r
    echo ""
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "❌ لغو شد."
        exit 1
    fi
fi

rm -rf "$ICONSET"
mkdir "$ICONSET"

echo "🎨 ساخت سایزهای مختلف آیکون..."

sips -z 16 16     "$INPUT" --out "$ICONSET/icon_16x16.png"       > /dev/null 2>&1
sips -z 32 32     "$INPUT" --out "$ICONSET/icon_16x16@2x.png"    > /dev/null 2>&1
sips -z 32 32     "$INPUT" --out "$ICONSET/icon_32x32.png"       > /dev/null 2>&1
sips -z 64 64     "$INPUT" --out "$ICONSET/icon_32x32@2x.png"    > /dev/null 2>&1
sips -z 128 128   "$INPUT" --out "$ICONSET/icon_128x128.png"     > /dev/null 2>&1
sips -z 256 256   "$INPUT" --out "$ICONSET/icon_128x128@2x.png"  > /dev/null 2>&1
sips -z 256 256   "$INPUT" --out "$ICONSET/icon_256x256.png"     > /dev/null 2>&1
sips -z 512 512   "$INPUT" --out "$ICONSET/icon_256x256@2x.png"  > /dev/null 2>&1
sips -z 512 512   "$INPUT" --out "$ICONSET/icon_512x512.png"     > /dev/null 2>&1
sips -z 1024 1024 "$INPUT" --out "$ICONSET/icon_512x512@2x.png"  > /dev/null 2>&1

echo "📦 ساخت فایل نهایی .icns..."
iconutil -c icns "$ICONSET" -o icon.icns

rm -rf "$ICONSET"

if [ -f "icon.icns" ]; then
    echo ""
    echo "✅ فایل icon.icns با موفقیت ساخته شد!"
    ls -lh icon.icns
else
    echo "❌ خطا در ساخت فایل .icns"
    exit 1
fi
EOF