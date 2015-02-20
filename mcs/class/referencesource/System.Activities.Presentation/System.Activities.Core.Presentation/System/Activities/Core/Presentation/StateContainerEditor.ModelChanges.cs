//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Diagnostics;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;
    using System.Windows.Media;

    partial class StateContainerEditor
    {
        internal int DeleteConnectorModelItem(Connector connector, bool rerouting = false)
        {
            ModelItem connectorModelItem = StateContainerEditor.GetConnectorModelItem(connector);

            if (!rerouting)
            {
                if (connector is ConnectorWithStartDot)
                {
                    connector.StartDot.MouseDown -= new MouseButtonEventHandler(OnConnectorStartDotMouseDown);
                    connector.StartDot.MouseUp -= new MouseButtonEventHandler(OnConnectorStartDotMouseUp);
                }

                connector.GotKeyboardFocus -= new KeyboardFocusChangedEventHandler(OnConnectorGotKeyboardFocus);
                connector.RequestBringIntoView -= new RequestBringIntoViewEventHandler(OnConnectorRequestBringIntoView);
                connector.GotFocus -= new RoutedEventHandler(OnConnectorGotFocus);
                connector.MouseDoubleClick -= new MouseButtonEventHandler(OnConnectorMouseDoubleClick);
                connector.MouseDown -= new MouseButtonEventHandler(OnConnectorMouseDown);
                connector.KeyDown -= new KeyEventHandler(OnConnectorKeyDown);
                connector.ContextMenuOpening -= new ContextMenuEventHandler(OnConnectorContextMenuOpening);
                connector.Unloaded -= new RoutedEventHandler(OnConnectorUnloaded);
            }

            int removedIndex = InvalidIndex;
            if (connectorModelItem.ItemType == typeof(Transition))
            {
                ModelItemCollection transitions = StateContainerEditor.GetParentStateModelItemForTransition(connectorModelItem).Properties[StateDesigner.TransitionsPropertyName].Collection;
                removedIndex = transitions.IndexOf(connectorModelItem);
                Fx.Assert(removedIndex >= 0, "can't find the connector ModelItem in collection");
                transitions.Remove(connectorModelItem);
            }
            // Connector from initial node
            else if (connectorModelItem.ItemType == typeof(StateMachine))
            {
                using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.ClearInitialState))
                {
                    connectorModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].SetValue(null);
                    if (!rerouting)
                    {
                        this.ViewStateService.StoreViewStateWithUndo(connectorModelItem, ConnectorLocationViewStateKey, null);
                    }
                    es.Complete();
                }
            }
            return removedIndex;
        }

        void DeleteState(ModelItem stateModelItem, bool clearInitialState)
        {
            Fx.Assert(this.ModelItem.ItemType == typeof(StateMachine), "Should only delete states with StateMachine.");

            this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection.Remove(stateModelItem);
            if (clearInitialState &&
                this.ModelItem.ItemType == typeof(StateMachine) &&
                stateModelItem == this.ModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].Value)
            {
                this.ModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].SetValue(null);
                this.ViewStateService.StoreViewStateWithUndo(this.ModelItem, ConnectorLocationViewStateKey, null);
            }
        }

        // referenceTransitionModelItem is used when a connector is re-linked.
        void CreateTransition(ConnectionPoint sourceConnPoint, ConnectionPoint destConnPoint, ModelItem referenceTransitionModelItem, bool isSourceMoved)
        {
            VirtualizedContainerService.VirtualizingContainer srcDesigner = sourceConnPoint.ParentDesigner as VirtualizedContainerService.VirtualizingContainer;
            Fx.Assert(srcDesigner != null, "srcDesigner should not be null.");
            VirtualizedContainerService.VirtualizingContainer destDesigner = destConnPoint.ParentDesigner as VirtualizedContainerService.VirtualizingContainer;
            Fx.Assert(destDesigner != null, "destDesigner should not be null.");

            ModelItem srcModelItem = srcDesigner.ModelItem;
            ModelItem destModelItem = destDesigner.ModelItem;
            ModelItem transitionModelItem = null;

            // We are moving the connector.
            if (referenceTransitionModelItem != null && referenceTransitionModelItem.ItemType == typeof(Transition))
            {
                transitionModelItem = referenceTransitionModelItem;
                // We are moving the start of the connector. We only preserve the trigger if it is not shared.
                if (isSourceMoved)
                {
                    Transition referenceTransition = referenceTransitionModelItem.GetCurrentValue() as Transition;
                    ModelItem stateModelItem = GetParentStateModelItemForTransition(referenceTransitionModelItem);
                    State state = stateModelItem.GetCurrentValue() as State;
                    bool isTriggerShared = false;
                    foreach (Transition transition in state.Transitions)
                    {
                        if (transition != referenceTransition && transition.Trigger == referenceTransition.Trigger)
                        {
                            isTriggerShared = true;
                            break;
                        }
                    }
                    if (isTriggerShared)
                    {
                        transitionModelItem.Properties[TransitionDesigner.TriggerPropertyName].SetValue(null);
                    }
                }
                transitionModelItem.Properties[TransitionDesigner.ToPropertyName].SetValue(destModelItem);
                srcModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection.Add(transitionModelItem);
            }
            // We are creating a new connector. 
            else
            {
                ModelItem stateMachineModelItem = GetStateMachineModelItem(srcModelItem);
                Transition newTransition = new Transition() { DisplayName = StateContainerEditor.GenerateTransitionName(stateMachineModelItem) };
                newTransition.To = destModelItem.GetCurrentValue() as State;
                // Assign the shared trigger.
                if (sourceConnPoint.AttachedConnectors.Count > 0)
                {
                    Connector connector = sourceConnPoint.AttachedConnectors[0];
                    Transition existingTransition = StateContainerEditor.GetConnectorModelItem(connector).GetCurrentValue() as Transition;
                    newTransition.Trigger = existingTransition.Trigger;
                }
                transitionModelItem = srcModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection.Add(newTransition);
            }
            if (transitionModelItem != null)
            {
                // if the transition connection is re-routed, the SrcConnPointIndex needs to be updated.
                PointCollection connectorViewState = new PointCollection(ConnectorRouter.Route(this.panel, sourceConnPoint, destConnPoint));
                int srcConnectionPointIndex = StateContainerEditor.GetConnectionPoints(sourceConnPoint.ParentDesigner).IndexOf(sourceConnPoint);
                int destConnectionPointIndex = StateContainerEditor.GetConnectionPoints(destConnPoint.ParentDesigner).IndexOf(destConnPoint);
                this.StoreConnectorLocationViewState(transitionModelItem, connectorViewState, true);
                this.ViewStateService.StoreViewStateWithUndo(transitionModelItem, SrcConnectionPointIndexStateKey, srcConnectionPointIndex);
                this.ViewStateService.StoreViewStateWithUndo(transitionModelItem, DestConnectionPointIndexStateKey, destConnectionPointIndex);
            }
        }

        // referenceConnector is used when we are re-linking the connector.
        internal ConnectorCreationResult CreateConnectorGesture(ConnectionPoint sourceConnectionPoint, ConnectionPoint destConnectionPoint, Connector referenceConnector, bool isConnectorStartMoved)
        {
            Fx.Assert(sourceConnectionPoint != null, "sourceConnectionPoint is null.");
            Fx.Assert(destConnectionPoint != null, "destConnectionPoint is null.");
            ConnectorCreationResult result = ConnectorCreationResult.OtherFailure;
            if (destConnectionPoint.PointType != ConnectionPointKind.Outgoing && sourceConnectionPoint.PointType != ConnectionPointKind.Incoming)
            {
                if (sourceConnectionPoint.ParentDesigner is VirtualizedContainerService.VirtualizingContainer)
                {
                    //bool sameDestination = false;
                    ModelItem refTransitionModelItem = null;
                    if (referenceConnector != null)
                    {
                        refTransitionModelItem = StateContainerEditor.GetConnectorModelItem(referenceConnector);
                    }

                    using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.CreateTransition))
                    {
                        if (refTransitionModelItem != null)
                        {
                            this.CreateTransition(sourceConnectionPoint, destConnectionPoint, refTransitionModelItem, isConnectorStartMoved);
                        }
                        else
                        {
                            this.CreateTransition(sourceConnectionPoint, destConnectionPoint, null, false);
                        }
                        result = ConnectorCreationResult.Success;
                        es.Complete();
                    }
                }
                else if (sourceConnectionPoint.ParentDesigner is StartSymbol)
                {
                    ModelItem stateModelItem = ((VirtualizedContainerService.VirtualizingContainer)destConnectionPoint.ParentDesigner).ModelItem;

                    if (IsFinalState(stateModelItem))
                    {
                        result = ConnectorCreationResult.CannotSetFinalStateAsInitialState;
                    }
                    else
                    {
                        using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.SetInitialState))
                        {
                            this.StateMachineModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].SetValue(stateModelItem);
                            PointCollection connectorViewState = new PointCollection(ConnectorRouter.Route(this.panel, sourceConnectionPoint, destConnectionPoint));
                            this.StoreConnectorLocationViewState(this.StateMachineModelItem, connectorViewState, true);
                            result = ConnectorCreationResult.Success;
                            es.Complete();
                        }
                    }
                }
            }
            return result;
        }

        internal ConnectorCreationResult CreateConnectorGesture(ConnectionPoint sourceConnectionPoint, UIElement dest, Connector referenceConnector, bool isConnectorStartMoved)
        {
            ConnectionPoint destConnectionPoint = null;
            if (this.activeConnectionPoint != null)
            {
                destConnectionPoint = this.activeConnectionPoint;
            }
            else
            {
                destConnectionPoint = GetClosestDestConnectionPoint(sourceConnectionPoint, dest);
            }
            
            if (destConnectionPoint != null)
            {
                return CreateConnectorGesture(sourceConnectionPoint, destConnectionPoint, referenceConnector, isConnectorStartMoved);
            }
            return ConnectorCreationResult.OtherFailure;
        }

        internal ConnectorCreationResult CreateConnectorGesture(UIElement source, ConnectionPoint destConnectionPoint, Connector referenceConnector, bool isConnectorStartMoved)
        {
            ConnectionPoint sourceConnectionPoint = null;
            if (this.activeConnectionPoint != null)
            {
                sourceConnectionPoint = this.activeConnectionPoint;
            }
            else
            {
                sourceConnectionPoint = GetClosestSrcConnectionPoint(source, destConnectionPoint);
            }

            if (sourceConnectionPoint != null)
            {
                return CreateConnectorGesture(sourceConnectionPoint, destConnectionPoint, referenceConnector, isConnectorStartMoved);
            }
            return ConnectorCreationResult.OtherFailure;
        }

        void StoreShapeLocationViewState(UIElement view, Point newLocation)
        {
            ModelItem storageModelItem = null;
            if (view is StartSymbol)
            {
                storageModelItem = this.ModelItem;
            }
            else if (view is VirtualizedContainerService.VirtualizingContainer)
            {
                storageModelItem = ((VirtualizedContainerService.VirtualizingContainer)view).ModelItem;
            }
            StoreShapeLocationViewState(storageModelItem, newLocation);
        }

        void StoreShapeLocationViewState(ModelItem storageModelItem, Point newLocation)
        {
            if (this.ViewStateService.RetrieveViewState(storageModelItem, ShapeLocationViewStateKey) != null)
            {
                this.ViewStateService.StoreViewStateWithUndo(storageModelItem, ShapeLocationViewStateKey, newLocation);
            }
            else
            {
                this.ViewStateService.StoreViewState(storageModelItem, ShapeLocationViewStateKey, newLocation);
            }
        }
        
        void StoreConnectorLocationViewState(ModelItem connectorModelItem, PointCollection viewState, bool isUndoableViewState)
        {
            if (isUndoableViewState)
            {
                this.ViewStateService.StoreViewStateWithUndo(connectorModelItem, ConnectorLocationViewStateKey, viewState);
            }
            else
            {
                this.ViewStateService.StoreViewState(connectorModelItem, ConnectorLocationViewStateKey, viewState);
            }
        }

        void StoreConnectorLocationViewState(Connector connector, bool isUndoableViewState)
        {
            //This method will be called whenever the FreeFormPanel raises a location changed event on a connector.
            //Such location changed events are a result of changes already committed in the UI. Hence we do not want to react to such view state changes.
            //Using internalViewStateChange flag for that purpose.
            this.internalViewStateChange = true;
            this.StoreConnectorLocationViewState(StateContainerEditor.GetConnectorModelItem(connector), connector.Points, isUndoableViewState);
            this.internalViewStateChange = false;
        }

        // While adding an new StateContainer inside an outer StateContainer, the outter StateContainer size might change.  
        // InsertState would recursively stores the outer containers before the insertion happens, and capture the size within a single EditingScope to facilitate Undo.
        ModelItem InsertState(Object droppedObject)
        {
            ModelItem droppedModelItem = null;
            Fx.Assert(this.ModelItem.ItemType == typeof(StateMachine), "Should only drop state with StateMachine.");
            using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(System.Activities.Presentation.SR.CollectionAddEditingScopeDescription))
            {
                StoreShapeSizeWithUndoRecursively(this.ModelItem);
                droppedModelItem = this.ModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection.Add(droppedObject);
                es.Complete();
            }
            return droppedModelItem;
        }

        // Recursively store the current StateContainerHeight and StateContainerWidth of the target StateContainer and its ancestors to the current editing scope, 
        // up to the StateMachine instance.
        internal void StoreShapeSizeWithUndoRecursively(ModelItem modelItem)
        {
            if (modelItem.ItemType == typeof(State))
            {
                ModelItem parent = GetStateMachineModelItem(modelItem);
                if (null != parent)
                {
                    // State can be dropped to a non-StateMachine container (Bug 220966)
                    // so a null check is needed.
                    StoreShapeSizeWithUndoRecursively(parent);
                }
            }

            if (modelItem.ItemType == typeof(State) || modelItem.ItemType == typeof(StateMachine))
            {
                this.ViewStateService.StoreViewStateWithUndo(
                    modelItem,
                    StateContainerWidthViewStateKey,
                    this.ViewStateService.RetrieveViewState(modelItem, StateContainerWidthViewStateKey));

                this.ViewStateService.StoreViewStateWithUndo(
                    modelItem,
                    StateContainerHeightViewStateKey,
                    this.ViewStateService.RetrieveViewState(modelItem, StateContainerHeightViewStateKey));
            }
        }
    }
}
