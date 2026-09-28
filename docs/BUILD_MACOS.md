# 🍎 راهنمای کامپایل پروژه برای مک‌او‌اس (Build Guide)

این راهنما به شما کمک می‌کند نسخه مک‌او‌اس پروژه را کامپایل کنید.

## 📋 پیش‌نیازها
- مک‌او‌اس (Apple Silicon یا Intel)
- نصب **.NET 8.0 SDK** یا بالاتر

برای نصب .NET:
```bash
brew install --cask dotnet-sdk
```

## 🔨 مراحل بیلد و کامپایل

۱. وارد پوشه `macos` شوید:
```bash
cd macos
```

۲. دریافت وابستگی‌ها:
```bash
dotnet restore
```

۳. ساخت آیکون (فقط بار اول - نیاز به `icon.png` با سایز حداقل 512x512):
```bash
chmod +x make_icon.sh
./make_icon.sh
```

۴. کامپایل و ساخت `.app`:
```bash
chmod +x create_app.sh
./create_app.sh osx-arm64    # برای Apple Silicon (M1/M2/M3/M4)
# یا
./create_app.sh osx-x64      # برای Intel Mac
```

۵. رفع قرنطینه و اجرا:
```bash
xattr -d com.apple.quarantine SSHProxyManager.app
# یا اگر ارور داد:
sudo xattr -rd com.apple.quarantine SSHProxyManager.app

open SSHProxyManager.app
```

## 📦 ساخت فایل توزیع (ZIP)
```bash
zip -r SSHProxyManager-macos-arm64-v1.0.0.zip SSHProxyManager.app
```

## ⚠️ نکات مهم
- در مک‌او‌اس به دلیل محدودیت‌های امنیتی (SIP)، امکان ساخت فایل Single File برای اپلیکیشن‌های Avalonia وجود ندارد. به همین دلیل برنامه به صورت یک Bundle استاندارد `.app` ساخته می‌شود.
- اگر هنگام اجرا ارور "app is damaged" دیدید، از این دستور استفاده کنید:
```bash
sudo xattr -rd com.apple.quarantine SSHProxyManager.app
```
