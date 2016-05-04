//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//---------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System.Windows;
    using System.Runtime;

    [Fx.Tag.XamlVisible(false)]
    sealed class ExtensionWindowClosingRoutedEventArgs : RoutedEventArgs
    {
        internal ExtensionWindowClosingRoutedEventArgs(RoutedEvent routedEvent, object source)
            : base(routedEvent, source)
        {
        }

        public bool Cancel
        {
            get;
            set;
        }
    }
}
