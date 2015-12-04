//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Statements;
    using System.Runtime;
    using System.Linq;

    partial class StateContainerEditor
    {
        void OnStateCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (ModelItem deleted in e.OldItems)
                    {
                        if (deleted != null)
                        {
                            ModelItemCollection transitions = deleted.Properties[StateDesigner.TransitionsPropertyName].Collection;
                            if (this.listenedTransitionCollections.Contains(transitions))
                            {
                                transitions.CollectionChanged -=
                                    new NotifyCollectionChangedEventHandler(this.OnTransitionCollectionChanged);
                                this.listenedTransitionCollections.Remove(transitions);
                            }

                            if (this.modelItemToUIElement.ContainsKey(deleted))
                            {
                                this.RemoveStateVisual(this.modelItemToUIElement[deleted]);
                            }
                        }
                    }
                }
            }

            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                if (e.NewItems != null)
                {
                    foreach (ModelItem added in e.NewItems)
                    {
                        if (added != null)
                        {
                            ModelItemCollection transitions = added.Properties[StateDesigner.TransitionsPropertyName].Collection;
                            if (!this.listenedTransitionCollections.Contains(transitions))
                            {
                                transitions.CollectionChanged +=
                                    new NotifyCollectionChangedEventHandler(this.OnTransitionCollectionChanged);
                                this.listenedTransitionCollections.Add(transitions);
                            }
                            this.AddStateVisuals(new List<ModelItem> { added });
                        }
                    }
                }
            }
        }

        void OnTransitionCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                if (e.OldItems != null)
                {
                    foreach (ModelItem deleted in e.OldItems)
                    {
                        if (deleted != null)
                        {
                            if (!this.transitionModelItemsRemoved.Contains(deleted))
                            {
                                this.transitionModelItemsRemoved.Add(deleted);
                            }
                        }
                    }
                }
            }

            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                // We have to postpone updating the visual until the editing scope completes because 
                // the connector view state is not available at this moment
                foreach (ModelItem item in e.NewItems)
                {
                    if (!this.transitionModelItemsAdded.Contains(item))
                    {
                        this.transitionModelItemsAdded.Add(item);
                    }
                }
            }
        }

        void OnModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == StateMachineDesigner.InitialStatePropertyName)
            {
                Fx.Assert(this.ModelItem.ItemType == typeof(StateMachine), "Only StateMachine should have initial state");
                this.initialStateChanged = true;
            }
        }

        static bool ShouldSuppressAddingConnectorsWhenAddingStateVisuals(EditingScope scope)
        {
            return scope.Changes.Any<Change>((p) =>
            {
                return p != null && p.GetType() == typeof(SuppressAddingConnectorWhenAddingStateVisual);
            });
        }

        static bool IsTransitionReordering(EditingScope scope)
        {
            return scope.Changes.Any<Change>((p) =>
            {
                return p != null && p.GetType() == typeof(TransitionReorderChange);
            });
        }

        // All the connectors are directly contained by the statemachine editor. This is because connectors can go across states.
        void OnEditingScopeCompleted(object sender, EditingScopeEventArgs e)
        {
            foreach (ModelItem item in e.EditingScope.ItemsRemoved)
            {
                modelItemToGuid.Remove(item);
            }

            if (ShouldSuppressAddingConnectorsWhenAddingStateVisuals(e.EditingScope))
            {
                this.suppressAddingConnectorsWhenAddingStateVisuals = true;
            }
            else
            {
                this.suppressAddingConnectorsWhenAddingStateVisuals = false;
            }

            foreach (Change change in e.EditingScope.Changes)
            {
                if (change is PropertyChange)
                {
                    PropertyChange propertyChange = change as PropertyChange;
                    if (propertyChange.Owner.ItemType == typeof(Transition)
                        && propertyChange.PropertyName == TransitionDesigner.ToPropertyName
                        && propertyChange.NewValue != propertyChange.OldValue
                        && !this.transitionModelItemsRemoved.Contains(propertyChange.Owner)
                        && !this.transitionModelItemsAdded.Contains(propertyChange.Owner)
                        && this.modelItemToUIElement.ContainsKey(propertyChange.NewValue))
                    {
                        if (propertyChange.OldValue != null)
                        {
                            Connector connector = this.GetConnectorInStateMachine(propertyChange.Owner);
                            if (connector != null)
                            {
                                this.Remove(connector);
                            }
                        }

                        if (propertyChange.NewValue != null)
                        {
                            this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                            {
                                if (this.populated)
                                {
                                    this.AddTransitionVisual(propertyChange.Owner);
                                }
                            }));
                        }
                    }
                }
            }

            if (!IsTransitionReordering(e.EditingScope))
            {
                if (this.transitionModelItemsAdded.Count > 0)
                {
                    // We need to wait until after the state visuals are updated
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                    {
                        if (this.populated)
                        {
                            foreach (ModelItem transition in this.transitionModelItemsAdded)
                            {
                                if (transition.Properties[TransitionDesigner.ToPropertyName].Value != null)
                                {
                                    this.AddTransitionVisual(transition);
                                }
                            }
                        }
                        this.transitionModelItemsAdded.Clear();
                    }));
                }

                if (this.transitionModelItemsRemoved.Count > 0)
                {
                    foreach (ModelItem transition in this.transitionModelItemsRemoved)
                    {
                        Connector connector = this.GetConnectorInStateMachine(transition);
                        if (connector != null)
                        {
                            this.Remove(connector);
                        }
                    }
                    this.transitionModelItemsRemoved.Clear();
                }
            }

            if (this.initialStateChanged)
            {
                Fx.Assert(this.ModelItem.ItemType == typeof(StateMachine), "Only StateMachine should have initial state");
                Fx.Assert(this.initialNode != null, "Initial node should not be null");

                // Remove the old link
                if (GetAttachedConnectors(this.initialNode).Count > 0)
                {
                    this.Remove(GetAttachedConnectors(this.initialNode)[0]);
                }
                // Add the new link if the new initial state is not null
                ModelItem initialStateModelItem = this.ModelItem.Properties[StateMachineDesigner.InitialStatePropertyName].Value;
                if (initialStateModelItem != null)
                {
                    // We need to wait until after the state visuals are updated
                    this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                    {
                        if (this.populated)
                        {
                            this.AddInitialNodeConnector(GetStateView(initialStateModelItem));
                        }
                    }));
                }
                this.initialStateChanged = false;
            }
        }

        void OnViewStateChanged(object sender, ViewStateChangedEventArgs e)
        {
            Fx.Assert(e.ParentModelItem != null, "ViewState should be associated with some modelItem");

            if (!this.internalViewStateChange)
            {
                if (e.ParentModelItem == this.ModelItem)
                {
                    if (string.Equals(e.Key, StateContainerWidthViewStateKey, StringComparison.Ordinal))
                    {
                        double defaultWidth = ((this.ModelItem.ItemType == typeof(State)) ? DefaultWidthForState : DefaultWidthForStateMachine);
                        object widthViewState = this.ViewStateService.RetrieveViewState(this.ModelItem, StateContainerWidthViewStateKey);
                        this.StateContainerWidth = (widthViewState != null) ? (double)widthViewState : defaultWidth;
                    }
                    else if (string.Equals(e.Key, StateContainerHeightViewStateKey, StringComparison.Ordinal))
                    {
                        double defaultHeight = ((this.ModelItem.ItemType == typeof(State)) ? DefaultHeightForState : DefaultHeightForStateMachine);
                        object heightViewState = this.ViewStateService.RetrieveViewState(this.ModelItem, StateContainerHeightViewStateKey);
                        this.StateContainerHeight = (heightViewState != null) ? (double)heightViewState : defaultHeight;
                    }
                }

                if ((e.ParentModelItem.ItemType == typeof(State) || (e.ParentModelItem.ItemType == typeof(StateMachine) && e.ParentModelItem == this.ModelItem)) &&
                    e.Key.Equals(ShapeLocationViewStateKey))
                {
                    ModelItem modelItem = e.ParentModelItem;
                    if (modelItem.ItemType == typeof(StateMachine))
                    {
                        modelItem = this.initialModelItem;
                    }
                    if (this.modelItemToUIElement.ContainsKey(modelItem))
                    {
                        if (e.NewValue != null)
                        {
                            FreeFormPanel.SetLocation(this.modelItemToUIElement[modelItem], (Point)e.NewValue);
                            this.panel.InvalidateMeasure();
                            if (e.OldValue != null)
                            {
                                this.shapeLocations.Remove((Point)e.OldValue);
                            }
                            this.shapeLocations.Add((Point)e.NewValue);
                            // To reroute the links
                            this.InvalidateMeasureForStateMachinePanel();
                        }
                    }
                }

                else if (e.ParentModelItem.ItemType == typeof(State) && e.Key.Equals(ShapeSizeViewStateKey))
                {
                    // To reroute the links
                    this.InvalidateMeasureForStateMachinePanel();
                }

                // Only the statemachine editor should respond to connector changes because all connectors are
                // only added to the outmost editor
                else if (e.Key.Equals(ConnectorLocationViewStateKey) && !this.GetStateMachineContainerEditor().internalViewStateChange)
                {
                    Connector changedConnector = this.GetConnectorInStateMachine(e.ParentModelItem);
                    if (changedConnector != null)
                    {
                        if (e.NewValue != null)
                        {
                            Fx.Assert(e.NewValue is PointCollection, "e.NewValue is not PointCollection");
                            changedConnector.Points = e.NewValue as PointCollection;
                            this.GetStateMachineContainerEditor().panel.RemoveConnectorEditor();
                            this.InvalidateMeasureForStateMachinePanel();
                            if (IsConnectorFromInitialNode(changedConnector))
                            {
                                this.initialStateChanged = true;
                            }
                        }
                    }
                }
            }
        }
    }
}
