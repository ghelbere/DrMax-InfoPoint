using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace InfoPointUI.Views.Standby
{
    /// <summary>
    /// Interaction logic for StandbyWindow.xaml
    /// </summary>
    public partial class StandbyWindow : Window
    {
        public event EventHandler? StandbyClicked;                // click/touch pe fereastră
        public event EventHandler<bool>? SensorButtonClicked;     // buton simulare: payload = isDetected (true)

        public StandbyWindow()
        {
            InitializeComponent();

            this.MouseDown += StandbyWindow_OnMouseDown;
            this.TouchDown += StandbyWindow_OnTouchDown;

            this.Closing += StandbyWindow_Closing;
            Loaded += Window_Loaded;

        }



        private void StandbyWindow_OnMouseDown(object? sender, MouseButtonEventArgs e)
        {
            StandbyClicked?.Invoke(this, EventArgs.Empty);
            this.Hide();
        }

        private void StandbyWindow_OnTouchDown(object? sender, TouchEventArgs e)
        {
            StandbyClicked?.Invoke(this, EventArgs.Empty);
            this.Hide();
        }

        private void btnToggleSensor_Click(object sender, RoutedEventArgs e)
        {
            SensorButtonClicked?.Invoke(this, true);
            this.Hide();
        }

        private void StandbyWindow_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true; // prevenim închiderea completă
            StandbyClicked?.Invoke(this, EventArgs.Empty);
            this.Hide();
        }

        //animatie text standby
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

            double canvasWidth = MyText.Parent is Canvas c ? c.ActualWidth : 0;
            double textWidth = MyText.ActualWidth;

            Canvas.SetLeft(MyText, (canvasWidth - textWidth) / 2); // center
            Canvas.SetTop(MyText, 50); // poziția verticală

            // Animație pentru mișcarea textului sus-jos
            var animation = new DoubleAnimation
            {
                From = 0,
                To = 100,
                Duration = TimeSpan.FromSeconds(3),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever
            };

            Storyboard.SetTarget(animation, MyText);
            Storyboard.SetTargetProperty(animation, new PropertyPath("(Canvas.Top)"));

            var storyboard = new Storyboard();
            storyboard.Children.Add(animation);
            storyboard.Begin();
        }

    }
}
