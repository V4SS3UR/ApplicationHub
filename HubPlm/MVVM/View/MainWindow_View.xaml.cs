using ApplicationHub.MVVM.ViewModel;
using System;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace ApplicationHub
{
    /// <summary>
    /// Logique d'interaction pour MainWindow.xaml
    /// </summary>
    public partial class MainWindow_View : Window
    {
        public static MainWindow_View Instance;

        public MainWindow_ViewModel ViewModel { get; set; }

        public MainWindow_View()
        {
            InitializeComponent();
            this.StateChanged += MainWindow_View_StateChanged;

            Instance = this;

            this.DataContext = ViewModel = new MainWindow_ViewModel();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            base.OnLocationChanged(e);

            Screen screen = Screen.FromHandle(new WindowInteropHelper(this).Handle);
            if (this.MaxHeight != screen.Bounds.Height)
                this.MaxHeight = screen.Bounds.Height - 2 * 7 - 10;
        }

        private void MainWindow_View_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.BorderThickness = new System.Windows.Thickness(7);
                this.EdgeBorder.CornerRadius = new CornerRadius(0);
            }
            else
            {
                //this.BorderThickness = new System.Windows.Thickness(0);
                this.EdgeBorder.CornerRadius = new CornerRadius(10, 10, 0, 0);
            }
        }

        private void TitleBarBorder_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (this.WindowState == WindowState.Maximized)
                {
                    var point = PointToScreen(e.MouseDevice.GetPosition(this));

                    Left = point.X - (RestoreBounds.Width * 0.5);
                    Top = point.Y;

                    WindowState = WindowState.Normal;
                }
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
    }
}