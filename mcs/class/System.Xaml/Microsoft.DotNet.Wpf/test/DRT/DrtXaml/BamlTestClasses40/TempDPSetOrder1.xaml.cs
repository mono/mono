using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace BamlTestClasses40
{
    public partial class TempDPSetOrder1 : StackPanel
    {
        public TempDPSetOrder1()
        {
            InitializeComponent();
        }
    }

    public enum Property
    {
        IsSynchronizedWithCurrentItem,
        ItemsSource,
        SelectedIndex
    }

    public class TempDPSetOrder1ListBox : ListBox
    {
        public static List<Property> Order { get; private set; }

        static TempDPSetOrder1ListBox()
        {
            Order = new List<Property>();
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property.OwnerType.Equals(typeof(Selector)))
            {
                switch (e.Property.Name)
                {
                    case "IsSynchronizedWithCurrentItem":
                        Order.Add(Property.IsSynchronizedWithCurrentItem);
                        break;
                    case "SelectedIndex":
                        Order.Add(Property.SelectedIndex);
                        break;
                }
            }
            else if (e.Property.OwnerType.Equals(typeof(ItemsControl)))
            {
                switch (e.Property.Name)
                {
                    case "ItemsSource":
                        Order.Add(Property.ItemsSource);
                        break;
                }
            }
        }
    }
}
