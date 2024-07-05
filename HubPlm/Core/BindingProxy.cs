using System.Windows;

namespace ApplicationHub.Core
{
    public class BindingProxy : Freezable
    {
        public static readonly DependencyProperty MyDataProperty =
            DependencyProperty.Register("MyData", typeof(object), typeof(BindingProxy), new UIPropertyMetadata(null));

        public object MyData
        {
            get { return (object)GetValue(MyDataProperty); }
            set { SetValue(MyDataProperty, value); }
        }

        protected override Freezable CreateInstanceCore()
        {
            return new BindingProxy();
        }     
    }
}
