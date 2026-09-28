using System;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace SSHProxyManager
{
    public partial class MainWindow : Window
    {
        private SshClient? sshClient;
        private ForwardedPortDynamic? portForward;
        private CancellationTokenSource? pingCts;
        private AppConfig config = new();
        private readonly string configPath;

        public MainWindow()
        {
            InitializeComponent();
            configPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
            LoadConfig();
            StartPingMonitor();
        }

        // این متد تضمین می‌کند که لاگ فقط بعد از رسم کامل پنجره نوشته می‌شود
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Log("🚀 Application started successfully. Ready to connect.");
        }

        private void LoadConfig()
        {
            if (File.Exists(configPath))
            {
                try
                {
                    var json = File.ReadAllText(configPath);
                    config = JsonSerializer.Deserialize<AppConfig>(json) ?? new AppConfig();
                }
                catch { config = new AppConfig(); }
            }

            txtIP.Text = config.IP;
            txtUsername.Text = config.Username;
            pwdPassword.Password = config.Password;
            txtPort.Text = config.Port.ToString();
        }

        private void SaveConfig()
        {
            config.IP = txtIP.Text.Trim();
            config.Username = txtUsername.Text.Trim();
            config.Password = pwdPassword.Password;
            config.Port = int.TryParse(txtPort.Text, out int port) ? port : 10808;

            var json = JsonSerializer.Serialize(config, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(configPath, json);
        }

        private void ShowProxyLogs_Click(object sender, RoutedEventArgs e)
        {
            MainView.Visibility = Visibility.Collapsed;
            ProxyView.Visibility = Visibility.Visible;
        }

        private void BackToMain_Click(object sender, RoutedEventArgs e)
        {
            ProxyView.Visibility = Visibility.Collapsed;
            MainView.Visibility = Visibility.Visible;
        }

        private async void BtnPing_Click(object sender, RoutedEventArgs e)
        {
            await CheckPingAsync(true);
        }

        private async Task<bool> CheckPingAsync(bool isManual = false)
        {
            string ip = txtIP.Text.Trim();
            if (string.IsNullOrWhiteSpace(ip))
            {
                if (isManual) Log("⚠️ Please enter an IP or Domain first.");
                return false;
            }

            if (!isManual) Log($"🔄 Pinging {ip} before connection...");

            try
            {
                using var ping = new Ping();
                var reply = ping.Send(ip, 3000);

                if (reply.Status == IPStatus.Success)
                {
                    UpdatePingUI(reply.RoundtripTime, true);
                    if (!isManual) Log($"✅ Ping successful: {reply.RoundtripTime} ms");
                    return true;
                }
                else
                {
                    UpdatePingUI(-1, false);
                    if (!isManual) Log($"⚠️ Ping failed ({reply.Status}). ICMP may be blocked, attempting connection anyway...");
                    return false;
                }
            }
            catch (Exception ex)
            {
                UpdatePingUI(-1, false);
                if (!isManual) Log($"⚠️ Ping error: {ex.Message}. Attempting connection anyway...");
                return false;
            }
        }

        private void UpdatePingUI(long roundtripTime, bool success)
        {
            Dispatcher.Invoke(() =>
            {
                if (success)
                {
                    txtPing.Text = $"📶 Ping: {roundtripTime} ms";
                    txtPing.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"));
                }
                else
                {
                    txtPing.Text = "📶 Ping: Unreachable/Blocked";
                    txtPing.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#F44336"));
                }
            });
        }

        private async void BtnConnect_Click(object sender, RoutedEventArgs e)
        {
            if (sshClient != null && sshClient.IsConnected)
            {
                await DisconnectAsync();
            }
            else
            {
                await ConnectAsync();
            }
        }

        private async Task ConnectAsync()
        {
            SaveConfig();

            if (string.IsNullOrWhiteSpace(txtIP.Text) || string.IsNullOrWhiteSpace(txtUsername.Text))
            {
                Log("❌ Error: IP and Username are required!");
                return;
            }

            try
            {
                btnConnect.IsEnabled = false;
                await CheckPingAsync(false);
                Log($"🔄 Connecting to {txtUsername.Text}@{txtIP.Text}...");

                var connectionInfo = new ConnectionInfo(
                    txtIP.Text,
                    22,
                    txtUsername.Text,
                    new PasswordAuthenticationMethod(txtUsername.Text, pwdPassword.Password)
                );

                sshClient = new SshClient(connectionInfo);
                await Task.Run(() => sshClient.Connect());

                portForward = new ForwardedPortDynamic("127.0.0.1", (uint)config.Port);

                portForward.Exception += (sender, e) =>
                {
                    LogProxy($"⚠️ Proxy Error: {e.Exception.Message}");
                };

                portForward.RequestReceived += (sender, e) =>
                {
                    LogProxy($"🔗 Local app connected to proxy from: {e.OriginatorHost}:{e.OriginatorPort}");
                };

                sshClient.AddForwardedPort(portForward);
                portForward.Start();

                UpdateStatus(true);
                Log($"✅ Connected! SSH tunnel established.");
                LogProxy($"🟢 SOCKS5 Proxy started on 127.0.0.1:{config.Port}");
                LogProxy("ℹ️ Technical Note: SOCKS5 is a Layer-4 (TCP) blind tunnel. It logs local app connections, but cannot read encrypted HTTPS payloads.");
            }
            catch (Exception ex)
            {
                Log($"❌ Connection failed: {ex.Message}");
                UpdateStatus(false);
            }
            finally
            {
                btnConnect.IsEnabled = true;
            }
        }

        private async Task DisconnectAsync()
        {
            try
            {
                Log("🔌 Disconnecting...");
                LogProxy("🔴 SOCKS5 Proxy stopped.");

                portForward?.Stop();
                sshClient?.Disconnect();
                sshClient?.Dispose();
                sshClient = null;
                portForward = null;

                UpdateStatus(false);
                Log("✅ Disconnected.");
            }
            catch (Exception ex)
            {
                Log($"❌ Error during disconnect: {ex.Message}");
            }
        }

        private void UpdateStatus(bool connected)
        {
            if (connected)
            {
                statusIndicator.Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E676"));
                txtStatus.Text = "Connected";
                btnConnect.Content = "⏹ Disconnect";
                btnConnect.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D32F2F"));
            }
            else
            {
                statusIndicator.Fill = new SolidColorBrush(Colors.Red);
                txtStatus.Text = "Disconnected";
                btnConnect.Content = "▶ Connect";
                btnConnect.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0E7A0D"));
            }
        }

        private void StartPingMonitor()
        {
            pingCts = new CancellationTokenSource();
            Task.Run(async () =>
            {
                while (!pingCts.Token.IsCancellationRequested)
                {
                    if (!string.IsNullOrWhiteSpace(txtIP.Text) && sshClient != null && sshClient.IsConnected)
                    {
                        await CheckPingAsync(false);
                    }
                    await Task.Delay(5000, pingCts.Token);
                }
            }, pingCts.Token);
        }

        private void Log(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                txtLog.ScrollToEnd();
            });
        }

        private void LogProxy(string message)
        {
            Dispatcher.Invoke(() =>
            {
                txtProxyLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}\n");
                txtProxyLog.ScrollToEnd();
            });
        }

        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            txtLog.Clear();
            Log("🧹 Log cleared by user.");
        }

        private void BtnClearProxyLog_Click(object sender, RoutedEventArgs e)
        {
            txtProxyLog.Clear();
            LogProxy("🧹 Proxy log cleared by user.");
        }

        protected override void OnClosed(EventArgs e)
        {
            pingCts?.Cancel();
            portForward?.Stop();
            sshClient?.Disconnect();
            sshClient?.Dispose();
            base.OnClosed(e);
        }
    }

    public class AppConfig
    {
        public string IP { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public int Port { get; set; } = 10808;
    }
}