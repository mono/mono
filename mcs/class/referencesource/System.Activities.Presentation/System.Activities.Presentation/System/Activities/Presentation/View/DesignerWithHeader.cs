//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.View
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Windows;

    class DesignerWithHeader : WorkflowViewElement
    {

        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(DataTemplate), typeof(DesignerWithHeader), new UIPropertyMetadata(null));
        public DataTemplate Header
        {
            get { return (DataTemplate)GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }
    }
}
