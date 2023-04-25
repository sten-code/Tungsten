using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Tungsten.Settings
{
    public partial class SettingsMenu : UserControl
    {
        private List<StackPanel> _pages;
        public SaveManager SaveManager { get; set; }

        public SettingsMenu()
        {
            InitializeComponent();
            _pages = new List<StackPanel>();
            SaveManager = new SaveManager(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location) + "\\bin\\settings.json");
        }

        public void LoadPage(int index)
        {
            SettingsContent.Child = _pages[index];
        }

        public void AddSettingPage(string pageName, List<Setting> settings)
        {
            // Add the button on the sidebar
            Button btn = new Button
            {
                Content = pageName,
                Background = new SolidColorBrush(Color.FromRgb(25, 27, 33)),
                Foreground = new SolidColorBrush(Color.FromRgb(197, 199, 211)),
                Margin = new Thickness(5, 5, 5, 0),
                Height = 30,
                FontSize = 13,
                Tag = _pages.Count
            };
            btn.Click += (s, e) =>
            {
                SettingsContent.Child = _pages[(int)btn.Tag];
            };
            SettingPages.Children.Add(btn);

            // Add the setting components to the page
            StackPanel settingComponents = new StackPanel();
            foreach (Setting setting in settings)
            {
                Border baseComponent = new Border
                {
                    Margin = new Thickness(10, 10, 10, -5),
                    BorderThickness = new Thickness(1),
                    BorderBrush = new SolidColorBrush(Color.FromRgb(44, 47, 58)),
                    CornerRadius = new CornerRadius(4),
                    Height = 35
                };
                baseComponent.Child = setting.GetComponent();
                settingComponents.Children.Add(baseComponent);
            }
            _pages.Add(settingComponents);
        }

    }
}
