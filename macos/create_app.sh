#!/bin/bash
# ═══════════════════════════════════════════════
#  SSH Proxy Manager - macOS App Bundle Builder
#  Developed by: Mohammad Hossein Soleymani (MHS)
# ═══════════════════════════════════════════════

set -e

APP_NAME="SSHProxyManager"
APP_DISPLAY_NAME="SSH Proxy Manager"
VERSION="1.0.0"
BUNDLE_ID="com.mhs.sshproxymanager"
ARCH="${1:-osx-arm64}"

echo "🔨 Building for $ARCH..."

# ۱. بیلد با دات‌نت (بدون PublishSingleFile به دلیل مشکلات SkiaSharp در مک)
dotnet publish \
  -c Release \
  -r "$ARCH" \
  --self-contained true \
  -o publish_temp

echo "📦 Creating .app bundle..."

# ۲. ساخت ساختار .app
rm -rf "$APP_NAME.app"
mkdir -p "$APP_NAME.app/Contents/MacOS"
mkdir -p "$APP_NAME.app/Contents/Resources"

# ۳. کپی تمام فایل‌های اجرایی و کتابخانه‌ها (DLL و dylib)
cp -R publish_temp/* "$APP_NAME.app/Contents/MacOS/"
chmod +x "$APP_NAME.app/Contents/MacOS/$APP_NAME"

# ۴. کپی آیکون
if [ -f "icon.icns" ]; then
  cp icon.icns "$APP_NAME.app/Contents/Resources/icon.icns"
  echo "🎨 Icon copied."
else
  echo "⚠️  icon.icns not found, skipping icon."
fi

# ۵. ساخت Info.plist
cat > "$APP_NAME.app/Contents/Info.plist" << PLIST
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN"
  "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleExecutable</key>
    <string>$APP_NAME</string>
    <key>CFBundleIconFile</key>
    <string>icon.icns</string>
    <key>CFBundleIdentifier</key>
    <string>$BUNDLE_ID</string>
    <key>CFBundleName</key>
    <string>$APP_DISPLAY_NAME</string>
    <key>CFBundleDisplayName</key>
    <string>$APP_DISPLAY_NAME</string>
    <key>CFBundleVersion</key>
    <string>$VERSION</string>
    <key>CFBundleShortVersionString</key>
    <string>$VERSION</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>LSMinimumSystemVersion</key>
    <string>10.13</string>
    <key>NSHighResolutionCapable</key>
    <true/>
    <key>NSAppTransportSecurity</key>
    <dict>
        <key>NSAllowsArbitraryLoads</key>
        <true/>
    </dict>
</dict>
</plist>
PLIST