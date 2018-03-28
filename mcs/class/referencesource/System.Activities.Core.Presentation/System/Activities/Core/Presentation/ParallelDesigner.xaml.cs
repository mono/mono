//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.ComponentModel;

    // <summary>
    // Paralle Designer
    // </summary>
    partial class ParallelDesigner
    {
        public ParallelDesigner()
        {
            InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(Parallel);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(ParallelDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Branches"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);

            builder.AddCustomAttributes(type, type.GetProperty("Branches"), new ShowPropertyInOutlineViewAttribute() { CurrentPropertyVisible = false });
            builder.AddCustomAttributes(type, type.GetProperty("CompletionCondition"), new HidePropertyInOutlineViewAttribute());
        }
    }
}
