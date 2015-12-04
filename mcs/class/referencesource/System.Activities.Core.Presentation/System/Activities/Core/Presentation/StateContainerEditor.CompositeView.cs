//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Xaml;

    partial class StateContainerEditor : IMultipleDragEnabledCompositeView
    {
        public static readonly DependencyProperty DroppingTypeResolvingOptionsProperty =
            DependencyProperty.Register("DroppingTypeResolvingOptions", typeof(TypeResolvingOptions), typeof(StateContainerEditor));

        public TypeResolvingOptions DroppingTypeResolvingOptions
        {
            get { return (TypeResolvingOptions)GetValue(DroppingTypeResolvingOptionsProperty); }
            set { SetValue(DroppingTypeResolvingOptionsProperty, value); }
        }

        public bool IsDefaultContainer
        {
            get { return true; }
        }        

        public void OnItemMoved(ModelItem modelItem)
        {
            Fx.Assert(this.modelItemToUIElement.ContainsKey(modelItem), "Moved item does not exist.");
            this.DoDeleteItems(new List<ModelItem> { modelItem }, false);
        }

        public object OnItemsCopied(List<ModelItem> itemsToCopy)
        {
            itemsToCopy.Remove(this.initialModelItem);

            // If the item copied is Transition, save its destination state guid to find the destination state when pasting.
            if (itemsToCopy.Count == 1)
            {
                ModelItem item = itemsToCopy.First();
                if (item != null && item.ItemType == typeof(Transition))
                {
                    ModelItem destinationState = item.Properties[TransitionDesigner.ToPropertyName].Value;

                    if (!modelItemToGuid.ContainsKey(destinationState))
                    {
                        modelItemToGuid.Add(destinationState, Guid.NewGuid().ToString());
                    }

                    CopiedTransitionDestinationState = destinationState;
                    return modelItemToGuid[destinationState];
                }
            }

            itemsToCopy.RemoveAll(item => item.ItemType == typeof(Transition));

            // Save the locations of copied items relative to the statemachine editor to the metadata.
            // The metadata will be used to translate the location view states of pasted items to the pasting target.
            PointCollection metaData = new PointCollection();
            foreach (ModelItem modelItem in itemsToCopy)
            {
                object viewState = this.ViewStateService.RetrieveViewState(modelItem, ShapeLocationViewStateKey);
                Point location = (Point)viewState;
                StateContainerEditor parentDesigner = VisualTreeUtils.FindVisualAncestor<StateContainerEditor>(GetStateView(modelItem));
                location = parentDesigner.panel.GetLocationRelativeToOutmostPanel(location);
                metaData.Add(location);
            }
            
            CopiedTransitionDestinationState = null;
            return metaData;
        }

        public object OnItemsCut(List<ModelItem> itemsToCut)
        {
            object metaData = OnItemsCopied(itemsToCut);
            this.OnItemsDelete(itemsToCut);
            return metaData;
        }

        public void OnItemsDelete(List<ModelItem> itemsToDelete)
        {
            DoDeleteItems(itemsToDelete, true);
        }

        void DoDeleteItems(List<ModelItem> itemsToDelete, bool removeIncomingConnectors)
        {
            itemsToDelete.Remove(this.initialModelItem);

            if (itemsToDelete.Count == 1 && itemsToDelete.First().ItemType == typeof(Transition))
            {
                this.DeleteConnectorModelItem(this.selectedConnector);
                return;
            }

            itemsToDelete.RemoveAll(item => item.ItemType == typeof(Transition));

            HashSet<Connector> connectorsToDelete = new HashSet<Connector>();
            List<ModelItem> allStateModelItemsToDelete = new List<ModelItem>();
            IEnumerable<ModelItem> selectedStateModelItems = this.Context.Items.GetValue<Selection>().SelectedObjects
                .Where<ModelItem>((p) => { return p.ItemType == typeof(State); });

            foreach (ModelItem stateModelItem in itemsToDelete)
            {
                allStateModelItemsToDelete.Add(stateModelItem);                
            }

            foreach (ModelItem modelItem in allStateModelItemsToDelete)
            {
                // We only need to delete incoming connectors to the states to be deleted; outgoing connectors will be deleted
                // automatically when the containing state is deleted.
                List<Connector> incomingConnectors = StateContainerEditor.GetIncomingConnectors(GetStateView(modelItem));
                foreach (Connector connector in incomingConnectors)
                {
                    ModelItem transitionModelItem = StateContainerEditor.GetConnectorModelItem(connector);
                    // If the transition is contained by the states to delete, we don't bother to delete it separately.
                    if (!StateContainerEditor.IsTransitionModelItemContainedByStateModelItems(transitionModelItem, selectedStateModelItems))
                    {
                        connectorsToDelete.Add(connector);
                    }
                }
            }

            // If we don't need to remove incoming connectors, we still remove the transitions but then add them back later.
            // This is in order to create an undo unit that contains the change notifications needed to make undo/redo work correctly.
            foreach (Connector connector in connectorsToDelete)
            {
                ModelItem connectorModelItem = StateContainerEditor.GetConnectorModelItem(connector);
                if (removeIncomingConnectors || connectorModelItem.ItemType == typeof(Transition))
                {
                    this.DeleteConnectorModelItem(connector);
                }
            }
            if (!removeIncomingConnectors)
            {
                foreach (Connector connector in connectorsToDelete)
                {
                    ModelItem connectorModelItem = StateContainerEditor.GetConnectorModelItem(connector);
                    if (connectorModelItem.ItemType == typeof(Transition))
                    {
                        StateContainerEditor.GetParentStateModelItemForTransition(connectorModelItem).Properties[StateDesigner.TransitionsPropertyName].Collection.Add(connectorModelItem);
                    }
                }
            }

            if (null != itemsToDelete)
            {
                itemsToDelete.ForEach(p => this.DeleteState(p, removeIncomingConnectors));
            }
        }

        public bool CanPasteItems(List<object> itemsToPaste)
        {
            if (itemsToPaste != null && itemsToPaste.Count > 0)
            {
                if (itemsToPaste.Count == 1 && itemsToPaste.First() is Transition)
                {
                    string errorMessage;
                    IEnumerable<ModelItem> selectedStateModelItems = this.Context.Items.GetValue<Selection>().SelectedObjects;
                    return selectedStateModelItems.All(item => CanPasteTransition(item, out errorMessage));
                }
                else
                {
                    return itemsToPaste.All(p =>
                        {
                            Type type = (p is Type) ? (Type)p : p.GetType();
                            return (typeof(State) == type || typeof(FinalState) == type);
                        }
                    );
                }
            }

            return false;
        }

        private bool CanPasteTransition(ModelItem sourceStateItem, out string errorMessage)
        {
            Fx.Assert(sourceStateItem != null, "sourceStateItem cannot be null");

            if (sourceStateItem.ItemType != typeof(State))
            {
                errorMessage = SR.PasteTransitionOnNonStateItem;
                return false;
            }

            if (!this.modelItemToUIElement.ContainsKey(sourceStateItem))
            {
                errorMessage = SR.PasteTransitionWithoutDestinationState;
                return false;
            }

            State sourceState = (State)sourceStateItem.GetCurrentValue();

            if (sourceState.IsFinal)
            {
                errorMessage = SR.PasteTransitionOnFinalState;
                return false;
            }

            if (GetEmptyConnectionPoints(sourceStateItem.View as UIElement).Count < 1)
            {
                errorMessage = string.Format(CultureInfo.CurrentUICulture, SR.PasteTransitionWithoutAvailableConnectionPoints, sourceState.DisplayName);
                return false;
            }

            errorMessage = null;
            return true;
        }

        internal bool CanPasteTransition(ModelItem destinationStateItem, out string errorMessage, params ModelItem[] sourceStateItems)
        {
            bool isDestinationStateSelected = false;
            foreach (ModelItem sourceStateItem in sourceStateItems)
            {                
                if (!CanPasteTransition(sourceStateItem, out errorMessage))
                {
                    return false;
                }

                if (sourceStateItem == destinationStateItem)
                {
                    isDestinationStateSelected = true;
                }
            }

            int emptyConnectionPointsCountNeeded = isDestinationStateSelected ? sourceStateItems.Count() + 1 : sourceStateItems.Count();

            if (GetEmptyConnectionPoints(destinationStateItem.View as UIElement).Count < emptyConnectionPointsCountNeeded)
            {
                errorMessage = string.Format(CultureInfo.CurrentUICulture, SR.PasteTransitionWithoutAvailableConnectionPoints, destinationStateItem.Properties["DisplayName"].Value);
                return false;
            }

            errorMessage = null;
            return true;
        }

        private ModelItem FindState(string guid)
        {            
            foreach (ModelItem item in this.modelItemToUIElement.Keys)
            {
                string itemGuid;
                if (modelItemToGuid.TryGetValue(item, out itemGuid) && guid == itemGuid)
                {
                    return item;
                }
            }

            return null;
        }

        public void OnItemsPasted(List<object> itemsToPaste, List<object> metaData, Point pastePoint, WorkflowViewElement pastePointReference)
        {
            if (this.ModelItem.ItemType == typeof(State))
            {
                WorkflowViewElement view = VisualTreeUtils.FindVisualAncestor<WorkflowViewElement>(this);
                if (view != null)
                {
                    StateContainerEditor container = (StateContainerEditor)DragDropHelper.GetCompositeView(view);
                    container.OnItemsPasted(itemsToPaste, metaData, pastePoint, pastePointReference);
                }

                return;
            }

            if (itemsToPaste.Count == 1 && itemsToPaste.First() is Transition)
            {
                if (metaData == null || metaData.Count != 1 || !(metaData.First() is string))
                {
                    ShowMessageBox(SR.PasteTransitionWithoutDestinationState);
                    return;
                }

                ModelItem destinationState = FindState(metaData.First() as string);

                if (destinationState == null)
                {
                    ShowMessageBox(SR.PasteTransitionWithoutDestinationState);
                    return;
                }

                this.PopulateVirtualizingContainer(destinationState);

                ModelItem[] selectedItems = this.Context.Items.GetValue<Selection>().SelectedObjects.ToArray();
                string errorMessage;
                if (!CanPasteTransition(destinationState, out errorMessage, selectedItems))
                {
                    ShowMessageBox(errorMessage);
                    return;
                }

                Transition pastedTransition = itemsToPaste.First() as Transition;
                Fx.Assert(pastedTransition != null, "Copied Transition should not be null.");

                using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(System.Activities.Presentation.SR.PropertyChangeEditingScopeDescription))
                {
                    string displayName = pastedTransition.DisplayName;
                    bool isFirst = true;
                    foreach (ModelItem selectedItem in selectedItems)
                    {
                        if (!isFirst)
                        {
                            StringReader reader = new StringReader(XamlServices.Save(pastedTransition));
                            pastedTransition = (Transition)XamlServices.Load(reader);
                        }

                        ModelItem transitionModelItem = this.Context.Services.GetRequiredService<ModelTreeManager>().WrapAsModelItem(pastedTransition);
                        ModelItem sourceState = selectedItem;
                        sourceState.Properties[StateDesigner.TransitionsPropertyName].Collection.Add(transitionModelItem);
                        transitionModelItem.Properties[TransitionDesigner.ToPropertyName].SetValue(destinationState);

                        if (isFirst)
                        {
                            this.ViewStateService.RemoveViewState(transitionModelItem, ConnectorLocationViewStateKey);
                            this.ViewStateService.RemoveViewState(transitionModelItem, SrcConnectionPointIndexStateKey);
                            this.ViewStateService.RemoveViewState(transitionModelItem, DestConnectionPointIndexStateKey);
                            isFirst = false;
                        }
                    }

                    es.Complete();
                }
            }
            else
            {
                List<ModelItem> modelItemsPasted = new List<ModelItem>();
                List<State> states = new List<State>();

                foreach (object obj in itemsToPaste)
                {
                    State state;
                    if (obj is FinalState)
                    {
                        state = new State() { DisplayName = DefaultFinalStateDisplayName, IsFinal = true };
                    }
                    else
                    {
                        state = (State)obj;
                        if (state.DisplayName == null)
                        {
                            state.DisplayName = DefaultStateDisplayName;
                        }
                    }
                    states.Add(state);
                }

                RemoveDanglingTransitions(states);

                using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(
                    System.Activities.Presentation.SR.CollectionAddEditingScopeDescription))
                {
                    // Fix 157591 by storing the height and width of the container "before" the new states are added to the
                    // panel, and group the insertion inside one editing scope - such that Undo will also restore the 
                    // size of the StateMachineContainer to pre-insert size.
                    StoreShapeSizeWithUndoRecursively(this.ModelItem);

                    foreach (State state in states)
                    {
                        ModelItem stateModelItem =
                            (this.ModelItem.ItemType == typeof(StateMachine)) ?
                            this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection.Add(state) :
                            GetStateMachineModelItem(this.ModelItem).Properties[StateMachineDesigner.StatesPropertyName].Collection.Add(state);
                        modelItemsPasted.Add(stateModelItem);
                    }

                    es.Complete();
                }

                if (modelItemsPasted.Count > 0)
                {
                    // translate location view states to be in the coordinate system of the pasting target
                    Fx.Assert(this.ModelItem.ItemType == typeof(StateMachine), "Only StateMachine contain the StateContainerEditor.");

                    this.UpdateLocationViewStatesByMetaData(modelItemsPasted, metaData, this);

                    if (pastePoint.X > 0 && pastePoint.Y > 0)
                    {
                        if (pastePointReference != null)
                        {
                            pastePoint = pastePointReference.TranslatePoint(pastePoint, this.panel);
                            pastePoint.X = pastePoint.X < 0 ? 0 : pastePoint.X;
                            pastePoint.Y = pastePoint.Y < 0 ? 0 : pastePoint.Y;
                        }
                        this.UpdateLocationViewStatesByPoint(modelItemsPasted, pastePoint);
                    }
                    // If paste point is not available, paste the items to the top left corner.
                    else
                    {
                        this.UpdateLocationViewStatesToAvoidOverlap(modelItemsPasted);
                    }
                }

                this.Dispatcher.BeginInvoke(() =>
                {
                    if (modelItemsPasted.Count > 0 && modelItemsPasted[0] != null)
                    {
                        Keyboard.Focus(modelItemsPasted[0].View as IInputElement);
                    }
                    this.Context.Items.SetValue(new Selection(modelItemsPasted));
                },
                DispatcherPriority.ApplicationIdle
                );
            }
        }


        public List<ModelItem> SortSelectedItems(List<ModelItem> selectedItems)
        {
            if (selectedItems == null)
            {
                throw FxTrace.Exception.ArgumentNull("selectedItems");
            }

            DragDropHelper.ValidateItemsAreOnView(selectedItems, this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection);
            return selectedItems;
        }

        public void OnItemsMoved(List<ModelItem> movedItems)
        {
            if (movedItems == null)
            {
                throw FxTrace.Exception.ArgumentNull("movedItems");
            }

            DragDropHelper.ValidateItemsAreOnView(movedItems, this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection);
            this.DoDeleteItems(movedItems, false);
        }

        void UpdateLocationViewStatesByPoint(List<ModelItem> itemsPasted, Point point)
        {
            Point topLeft = new Point(Double.PositiveInfinity, Double.PositiveInfinity);
            foreach (ModelItem stateModelItem in itemsPasted)
            {
                object viewState = this.ViewStateService.RetrieveViewState(stateModelItem, ShapeLocationViewStateKey);
                if (viewState != null)
                {
                    Point location = (Point)viewState;
                    topLeft.X = topLeft.X > location.X ? location.X : topLeft.X;
                    topLeft.Y = topLeft.Y > location.Y ? location.Y : topLeft.Y;
                }
            }
            OffsetLocationViewStates(new Vector(point.X - topLeft.X, point.Y - topLeft.Y), itemsPasted, GetTransitionModelItems(itemsPasted), false);
        }

        void UpdateLocationViewStatesByMetaData(List<ModelItem> itemsPasted, List<object> metaData, StateContainerEditor container)
        {
            Fx.Assert(container != null, "The view states must be calculated related to a parent StateContainerEditor.");
            // If the states are not copied from state machine view (e.g., when the State designer is the breadcrumb root), 
            // there is no meta data
            if (metaData != null && metaData.Count > 0)
            {
                int ii = 0;
                foreach (object data in metaData)
                {
                    PointCollection points = (PointCollection)data;
                    foreach (Point point in points)
                    {
                        // translate location view states to be in the coordinate system of the pasting target
                        this.ViewStateService.StoreViewState(itemsPasted[ii], ShapeLocationViewStateKey, container.panel.TranslatePoint(point, container.panel));
                        ++ii;
                    }
                }
                Fx.Assert(itemsPasted.Count == ii, "itemsCopied does not match the metaData.");
            }
        }

        void OffsetLocationViewStates(Vector offsetVector, IEnumerable<ModelItem> stateModelItems, IEnumerable<ModelItem> transitionModelItems, bool enableUndo)
        {
            // Offset view state for states
            if (stateModelItems != null)
            {
                foreach (ModelItem modelItem in stateModelItems)
                {
                    object viewState = this.ViewStateService.RetrieveViewState(modelItem, ShapeLocationViewStateKey);
                    if (viewState != null)
                    {
                        viewState = Point.Add((Point)viewState, offsetVector);
                        if (enableUndo)
                        {
                            this.ViewStateService.StoreViewStateWithUndo(modelItem, ShapeLocationViewStateKey, viewState);
                        }
                        else
                        {
                            this.ViewStateService.StoreViewState(modelItem, ShapeLocationViewStateKey, viewState);
                        }
                    }
                }
            }
            // Offset view state for transitions
            if (transitionModelItems != null)
            {
                foreach (ModelItem modelItem in transitionModelItems)
                {
                    object viewState = this.ViewStateService.RetrieveViewState(modelItem, ConnectorLocationViewStateKey);
                    if (viewState != null)
                    {
                        PointCollection locations = (PointCollection)viewState;
                        PointCollection newLocations = new PointCollection();
                        foreach (Point location in locations)
                        {
                            Point newLocation = Point.Add(location, offsetVector);
                            newLocation.X = newLocation.X < 0 ? 0 : newLocation.X;
                            newLocation.Y = newLocation.Y < 0 ? 0 : newLocation.Y;
                            newLocations.Add(newLocation);
                        }
                        if (enableUndo)
                        {
                            this.ViewStateService.StoreViewStateWithUndo(modelItem, ConnectorLocationViewStateKey, newLocations);
                        }
                        else
                        {
                            this.ViewStateService.StoreViewState(modelItem, ConnectorLocationViewStateKey, newLocations);
                        }
                    }
                }
            }
        }

        void UpdateLocationViewStatesToAvoidOverlap(List<ModelItem> itemsPasted)
        {
            int offset = 0;
            if (itemsPasted.Count > 0)
            {
                //Check to see if the first element in the input list needs offset. Generalize that information for all ModelItems in the input list.
                object location = this.ViewStateService.RetrieveViewState(itemsPasted[0], ShapeLocationViewStateKey);
                HashSet<Point> targetOccupiedLocations = null;

                if (this.ModelItem.ItemType == typeof(StateMachine))
                {
                    targetOccupiedLocations = this.shapeLocations;
                }
                else
                {
                    ModelItem stateMachineModelItem = StateContainerEditor.GetStateMachineModelItem(this.ModelItem);
                    StateMachineDesigner designer = stateMachineModelItem.View as StateMachineDesigner;

                    if (designer != null)
                    {
                        targetOccupiedLocations = designer.StateContainerEditor.shapeLocations;
                    }
                }

                if (location != null && targetOccupiedLocations != null)
                {
                    Point locationOfShape = (Point)location;

                    bool isOverlapped;

                    do
                    {
                        isOverlapped = false;
                        // need to check for each point on the canvas
                        foreach (var point in targetOccupiedLocations)
                        {
                            // When the pasting occurs, the pasted point may not be exactly the same
                            // as the copied point (with a slight margin of offset).  Therefore,
                            // we need to detect if the pasted point is within the boundary of the copied
                            // object.  If so, offset the pasted position such that the overlap is not observable.
                            if ((locationOfShape.X < point.X + FreeFormPanel.GridSize &&
                                 locationOfShape.X > point.X - FreeFormPanel.GridSize) &&
                                (locationOfShape.Y < point.Y + FreeFormPanel.GridSize &&
                                 locationOfShape.Y > point.Y - FreeFormPanel.GridSize))
                            {
                                offset++;
                                locationOfShape.Offset(FreeFormPanel.GridSize, FreeFormPanel.GridSize);
                                isOverlapped = true;
                                break;
                            }
                        }
                    } while (isOverlapped);
                }
            }
            //Update ViewState according to calculated offset.
            if (offset > 0)
            {
                double offsetValue = FreeFormPanel.GridSize * offset;
                OffsetLocationViewStates(new Vector(offsetValue, offsetValue), itemsPasted, GetTransitionModelItems(itemsPasted), false);
            }
        }
    }
}
