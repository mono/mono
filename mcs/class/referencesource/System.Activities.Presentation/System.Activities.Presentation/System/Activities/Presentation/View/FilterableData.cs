//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Windows;
    using System.Globalization;
    using System.Runtime;

    class FilterableData : DependencyObject
    {
        public static readonly DependencyProperty DataProperty =
            DependencyProperty.Register("Data", typeof(object), typeof(FilterableData), new UIPropertyMetadata(null));

        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(FilterableData), new UIPropertyMetadata(false));

        public static readonly DependencyProperty VisibilityProperty =
            DependencyProperty.Register("Visibility", typeof(Visibility), typeof(FilterableData), new UIPropertyMetadata(Visibility.Visible));

        public object Data
        {
            get { return (object)GetValue(DataProperty); }
            set { SetValue(DataProperty, value); }
        }

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        public Visibility Visibility
        {
            get { return (Visibility)GetValue(VisibilityProperty); }
            set { SetValue(VisibilityProperty, value); }
        }

        internal object Owner
        {
            get;
            set;
        }

        public override string ToString()
        {
            return null == this.Data ? "<null>" : this.Data.ToString();
        }
    }

    class FilterableData<TData> : FilterableData
    {
        [Fx.Tag.KnownXamlExternal]
        public TData TypedData
        {
            get { return (TData)base.Data; }
            set { base.Data = value; }
        }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentUICulture, "{0}:({1})", base.ToString(), typeof(TData).Name);
        }
    }
}
