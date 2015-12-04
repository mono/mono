//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Input;

    partial class PickDesigner
    {
        public static readonly RoutedCommand CreateBranchCommand = new RoutedCommand("CreateBranchCommand", typeof(PickDesigner));
        WorkflowItemsPresenter branchesPresenter;

        public PickDesigner()
        {
            this.InitializeComponent();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type type = typeof(System.Activities.Statements.Pick);
            builder.AddCustomAttributes(type, new DesignerAttribute(typeof(PickDesigner)));
            builder.AddCustomAttributes(type, type.GetProperty("Branches"), BrowsableAttribute.No);
            builder.AddCustomAttributes(type, type.GetProperty("Branches"), new ShowPropertyInOutlineViewAttribute() { CurrentPropertyVisible = false });
            builder.AddCustomAttributes(type, new FeatureAttribute(typeof(PickValidationErrorSourceLocatorFeature)));
        }

        void OnBranchesPresenterLoaded(object sender, RoutedEventArgs e)
        {
            this.branchesPresenter = (WorkflowItemsPresenter)sender;
        }

        void OnBranchesPresenterUnloaded(object sender, RoutedEventArgs e)
        {
            this.branchesPresenter = null;
        }

        void OnCreateBranchCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (!e.Handled)
            {
                // Simulate a PickBranch being drop onto the Branches WIsP.
                object instance = DragDropHelper.GetDroppedObjectInstance(this.branchesPresenter, this.Context, typeof(PickBranch), null);
                if (instance != null)
                {
                    this.ModelItem.Properties["Branches"].Collection.Add(instance);
                }
                e.Handled = true;
            }
        }
    }
}
