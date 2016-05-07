//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Activities.Presentation.Model;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Media;

    [Fx.Tag.XamlVisible(false)]
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class)]
    public sealed class ActivityDesignerOptionsAttribute : Attribute
    {
        public ActivityDesignerOptionsAttribute()
        {
            this.AllowDrillIn = true;
            this.AlwaysCollapseChildren = false;
        }

        public bool AllowDrillIn
        {
            get;
            set;
        }

        public bool AlwaysCollapseChildren
        {
            get;
            set;
        }

        public Func<ModelItem, DrawingBrush> OutlineViewIconProvider
        {
            get;
            set;
        }
    }
}
