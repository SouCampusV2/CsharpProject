﻿using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace FinalProject
{
    public partial class MainWindow : Window
    {
        private static readonly Random Random = new Random();

        public MainWindow()
        {
            InitializeComponent();
        }

        private void AddColumnButton_Click(object sender, RoutedEventArgs e)
        {
            string title = ColumnTitleInput.Text.Trim();
            if (string.IsNullOrEmpty(title) || title == "Введите название колонки")
            {
                MessageBox.Show("Название колонки не может быть пустым.");
                return;
            }

            var columnGrid = CreateColumn(title);

            var columnDefinition = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            ColumnsGrid.ColumnDefinitions.Add(columnDefinition);
            Grid.SetColumn(columnGrid, ColumnsGrid.ColumnDefinitions.Count - 1);
            ColumnsGrid.Children.Add(columnGrid);

            ColumnTitleInput.Clear();
        }

        private Grid CreateColumn(string title)
        {
            var columnGrid = new Grid();
            columnGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            columnGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
            columnGrid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            var header = new TextBlock
            {
                Text = title,
                FontWeight = FontWeights.Bold,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };
            Grid.SetRow(header, 0);
            columnGrid.Children.Add(header);

            var listBox = new ListBox
            {
                Margin = new Thickness(5),
                AllowDrop = true
            };
            listBox.PreviewMouseMove += ListBox_PreviewMouseMove;
            listBox.DragOver += Group_DragOver;
            listBox.Drop += Group_Drop;
            Grid.SetRow(listBox, 1);
            columnGrid.Children.Add(listBox);

            var bottomPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                Margin = new Thickness(5)
            };

            var cardNameInput = new TextBox
            {
                Width = 100,
                Margin = new Thickness(5),
                Text = "Название карточки"
            };
            cardNameInput.GotFocus += (s, e) =>
            {
                if (cardNameInput.Text == "Название карточки")
                {
                    cardNameInput.Text = string.Empty;
                    cardNameInput.Foreground = Brushes.Black;
                }
            };
            cardNameInput.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(cardNameInput.Text))
                {
                    cardNameInput.Text = "Название карточки";
                    cardNameInput.Foreground = Brushes.Gray;
                }
            };
            bottomPanel.Children.Add(cardNameInput);

            var colorPicker = CreateColorPicker();
            bottomPanel.Children.Add(colorPicker);

            var addCardButton = new Button
            {
                Content = "+",
                Width = 25,
                Height = 25,
                Margin = new Thickness(5)
            };

            addCardButton.Click += (s, e) =>
            {
                string cardText = cardNameInput.Text.Trim();
                if (string.IsNullOrEmpty(cardText) || cardText == "Название карточки")
                {
                    MessageBox.Show("Введите название карточки.");
                    return;
                }

                var colorBrush = colorPicker.SelectedItem as SolidColorBrush ?? Brushes.LightBlue;
                var card = CreateCard(cardText, colorBrush);
                listBox.Items.Add(card);
                cardNameInput.Clear();
            };

            bottomPanel.Children.Add(addCardButton);
            Grid.SetRow(bottomPanel, 2);
            columnGrid.Children.Add(bottomPanel);

            var contextMenu = new ContextMenu();
            var deleteColumnItem = new MenuItem { Header = "Удалить колонку" };
            deleteColumnItem.Click += (s, e) =>
            {
                var columnIndex = Grid.GetColumn(columnGrid);
                ColumnsGrid.ColumnDefinitions.RemoveAt(columnIndex);
                ColumnsGrid.Children.Remove(columnGrid);

                for (int i = 0; i < ColumnsGrid.Children.Count; i++)
                {
                    Grid.SetColumn(ColumnsGrid.Children[i], i);
                }
            };
            contextMenu.Items.Add(deleteColumnItem);
            columnGrid.ContextMenu = contextMenu;

            return columnGrid;
        }

        private ComboBox CreateColorPicker()
        {
            var colorPicker = new ComboBox
            {
                Width = 50,
                Margin = new Thickness(0, 0, 10, 0),
                ItemsSource = new[]
                {
                    new SolidColorBrush(Colors.LightBlue),
                    new SolidColorBrush(Colors.LightGreen),
                    new SolidColorBrush(Colors.Yellow),
                    new SolidColorBrush(Colors.LightCoral),
                    new SolidColorBrush(Colors.Plum)
                },
                SelectedIndex = 0
            };

            colorPicker.ItemTemplate = CreateColorTemplate();
            return colorPicker;
        }

        private DataTemplate CreateColorTemplate()
        {
            var template = new DataTemplate(typeof(SolidColorBrush));
            var factory = new FrameworkElementFactory(typeof(Rectangle));
            factory.SetValue(Rectangle.WidthProperty, 20.0);
            factory.SetValue(Rectangle.HeightProperty, 20.0);
            factory.SetBinding(Rectangle.FillProperty, new Binding("."));
            template.VisualTree = factory;
            return template;
        }

        private Border CreateCard(string text, SolidColorBrush backgroundColor)
        {
            var card = new Border
            {
                Background = backgroundColor,
                Margin = new Thickness(5),
                Padding = new Thickness(10),
                Child = new TextBlock
                {
                    Text = text,
                    Foreground = Brushes.White,
                    FontSize = 14
                }
            };

            var contextMenu = new ContextMenu();

            var deleteMenuItem = new MenuItem { Header = "Удалить" };
            deleteMenuItem.Click += (s, e) =>
            {
                var parent = VisualTreeHelper.GetParent(card);
                while (parent != null && parent is not ListBox)
                {
                    parent = VisualTreeHelper.GetParent(parent);
                }

                if (parent is ListBox listBox)
                {
                    listBox.Items.Remove(card);
                }
            };

            var changeColorMenuItem = new MenuItem { Header = "Изменить цвет" };
            var colorPicker = CreateColorPicker();
            changeColorMenuItem.Click += (s, e) =>
            {
                var popup = new Window
                {
                    Width = 150,
                    Height = 100,
                    Content = colorPicker,
                    WindowStartupLocation = WindowStartupLocation.CenterScreen
                };

                colorPicker.SelectionChanged += (cs, ce) =>
                {
                    card.Background = colorPicker.SelectedItem as SolidColorBrush;
                    popup.Close();
                };

                popup.ShowDialog();
            };

            contextMenu.Items.Add(deleteMenuItem);
            contextMenu.Items.Add(changeColorMenuItem);

            card.ContextMenu = contextMenu;

            return card;
        }

        private void ColumnTitleInput_GotFocus(object sender, RoutedEventArgs e)
        {
            if (ColumnTitleInput.Text == "Введите название колонки")
            {
                ColumnTitleInput.Text = string.Empty;
                ColumnTitleInput.Foreground = Brushes.Black;
            }
        }

        private void ColumnTitleInput_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(ColumnTitleInput.Text))
            {
                ColumnTitleInput.Text = "Введите название колонки";
                ColumnTitleInput.Foreground = Brushes.Gray;
            }
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var listBox = sender as ListBox;
                if (listBox?.SelectedItem != null)
                {
                    var data = new DataObject();
                    data.SetData(typeof(UIElement), listBox.SelectedItem);
                    data.SetData("SourceListBox", listBox);

                    DragDrop.DoDragDrop(listBox, data, DragDropEffects.Move);
                }
            }
        }

        private void Group_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.Move;
            e.Handled = true;
        }

        private void Group_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(UIElement)))
            {
                var card = e.Data.GetData(typeof(UIElement)) as UIElement;
                var targetListBox = sender as ListBox;

                if (card != null && targetListBox != null)
                {
                    var sourceListBox = e.Data.GetData("SourceListBox") as ListBox;

                    if (sourceListBox != null && !ReferenceEquals(sourceListBox, targetListBox))
                    {
                        sourceListBox.Items.Remove(card);
                    }

                    if (!targetListBox.Items.Contains(card))
                    {
                        targetListBox.Items.Add(card);
                    }

                    e.Handled = true;
                }
            }
        }
    }
}
