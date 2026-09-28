# 🐧 راهنمای کامپایل پروژه برای لینوکس (Build Guide)

این راهنما به شما کمک می‌کند نسخه لینوکسی را کامپایل کنید.

## 📋 پیش‌نیازها
- توزیع‌های لینوکس ۶۴ بیتی
- نصب **.NET 8.0 SDK**

برای نصب پیش‌نیازها:
`sudo apt update && sudo apt install -y dotnet-sdk-8.0`

## 🔨 مراحل بیلد و کامپایل

۱. وارد پوشه `linux` شوید:
`cd linux`

۲. دریافت وابستگی‌ها:
`dotnet restore`

۳. کامپایل نهایی و ساخت نسخه مستقل:
`dotnet publish -c Release -r linux-x64 --self-contained true -p:PublishSingleFile=true -o publish_final`
