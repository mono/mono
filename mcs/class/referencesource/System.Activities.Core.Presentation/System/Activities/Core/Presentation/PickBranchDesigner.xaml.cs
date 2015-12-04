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
    /// Interaction logic for PickBranchDesigner.xaml
    /// </summary>
    partial class PickBranchDesigner
    {
        public PickBranchDesigner()
        {
            this.InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(PickBranch);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(PickBranchDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Action"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Trigger"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, new ShowInOutlineViewAttribute());
        }

        protected override string GetAutomationIdMemberName()
        {
            return "DisplayName";
        }
    }
}
