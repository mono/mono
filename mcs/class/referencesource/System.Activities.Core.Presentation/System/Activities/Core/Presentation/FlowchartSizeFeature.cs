//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Collections.Generic;
    using System.Windows;

    class FlowchartSizeFeature : ViewStateAttachedPropertyFeature
    {
        public const string WidthPropertyName = "Width";
        public const string HeightPropertyName = "Height";
        public const double DefaultWidth = 600;
        public const double DefaultHeight = 600;

        protected override IEnumerable<AttachedPropertyInfo> AttachedProperties
        {
            get
            {
                yield return new AttachedPropertyInfo<Nullable<double>> { IsBrowsable = false, PropertyName = WidthPropertyName, DefaultValue = DefaultWidth };
                yield return new AttachedPropertyInfo<Nullable<double>> { IsBrowsable = false, PropertyName = HeightPropertyName, DefaultValue = DefaultHeight };
            }
        }
    }
}
