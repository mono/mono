using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Markup;
using System.Windows;
using System.Windows.Controls;

namespace Test.Elements
{
    public class HasTitlePropObsolete : Element
    {
        private string _title;


        public HasTitlePropObsolete()
        {
            this._title = this.GetType().Name;
        }

        [Obsolete("Sample msg", true)]
        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }
    }


    
    /// <summary>
    /// Test class for obsolete Routed Event named 'Tap'
    /// </summary>
    public class HasTapREObsolete : Button
    {

//The following pragma statement disables compiler errors for using obsolete elements
#pragma warning disable 0612

        public static readonly RoutedEvent TapEvent = EventManager.RegisterRoutedEvent("Tap", RoutingStrategy.Bubble, typeof(RoutedEventHandler), typeof(HasTapREObsolete));

        [Obsolete()]
        public event RoutedEventHandler Tap
        {
            add { this.AddHandler(TapEvent, value); }
            remove { this.RemoveHandler(TapEvent, value); }
        }

        public void RaiseTapEvent()
        {
            RoutedEventArgs e = new RoutedEventArgs(HasTapREObsolete.TapEvent);
            RaiseEvent(e);
        }
#pragma warning restore 0612

    }



    /// <summary>
    /// Test class for obsolete Attached property named 'BubbleSource'
    /// </summary>
    public class HasBubbleSourceAPObsolete : DPElement
    {
//The following pragma statement disables compiler errors for using obsolete elements
#pragma warning disable 0612
        
        public static readonly DependencyProperty IsBubbleSourceProperty = DependencyProperty.RegisterAttached(
            "IsBubbleSource", typeof(Boolean), typeof(HasBubbleSourceAPObsolete),
            new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.None));

        public static void SetIsBubbleSource(UIElement element, Boolean value)
        {
            element.SetValue(IsBubbleSourceProperty, value);
        }

        [Obsolete()]
        public static Boolean GetIsBubbleSource(UIElement element)
        {
            return (Boolean)element.GetValue(IsBubbleSourceProperty);
        }
#pragma warning restore 0612

    }




    [Obsolete("Sample msg", false)]
    public class ObsoleteType : Element
    {
        private string _title;

        public ObsoleteType()
        {
            this._title = this.GetType().Name;
        }

        public string Title
        {
            get { return this._title; }
            set { this._title = value; }
        }
    }


    

}
