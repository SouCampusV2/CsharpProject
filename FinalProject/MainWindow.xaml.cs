using System;
using System.ComponentModel;
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

            var selectedColorItem = CardColorPicker.SelectedItem as ComboBoxItem;
            if (selectedColorItem == null)
            {
                MessageBox.Show("Выберите цвет для карточки.");
                return;
            }

            string selectedColor = selectedColorItem.Tag.ToString();
            var color = (Color)ColorConverter.ConvertFromString(selectedColor);

            var card = new Card
            {
                Text = cardText,
                BackgroundColor = new SolidColorBrush(color),
            };

            var activeColumn = ColumnsGrid.Children.OfType<ListBox>().FirstOrDefault();
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

        private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                var listBox = sender as ListBox;
                if (listBox?.SelectedItem != null)
                {
                    var data = new DataObject();
                    data.SetData(typeof(Card), listBox.SelectedItem);
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
            if (e.Data.GetDataPresent(typeof(Card)))
            {
                var card = e.Data.GetData(typeof(Card)) as Card;
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

        private void DeleteCardMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var contextMenu = menuItem.Parent as ContextMenu;
                var border = contextMenu?.PlacementTarget as Border;

                if (border != null)
                {
                    var card = border.DataContext as Card;
                    if (card != null)
                    {
                        var parentListBox = FindParent<ListBox>(border);
                        parentListBox?.Items.Remove(card);
                    }
                }
            }
        }

        private void ChangeColorMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (sender is MenuItem menuItem)
            {
                var contextMenu = menuItem.Parent as ContextMenu;
                var border = contextMenu?.PlacementTarget as Border;

                if (border != null)
                {
                    var card = border.DataContext as Card;
                    if (card != null)
                    {
                        var random = new Random();
                        var color = Color.FromRgb(
                            (byte)random.Next(0, 256),
                            (byte)random.Next(0, 256),
                            (byte)random.Next(0, 256)
                        );
                        card.BackgroundColor = new SolidColorBrush(color);
                    }
                }
            }
        }

        private T FindParent<T>(DependencyObject child) where T : DependencyObject
        {
            var parent = VisualTreeHelper.GetParent(child);
            if (parent == null) return null;
            if (parent is T parentAsT) return parentAsT;
            return FindParent<T>(parent);
        }
    }

    public class Card : INotifyPropertyChanged
    {
        private SolidColorBrush _backgroundColor;

        public string Text { get; set; }

        public SolidColorBrush BackgroundColor
        {
            get => _backgroundColor;
            set
            {
                if (_backgroundColor != value)
                {
                    _backgroundColor = value;
                    OnPropertyChanged(nameof(BackgroundColor));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
