# 🪟 راهنمای کامپایل پروژه برای ویندوز (Build Guide)

این راهنما به شما کمک می‌کند تا نسخه ویندوزی پروژه را از روی سورس‌کد کامپایل کنید.

## 📋 پیش‌نیازها
- سیستم عامل Windows (نسخه ۶۴ بیتی)
- نصب بودن **.NET 8.0 SDK** (قابل دانلود از سایت مایکروسافت)

برای اطمینان از نصب بودن .NET، در محیط خط فرمان دستور زیر را وارد کنید:
`dotnet --version`

## 🔨 مراحل بیلد و کامپایل

۱. ابتدا وارد پوشه `windows` شوید:
`cd windows`

۲. دریافت وابستگی‌ها:
`dotnet restore`

۳. ساخت نسخه نهایی و مستقل:
`dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish_final`
