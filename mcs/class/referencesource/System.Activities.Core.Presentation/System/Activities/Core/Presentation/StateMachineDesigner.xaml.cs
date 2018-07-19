//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.ComponentModel;
    using System.Windows;
    using System.Windows.Input;

    [ActivityDesignerOptions(AlwaysCollapseChildren = true)]
    partial class StateMachineDesigner
    {
        const string ExpandViewStateKey = "IsExpanded";
        internal const string InitialStatePropertyName = "InitialState";
        internal const string VariablesPropertyName = "Variables";
        internal const string StatesPropertyName = "States";

        StateContainerEditor stateContainerEditor = null;

        public StateMachineDesigner()
        {
            InitializeComponent();
        }

        internal bool IsResizing { get; set; }

        internal StateContainerEditor StateContainerEditor
        {
            get { return this.stateContainerEditor; }
        }

        void OnStateContainerLoaded(object sender, RoutedEventArgs e)
        {
            this.stateContainerEditor = sender as StateContainerEditor;
        }

        void OnStateContainerUnloaded(object sender, RoutedEventArgs e)
        {
            this.stateContainerEditor = null;
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type stateMachineType = typeof(StateMachine);
            builder.AddCustomAttributes(stateMachineType, new DesignerAttribute(typeof(StateMachineDesigner)));
            builder.AddCustomAttributes(stateMachineType, stateMachineType.GetProperty(StateMachineDesigner.StatesPropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(stateMachineType, stateMachineType.GetProperty(StateMachineDesigner.VariablesPropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(stateMachineType, stateMachineType.GetProperty(StateMachineDesigner.InitialStatePropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(stateMachineType, stateMachineType.GetProperty(StateMachineDesigner.InitialStatePropertyName), new ShowPropertyInOutlineViewAttribute() { DuplicatedChildNodesVisible = true });
            builder.AddCustomAttributes(stateMachineType, stateMachineType.GetProperty(StateMachineDesigner.StatesPropertyName), new ShowPropertyInOutlineViewAttribute());


            builder.AddCustomAttributes(stateMachineType, new FeatureAttribute(typeof(StateMachineValidationErrorSourceLocatorFeature)));
        }

        protected override void OnModelItemChanged(object newItem)
        {
            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();
            if (viewStateService != null)
            {
                // Make StateMachine designer always collapsed by default, but only if the user didn't explicitly specify collapsed or expanded.
                bool? isExpanded = (bool?)viewStateService.RetrieveViewState((ModelItem)newItem, ExpandViewStateKey);
                if (isExpanded == null)
                {
                    viewStateService.StoreViewState((ModelItem)newItem, ExpandViewStateKey, false);
                }
            }
            base.OnModelItemChanged(newItem);
        }

        // do not proprogate up to StateMachineDesigner, because designer will set selection to itself on GotFocus event.
        private void OnAdornerLayerGotFocus(object sender, RoutedEventArgs e)
        {
            e.Handled = true;
        }

        private void StateMachineDesignerKeyDown(object sender, KeyEventArgs e)
        {
            // Ignore KeyBoard input when in resizing mode.
            e.Handled = IsResizing;
        }

        private void StateMachineDesignerPreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Enter cannot be captured in KeyDown, so handle it in PreviewKeyDown event.
            e.Handled = IsResizing && e.Key == Key.Enter;
        }
    }
 }
