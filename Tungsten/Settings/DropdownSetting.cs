using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows;
using System;

namespace Tungsten.Settings
{
    public class DropdownSetting : Setting
    {
        public List<string> Items { get; private set; }
        public string Value { get; private set; }
        public Action<string, bool> OnChangeEvent { get; private set; }

        public DropdownSetting(string name, string identifier, List<string> items, string defaultItem, Action<string, bool> onChange = null) : base(name, identifier)
        {
            Items = items;
            OnChangeEvent = onChange;
            Value = GetValue(defaultItem);
            if (OnChangeEvent != null)
                OnChangeEvent(Value, true);
        }

        public override Grid GetComponent()
        {
            Grid grid = base.GetComponent();
            ComboBox comboBox = new ComboBox
            {
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5),
                SelectedItem = Value,
                ItemsSource = Items
            };
            comboBox.SelectionChanged += (s, e) =>
            {
                Value = (string)comboBox.SelectedItem;
                if (OnChangeEvent != null)
                    OnChangeEvent(Value, false);

                if (SaveManager.Instance != null)
                    SaveManager.Instance.Save(Identifier, Value);
            };
            grid.Children.Add(comboBox);
            return grid;
        }

    }
}
