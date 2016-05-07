//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Activities.Core.Presentation
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Activities.Statements;
    using System.Activities.Presentation;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Model;
    using System.Runtime;
    using System.Globalization;
    using System.Activities.Presentation.FreeFormEditing;

    partial class FlowchartDesigner
    {
        // The method will return the CaseKey if any.
        // referenceUpdatedModelItems: say A linked to B by linker C, there is a relationship:
        //   A.Properties["Relation"] = B. 
        //   When delete the linker C, A will be updated like, A.Properties["Relation"] = null;
        //   In multiple drag/drop, the A.Properties["Relation"] is set correctly before coming
        //   here, which means we should not set the value again, otherwise the correct,
        //   value which is set previously, will be removed.
        internal IFlowSwitchLink DeleteLink(Connector link, bool isMoveOrAutoSplit = false,
            HashSet<ModelItem> referenceUpdatedModelItems = null)
        {
            IFlowSwitchLink caseKey = null;
            using (EditingScope deleteLinkEditingScope =
                        ((IModelTreeItem)this.ModelItem).ModelTreeManager.CreateEditingScope(SR.FCDeleteLink))
            {
                caseKey = DeleteLinkImpl(link, isMoveOrAutoSplit, referenceUpdatedModelItems);
                deleteLinkEditingScope.Complete();
            }
            return caseKey;
        }

        private IFlowSwitchLink DeleteLinkImpl(Connector link, bool isMoveOrAutoSplit = false, 
            HashSet<ModelItem> referenceUpdatedModelItems = null)
        {
            IFlowSwitchLink caseKey = null;
            ModelItem linkModelItem = FlowchartDesigner.GetLinkModelItem(link);

            if (referenceUpdatedModelItems != null
                && referenceUpdatedModelItems.Contains(linkModelItem))
            {
                return caseKey;
            }
            ConnectionPoint srcConnectionPoint = FreeFormPanel.GetSourceConnectionPoint(link);
            ConnectionPoint destConnectionPoint = FreeFormPanel.GetDestinationConnectionPoint(link);

            if (typeof(FlowStep).IsAssignableFrom(linkModelItem.ItemType))
            {
                linkModelItem.Properties["Next"].SetValue(null);
            }
            else if (typeof(FlowDecision).IsAssignableFrom(linkModelItem.ItemType))
            {
                //Determine if it is True or False branch.
                if (srcConnectionPoint.Equals(FlowchartDesigner.GetTrueConnectionPoint(srcConnectionPoint.ParentDesigner)))
                {
                    //True branch
                    linkModelItem.Properties["True"].SetValue(null);
                }
                else
                {
                    linkModelItem.Properties["False"].SetValue(null);
                }
            }
            else if (typeof(IFlowSwitchLink).IsAssignableFrom(linkModelItem.ItemType))
            {
                IFlowSwitchLink flowSwitchLink = (IFlowSwitchLink)linkModelItem.GetCurrentValue();
                caseKey = flowSwitchLink;
                //Transitioning from the fakeModelItem world to the real ModelItem world.
                FlowNode fs = flowSwitchLink.ParentFlowSwitch;
                ModelItem realFlowSwitchMI = (this.ModelItem as IModelTreeItem).ModelTreeManager.WrapAsModelItem(fs);
                if (referenceUpdatedModelItems != null
                    && referenceUpdatedModelItems.Contains(realFlowSwitchMI))
                {
                    return caseKey;
                }

                if (flowSwitchLink.IsDefaultCase)
                {
                    realFlowSwitchMI.Properties["Default"].SetValue(null);

                    if (!isMoveOrAutoSplit)
                    {
                        realFlowSwitchMI.Properties[FlowSwitchLabelFeature.DefaultCaseDisplayNamePropertyName].SetValue(FlowSwitchLabelFeature.DefaultCaseDisplayNameDefaultValue);
                    }
                }
                else
                {
                   GenericFlowSwitchHelper.RemoveCase(realFlowSwitchMI.Properties["Cases"], flowSwitchLink.CaseObject);
                }

            }
            else // StartNode
            {
                this.ModelItem.Properties["StartNode"].SetValue(null);
            }
            
            this.StoreConnectorViewState(linkModelItem, null, srcConnectionPoint, true);
            return caseKey;
        }


        void DeleteShape(ModelItem shapeModelItem, HashSet<ModelItem> updatedItems = null)
        {
            ModelItem flowElementMI = GetFlowElementMI(shapeModelItem);
            Fx.Assert(flowElementMI != null, "Invalid shape in Flowchart");
            bool itemRemoved = this.ModelItem.Properties["Nodes"].Collection.Remove(flowElementMI);
            //Clean up the FlowStep so that shapeModelItem.Parents will be updated and FlowStep will not be leaked.
            if (typeof(FlowStep).IsAssignableFrom(flowElementMI.ItemType)
                && (updatedItems == null || !updatedItems.Contains(flowElementMI)))
            {
                flowElementMI.Properties["Action"].SetValue(null);
            }
            Fx.Assert(itemRemoved, "Selected item not present in the Flowchart object");
        }


        bool UpdateFlowChartObject(ConnectionPoint sourceConnPoint, ConnectionPoint destConnPoint, out string errorMessage, bool isLinkValidDueToLinkMove, IFlowSwitchLink caseKey)
        {
            //srcDesigner will be null for the case where source designer is StartSymbol.
            VirtualizedContainerService.VirtualizingContainer srcDesigner = sourceConnPoint.ParentDesigner as VirtualizedContainerService.VirtualizingContainer;
            VirtualizedContainerService.VirtualizingContainer destDesigner = destConnPoint.ParentDesigner as VirtualizedContainerService.VirtualizingContainer;
            ModelItem linkSource;
            ModelItem linkDest = destDesigner.ModelItem;
            ModelItem destFlowElementMI = GetFlowElementMI(linkDest);
            PointCollection connectorViewState = new PointCollection(ConnectorRouter.Route(this.panel, sourceConnPoint, destConnPoint));
            errorMessage = string.Empty;
            
            if (sourceConnPoint.ParentDesigner is StartSymbol)
            {
                linkSource = this.ModelItem;
                if (linkSource.Properties["StartNode"].Value == null || isLinkValidDueToLinkMove)
                {
                    this.StoreConnectorViewState(linkSource, connectorViewState, sourceConnPoint);
                    linkSource.Properties["StartNode"].SetValue(destFlowElementMI);
                }
                else
                {
                    errorMessage = SR.FCNextLinkDefined;
                }
            }
            else
            {
                linkSource = srcDesigner.ModelItem;
                ModelItem srcFlowElementMI = GetFlowElementMI(linkSource);

                if (typeof(FlowStep).IsAssignableFrom(srcFlowElementMI.ItemType))
                {
                    if (srcFlowElementMI.Properties["Next"].Value == null || isLinkValidDueToLinkMove)
                    {
                        this.StoreConnectorViewState(srcFlowElementMI, connectorViewState, sourceConnPoint);
                        srcFlowElementMI.Properties["Next"].SetValue(destFlowElementMI);
                    }
                    else
                    {
                        errorMessage = SR.FCNextLinkDefined;
                    }
                }
                else if (typeof(FlowDecision).IsAssignableFrom(srcFlowElementMI.ItemType))
                {
                    if (sourceConnPoint.Equals(FlowchartDesigner.GetTrueConnectionPoint(this.modelElement[linkSource])))
                    {
                        if (linkSource.Properties["True"].Value == null || isLinkValidDueToLinkMove)
                        {
                            this.StoreConnectorViewState(srcFlowElementMI, connectorViewState, sourceConnPoint);
                            linkSource.Properties["True"].SetValue(destFlowElementMI);
                        }
                        else
                        {
                            errorMessage = SR.FCTrueBranchExists;
                        }
                    }
                    else if (sourceConnPoint.Equals(FlowchartDesigner.GetFalseConnectionPoint(this.modelElement[linkSource])))
                    {
                        if (linkSource.Properties["False"].Value == null || isLinkValidDueToLinkMove)
                        {
                            this.StoreConnectorViewState(srcFlowElementMI, connectorViewState, sourceConnPoint);
                            linkSource.Properties["False"].SetValue(destFlowElementMI);
                        }
                        else
                        {
                            errorMessage = SR.FCFalseBranchExists;
                        }
                    }
                    else
                    {
                        errorMessage = SR.FCFlowConditionLinkError;
                    }

                }
                else //FlowSwitch
                {
                    if (!CreateFlowSwitchLink(sourceConnPoint, srcFlowElementMI, destFlowElementMI, caseKey, connectorViewState, ref errorMessage))
                    {
                        return false;
                    }
                }
            }
            return errorMessage.Equals(string.Empty);
        }

        bool CreateFlowSwitchLink(ConnectionPoint sourceConnPoint, ModelItem srcFlowElementMI, ModelItem destFlowElementMI, IFlowSwitchLink caseKey, PointCollection connectorViewState, ref string errorMessage)
        {
            IModelTreeItem modelTreeItem = this.ModelItem as IModelTreeItem;
            if ((caseKey != null && caseKey.IsDefaultCase) || (caseKey == null && srcFlowElementMI.Properties["Default"].Value == null))
            {
                IFlowSwitchLink link = GenericFlowSwitchHelper.CreateFlowSwitchLink(srcFlowElementMI.ItemType, srcFlowElementMI, null, true);
                ModelItem linkModelItem = new FakeModelItemImpl(modelTreeItem.ModelTreeManager, link.GetType(), link, null);
                link.ModelItem = linkModelItem;
                if (connectorViewState != null)
                {
                    this.StoreConnectorViewState(linkModelItem, connectorViewState, sourceConnPoint);
                }
                srcFlowElementMI.Properties["Default"].SetValue(destFlowElementMI);
            }
            else
            {
                ModelProperty casesProp = srcFlowElementMI.Properties["Cases"];

                string uniqueCaseName = null;

                if (caseKey == null)
                {
                    Type typeArgument = srcFlowElementMI.ItemType.GetGenericArguments()[0];
                    if (GenericFlowSwitchHelper.CanBeGeneratedUniquely(typeArgument))
                    {
                        uniqueCaseName = GenericFlowSwitchHelper.GetCaseName(casesProp, typeArgument, out errorMessage);
                    }
                    else
                    {
                        FlowSwitchCaseEditorDialog editor = new FlowSwitchCaseEditorDialog(srcFlowElementMI, this.Context, this, SR.AddNewCase, typeArgument);
                        editor.WindowSizeToContent = SizeToContent.WidthAndHeight;
                        if (!editor.ShowOkCancel())
                        {
                            return false;
                        }
                        uniqueCaseName = editor.CaseName;
                    }
                }
                else
                {
                    uniqueCaseName = caseKey.CaseName;
                }

                if (string.IsNullOrEmpty(errorMessage))
                {
                    IFlowSwitchLink link = GenericFlowSwitchHelper.CreateFlowSwitchLink(srcFlowElementMI.ItemType, srcFlowElementMI, uniqueCaseName, false);
                    ModelItem linkModelItem = new FakeModelItemImpl(modelTreeItem.ModelTreeManager, link.GetType(), link, null);
                    link.ModelItem = linkModelItem;
                    if (connectorViewState != null)
                    {
                        this.StoreConnectorViewState(linkModelItem, connectorViewState, sourceConnPoint);
                    }
                    GenericFlowSwitchHelper.AddCase(srcFlowElementMI.Properties["Cases"], link.CaseObject, destFlowElementMI.GetCurrentValue());
                }
            }
            return true;
        }

        //Interfaces for users to create links on the flowchart.
        internal bool CreateLinkGesture(ConnectionPoint sourceConnectionPoint, ConnectionPoint destConnectionPoint, out string errorMessage, IFlowSwitchLink caseKey)
        {
            return CreateLinkGesture(sourceConnectionPoint, destConnectionPoint, out errorMessage, false, caseKey);
        }
        
        internal bool CreateLinkGesture(ConnectionPoint sourceConnectionPoint, ConnectionPoint destConnectionPoint, out string errorMessage, bool isLinkValidDueToLinkMove, IFlowSwitchLink caseKey)
        {
            Fx.Assert(this.panel != null, "This code should not be hit if panel is null");
            Fx.Assert(sourceConnectionPoint != null, "sourceConnectionPoint is null.");
            Fx.Assert(destConnectionPoint != null, "destConnectionPoint is null.");
            bool linkCreated = false;
            errorMessage = string.Empty;
            if (destConnectionPoint.PointType != ConnectionPointKind.Outgoing && sourceConnectionPoint.PointType != ConnectionPointKind.Incoming)
            {
                using (EditingScope es = (EditingScope)this.ModelItem.BeginEdit(SR.FCCreateLink))
                {
                    linkCreated = UpdateFlowChartObject(sourceConnectionPoint, destConnectionPoint, out errorMessage, isLinkValidDueToLinkMove, caseKey);
                    try
                    {
                        es.Complete();
                    }
                    catch (ArgumentException)
                    {
                        errorMessage = SR.InvalidFlowSwitchCaseMessage;
                        linkCreated = false;
                    }

                }
            }
            else
            {
                errorMessage = SR.FCInvalidLink;
            }
         
            return linkCreated;
        }

        internal bool CreateLinkGesture(ConnectionPoint sourceConnectionPoint, UIElement dest, Point mouseLocation, out string errorMessage, bool isLinkValidDueToLinkMove, IFlowSwitchLink caseKey)
        {
            bool linkCreated = false;
            double minDist;
            errorMessage = string.Empty;

            ConnectionPoint destConnectionPoint = FindClosestConnectionPoint(
                mouseLocation, 
                FlowchartDesigner.GetConnectionPoints(dest).Where(p => p.PointType != ConnectionPointKind.Outgoing).ToList(), 
                out minDist);

            if (destConnectionPoint != null)
            {
                linkCreated = CreateLinkGesture(sourceConnectionPoint, destConnectionPoint, out errorMessage, isLinkValidDueToLinkMove, caseKey);
            }
            else
            {
                errorMessage = SR.FCInvalidLink;
            }

            return linkCreated;
        }
        internal bool CreateLinkGesture(ConnectionPoint sourceConnectionPoint, UIElement dest, out string errorMessage, bool isLinkValidDueToLinkMove, IFlowSwitchLink caseKey)
        {
            bool linkCreated = false;
            ConnectionPoint destConnectionPoint = ClosestDestConnectionPoint(sourceConnectionPoint, dest, out errorMessage);
            if (destConnectionPoint != null)
            {
                linkCreated = CreateLinkGesture(sourceConnectionPoint, destConnectionPoint, out errorMessage, isLinkValidDueToLinkMove, caseKey);
            }
            return linkCreated;
        }

        internal bool CreateLinkGesture(UIElement source, ConnectionPoint destConnectionPoint, Point mouseLocation, out string errorMessage, bool isLinkValidDueToLinkMove, IFlowSwitchLink caseKey)
        {
            bool linkCreated = false;
            double minDist;
            errorMessage = string.Empty;

            ConnectionPoint sourceConnectionPoint = FindClosestConnectionPoint(
                mouseLocation,
                FlowchartDesigner.GetConnectionPoints(source).Where(p => p.PointType != ConnectionPointKind.Incoming).ToList(),
                out minDist);

            if (sourceConnectionPoint != null)
            {
                linkCreated = CreateLinkGesture(sourceConnectionPoint, destConnectionPoint, out errorMessage, isLinkValidDueToLinkMove, caseKey);
            }
            else
            {
                errorMessage = SR.FCInvalidLink;
            }

            return linkCreated;
        }

        internal bool CreateLinkGesture(UIElement source, UIElement dest, out string errorMessage, IFlowSwitchLink caseKey)
        {
            bool linkCreated = false;
            ConnectionPoint srcConnPoint, destConnPoint;
            GetSrcDestConnectionPoints(source, dest, out srcConnPoint, out destConnPoint, out errorMessage);
            if (srcConnPoint != null && destConnPoint != null)
            {
                linkCreated = CreateLinkGesture(srcConnPoint, destConnPoint, out errorMessage, caseKey);
            }
            return linkCreated;
        }
    }
}
