using System.Windows;
using System.Windows.Controls;

namespace InfoPointUI.Controls
{
    public partial class LoyalityCard : UserControl
    {
        public LoyalityCard()
        {
            InitializeComponent();
        }

        // Proprietate pentru textul suprapus
        public string CardText
        {
            get { return (string)GetValue(CardTextProperty); }
            set { SetValue(CardTextProperty, value); }
        }

        public static readonly DependencyProperty CardTextProperty =
            DependencyProperty.Register("CardText", typeof(string), typeof(LoyalityCard), new PropertyMetadata(string.Empty));
    }
}
