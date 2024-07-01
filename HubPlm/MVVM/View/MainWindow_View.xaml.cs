using ApplicationHub.MVVM.ViewModel;
using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace ApplicationHub
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow_View : Window
    {
        public static MainWindow_View Instance;

        public MainWindow_ViewModel ViewModel { get; set; }



        private double baseHeight;
        private double baseWidth;
        private double baseLeft;
        private double baseTop;
        private bool floatingState;
        private Stopwatch stopwatch;

        private Screen currentScreen;


        public MainWindow_View()
        {
            Instance = this;

            InitializeComponent();

            this.DataContext = ViewModel = new MainWindow_ViewModel();

            stopwatch = new Stopwatch();

            currentScreen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            currentScreen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
        }
        private new void DragMove()
        {
            base.DragMove();            

            if (floatingState)
            {                
                SnapToScreenEdge();
            }
        }

        private void TitleBarBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {                
                DragMove();
            }
        }

        private void CloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void DockBtn_Click(object sender, RoutedEventArgs e)
        {
            switch (this.WindowState)
            {
                case WindowState.Normal:
                    this.WindowState = WindowState.Maximized;
                    break;

                case WindowState.Maximized:
                    this.WindowState = WindowState.Normal;
                    break;
            }
        }

        private void FloatingLogoImage_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if(e.LeftButton == MouseButtonState.Pressed)
            {
                if(!stopwatch.IsRunning)
                {
                    stopwatch.Start();
                }
                DragMove();
            }
            if (e.LeftButton == MouseButtonState.Released)
            {
                if (stopwatch.ElapsedMilliseconds < 150)
                {
                    if (e.ClickCount == 1)
                    {
                        ToggleFloatingState();
                    }
                }

                stopwatch.Stop();
                stopwatch.Reset();
            }
        }
        private void ToggleFloatingState()
        {
            var edgeBorderScaleTransform = this.EdgeBorder.RenderTransform as ScaleTransform;

            var floatingBorderTransformGroup = this.FloatingBorder.RenderTransform as TransformGroup;
            var floatingBorderScaleTransform = floatingBorderTransformGroup.Children[0] as ScaleTransform;

            if (!floatingState)
            {
                floatingState = true;

                baseHeight = this.ActualHeight;
                baseWidth = this.ActualWidth;
                baseLeft = this.Left;
                baseTop = this.Top;

                this.Topmost = true;
                this.Height = 100;
                this.Width = 100;    
                this.ResizeMode = ResizeMode.NoResize;
                this.EdgeBorder.Visibility = Visibility.Collapsed;                
                this.FloatingBorder.Margin = new Thickness(10);
                this.FloatingBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                this.FloatingBorder.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                this.FloatingBorder.Effect = new DropShadowEffect()
                {
                    Color = (Color)ColorConverter.ConvertFromString("#8000"),
                    Direction = 0,
                    ShadowDepth = 0,
                    Opacity = 0.5,
                    BlurRadius = 10
                };

                //Position the window at the bottom right corner of the current screen
                var screenBounds = currentScreen.WorkingArea;
                var left = screenBounds.Right - this.Width;
                var top = screenBounds.Bottom - this.Height;               

                AnimateTransform(edgeBorderScaleTransform, 0, 0, floatingState, TimeSpan.FromMilliseconds(0));
                AnimateTransform(floatingBorderScaleTransform, 2, 2, floatingState, TimeSpan.FromMilliseconds(150));
                AnimatePosition(left, top, TimeSpan.Zero, TimeSpan.FromMilliseconds(300));
            }
            else
            {
                floatingState = false;

                //Position the window at the last known position
                AnimateTransform(edgeBorderScaleTransform, 1, 1, floatingState, TimeSpan.FromMilliseconds(150));
                AnimateTransform(floatingBorderScaleTransform, 1, 1, floatingState, TimeSpan.FromMilliseconds(0));
                AnimatePosition(baseLeft, baseTop, TimeSpan.Zero, TimeSpan.FromMilliseconds(300));

                this.Topmost = false;
                this.Height = baseHeight;
                this.Width = baseWidth;
                this.ResizeMode = ResizeMode.CanResize;
                this.FloatingBorder.Effect = null;
                this.FloatingBorder.Margin = new Thickness(5);
                this.FloatingBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
                this.FloatingBorder.VerticalAlignment = System.Windows.VerticalAlignment.Top;                
                this.EdgeBorder.Visibility = Visibility.Visible;
            }
        }

        private void SnapToScreenEdge(bool animated = true)
        {
            var screenBounds = currentScreen.WorkingArea;

            var left = this.RestoreBounds.Left;
            var top = this.RestoreBounds.Top;
            var right = screenBounds.Right - (left + this.Width);
            var bottom = screenBounds.Bottom - (top + this.Height);

            double targetLeft = left;
            double targetTop = top;

            // Determine the nearest screen edge
            var absLeft = Math.Abs(left);
            var absTop = Math.Abs(top);
            var absRight = Math.Abs(right);
            var absBottom = Math.Abs(bottom);

            var minHorizontalDistance = Math.Min(absLeft, absRight);
            var minVerticalDistance = Math.Min(absTop, absBottom);
            var minDistance = Math.Min(minHorizontalDistance, minVerticalDistance);

            //trigger only if the window is closer to the edge than the threshold
            double threshold = 150;

            if (minDistance < threshold)
            {                
                //Near corner, snap to corner
                //if (minHorizontalDistance < (threshold * 2/3) && minVerticalDistance < (threshold * 2 / 3))
                if (minHorizontalDistance < threshold && minVerticalDistance < threshold)
                {
                    if(minHorizontalDistance == absLeft)
                    {
                        targetLeft = screenBounds.Left; 
                    }
                    else if(minHorizontalDistance == absRight)
                    {
                        targetLeft = screenBounds.Right - this.Width;
                    }
                    if (minVerticalDistance == absTop)
                    {
                        targetTop = screenBounds.Top;
                    }
                    else if (minVerticalDistance == absBottom)
                    {
                        targetTop = screenBounds.Bottom - this.Height;
                    }
                }
                else
                {
                    if (minDistance == absLeft)
                    {
                        targetLeft = screenBounds.Left;
                    }
                    else if (minDistance == absRight)
                    {
                        targetLeft = screenBounds.Right - this.Width;
                    }
                    if (minDistance == absTop)
                    {
                        targetTop = screenBounds.Top;
                    }
                    else if (minDistance == absBottom)
                    {
                        targetTop = screenBounds.Bottom - this.Height;
                    }
                }


                if (animated)
                {
                    AnimatePosition(targetLeft, targetTop, TimeSpan.Zero, TimeSpan.FromMilliseconds(300));
                }
                else
                {
                    AnimatePosition(targetLeft, targetTop, TimeSpan.Zero, TimeSpan.Zero);
                }
            }
        }

        private void AnimateTransform(ScaleTransform transform, double scaleX, double scaleY, bool floatingState, TimeSpan beginTime)
        {
            var scaleXAnimation = new DoubleAnimation(scaleX, TimeSpan.FromMilliseconds(300));
            var scaleYAnimation = new DoubleAnimation(scaleY, TimeSpan.FromMilliseconds(300));

            scaleXAnimation.BeginTime = beginTime;
            scaleYAnimation.BeginTime = beginTime;

            if (floatingState)
            {
                scaleXAnimation.EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut };
                scaleYAnimation.EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut };
            }
            else
            {
                scaleXAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
                scaleYAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };                
            }

            transform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation);
            transform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation);
        }
        private void AnimatePosition(double left, double top, TimeSpan beginTime, TimeSpan duration)
        {            
            //Debug.WriteLine("---------------------------------------------------");
            //Debug.WriteLine("OriginalPosition: " + this.Left + ", " + this.Top);
            //Debug.WriteLine("RestoreBoundsPosition: " + this.RestoreBounds.Left + ", " + this.RestoreBounds.Top);
            //Debug.WriteLine("AnimatePosition: " + left + ", " + top);

            var leftAnimation = new DoubleAnimation(this.Left, left, duration);
            var topAnimation = new DoubleAnimation(this.Top, top, duration);

            leftAnimation.BeginTime = beginTime;
            topAnimation.BeginTime = beginTime;

            leftAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };
            topAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };

            leftAnimation.Completed += (s, e) =>
            {
                this.BeginAnimation(Window.LeftProperty, null);

            };
            topAnimation.Completed += (s, e) =>
            {
                this.BeginAnimation(Window.TopProperty, null);
            };

            this.BeginAnimation(Window.LeftProperty, leftAnimation);
            this.BeginAnimation(Window.TopProperty, topAnimation);
        }


        private void FloatingLogoImage_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Spin the floatting logo very fast
            var floatingBorderTransformGroup = this.FloatingBorder.RenderTransform as TransformGroup;
            var floatingBorderScaleTransform = floatingBorderTransformGroup.Children[0] as ScaleTransform;
            var floatingBorderRotateTransform = floatingBorderTransformGroup.Children[1] as RotateTransform;
                        
            var rotateAnimation = new DoubleAnimation(45, TimeSpan.FromMilliseconds(300));
            rotateAnimation.EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut };

            //Grow the logo by 10%
            var currentScale = floatingState ? 2 : 1;
            var scaleAnimation = new DoubleAnimation(currentScale * 1.1, TimeSpan.FromMilliseconds(300));
            scaleAnimation.EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut };

            floatingBorderRotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
            floatingBorderScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            floatingBorderScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }
        private void FloatingLogoImage_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            //Stop the spinning animation
            var floatingBorderTransformGroup = this.FloatingBorder.RenderTransform as TransformGroup;
            var floatingBorderScaleTransform = floatingBorderTransformGroup.Children[0] as ScaleTransform;
            var floatingBorderRotateTransform = floatingBorderTransformGroup.Children[1] as RotateTransform;

            //Rotate the logo to 0
            var rotateAnimation = new DoubleAnimation(0, TimeSpan.FromMilliseconds(300));
            rotateAnimation.EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut };

            //Shrink the logo back to its original size
            var currentScale = floatingState ? 2 : 1;
            var scaleAnimation = new DoubleAnimation(currentScale, TimeSpan.FromMilliseconds(300));
            scaleAnimation.EasingFunction = new BackEase() { EasingMode = EasingMode.EaseOut };

            floatingBorderRotateTransform.BeginAnimation(RotateTransform.AngleProperty, rotateAnimation);
            floatingBorderScaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleAnimation);
            floatingBorderScaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleAnimation);
        }        
    }
}