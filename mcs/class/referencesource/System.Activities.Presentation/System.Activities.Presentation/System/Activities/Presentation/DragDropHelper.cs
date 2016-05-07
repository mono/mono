//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation
{
    using System.Diagnostics;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Media;
    using System.Windows.Shapes;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View;
    using System.Windows.Threading;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Runtime;
    using System.Reflection;
    using System.Activities.Presentation.Sqm;
    using System.Collections.Generic;
    using System.Windows.Controls;
    using System.Linq;
    using Microsoft.Activities.Presentation;


    // This is a helper class for making dragdrop inside workflow designer easy. This abstracts out te encoding formats used in the
    // DataObject that is passed on from Drag source to target.
    public static class DragDropHelper
    {
        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty DragSourceProperty =
            DependencyProperty.RegisterAttached("DragSource", typeof(UIElement), typeof(DragDropHelper), new UIPropertyMetadata(null));
        public static readonly string ModelItemDataFormat;
        public static readonly string CompositeViewFormat;
        public static readonly string CompletedEffectsFormat = "DragCompletedEffectsFormat";
        public static readonly string WorkflowItemTypeNameFormat = "WorkflowItemTypeNameFormat";
        public static readonly string DragAnchorPointFormat;
        internal static readonly string ModelItemsDataFormat;
        internal static readonly string MovedViewElementsFormat;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static DragDropHelper()
        {
            string postfix = Guid.NewGuid().ToString();
            //set per process unique data format names - this will disable possibility of trying to drag & drop operation 
            //between designers in two different VS instances (use Cut-Copy-Paste for that)
            ModelItemDataFormat = string.Format(CultureInfo.InvariantCulture, "ModelItemFormat_{0}", postfix);
            CompositeViewFormat = string.Format(CultureInfo.InvariantCulture, "CompositeViewFormat_{0}", postfix);
            DragAnchorPointFormat = string.Format(CultureInfo.InvariantCulture, "DragAnchorFormat_{0}", postfix);
            ModelItemsDataFormat = string.Format(CultureInfo.InvariantCulture, "ModelItemsFormat_{0}", postfix);
            MovedViewElementsFormat = string.Format(CultureInfo.InvariantCulture, "MovedViewElementsFormat_{0}", postfix);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static void SetCompositeView(WorkflowViewElement workflowViewElement, UIElement dragSource)
        {
            if (workflowViewElement == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflowViewElement");
            }
            if (dragSource == null)
            {
                throw FxTrace.Exception.ArgumentNull("dragSource");
            }
            workflowViewElement.SetValue(DragDropHelper.DragSourceProperty, dragSource);
        }

        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        public static UIElement GetCompositeView(WorkflowViewElement workflowViewElement)
        {
            if (workflowViewElement == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflowViewElement");
            }
            return (UIElement)workflowViewElement.GetValue(DragDropHelper.DragSourceProperty);
        }

        internal static DataObject DoDragMoveImpl(IEnumerable<WorkflowViewElement> draggedViewElements, Point referencePoint)
        {
            List<ModelItem> draggedModelItems = new List<ModelItem>();
            bool first = true;
            WorkflowViewElement viewElement = null;
            foreach (WorkflowViewElement view in draggedViewElements)
            {
                if (view != null)
                {
                    if (first)
                    {
                        viewElement = view;
                        first = false;
                    }
                    draggedModelItems.Add(view.ModelItem);
                    view.IsHitTestVisible = false;
                }
            }
            DataObject dataObject = new DataObject(ModelItemsDataFormat, draggedModelItems);

            // For compatiblity
            if (viewElement != null)
            {
                dataObject.SetData(ModelItemDataFormat, viewElement.ModelItem);
                dataObject.SetData(CompositeViewFormat, GetCompositeView(viewElement));
            }

            dataObject.SetData(DragAnchorPointFormat, referencePoint);

            if (viewElement != null)
            {
                DesignerView designerView = viewElement.Context.Services.GetService<DesignerView>();
                ViewElementDragShadow dragShadow = new ViewElementDragShadow(designerView.scrollableContent, draggedViewElements, referencePoint, designerView.ZoomFactor);
                designerView.BeginDragShadowTracking(dragShadow);
                //whenever drag drop fails - ensure getting rid of drag shadow
                try
                {
                    DragDrop.DoDragDrop(designerView, dataObject, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll | DragDropEffects.Link);
                }
                catch
                {
                    //let the caller handle exception
                    throw;
                }
                finally
                {
                    designerView.EndDragShadowTracking(dragShadow);
                    foreach (WorkflowViewElement view in draggedViewElements)
                    {
                        if (view != null)
                        {
                            view.IsHitTestVisible = true;
                        }
                    }
                }
            }
            return dataObject;
        }

        [Obsolete("This method does not support dragging multiple items. Use \"public static IEnumerable<WorkflowViewElement> DoDragMove(IEnumerable<WorkflowViewElement> draggedViewElements, Point referencePoint)\" instead.")]
        public static DragDropEffects DoDragMove(WorkflowViewElement draggedViewElement, Point referencePoint)
        {
            if (draggedViewElement == null)
            {
                throw FxTrace.Exception.ArgumentNull("draggedViewElement");
            }
            if (referencePoint == null)
            {
                throw FxTrace.Exception.ArgumentNull("referencePoint");
            }
            ModelItem draggedActivityModelItem = draggedViewElement.ModelItem;
            DataObject dataObject = new DataObject(ModelItemDataFormat, draggedActivityModelItem);
            dataObject.SetData(CompositeViewFormat, GetCompositeView(draggedViewElement));
            dataObject.SetData(DragAnchorPointFormat, referencePoint);
            List<ModelItem> draggedModelItems = new List<ModelItem>();
            draggedModelItems.Add(draggedActivityModelItem);
            dataObject.SetData(ModelItemsDataFormat, draggedModelItems);

            DesignerView view = draggedViewElement.Context.Services.GetService<DesignerView>();
            ViewElementDragShadow dragShadow = new ViewElementDragShadow(view.scrollableContent, draggedViewElement, referencePoint, view.ZoomFactor);

            draggedViewElement.IsHitTestVisible = false;
            view.BeginDragShadowTracking(dragShadow);

            //whenever drag drop fails - ensure getting rid of drag shadow
            try
            {
                DragDrop.DoDragDrop(GetCompositeView(draggedViewElement), dataObject, DragDropEffects.Move | DragDropEffects.Copy | DragDropEffects.Scroll | DragDropEffects.Link);
            }
            catch
            {
                //let the caller handle exception
                throw;
            }
            finally
            {
                view.EndDragShadowTracking(dragShadow);
                draggedViewElement.IsHitTestVisible = true;
            }

            return GetDragDropCompletedEffects(dataObject);
        }

        public static bool AllowDrop(IDataObject draggedDataObject, EditingContext context, params Type[] allowedItemTypes)
        {

            if (draggedDataObject == null)
            {
                throw FxTrace.Exception.ArgumentNull("draggedDataObject");
            }
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }
            if (allowedItemTypes == null)
            {
                throw FxTrace.Exception.ArgumentNull("allowedItemTypes");
            }
            ReadOnlyState readOnlyState = context.Items.GetValue<ReadOnlyState>();
            if (readOnlyState != null && readOnlyState.IsReadOnly)
            {
                return false;
            }
            if (!AllowDrop(draggedDataObject, context))
            {
                return false;
            }
            List<Type> draggedTypes = GetDraggedTypes(draggedDataObject);
            return draggedTypes != null
                && draggedTypes.Count != 0
                && draggedTypes.All<Type>((p) =>
                {
                    for (int i = 0; i < allowedItemTypes.Length; ++i)
                    {
                        if (allowedItemTypes[i] == null)
                        {
                            throw FxTrace.Exception.ArgumentNull(string.Format(CultureInfo.InvariantCulture, "allowedItemTypes[{0}]", i));
                        }
                        if (AllowDrop(p, allowedItemTypes[i]))
                        {
                            return true;
                        }
                    }
                    return false;
                });
        }

        static bool AllowDrop(IDataObject draggedDataObject, EditingContext context)
        {
            ModelItem droppedModelItem = draggedDataObject.GetData(ModelItemDataFormat) as ModelItem;
            if (droppedModelItem == null)
            {
                return true;
            }
            return ((IModelTreeItem)droppedModelItem).ModelTreeManager.Context.Equals(context);
        }

        internal static bool AllowDrop(Type draggedType, Type allowedItemType)
        {
            if (draggedType == null)
            {
                // This is the case where some external stuff (e.g. Recycle bin) get dragged over.
                return false;
            }
            // This is a special case in GetDroppedObject() and replicated here.
            // Check whether dragged type is IActivityTemplateFactory, if true, use Factory's implement type instead.
            Type factoryType;
            if (draggedType.TryGetActivityTemplateFactory(out factoryType))
            {
                draggedType = factoryType;
            }

            if (allowedItemType.IsAssignableFrom(draggedType))
            {
                return true;
            }
            else if (allowedItemType.IsGenericTypeDefinition && draggedType.IsGenericType)
            {
                // We don't have inheritance relationship for GenericTypeDefinition, therefore the right check is equality
                return allowedItemType.Equals(draggedType.GetGenericTypeDefinition());
            }
            else if (allowedItemType.IsGenericType && draggedType.IsGenericTypeDefinition)
            {
                // Allow GenericTypeDefinition to be dropped with GenericType constraint, if user select a correct argument type, drop should work.
                return draggedType.Equals(allowedItemType.GetGenericTypeDefinition());
            }
            else if (allowedItemType.IsGenericType && draggedType.IsGenericType && draggedType.ContainsGenericParameters)
            {
                // If the draggedType is generic type but it contains generic parameters, which may happen to match the constraint.
                return allowedItemType.GetGenericTypeDefinition() == draggedType.GetGenericTypeDefinition();
            }
            else
            {
                return false;
            }
        }

        internal static List<Type> GetDraggedTypes(IDataObject draggedDataObject)
        {
            List<Type> types = new List<Type>();
            if (draggedDataObject != null)
            {
                if (draggedDataObject.GetDataPresent(ModelItemsDataFormat))
                {
                    IEnumerable<ModelItem> modelItems = draggedDataObject.GetData(ModelItemsDataFormat) as IEnumerable<ModelItem>;
                    foreach (ModelItem modelItem in modelItems)
                    {
                        if (modelItem != null)
                        {
                            types.Add(modelItem.ItemType);
                        }
                    }
                }
                else if (draggedDataObject.GetDataPresent(ModelItemDataFormat))
                {
                    ModelItem modelItem = draggedDataObject.GetData(ModelItemDataFormat) as ModelItem;
                    if (modelItem != null)
                    {
                        types.Add(modelItem.ItemType);
                    }
                }

                // This is an object dragged from somewhere else other than from within the designer surface
                if (draggedDataObject.GetDataPresent(WorkflowItemTypeNameFormat))
                {
                    // This is the case where the object is dropped from the toolbox
                    string text = draggedDataObject.GetData(WorkflowItemTypeNameFormat) as string;
                    if (!string.IsNullOrEmpty(text))
                    {
                        types.Add(Type.GetType(text));
                    }
                }
            }

            return types;
        }

        internal static bool IsDraggingFromToolbox(DragEventArgs e)
        {
            return e.Data.GetDataPresent(WorkflowItemTypeNameFormat);
        }

        public static IEnumerable<object> GetDroppedObjects(DependencyObject dropTarget, DragEventArgs e, EditingContext context)
        {
            List<object> droppedObjects = new List<object>();
            if (e.Data.GetDataPresent(ModelItemsDataFormat))
            {
                IEnumerable<ModelItem> droppedModelItems = e.Data.GetData(ModelItemsDataFormat) as IEnumerable<ModelItem>;
                foreach (ModelItem modelItem in droppedModelItems)
                {
                    droppedObjects.Add(modelItem);
                }
            }
            else
            {
                object droppedObject = e.Data.GetData(ModelItemDataFormat) as ModelItem;
                // could have been dropped from toolbox.
                if (droppedObject == null)
                {
                    Type type = null;
                    if (e.Data.GetDataPresent(WorkflowItemTypeNameFormat))
                    {
                        string text = e.Data.GetData(WorkflowItemTypeNameFormat) as string;
                        if (!string.IsNullOrEmpty(text))
                        {
                            //try to use the text format to see if it holds a type name and  try to create an object out of it.
                            type = Type.GetType(text);
                        }
                    }
                    droppedObject = GetDroppedObjectInstance(dropTarget, context, type, e.Data);
                }
                if (droppedObject != null)
                {
                    droppedObjects.Add(droppedObject);
                }
            }
            e.Handled = true;
            context.Services.GetService<DesignerPerfEventProvider>().WorkflowDesignerDrop();

            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle,
                    new Action(() =>
                    {
                        context.Services.GetService<DesignerPerfEventProvider>().WorkflowDesignerIdleAfterDrop();

                    }));
            return droppedObjects;
        }

        internal static void ValidateItemsAreOnView(IList<ModelItem> items, ICollection<ModelItem> modelItemsOnView)
        {
            Fx.Assert(items != null, "items");
            Fx.Assert(modelItemsOnView != null, "modelItemsOnView");

            for (int index = 0; index < items.Count; ++index)
            {
                if (!modelItemsOnView.Contains(items[index]))
                {
                    throw FxTrace.Exception.AsError(
                        new ArgumentException(string.Format(CultureInfo.CurrentCulture, SR.Error_ItemNotOnView, index)));
                }
            }
        }

        [Obsolete("This method does not support dropping multiple items. Use \"public static IEnumerable<object> GetDroppedObjects(DependencyObject dropTarget, DragEventArgs e, EditingContext context)\" instead.")]
        public static object GetDroppedObject(DependencyObject dropTarget, DragEventArgs e, EditingContext context)
        {
            IEnumerable<object> droppedObjects = GetDroppedObjects(dropTarget, e, context);
            if (droppedObjects.Count() > 0)
            {
                return droppedObjects.First();
            }
            return null;
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes,
            Justification = "Any exception can be thrown from custom code. We don't want to crash VS.")]
        [SuppressMessage("Reliability", "Reliability108",
            Justification = "Any exception can be thrown from custom code. We don't want to crash VS.")]
        internal static object GetDroppedObjectInstance(DependencyObject dropTarget, EditingContext context, Type type, IDataObject dataObject)
        {
            if (type != null)
            {
                //check if type is generic
                if (type.IsGenericTypeDefinition)
                {
                    type = ResolveGenericParameters(dropTarget, context, type);
                }
            }

            object droppedObject = null;
            if (null != type)
            {
                try
                {
                    droppedObject = Activator.CreateInstance(type);

                    if (type.IsActivityTemplateFactory() && type.IsClass)
                    {
                        //find parent WorkflowViewElement - in case of mouse drop, current drop target most likely is ISourceContainer
                        if (!(dropTarget is WorkflowViewElement))
                        {
                            dropTarget = VisualTreeUtils.FindVisualAncestor<WorkflowViewElement>(dropTarget);
                        }

                        Type templateFactoryInterface2 = type.GetInterface(typeof(IActivityTemplateFactory<>).FullName);
                        if (templateFactoryInterface2 != null)
                        {
                            droppedObject = templateFactoryInterface2.InvokeMember("Create", BindingFlags.InvokeMethod, null, droppedObject, new object[] { dropTarget, dataObject }, CultureInfo.InvariantCulture);
                        }
                        else if (droppedObject is IActivityTemplateFactory)
                        {
                            droppedObject = ((IActivityTemplateFactory)droppedObject).Create(dropTarget);
                        }

                    }

                    // SQM: Log activity usage count
                    ActivityUsageCounter.ReportUsage(context.Services.GetService<IVSSqmService>(), type);
                }
                catch (Exception ex)
                {
                    if (Fx.IsFatal(ex))
                    {
                        throw;
                    }

                    string details = ex.Message;
                    if (ex is TargetInvocationException && ex.InnerException != null)
                    {
                        details = ex.InnerException.Message;
                    }


                    ErrorReporting.ShowErrorMessage(string.Format(CultureInfo.CurrentUICulture, SR.CannotCreateInstance, TypeNameHelper.GetDisplayName(type, false)), details);
                }
            }

            return droppedObject;
        }

        static Type ResolveGenericParameters(DependencyObject dropTarget, EditingContext context, Type type)
        {
            // look to see if there is a DefaultTypeArgumentAttribute on it 
            DefaultTypeArgumentAttribute typeArgumentAttribute = ExtensibilityAccessor.GetAttribute<DefaultTypeArgumentAttribute>(type);
            if (typeArgumentAttribute != null && typeArgumentAttribute.Type != null)
            {
                type = type.MakeGenericType(typeArgumentAttribute.Type);
            }
            else //require user to resolve generic arguments
            {
                ActivityTypeResolver wnd = new ActivityTypeResolver();
                if (null != context)
                {
                    WindowHelperService service = context.Services.GetService<WindowHelperService>();
                    if (null != service)
                    {
                        service.TrySetWindowOwner(dropTarget, wnd);
                    }
                }

                TypeResolvingOptions dropTargetOptions = null;
                TypeResolvingOptions activityTypeOptions = null;

                //try to see if the container has any customization for type resolver
                ICompositeView container = dropTarget as ICompositeView;
                if (container != null)
                {
                    dropTargetOptions = container.DroppingTypeResolvingOptions;
                }

                //try to see if the activity type in discourse has any customization for type resolver
                TypeResolvingOptionsAttribute attr = WorkflowViewService.GetAttribute<TypeResolvingOptionsAttribute>(type);
                if (attr != null)
                {
                    activityTypeOptions = attr.TypeResolvingOptions;
                }
                //if both have type resolver, try to merge them
                TypeResolvingOptions options = TypeResolvingOptions.Merge(dropTargetOptions, activityTypeOptions);
                if (options != null)
                {
                    wnd.Options = options;
                }

                wnd.Context = context;
                wnd.EditedType = type;
                wnd.Width = 340;
                wnd.Height = 200;
                type = (true == wnd.ShowDialog() ? wnd.ConcreteType : null);
            }
            return type;
        }

        [Obsolete("This method does not support dragging multiple items. Use \"public static IEnumerable<ModelItem> GetDraggedModelItems(DragEventArgs e)\" instead.")]
        public static ModelItem GetDraggedModelItem(DragEventArgs e)
        {
            return GetDraggedModelItemInternal(e);
        }

        internal static ModelItem GetDraggedModelItemInternal(DragEventArgs e)
        {
            IEnumerable<ModelItem> draggedModelItems = GetDraggedModelItems(e);
            if (draggedModelItems.Count() > 0)
            {
                return draggedModelItems.First();
            }
            return null;
        }

        public static IEnumerable<ModelItem> GetDraggedModelItems(DragEventArgs e)
        {
            IEnumerable<ModelItem> draggedModelItems = e.Data.GetData(ModelItemsDataFormat) as IEnumerable<ModelItem>;
            if (draggedModelItems != null)
            {
                return draggedModelItems;
            }
            else
            {
                ModelItem draggedItem = e.Data.GetData(ModelItemDataFormat) as ModelItem;
                if (draggedItem != null)
                {
                    return new ModelItem[] { draggedItem };
                }
            }
            return new ModelItem[] { };
        }

        internal static bool AreListsIdenticalExceptOrder<T>(IList<T> sourceList, IList<T> destinationList)
        {
            // User does not 
            // 1) introduce unseen object into the collection.
            // 2) remove object from the collection.
            // 3) introduce null in the collection.
            // 4) return null
            if (sourceList == null)
            {
                return destinationList == null;
            }

            if (destinationList == null)
            {
                return false;
            }

            if (sourceList.Count != destinationList.Count)
            {
                return false;
            }
            HashSet<T> checkingMap = new HashSet<T>();

            // create set
            foreach (T item in sourceList)
            {
                bool ret = checkingMap.Add(item);
                // an internal error, the item in src should be identical.
                Fx.Assert(ret, "item in source list is not identical?");
            }

            foreach (T item in destinationList)
            {
                if (!checkingMap.Remove(item))
                {
                    return false;
                }
            }
            return checkingMap.Count == 0;
        }

        // 1) obj with CompositeView2: sort by IMultipleDragEnabledCompositeView SortSelectedItems.
        // 2) obj with CompoisteView: no sort.
        // 3) obj without CompositeView: just put them at the end of the list as the order in selectedObjects.
        internal static List<object> SortSelectedObjects(IEnumerable<object> selectedObjects)
        {
            //1) Separate objects
            Dictionary<ICompositeView, List<ModelItem>> viewItemListDictionary = new Dictionary<ICompositeView, List<ModelItem>>();
            List<object> nonCompositeView = new List<object>();
            List<object> retList = new List<object>();
            foreach (object obj in selectedObjects)
            {
                ModelItem modelItem = obj as ModelItem;
                if (modelItem == null || modelItem.View == null)
                {
                    nonCompositeView.Add(obj);
                    continue;
                }
                ICompositeView container = DragDropHelper
                    .GetCompositeView(modelItem.View as WorkflowViewElement) as ICompositeView;
                if (container == null)
                {
                    nonCompositeView.Add(obj);
                    continue;
                }

                // add to dictionary.
                if (!viewItemListDictionary.ContainsKey(container))
                {
                    viewItemListDictionary.Add(container, new List<ModelItem>());
                }

                viewItemListDictionary[container].Add(modelItem);
            }

            // 2) sort when possible
            foreach (KeyValuePair<ICompositeView, List<ModelItem>> pair in viewItemListDictionary)
            {
                IMultipleDragEnabledCompositeView view2 = pair.Key as IMultipleDragEnabledCompositeView;
                List<ModelItem> sortedList = view2 == null ?
                    pair.Value : view2.SortSelectedItems(new List<ModelItem>(pair.Value));
                if (!AreListsIdenticalExceptOrder(pair.Value, sortedList))
                {
                    // check consistens.
                    throw FxTrace.Exception.AsError(
                        new InvalidOperationException(SR.Error_BadOutputFromSortSelectedItems));
                }
                retList.AddRange(sortedList);
            }

            retList.AddRange(nonCompositeView);
            return retList;
        }

        [Obsolete("This method does not support dragging multiple items. Use \"public static UIElement GetCompositeView(WorkflowViewElement workflowViewElement)\" instead.")]
        public static ICompositeView GetCompositeView(DragEventArgs e)
        {
            return (ICompositeView)e.Data.GetData(CompositeViewFormat);
        }

        public static Point GetDragDropAnchorPoint(DragEventArgs e)
        {
            Point referencePoint;
            if (e.Data.GetDataPresent(DragAnchorPointFormat))
            {
                referencePoint = (Point)e.Data.GetData(DragAnchorPointFormat);
            }
            else
            {
                referencePoint = new Point(-1, -1);
            }
            return referencePoint;
        }

        [Obsolete("This method does not support dragging multiple items. Consider using \"public static void SetDragDropMovedViewElements(DragEventArgs e, IEnumerable<WorkflowViewElement> movedViewElements)\" instead.")]
        public static void SetDragDropCompletedEffects(DragEventArgs e, DragDropEffects completedEffects)
        {
            try
            {
                e.Data.SetData(CompletedEffectsFormat, completedEffects);
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception.ToString());
            }
        }

        [Obsolete("This method does not support dragging multiple items. Consider using \"public static IEnumerable<WorkflowViewElement> GetDragDropMovedViewElements(DataObject data)\" instead.")]
        public static DragDropEffects GetDragDropCompletedEffects(DataObject data)
        {
            if (data == null)
            {
                throw FxTrace.Exception.ArgumentNull("data");
            }
            DragDropEffects completedEffects = DragDropEffects.None;
            if (data.GetDataPresent(CompletedEffectsFormat))
            {
                completedEffects = (DragDropEffects)data.GetData(CompletedEffectsFormat);
            }
            return completedEffects;
        }

        internal static void SetDragDropMovedViewElements(DragEventArgs e, IEnumerable<WorkflowViewElement> movedViewElements)
        {
            if (e == null)
            {
                throw FxTrace.Exception.ArgumentNull("e");
            }

            if (movedViewElements == null)
            {
                throw FxTrace.Exception.ArgumentNull("movedViewElements");
            }

            try
            {
                e.Data.SetData(MovedViewElementsFormat, movedViewElements);
            }
            catch (InvalidOperationException exception)
            {
                Trace.WriteLine(exception.ToString());
            }
        }

        internal static IEnumerable<WorkflowViewElement> GetDragDropMovedViewElements(DataObject data)
        {
            if (data == null)
            {
                throw FxTrace.Exception.ArgumentNull("data");
            }

            if (data.GetDataPresent(MovedViewElementsFormat))
            {
                return (IEnumerable<WorkflowViewElement>)data.GetData(MovedViewElementsFormat);
            }
            return null;
        }

        internal static int GetDraggedObjectCount(DragEventArgs e)
        {
            return GetDraggedTypes(e.Data).Count;
        }

        internal static Dictionary<WorkflowViewElement, Point> GetViewElementRelativeLocations(IEnumerable<WorkflowViewElement> viewElements)
        {
            DesignerView designerView = null;
            Dictionary<WorkflowViewElement, Point> locations = new Dictionary<WorkflowViewElement, Point>();
            Point topLeftPoint = new Point(double.PositiveInfinity, double.PositiveInfinity);
            foreach (WorkflowViewElement viewElement in viewElements)
            {
                if (designerView == null)
                {
                    designerView = viewElement.Context.Services.GetService<DesignerView>();
                }

                Point location = new Point(0, 0);
                if (designerView.scrollableContent.IsAncestorOf(viewElement))
                {
                    GeneralTransform transform = viewElement.TransformToAncestor(designerView.scrollableContent);
                    location = transform.Transform(new Point(0, 0));
                }

                if (location.X < topLeftPoint.X)
                {
                    topLeftPoint.X = location.X;
                }

                if (location.Y < topLeftPoint.Y)
                {
                    topLeftPoint.Y = location.Y;
                }

                locations.Add(viewElement, location);
            }

            foreach (WorkflowViewElement viewElement in viewElements)
            {
                locations[viewElement] = Vector.Add(new Vector(-topLeftPoint.X, -topLeftPoint.Y), locations[viewElement]);
            }
            return locations;
        }

        internal static Dictionary<WorkflowViewElement, Point> GetDraggedViewElementRelativeLocations(DragEventArgs e)
        {
            List<WorkflowViewElement> draggedViewElements = new List<WorkflowViewElement>();
            if (e.Data.GetDataPresent(ModelItemsDataFormat))
            {
                IEnumerable<ModelItem> draggedModelItems = e.Data.GetData(ModelItemsDataFormat) as IEnumerable<ModelItem>;
                if (draggedModelItems != null)
                {
                    foreach (ModelItem draggedModelItem in draggedModelItems)
                    {
                        if (draggedModelItem != null && draggedModelItem.View != null)
                        {
                            draggedViewElements.Add((WorkflowViewElement)draggedModelItem.View);
                        }
                    }
                }
            }
            else if (e.Data.GetDataPresent(ModelItemDataFormat))
            {
                ModelItem draggedModelItem = e.Data.GetData(ModelItemDataFormat) as ModelItem;
                if (draggedModelItem != null && draggedModelItem.View != null)
                {
                    draggedViewElements.Add((WorkflowViewElement)draggedModelItem.View);
                }
            }
            return GetViewElementRelativeLocations(draggedViewElements);
        }

        // Get rid of descendant model items when both ancestor and descendant and ancestor model items are selected
        internal static IEnumerable<ModelItem> GetModelItemsToDrag(IEnumerable<ModelItem> modelItems)
        {
            HashSet<ModelItem> modelItemsToDrag = new HashSet<ModelItem>();
            foreach (ModelItem modelItem in modelItems)
            {
                HashSet<ModelItem> parentModelItems = CutCopyPasteHelper.GetSelectableParentModelItems(modelItem);
                parentModelItems.IntersectWith(modelItems);
                if (parentModelItems.Count == 0)
                {
                    modelItemsToDrag.Add(modelItem);
                }
            }
            return modelItemsToDrag;
        }


        internal sealed class ViewElementDragShadow : Adorner
        {
            Rectangle content;
            double x;
            double y;
            double offsetX;
            double offsetY;
            double scaleFactor;
            double width;
            double height;
            AdornerLayer layer;

            public ViewElementDragShadow(UIElement owner, WorkflowViewElement viewElement, Point offset, double scaleFactor)
                : base(owner)
            {
                Rect bounds = VisualTreeHelper.GetDescendantBounds(viewElement);
                this.width = bounds.Width;
                this.height = bounds.Height;

                this.content = new Rectangle()
                {
                    Width = this.width,
                    Height = this.height,
                    Fill = new VisualBrush(viewElement)
                    {
                        Opacity = 0.6
                    }
                };
                this.InitializeCommon(offset, scaleFactor);
            }

            public ViewElementDragShadow(UIElement owner, IEnumerable<WorkflowViewElement> viewElements, Point offset, double scaleFactor)
                : base(owner)
            {
                Dictionary<WorkflowViewElement, Point> locations = DragDropHelper.GetViewElementRelativeLocations(viewElements);

                Grid grid = new Grid();
                foreach (WorkflowViewElement viewElement in viewElements)
                {
                    Rect bounds = VisualTreeHelper.GetDescendantBounds(viewElement);
                    Rectangle rectangle = new Rectangle()
                    {
                        Width = bounds.Width,
                        Height = bounds.Height,
                        Fill = new VisualBrush(viewElement),
                        Margin = new Thickness(locations[viewElement].X, locations[viewElement].Y, 0, 0),
                        VerticalAlignment = VerticalAlignment.Top,
                        HorizontalAlignment = HorizontalAlignment.Left
                    };
                    grid.Children.Add(rectangle);
                }
                grid.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                this.width = grid.DesiredSize.Width;
                this.height = grid.DesiredSize.Height;

                this.content = new Rectangle()
                {
                    Width = this.width,
                    Height = this.height,
                    Fill = new VisualBrush(grid)
                    {
                        Opacity = 0.6
                    }
                };
                this.InitializeCommon(offset, scaleFactor);
            }

            internal void UpdatePosition(double x, double y)
            {
                if (this.Visibility == Visibility.Hidden)
                {
                    this.Visibility = Visibility.Visible;
                }
                double oldX = this.x;
                double oldY = this.y;
                this.x = x - this.offsetX;
                this.y = y - this.offsetY;
                if (oldX != this.x || oldY != this.y)
                {
                    this.layer = this.Parent as AdornerLayer;
                    this.layer.Update(this.AdornedElement);
                }
            }

            public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
            {
                GeneralTransformGroup result = new GeneralTransformGroup();
                result.Children.Add(new TranslateTransform(this.x, this.y));
                result.Children.Add(new ScaleTransform(this.scaleFactor, this.scaleFactor, this.x, this.y));
                return result;
            }

            protected override Visual GetVisualChild(int index)
            {
                return this.content;
            }

            protected override int VisualChildrenCount
            {
                get { return 1; }
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                this.content.Arrange(new Rect(this.content.DesiredSize));
                System.Diagnostics.Debug.WriteLine("DragShadow.ArrangeOverride " + this.content.DesiredSize);
                return this.content.DesiredSize;
            }

            protected override Size MeasureOverride(Size constraint)
            {
                this.content.Measure(constraint);
                System.Diagnostics.Debug.WriteLine("DragShadow.MeasureOverride " + this.content.DesiredSize);
                return this.content.DesiredSize;
            }

            private void InitializeCommon(Point offset, double scaleFactor)
            {
                this.offsetX = offset.X * scaleFactor;
                this.offsetY = offset.Y * scaleFactor;
                this.Visibility = Visibility.Hidden;
                this.scaleFactor = scaleFactor;
            }
        }
    }
}
