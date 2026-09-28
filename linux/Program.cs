using Avalonia;
using System;

namespace SSHProxyManager
{
    internal class Program
    {
        // نقطه شروع اجرای برنامه (Entry Point)
        [STAThread]
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        // تنظیمات Avalonia (این بخش را تغییر ندهید)
        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .LogToTrace();
    }
}
