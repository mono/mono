//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Statements;
    using System.ComponentModel;

    partial class NoPersistScopeDesigner
    {
        public const string BodyPropertyName = "Body";

        public NoPersistScopeDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(NoPersistScope);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(NoPersistScopeDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty(NoPersistScopeDesigner.BodyPropertyName), BrowsableAttribute.No);
        }
    }
}
