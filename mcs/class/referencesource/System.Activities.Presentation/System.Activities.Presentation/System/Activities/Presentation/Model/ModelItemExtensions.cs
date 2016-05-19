//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Model
{
    using System;
    using System.Activities.Debugger;
    using System.Activities.Presentation.Annotations;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.View;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Text;
    using System.Windows;
    using System.Windows.Input;
    using System.Windows.Threading;

    public static class ModelItemExtensions
    {
        const int MaxExpandLevel = 50;
        const string rootPath = "Root";

        public static EditingContext GetEditingContext(this ModelItem modelItem)
        {
            EditingContext result = null;
            IModelTreeItem modelTreeItem = modelItem as IModelTreeItem;
            if (null != modelTreeItem && null != modelTreeItem.ModelTreeManager)
            {
                result = modelTreeItem.ModelTreeManager.Context;
            }
            return result;
        }

        internal static ModelItem FindParentModelItem(this ModelItem item, Type parentType)
        {
            if (null == item)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            if (null == parentType)
            {
                throw FxTrace.Exception.ArgumentNull("parentType");
            }

            ModelItem result = null;
            item = item.Parent;
            while (item != null && !parentType.IsAssignableFrom(item.ItemType))
            {
                item = item.Parent;
            }
            if (null != item && parentType.IsAssignableFrom(item.ItemType))
            {
                result = item;
            }
            return result;
        }

        internal static bool SwitchKeys(this ModelItemDictionary dictionary, ModelItem oldKey, ModelItem newKey)
        {
            if (null == dictionary)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("dictionary"));
            }
            if (null == oldKey)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("oldKey"));
            }
            if (null == newKey)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("newKey"));
            }
            if (!dictionary.ContainsKey(oldKey))
            {
                throw FxTrace.Exception.AsError(new KeyNotFoundException(null == oldKey.GetCurrentValue() ? "oldKey" : oldKey.GetCurrentValue().ToString()));
            }
            bool result = false;
            if (!dictionary.ContainsKey(newKey))
            {
                ModelItem value = dictionary[oldKey];
                dictionary.Remove(oldKey);
                dictionary[newKey] = value;
                result = true;
            }
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
           Justification = "This is a TryGet pattern that requires out parameters")]
        internal static bool SwitchKeys(this ModelItemDictionary dictionary, object oldKey, object newKey, out ModelItem newKeyItem)
        {
            if (null == dictionary)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("dictionary"));
            }
            if (null == oldKey)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("oldKey"));
            }
            if (null == newKey)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("newKey"));
            }
            if (!dictionary.ContainsKey(oldKey))
            {
                throw FxTrace.Exception.AsError(new KeyNotFoundException(oldKey.ToString()));
            }
            bool result = false;
            newKeyItem = null;
            if (typeof(ModelItem).IsAssignableFrom(oldKey.GetType()) && typeof(ModelItem).IsAssignableFrom(newKey.GetType()))
            {
                result = SwitchKeys(dictionary, (ModelItem)oldKey, (ModelItem)newKey);
                newKeyItem = (ModelItem)newKey;
            }
            else
            {
                if (typeof(ModelItem).IsAssignableFrom(oldKey.GetType()))
                {
                    oldKey = ((ModelItem)oldKey).GetCurrentValue();
                    if (null == oldKey)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException("((ModelItem)oldKey).GetCurrentValue()"));
                    }
                }
                if (typeof(ModelItem).IsAssignableFrom(newKey.GetType()))
                {
                    newKey = ((ModelItem)newKey).GetCurrentValue();
                    if (null == newKey)
                    {
                        throw FxTrace.Exception.AsError(new InvalidOperationException("((ModelItem)newKey).GetCurrentValue()"));
                    }
                }
            }
            if (!dictionary.ContainsKey(newKey))
            {
                ModelItem value = dictionary[oldKey];
                dictionary.Remove(oldKey);
                dictionary[newKey] = value;
                newKeyItem = dictionary.Keys.First<ModelItem>(p => object.Equals(p.GetCurrentValue(), newKey));
                result = true;
            }
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
           Justification = "This is a TryGet pattern that requires out parameters")]
        internal static bool TryGetPropertyValue(this ModelItem item, out ModelItemCollection value, params string[] path)
        {
            ModelItem temp;
            value = null;
            bool result = TryGetPropertyValue(item, out temp, path);
            if (null != item)
            {
                value = (ModelItemCollection)temp;
            }
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
           Justification = "This is a TryGet pattern that requires out parameters")]
        internal static bool TryGetPropertyValue(this ModelItem item, out ModelItemDictionary value, params string[] path)
        {
            ModelItem temp;
            value = null;
            bool result = TryGetPropertyValue(item, out temp, path);
            if (null != item)
            {
                value = (ModelItemDictionary)temp;
            }
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
           Justification = "This is a TryGet pattern that requires out parameters")]
        internal static bool TryGetPropertyValue(this ModelItem item, out ModelItem value, params string[] path)
        {
            if (null == item)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }
            if (null == path)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("path"));
            }
            if (path.Length < 1)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.ModelItemPathArrayShouldNotBeEmpty));
            }
            value = item;
            bool result = true;
            for (int i = 0; i < path.Length && true == result && null != value; ++i)
            {
                ModelProperty property = value.Properties[path[i]];
                if (null != property)
                {
                    value = property.Value;
                    if (null == value)
                    {
                        result = false;
                    }
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.PropertyDoesntExistFormatString, path[i])));
                }
            }
            return result;
        }

        [SuppressMessage("Microsoft.Design", "CA1021:AvoidOutParameters",
           Justification = "This is a TryGet pattern that requires out parameters")]
        internal static bool TrySetPropertyValue(this ModelItem item, object value, out ModelItem wrappedValue, params string[] path)
        {
            if (null == item)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }
            if (null == path)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("path"));
            }
            if (path.Length < 1)
            {
                throw FxTrace.Exception.AsError(new ArgumentException(SR.ModelItemPathArrayShouldNotBeEmpty));
            }
            wrappedValue = null;
            bool result = true;
            for (int i = 0; i < path.Length && true == result; ++i)
            {
                ModelProperty property = item.Properties[path[i]];
                if (null != property)
                {
                    if (i == path.Length - 1)
                    {
                        if (null != value)
                        {
                            wrappedValue = property.SetValue(value);
                        }
                        else
                        {
                            property.ClearValue();
                        }
                    }
                    else
                    {
                        item = property.Value;
                        if (null == item)
                        {
                            result = false;
                        }
                    }
                }
                else
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, SR.PropertyDoesntExistFormatString, path[i])));
                }
            }
            return result;
        }

        internal static bool HasAnnotation(this ModelItem modelItem)
        {
            Fx.Assert(modelItem != null, "modelItem should not be null.");

            ModelProperty property = modelItem.Properties.Find(Annotation.AnnotationTextPropertyName);

            Fx.Assert(property != null, "Annotation property should not be null");

            if (property.ComputedValue == null)
            {
                return false;
            }

            return true;
        }

        public static string GetModelPath(this ModelItem modelItem)
        {
            if (modelItem == null)
            {
                return null;
            }
            StringBuilder sb = new StringBuilder();
            bool isValid = true;
            HashSet<ModelItem> visited = new HashSet<ModelItem>();
            // walk up the parent chain and create a modelpath from reverse
            // eg. Root.Foo.Bar.Collectionproperty[3].----

            // if modelItem doesn't have parent and it's not root, return string.Empty;
            if (modelItem.Parent == null && modelItem.Root != modelItem)
            {
                return null;
            }

            while (modelItem != null)
            {
                // paths causing us to get into loops are invalid.
                if (visited.Contains(modelItem))
                {
                    isValid = false;
                    break;
                }
                // remember the visited.
                visited.Add(modelItem);
                // if parent is collection store just the index
                if (modelItem.Parent is ModelItemCollection)
                {
                    sb.Insert(0, "[" + ((ModelItemCollection)modelItem.Parent).IndexOf(modelItem).ToString(CultureInfo.InvariantCulture) + "]");
                }
                // if parent is a modelproperty store the property name
                else if (modelItem.Source != null)
                {
                    sb.Insert(0, "." + modelItem.Source.Name);
                }
                //Our model path doesnt work with dictionaries, so in dictionary case follow the mutablekeyvaluepair 
                if (modelItem.Parent is ModelItemDictionary)
                {
                    if (modelItem.Source != null)
                    {
                        modelItem = modelItem.Source.Parent;
                    }
                    else
                    {
                        isValid = false;
                        break;
                    }
                } // when parent is not a dictionary follow the parent up towards the root.
                else
                {
                    modelItem = modelItem.Parent;
                }
            }
            string s = null;
            if (isValid)
            {
                sb.Insert(0, rootPath);
                s = sb.ToString();
            }
            return s;
        }

        public static ModelItem GetModelItemFromPath(string path, ModelItem root)
        {
            if (null == root)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("root"));
            }
            if (null == path)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("path"));
            }
            ModelItem foundModelItem = null;
            path = path.Trim();
            string[] segments = path.Split('.');
            // process each path. path should atleast be 'Root' and should always  begin with 'Root'
            if (segments.Length > 0 && segments[0] == rootPath)
            {
                foundModelItem = root;
                for (int segmentIndex = 1; segmentIndex < segments.Length; segmentIndex++)
                {
                    string segment = segments[segmentIndex];
                    if (!string.IsNullOrEmpty(segment))
                    {
                        ModelItem next = GetModelItemFromSegment(foundModelItem, segment);
                        if (next != null)
                        {
                            foundModelItem = next;
                        }
                        else
                        {
                            foundModelItem = null;
                            break;
                        }
                    }
                    else
                    {
                        foundModelItem = null;
                        break;
                    }
                }
            }
            return foundModelItem;
        }

        private static ModelItem GetModelItemFromSegment(ModelItem currentModelItem, string segment)
        {
            ModelItem modelItemFromSegment = null;
            int indexOfSquareBrackets = segment.IndexOf('[');
            // e.g Sequence.Activities[0] segment = "Activities[0]"
            if (indexOfSquareBrackets > 0)
            {
                string collectionProperty = segment.Substring(0, indexOfSquareBrackets);
                // find the value of the collection property
                ModelItemCollection segmentCollection = GetModelItemFromSegment(currentModelItem, collectionProperty) as ModelItemCollection;
                if (segmentCollection != null)
                {
                    try
                    {
                        // parse the [index] to find the index
                        string indexString = segment.Substring(indexOfSquareBrackets + 1);
                        indexString = indexString.Substring(0, indexString.Length - 1);
                        int index = Int32.Parse(indexString, CultureInfo.InvariantCulture);
                        if (index >= 0 && index < segmentCollection.Count)
                        {
                            // now index into the collection
                            modelItemFromSegment = segmentCollection[index];
                        }
                    }
                    // dont crash ever.
                    catch (FormatException)
                    {
                    }
                    catch (OverflowException)
                    {
                    }
                }
            }
            // e.g SomeFoo.Then segment = "Then"
            else
            {
                ModelProperty property = currentModelItem.Properties[segment];
                if (property != null)
                {
                    modelItemFromSegment = property.Value;
                }
            }
            return modelItemFromSegment;
        }

        internal static IEnumerable<ModelItem> GetParentEnumerator(this ModelItem item)
        {
            return ModelItemExtensions.GetParentEnumerator(item, null);
        }

        internal static IEnumerable<ModelItem> GetParentEnumerator(this ModelItem item, Func<ModelItem, bool> continueEnumerationPredicate)
        {
            if (null == item)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("item"));
            }
            while (null != item.Parent)
            {
                if (null != continueEnumerationPredicate && !continueEnumerationPredicate(item.Parent))
                {
                    break;
                }
                yield return item.Parent;
                item = item.Parent;
            }
            yield break;
        }

        internal static string GetUniqueName(this ModelItemCollection collection, string nameDefaultPrefix, Func<ModelItem, string> nameGetter)
        {
            return collection.GetUniqueName<ModelItem>(nameDefaultPrefix, nameGetter);
        }

        internal static string GetUniqueName(this ModelItemDictionary dictionary, string nameDefaultPrefix, Func<ModelItem, string> nameGetter)
        {
            if (dictionary != null)
            {
                return dictionary.Keys.GetUniqueName(nameDefaultPrefix, nameGetter);
            }
            else
            {
                throw FxTrace.Exception.ArgumentNull("dictionary");
            }
        }

        internal static string GetUniqueName<T>(this IEnumerable<T> collection, string nameDefaultPrefix, Func<T, string> nameGetter)
        {
            if (null == collection)
            {
                throw FxTrace.Exception.ArgumentNull("collection");
            }
            if (null == nameDefaultPrefix)
            {
                throw FxTrace.Exception.ArgumentNull("nameDefaultPrefix");
            }
            if (nameDefaultPrefix.Length == 0)
            {
                throw FxTrace.Exception.Argument("nameDefaultPrefix", "length == 0");
            }
            if (null == nameGetter)
            {
                throw FxTrace.Exception.ArgumentNull("nameGetter");
            }

            var maxId = (int?)collection
                .Where(p =>
                {
                    var value = nameGetter(p);
                    if (null != value)
                    {
                        return (0 == string.Compare(value, 0, nameDefaultPrefix, 0, nameDefaultPrefix.Length, CultureInfo.CurrentCulture, CompareOptions.None));
                    }
                    return false;
                })
                .Select(p =>
                {
                    int result = 0;
                    return (int.TryParse(nameGetter(p).Substring(nameDefaultPrefix.Length), out result))
                        ? result : 0;
                })
                .OrderByDescending(p => p)
                .FirstOrDefault();

            int id = maxId.HasValue ? maxId.Value + 1 : 1;

            return string.Format(CultureInfo.CurrentCulture, "{0}{1}", nameDefaultPrefix, id);
        }

        internal static bool IsAssignableFrom<T>(this ModelItem item) where T : class
        {
            if (null == item)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }

            return typeof(T).IsAssignableFrom(item.ItemType);
        }

        internal static Activity GetRootActivity(this ModelItem item)
        {
            Object root = item.GetCurrentValue();
            if (root is IDebuggableWorkflowTree)
            {
                return ((IDebuggableWorkflowTree)root).GetWorkflowRoot();
            }
            else
            {
                return root as Activity;
            }
        }

        public static bool IsParentOf(this ModelItem item, ModelItem child)
        {
            if (null == item)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            if (null == child)
            {
                throw FxTrace.Exception.ArgumentNull("child");
            }

            bool isParent = false;
            child.GetParentEnumerator(p => { isParent = ModelItem.Equals(p, item); return !isParent; }).LastOrDefault();
            return isParent;
        }

        public static void Focus(this ModelItem item)
        {
            Focus(item, MaxExpandLevel);
        }

        internal static void Highlight(this ModelItem item)
        {
            ModelItemFocusHelper.Focus(item, MaxExpandLevel, false, Rect.Empty);
        }

        internal static void Highlight(this ModelItem item, Rect rectToBringIntoView)
        {
            ModelItemFocusHelper.Focus(item, MaxExpandLevel, false, rectToBringIntoView);
        }

        public static void Focus(this ModelItem item, int level)
        {
            if (null == item)
            {
                throw FxTrace.Exception.ArgumentNull("item");
            }
            if (level < 1)
            {
                throw FxTrace.Exception.AsError(new ArgumentOutOfRangeException("level"));
            }
            ModelItemFocusHelper.Focus(item, level);
        }

        internal static ModelItem FindParent(this ModelItem item, Predicate<ModelItem> predicate)
        {
            ModelItem parent = item.Parent;

            while (parent != null && !predicate(parent))
            {
                parent = parent.Parent;
            }

            return parent;
        }

        private sealed class ModelItemFocusHelper
        {
            static ModelItemFocusHelper focusTicket = null;

            ModelItem itemToFocus;
            int currentLevel;
            bool shouldAbort = false;

            EditingContext context;
            VirtualizedContainerService containerService;
            WorkflowViewService viewService;
            DesignerView designerView;
            ModelItem[] itemsToExpand;
            bool shouldGetKeyboardFocus;
            Rect rectToBringIntoView;

            EditingContext Context
            {
                get
                {
                    if (null == this.context)
                    {
                        this.context = this.itemToFocus.GetEditingContext();
                    }
                    return this.context;
                }
            }
            VirtualizedContainerService ContainerService
            {
                get
                {
                    if (null == this.containerService)
                    {
                        this.containerService = this.Context.Services.GetService<VirtualizedContainerService>();
                    }
                    return this.containerService;
                }
            }
            WorkflowViewService ViewService
            {
                get
                {
                    if (null == this.viewService)
                    {
                        this.viewService = (WorkflowViewService)this.Context.Services.GetService<ViewService>();
                    }
                    return this.viewService;
                }
            }
            DesignerView DesignerView
            {
                get
                {
                    if (null == this.designerView)
                    {
                        this.designerView = this.Context.Services.GetService<DesignerView>();
                    }
                    return this.designerView;
                }
            }

            Action<VirtualizedContainerService.VirtualizingContainer> onContainerPopulatingDelegate;
            Action<ModelItem> onElementFocusingDelegate;
            Action<Visibility> onSetDesignerContentVisibilityDelegate;
            Action onForceElementFocusDelegate;


            private ModelItemFocusHelper(ModelItem itemToFocus, int maxExpandLevel, bool shouldGetKeyboardFocus, Rect rectToBringIntoView)
            {
                this.itemToFocus = itemToFocus;
                this.currentLevel = maxExpandLevel;
                this.onContainerPopulatingDelegate = this.OnPopulateContainer;
                this.onElementFocusingDelegate = this.OnFocusElement;
                this.onSetDesignerContentVisibilityDelegate = this.ChangeDesignerViewVisibility;
                this.onForceElementFocusDelegate = this.OnForceFocusElement;
                this.shouldGetKeyboardFocus = shouldGetKeyboardFocus;
                this.rectToBringIntoView = rectToBringIntoView;
            }

            // Checks if a model item is rooted at a specific model item
            static bool IsRootedAt(ModelItem item, ModelItem root)
            {
                Fx.Assert(item != null, "item must not be null");
                Fx.Assert(root != null, "root must not be null");
                ModelItem currentItem = item;
                while (currentItem.Parent != null)
                {
                    currentItem = currentItem.Parent;
                }
                return currentItem == root;
            }

            public static void Focus(ModelItem itemToFocus, int maxExpandLevel)
            {
                Focus(itemToFocus, maxExpandLevel, true);
            }

            internal static void Focus(ModelItem itemToFocus, int maxExpandLevel, bool shouldGetKeyboardFocus)
            {
                Focus(itemToFocus, maxExpandLevel, shouldGetKeyboardFocus, Rect.Empty);
            }

            internal static void Focus(ModelItem itemToFocus, int maxExpandLevel, bool shouldGetKeyboardFocus, Rect rectToBringIntoView)
            {
                // Check if this model item exist in the model tree
                IModelTreeItem modelTreeItem = itemToFocus as IModelTreeItem;
                if (modelTreeItem != null)
                {
                    // If this model item doesn't exist in the tree, don't do anything,
                    //  chances are it's an activity that has been deleted.
                    if (!IsRootedAt(itemToFocus, modelTreeItem.ModelTreeManager.Root) && !(itemToFocus is FakeModelItemImpl))
                    {
                        return;
                    }
                }

                //if there is another focus operation in progress, mark it so it would abort itself on next OnContextIdle processing - 
                //we don't want to multiple elements racing for keyboard focus
                if (null != focusTicket)
                {
                    focusTicket.shouldAbort = true;
                }
                //create new focus ticket
                focusTicket = new ModelItemFocusHelper(itemToFocus, maxExpandLevel, shouldGetKeyboardFocus, rectToBringIntoView);
                //and start its processing as soon as application gets idle
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action<ModelItemFocusHelper>((p) => { p.Focus(); }), DispatcherPriority.ContextIdle, focusTicket);
            }

            // Entry point method for setting focus.
            // it is executed exactly once, on application idle event
            // there are 3 basic paths:
            // a) optimistic - element we are looking for, is visible - i bring it into view and set keyboard focus to it
            // b) unlikely - element doesn't have any visual parents - i make it a root designer, wait for it to load and set keyboard focus to it
            // c) pesimistic/complex - element isn't in the view, moreover, it is located in a tree branch which is not (or is partialy) visible
            void Focus()
            {
                //can i continue?
                if (shouldAbort)
                {
                    return;
                }

                //hide the designer view until focus is set
                this.onSetDesignerContentVisibilityDelegate(Visibility.Hidden);
                //delegate visibility restore for designer view after focus update is complete
                Dispatcher.CurrentDispatcher.BeginInvoke(this.onSetDesignerContentVisibilityDelegate, DispatcherPriority.ApplicationIdle, Visibility.Visible);

                //set selection to the item to focus, so all apropriate designers get a chance to update themselfs before we start expanding - this may 
                //result in visual tree change
                Selection.SelectOnly(this.Context, this.itemToFocus);

                //easy path - if the current designer is available and visible - bring it to view and focus
                if (null != this.itemToFocus.View && ((UIElement)this.itemToFocus.View).IsVisible)
                {
                    this.onElementFocusingDelegate(this.itemToFocus);
                    return;
                }

                //get items up to the tree root, which can be visualized (have associated designer)
                //include only up to "level" items (avoid expanding whole tree)                
                bool shouldContinue = true;
                int visualItemsCount = 0;
                var visualItems = this.itemToFocus
                    .GetParentEnumerator(p => shouldContinue)
                    .Where(p =>
                    {
                        //filter only items with designer attribute 
                        bool result = false;
                        var designerType = this.ViewService.GetDesignerType(p.ItemType);
                        if (null != designerType)
                        {
                            result = true;
                            visualItemsCount++;
                            //if designer has Options attribute, check if it always collapsed children - if so, this will be the topmost parent
                            //(displaying anything above, will never display its children)
                            var options = WorkflowViewService.GetAttribute<ActivityDesignerOptionsAttribute>(designerType);
                            if (null != options && options.AlwaysCollapseChildren && visualItemsCount > 2)
                            {
                                shouldContinue = false;
                            }
                        }
                        return result;
                    })
                    .Take(this.currentLevel)
                    .ToArray();



                //nothing to expand, rather unlikely, but handle it anyway
                if (visualItems.Length == 0)
                {
                    //reset ticket, to prevent any further calls from executing
                    ModelItemFocusHelper.focusTicket = null;
                    //force item to be root designer (this is last resort, it is executed only if focusTicket is null)
                    this.onForceElementFocusDelegate();
                    return;
                }

                //get the first parent of an item, which is visible 
                var firstVisibleItem = visualItems.FirstOrDefault(p => null != p.View && ((UIElement)p.View).IsVisible);

                bool enqueueFirstExpand = false;

                //is there anything visible in the path between item and its parents?
                if (null != firstVisibleItem)
                {
                    //yes - limit the amount of items to expand to only designers which are not visible yet 
                    //(include the first visible designer, so algorithm can have a start point with something visible)
                    this.itemsToExpand = visualItems.TakeWhile(p => firstVisibleItem != p).Concat(new ModelItem[] { firstVisibleItem }).ToArray();
                }
                else
                {
                    //no, nothing is visible yet
                    this.itemsToExpand = visualItems;
                    enqueueFirstExpand = true;
                    //make the top most parent as root designer
                    this.DesignerView.MakeRootDesigner(this.itemsToExpand[this.itemsToExpand.Length - 1], false);
                }
                //delegate Expand call - if nothing is visible yet - onIdle - give new designer time to fully render, if someting is visible - execute immediatelly
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() => { this.Expand(null); }), enqueueFirstExpand ? DispatcherPriority.ContextIdle : DispatcherPriority.Send);
            }

            //Expand method is executed repeatadly until maximum expand level is reached. it iterates through the model item tree 
            //(from child up to MaximumExpandLevel parents) and tries to find first visible designer and populate it with content
            //If one elemnt is visited twice (denoted by currentItem argument) it means that expansion failed - (i.e. element is collapsed),
            //so i try to set that element as root designer and restart algoritm with that designer beeing new root
            void Expand(ModelItem currentItem)
            {
                //can i continue?
                if (this.shouldAbort)
                {
                    return;
                }

                //stop condition - prevents infinite loop (the method is delegated into dispatcher, so it would never cause stack overflow
                if (0 > this.currentLevel)
                {
                    ModelItemFocusHelper.focusTicket = null;
                    return;
                }

                //browse direct parents, and Populate the fist one which is visible
                for (int index = 0; null != this.itemsToExpand && index < this.itemsToExpand.Length; ++index)
                {
                    //is given parent visible? (it would return container for given model item)
                    var container = this.ContainerService.QueryContainerForItem(this.itemsToExpand[index]);

                    if (null != container)
                    {
                        //check if container we are trying to expand is not the same as the one in previous iteration 
                        //if it isn't --> populate its content
                        if (!ModelItem.Equals(currentItem, this.itemsToExpand[index]))
                        {
                            this.Populate(container);
                            return;
                        }
                        //if it is --> it means it is collapsed and further expand doesn't make sense. 
                        else if (null != currentItem)
                        {
                            int j = 0;
                            //get index of item which we've tried to expand recently
                            for (; j < this.itemsToExpand.Length; ++j)
                            {
                                if (ModelItem.Equals(this.itemsToExpand[j], currentItem))
                                {
                                    break;
                                }
                            }
                            //starting at that point, see if given item can be a breadcrumb root
                            for (int skipLevel = 0; j >= 0; --j)
                            {
                                currentItem = this.itemsToExpand[j];
                                //if it can - make it a new breadcrumb root and restart
                                if (this.viewService.ShouldAppearOnBreadCrumb(currentItem, true))
                                {
                                    //make that designer a new root (don't set selection)
                                    this.DesignerView.MakeRootDesigner(currentItem, false);
                                    //and try to set focus with less maximum expand level, assuming that current designer is now expanded
                                    ModelItemFocusHelper.Focus(this.itemToFocus, this.currentLevel - skipLevel, this.shouldGetKeyboardFocus);
                                    return;
                                }
                                ++skipLevel;
                            }
                            //nothing in parent list can be made a breadcrumb, try set item which is supposed to get focus as a root 
                            if (this.viewService.ShouldAppearOnBreadCrumb(this.itemToFocus, true))
                            {
                                this.DesignerView.MakeRootDesigner(this.itemToFocus, false);
                                ModelItemFocusHelper.Focus(this.itemToFocus, 1, this.shouldGetKeyboardFocus);
                                return;
                            }
                            //the item we want to set focus to, also cannot be displayed as root;
                            //at this point - simply set selection to the current item, check if visibility has changed due to selection change
                            this.Context.Items.SetValue(new Selection(currentItem));
                            Dispatcher.CurrentDispatcher.BeginInvoke(this.onElementFocusingDelegate, DispatcherPriority.ContextIdle, currentItem);
                            //the final check - if item is still not visible, force it to be 
                            Dispatcher.CurrentDispatcher.BeginInvoke(this.onForceElementFocusDelegate, DispatcherPriority.ContextIdle);
                            return;
                        }
                    }
                }
                ModelItemFocusHelper.focusTicket = null;
                //if we end up here and itemsToExpand is not null - something is wrong...
                //it is possible that algorithm stops here and itemsToExpand is null - this would be scenario when user tries to set focus to model item which cannot be
                //visualized and doesn't have any visual parent - i.e. Service or ActivityBuilder (they have a child property Body which can be visualized, but themselves - are not)
                if (null != this.itemsToExpand)
                {
                    var displayProperty = this.itemToFocus.Properties["DisplayName"];
                    var displayName = displayProperty == null ? "(unknown)" : displayProperty.ComputedValue.ToString();
                    Fx.Assert("Expand is in invalid state - we should never end up here. Item to focus: " + displayName + " (" + this.itemToFocus.ItemType.Name + ")");
                }
            }

            //Populate method is executed by Expand method. It is supposed to bring container element into view, 
            //find the elemennt we are looking for (or at least container which contains it). After bringing contaner into view, it delegates calls to 
            //OnPopulateContainer (if we have virutal container) and then to OnFocusElement delegate
            void Populate(FrameworkElement container)
            {
                //ensure container is in the view
                container.BringIntoView();
                //is it virtualized container?
                var virtualContainer = container as VirtualizedContainerService.VirtualizingContainer;
                var viewElement = container as WorkflowViewElement;
                var modelItem = (null != virtualContainer ? virtualContainer.ModelItem : (viewElement != null ? viewElement.ModelItem : null));
                var dispatchParameter = new object[] { modelItem };
                DispatcherPriority priority = DispatcherPriority.Send;

                if (null != virtualContainer)
                {
                    priority = DispatcherPriority.ContextIdle;
                    //yes - ensure its content is populated
                    virtualContainer.Populate();
                    //wait until container content renders (delegate calls to application idle)
                    Dispatcher.CurrentDispatcher.BeginInvoke(this.onContainerPopulatingDelegate, priority, virtualContainer);
                }
                //if we have a virtual contianer - we may need to drill further or simply display an element, 
                //otherwise - just try to focus on element (it should be visible, so execute callback immediately)
                Dispatcher.CurrentDispatcher.BeginInvoke(this.onElementFocusingDelegate, priority, dispatchParameter);
            }

            void OnPopulateContainer(VirtualizedContainerService.VirtualizingContainer virtualContainer)
            {
                if (this.shouldAbort)
                {
                    return;
                }
                //if this is virutal container, it might contain multiple other virtual containers - i need to find the one
                //which either is a container for item i want to focus, or one which is parent designer for the item i'm looking for
                //look for the container which contains or is a parent of container i look for
                var target = virtualContainer
                    .ChildContainers
                    .FirstOrDefault(p => ModelItem.Equals(this.itemToFocus, p.ModelItem) || p.ModelItem.IsParentOf(this.itemToFocus));

                //if one is found - populate it and bring it into view
                if (null != target)
                {
                    target.Populate();
                    target.BringIntoView();
                }
            }

            void OnFocusElement(ModelItem currentItem)
            {
                if (this.shouldAbort)
                {
                    return;
                }

                //after virtual container is loaded and populated, check if the item i'm looking for is visible
                if (null != this.itemToFocus.View && ((FrameworkElement)this.itemToFocus.View).IsVisible)
                {
                    //yes! - it is visible, bring it into view and set focus
                    if (rectToBringIntoView != Rect.Empty)
                    {
                        ((FrameworkElement)this.itemToFocus.View).BringIntoView(rectToBringIntoView);
                    }
                    else
                    {
                        ((FrameworkElement)this.itemToFocus.View).BringIntoView();
                    }
                    if (this.shouldGetKeyboardFocus)
                    {
                        Keyboard.Focus(this.itemToFocus.View as IInputElement);
                    }
                    ModelItemFocusHelper.focusTicket = null;
                }
                else if (null != currentItem)
                {
                    //no, it still isn't visible - try to expand next level
                    --this.currentLevel;
                    this.Expand(currentItem);
                }
                else
                {
                    ModelItemFocusHelper.focusTicket = null;
                    var displayProperty = this.itemToFocus.Properties["DisplayName"];
                    var displayName = displayProperty == null ? "(unknown)" : displayProperty.ComputedValue.ToString();
                    Fx.Assert("OnFocusElement is in invalid state - we should never get here. Item to focus: " + displayName + " (" + this.itemToFocus.ItemType.Name + ")");
                }
            }

            void OnForceFocusElement()
            {
                if (this.shouldAbort)
                {
                    return;
                }
                //if we did exploit all possibilites but model item is still not visible and focused - force the lowest parent that can be made root as the root designer
                if (null == ModelItemFocusHelper.focusTicket && (null == this.itemToFocus.View || !((UIElement)this.itemToFocus.View).IsVisible))
                {
                    ModelItem item = this.itemToFocus;
                    while (item != null && !this.ViewService.ShouldAppearOnBreadCrumb(item, true))
                    {
                        item = item.Parent;
                    }
                    if (item != null)
                    {
                        this.DesignerView.MakeRootDesigner(item, false, false);
                        Dispatcher.CurrentDispatcher.BeginInvoke(this.onElementFocusingDelegate, DispatcherPriority.ContextIdle, item);
                    }
                }
            }

            void ChangeDesignerViewVisibility(Visibility state)
            {
                if (!this.shouldAbort)
                {
                    //i can't set visibility to hidden, so in order to avoid flickering, i simply set opacity to very low value - 
                    //visual tree is still visible, but user won't notice it.
                    //this.DesignerView.ScrollableContent.Opacity = (state == Visibility.Visible ? 1.0 : 0.01);
                    Mouse.OverrideCursor = (state == Visibility.Visible ? null : Cursors.Wait);
                }
            }
        }
    }
}
