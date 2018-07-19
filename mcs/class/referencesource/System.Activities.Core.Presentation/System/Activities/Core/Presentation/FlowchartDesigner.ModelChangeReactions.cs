//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Activities.Presentation;
    using System.Activities.Presentation.FreeFormEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Threading;
    using System.Windows.Input;
    using System.Windows.Media;

    partial class FlowchartDesigner
    {
        ModelItem flowStart;
        const string ExpandViewStateKey = "IsExpanded";

        void CreateStartSymbol()
        {
            //Instantiate the start symbol
            StartSymbol start = System.Activities.Core.Presentation.StartSymbol.CreateStartSymbol(this.Context);
            start.Text = "Start";
            this.flowStart = start.ModelItem;
            DragDropHelper.SetCompositeView(start, this);
            modelElement.Add(flowStart, start);
            start.SizeChanged += new SizeChangedEventHandler(ChildSizeChanged);
            this.StartSymbol = start;
            PopulateConnectionPoints(this.StartSymbol, null);
            this.StartSymbol.MouseEnter += new MouseEventHandler(ChildElement_MouseEnter);
            this.StartSymbol.MouseLeave += new MouseEventHandler(ChildElement_MouseLeave);

            //Getting the View state information.
            object locationOfShape = this.ViewStateService.RetrieveViewState(this.ModelItem, shapeLocation);
            object sizeOfShape = this.ViewStateService.RetrieveViewState(this.ModelItem, shapeSize);
            if (locationOfShape != null)
            {
                Point locationPt = (Point)locationOfShape;
                FreeFormPanel.SetLocation(this.StartSymbol, locationPt);
            }
            else
            {
                //Set the location of the start symbol.
                this.StartSymbol.Measure(new Size(Double.PositiveInfinity, Double.PositiveInfinity));
                double startHeight = this.StartSymbol.DesiredSize.Height;
                double startWidth = this.StartSymbol.DesiredSize.Width;
                Point startPoint = new Point(panel.MinWidth / 2, startSymbolTopMargin + startHeight / 2);
                Point startLocation = SnapVisualToGrid(this.StartSymbol, startPoint, new Point(-1, -1), false);
                FreeFormPanel.SetLocation(this.StartSymbol, startLocation);
                this.internalViewStateChange = true;
                this.StoreShapeViewState(this.ModelItem, startLocation);
                this.internalViewStateChange = false;
            }
            if (sizeOfShape != null)
            {
                FreeFormPanel.SetChildSize(this.StartSymbol, (Size)sizeOfShape);
            }
        }

        protected override void OnModelItemChanged(object newItem)
        {
            // Make flowchart designer always collapse by default, but only if the user didnt explicitly specify collapsed or expanded.
            ViewStateService viewStateService = this.Context.Services.GetService<ViewStateService>();
            if (viewStateService != null)
            {
                bool? isExpanded = (bool?)viewStateService.RetrieveViewState((ModelItem)newItem, ExpandViewStateKey);
                if (isExpanded == null)
                {
                    viewStateService.StoreViewState((ModelItem)newItem, ExpandViewStateKey, false);
                }
            }
            base.OnModelItemChanged(newItem);

        }

        void OnViewStateChanged(object sender, ViewStateChangedEventArgs e)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            Fx.Assert(e.ParentModelItem != null, "ViewState should be associated with some modelItem");
            Connector changedConnector = null;
            if (e.ParentModelItem == this.ModelItem)
            {
                if (string.Equals(e.Key, FlowchartSizeFeature.WidthPropertyName, StringComparison.Ordinal))
                {
                    this.FlowchartWidth = (double)TypeDescriptor.GetProperties(this.ModelItem)[FlowchartSizeFeature.WidthPropertyName].GetValue(this.ModelItem);
                }
                else if (string.Equals(e.Key, FlowchartSizeFeature.HeightPropertyName, StringComparison.Ordinal))
                {
                    this.FlowchartHeight = (double)TypeDescriptor.GetProperties(this.ModelItem)[FlowchartSizeFeature.HeightPropertyName].GetValue(this.ModelItem);
                }
            }
            if ((IsFlowNode(e.ParentModelItem) || this.ModelItem.Equals(e.ParentModelItem)) && !this.internalViewStateChange)
            {
                ModelItem itemOnCanvas = this.GetCorrespondingElementOnCanvas(e.ParentModelItem);
                if (this.modelElement.ContainsKey(itemOnCanvas))
                {
                    if (e.Key.Equals(shapeLocation))
                    {
                        if (e.NewValue != null)
                        {
                            FreeFormPanel.SetLocation(this.modelElement[itemOnCanvas], (Point)e.NewValue);
                            this.panel.InvalidateMeasure();
                            if (e.OldValue != null)
                            {
                                this.shapeLocations.Remove((Point)e.OldValue);
                            }
                            this.shapeLocations.Add((Point)e.NewValue);
                        }
                    }
                    else
                    {
                        if (this.ModelItem.Equals(e.ParentModelItem)
                            && e.Key.Equals(ConnectorViewStateKey))
                        {
                            changedConnector = this.GetLinkOnCanvas(e.ParentModelItem, e.ParentModelItem.Properties["StartNode"].Value, "StartNode");
                        }
                        else if (typeof(FlowStep).IsAssignableFrom(e.ParentModelItem.ItemType)
                            && e.Key.Equals(ConnectorViewStateKey))
                        {
                            changedConnector = this.GetLinkOnCanvas(e.ParentModelItem, e.ParentModelItem.Properties["Next"].Value, "Next");
                        }
                        else if (typeof(FlowDecision).IsAssignableFrom(e.ParentModelItem.ItemType))
                        {
                            if (e.Key.Equals(TrueConnectorViewStateKey))
                            {
                                changedConnector = this.GetLinkOnCanvas(e.ParentModelItem, e.ParentModelItem.Properties["True"].Value, "True");
                            }
                            else if (e.Key.Equals(FalseConnectorViewStateKey))
                            {
                                changedConnector = this.GetLinkOnCanvas(e.ParentModelItem, e.ParentModelItem.Properties["False"].Value, "False");
                            }
                        }
                        else if (GenericFlowSwitchHelper.IsGenericFlowSwitch(e.ParentModelItem.ItemType))
                        {
                            if (e.Key.Equals(FlowchartDesigner.FlowSwitchDefaultViewStateKey, StringComparison.CurrentCulture))
                            {
                                changedConnector = this.GetLinkOnCanvas(e.ParentModelItem, e.ParentModelItem.Properties["Default"].Value, e.Key);
                            }
                            else if (e.Key.EndsWith(CaseViewStateKeyAppendString, StringComparison.CurrentCulture))
                            {
                                string switchCaseName = e.Key.Substring(0, e.Key.Length - CaseViewStateKeyAppendString.Length);
                                object switchCase = switchCaseName;
                                Type genericType = e.ParentModelItem.ItemType.GetGenericArguments()[0];
                                switchCase = GenericFlowSwitchHelper.GetObject(switchCaseName, genericType);

                                if (GenericFlowSwitchHelper.ContainsCaseKey(e.ParentModelItem.Properties["Cases"], switchCase))
                                {
                                    //Prepending with GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier to differentiate between the property Default and the key Default.
                                    changedConnector = this.GetLinkOnCanvas(e.ParentModelItem, GenericFlowSwitchHelper.GetCaseModelItem(e.ParentModelItem.Properties["Cases"], switchCase), GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier + switchCase);
                                }
                            }

                        }
                    }
                }
            }
            if (changedConnector != null)
            {
                if (e.NewValue != null)
                {
                    Fx.Assert(e.NewValue is PointCollection, "e.NewValue is not PointCollection");
                    changedConnector.Points = e.NewValue as PointCollection;
                    this.panel.RemoveConnectorEditor();
                    this.panel.InvalidateMeasure();
                }
            }
        }

        void RefreshFlowSwitchLinkModelItem(ModelItem flowSwitchModelItem, Connector connector, bool isDefault)
        {
            ModelItem oldLinkModelItem = FlowchartDesigner.GetLinkModelItem(connector);

            IModelTreeItem modelTreeItem = flowSwitchModelItem as IModelTreeItem;
            IFlowSwitchLink link = GenericFlowSwitchHelper.CreateFlowSwitchLink(flowSwitchModelItem.ItemType, flowSwitchModelItem, ((IFlowSwitchLink)oldLinkModelItem.GetCurrentValue()).CaseObject, isDefault);
            ModelItem linkModelItem = new FakeModelItemImpl(modelTreeItem.ModelTreeManager, link.GetType(), link, null);
            link.ModelItem = linkModelItem;

            FlowchartDesigner.SetLinkModelItem(connector, linkModelItem);
            connector.SetBinding(Connector.LabelTextProperty, link.CreateConnectorLabelTextBinding());

            Selection currentSelection = this.Context.Items.GetValue<Selection>();
            if (currentSelection.SelectedObjects.Contains(oldLinkModelItem))
            {
                Selection.Toggle(this.Context, oldLinkModelItem);
                Selection.Select(this.Context, linkModelItem);
            }
        }

        //For flowchart reacting to ModelItem changes we are concerned of the following scenarios:
        //1. FlowElements being deleted from the Flowchart.Nodes collection or Flowswitch cases being deleted from ItemsCollection
        //2. FlowElements being added to the Flowchart.Nodes collection or Flowswitch cases being added from ItemsCollection
        //3. Properties being changed in FlowStep(Next), FlowDecision(True, false), FlowSwitch(Default) (Any of the flowelemnet should be present in the elements collection).
        //4. Flowswitch cases being added/remove via Cases.Dicitionary
        void ModelTreeManager_EditingScopeCompleted(object sender, EditingScopeEventArgs e)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            foreach (Change change in e.EditingScope.Changes)
            {
                //Case 1, 2.
                if (change is CollectionChange)
                {
                    CollectionChange collectionChange = change as CollectionChange;
                    if (collectionChange.Collection.Equals(this.ModelItem.Properties["Nodes"].Collection))
                    {
                        if (collectionChange.Operation == CollectionChange.OperationType.Delete)
                        {
                            this.DeleteShapeVisual(this.flowNodeToUIElement[collectionChange.Item]);
                        }
                        else
                        {
                            this.AddFlowElementsToDesigner(new List<ModelItem> { collectionChange.Item });
                            //An editing scope change references the ModelItem. 
                            //Hence in case of multiple changes to the same modelItem within the same EditingScope, we will see all the changes on the ModelItem for each change.
                            //Eg. Suppose following two changes are in the same editing scope: 1. Add ModelItem item1 to Collection, 2. Change a property on this MI, item1.Prop1
                            //In this case, EditingScope.Changes.Count will be 2. 
                            //Since an EditingScope change keeps a reference to the ModelItem changed, when we process the first change, the second change would already be reflected on the ModelItem.
                            //Hence, while processing CollectionChange for item1, item1.Prop1 will already reflect the new value. 
                            //Also there will be another change notifying the change in item1.Prop1.
                            //AddFlowElementsToDesigner() method, walks through the properties of a newly added item and creates any links if required. 
                            //This is necessary for Paste scenario where we want to create links between Items added to the Nodes Collection.
                            //Because of this behavior of AddFlowElementsToDesigner(), before reacting to a property change for adding a link, we will always verify that the link does not already exists.
                        }
                    }
                    if (collectionChange.Collection.Parent != null && collectionChange.Collection.Parent.Parent != null &&
                        this.ModelItem.Properties["Nodes"].Collection.Contains(collectionChange.Collection.Parent.Parent) &&
                        collectionChange.Collection.Parent.Parent.ItemType.IsGenericType &&
                        collectionChange.Collection.Parent.Parent.ItemType.GetGenericTypeDefinition() == typeof(FlowSwitch<>))
                    {
                        ModelItem item = collectionChange.Item;
                        string caseName = GenericFlowSwitchHelper.GetString(item.Properties["Key"].ComputedValue, item.Properties["Key"].PropertyType);

                        Connector connector = this.GetLinkOnCanvas(collectionChange.Collection.Parent.Parent,
                            item.Properties["Value"].Value, GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier + caseName);
                        if (collectionChange.Operation == CollectionChange.OperationType.Delete)
                        {
                            if (connector != null)
                            {
                                this.DeleteLinkVisual(connector);
                            }
                        }
                        else if (collectionChange.Operation == CollectionChange.OperationType.Insert)
                        {
                            if (connector == null)
                            {
                                //Prepending GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier to differentiate between the FlowSwitch's Property Default and key Default.
                                connector = this.CreatePropertyLink(collectionChange.Collection.Parent.Parent,
                                    item.Properties["Value"].Value,
                                    GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier + caseName);
                                Fx.Assert(connector != null, "Link not created");
                                this.panel.Children.Add(connector);
                            }
                            else
                            {
                                RefreshFlowSwitchLinkModelItem(/* flowSwitchModelItem = */ collectionChange.Collection.Parent.Parent, connector, false);
                            }
                        }
                    }
                }
                else if (change is DictionaryChange)
                {
                    // case 4
                    DictionaryChange dictionaryChange = change as DictionaryChange;

                    if (dictionaryChange.Dictionary.Parent != null &&
                        this.ModelItem.Properties["Nodes"].Collection.Contains(dictionaryChange.Dictionary.Parent) &&
                        dictionaryChange.Dictionary.Parent.ItemType.IsGenericType &&
                        dictionaryChange.Dictionary.Parent.ItemType.GetGenericTypeDefinition() == typeof(FlowSwitch<>))
                    {
                        ModelItem flowSwitchModelItem = dictionaryChange.Dictionary.Parent;
                        ModelItem caseTargetModelItem = dictionaryChange.Value;
                        string caseName = GenericFlowSwitchHelper.GetString(dictionaryChange.Key == null ? null : dictionaryChange.Key.GetCurrentValue(), dictionaryChange.Key == null ? null : dictionaryChange.Key.ItemType);
                        string caseNameInModelItem = GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier + caseName;

                        Connector connector = this.GetLinkOnCanvas(
                            flowSwitchModelItem,
                            caseTargetModelItem,
                            caseNameInModelItem);

                        if (dictionaryChange.Operation == DictionaryChange.OperationType.Delete)
                        {
                            if (connector != null)
                            {
                                this.DeleteLinkVisual(connector);
                            }
                        }
                        else if (dictionaryChange.Operation == DictionaryChange.OperationType.Insert)
                        {
                            if (connector == null)
                            {
                                connector = this.CreatePropertyLink(
                                    flowSwitchModelItem,
                                    caseTargetModelItem,
                                    caseNameInModelItem);
                                this.panel.Children.Add(connector);
                            }
                        }
                    }
                }
                //Case 3.
                else if (change is PropertyChange)
                {
                    PropertyChange propertyChange = change as PropertyChange;

                    if (this.ModelItem.Properties["Nodes"].Collection.Contains(propertyChange.Owner)
                        || (propertyChange.PropertyName == "StartNode" && propertyChange.Owner == this.ModelItem))
                    {
                        if (propertyChange.OldValue != null
                            && IsFlowNode(propertyChange.OldValue))
                        {
                            Connector link = GetLinkOnCanvas(propertyChange.Owner, propertyChange.OldValue, propertyChange.PropertyName);
                            //Debug.Assert(link != null, "Link not found on designer");
                            if (link != null)
                            {
                                this.DeleteLinkVisual(link);
                            }
                        }
                        if (propertyChange.NewValue != null
                            && IsFlowNode(propertyChange.NewValue))
                        {
                            Connector oldLink = GetLinkOnCanvas(propertyChange.Owner, propertyChange.NewValue, propertyChange.PropertyName);
                            //If this connector has already been added don't add again. 
                            if (oldLink == null)
                            {
                                Connector link = CreatePropertyLink(propertyChange.Owner, propertyChange.NewValue, propertyChange.PropertyName);
                                Fx.Assert(link != null, "Link not created");
                                this.panel.Children.Add(link);
                            }
                            else
                            {
                                if (GenericFlowSwitchHelper.IsGenericFlowSwitch(propertyChange.Owner.ItemType))
                                {
                                    this.RefreshFlowSwitchLinkModelItem(/* flowSwitchModelItem = */ propertyChange.Owner, oldLink, true);
                                }
                            }
                        }

                        //handling for the case where the FlowStep.Action changes:
                        //Explicitly adding a check for FlowStep, because other FlowNodes have properties of type Activity, which we don't want to react to.
                        //AddFlowElementsToDesigner() will add the links originating out of the shape that is changing.
                        //We have to take care of refreshing the links coming into the shape that is changing.
                        if (typeof(FlowStep).IsAssignableFrom(propertyChange.Owner.ItemType))
                        {
                            List<Connector> oldIncomingConnectors = new List<Connector>();
                            if (propertyChange.OldValue != null && IsFlowStepAction(propertyChange.OldValue))
                            {
                                UIElement oldShape = this.flowNodeToUIElement[propertyChange.Owner];
                                oldIncomingConnectors = this.GetInComingConnectors(oldShape);
                                this.DeleteShapeVisual(oldShape);
                            }
                            if (propertyChange.NewValue != null && IsFlowStepAction(propertyChange.NewValue))
                            {
                                this.AddFlowElementsToDesigner(new List<ModelItem> { propertyChange.Owner });
                                foreach (Connector oldConnector in oldIncomingConnectors)
                                {
                                    Connector newConnector = CreateLink(FreeFormPanel.GetSourceConnectionPoint(oldConnector),
                                        this.flowNodeToUIElement[propertyChange.Owner], FlowchartDesigner.GetLinkModelItem(oldConnector));
                                    this.panel.Children.Add(newConnector);
                                }
                            }
                        }
                    }
                }
            }
        }

        void AddFlowElementsToDesigner(IList<ModelItem> flowElementMICollection, bool addConnectorAfterLoaded = false)
        {
            Queue<ModelItem> flowElementsToProcess = new Queue<ModelItem>();
            List<UIElement> viewsAdded = new List<UIElement>();
            foreach (ModelItem model in flowElementMICollection)
            {
                ModelItem itemOnCanvas = GetCorrespondingElementOnCanvas(model);
                if (!this.modelElement.ContainsKey(itemOnCanvas))
                {
                    flowElementsToProcess.Enqueue(model);
                    viewsAdded.Add(ProcessAndGetModelView(itemOnCanvas));
                }
                else if (!this.panel.Children.Contains(this.modelElement[itemOnCanvas]))
                {
                    flowElementsToProcess.Enqueue(model);
                    viewsAdded.Add(this.modelElement[itemOnCanvas]);
                }
            }

            ModelItem startNodeModelItem = null;
            List<Tuple<UIElement, UIElement, ModelItem>> elem2elemConnections = new List<Tuple<UIElement, UIElement, ModelItem>>();
            List<Tuple<ConnectionPoint, UIElement, ModelItem>> point2elemConnections = new List<Tuple<ConnectionPoint, UIElement, ModelItem>>();

            while (flowElementsToProcess.Count > 0)
            {
                ModelItem currentMI = flowElementsToProcess.Dequeue();
                //Create links for the current FlowNode.
                //First of all check if this is connected to the start node.
                if (this.ModelItem.Properties["StartNode"].Value != null
                    && this.ModelItem.Properties["StartNode"].Value.Equals(currentMI))
                {
                    startNodeModelItem = currentMI;
                }
                if (typeof(FlowStep).IsAssignableFrom(currentMI.ItemType))
                {
                    ModelItem linkDest = currentMI.Properties["Next"].Value;
                    if (linkDest != null)
                    {
                        ModelItem src = GetCorrespondingElementOnCanvas(currentMI);
                        ModelItem dest = GetCorrespondingElementOnCanvas(linkDest);
                        if (!modelElement.ContainsKey(dest))
                        {
                            viewsAdded.Add(ProcessAndGetModelView(dest));
                            flowElementsToProcess.Enqueue(linkDest);
                        }
                        elem2elemConnections.Add(Tuple.Create(modelElement[src], modelElement[dest], currentMI));
                    }
                }
                else if (typeof(FlowDecision).IsAssignableFrom(currentMI.ItemType))
                {
                    ModelItem trueDest = currentMI.Properties["True"].Value;
                    ModelItem falseDest = currentMI.Properties["False"].Value;
                    if (trueDest != null)
                    {
                        ConnectionPoint srcConnectionPoint = FlowchartDesigner.GetTrueConnectionPoint(modelElement[currentMI]);
                        ModelItem trueDestOnCanvas = GetCorrespondingElementOnCanvas(trueDest);
                        if (!modelElement.ContainsKey(trueDestOnCanvas))
                        {
                            viewsAdded.Add(ProcessAndGetModelView(trueDestOnCanvas));
                            flowElementsToProcess.Enqueue(trueDest);
                        }
                        point2elemConnections.Add(Tuple.Create(srcConnectionPoint, modelElement[trueDestOnCanvas], currentMI));
                    }
                    if (falseDest != null)
                    {
                        ConnectionPoint srcConnectionPoint = FlowchartDesigner.GetFalseConnectionPoint(modelElement[currentMI]);
                        ModelItem falseDestOnCanvas = GetCorrespondingElementOnCanvas(falseDest);
                        if (!modelElement.ContainsKey(falseDestOnCanvas))
                        {
                            viewsAdded.Add(ProcessAndGetModelView(falseDestOnCanvas));
                            flowElementsToProcess.Enqueue(falseDest);
                        }
                        point2elemConnections.Add(Tuple.Create(srcConnectionPoint, modelElement[falseDestOnCanvas], currentMI));
                    }
                }
                else if (GenericFlowSwitchHelper.IsGenericFlowSwitch(currentMI.ItemType))
                {
                    IModelTreeItem modelTreeItem = this.ModelItem as IModelTreeItem;
                    ModelItem defaultCase = currentMI.Properties["Default"].Value;

                    if (defaultCase != null)
                    {
                        ModelItem defaultCaseOnCanvas = GetCorrespondingElementOnCanvas(defaultCase);
                        if (!modelElement.ContainsKey(defaultCaseOnCanvas))
                        {
                            viewsAdded.Add(ProcessAndGetModelView(defaultCaseOnCanvas));
                            flowElementsToProcess.Enqueue(defaultCase);
                        }
                        IFlowSwitchLink link = GenericFlowSwitchHelper.CreateFlowSwitchLink(currentMI.ItemType, currentMI, null, true);
                        ModelItem linkModelItem = new FakeModelItemImpl(modelTreeItem.ModelTreeManager, link.GetType(), link, null);
                        link.ModelItem = linkModelItem;

                        elem2elemConnections.Add(Tuple.Create(modelElement[currentMI], modelElement[defaultCaseOnCanvas], linkModelItem));
                    }
                    Type genericType = currentMI.ItemType.GetGenericArguments()[0];

                    foreach (ModelItem key in GenericFlowSwitchHelper.GetCaseKeys(currentMI.Properties["Cases"]))
                    {
                        ModelItem destFlowElementMI = GenericFlowSwitchHelper.GetCaseModelItem(currentMI.Properties["Cases"], (key == null) ? null : key.GetCurrentValue());
                        IFlowSwitchLink link = GenericFlowSwitchHelper.CreateFlowSwitchLink(currentMI.ItemType, currentMI, (key == null) ? null : key.GetCurrentValue(), false);
                        ModelItem linkModelItem = new FakeModelItemImpl(modelTreeItem.ModelTreeManager, link.GetType(), link, null);
                        link.ModelItem = linkModelItem;
                        ModelItem destModelItem = GetCorrespondingElementOnCanvas(destFlowElementMI);
                        if (!modelElement.ContainsKey(destModelItem))
                        {
                            viewsAdded.Add(ProcessAndGetModelView(destModelItem));
                            flowElementsToProcess.Enqueue(destFlowElementMI);
                        }

                        elem2elemConnections.Add(Tuple.Create(modelElement[currentMI], modelElement[destModelItem], linkModelItem));
                    }
                }
                else
                {
                    Fx.Assert(false, "Unknown type of FlowNode");
                }
            }

            if (!this.startNodeAdded)
            {
                panel.Children.Add(this.StartSymbol);
                this.startNodeAdded = true;
            }
            foreach (UIElement view in viewsAdded)
            {
                panel.Children.Add(view);
            }

            // connection between flownode should be create only after all flownodes have been loaded on the canvas
            if (addConnectorAfterLoaded)
            {
                this.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new Action(() =>
                {
                    if (this.isLoaded)
                    {
                        AddConnectorsToPanel(startNodeModelItem, elem2elemConnections, point2elemConnections);
                    }
                }));
            }
            else
            {
                AddConnectorsToPanel(startNodeModelItem, elem2elemConnections, point2elemConnections);
            }
        }

        private void AddConnectorsToPanel(
            ModelItem startNodeModelItem, 
            List<Tuple<UIElement, UIElement, ModelItem>> elem2elemConnections, 
            List<Tuple<ConnectionPoint, UIElement, ModelItem>> point2elemConnections)
        {
            List<Connector> connectorList = new List<Connector>();

            if (startNodeModelItem != null)
            {
                ModelItem dest = GetCorrespondingElementOnCanvas(startNodeModelItem);
                connectorList.Add(CreateLink(this.StartSymbol, modelElement[dest], this.ModelItem));
            }

            foreach (var connection in elem2elemConnections)
            {
                connectorList.Add(CreateLink(connection.Item1, connection.Item2, connection.Item3));
            }

            foreach (var connection in point2elemConnections)
            {
                connectorList.Add(CreateLink(connection.Item1, connection.Item2, connection.Item3));
            }

            foreach (Connector connector in connectorList)
            {
                panel.Children.Add(connector);
            }
        }

        void DeleteLinkVisual(Connector link)
        {
            ConnectionPoint srcConnectionPoint = FreeFormPanel.GetSourceConnectionPoint(link);
            ConnectionPoint destConnectionPoint = FreeFormPanel.GetDestinationConnectionPoint(link);
            //Update ConnectionPoints.            
            srcConnectionPoint.AttachedConnectors.Remove(link);
            destConnectionPoint.AttachedConnectors.Remove(link);

            this.panel.Children.Remove(link);
        }

        void DeleteShapeVisual(UIElement deleteShape)
        {
            //Remove any link visuals attached to this shape. This is required for the scenarios as follows:
            //Copy paste two Connected activities into flowchart and undo the paste. 
            //The property is not removed as a model change. Hence the link visual will remain dangling on the designer.
            List<Connector> attachedConnectors = GetAttachedConnectors(deleteShape);

            foreach (Connector connector in attachedConnectors)
            {
                Fx.Assert(this.panel.Children.Contains(connector), "Connector does not exist");
                this.DeleteLinkVisual(connector);
            }

            List<ConnectionPoint> connectionPoints = GetConnectionPoints(deleteShape);
            if (connectionPoints.Contains(this.srcConnectionPoint))
            {
                this.srcConnectionPoint = null;
                RemoveAdorner(this.panel, typeof(ConnectorCreationAdorner));
            }

            ModelItem shapeModelItem = ((VirtualizedContainerService.VirtualizingContainer)deleteShape).ModelItem;
            ModelItem flowNodeMI = this.GetFlowElementMI(shapeModelItem);

            this.modelElement.Remove(shapeModelItem);
            this.flowNodeToUIElement.Remove(flowNodeMI);
            deleteShape.MouseEnter -= new MouseEventHandler(ChildElement_MouseEnter);
            deleteShape.MouseLeave -= new MouseEventHandler(ChildElement_MouseLeave);
            ((FrameworkElement)deleteShape).SizeChanged -= new SizeChangedEventHandler(ChildSizeChanged);
            this.panel.Children.Remove(deleteShape);

            // deselect removed item
            if (this.Context != null)
            {
                HashSet<ModelItem> selectedItems = new HashSet<ModelItem>(this.Context.Items.GetValue<Selection>().SelectedObjects);
                if (selectedItems.Contains(shapeModelItem))
                {
                    Selection.Toggle(this.Context, shapeModelItem);
                }
            }

            //Update this.shapeLocations.
            object locationOfShape = this.ViewStateService.RetrieveViewState(flowNodeMI, shapeLocation);
            if (locationOfShape != null)
            {
                this.shapeLocations.Remove((Point)locationOfShape);
            }

        }

        Connector CreatePropertyLink(ModelItem srcModelItem, ModelItem propertyValue, string propertyName)
        {
            Connector newConnector = null;
            if (typeof(FlowStep).IsAssignableFrom(srcModelItem.ItemType))
            {
                ModelItem src = GetCorrespondingElementOnCanvas(srcModelItem);
                ModelItem dest = GetCorrespondingElementOnCanvas(propertyValue);
                newConnector = CreateLink(modelElement[src], modelElement[dest], srcModelItem);

            }
            else if (typeof(FlowDecision).IsAssignableFrom(srcModelItem.ItemType))
            {
                ModelItem dest = GetCorrespondingElementOnCanvas(propertyValue);
                ConnectionPoint srcConnPoint;
                if (propertyName.Equals("True"))
                {
                    srcConnPoint = FlowchartDesigner.GetTrueConnectionPoint(modelElement[srcModelItem]);
                }
                else
                {
                    srcConnPoint = FlowchartDesigner.GetFalseConnectionPoint(modelElement[srcModelItem]);
                }
                newConnector = CreateLink(srcConnPoint, modelElement[dest], srcModelItem);
            }
            else if (GenericFlowSwitchHelper.IsGenericFlowSwitch(srcModelItem.ItemType))
            {
                ModelItem dest = GetCorrespondingElementOnCanvas(propertyValue);
                IFlowSwitchLink link;
                if (propertyName.Equals("Default"))
                {
                    link = GenericFlowSwitchHelper.CreateFlowSwitchLink(srcModelItem.ItemType, srcModelItem, null, true);
                }
                else
                {
                    Fx.Assert(propertyName.Length >= GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier.Length, "Case property names should be prepended by the string GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier");
                    link = GenericFlowSwitchHelper.CreateFlowSwitchLink(srcModelItem.ItemType, srcModelItem, propertyName.Substring(GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier.Length), false);
                }
                IModelTreeItem modelTreeItem = (IModelTreeItem)this.ModelItem;
                ModelItem linkModelItem = new FakeModelItemImpl(modelTreeItem.ModelTreeManager, link.GetType(), link, null);
                link.ModelItem = linkModelItem;
                newConnector = CreateLink(modelElement[srcModelItem], modelElement[dest], linkModelItem);
            }
            else // FlowStart
            {
                ModelItem dest = GetCorrespondingElementOnCanvas(propertyValue);
                newConnector = CreateLink(this.StartSymbol, modelElement[dest], this.ModelItem);
            }
            return newConnector;

        }

        internal Connector GetLinkOnCanvas(ModelItem srcFlowElementModelItem, ModelItem destflowElementModelItem, string propertyName)
        {
            Connector linkOnCanvas = null;
            ModelItem shapeModelItem = null;
            List<Connector> outGoingConnectors = null;
            if (!srcFlowElementModelItem.Equals(this.ModelItem))
            {
                shapeModelItem = this.GetCorrespondingElementOnCanvas(srcFlowElementModelItem);
                outGoingConnectors = GetOutGoingConnectors(this.modelElement[shapeModelItem]);
            }
            else // Must be startNode
            {
                outGoingConnectors = GetOutGoingConnectors(this.StartSymbol);
            }

            foreach (Connector connector in outGoingConnectors)
            {
                ModelItem connectorDestModelItem = ((VirtualizedContainerService.VirtualizingContainer)FreeFormPanel.GetDestinationConnectionPoint(connector).ParentDesigner).ModelItem;
                ModelItem connectorDestFlowElementMI = this.GetFlowElementMI(connectorDestModelItem);
                //Following condition checks if the destination for current connector is equal to the destination passed in.
                if (destflowElementModelItem != null && destflowElementModelItem.Equals(connectorDestFlowElementMI))
                {
                    if (GenericFlowSwitchHelper.IsGenericFlowSwitch(srcFlowElementModelItem.ItemType))
                    {
                        ModelItem linkModelItem = FlowchartDesigner.GetLinkModelItem(connector);
                        if (linkModelItem.Properties["IsDefaultCase"].Value.GetCurrentValue().Equals(true) && propertyName.Equals("Default"))
                        {
                            linkOnCanvas = connector;
                            break;
                        }
                        else
                        {
                            ModelItem connectorCaseMI = linkModelItem.Properties["Case"].Value;
                            if (linkModelItem.Properties["IsDefaultCase"].Value.GetCurrentValue().Equals(false))
                            {
                                string caseName = connectorCaseMI == null ? null : GenericFlowSwitchHelper.GetString(connectorCaseMI.GetCurrentValue(), connectorCaseMI.ItemType);
                                if (connectorCaseMI != null && caseName.Equals(propertyName.Substring(GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier.Length)))
                                {
                                    linkOnCanvas = connector;
                                    break;
                                }
                                else if (connectorCaseMI == null)
                                {
                                    if (GenericFlowSwitchHelper.FlowSwitchNullCaseKeyIdentifier.Equals(propertyName.Substring(GenericFlowSwitchHelper.FlowSwitchCasesKeyIdentifier.Length)))
                                    {
                                        linkOnCanvas = connector;
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    else if (typeof(FlowDecision).IsAssignableFrom(srcFlowElementModelItem.ItemType))
                    {
                        ConnectionPoint trueConnPoint = FlowchartDesigner.GetTrueConnectionPoint(this.modelElement[shapeModelItem]);
                        ConnectionPoint falseConnPoint = FlowchartDesigner.GetFalseConnectionPoint(this.modelElement[shapeModelItem]);
                        ConnectionPoint connectorSrcConnPoint = FreeFormPanel.GetSourceConnectionPoint(connector);
                        if ((propertyName.Equals("True") && connectorSrcConnPoint.Equals(trueConnPoint))
                            || (propertyName.Equals("False") && connectorSrcConnPoint.Equals(falseConnPoint)))
                        {
                            linkOnCanvas = connector;
                            break;
                        }
                    }
                    else    //FlowStep case.
                    {
                        linkOnCanvas = connector;
                        break;
                    }
                }
            }
            return linkOnCanvas;
        }


        //Finally the call to CreateLink ends in calling this overloaded method.
        Connector CreateLink(ConnectionPoint sourceConnectionPoint, ConnectionPoint destConnectionPoint, ModelItem linkModelItem)
        {
            Fx.Assert(sourceConnectionPoint != null, "sourceConnectionPoint is null.");
            Fx.Assert(destConnectionPoint != null, "destinationConnectionPoint is null.");
            Connector newConnector = null;
            if (destConnectionPoint.PointType != ConnectionPointKind.Outgoing && sourceConnectionPoint.PointType != ConnectionPointKind.Incoming)
            {
                newConnector = GetConnectorViewState(sourceConnectionPoint.ParentDesigner, destConnectionPoint.ParentDesigner, linkModelItem, sourceConnectionPoint);
                if (newConnector == null)
                {
                    newConnector = GetConnector(linkModelItem, sourceConnectionPoint, destConnectionPoint);
                }
                else
                {
                    //This is a workaround for CSDMain 139197, if any sectment of a connector is neither vertical nor horizontal, we'll reroute it.
                    RerouteIfInvalid(newConnector, linkModelItem);
                }
                Fx.Assert(newConnector != null, "Link could not be created");
            }
            return newConnector;
        }

        Connector CreateLink(ConnectionPoint sourceConnectionPoint, UIElement dest, ModelItem linkModelItem)
        {
            Connector newConnector = null;
            ConnectionPoint destConnectionPoint = null;
            if (this.srcConnectionPointForAutoConnect != null)
            {
                Fx.Assert(this.srcConnectionPointForAutoConnect == sourceConnectionPoint, "sourceConnectionPoint should equal to this.srcConnectionPointForAutoConnect");
                destConnectionPoint = FlowchartDesigner.GetDestinationConnectionPointForAutoConnect(dest, sourceConnectionPoint);
                this.srcConnectionPointForAutoConnect = null;
            }
            else if (this.srcConnectionPointForAutoSplit == sourceConnectionPoint)
            {
                destConnectionPoint = this.GetDestinationConnectionPointForAutoSplit(this.srcConnectionPointForAutoSplit, dest);
                this.srcConnectionPointForAutoSplit = null;
            }
            else
            {
                string errorMessage;
                destConnectionPoint = FindBestMatchDestConnectionPoint(sourceConnectionPoint, dest, out errorMessage);
            }
            if (destConnectionPoint != null)
            {
                newConnector = CreateLink(sourceConnectionPoint, destConnectionPoint, linkModelItem);
            }
            return newConnector;
        }


        Connector CreateLink(UIElement source, UIElement dest, ModelItem linkModelItem)
        {
            Connector newConnector = null;
            ConnectionPoint srcConnPoint = null, destConnPoint = null;
            if (this.srcConnectionPointForAutoConnect != null)
            {
                srcConnPoint = this.srcConnectionPointForAutoConnect;
                destConnPoint = FlowchartDesigner.GetDestinationConnectionPointForAutoConnect(dest, srcConnPoint);
                this.srcConnectionPointForAutoConnect = null;
            }
            else if (this.srcConnectionPointForAutoSplit != null && this.srcConnectionPointForAutoSplit.ParentDesigner == source)
            {
                srcConnPoint = this.srcConnectionPointForAutoSplit;
                destConnPoint = this.GetDestinationConnectionPointForAutoSplit(srcConnPoint, dest);
                this.srcConnectionPointForAutoSplit = null;
            }
            else if (this.destConnectionPointForAutoSplit != null && this.destConnectionPointForAutoSplit.ParentDesigner == dest)
            {
                destConnPoint = this.destConnectionPointForAutoSplit;
                srcConnPoint = this.GetSourceConnectionPointForAutoSplit(destConnPoint, source);
                this.destConnectionPointForAutoSplit = null;
            }
            else
            {
                string errorMessage;
                GetSrcDestConnectionPoints(source, dest, out srcConnPoint, out destConnPoint, out errorMessage);
            }
            if (srcConnPoint != null && destConnPoint != null)
            {
                newConnector = CreateLink(srcConnPoint, destConnPoint, linkModelItem);
            }
            return newConnector;
        }

        [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.ReviewUnusedParameters, Justification = "Existing code")]
        void RerouteIfInvalid(Connector connector, ModelItem linkModelItem)
        {
            if (connector.Points != null)
            {
                Point[] points = new Point[connector.Points.Count];
                connector.Points.CopyTo(points, 0);
                if (!ConnectorRouter.AreSegmentsValid(points))
                {
                    Reroute(connector, false);
                }
            }
        }
    }
}
