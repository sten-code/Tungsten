using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using System.Web;
using System.Windows;

namespace Tungsten.Controls
{
    public class MonacoEditor : WebView2
    {
        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(MonacoEditor), new PropertyMetadata(default(string)));

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                SetText(value);
            }
        }

        private async void SetText(string text)
        {
            while (!IsLoaded)
                await Task.Delay(100);
            await CoreWebView2.ExecuteScriptAsync("SetText(\"" + HttpUtility.JavaScriptStringEncode(text) + "\")");
        }

        private string text = "";
        public new bool IsLoaded = false;

        public MonacoEditor()
        {
            Source = new Uri(Path.GetDirectoryName(Assembly.GetEntryAssembly()?.Location) + "\\bin\\Monaco\\Monaco.html");
            CoreWebView2InitializationCompleted += (s, e) =>
            {
                CoreWebView2.WebMessageReceived += (sender, args) =>
                {
                    text = args.TryGetWebMessageAsString();
                };
                CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
                CoreWebView2.Settings.AreDevToolsEnabled = false;
                CoreWebView2.NewWindowRequested += (sender, args) =>
                {
                    args.Handled = true;
                };
            };
            NavigationCompleted += (s, e) =>
            {
                IsLoaded = true;
            };
            NavigationStarting += (s, e) =>
            {
                IsLoaded = false;
            };
            AllowDrop = false;
        }

    }
}
