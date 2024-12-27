using System.Windows.Media;

namespace FinalProject
{
    public class Card
    {
        public string Title { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }

        public Card(string title, SolidColorBrush backgroundColor)
        {
            Title = title;
            BackgroundColor = backgroundColor;
        }
    }
}
