using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.IO;
using System.Reflection;
using Microsoft.Web.WebView2.Wpf;
using System.Windows.Markup;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Interop;
using Tungsten.Controls;
using System.Globalization;
using System.Windows.Media;
using System.Collections.Generic;
using Tungsten.Settings;
using System.Threading;
using System.Diagnostics;
using System.IO.Compression;

namespace Tungsten
{
    public partial class MainWindow : Window
    {
        public ScrollViewer TabScroller;
        public string ApplicationPath = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        public string PipeName;
        public string DLLPath;

        public T GetTemplateItem<T>(Control elem, string name)
        {
            return elem.Template.FindName(name, elem) is T name1 ? name1 : default;
        }

        public MainWindow()
        {
            InitializeComponent();
            if (!Directory.Exists(ApplicationPath + "\\bin"))
                Directory.CreateDirectory(ApplicationPath + "\\bin");

            if (!Directory.Exists(ApplicationPath + "\\bin\\Monaco"))
            {
                Directory.CreateDirectory(ApplicationPath + "\\bin\\Monaco");
                try
                {
                    File.WriteAllBytes(ApplicationPath + "\\Monaco.zip", Properties.Resources.Monaco);
                    ZipFile.ExtractToDirectory(ApplicationPath + "\\Monaco.zip", ApplicationPath + "\\bin\\Monaco");
                    File.Delete(ApplicationPath + "\\Monaco.zip");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            Tabs.MakeTab();

            bool autoInject = false;
            int delay = 5;
            Thread thread = new Thread(() =>
            {
                while (true)
                {
                    Thread.Sleep(1000);
                    if (autoInject && Process.GetProcessesByName("RobloxPlayerBeta").Length > 0)
                    {
                        Dispatcher.Invoke(() =>
                        {
                            LogOutput($"Roblox Process Detected, Auto Injecting in {delay}s");
                        });
                        Thread.Sleep(delay * 1000);
                        Dispatcher.Invoke(Inject);
                    }
                    while (Process.GetProcessesByName("RobloxPlayerBeta").Length > 0)
                        Thread.Sleep(1000);
                }
            });
            thread.Start();
            Application.Current.Exit += (s, e) =>
            {
                thread.Abort();
            };

            StringSetting pipeSetting = new StringSetting("Pipe Name", "pipename", "CarbonAPI", onChange: (value) =>
            {
                PipeName = value;
            });
            FilePathSetting filePathSetting = new FilePathSetting("DLL Path", "dllpath", ApplicationPath + "\\CarbonAPI.dll", onChange: (value) =>
            {
                DLLPath = value;
            });

            SettingsMenu.AddSettingPage("API", new List<Setting>
            {
                new DropdownSetting("DLL", "dll", new List<string>
                {
                    "WeAreDevs",
                    "CarbonAPI",
                    "Custom",
                }, "CarbonAPI", (value) =>
                {
                    switch (value)
                    {
                        case "WeAreDevs":
                            filePathSetting.SetText(ApplicationPath + "\\exploit-main.dll");
                            pipeSetting.SetText("WeAreDevsPublicAPI_Lua");
                            pipeSetting.SetEnabled(false);
                            filePathSetting.SetEnabled(false);
                            break;
                        case "CarbonAPI":
                            filePathSetting.SetText(ApplicationPath + "\\CarbonAPI.dll");
                            pipeSetting.SetText("CarbonAPI");
                            pipeSetting.SetEnabled(false);
                            filePathSetting.SetEnabled(false);
                            break;
                        case "Custom":
                            pipeSetting.SetEnabled(true);
                            filePathSetting.SetEnabled(true);
                            break;
                        default:
                            break;
                    }
                }),
                pipeSetting,
                filePathSetting
            });
            SettingsMenu.AddSettingPage("UI", new List<Setting>
            {
                new BooleanSetting("Top Most", "topmost", false, (value) =>
                {
                    Topmost = value;
                })
            });
            SettingsMenu.AddSettingPage("Internal", new List<Setting>
            {
                new BooleanSetting("Auto Inject", "autoinject", false, (value) =>
                {
                    autoInject = value;
                }),
                new NumberSetting("Auto Inject Delay", "autoinjectdelay", delay, 0, 120, 1, (value) =>
                {
                    delay = (int)value;
                })
            });
            SettingsMenu.LoadPage(0);
        }

        public void LogOutput(string message)
        {
            OutputBox.Text += $"[{DateTime.Now:HH:mm:ss}] {message}\n";
            OutputBox.ScrollToEnd();
        }

        #region Tab bullshit

        #endregion

        #region Injection and Execution

        public async void Inject()
        {
            if (Injection.NamedPipeExists(PipeName))
            {
                LogOutput("Named Pipe already exists.");
                return;
            }    

            LogOutput("Injecting...");
            DllInjectionResult result = Injection.Inject(DLLPath, "RobloxPlayerBeta");
            switch (result)
            {
                case DllInjectionResult.GameProcessNotFound:
                    LogOutput("Couldn't find a running Roblox game process.");
                    return;
                case DllInjectionResult.DllNotFound:
                    LogOutput("Couldn't find the DLL, did your antivirus delete it?");
                    return;
                case DllInjectionResult.InjectionFailed:
                    LogOutput("Something went wrong while injecting, maybe restart your game and try again.");
                    return;
                case DllInjectionResult.Success:
                    LogOutput("DLL Initializing...");
                    break;
                default:
                    break;
            }

            while (!Injection.NamedPipeExists(PipeName))
            {
                await Task.Delay(10);
            }
            LogOutput("Injection complete.");
        }

        private void InjectButton_Click(object sender, RoutedEventArgs e)
        {
            Inject();
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Injection.NamedPipeExists(PipeName))
            {
                LogOutput("Named Pipe doesn't exist.");
                return;
            }

            MonacoEditor editor = (MonacoEditor)Tabs.SelectedContent;
            Injection.WriteToPipe(editor.Text, PipeName);
        }

        #endregion

        #region Drop Files

        const int WM_DROPFILES = 0x233;

        [DllImport("shell32.dll")]
        static extern void DragAcceptFiles(IntPtr hwnd, bool fAccept);

        [DllImport("shell32.dll")]
        static extern uint DragQueryFile(IntPtr hDrop, uint iFile, [Out] StringBuilder filename, uint cch);

        [DllImport("shell32.dll")]
        static extern void DragFinish(IntPtr hDrop);

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            var helper = new WindowInteropHelper(this);
            var hwnd = helper.Handle;

            HwndSource source = (HwndSource)PresentationSource.FromVisual(this);
            source.AddHook(WndProc);

            DragAcceptFiles(hwnd, true);
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == WM_DROPFILES)
            {
                handled = true;
                return HandleDropFiles(wParam);
            }

