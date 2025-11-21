using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace InfoPointUI.Controls
{
    public partial class DrMaxCard : UserControl
    {
        public DrMaxCard()
        {
            InitializeComponent();
            // Generate initial barcode pattern
            UpdateBarcodePattern();
        }

        // Center text
        public string CardText
        {
            get => (string)GetValue(CardTextProperty);
            set => SetValue(CardTextProperty, value);
        }
        public static readonly DependencyProperty CardTextProperty =
            DependencyProperty.Register(nameof(CardText), typeof(string), typeof(DrMaxCard),
                new PropertyMetadata(string.Empty));

        // Center text appearance
        public double CenterTextSize
        {
            get => (double)GetValue(CenterTextSizeProperty);
            set => SetValue(CenterTextSizeProperty, value);
        }
        public static readonly DependencyProperty CenterTextSizeProperty =
            DependencyProperty.Register(nameof(CenterTextSize), typeof(double), typeof(DrMaxCard),
                new PropertyMetadata(20d));

        public Brush CenterTextBrush
        {
            get => (Brush)GetValue(CenterTextBrushProperty);
            set => SetValue(CenterTextBrushProperty, value);
        }
        public static readonly DependencyProperty CenterTextBrushProperty =
            DependencyProperty.Register(nameof(CenterTextBrush), typeof(Brush), typeof(DrMaxCard),
                new PropertyMetadata(Brushes.Black));

        // Barcode value (string)
        public string BarcodeValue
        {
            get => (string)GetValue(BarcodeValueProperty);
            set => SetValue(BarcodeValueProperty, value);
        }
        public static readonly DependencyProperty BarcodeValueProperty =
            DependencyProperty.Register(nameof(BarcodeValue), typeof(string), typeof(DrMaxCard),
                new PropertyMetadata("7993581070720991336050636", OnBarcodeValueChanged));

        private static void OnBarcodeValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DrMaxCard card) card.UpdateBarcodePattern();
        }

        // Optional label above barcode
        public string BarcodeLabel
        {
            get => (string)GetValue(BarcodeLabelProperty);
            set => SetValue(BarcodeLabelProperty, value);
        }
        public static readonly DependencyProperty BarcodeLabelProperty =
            DependencyProperty.Register(nameof(BarcodeLabel), typeof(string), typeof(DrMaxCard),
                new PropertyMetadata(string.Empty));

        public Visibility IsBarcodeLabelVisible
        {
            get => (Visibility)GetValue(IsBarcodeLabelVisibleProperty);
            set => SetValue(IsBarcodeLabelVisibleProperty, value);
        }
        public static readonly DependencyProperty IsBarcodeLabelVisibleProperty =
            DependencyProperty.Register(nameof(IsBarcodeLabelVisible), typeof(Visibility), typeof(DrMaxCard),
                new PropertyMetadata(Visibility.Collapsed));

        // Generated pattern for bars: collection of widths
        public ObservableCollection<double> BarcodePattern
        {
            get => (ObservableCollection<double>)GetValue(BarcodePatternProperty);
            private set => SetValue(BarcodePatternProperty, value);
        }
        public static readonly DependencyProperty BarcodePatternProperty =
            DependencyProperty.Register(nameof(BarcodePattern), typeof(ObservableCollection<double>), typeof(DrMaxCard),
                new PropertyMetadata(new ObservableCollection<double>()));

        // Simple pattern generator: NOT a true EAN/Code128 encoder.
        // It produces alternating thin/medium/thick bars based on chars.
        private void UpdateBarcodePattern()
        {
            var pattern = new ObservableCollection<double>();
            if (string.IsNullOrWhiteSpace(BarcodeValue))
            {
                BarcodePattern = pattern;
                return;
            }

            // Map characters to widths (2, 3, 4 px) for a visual-only barcode
            var map = new Func<char, double>(c =>
            {
                int v = c;
                int bucket = (v + 7) % 3; // simple spread
                return bucket switch
                {
                    0 => 2.0,
                    1 => 3.0,
                    _ => 4.0
                };
            });

            // Build bars with guard bars at start/end
            pattern.Add(4.0);
            foreach (var ch in BarcodeValue.Where(char.IsLetterOrDigit))
            {
                pattern.Add(map(ch));
                // Add a spacer (thin) to simulate white space between bars
                pattern.Add(2.0);
            }
            pattern.Add(4.0);

            BarcodePattern = pattern;
        }

        private void OnScanClicked(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Scanati cardul de fidelitate");
        }
    }

    /*
                <!-- Dr.Max logo (vector) -->
                <Grid Grid.Column="0" HorizontalAlignment="Left" VerticalAlignment="Center"
                      Width="150" Height="80">
                    <!-- green capsule -->
                <Border CornerRadius="24" Background="#19A647" />
                <!-- white inner capsule to mimic brand outline -->
                <Border Margin="6" CornerRadius="20" Background="White" />
                <!-- red Dr.Max text -->
                <TextBlock Text="Dr.Max"
                               Foreground="#E1251B"
                               FontWeight="Bold"
                               FontSize="28"
                               HorizontalAlignment="Center"
                               VerticalAlignment="Center" />
                <!-- green plus in upper-right of the text area -->
                <TextBlock Text="+"
                               Foreground="#19A647"
                               FontWeight="Bold"
                               FontSize="18"
                               HorizontalAlignment="Right"
                               VerticalAlignment="Top"
                               Margin="0,6,10,0"/>
                </Grid>
*/
}
