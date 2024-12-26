using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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

            // Добавляем контекстное меню для колонки
            var contextMenu = new ContextMenu();
            var deleteColumnItem = new MenuItem { Header = "Удалить колонку" };
            deleteColumnItem.Click += (s, e) =>
            {
                var columnIndex = Grid.GetColumn(columnGrid);
                ColumnsGrid.ColumnDefinitions.RemoveAt(columnIndex);
                ColumnsGrid.Children.Remove(columnGrid);

                // Обновляем индексы для оставшихся колонок
                for (int i = 0; i < ColumnsGrid.Children.Count; i++)
                {
                    Grid.SetColumn(ColumnsGrid.Children[i], i);
                }
            };
            contextMenu.Items.Add(deleteColumnItem);

            columnGrid.ContextMenu = contextMenu;

            return columnGrid;
        }

        private void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            string cardText = CardInput.Text.Trim();
            if (string.IsNullOrEmpty(cardText))
            {
                MessageBox.Show("Текст карточки не может быть пустым.");
                return;
            }

            var selectedColorItem = CardColorPicker.SelectedItem as ComboBoxItem;
            if (selectedColorItem == null)
            {
                MessageBox.Show("Выберите цвет для карточки.");
                return;
            }

            string selectedColor = selectedColorItem.Tag.ToString();
            var color = (Color)ColorConverter.ConvertFromString(selectedColor);

            var card = CreateCard(cardText, new SolidColorBrush(color));

            var activeColumn = ColumnsGrid.Children.OfType<Grid>()
                .LastOrDefault()?.Children.OfType<ListBox>().FirstOrDefault();

            if (activeColumn != null)
            {
                activeColumn.Items.Add(card);
            }
            else
            {
                MessageBox.Show("Нет доступных колонок для добавления карточки.");
            }

            CardInput.Clear();
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
            changeColorMenuItem.Click += (s, e) =>
            {
                var colors = new[] { Colors.LightBlue, Colors.LightGreen, Colors.Yellow, Colors.LightCoral, Colors.Plum };
                var newColor = colors[Random.Next(colors.Length)];
                card.Background = new SolidColorBrush(newColor);
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
