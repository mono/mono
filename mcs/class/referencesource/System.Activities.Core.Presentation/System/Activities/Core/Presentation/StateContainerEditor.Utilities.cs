//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Data;
    using System.Windows.Documents;
    using System.Windows.Media;

    partial class StateContainerEditor
    {
        internal static ModelItem GetConnectorModelItem(DependencyObject obj)
        {
            return (ModelItem)obj.GetValue(StateContainerEditor.ConnectorModelItemProperty);
        }

        static void SetConnectorModelItem(DependencyObject obj, ModelItem modelItem)
        {
            obj.SetValue(StateContainerEditor.ConnectorModelItemProperty, modelItem);
        }

        internal static List<ConnectionPoint> GetConnectionPoints(DependencyObject obj)
        {
            if (obj is StartSymbol)
            {
                return (List<ConnectionPoint>)obj.GetValue(StateContainerEditor.ConnectionPointsProperty);
            }
            if (!(obj is VirtualizedContainerService.VirtualizingContainer))
            {
                obj = VisualTreeUtils.FindVisualAncestor<VirtualizedContainerService.VirtualizingContainer>(obj);
            }
            return (List<ConnectionPoint>)obj.GetValue(StateContainerEditor.ConnectionPointsProperty);
        }

        static void SetConnectionPoints(DependencyObject obj, List<ConnectionPoint> connectionPoints)
        {
            obj.SetValue(StateContainerEditor.ConnectionPointsProperty, connectionPoints);
        }

        static void SetConnectorSrcDestConnectionPoints(Connector connector, ConnectionPoint srcConnectionPoint, ConnectionPoint destConnectionPoint)
        {
            FreeFormPanel.SetSourceConnectionPoint(connector, srcConnectionPoint);
            FreeFormPanel.SetDestinationConnectionPoint(connector, destConnectionPoint);
            srcConnectionPoint.AttachedConnectors.Add(connector);
            destConnectionPoint.AttachedConnectors.Add(connector);
        }

        static void SetConnectorLabel(Connector connector, ModelItem connectorModelItem)
        {

            connector.SetBinding(Connector.LabelTextProperty,  new Binding()
            {
                Source = connectorModelItem,
                Path = new PropertyPath("DisplayName")
            });

            TextBlock toolTip = new TextBlock();
            toolTip.SetBinding(TextBlock.TextProperty, new Binding()
            {
                Source = connectorModelItem,
                Path = new PropertyPath("DisplayName"),
                StringFormat = TransitionNameToolTip + Environment.NewLine + SR.EditTransitionTooltip + Environment.NewLine + SR.CopyTransitionToolTip
            });

            connector.SetLabelToolTip(toolTip);
        }

        static void SetConnectorStartDotToolTip(FrameworkElement startDot, ModelItem connectorModelItem)
        {
            ModelItem triggerModelItem = connectorModelItem.Properties[TransitionDesigner.TriggerPropertyName].Value as ModelItem;
            string triggerName = null;
            if (triggerModelItem == null)
            {
                triggerName = "(null)";
            }
            else
            {
                ModelItem displayNameModelItem = triggerModelItem.Properties["DisplayName"].Value;
                if (displayNameModelItem != null)
                {
                    triggerName = displayNameModelItem.GetCurrentValue() as string;
                }
            }
            startDot.ToolTip = string.Format(CultureInfo.InvariantCulture, TriggerNameToolTip, triggerName) + Environment.NewLine + SR.SharedTriggerToolTip;
        }


        // Returns true if visual is on the visual tree for point p relative to the reference.
        static bool IsVisualHit(UIElement visual, UIElement reference, Point point)
        {
            bool visualIsHit = false;
            HitTestResult result = VisualTreeHelper.HitTest(reference, point);
            if (result != null)
            {
                DependencyObject obj = result.VisualHit;
                while (obj != null)
                {
                    if (visual.Equals(obj))
                    {
                        visualIsHit = true;
                        break;
                    }
                    obj = VisualTreeHelper.GetParent(obj);
                }
            }
            return visualIsHit;
        }

        //This snaps the center of the element to grid.
        //Wherever shapeAnchorPoint is valid, it is made co-incident with the drop location.
        static Point SnapVisualToGrid(UIElement element, Point location, Point shapeAnchorPoint, bool isAnchorPointValid)
        {
            Fx.Assert(element != null, "Input UIElement is null");
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            Point oldCenter = location;
            if (!isAnchorPointValid)
            {
                //shapeAnchorPoint is invalid in case where it does not make sense (eg. toolbox drop).
                location.X -= InitialNodeWidth / 2;
                location.Y -= InitialNodeHeight / 2;
            }
            else
            {
                location.X -= shapeAnchorPoint.X;
                location.Y -= shapeAnchorPoint.Y;
                oldCenter = new Point(location.X + element.DesiredSize.Width / 2, location.Y + element.DesiredSize.Height / 2);
            }

            Point newCenter = SnapPointToGrid(oldCenter);

            location.Offset(newCenter.X - oldCenter.X, newCenter.Y - oldCenter.Y);

            if (location.X < 0)
            {
                double correction = FreeFormPanel.GridSize - ((location.X * (-1)) % FreeFormPanel.GridSize);
                location.X = (correction == FreeFormPanel.GridSize) ? 0 : correction;
            }
            if (location.Y < 0)
            {
                double correction = FreeFormPanel.GridSize - ((location.Y * (-1)) % FreeFormPanel.GridSize);
                location.Y = (correction == FreeFormPanel.GridSize) ? 0 : correction;
            }
            return location;
        }

        static Point SnapPointToGrid(Point pt)
        {
            pt.X -= pt.X % FreeFormPanel.GridSize;
            pt.Y -= pt.Y % FreeFormPanel.GridSize;
            pt.X = pt.X < 0 ? 0 : pt.X;
            pt.Y = pt.Y < 0 ? 0 : pt.Y;
            return pt;
        }

        static IEnumerable<Adorner> RemoveAdorner(UIElement adornedElement, Type adornerType)
        {
            Fx.Assert(adornedElement != null, "Invalid argument");
            Fx.Assert(typeof(Adorner).IsAssignableFrom(adornerType), "Invalid argument");
            List<Adorner> adornersRemoved = new List<Adorner>();
            AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(adornedElement);
            if (adornerLayer != null)
            {
                Adorner[] adorners = adornerLayer.GetAdorners(adornedElement);
                if (adorners != null)
                {
                    foreach (Adorner adorner in adorners)
                    {
                        if (adornerType.IsAssignableFrom(adorner.GetType()))
                        {
                            adornerLayer.Remove(adorner);
                            adornersRemoved.Add(adorner);
                        }
                    }
                }
            }
            return adornersRemoved;
        }

        internal static List<Connector> GetAttachedConnectors(UIElement shape)
        {
            HashSet<Connector> attachedConnectors = new HashSet<Connector>();
            List<ConnectionPoint> allConnectionPoints = GetConnectionPoints(shape);
            if (allConnectionPoints != null)
            {
                foreach (ConnectionPoint connPoint in allConnectionPoints)
                {
                    if (connPoint != null)
                    {
                        foreach (Connector connector in connPoint.AttachedConnectors)
                        {
                            attachedConnectors.Add(connector);
                        }
                    }
                }
            }
            return attachedConnectors.ToList<Connector>();
        }

        static List<Connector> GetOutgoingConnectors(UIElement shape)
        {
            List<Connector> outgoingConnectors = new List<Connector>();
            List<ConnectionPoint> allConnectionPoints = GetConnectionPoints(shape);
            foreach (ConnectionPoint connPoint in allConnectionPoints)
            {
                if (connPoint != null)
                {
                    outgoingConnectors.AddRange(connPoint.AttachedConnectors.Where(p => FreeFormPanel.GetSourceConnectionPoint(p).Equals(connPoint)));
                }
            }
            return outgoingConnectors;
        }

        static List<Connector> GetIncomingConnectors(UIElement shape)
        {
            List<Connector> incomingConnectors = new List<Connector>();
            List<ConnectionPoint> allConnectionPoints = GetConnectionPoints(shape);
            foreach (ConnectionPoint connPoint in allConnectionPoints)
            {
                if (connPoint != null)
                {
                    incomingConnectors.AddRange(connPoint.AttachedConnectors.Where(p => FreeFormPanel.GetDestinationConnectionPoint(p).Equals(connPoint)));
                }
            }
            return incomingConnectors;
        }

        static ConnectionPoint ConnectionPointHitTest(UIElement element, Point hitPoint)
        {
            FreeFormPanel panel = VisualTreeUtils.FindVisualAncestor<FreeFormPanel>(element);
            List<ConnectionPoint> connectionPoints = StateContainerEditor.GetConnectionPoints(element);
            return FreeFormPanel.ConnectionPointHitTest(hitPoint, connectionPoints, panel);
        }

        static ConnectionPoint GetConnectionPoint(UIElement element, Point location)
        {
            List<ConnectionPoint> connectionPoints = StateContainerEditor.GetConnectionPoints(element);
            foreach (ConnectionPoint connectionPoint in connectionPoints)
            {
                if (DesignerGeometryHelper.ManhattanDistanceBetweenPoints(location, connectionPoint.Location) <= ConnectorRouter.EndPointTolerance)
                {
                    return connectionPoint;
                }
            }
            return null;
        }

        internal static ModelItem GetStateMachineModelItem(ModelItem modelItem)
        {
            ModelItem currentModelItem = modelItem;
            while (currentModelItem != null && currentModelItem.ItemType != typeof(StateMachine))
            {
                currentModelItem = currentModelItem.Parent;
            }
            return currentModelItem;
        }

        static bool AreInSameStateMachine(ModelItem modelItem1, ModelItem modelItem2)
        {
            return GetStateMachineModelItem(modelItem1) == GetStateMachineModelItem(modelItem2);
        }

        internal static ModelItem GetParentStateModelItemForTransition(ModelItem transitionModelItem)
        {
            ModelItem parent = transitionModelItem;
            while (parent != null && parent.ItemType != typeof(State))
            {
                parent = parent.Parent;
            }
            return parent;
        }

        internal static UIElement GetStateView(ModelItem stateModelItem)
        {
            ModelItem parent = GetStateMachineModelItem(stateModelItem);
            if (parent.View is StateMachineDesigner)
            {
                return ((StateMachineDesigner)parent.View).StateContainerEditor.modelItemToUIElement[stateModelItem];
            }
            return null;
        }

        static ModelItem GetModelItemFromView(UIElement element)
        {
            ModelItem modelItem = null;
            if (element is StartSymbol)
            {
                modelItem = ((StartSymbol)element).ModelItem;
            }
            else
            {
                modelItem = ((VirtualizedContainerService.VirtualizingContainer)element).ModelItem;
            }
            return modelItem;
        }

        static internal ConnectionPoint GetClosestConnectionPoint(ConnectionPoint srcConnPoint, List<ConnectionPoint> destConnPoints, out double minDist)
        {
            minDist = double.PositiveInfinity;
            double dist = 0;
            ConnectionPoint closestPoint = null;
            Point srcPoint = FreeFormPanel.GetLocationRelativeToOutmostPanel(srcConnPoint);
            foreach (ConnectionPoint destConnPoint in destConnPoints)
            {
                if (srcConnPoint != destConnPoint)
                {
                    dist = DesignerGeometryHelper.ManhattanDistanceBetweenPoints(srcPoint, FreeFormPanel.GetLocationRelativeToOutmostPanel(destConnPoint));
                    if (dist < minDist)
                    {
                        minDist = dist;
                        closestPoint = destConnPoint;
                    }
                }
            }

            return closestPoint;
        }

        static ConnectionPoint GetClosestConnectionPointNotOfType(ConnectionPoint srcConnectionPoint, List<ConnectionPoint> targetConnectionPoints, ConnectionPointKind illegalConnectionPointKind)
        {
            double minDist;
            List<ConnectionPoint> filteredConnectionPoints = new List<ConnectionPoint>();
            foreach (ConnectionPoint connPoint in targetConnectionPoints)
            {
                if (connPoint.PointType != illegalConnectionPointKind && !connPoint.Equals(srcConnectionPoint) && connPoint.AttachedConnectors.Count == 0)
                {
                    filteredConnectionPoints.Add(connPoint);
                }
            }
            return GetClosestConnectionPoint(srcConnectionPoint, filteredConnectionPoints, out minDist);
        }

        static void GetClosestConnectionPointPair(List<ConnectionPoint> srcConnPoints, List<ConnectionPoint> destConnPoints, out ConnectionPoint srcConnPoint, out ConnectionPoint destConnPoint)
        {
            double minDist = double.PositiveInfinity;
            double dist;
            ConnectionPoint tempConnPoint;
            srcConnPoint = null;
            destConnPoint = null;
            foreach (ConnectionPoint connPoint in srcConnPoints)
            {
                tempConnPoint = GetClosestConnectionPoint(connPoint, destConnPoints, out dist);
                if (dist < minDist)
                {
                    minDist = dist;
                    srcConnPoint = connPoint;
                    destConnPoint = tempConnPoint;
                }
            }
            Fx.Assert(srcConnPoint != null, "No ConnectionPoint found");
            Fx.Assert(destConnPoint != null, "No ConnectionPoint found");
        }

        static void GetEmptySrcDestConnectionPoints(UIElement source, UIElement dest, out ConnectionPoint srcConnPoint, out ConnectionPoint destConnPoint)
        {
            srcConnPoint = null;
            destConnPoint = null;
            List<ConnectionPoint> srcConnectionPoints = GetEmptyConnectionPoints(source);
            List<ConnectionPoint> destConnectionPoints = GetEmptyConnectionPoints(dest);
            if (srcConnectionPoints.Count > 0 && destConnectionPoints.Count > 0)
            {
                GetClosestConnectionPointPair(srcConnectionPoints, destConnectionPoints, out srcConnPoint, out destConnPoint);
            }
        }

        internal static List<ConnectionPoint> GetEmptyConnectionPoints(UIElement designer)
        {
            List<ConnectionPoint> connectionPoints = StateContainerEditor.GetConnectionPoints(designer);
            if (connectionPoints != null)
            {
                return new List<ConnectionPoint>(connectionPoints.Where<ConnectionPoint>(
                    (p) => { return p.AttachedConnectors == null || p.AttachedConnectors.Count == 0; }));
            }
            return new List<ConnectionPoint>();
        }

        //This returns the closest non-incoming connectionPoint on source. Return value will be different than destConnectionPoint.
        static ConnectionPoint GetClosestSrcConnectionPoint(UIElement src, ConnectionPoint destConnectionPoint)
        {
            ConnectionPoint srcConnectionPoint = null;
            if (destConnectionPoint.PointType != ConnectionPointKind.Outgoing)
            {
                srcConnectionPoint = GetClosestConnectionPointNotOfType(destConnectionPoint, StateContainerEditor.GetConnectionPoints(src), ConnectionPointKind.Incoming);
            }
            return srcConnectionPoint;
        }

        //This returns the closest non-outgoing connectionPoint on dest. Return value will be different than sourceConnectionPoint.
        static ConnectionPoint GetClosestDestConnectionPoint(ConnectionPoint sourceConnectionPoint, UIElement dest)
        {
            ConnectionPoint destConnectionPoint = null;
            if (sourceConnectionPoint.PointType != ConnectionPointKind.Incoming)
            {
                destConnectionPoint = GetClosestConnectionPointNotOfType(sourceConnectionPoint, StateContainerEditor.GetConnectionPoints(dest), ConnectionPointKind.Outgoing);
            }
            return destConnectionPoint;
        }

        static ConnectionPoint GetSrcConnectionPointForSharedTrigger(UIElement sourceDesigner, ModelItem connectorModelItem)
        {
            ConnectionPoint sourceConnectionPoint = null;
            List<Connector> connectors = StateContainerEditor.GetOutgoingConnectors(sourceDesigner);
            foreach (Connector connector in connectors)
            {
                ModelItem modelItem = StateContainerEditor.GetConnectorModelItem(connector);
                if (modelItem != null && modelItem.ItemType == typeof(Transition))
                {
                    if (modelItem.Properties[TransitionDesigner.TriggerPropertyName].Value == connectorModelItem.Properties[TransitionDesigner.TriggerPropertyName].Value)
                    {
                        sourceConnectionPoint = FreeFormPanel.GetSourceConnectionPoint(connector);
                    }
                }
            }
            return sourceConnectionPoint;
        }

        // Test if the transition is contained by any of the states or their descendants
        static bool IsTransitionModelItemContainedByStateModelItems(ModelItem transitionModelItem, IEnumerable<ModelItem> stateModelItems)
        {
            foreach (ModelItem stateModelItem in stateModelItems)
            {
                if (stateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection.Contains(transitionModelItem))
                {
                    return true;
                }
            }
            return false;
        }

        static bool IsTransitionDestinationWithinStates(Transition transition, IEnumerable<State> states)
        {
            foreach (State state in states)
            {
                if (transition.To == state)
                {
                    return true;
                }
            }
            return false;
        }

        // Remove dangling transitions that are not pointing to any of the input states or their descendants
        static void RemoveDanglingTransitions(IEnumerable<State> states)
        {
            Queue<State> statesToProcess = new Queue<State>(states);
            while (statesToProcess.Count > 0)
            {
                State state = statesToProcess.Dequeue();

                IEnumerable<Transition> toRemove = state.Transitions.Where<Transition>((p) =>
                    { return !IsTransitionDestinationWithinStates(p, states); }).Reverse();
                foreach (Transition transition in toRemove)
                {
                    state.Transitions.Remove(transition);
                }

            }
        }

        static List<ModelItem> GetTransitionModelItems(IEnumerable<ModelItem> stateModelItems)
        {
            List<ModelItem> transitionModelItems = new List<ModelItem>();
            foreach (ModelItem stateModelItem in stateModelItems)
            {
                transitionModelItems.AddRange(stateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection);
                //transitionModelItems.AddRange(GetTransitionModelItems(stateModelItem.Properties[ChildStatesPropertyName].Collection));
            }
            return transitionModelItems;
        }

        internal static bool IsFinalState(ModelItem modelItem)
        {
            return modelItem.ItemType == typeof(State) && (bool)modelItem.Properties[StateDesigner.IsFinalPropertyName].Value.GetCurrentValue();
        }

        static void ShowMessageBox(string message)
        {
            MessageBox.Show(message, SR.ErrorMessageBoxTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        static void ReportConnectorCreationError(ConnectorCreationResult result)
        {
            switch (result)
            {
                case ConnectorCreationResult.CannotCreateTransitionToCompositeState:
                    ShowMessageBox(SR.CannotCreateTransitionToCompositeState);
                    break;
                case ConnectorCreationResult.CannotCreateTransitionFromAncestorToDescendant:
                    ShowMessageBox(SR.CannotCreateTransitionFromAncestorToDescendant);
                    break;
                case ConnectorCreationResult.CannotSetCompositeStateAsInitialState:
                    ShowMessageBox(SR.CannotSetCompositeStateAsInitialState);
                    break;
                case ConnectorCreationResult.CannotSetFinalStateAsInitialState:
                    ShowMessageBox(SR.CannotSetFinalStateAsInitialState);
                    break;
                case ConnectorCreationResult.OtherFailure:
                    ShowMessageBox(SR.CannotCreateLink);
                    break;
            }
        }

        static bool IsConnectorFromInitialNode(Connector connector)
        {
            return GetConnectorModelItem(connector).ItemType == typeof(StateMachine);
        }

        internal static string GenerateTransitionName(ModelItem stateMachineModelItem)
        {
            Fx.Assert(stateMachineModelItem.ItemType == typeof(StateMachine), "ModelItem param should be a statemachine.");
            HashSet<String> existingTransitionNames = new HashSet<string>();

            foreach (ModelItem stateModelItem in stateMachineModelItem.Properties[StateMachineDesigner.StatesPropertyName].Collection)
            {
                foreach (ModelItem transitionModelItem in stateModelItem.Properties[StateDesigner.TransitionsPropertyName].Collection)
                {
                    existingTransitionNames.Add(((Transition)transitionModelItem.GetCurrentValue()).DisplayName);
                }
            }

            int suffix = 0;
            string name;

            do
            {
                name = string.Format(CultureInfo.InvariantCulture, "T{0}", ++suffix);
            } while (existingTransitionNames.Contains<string>(name));

            return name;
        }
    }
}
