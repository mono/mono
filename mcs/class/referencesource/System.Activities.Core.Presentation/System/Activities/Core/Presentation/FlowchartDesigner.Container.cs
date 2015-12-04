//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Windows.Threading;
    using System.Xaml;
    using System.Activities.Presentation.FreeFormEditing;

    partial class FlowchartDesigner : IMultipleDragEnabledCompositeView
    {
        public static readonly DependencyProperty DroppingTypeResolvingOptionsProperty =
            DependencyProperty.Register("DroppingTypeResolvingOptions", typeof(TypeResolvingOptions), typeof(FlowchartDesigner));

        [Fx.Tag.KnownXamlExternal]
        public TypeResolvingOptions DroppingTypeResolvingOptions
        {
            get { return (TypeResolvingOptions)GetValue(DroppingTypeResolvingOptionsProperty); }
            set { SetValue(DroppingTypeResolvingOptionsProperty, value); }
        }

        public void OnItemMoved(ModelItem modelItem)
        {
            Fx.Assert(this.modelElement.ContainsKey(modelItem), "Moved item does not exist.");
            this.OnItemsDelete(new List<ModelItem> { modelItem });
        }

        public object OnItemsCopied(List<ModelItem> itemsToCopy)
        {
            Fx.Assert(this.panel != null, "This code shouldn't be hit if panel is null");

            // Filter away flowStart
            itemsToCopy.Remove(flowStart);

            //Get the corresponding FlowElements and clone them.
            //We will work against actual objects here not the model items.
            Dictionary<FlowNode, FlowNode> clonedFlowElements = new Dictionary<FlowNode, FlowNode>();
            foreach (ModelItem modelItem in itemsToCopy)
            {
                ModelItem flowElementMI = GetFlowElementMI(modelItem);
                FlowNode flowElement = (FlowNode)flowElementMI.GetCurrentValue();
                clonedFlowElements[flowElement] = CloneFlowElement(flowElement);
            }

            //Traverse the FlowElements to reset Clone references to other FlowElements.
            foreach (FlowNode flowElement in clonedFlowElements.Keys)
            {
                UpdateCloneReferences(flowElement, clonedFlowElements);
            }

            //Return metadata.
            List<FlowNode> metaData = new List<FlowNode>(clonedFlowElements.Values);
            return (object)metaData;
        }

        public List<ModelItem> SortSelectedItems(List<ModelItem> selectedItems)
        {
            if (selectedItems == null)
            {
                throw FxTrace.Exception.ArgumentNull("selectedItems");
            }

            DragDropHelper.ValidateItemsAreOnView(selectedItems, this.modelElement.Keys);
            return selectedItems;
        }

        public void OnItemsMoved(List<ModelItem> movedItems)
        {
            if (movedItems == null)
            {
                throw FxTrace.Exception.ArgumentNull("movedItems");
            }

            DragDropHelper.ValidateItemsAreOnView(movedItems, this.modelElement.Keys);

            HashSet<ModelItem> updatedItems = new HashSet<ModelItem>();
            foreach (ModelItem modelItem in movedItems)
            {
                ModelItem flowModelItem = this.GetFlowElementMI(modelItem);
                updatedItems.Add(flowModelItem);
            }
            this.OnItemsDelete(movedItems, updatedItems);
        }

        public object OnItemsCut(List<ModelItem> itemsToCut)
        {
            Fx.Assert(this.panel != null, "This code shouldn't be hit if panel is null");
            object metaData = OnItemsCopied(itemsToCut);

            //Delete ModelItems.
            this.OnItemsDelete(itemsToCut);

            return metaData;
        }

        public void OnItemsDelete(List<ModelItem> itemsToDelete)
        {
            this.OnItemsDelete(itemsToDelete, null);
        }

        public void OnItemsDelete(List<ModelItem> itemsToDelete, HashSet<ModelItem> updatedItems)
        {
            // Filter away flowStart
            itemsToDelete.Remove(flowStart);

            HashSet<Connector> connectorsToDelete = GetRelatedConnectors(itemsToDelete);

            foreach (Connector connector in connectorsToDelete)
            {
                DeleteLink(connector, false, updatedItems);
            }

            if (null != itemsToDelete)
            {
                itemsToDelete.ForEach(p => this.DeleteShape(p, updatedItems));
            }
        }

        //This method updates the clone of currentFlowElement to reference cloned FlowElements.
        void UpdateCloneReferences(FlowNode currentFlowElement, Dictionary<FlowNode, FlowNode> clonedFlowElements)
        {
            if (typeof(FlowStep).IsAssignableFrom(currentFlowElement.GetType()))
            {
                FlowStep currentFlowStep = (FlowStep)currentFlowElement;
                FlowStep clonedFlowStep = (FlowStep)clonedFlowElements[currentFlowElement];
                FlowNode nextFlowElement = currentFlowStep.Next;
                if (nextFlowElement != null && clonedFlowElements.ContainsKey(nextFlowElement))
                {
                    clonedFlowStep.Next = clonedFlowElements[nextFlowElement];
                }
                else
                {
                    clonedFlowStep.Next = null;
                }
            }
            else if (typeof(FlowDecision).IsAssignableFrom(currentFlowElement.GetType()))
            {
                FlowDecision currentFlowDecision = (FlowDecision)currentFlowElement;
                FlowDecision clonedFlowDecision = (FlowDecision)clonedFlowElements[currentFlowElement];
                FlowNode trueElement = currentFlowDecision.True;
                FlowNode falseElement = currentFlowDecision.False;

                if (trueElement != null && clonedFlowElements.ContainsKey(trueElement))
                {
                    clonedFlowDecision.True = clonedFlowElements[trueElement];
                }
                else
                {
                    clonedFlowDecision.True = null;
                }

                if (falseElement != null && clonedFlowElements.ContainsKey(falseElement))
                {
                    clonedFlowDecision.False = clonedFlowElements[falseElement];
                }
                else
                {
                    clonedFlowDecision.False = null;
                }

            }
            else if (GenericFlowSwitchHelper.IsGenericFlowSwitch(currentFlowElement.GetType()))
            {
                GenericFlowSwitchHelper.Copy(currentFlowElement.GetType().GetGenericArguments()[0], currentFlowElement, clonedFlowElements);
            }
            else
            {
                Debug.Fail("Unknown FlowNode");
            }
        }

        // The logic is similar to UpdateCloneReferences.
        // the difference is in this function, we need to set reference by Property.
        void UpdateCloneReferenceByModelItem (FlowNode currentFlowElement,
            Dictionary<FlowNode, ModelItem> modelItems, Dictionary<FlowNode, FlowNode> clonedFlowElements)
        {
            if (typeof(FlowStep).IsAssignableFrom(currentFlowElement.GetType()))
            {
                FlowStep currentFlowStep = (FlowStep)currentFlowElement;
                FlowStep clonedFlowStep = (FlowStep)clonedFlowElements[currentFlowElement];
                ModelItem modelItem = modelItems[clonedFlowStep];
                FlowNode nextFlowElement = currentFlowStep.Next;
                if (nextFlowElement != null && clonedFlowElements.ContainsKey(nextFlowElement))
                {
                    modelItem.Properties["Next"].SetValue(clonedFlowElements[nextFlowElement]);
                }
                else
                {
                    modelItem.Properties["Next"].SetValue(null);
                }
            }
            else if (typeof(FlowDecision).IsAssignableFrom(currentFlowElement.GetType()))
            {
                if (!modelItems.ContainsKey(currentFlowElement))
                {
                    Fx.Assert("Should not happen.");
                }
                FlowDecision currentFlowDecision = (FlowDecision)currentFlowElement;
                FlowDecision clonedFlowDecision = (FlowDecision)clonedFlowElements[currentFlowElement];
                Fx.Assert(currentFlowDecision == clonedFlowDecision, "should not happen");
                ModelItem modelItem = modelItems[currentFlowElement];
                Fx.Assert(modelItem != null, "should not happen");
                FlowNode trueElement = currentFlowDecision.True;
                FlowNode falseElement = currentFlowDecision.False;

                if (trueElement != null && clonedFlowElements.ContainsKey(trueElement))
                {
                    modelItem.Properties["True"].SetValue(clonedFlowElements[trueElement]);
                }
                else
                {
                    modelItem.Properties["True"].SetValue(null);
                }

                if (falseElement != null && clonedFlowElements.ContainsKey(falseElement))
                {
                    modelItem.Properties["False"].SetValue(clonedFlowElements[falseElement]);
                }
                else
                {
                    modelItem.Properties["False"].SetValue(null);
                }

            }
            else if (GenericFlowSwitchHelper.IsGenericFlowSwitch(currentFlowElement.GetType()))
            {
                GenericFlowSwitchHelper.ReferenceCopy(currentFlowElement.GetType().GetGenericArguments()[0],
                    currentFlowElement,
                    modelItems,
                    clonedFlowElements);
            }
            else
            {
                Debug.Fail("Unknown FlowNode");
            }
        }

        public bool CanPasteItems(List<object> itemsToPaste)
        {
            if (this.ShowExpanded)
            {
                if (itemsToPaste != null)
                {
                    return itemsToPaste.All(p =>
                        typeof(Activity).IsAssignableFrom(p.GetType()) ||
                        typeof(FlowNode).IsAssignableFrom(p.GetType()) ||
                        (p is Type && typeof(Activity).IsAssignableFrom((Type)p)) ||
                        (p is Type && typeof(FlowNode).IsAssignableFrom((Type)p)));
                }
            }
            return false;
        }

        public void OnItemsPasted(List<object> itemsToPaste, List<object> metaData, Point pastePoint, WorkflowViewElement pastePointReference)
        {
            Fx.Assert(this.panel != null, "This code shouldn't be hit if panel is null");
            HashSet<Activity> workflowElementsPasted = new HashSet<Activity>();
            List<ModelItem> modelItemsToSelect = new List<ModelItem>();
            bool shouldStoreCurrentSizeViewState = true;

            Fx.Assert(this.ModelItem is IModelTreeItem, "this.ModelItem must implement IModelTreeItem");
            using (EditingScope editingScope = ((IModelTreeItem)this.ModelItem).ModelTreeManager.CreateEditingScope(System.Activities.Presentation.SR.CollectionAddEditingScopeDescription))
            {
                if (metaData != null)
                {
                    List<ModelItem> modelItemsPerMetaData = new List<ModelItem>();
                    foreach (object designerMetaData in metaData)
                    {
                        if (designerMetaData is List<FlowNode>)
                        {
                            //This is flowchart metadata.
                            foreach (FlowNode element in designerMetaData as List<FlowNode>)
                            {
                                FlowStep step = element as FlowStep;
                                if (step != null)
                                {
                                    workflowElementsPasted.Add(step.Action);
                                }

                                if (shouldStoreCurrentSizeViewState)
                                {
                                    // Pasting may change the size of flowchart; need this to undo the size change.
                                    this.StoreCurrentSizeViewStateWithUndo();
                                    shouldStoreCurrentSizeViewState = false;
                                }

                                ModelItem item = this.ModelItem.Properties["Nodes"].Collection.Add(element);

                                // if the pasted item is a flowswitch but the default target is not in the pasted selection,
                                // reset the DefaultCaseDisplayName to "Default".
                                if (GenericFlowSwitchHelper.IsGenericFlowSwitch(item.ItemType) &&
                                    item.Properties["Default"].Value == null)
                                {
                                    item.Properties[FlowSwitchLabelFeature.DefaultCaseDisplayNamePropertyName].SetValue(FlowSwitchLabelFeature.DefaultCaseDisplayNameDefaultValue);
                                }

                                modelItemsPerMetaData.Add(item);
                                if (item != null)
                                {
                                    if (item.ItemType.Equals(typeof(FlowStep)))
                                    {
                                        modelItemsToSelect.Add(item.Properties["Action"].Value);
                                    }
                                    else
                                    {
                                        modelItemsToSelect.Add(item);
                                    }
                                }
                            }
                            if (pastePoint.X > 0 && pastePoint.Y > 0)
                            {
                                Point panelPoint = this.TranslatePoint(pastePoint, this.panel);
                                if (pastePointReference != null && !pastePointReference.Equals(this))
                                {
                                    if (pastePointReference.ModelItem != null && this.modelElement.ContainsKey(pastePointReference.ModelItem))
                                    {
                                        panelPoint = pastePointReference.TranslatePoint(pastePoint, this.panel);
                                    }
                                }
                                panelPoint.X = panelPoint.X < 0 ? 0 : panelPoint.X;
                                panelPoint.Y = panelPoint.Y < 0 ? 0 : panelPoint.Y;
                                UpdateViewStateOnPastePoint(modelItemsPerMetaData, panelPoint);
                            }
                            else
                            {
                                UpdateViewStateToAvoidOverlapOnPaste(modelItemsPerMetaData);
                            }
                            modelItemsPerMetaData.Clear();
                        }
                    }
                }

                foreach (object itemToPaste in itemsToPaste)
                {
                    Activity workflowElementToPaste = itemToPaste as Activity;
                    if (workflowElementToPaste != null && !workflowElementsPasted.Contains(workflowElementToPaste))
                    {
                        FlowStep flowStep = new FlowStep { Action = workflowElementToPaste, Next = null };
                        if (shouldStoreCurrentSizeViewState)
                        {
                            // Pasting may change the size of flowchart; need this to undo the size change.
                            this.StoreCurrentSizeViewStateWithUndo();
                            shouldStoreCurrentSizeViewState = false;
                        }
                        
                        // When paste a non-flowstep object to flowchart, the existing hintsize of the object 
                        // should be removed, and let flowchart panel to compute the right size.
                        VirtualizedContainerService.SetHintSize(workflowElementToPaste, null);
                        ModelItem flowStepItem = this.ModelItem.Properties["Nodes"].Collection.Add(flowStep);
                     
                        if (flowStepItem != null)
                        {
                            modelItemsToSelect.Add(flowStepItem.Properties["Action"].Value);
                        }
                    }
                }

                editingScope.Complete();
            }

            this.Dispatcher.BeginInvoke(() =>
            {
                if (modelItemsToSelect.Count > 0 && modelItemsToSelect[0] != null)
                {
                    Keyboard.Focus(modelItemsToSelect[0].View as IInputElement);
                }
                this.Context.Items.SetValue(new Selection(modelItemsToSelect));
            },
            DispatcherPriority.ApplicationIdle
            );
        }

        void UpdateViewStateToAvoidOverlapOnPaste(List<ModelItem> modelItemsPerMetaData)
        {
            //Determine Offset.
            int offSetInMultipleOfGridSize = 0;
            if (modelItemsPerMetaData.Count > 0)
            {
                //Check to see if the first element in the input list needs offset. Generalize that information for all ModelItems in the input list.
                //Get location information of the first element
                object location = this.ViewStateService.RetrieveViewState(modelItemsPerMetaData[0], shapeLocation);
                if (location != null)
                {
                    Point locationOfShape = (Point)location;

                    foreach (var point in this.shapeLocations)
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
                            offSetInMultipleOfGridSize++;
                            locationOfShape.Offset(FreeFormPanel.GridSize, FreeFormPanel.GridSize);
                        }
                    }
                }
            }
            //Update viewstate according to calculated offset.
            if (offSetInMultipleOfGridSize > 0)
            {
                double offsetValue = FreeFormPanel.GridSize * offSetInMultipleOfGridSize;
                OffSetViewState(new Vector(offsetValue, offsetValue), modelItemsPerMetaData);
            }

        }

        void UpdateViewStateOnPastePoint(List<ModelItem> modelItemsInMetaData, Point newOrigin)
        {
            //Determine top left of pasted objects.
            Point topLeft = new Point(Double.PositiveInfinity, Double.PositiveInfinity);
            foreach (ModelItem modelItem in modelItemsInMetaData)
            {
                Dictionary<string, object> viewState = this.ViewStateService.RetrieveAllViewState(modelItem);

                foreach (object viewStateValue in viewState.Values)
                {
                    PointCollection viewStatePoints = viewStateValue as PointCollection;
                    if (viewStatePoints == null && viewStateValue is Point)
                    {
                        viewStatePoints = new PointCollection { (Point)viewStateValue };
                    }
                    if (viewStatePoints != null)
                    {
                        foreach (Point viewStatePoint in viewStatePoints)
                        {
                            topLeft.X = topLeft.X > viewStatePoint.X ? viewStatePoint.X : topLeft.X;
                            topLeft.Y = topLeft.Y > viewStatePoint.Y ? viewStatePoint.Y : topLeft.Y;
                        }
                    }
                }
            }

            //Update the viewState.
            OffSetViewState(new Vector(newOrigin.X - topLeft.X, newOrigin.Y - topLeft.Y), modelItemsInMetaData);
        }

        PointCollection OffsetPointCollection(PointCollection collection, Vector offset)
        {
            if (collection == null)
            {
                return null;
            }

            PointCollection newcollection = new PointCollection();
            foreach (Point pt in collection)
            {
                Point nPt = Point.Add(pt, offset);
                newcollection.Add(nPt);
            }
            return newcollection;
        }

        void OffSetViewState(Vector offsetVector, ModelItem modelItem, bool isUndoableViewState)
        {
            Dictionary<string, object> modifiedValues = new Dictionary<string, object>();
            Dictionary<string, object> viewState = this.ViewStateService.RetrieveAllViewState(modelItem);
            foreach (KeyValuePair<string, object> viewStatePair in viewState)
            {
                PointCollection viewStatePoints = viewStatePair.Value as PointCollection;
                if (viewStatePoints != null)
                {

                    modifiedValues.Add(viewStatePair.Key, OffsetPointCollection(viewStatePoints, offsetVector));
                }
                else if (viewStatePair.Value is Point)
                {
                    modifiedValues.Add(viewStatePair.Key, Point.Add((Point)viewStatePair.Value, offsetVector));
                }
            }
            foreach (KeyValuePair<string, object> kvPair in modifiedValues)
            {
                if (isUndoableViewState)
                {
                    this.ViewStateService.StoreViewStateWithUndo(modelItem, kvPair.Key, kvPair.Value);
                }
                else
                {
                    this.ViewStateService.StoreViewState(modelItem, kvPair.Key, kvPair.Value);
                }
            }

            modifiedValues.Clear();
        }


        void OffSetViewState(Vector offsetVector, List<ModelItem> modelItemsInMetaData)
        {
            foreach (ModelItem modelItem in modelItemsInMetaData)
            {
                OffSetViewState(offsetVector, modelItem, false);
            }
        }

        HashSet<Connector> GetRelatedConnectors(IEnumerable<ModelItem> modelItems)
        {
            HashSet<Connector> connectors = new HashSet<Connector>();
            foreach (ModelItem shapeModelItem in modelItems)
            {
                UIElement deleteElement = (UIElement)(this.modelElement[shapeModelItem]);
                List<Connector> attachedConnectors = GetAttachedConnectors(deleteElement);
                connectors.UnionWith(attachedConnectors);
            }
            return connectors;
        }

        //This does a shallow copy of all the public properties with getter and setter. 
        //It also replicates Xaml Attached properties.
        FlowNode CloneFlowElement(FlowNode flowElement, Predicate<AttachableMemberIdentifier> allowAttachableProperty = null)
        {
            Type flowElementType = flowElement.GetType();
            FlowNode clonedObject = (FlowNode)Activator.CreateInstance(flowElementType);
            foreach (PropertyInfo propertyInfo in flowElementType.GetProperties())
            {
                if (propertyInfo.GetGetMethod() != null && propertyInfo.GetSetMethod() != null)
                {
                    propertyInfo.SetValue(clonedObject, propertyInfo.GetValue(flowElement, null), null);
                }
            }

            //Replicate any Xaml Attached Property.
            KeyValuePair<AttachableMemberIdentifier, object>[] attachedProperties = new KeyValuePair<AttachableMemberIdentifier, object>[AttachablePropertyServices.GetAttachedPropertyCount(flowElement)];
            AttachablePropertyServices.CopyPropertiesTo(flowElement, attachedProperties, 0);
            foreach (KeyValuePair<AttachableMemberIdentifier, object> attachedProperty in attachedProperties)
            {
                if (allowAttachableProperty != null && !allowAttachableProperty(attachedProperty.Key))
                {
                    continue;
                }
                AttachablePropertyServices.SetProperty(clonedObject, attachedProperty.Key, attachedProperty.Value);
            }

            return clonedObject;
        }

        public bool IsDefaultContainer
        {
            get { return true; }
        }
    }
}