            return IntPtr.Zero;
        }

        private Size MeasureString(string candidate)
        {
            FormattedText formattedText = new FormattedText(
                candidate,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(FontFamily, FontStyle, FontWeight, FontStretch),
                FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(this).PixelsPerDip);

            return new Size(formattedText.Width, formattedText.Height);
        }

        private IntPtr HandleDropFiles(IntPtr hDrop)
        {
            const int MAX_PATH = 260;
            uint count = DragQueryFile(hDrop, 0xFFFFFFFF, null, 0);

            for (uint i = 0; i < count; i++)
            {
                int size = (int)DragQueryFile(hDrop, i, null, 0);

                StringBuilder filename = new StringBuilder(size + 1);
                DragQueryFile(hDrop, i, filename, MAX_PATH);

                string file = filename.ToString();
                string title = Path.GetFileName(file);
                while (MeasureString(title).Width > 70)
                {
                    title = title.Substring(0, title.Length - 1);
                }

                Tabs.MakeTab(File.ReadAllText(file), title);
            }

            DragFinish(hDrop);
            return IntPtr.Zero;
        }

        #endregion

        #region Settings

        public bool SettingsVisibile = false;
        public bool SettingsAnimating = false;

        private async void ToggleSettings_Click(object sender, RoutedEventArgs e)
        {
            if (SettingsAnimating || SettingsAnimating)
                return;

            SettingsVisibile = !SettingsVisibile;
            SettingsMenu.Width = SettingsMenu.ActualWidth;
            WebView2 webView = (WebView2)Tabs.SelectedContent;
            if (SettingsVisibile)
            {
                SettingsAnimating = true;
                Panel.SetZIndex(SettingsMenu, 100);
                Panel.SetZIndex(ScriptHub, 99);
                SettingsMenu.Visibility = Visibility.Visible;
                SettingsMenu.Margin = new Thickness(SettingsMenu.ActualWidth, 0, 0, 0);
                if (webView != null && !ScriptHubVisibile)
                    webView.Margin = new Thickness(0);

                AnimationUtils.ObjectShift(SettingsMenu, new Thickness(SettingsMenu.ActualWidth, 0, 0, 0), new Thickness(0), AnimationUtils.EaseInOut);
                if (webView != null && !ScriptHubVisibile)
                    AnimationUtils.ObjectShift(webView, webView.Margin, new Thickness(0, 0, webView.ActualWidth, 0), AnimationUtils.EaseInOut);
                await Task.Delay(500);

                SettingsMenu.Visibility = Visibility.Visible;
                SettingsMenu.Margin = new Thickness(0);
                if (webView != null && !ScriptHubVisibile)
                    webView.Margin = new Thickness(0, 0, webView.ActualWidth, 0);
                SettingsAnimating = false;

                // If settings is open, close scripthub
                ScriptHub.Visibility = Visibility.Hidden;
                ScriptHubVisibile = false;
            }
            else
            {
                SettingsAnimating = true;
                SettingsMenu.Visibility = Visibility.Visible;
                SettingsMenu.Margin = new Thickness(0);
                if (webView != null && !ScriptHubVisibile)
                    webView.Margin = new Thickness(0, 0, webView.ActualWidth, 0);

                AnimationUtils.ObjectShift(SettingsMenu, new Thickness(0), new Thickness(SettingsMenu.ActualWidth, 0, 0, 0), AnimationUtils.EaseInOut);
                if (webView != null && !ScriptHubVisibile)
                    AnimationUtils.ObjectShift(webView, webView.Margin, new Thickness(0), AnimationUtils.EaseInOut);
                await Task.Delay(500);

                SettingsMenu.Visibility = Visibility.Hidden;
                SettingsMenu.Margin = new Thickness(SettingsMenu.ActualWidth, 0, 0, 0);
                if (webView != null && !ScriptHubVisibile)
                    webView.Margin = new Thickness(0);
                SettingsAnimating = false;
            }
        }

