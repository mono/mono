//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Statements;

    using System.ComponentModel;

    partial class CancellationScopeDesigner
    {
        public CancellationScopeDesigner()
        {
            this.InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(CancellationScope);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(CancellationScopeDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Body"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("CancellationHandler"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);
        }
    }
}
