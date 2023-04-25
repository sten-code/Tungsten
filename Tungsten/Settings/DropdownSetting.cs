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
        public Action<string> OnChangeEvent { get; private set; }

        public DropdownSetting(string name, string identifier, List<string> items, string defaultItem, Action<string> onChange = null) : base(name, identifier)
        {
            Items = items;
            OnChangeEvent = onChange;
            Value = GetValue(defaultItem);
            if (OnChangeEvent != null)
                OnChangeEvent(Value);
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
                    OnChangeEvent(Value);

                if (SaveManager.Instance != null)
                    SaveManager.Instance.Save(Identifier, Value);
            };
            grid.Children.Add(comboBox);
            return grid;
        }

    }
}
