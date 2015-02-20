//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System;
    using System.Activities.Presentation.Metadata;
    using System.ComponentModel;
    using System.Activities.Presentation;

    partial class CorrelationScopeDesigner 
    {
        public CorrelationScopeDesigner()
        {
            InitializeComponent();
        }

        internal static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(CorrelationScope);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(CorrelationScopeDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Body"), BrowsableAttribute.No);
        }
    }
}
