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
        public MainWindow()
        {
            InitializeComponent();
            InitializeColumns();
        }

        private void InitializeColumns()
        {
            foreach (var child in ColumnsGrid.Children.OfType<ListBox>())
            {
                AttachColumnEvents(child);
            }
        }

        private void AttachColumnEvents(ListBox column)
        {
            column.AllowDrop = true;
            column.PreviewMouseMove += ListBox_PreviewMouseMove;
            column.DragOver += Group_DragOver;
            column.Drop += Group_Drop;
        }

        private void AddColumnButton_Click(object sender, RoutedEventArgs e)
        {
            string title = ColumnTitleInput.Text.Trim();
            if (string.IsNullOrEmpty(title) || title == "Введите название колонки")
            {
                MessageBox.Show("Название колонки не может быть пустым.");
                return;
            }

            var newColumn = new ListBox
            {
                Margin = new Thickness(5, 0, 5, 0),
                ItemTemplate = (DataTemplate)Resources["CardTemplate"]
            };
            AttachColumnEvents(newColumn);

            var columnDefinition = new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) };
            ColumnsGrid.ColumnDefinitions.Add(columnDefinition);
            Grid.SetColumn(newColumn, ColumnsGrid.ColumnDefinitions.Count - 1);
            ColumnsGrid.Children.Add(newColumn);

            ColumnTitleInput.Clear();
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

        private void AddCardButton_Click(object sender, RoutedEventArgs e)
        {
            string cardText = CardInput.Text.Trim();
            if (string.IsNullOrEmpty(cardText))
            {
                MessageBox.Show("Текст карточки не может быть пустым.");
                return;
            }

            // Получение выбранного цвета
            var selectedColorItem = CardColorPicker.SelectedItem as ComboBoxItem;
            if (selectedColorItem == null)
            {
                MessageBox.Show("Выберите цвет для карточки.");
                return;
            }

            // Используем Tag вместо Content
            string selectedColor = selectedColorItem.Tag.ToString();
            var color = (Color)ColorConverter.ConvertFromString(selectedColor);

            // Создаем карточку
            var card = new Card
            {
                Text = cardText,
                BackgroundColor = new SolidColorBrush(color),
            };

            // Найдем активную колонку (например, первую)
            var activeColumn = ColumnsGrid.Children.OfType<ListBox>().FirstOrDefault();
            if (activeColumn != null)
            {
                activeColumn.Items.Add(card);
            }
            else
            {
                MessageBox.Show("Нет доступных колонок для добавления карточки.");
            }

            // Очистка ввода
            CardInput.Clear();
        }

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var listBox = sender as ListBox;
                if (listBox?.SelectedItem != null)
                {
                    var data = new DataObject();
                    data.SetData(typeof(Card), listBox.SelectedItem);
                    data.SetData("SourceListBox", listBox); // Передаём исходный ListBox

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
            if (e.Data.GetDataPresent(typeof(Card)))
            {
                var card = e.Data.GetData(typeof(Card)) as Card;
                var targetListBox = sender as ListBox;

                if (card != null && targetListBox != null)
                {
                    // Получаем исходный ListBox через e.Data
                    var sourceListBox = e.Data.GetData("SourceListBox") as ListBox;

                    if (sourceListBox != null)
                    {
                        // Удаляем карточку из источника, только если исходный ListBox не является целевым
                        if (!ReferenceEquals(sourceListBox, targetListBox))
                        {
                            sourceListBox.Items.Remove(card);
                        }
                    }

                    // Добавляем карточку в целевой ListBox, только если её ещё нет
                    if (!targetListBox.Items.Contains(card))
                    {
                        targetListBox.Items.Add(card);
                    }

                    // Останавливаем дальнейшую обработку события
                    e.Handled = true;
                }
            }
        }

    }

    public class Card
    {
        public string Text { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }
    }
}