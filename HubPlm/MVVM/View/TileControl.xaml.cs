using ApplicationHub.Properties;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace ApplicationHub
{
    /// <summary>
    /// Logique d'interaction pour TileControl.xaml
    /// </summary>
    public partial class TileControl : UserControl
    {
        private const double ScaleFactor = 1.15;

        public TileControl()
        {
            InitializeComponent();
        }

        private void TileControl_MouseEnter(object sender, MouseEventArgs e)
        {
            // Create animation to smoothly scale the control
            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = ScaleFactor,
                Duration = new Duration(TimeSpan.FromMilliseconds(100)),
                EasingFunction = new CubicEase()
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }

        private void TileControl_MouseLeave(object sender, MouseEventArgs e)
        {
            // Create animation to smoothly scale the control
            DoubleAnimation scaleAnimation = new DoubleAnimation
            {
                To = 1,
                Duration = new Duration(TimeSpan.FromMilliseconds(300)),
                EasingFunction = new CubicEase()
            };

            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
    }
}