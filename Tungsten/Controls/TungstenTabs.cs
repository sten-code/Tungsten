using System.Threading.Tasks;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Wpf;
using System.Diagnostics;
using Newtonsoft.Json;

namespace Tungsten.Controls
{
    public class TungstenTabs : TabControl
    {
        public double TabWidth { get; set; } = 100;
        public double TabSpacing { get; set; } = 4;
        public double MaxWidthSubtraction { get; set; } = 80;
        private double addTabButtonWidth = 23;

        private T GetTemplateItem<T>(Control elem, string name)
        {
            return elem.Template.FindName(name, elem) is T name1 ? name1 : default;
        }

        public TungstenTabs()
        {
            Loaded += (s, e) =>
            {
                GetTemplateItem<Button>(this, "AddTabButton").Click += (t, a) =>
                {
                    MakeTab();
                };
            };
        }

        public TabItem MakeTab(string text = "", string title = "New Tab")
        {
            TabItem tab = new TabItem
            {
                Header = title
            };
            if (Editor == "Ace") tab.Content = new AceEditor
            {
                Text = text
            };
            if (Editor == "Monaco") tab.Content = new MonacoEditor
            {
                Text = text
            };

            tab.MouseDown += (s, e) =>
            {
                if (e.OriginalSource is Border || e.OriginalSource is TextBlock)
                {
                    if (e.MiddleButton == MouseButtonState.Pressed)
                    {
                        CloseTab(tab);
                    }
                }
            };
            tab.Loaded += (s, e) =>
            {
                GetTemplateItem<Button>(tab, "CloseButton").Click += (c, a) =>
                {
                    CloseTab(tab);
                };
            };

            double maxWidth = ActualWidth - MaxWidthSubtraction;
            double width = addTabButtonWidth + (TabSpacing * 2) + TabWidth;
            foreach (TabItem t in Items)
            {
                width += t.ActualWidth;
            }
            double newWidth = TabWidth;
            if (width > maxWidth)
            {
                newWidth = (maxWidth - addTabButtonWidth) / (Items.Count + 1);
                foreach (TabItem t in Items)
                {
                    AnimationUtils.AnimateWidth(t, t.ActualWidth, newWidth, AnimationUtils.EaseInOut, 200);
                }
            }

            SelectedIndex = Items.Add(tab);
            AnimationUtils.AnimateWidth(tab, 0, newWidth, AnimationUtils.EaseInOut, 200);
            return tab;
        }

        public string Editor;

        public async void SwitchToEditor(string editor)
        {
            if (Editor == editor) return;

            Editor = editor;
            if (editor == "Ace")
            {
                foreach (TabItem t in Items)
                {
                    MonacoEditor monaco = (MonacoEditor)t.Content;
                    string content = monaco.Text;
                    Thickness margin = monaco.Margin;
                    monaco.Dispose();
                    t.Content = new AceEditor
                    {
                        Text = content,
                        Margin = margin
                    };
                }
            } else if (editor == "Monaco")
            {
                foreach (TabItem t in Items)
                {
                    AceEditor ace = (AceEditor)t.Content;
                    string content = await ace.GetText();
                    Thickness margin = ace.Margin;
                    ace.Dispose();
                    t.Content = new MonacoEditor
                    {
                        Text = content,
                        Margin = margin,
                    };
                }
            }
        }

        public async void CloseTab(TabItem tab)
        {
            AnimationUtils.AnimateWidth(tab, tab.ActualWidth, 0, AnimationUtils.EaseInOut, 200);
            double maxWidth = ActualWidth - 120;
            double width = -(TabWidth + TabSpacing);
            foreach (TabItem t in Items)
            {
                width += t.ActualWidth;
            }

            if (width < maxWidth)
            {
                double newWidth = Math.Min((maxWidth - addTabButtonWidth) / (Items.Count - 1), TabWidth);
                foreach (TabItem t in Items)
                {
                    if (t != tab)
                        AnimationUtils.AnimateWidth(t, t.ActualWidth, newWidth, AnimationUtils.EaseInOut, 200);
                }
            }

            await Task.Delay(200);
            Items.Remove(tab);
            ((WebView2)tab.Content).Dispose();
        }

        public void WindowResized()
        {
            double maxWidth = ActualWidth - MaxWidthSubtraction;
            double width = 0;
            foreach (TabItem t in Items)
            {
                width += t.ActualWidth;
            }

            double newWidth = Math.Min((maxWidth - addTabButtonWidth) / (Items.Count), TabWidth);
            foreach (TabItem t in Items)
            {
                AnimationUtils.AnimateWidth(t, t.ActualWidth, newWidth, AnimationUtils.EaseInOut, 0); // For some reason setting the width doesn't work so an animation with a duration of 0ms will have to do.
            }
        }

    }
}
