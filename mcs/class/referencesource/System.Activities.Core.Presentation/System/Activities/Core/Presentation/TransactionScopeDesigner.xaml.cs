//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Statements;

    using System.ComponentModel;

    partial class TransactionScopeDesigner
    {
        public TransactionScopeDesigner()
        {
            this.InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(TransactionScope);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(TransactionScopeDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Body"), BrowsableAttribute.No);
        }
    }
}