        #endregion

        #region ScriptHub

        public bool ScriptHubVisibile = false;
        public bool ScriptHubAnimating = false;

        private async void ToggleScriptHub_Click(object sender, RoutedEventArgs e)
        {
            if (ScriptHubAnimating || SettingsAnimating)
                return;

            ScriptHubVisibile = !ScriptHubVisibile;
            ScriptHub.Width = ScriptHub.ActualWidth;
            WebView2 webView = (WebView2)Tabs.SelectedContent;
            if (ScriptHubVisibile)
            {
                ScriptHubAnimating = true;
                Panel.SetZIndex(ScriptHub, 100);
                Panel.SetZIndex(SettingsMenu, 99);
                ScriptHub.Visibility = Visibility.Visible;
                ScriptHub.Margin = new Thickness(ScriptHub.ActualWidth, 0, 0, 0);
                if (webView != null && !SettingsVisibile)
                    webView.Margin = new Thickness(0);

                AnimationUtils.ObjectShift(ScriptHub, new Thickness(ScriptHub.ActualWidth, 0, 0, 0), new Thickness(0), AnimationUtils.EaseInOut);
                if (webView != null && !SettingsVisibile)
                    AnimationUtils.ObjectShift(webView, webView.Margin, new Thickness(0, 0, webView.ActualWidth, 0), AnimationUtils.EaseInOut);
                await Task.Delay(500);

                ScriptHub.Visibility = Visibility.Visible;
                ScriptHub.Margin = new Thickness(0);
                if (webView != null && !SettingsVisibile)
                    webView.Margin = new Thickness(0, 0, webView.ActualWidth, 0);
                ScriptHubAnimating = false;

                // If scripthub is open, close settings
                SettingsMenu.Visibility = Visibility.Hidden;
                SettingsVisibile = false;
            }
            else
            {
                ScriptHubAnimating = true;
                ScriptHub.Visibility = Visibility.Visible;
                ScriptHub.Margin = new Thickness(0);
                if (webView != null && !SettingsVisibile)
                    webView.Margin = new Thickness(0, 0, webView.ActualWidth, 0);

                AnimationUtils.ObjectShift(ScriptHub, new Thickness(0), new Thickness(ScriptHub.ActualWidth, 0, 0, 0), AnimationUtils.EaseInOut);
                if (webView != null && !SettingsVisibile)
                    AnimationUtils.ObjectShift(webView, webView.Margin, new Thickness(0), AnimationUtils.EaseInOut);
                await Task.Delay(500);

                ScriptHub.Visibility = Visibility.Hidden;
                ScriptHub.Margin = new Thickness(ScriptHub.ActualWidth, 0, 0, 0);
                if (webView != null && !SettingsVisibile)
                    webView.Margin = new Thickness(0);
                ScriptHubAnimating = false;
            }
        }

        #endregion

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            Tabs.WindowResized();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

    }
}
