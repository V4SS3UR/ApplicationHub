using ApplicationHub.MVVM.View;
using ApplicationHub.MVVM.ViewModel;
using ApplicationHub.Properties;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Windows.Threading;

namespace ApplicationHub
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow_View : Window
    {
        public static MainWindow_View Instance;

        public MainWindow_ViewModel ViewModel { get; set; }


        private bool floatingState
        {
            get => Settings.Default.FloatingState;
            set
            {
                Settings.Default.FloatingState = value;
                Settings.Default.Save();
            }
        }


        private double baseHeight;
        private double baseWidth;
        private double baseLeft;
        private double baseTop;
        private Stopwatch stopwatch;

        private Screen currentScreen;
        private Window minimalWindow;
        private DispatcherTimer mouseCheckTimer;

        private Rectangle workingAreaBounds;

        public MainWindow_View()
        {
            Instance = this;

            InitializeComponent();

            this.DataContext = ViewModel = new MainWindow_ViewModel();

            stopwatch = new Stopwatch();

            currentScreen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            
            this.Loaded += MainWindow_View_Loaded;
        }

        private void MainWindow_View_Loaded(object sender, RoutedEventArgs e)
        {
            workingAreaBounds = new Rectangle(int.MaxValue, int.MaxValue, int.MinValue, int.MinValue);
            foreach (Screen screen in Screen.AllScreens)
            {
                workingAreaBounds = Rectangle.Union(workingAreaBounds, screen.Bounds);
            }

            if (floatingState)
            {
                currentScreen = Screen.PrimaryScreen;

                SetFloatingState();

                //Snap the window to the screen center if it goes out of bounds
                if (this.Left < workingAreaBounds.Left || this.Top < workingAreaBounds.Top || this.Left + this.Width > workingAreaBounds.Right || this.Top + this.Height > workingAreaBounds.Bottom)
                {
                    var middleX = Screen.PrimaryScreen.WorkingArea.Width / 2;
                    var middleY = Screen.PrimaryScreen.WorkingArea.Height / 2;

                    AnimatePosition(middleX, middleY, TimeSpan.FromSeconds(1), TimeSpan.Zero);
                }
            }
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            currentScreen = Screen.FromHandle(new WindowInteropHelper(this).Handle);

            if (floatingState && minimalWindow != null)
            {
                minimalWindow.Close();
                minimalWindow = null;
            }
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
            SetFloatingState();
            //this.WindowState = WindowState.Minimized;
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
                        if(floatingState)
                        {
                            UnsetFloatingState();
                        }
                        else
                        {
                            SetFloatingState();
                        }
                    }
                }

                stopwatch.Stop();
                stopwatch.Reset();
            }
        }
        private void SetFloatingState()
        {
            var edgeBorderScaleTransform = this.EdgeBorder.RenderTransform as ScaleTransform;

            var floatingBorderTransformGroup = this.FloatingBorder.RenderTransform as TransformGroup;
            var floatingBorderScaleTransform = floatingBorderTransformGroup.Children[0] as ScaleTransform;

            floatingState = true;

            baseHeight = this.ActualHeight == 0 ? 600 : this.ActualHeight;
            baseWidth = this.ActualWidth == 0 ? 700 : this.ActualWidth;
            baseLeft = double.IsNaN(this.Left) ? 0 : this.Left;
            baseTop = double.IsNaN(this.Top) ? 0 : this.Top;

            this.Topmost = true;
            this.SizeToContent = SizeToContent.Width;
            this.MinHeight = this.Height = 100;
            this.MinWidth = this.Width = 100;    
            this.ResizeMode = ResizeMode.NoResize;
            this.EdgeBorder.Visibility = Visibility.Collapsed;                
            this.FloatingBorder.Margin = new Thickness(10);
            this.FloatingBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
            this.FloatingBorder.VerticalAlignment = System.Windows.VerticalAlignment.Center;
            this.FloatingBorder.Effect = new DropShadowEffect()
            {
                Color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#8000"),
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
        private void UnsetFloatingState()
        {
            var edgeBorderScaleTransform = this.EdgeBorder.RenderTransform as ScaleTransform;

            var floatingBorderTransformGroup = this.FloatingBorder.RenderTransform as TransformGroup;
            var floatingBorderScaleTransform = floatingBorderTransformGroup.Children[0] as ScaleTransform;

            
            floatingState = false;
            CloseMinimalWindow();

            //Position the window at the last known position
            AnimateTransform(edgeBorderScaleTransform, 1, 1, floatingState, TimeSpan.FromMilliseconds(150));
            AnimateTransform(floatingBorderScaleTransform, 1, 1, floatingState, TimeSpan.FromMilliseconds(150));
            AnimatePosition(baseLeft, baseTop, TimeSpan.Zero, TimeSpan.FromMilliseconds(300));

            this.Topmost = false;
            this.SizeToContent = SizeToContent.Manual;
            this.Height = baseHeight;
            this.Width = baseWidth;
            this.ResizeMode = ResizeMode.CanResize;
            this.FloatingBorder.Effect = null;
            this.FloatingBorder.Margin = new Thickness(5);
            this.FloatingBorder.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            this.FloatingBorder.VerticalAlignment = System.Windows.VerticalAlignment.Top;
            this.EdgeBorder.Visibility = Visibility.Visible;
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

            //Convert to local screen space
            left -= screenBounds.Left;
            top -= screenBounds.Top;                        

            // Determine the nearest screen edge
            var minHorizontalDistance = Math.Min(left, right);
            var minVerticalDistance = Math.Min(top, bottom);
            var minDistance = Math.Min(minHorizontalDistance, minVerticalDistance);

            //trigger only if the window is closer to the edge than the threshold
            double threshold = 150;
            double margin = 0;

            if (minDistance <= threshold)
            {                
                //Near corner, snap to corner
                if (minHorizontalDistance < threshold && minVerticalDistance < threshold)
                {
                    if(minHorizontalDistance == left)
                    {
                        targetLeft = screenBounds.Left + margin; 
                    }
                    else if(minHorizontalDistance == right)
                    {
                        targetLeft = screenBounds.Right - this.Width - margin;
                    }
                    if (minVerticalDistance == top)
                    {
                        targetTop = screenBounds.Top + margin;
                    }
                    else if (minVerticalDistance == bottom)
                    {
                        targetTop = screenBounds.Bottom - this.Height - margin;
                    }
                }
                else
                {
                    if (minDistance == left)
                    {
                        targetLeft = screenBounds.Left + margin;
                    }
                    else if (minDistance == right)
                    {
                        targetLeft = screenBounds.Right - this.Width - margin;
                    }
                    if (minDistance == top)
                    {
                        targetTop = screenBounds.Top + margin;
                    }
                    else if (minDistance == bottom)
                    {
                        targetTop = screenBounds.Bottom - this.Height - margin;
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

            //Ensure non Nan values
            var fromLeft = double.IsNaN(this.Left) ? 0 : this.Left;
            var fromTop = double.IsNaN(this.Top) ? 0 : this.Top;

            var leftAnimation = new DoubleAnimation(fromLeft, left, duration);
            var topAnimation = new DoubleAnimation(fromTop, top, duration);

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


            //Create a sub window below the floating logo with a list of all the applications
            if (floatingState)
            {
                CreateMinimalWindow();
            }

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



        

        private void CreateMinimalWindow()
        {
            if(minimalWindow != null)
            {
                return;
            }

            minimalWindow = new Window()
            {
                Owner = this,
                SizeToContent = SizeToContent.WidthAndHeight,
                WindowStyle = WindowStyle.None,
                ResizeMode = ResizeMode.NoResize,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Background = new SolidColorBrush(Colors.Transparent),
                Content = new MinimalListe_View()
                {
                    DataContext = ((MainWindow_ViewModel)this.DataContext).CurrentView.DataContext,
                    RenderTransform = new TranslateTransform()
                }
            };

            minimalWindow.Loaded += MinimalWindow_Loaded;
            minimalWindow.Show();
        }
        private void MinimalWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Set initial position behind the main window
            minimalWindow.Left = this.Left + (this.Width - minimalWindow.ActualWidth) / 2;
            minimalWindow.Top = this.Top + (this.Height - minimalWindow.ActualHeight) / 2;
            minimalWindow.Opacity = 0; // Start fully transparent

            // Determine docking side
            bool isDockedLeft = this.Left <= (currentScreen.WorkingArea.X + currentScreen.WorkingArea.Width / 2);
            var margin = -10;
            var destinationLeft = isDockedLeft ? this.Left + this.Width + margin : this.Left - minimalWindow.ActualWidth - margin;

            // Animate appearance
            var leftAnimation = new DoubleAnimation(this.Left + (this.Width / 2), destinationLeft, TimeSpan.FromMilliseconds(300));
            leftAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };

            var opacityAnimation = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(200));
            opacityAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseOut };

            minimalWindow.BeginAnimation(Window.LeftProperty, leftAnimation);
            minimalWindow.BeginAnimation(Window.OpacityProperty, opacityAnimation);

            // Set up a timer to check mouse position
            mouseCheckTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            mouseCheckTimer.Tick += MouseCheckTimer_Tick;
            mouseCheckTimer.Start();
        }


        private void MouseCheckTimer_Tick(object sender, EventArgs e)
        {
            if(minimalWindow == null)
            {
                return;
            }

            double margin = 10;

            // Get the mouse position relative to the screen
            var mousePosition = System.Windows.Forms.Cursor.Position;

            // Convert screen coordinates to WPF coordinates
            var presentationSource = PresentationSource.FromVisual(this);
            if (presentationSource != null)
            {
                var transform = presentationSource.CompositionTarget.TransformFromDevice;
                var mousePoint = transform.Transform(new System.Windows.Point(mousePosition.X, mousePosition.Y));

                // Check if mouse is within marginpx of the floating window or the minimal window
                bool isMouseOverFloatingOrMinimal =
                    IsPointInBounds(mousePoint, new Rect(this.Left - margin, this.Top - margin, this.Width + 100, this.Height + 100)) ||
                    IsPointInBounds(mousePoint, new Rect(minimalWindow.Left - margin, minimalWindow.Top - margin, minimalWindow.ActualWidth + 100, minimalWindow.ActualHeight + 100));

                if (!isMouseOverFloatingOrMinimal)
                {
                    CloseMinimalWindow();
                }
            }
        }

        private bool IsPointInBounds(System.Windows.Point point, Rect bounds)
        {
            return bounds.Contains(point);
        }

        private void CloseMinimalWindow()
        {
            if (minimalWindow != null)
            {
                // Determine docking side
                bool isDockedLeft = this.Left < (currentScreen.WorkingArea.X + currentScreen.WorkingArea.Width / 2);
                var closeDestinationLeft = isDockedLeft ? this.Left - minimalWindow.ActualWidth : this.Left + this.Width;

                // Animate closing
                var closeLeftAnimation = new DoubleAnimation(minimalWindow.Left, closeDestinationLeft, TimeSpan.FromMilliseconds(300));
                closeLeftAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };

                var closeOpacityAnimation = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(200));
                closeOpacityAnimation.EasingFunction = new CubicEase() { EasingMode = EasingMode.EaseIn };

                closeOpacityAnimation.Completed += (s, e) =>
                {
                    minimalWindow?.Close();
                    minimalWindow = null;
                };

                minimalWindow.BeginAnimation(Window.LeftProperty, closeLeftAnimation);
                minimalWindow.BeginAnimation(Window.OpacityProperty, closeOpacityAnimation);
            }

            if (mouseCheckTimer != null)
            {
                mouseCheckTimer.Stop();
                mouseCheckTimer.Tick -= MouseCheckTimer_Tick;
                mouseCheckTimer = null;
            }
        }
    }
}