//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Debug;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Input;
    using System.Windows.Threading;
    using System.Windows.Automation;

    partial class StateDesigner
    {
        Guid guid;

        internal const string EntryPropertyName = "Entry";
        internal const string ExitPropertyName = "Exit";
        internal const string IsFinalPropertyName = "IsFinal";
        internal const string DisplayNamePropertyName = "DisplayName";
        internal const string TransitionsPropertyName = "Transitions";
        internal const string ChildStatesPropertyName = "States";
        internal const string VariablesPropertyName = "Variables";

        const double StateMinWidth = 20;

        public static readonly RoutedCommand SetAsInitialCommand = new RoutedCommand("SetAsInitial", typeof(StateDesigner));

        public StateDesigner()
        {
            InitializeComponent();
            this.guid = Guid.NewGuid();
            this.Collapsible = false;
            this.SetValue(AutomationProperties.ItemStatusProperty, this.guid.ToString());
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            this.PinAsExpanded();
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type stateType = typeof(State);
            builder.AddCustomAttributes(stateType, new DesignerAttribute(typeof(StateDesigner)));
            builder.AddCustomAttributes(stateType, stateType.GetProperty(StateDesigner.EntryPropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(stateType, stateType.GetProperty(StateDesigner.ExitPropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(stateType, stateType.GetProperty(StateDesigner.TransitionsPropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(stateType, stateType.GetProperty(StateDesigner.IsFinalPropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(stateType, stateType.GetProperty(StateDesigner.VariablesPropertyName), BrowsableAttribute.No);

            builder.AddCustomAttributes(stateType, new ShowInOutlineViewAttribute());
            builder.AddCustomAttributes(stateType, new AllowBreakpointAttribute());
            builder.AddCustomAttributes(stateType, stateType.GetProperty(StateDesigner.TransitionsPropertyName), new ShowPropertyInOutlineViewAttribute() { CurrentPropertyVisible = false });
            builder.AddCustomAttributes(stateType, new ActivityDesignerOptionsAttribute
            {
                OutlineViewIconProvider = (modelItem) =>
                {
                    ResourceDictionary icons = EditorResources.GetIcons();

                    if (modelItem != null)
                    {
                        object icon = null;

                        if (StateContainerEditor.IsFinalState(modelItem) && icons.Contains("FinalStateIcon"))
                        {
                            icon = icons["FinalStateIcon"];
                        }
                        else if (icons.Contains("StateIcon"))
                        {
                            icon = icons["StateIcon"];
                        }

                        if (icon != null && icon is DrawingBrush)
                        {
                            return (DrawingBrush)icon;
                        }
                    }

                    return null;
                }
            });
        }

        protected override void OnShowExpandedChanged(bool newValue)
        {
            this.PinAsExpanded();
        }

        // Make sure StateDesigner is always expanded
        void PinAsExpanded()
        {
            this.ExpandState = true;
            this.PinState = true;
        }

        protected override void OnModelItemChanged(object newItem)
        {
            this.MinWidth = StateMinWidth;
            base.OnModelItemChanged(newItem);
        }

        protected internal override string GetAutomationItemStatus()
        {
            string status = base.GetAutomationItemStatus();
            status = status + "Guid=" + this.guid.ToString() + " ";
            status = status + "IsFinal=" + (this.IsFinalState() ? "True " : "False ");
            return status;
        }


        void OnSetAsInitialCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            ModelItem stateMachineModelItem = StateContainerEditor.GetStateMachineModelItem(this.ModelItem);
            e.CanExecute = (!this.IsReadOnly && stateMachineModelItem != null && this.ModelItem != stateMachineModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].Value &&
                            !this.IsFinalState() &&
                            !this.IsRootDesigner && StateContainerEditor.GetEmptyConnectionPoints(this).Count > 0);
            e.Handled = true;
        }

        void OnSetAsInitialExecute(object sender, ExecutedRoutedEventArgs e)
        {
            ModelItem stateMachineModelItem = StateContainerEditor.GetStateMachineModelItem(this.ModelItem);

            using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.SetInitialState))
            {
                this.ViewStateService.RemoveViewState(stateMachineModelItem, StateContainerEditor.ConnectorLocationViewStateKey);
                stateMachineModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].SetValue(this.ModelItem.GetCurrentValue());
                es.Complete();
            }
            e.Handled = true;
        }

        void OnStateSpecificMenuItemLoaded(object sender, RoutedEventArgs e)
        {
            MenuItem item = sender as MenuItem;
            if (null != item && (this.IsFinalState()))
            {
                item.Visibility = Visibility.Collapsed;
            }
            e.Handled = true;
        }

        internal bool IsFinalState()
        {
            return StateContainerEditor.IsFinalState(this.ModelItem);
        }

        public FreeFormPanel GetStateMachineFreeFormPanel()
        {
            StateDesigner current = this;
            WorkflowViewElement parent = VisualTreeUtils.FindVisualAncestor<WorkflowViewElement>(current);
            while (parent != null)
            {
                if (parent is StateDesigner)
                {
                    current = (StateDesigner)parent;
                    parent = VisualTreeUtils.FindVisualAncestor<WorkflowViewElement>(current);
                }
                else if (parent is StateMachineDesigner)
                {
                    return VisualTreeUtils.FindVisualAncestor<FreeFormPanel>(current);
                }
                else
                {
                    return null;
                }
            }

            return null;
        }

        private void OnTransitionClicked(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;

            if (button != null)
            {
                ModelItem transitionModelItem = button.Tag as ModelItem;

                if (transitionModelItem != null)
                {
                    this.Designer.MakeRootDesigner(transitionModelItem);
                }
            }
        }

        private void OnToStateClicked(object sender, RoutedEventArgs e)
        {
            Button button = e.Source as Button;

            if (button != null)
            {
                ModelItem toStateModelItem = button.Tag as ModelItem;

                if (toStateModelItem != null)
                {
                    this.Designer.MakeRootDesigner(toStateModelItem);
                }
            }
        }

        private void StateDesignerToolTipOpening(object sender, ToolTipEventArgs e)
        {
            StateContainerEditor stateContainerEditor = (StateContainerEditor)sender;

            if (StateContainerEditor.CopiedTransitionDestinationState != null)
            {
                WorkflowViewElement view = VisualTreeUtils.FindVisualAncestor<WorkflowViewElement>(stateContainerEditor);
                if (view != null)
                {
                    StateContainerEditor container = (StateContainerEditor)DragDropHelper.GetCompositeView(view);
                    string errorMessage;
                    if (container != null && container.CanPasteTransition(StateContainerEditor.CopiedTransitionDestinationState, out errorMessage, view.ModelItem))
                    {
                        stateContainerEditor.ToolTip = SR.EditStateToolTip + Environment.NewLine + SR.PasteTransitionToolTip;
                        return;
                    }
                }
            }

            stateContainerEditor.ToolTip = SR.EditStateToolTip;
        }
    }
}
