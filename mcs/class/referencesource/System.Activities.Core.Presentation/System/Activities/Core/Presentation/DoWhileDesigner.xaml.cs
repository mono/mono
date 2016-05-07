//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Statements;
    using System.ComponentModel;

    /// <summary>
    /// Interaction logic for WhileDesigner.xaml
    /// </summary>
    partial class DoWhileDesigner
    {
        public DoWhileDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(DoWhile);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(DoWhileDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Body"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);
        }
    }
}
