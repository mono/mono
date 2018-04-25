//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Activities.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.ComponentModel;
    using System.ServiceModel.Activities;

    partial class TransactedReceiveScopeDesigner
    {
        public TransactedReceiveScopeDesigner()
        {
            this.InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(TransactedReceiveScope);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(TransactedReceiveScopeDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Body"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Request"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);
        }
    }
}
