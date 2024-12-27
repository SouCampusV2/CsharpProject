using System;
using System.Collections.Generic;
using System.Windows.Media;

namespace FinalProject
{
    public class Column
    {
        public string Title { get; set; }
        public SolidColorBrush BackgroundColor { get; set; }
        public List<Card> Cards { get; set; }

        public Column(string title, SolidColorBrush backgroundColor)
        {
            Title = title;
            BackgroundColor = backgroundColor;
            Cards = new List<Card>();
        }
    }
}
