//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Metadata;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.View.OutlineView;
    using System.Activities.Statements;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls.Primitives;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Controls;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    partial class TransitionDesigner
    {
        public static readonly DependencyProperty NewTransitionProperty = DependencyProperty.Register("NewTransition", typeof(object), typeof(TransitionDesigner), new PropertyMetadata(null));
        public static readonly DependencyProperty TransitionsSharingTriggerProperty = DependencyProperty.Register("TransitionsSharingTrigger", typeof(ObservableCollection<ExpandableItemWrapper>), typeof(TransitionDesigner), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedTransitionProperty = DependencyProperty.Register("SelectedTransition", typeof(ExpandableItemWrapper), typeof(TransitionDesigner), new PropertyMetadata(null));

        internal const string TriggerPropertyName = "Trigger";
        internal const string ActionPropertyName = "Action";
        internal const string ToPropertyName = "To";
        internal const string ConditionPropertyName = "Condition";
        const string ExpandViewStateKey = "IsExpanded";
        const int TotalFreeConnectionPointNum = StateContainerEditor.ConnectionPointNum * 4;

        private ModelItem parentStateModelItem = null;
        private bool suppressUpdatingTransitionsSharingTrigger = false;
        private bool isPopulated = false;
        private CaseKeyBox addNewTransitionBox = null;
        private TextBlock addNewTransitionLabel = null;

        [SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public TransitionDesigner()
        {
            InitializeComponent();
            this.TransitionsSharingTrigger = new ObservableCollection<ExpandableItemWrapper>();

            this.Loaded += (sender, e) =>
            {
                if (!this.isPopulated)
                {
                    this.isPopulated = true;
                    this.TransitionsSharingTrigger.CollectionChanged += OnTransitionsCollectionChanged;
                    this.ModelItem.PropertyChanged += OnModelItemPropertyChanged;
                    this.parentStateModelItem = StateContainerEditor.GetParentStateModelItemForTransition(this.ModelItem);
                    this.parentStateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection.CollectionChanged += OnTransitionsModelItemCollectionChanged;
                    ExpandableItemWrapper selectedItem = this.UpdateTransitionsSharingTrigger();
                    if (null != selectedItem)
                    {
                        this.SelectedTransition = selectedItem;
                    }
                }
            };

            this.Unloaded += (sender, e) =>
            {
                if (this.isPopulated)
                {
                    this.isPopulated = false;
                    this.TransitionsSharingTrigger.Clear();
                    this.TransitionsSharingTrigger.CollectionChanged -= OnTransitionsCollectionChanged;
                    this.ModelItem.PropertyChanged -= OnModelItemPropertyChanged;
                    this.parentStateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection.CollectionChanged -= OnTransitionsModelItemCollectionChanged;
                    this.SelectedTransition = null;
                    this.parentStateModelItem = null;
                }
            };
        }

        public object NewTransition
        {
            get { return (object)this.GetValue(NewTransitionProperty); }
            set { this.SetValue(NewTransitionProperty, value); }
        }

        public ObservableCollection<ExpandableItemWrapper> TransitionsSharingTrigger
        {
            get { return (ObservableCollection<ExpandableItemWrapper>)this.GetValue(TransitionsSharingTriggerProperty); }
            set { this.SetValue(TransitionsSharingTriggerProperty, value); }
        }

        public ExpandableItemWrapper SelectedTransition 
        {
            get { return (ExpandableItemWrapper)this.GetValue(SelectedTransitionProperty); }
            set { this.SetValue(SelectedTransitionProperty, value); }
        }

        private void OnNewTransitionLoaded(object sender, RoutedEventArgs e)
        {
            this.addNewTransitionBox = (CaseKeyBox)sender;
        }

        private void OnNewTransitionUnloaded(object sender, RoutedEventArgs e)
        {
            this.addNewTransitionBox = null;
        }

        private ExpandableItemWrapper UpdateTransitionsSharingTrigger()
        {
            ExpandableItemWrapper wrapper = null;
            if (!this.suppressUpdatingTransitionsSharingTrigger)
            {
                this.TransitionsSharingTrigger.Clear();
                bool expandTargetTransition = true;
                object expandCollapseTargetViewState = this.ViewStateService.RetrieveViewState(this.ModelItem, ExpandViewStateKey);

                if (expandCollapseTargetViewState != null)
                {
                    expandTargetTransition = (bool)expandCollapseTargetViewState;
                }

                wrapper = new ExpandableItemWrapper()
                {
                    Item = this.ModelItem,
                    IsExpanded = expandTargetTransition
                };
                ModelItem triggerModelItem = this.ModelItem.Properties[TriggerPropertyName].Value;
                if (triggerModelItem != null)
                {
                    foreach (ModelItem transitionModelItem in this.parentStateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection)
                    {
                        if (transitionModelItem != this.ModelItem)
                        {
                            if (triggerModelItem == transitionModelItem.Properties[TriggerPropertyName].Value)
                            {
                                bool expandTransition = false;
                                object expandCollapseViewState = this.ViewStateService.RetrieveViewState(transitionModelItem, ExpandViewStateKey);
                                if (expandCollapseViewState != null)
                                {
                                    expandTransition = (bool)expandCollapseViewState;
                                }

                                this.TransitionsSharingTrigger.Add(new ExpandableItemWrapper()
                                {
                                    Item = transitionModelItem,
                                    IsExpanded = expandTransition
                                });
                            }
                        }
                        else
                        {
                            this.TransitionsSharingTrigger.Add(wrapper);
                        }
                    }
                }
                // Connectors starting from the same point should share the same trigger
                else
                {
                    PointCollection thisPointCollection = this.ViewStateService.RetrieveViewState(this.ModelItem, StateContainerEditor.ConnectorLocationViewStateKey) as PointCollection;
                    if (thisPointCollection != null && thisPointCollection.Count > 1)
                    {
                        foreach (ModelItem transitionModelItem in this.parentStateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection)
                        {
                            if (transitionModelItem != this.ModelItem)
                            {
                                PointCollection pointCollection = this.ViewStateService.RetrieveViewState(transitionModelItem, StateContainerEditor.ConnectorLocationViewStateKey) as PointCollection;
                                if (pointCollection != null && pointCollection.Count > 0)
                                {
                                    if (pointCollection[0].IsEqualTo(thisPointCollection[0]))
                                    {
                                        Fx.Assert(transitionModelItem.Properties[TriggerPropertyName].Value == null, "Transition trigger should be null.");
                                        bool expandTransition = false;
                                        object expandCollapseViewState = this.ViewStateService.RetrieveViewState(transitionModelItem, ExpandViewStateKey);

                                        if (expandCollapseViewState != null)
                                        {
                                            expandTransition = (bool)expandCollapseViewState;
                                        }

                                        this.TransitionsSharingTrigger.Add(new ExpandableItemWrapper()
                                        {
                                            Item = transitionModelItem,
                                            IsExpanded = expandTransition
                                        });
                                    }
                                }
                            }
                            else
                            {
                                this.TransitionsSharingTrigger.Add(wrapper);
                            }
                        }
                    }
                }
            }

            return wrapper;
        }

        public static void RegisterMetadata(AttributeTableBuilder builder)
        {
            Type transitionType = typeof(Transition);
            builder.AddCustomAttributes(transitionType, new DesignerAttribute(typeof(TransitionDesigner)));
            builder.AddCustomAttributes(transitionType, transitionType.GetProperty(TransitionDesigner.TriggerPropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(transitionType, transitionType.GetProperty(TransitionDesigner.ActionPropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(transitionType, transitionType.GetProperty(TransitionDesigner.ToPropertyName), BrowsableAttribute.No);
            builder.AddCustomAttributes(transitionType, transitionType.GetProperty(TransitionDesigner.ConditionPropertyName), new HidePropertyInOutlineViewAttribute());
        }

        private static void SwapItems(ModelItemCollection collection, ModelItem modelItem1, ModelItem modelItem2)
        {
            int index1 = collection.IndexOf(modelItem1);
            int index2 = collection.IndexOf(modelItem2);
            collection.Remove(modelItem1);
            collection.Insert(index2, modelItem1);
            collection.Remove(modelItem2);
            collection.Insert(index1, modelItem2);
        }

        private void OnTransitionsModelItemCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.UpdateTransitionsSharingTrigger();
        }

        private void OnModelItemPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // Update the Trigger property for all transitions that share the trigger
            if (e.PropertyName.Equals(TriggerPropertyName) && this.TransitionsSharingTrigger.Count > 0)
            {
                foreach (ExpandableItemWrapper wrapper in this.TransitionsSharingTrigger)
                {
                    if (wrapper.Item != this.ModelItem)
                    {
                        wrapper.Item.Properties[TriggerPropertyName].SetValue(this.ModelItem.Properties[TriggerPropertyName].Value);
                    }
                }
            }
        }

        private void OnTransitionsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Move)
            {
                try
                {
                    // We are updating the Transitions collection in the parent State in response to
                    // the changes in this.TransitionsSharingTrigger. We don't want to update it again
                    // to introduce a dead loop
                    this.suppressUpdatingTransitionsSharingTrigger = true;
                    using (EditingScope scope = (EditingScope)this.ModelItem.BeginEdit(SR.ReorderItems))
                    {
                        ModelItem movedModelItem = this.TransitionsSharingTrigger[e.NewStartingIndex].Item;
                        ModelItemCollection transitionsCollection = this.parentStateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection;
                        // moving down
                        if (e.OldStartingIndex < e.NewStartingIndex)
                        {
                            ModelItem nextModelItem = this.TransitionsSharingTrigger[e.OldStartingIndex].Item;
                            SwapItems(transitionsCollection, movedModelItem, nextModelItem);
                        }
                        // moving up
                        else if (e.OldStartingIndex > e.NewStartingIndex)
                        {
                            ModelItem previousModelItem = this.TransitionsSharingTrigger[e.OldStartingIndex].Item;
                            SwapItems(transitionsCollection, previousModelItem, movedModelItem);
                        }
                        this.Context.Services.GetService<ModelTreeManager>().AddToCurrentEditingScope(new TransitionReorderChange());
                        scope.Complete();
                    }
                }
                finally
                {
                    this.suppressUpdatingTransitionsSharingTrigger = false;
                }
            }
        }

        private void OnDestinationStateClicked(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            ModelItem destinationState = button.Tag as ModelItem;
            if (destinationState != null)
            {
                this.Designer.MakeRootDesigner(destinationState);
            }
        }

        private void OnSourceStateClicked(object sender, RoutedEventArgs e)
        {
            ModelItem sourceState = StateContainerEditor.GetParentStateModelItemForTransition(this.ModelItem);
            if (sourceState != null)
            {
                this.Designer.MakeRootDesigner(sourceState);
            }
        }

        private void OnExpandCollapseButtonClicked(object sender, RoutedEventArgs e)
        {
            ToggleButton button = (ToggleButton)sender;
            ListBox listBox = VisualTreeUtils.FindVisualAncestor<ListBox>(button);
            ExpandableItemWrapper wrapper = (ExpandableItemWrapper)listBox.SelectedItem;
            wrapper.IsExpanded = button.IsChecked.Value;
            this.ViewStateService.StoreViewState(wrapper.Item, ExpandViewStateKey, button.IsChecked.Value);

            if ((wrapper.IsExpanded && this.Designer.ShouldCollapseAll) || !wrapper.IsExpanded && this.Designer.ShouldExpandAll)
            {
                // Pin the item so that it can still be expanded / collapsed when CollapseAll / ExpandAll is enabled
                wrapper.IsPinned = true;
            }
        }

        void OnTransitionNameTextBoxLostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            textBox.ScrollToHome();
        }

        void OnTransitionNameTextBoxContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            // to disable the context menu
            e.Handled = true;
        }

        void OnCopyCommandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            Selection selection = this.Context.Items.GetValue<Selection>();
            DesignerView designerView = this.Context.Services.GetService<DesignerView>();

            if (null != selection &&
                selection.SelectedObjects.Contains(this.ModelItem))
            {
                // Copy is intentionally disabled, when the root (TransitionDesigner)
                // is selected, because we don't support transition copy on the FreeFormPanel.
                e.CanExecute = false;
            }
            else
            {
                e.CanExecute = !this.IsReadOnly && CutCopyPasteHelper.CanCopy(this.Context);
            }

            e.ContinueRouting = false;
            e.Handled = true;
        }

        void OnCopyCommandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            this.Context.Services.GetService<DesignerPerfEventProvider>().WorkflowDesignerCopyStart();
            CutCopyPasteHelper.DoCopy(this.Context);
            e.Handled = true;
            this.Context.Services.GetService<DesignerPerfEventProvider>().WorkflowDesignerCopyEnd();
        }

        private void OnWIPPreviewDragEnter(object sender, DragEventArgs e)
        {
            // We want to disable hover-to-expand for WIPs when dragging from the designer surface.
            // This is because after hover-to-expand, the transition designer will be unloaded. 
            // As a result, 1) there is no way to update other transitions if the shared trigger is updated, and 
            // 2) The ReorderableListEditor will be cleared and there is no way to update the source container
            // for actions if actions are updated.

            WorkflowItemPresenter presenter = (WorkflowItemPresenter)sender;
            if (presenter.Item != null && DragDropHelper.GetDraggedModelItems(e).Count<ModelItem>() > 0)
            {
                WorkflowViewElement topmostWFViewElement = this.FindTopmostWorkflowViewelementByHitTest(
                    presenter, e.GetPosition(presenter));
                bool isAlreadyExpanded = topmostWFViewElement != null ? topmostWFViewElement.ShowExpanded : false;
                if (!isAlreadyExpanded)
                {
                    // Handling the DragEnter would not only disable Auot-expand but also Auto-surround UI gesture (Bug 202880).
                    // To circumvent this problem, a new method (ShowSpacerHelperOnDraggedItems) is used to show
                    // the spacer directly.
                    presenter.ShowSpacerHelperOnDraggedItems(e);
                    e.Handled = true;
                }
            }
        }

        void OnAddNewTransitionLabelLoaded(object sender, RoutedEventArgs e)
        {
            this.addNewTransitionLabel = (TextBlock)sender;
            this.addNewTransitionLabel.Visibility = Visibility.Collapsed;
        }

        void OnAddNewTransitionLabelUnloaded(object sender, RoutedEventArgs e)
        {
            this.addNewTransitionLabel = null;
        }
        
        void OnNewTransitionTextBoxGotFocus(object sender, RoutedEventArgs e)
        {
            this.addNewTransitionLabel.Visibility = Visibility.Visible;
            this.addNewTransitionBox.ViewModel.ComboBoxItems = new ObservableCollection<string>(
                GetAvailableStates().Select(
                modelItem => (modelItem.Properties[StateDesigner.DisplayNamePropertyName].ComputedValue as string)).Where(
                    displayName => !string.IsNullOrEmpty(displayName)).Distinct());
        }

        void OnNewTransitionCommitted(object sender, RoutedEventArgs e)
        {
            this.addNewTransitionLabel.Visibility = Visibility.Collapsed;

            try
            {
                string selectedItem = this.NewTransition as string;

                if (null != selectedItem)
                {
                    AddNewTransition(selectedItem);
                }

                this.addNewTransitionBox.ResetText();
            }
            catch (ArgumentException ex)
            {
                ErrorReporting.ShowErrorMessage(ex.Message);
            }
        }

        void OnNewTransitionEditCancelled(object sender, RoutedEventArgs e)
        {
            this.addNewTransitionLabel.Visibility = Visibility.Collapsed;
        }

        private void AddNewTransition(string stateName)
        {
            ModelItem stateMachineModelItem = StateContainerEditor.GetStateMachineModelItem(this.parentStateModelItem);
            ModelItem toStateModelItem = null;

            foreach (ModelItem stateModelItem in stateMachineModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection)
            {
                if (string.Equals(stateName, stateModelItem.Properties[StateDesigner.DisplayNamePropertyName].ComputedValue as string, StringComparison.Ordinal))
                {
                    toStateModelItem = stateModelItem;
                }
            }

            if (null == toStateModelItem)
            {
                return;
            }

            Fx.Assert(toStateModelItem != null, "To state cannot be null.");

            using (EditingScope editingScope = (EditingScope)this.ModelItem.BeginEdit(SR.CreateTransition))
            {
                ModelItem triggerModelItem = this.ModelItem.Properties[TriggerPropertyName].Value;
                State toState = toStateModelItem.GetCurrentValue() as State;

                ModelItem newTransitionItem = this.parentStateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection.Add(new Transition()
                {
                    Trigger = null == triggerModelItem ? null : triggerModelItem.GetCurrentValue() as Activity,
                    DisplayName = StateContainerEditor.GenerateTransitionName(stateMachineModelItem),
                    To = toState
                });

                this.ViewStateService.StoreViewState(newTransitionItem, ExpandViewStateKey, true);

                if (null == triggerModelItem)
                {
                    PointCollection thisPointCollection = this.ViewStateService.RetrieveViewState(this.ModelItem, StateContainerEditor.ConnectorLocationViewStateKey) as PointCollection;
                    if (null != thisPointCollection && thisPointCollection.Any())
                    {
                        PointCollection newTransitionViewState = new PointCollection
                            {
                                thisPointCollection[0] // start point
                            };

                        if (toState == this.parentStateModelItem.GetCurrentValue())
                        {
                            // add an invalid destination point for self-transition, to force a reroute of the connection point
                            newTransitionViewState.Add(new Point(0, 0));
                        }

                        this.ViewStateService.StoreViewState(newTransitionItem, StateContainerEditor.ConnectorLocationViewStateKey, newTransitionViewState);
                    }
                }

                editingScope.Complete();
            }

            this.UpdateTransitionsSharingTrigger();
        }

        private IEnumerable<ModelItem> GetAvailableStates()
        {
            List<ModelItem> availableStates = new List<ModelItem>();
            ModelItem stateMachineModelItem = StateContainerEditor.GetStateMachineModelItem(this.parentStateModelItem);
            Fx.Assert(null != stateMachineModelItem, "StateMachine must be the ancestor.");
            Dictionary<ModelItem, int> stateToConnectionMap = new Dictionary<ModelItem, int>();

            foreach (ModelItem stateModelItem in stateMachineModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection)
            {

                if (!stateToConnectionMap.ContainsKey(stateModelItem))
                {
                    stateToConnectionMap[stateModelItem] = 0;
                }

                foreach (ModelItem transitionModelItem in stateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection)
                {
                    // to simplify the model, count a source connection as one, regardless of whether it is shared Trigger or not.
                    stateToConnectionMap[stateModelItem]++;
                    
                    ModelItem toStateModelItem = transitionModelItem.Properties[TransitionDesigner.ToPropertyName].Value;
                    Fx.Assert(toStateModelItem != null, "To state of a transition cannot be null.");

                    if (stateToConnectionMap.ContainsKey(toStateModelItem))
                    {
                        stateToConnectionMap[toStateModelItem]++;
                    }
                    else
                    {
                        stateToConnectionMap[toStateModelItem] = 1;
                    }
                }
            }

            foreach (ModelItem stateModelItem in stateToConnectionMap.Keys)
            {
                if (stateToConnectionMap[stateModelItem] < TotalFreeConnectionPointNum)
                {
                    // only allow connection to state that have available connection points
                    availableStates.Add(stateModelItem);
                }
            }

            return availableStates.OrderBy(modelItem => modelItem.Properties[StateDesigner.DisplayNamePropertyName].Value == null ? 
                SR.EmptyName : 
                modelItem.Properties[StateDesigner.DisplayNamePropertyName].Value.GetCurrentValue());
        }

        private WorkflowViewElement FindTopmostWorkflowViewelementByHitTest(Visual visualToHitTest, Point point)
        {
            HitTestResult result = VisualTreeHelper.HitTest(visualToHitTest, point);
            if (result == null)
            {
                return null;
            }

            for (DependencyObject obj = result.VisualHit;
                obj != null;
                obj = VisualTreeHelper.GetParent(obj))
            {
                if (obj is WorkflowViewElement)
                {
                    return (WorkflowViewElement)obj;
                }
            }

            return null;
        }
    }
}
