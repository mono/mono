//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System;
    using System.Activities.Debugger;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Xaml;
    using System.Activities.Statements;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.InteropServices;
    using System.Runtime.Versioning;
    using System.ServiceModel.Activities;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;
    using System.Windows.Media;
    using System.Xaml;
    using Microsoft.Activities.Presentation;
    using Microsoft.Activities.Presentation.Xaml;

    public static class CutCopyPasteHelper
    {
        internal static readonly DependencyProperty ChildContainersProperty =
            DependencyProperty.RegisterAttached("ChildContainers", typeof(HashSet<ICompositeView>), typeof(CutCopyPasteHelper), new UIPropertyMetadata(null));

        static object workflowCallbackContext = null;

        internal const string WorkflowClipboardFormat = "WorkflowXamlFormat";
        internal const string WorkflowClipboardFormat_TargetFramework = "WorkflowXamlFormat_TargetFramework";

        //define a workflow callback clipboard format - make it unique across all processes
        static readonly string WorkflowCallbackClipboardFormat = string.Format(CultureInfo.InvariantCulture, "WorkflowCallbackFormat{0}", Guid.NewGuid());

        const string versionInfo = "1.0";

        static IList<Type> disallowedTypesForCopy;

        static IEnumerable<Type> DisallowedTypesForCopy
        {
            get
            {
                if (null == disallowedTypesForCopy)
                {
                    disallowedTypesForCopy = new List<Type>();
                    disallowedTypesForCopy.Add(typeof(ActivityBuilder));
                    disallowedTypesForCopy.Add(typeof(ModelItemKeyValuePair<,>));
                    disallowedTypesForCopy.Add(typeof(WorkflowService));
                    disallowedTypesForCopy.Add(typeof(Catch));
                }
                return disallowedTypesForCopy;
            }
        }

        internal static void AddDisallowedTypeForCopy(Type type)
        {
            if (!DisallowedTypesForCopy.Any(p => type == p))
            {
                disallowedTypesForCopy.Add(type);
            }
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static void AddChildContainer(WorkflowViewElement viewElement, ICompositeView sourceContainer)
        {
            if (viewElement == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("viewElement"));
            }
            if (sourceContainer == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("sourceContainer"));
            }

            HashSet<ICompositeView> containers = (HashSet<ICompositeView>)viewElement.GetValue(CutCopyPasteHelper.ChildContainersProperty);
            if (containers == null)
            {
                containers = new HashSet<ICompositeView>();
                viewElement.SetValue(CutCopyPasteHelper.ChildContainersProperty, containers);
            }
            containers.Add(sourceContainer);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        static HashSet<ICompositeView> GetChildContainers(WorkflowViewElement workflowViewElement)
        {
            HashSet<ICompositeView> childContainers = null;
            if (workflowViewElement != null && workflowViewElement.ShowExpanded)
            {
                childContainers = (HashSet<ICompositeView>)workflowViewElement.GetValue(CutCopyPasteHelper.ChildContainersProperty);
            }
            return childContainers;
        }

        //This enables us to get children ICompositeViews from WorkflowViewElements. 
        //Eg. get the WorkflowItemsPresenter from SequenceDesigner.
        //This is useful for Cut-Copy-Paste, Delete handling, etc.
        internal static void RegisterWithParentViewElement(ICompositeView container)
        {
            WorkflowViewElement parent = GetParentViewElement(container);
            if (parent != null)
            {
                CutCopyPasteHelper.AddChildContainer(parent, container);
            }
        }

        //Returns the first WorkflowViewElement in the parent chain.
        //If ICompositeView is a WorkflowViewElement this method returns the same object.
        static WorkflowViewElement GetParentViewElement(ICompositeView container)
        {
            DependencyObject parent = container as DependencyObject;
            return GetParentViewElement(parent);
        }

        //Returns the first WorkflowViewElement in the parent chain.
        //Move this to a helper class.
        internal static WorkflowViewElement GetParentViewElement(DependencyObject obj)
        {
            while (obj != null && !(obj is WorkflowViewElement))
            {
                obj = VisualTreeHelper.GetParent(obj);
            }
            return obj as WorkflowViewElement;
        }
        
        internal static IList<object> SortFromMetaData(IList<object> itemsToPaste, List<object> metaData)
        {
            IList<object> mergedItemsToPaste = SortFromMetaDataOnly(metaData);
            // append items that are not sorted
            foreach (object itemToPaste in itemsToPaste)
            {
                if (!mergedItemsToPaste.Contains(itemToPaste))
                {
                    mergedItemsToPaste.Add(itemToPaste);
                }
            }
            return mergedItemsToPaste;
        }

        internal static IList<object> SortFromMetaDataOnly(List<object> metaData)
        {
            List<object> mergedItemsToPaste = new List<object>();
            if (metaData == null)
            {
                return mergedItemsToPaste;
            }

            foreach (object metaDataObject in metaData)
            {
                List<object> orderedItemsMetaData = metaDataObject as List<object>;
                
                if (orderedItemsMetaData == null)
                {
                    continue;
                }

                foreach (object objectToPaste in orderedItemsMetaData)
                {
                    if (!mergedItemsToPaste.Contains(objectToPaste))
                    {
                        mergedItemsToPaste.Add(objectToPaste);
                    }
                }
            }
           
            return mergedItemsToPaste;
        }

        public static void DoCut(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            Selection currentSelection = context.Items.GetValue<Selection>();
            List<ModelItem> modelItemsToCut = new List<ModelItem>(currentSelection.SelectedObjects);
            CutCopyPasteHelper.DoCut(modelItemsToCut, context);
        }

        internal static void DoCut(List<ModelItem> modelItemsToCut, EditingContext context)
        {
            if (modelItemsToCut == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("modelItemsToCut"));
            }
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            modelItemsToCut.RemoveAll((modelItem) => { return modelItem == null; });
            if (modelItemsToCut.Count > 0)
            {
                using (EditingScope es = (EditingScope)modelItemsToCut[0].BeginEdit(SR.CutOperationEditingScopeDescription))
                {
                    try
                    {
                        CutCopyOperation(modelItemsToCut, context, true);
                    }
                    catch (ExternalException e)
                    {
                        es.Revert();
                        ErrorReporting.ShowErrorMessage(e.Message);
                        return;
                    }
                    DesignerView view = context.Services.GetService<DesignerView>();
                    //Setting the selection to Breadcrumb root.
                    Fx.Assert(view != null, "DesignerView Cannot be null during cut");
                    WorkflowViewElement rootView = view.RootDesigner as WorkflowViewElement;
                    if (rootView != null)
                    {
                        Selection.SelectOnly(context, rootView.ModelItem);
                    }
                    es.Complete();
                }
            }
        }

        public static void DoCopy(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }

            Selection currentSelection = context.Items.GetValue<Selection>();
            List<ModelItem> modelItemsToCopy = new List<ModelItem>(currentSelection.SelectedObjects);
            CutCopyPasteHelper.DoCopy(modelItemsToCopy, context);
        }

        private static void DoCopy(List<ModelItem> modelItemsToCopy, EditingContext context)
        {
            if (modelItemsToCopy == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("modelItemsToCopy"));
            }

            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }

            // copy only works if we have DesignerView up and running so check and throw here
            if (context.Services.GetService<DesignerView>() == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.CutCopyRequiresDesignerView));
            }
            modelItemsToCopy.RemoveAll((modelItem) => { return modelItem == null; });
            try
            {
                CutCopyOperation(modelItemsToCopy, context, false);
            }
            catch (ExternalException e)
            {
                ErrorReporting.ShowErrorMessage(e.Message);
            }
        }

        static void CutCopyOperation(List<ModelItem> modelItemsToCutCopy, EditingContext context, bool isCutOperation)
        {
            List<object> objectsOnClipboard = null;
            List<object> metaData = null;
            if (modelItemsToCutCopy.Count > 0)
            {
                objectsOnClipboard = new List<object>(modelItemsToCutCopy.Count);
                metaData = new List<object>();
                Dictionary<ICompositeView, List<ModelItem>> notifyDictionary = new Dictionary<ICompositeView, List<ModelItem>>();
                UIElement breadCrumbRootView = ((DesignerView)context.Services.GetService<DesignerView>()).RootDesigner;
                foreach (ModelItem modelItem in modelItemsToCutCopy)
                {
                    object currentElement = modelItem.GetCurrentValue();

                    if (typeof(Activity).IsAssignableFrom(currentElement.GetType()))
                    {
                        string fileName;
                        if (AttachablePropertyServices.TryGetProperty(currentElement, XamlDebuggerXmlReader.FileNameName, out fileName))
                        {
                            AttachablePropertyServices.RemoveProperty(currentElement, XamlDebuggerXmlReader.FileNameName);
                        }
                    }

                    if (modelItem.View != null)
                    {
                        //The case where the breadcrumbroot designer is Cut/Copied. We do not delete the root designer, we only copy it.
                        if (breadCrumbRootView.Equals(modelItem.View))
                        {
                            notifyDictionary.Clear();
                            objectsOnClipboard.Add(modelItem.GetCurrentValue());
                            break;
                        }
                        else
                        {
                            ICompositeView container = (ICompositeView)DragDropHelper.GetCompositeView((WorkflowViewElement)modelItem.View);
                            if (container != null)
                            {
                                //If the parent and some of its children are selected and cut/copied, we ignore the children. 
                                //The entire parent will be cut/copied. 
                                //HashSet parentModelItems contains all the model items in the parent chain of current modelItem.
                                //We use HashSet.IntersectWith operation to determine if one of the parents is set to be cut.
                                HashSet<ModelItem> parentModelItems = CutCopyPasteHelper.GetSelectableParentModelItems(modelItem);
                                parentModelItems.IntersectWith(modelItemsToCutCopy);
                                if (parentModelItems.Count == 0)
                                {
                                    if (!notifyDictionary.ContainsKey(container))
                                    {
                                        notifyDictionary[container] = new List<ModelItem>();
                                    }
                                    notifyDictionary[container].Add(modelItem);
                                }
                            }
                        }

                    }
                }

                foreach (ICompositeView container in notifyDictionary.Keys)
                {
                    object containerMetaData = false;
                    if (isCutOperation)
                    {
                        containerMetaData = container.OnItemsCut(notifyDictionary[container]);
                    }
                    else
                    {
                        containerMetaData = container.OnItemsCopied(notifyDictionary[container]);
                    }
                    if (containerMetaData != null)
                    {
                        metaData.Add(containerMetaData);
                    }
                    //Put the actual activities and not the modelItems in the clipboard.
                    foreach (ModelItem modelItem in notifyDictionary[container])
                    {
                        objectsOnClipboard.Add(modelItem.GetCurrentValue());
                    }
                }
                if (metaData.Count == 0)
                {
                    metaData = null;
                }
            }
            try
            {
                FrameworkName targetFramework = context.Services.GetService<DesignerConfigurationService>().TargetFrameworkName;
                PutOnClipBoard(objectsOnClipboard, metaData, targetFramework);
            }
            catch (XamlObjectReaderException exception)
            {
                if (modelItemsToCutCopy.Count > 0 && ErrorActivity.GetHasErrorActivities(modelItemsToCutCopy[0].Root.GetCurrentValue()))
                {
                    ErrorReporting.ShowErrorMessage(SR.CutCopyErrorActivityMessage);
                }
                else
                {
                    ErrorReporting.ShowErrorMessage(exception.Message);
                }
            }
        }

        //This method collects all the ModelItems in the parent chain by calling the GetSelectableParentViewElements method 
        //which walks the WPF Visual tree. We want to avoid walking ModelItem tree.
        internal static HashSet<ModelItem> GetSelectableParentModelItems(ModelItem modelItem)
        {
            if (null == modelItem)
            {
                throw FxTrace.Exception.ArgumentNull("modelItem");
            }
            List<WorkflowViewElement> parentViewElements = GetSelectableParentViewElements(modelItem.View as WorkflowViewElement);
            HashSet<ModelItem> parentModelItems = new HashSet<ModelItem>();
            foreach (WorkflowViewElement view in parentViewElements)
            {
                parentModelItems.Add(view.ModelItem);
            }
            return parentModelItems;
        }

        //This is more efficient than walking up the VisualTree looking for WorkflowViewElements.
        //Assuming that Cut-Copy will always be against selected elements. 
        //This implies that only elements under the BreadCrumbRoot can be cut/copied.
        static List<WorkflowViewElement> GetSelectableParentViewElements(WorkflowViewElement childElement)
        {
            List<WorkflowViewElement> parentViewElements = new List<WorkflowViewElement>();
            if (childElement != null)
            {
                UIElement breadcrumbRoot = childElement.Context.Services.GetService<DesignerView>().RootDesigner;
                ICompositeView container = (ICompositeView)DragDropHelper.GetCompositeView(childElement);
                while (!childElement.Equals(breadcrumbRoot) && container != null)
                {
                    childElement = CutCopyPasteHelper.GetParentViewElement(container);
                    Fx.Assert(childElement != null, "container should be present in a WorkflowViewElement");
                    parentViewElements.Add(childElement);
                    container = (ICompositeView)DragDropHelper.GetCompositeView(childElement);
                }
            }
            return parentViewElements;
        }

        public static void DoPaste(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            DoPaste(context, new Point(-1, -1), null);
        }

        internal static void DoPaste(EditingContext context, Point pastePoint, WorkflowViewElement pastePointReference)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }

            ModelItem modelItem = context.Items.GetValue<Selection>().PrimarySelection;
            if (modelItem == null)
            {
                return;
            }

            //Get data from clipboard.
            List<object> metaData = null;
            List<object> clipboardObjects = GetFromClipboard(out metaData, context);
            if (clipboardObjects != null)
            {
                using (EditingScope es = (EditingScope)modelItem.BeginEdit(SR.PasteUndoDescription))
                {
                    if (clipboardObjects.Count == 3 && clipboardObjects[1] is Func<ModelItem, object, object>)
                    {
                        var factoryMethod = (Func<ModelItem, object, object>)clipboardObjects[1];
                        object result = factoryMethod(modelItem, clipboardObjects[2]);
                        clipboardObjects = new List<object>();
                        clipboardObjects.Add(result);
                    }
                    ICompositeView container = GetContainerForPaste(modelItem, pastePoint);
                    if (container != null)
                    {
                        container.OnItemsPasted(clipboardObjects, metaData, pastePoint, pastePointReference);
                    }
                    es.Complete();
                }
            }
        }

        static ICompositeView GetClickedContainer(ModelItem clickedModelItem, Point clickPoint)
        {
            Visual parentVisual = clickedModelItem.View as Visual;
            if (parentVisual == null)
            {
                return null;
            }

            DependencyObject visualHit = null;
            HitTestResult hitTest = VisualTreeHelper.HitTest(parentVisual, clickPoint);
            if (hitTest != null)
            {
                visualHit = hitTest.VisualHit;
                while (visualHit != null && !visualHit.Equals(parentVisual) &&
                    !typeof(ICompositeView).IsAssignableFrom(visualHit.GetType()))
                {
                    visualHit = VisualTreeHelper.GetParent(visualHit);
                }
            }
            return visualHit as ICompositeView;

        }

        static ICompositeView GetContainerForPaste(ModelItem pasteModelItem, Point clickPoint)
        {
            ICompositeView pasteContainer = null;

            if (null != pasteModelItem && null != pasteModelItem.View && pasteModelItem.View is WorkflowViewElement)
            {
                pasteContainer = ((WorkflowViewElement)pasteModelItem.View).ActiveCompositeView;
            }
            if (null == pasteContainer)
            {
                //Get clicked container.
                if (clickPoint.X > 0 && clickPoint.Y > 0)
                {
                    pasteContainer = GetClickedContainer(pasteModelItem, clickPoint);
                }

                //If the container itself is a WVE, there's posibility that it's collapsed.
                //Thus, we need to check this as well.
                if (pasteContainer != null && pasteContainer is WorkflowViewElement)
                {
                    WorkflowViewElement view = pasteContainer as WorkflowViewElement;
                    if (!view.ShowExpanded)
                    {
                        pasteContainer = null;
                    }
                }
                else if (pasteContainer == null) //If the modelitem.View itself is a container.
                {
                    WorkflowViewElement view = pasteModelItem.View as WorkflowViewElement;
                    if (view != null && view.ShowExpanded)
                    {
                        pasteContainer = pasteModelItem.View as ICompositeView;
                    }
                }

                //Get the container registered with modelItem.View if unambigous
                //If nothing works take the container with keyboard focus if one exists.
                if (pasteContainer == null)
                {
                    HashSet<ICompositeView> childrenContainers = CutCopyPasteHelper.GetChildContainers(pasteModelItem.View as WorkflowViewElement);
                    if ((childrenContainers == null || childrenContainers.Count == 0) && null != pasteModelItem.View)
                    {
                        pasteContainer = (ICompositeView)DragDropHelper.GetCompositeView((WorkflowViewElement)pasteModelItem.View);
                    }
                    else if (null != childrenContainers && childrenContainers.Count == 1)
                    {
                        pasteContainer = new List<ICompositeView>(childrenContainers)[0];
                    }
                    else
                    {
                        pasteContainer = Keyboard.FocusedElement as ICompositeView;
                    }

                }
            }
            return pasteContainer;
        }

        private static void PutOnClipBoard(List<object> selectedData, List<object> metaData, FrameworkName targetFramework)
        {
            CutCopyPasteHelper.workflowCallbackContext = null;
            if (selectedData != null)
            {
                ClipboardData clipboardData = new ClipboardData();
                clipboardData.Data = selectedData;
                clipboardData.Metadata = metaData;
                clipboardData.Version = versionInfo;

                XamlReader reader = ViewStateXamlHelper.RemoveIdRefs(new XamlObjectReader(clipboardData));
                StringWriter stringWriter = new StringWriter(CultureInfo.InvariantCulture);
                XamlServices.Transform(reader, new XamlXmlWriter(stringWriter, reader.SchemaContext), true);
                string clipBoardString = stringWriter.ToString();

                DataObject dataObject = new DataObject(WorkflowClipboardFormat, clipBoardString);
                dataObject.SetData(DataFormats.Text, clipBoardString);
                dataObject.SetData(WorkflowClipboardFormat_TargetFramework, targetFramework);
                RetriableClipboard.SetDataObject(dataObject, true);
            }
        }

        //PutCallbackOnClipBoard - tries to put into private (this application only) clipboard a callback 
        //to a method. The method will be invoked when user retrieves clipboard content - i.e. by
        //calling a paste command.
        //the callback has to be:
        //- static method
        //- have return value (not void)
        //- takes 2 input parameters:
        //   * 1 parameter is modelitem - this is a target modelitem upon which callback is to be executed
        //   * 2 parameter is user provided context - any object. Since this callback will be executed within
        //    this application only, there is no need for context to be serializable.
        internal static void PutCallbackOnClipBoard(Func<ModelItem, object, object> callbackMethod, Type callbackResultType, object context)
        {
            if (null == callbackMethod || null == context)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException(null == callbackMethod ? "callbackMethod" : "context"));
            }
            ClipboardData clipboardData = new ClipboardData();
            List<object> data = new List<object>();
            data.Add(callbackResultType);
            data.Add(callbackMethod);
            clipboardData.Data = data;
            clipboardData.Version = versionInfo;
            CutCopyPasteHelper.workflowCallbackContext = context;
            try
            {
                RetriableClipboard.SetDataObject(new DataObject(WorkflowCallbackClipboardFormat, clipboardData, false), false);
            }
            catch (ExternalException e)
            {
                ErrorReporting.ShowErrorMessage(e.Message);
            }
        }

        private static FrameworkName GetTargetFrameworkFromClipboard(DataObject dataObject)
        {
            Fx.Assert(dataObject != null, "dataObject should not be null");

            FrameworkName clipboardFrameworkName = null;
            if (dataObject.GetDataPresent(WorkflowClipboardFormat_TargetFramework))
            {
                clipboardFrameworkName = TryGetData(dataObject, WorkflowClipboardFormat_TargetFramework) as FrameworkName;
            }

            if (clipboardFrameworkName == null)
            {
                clipboardFrameworkName = FrameworkNameConstants.NetFramework40;
            }

            return clipboardFrameworkName;
        }

        //This method returns the list of objects put on clipboard by cut/copy. 
        //Out parameter is the metaData information.
        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Deserialization of cliboard data might fail. Propagating exceptions might lead to VS crash.")]
        [SuppressMessage("Reliability", "Reliability108",
            Justification = "Deserialization of cliboard data might fail. Propagating exceptions might lead to VS crash.")]
        private static List<object> GetFromClipboard(out List<object> metaData, EditingContext editingContext)
        {
            Fx.Assert(editingContext != null, "editingContext should not be null");

            MultiTargetingSupportService multiTargetingService = editingContext.Services.GetService<IMultiTargetingSupportService>() as MultiTargetingSupportService;
            DesignerConfigurationService config = editingContext.Services.GetService<DesignerConfigurationService>();
            DataObject dataObject = RetriableClipboard.GetDataObject() as DataObject;
            List<object> workflowData = null;
            metaData = null;

            if (dataObject != null)
            {
                if (dataObject.GetDataPresent(WorkflowClipboardFormat))
                {
                    bool isCopyingFromHigherFrameworkToLowerFramework = false;

                    if (multiTargetingService != null && config != null)
                    {
                        isCopyingFromHigherFrameworkToLowerFramework = GetTargetFrameworkFromClipboard(dataObject).Version > config.TargetFrameworkName.Version;
                    }

                    string clipBoardString = (string)TryGetData(dataObject, WorkflowClipboardFormat);
                    using (StringReader stringReader = new StringReader(clipBoardString))
                    {
                        try
                        {
                            XamlSchemaContext schemaContext;
                            if (isCopyingFromHigherFrameworkToLowerFramework)
                            {
                                schemaContext = new MultiTargetingXamlSchemaContext(multiTargetingService);
                            }
                            else
                            {
                                schemaContext = new XamlSchemaContext();
                            }

                            using (XamlXmlReader xamlXmlReader = new XamlXmlReader(stringReader, schemaContext))
                            {
                                ClipboardData clipboardData = (ClipboardData)XamlServices.Load(xamlXmlReader);
                                metaData = clipboardData.Metadata;
                                workflowData = clipboardData.Data;
                            }
                        }
                        catch (Exception e)
                        {
                            Trace.WriteLine(e.Message);
                        }
                    }
                }
                else if (dataObject.GetDataPresent(WorkflowCallbackClipboardFormat))
                {
                    ClipboardData localData = (ClipboardData)TryGetData(dataObject, WorkflowCallbackClipboardFormat);
                    metaData = null;
                    workflowData = localData.Data;
                    workflowData.Add(CutCopyPasteHelper.workflowCallbackContext);
                }
            }
            return workflowData;
        }

        private static object TryGetData(DataObject dataObject, string dataFormat)
        {
            try
            {
                return dataObject.GetData(dataFormat);
            }
            catch (OutOfMemoryException)
            {
                Trace.TraceError("OutOfMemoryException thrown from DataObject.");
            }
            return null;
        }

        private static bool CanCopy(Type type)
        {
            foreach (Type disallowedType in CutCopyPasteHelper.DisallowedTypesForCopy)
            {
                if (disallowedType.IsAssignableFrom(type))
                {
                    return false;
                }
                if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(disallowedType))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool CanCopy(ModelItem item)
        {
            return null != item.View && item.View is WorkflowViewElement &&
                null != ((WorkflowViewElement)item.View).ModelItem &&
                CanCopy(((WorkflowViewElement)item.View).ModelItem.ItemType);
        }

        public static bool CanCopy(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            Selection selection = context.Items.GetValue<Selection>();
            return selection.SelectionCount > 0 && selection.SelectedObjects.All(p => CanCopy(p));
        }

        public static bool CanCut(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            bool result = false;
            Selection selection = context.Items.GetValue<Selection>();
            if (null != selection && selection.SelectionCount > 0)
            {
                DesignerView designerView = context.Services.GetService<DesignerView>();
                result = selection.SelectedObjects.All(p =>
                    CanCopy(p) && !p.View.Equals(designerView.RootDesigner));
            }
            return result;
        }

        public static bool CanPaste(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("context"));
            }
            bool result = false;
            ModelItem primarySelection = context.Items.GetValue<Selection>().PrimarySelection;
            if (null != primarySelection)
            {
                ICompositeView container = GetContainerForPaste(primarySelection, new Point(-1, -1));
                if (null != container)
                {
                    DataObject dataObject = RetriableClipboard.GetDataObject() as DataObject;
                    if (null != dataObject)
                    {
                        List<object> metaData = null;
                        List<object> itemsToPaste = null;
                        try
                        {
                            if (dataObject.GetDataPresent(WorkflowClipboardFormat))
                            {
                                itemsToPaste = GetFromClipboard(out metaData, context);
                                result = container.CanPasteItems(itemsToPaste);
                            }
                            else if (dataObject.GetDataPresent(WorkflowCallbackClipboardFormat))
                            {
                                itemsToPaste = GetFromClipboard(out metaData, context);
                                result = container.CanPasteItems(itemsToPaste.GetRange(0, 1));
                            }
                        }
                        //This is being defensive for the case where user code for CanPasteITems throws a non-fatal exception.
                        catch (Exception exp)
                        {
                            if (Fx.IsFatal(exp))
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            return result;
        }
    }
}
