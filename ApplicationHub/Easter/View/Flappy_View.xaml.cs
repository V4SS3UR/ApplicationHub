using ApplicationHub.Easter.ViewModel;
using System.Windows.Controls;

namespace ApplicationHub.Easter.View
{
    /// <summary>
    /// Logique d'interaction pour Flappy_View.xaml
    /// </summary>
    public partial class Flappy_View : UserControl
    {
        public Flappy_View()
        {
            InitializeComponent();
            this.DataContext = new Flappy_ViewModel(this.Flappy);
        }
    }
}
