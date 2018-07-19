//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.ComponentModel;

    partial class SequenceDesigner
    {
        const string ExpandViewStateKey = "IsExpanded";

        public SequenceDesigner()
        {
            this.InitializeComponent();
        }

        protected override void OnModelItemChanged(object newItem)
        {
            // Make sequence designer always expand by default, but only if the user didnt explicitly specify collapsed or expanded.
            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();
            if (viewStateService != null)
            {
                bool? isExpanded = (bool?)viewStateService.RetrieveViewState((ModelItem)newItem, ExpandViewStateKey);
                if (isExpanded == null)
                {
                    viewStateService.StoreViewState((ModelItem)newItem, ExpandViewStateKey, true);
                }
            }
            base.OnModelItemChanged(newItem);

        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(Sequence);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(SequenceDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Activities"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Variables"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Activities"), new ShowPropertyInOutlineViewAttribute() { CurrentPropertyVisible = false });
        }
    }
}
