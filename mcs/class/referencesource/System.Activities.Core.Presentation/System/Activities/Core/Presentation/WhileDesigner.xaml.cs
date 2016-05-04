//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.ComponentModel;

    /// <summary>
    /// Interaction logic for WhileDesigner.xaml
    /// </summary>
    partial class WhileDesigner
    {
        public WhileDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(While);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(WhileDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Body"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);

            builder.AddCustomAttributes(type, type.GetProperty("Condition"), new HidePropertyInOutlineViewAttribute());
        }
    }
}
