//----------------------------------------------------------------
// <copyright company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------

namespace System.Activities.Presentation.View.TreeView
{
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View.OutlineView;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;

    internal static class DesignerTreeAutoExpandHelper
    {
        internal static TreeViewItemViewModel Expand(TreeViewItemModelItemViewModel rootTreeViewItem, ModelItem modelItemToExpandTo)
        {
            Fx.Assert(modelItemToExpandTo != null && rootTreeViewItem != null, "rootTreeViewItem and modelItemToExpand should not have null value");

            // ModelItems with HidePropertyInOutlineViewAttribute are invisible in the designerTree.
            if (ExtensibilityAccessor.GetAttribute<HidePropertyInOutlineViewAttribute>(modelItemToExpandTo) != null)
            {
                return null;
            }

            Stack pathStack = new Stack();

            TreeViewItemViewModel itemToBeSelected = null;

            if (GetExpandingPath(modelItemToExpandTo, pathStack, new HashSet<ModelItem>()))
            {
                // If the root of modelItemToExpandTo differs from the root of the designerTree, it means modelItemToExpandTo doesn't belong to the designerTree.
                if (pathStack.Pop() != rootTreeViewItem.VisualValue)
                {
                    return null;
                }

                object item = null;
                TreeViewItemViewModel treeViewItem = rootTreeViewItem;
                TreeViewItemViewModel tempTreeViewItem = rootTreeViewItem;

                // Using the path to the root, expand the corresponding tree node. Ignore the items which is not visible on the designerTree.
                while (pathStack.Count > 0)
                {
                    if (tempTreeViewItem != null)
                    {
                        treeViewItem = tempTreeViewItem;
                        treeViewItem.IsExpanded = true;
                    }

                    item = pathStack.Pop();
                    tempTreeViewItem = (from child in treeViewItem.Children
                                        where (child is TreeViewItemModelItemViewModel && ((TreeViewItemModelItemViewModel)child).VisualValue == item as ModelItem)
                                        || (child is TreeViewItemModelPropertyViewModel && ((TreeViewItemModelPropertyViewModel)child).VisualValue == item as ModelProperty)
                                        || (child is TreeViewItemKeyValuePairModelItemViewModel && ((TreeViewItemKeyValuePairModelItemViewModel)child).VisualValue.Value == item as ModelItem)
                                        select child).FirstOrDefault();

                    // For TreeViewItemKeyValuePairModelItemViewModel, its path to the children is very complicated.
                    // Take Switch as example: Switch(ModelItem) -> Cases(ModelProperty) -> KeyDictionaryCollection(ModelItem) -> ItemsCollection(ModelProperty) -> ModelItemKeyValuePair<T, Activity>(ModelItem) -> Value(ModelProperty) -> Children
                    // All the path nodes except Switch and Children are invisible and can be ignored, the child node in the path is used twice to search for TreeViewItemKeyValuePairModelItemViewModel and its children in designerTree.
                    if (tempTreeViewItem is TreeViewItemKeyValuePairModelItemViewModel)
                    {
                        // For further searching
                        pathStack.Push(item);
                    }

                    if (pathStack.Count == 0)
                    {
                        itemToBeSelected = tempTreeViewItem;
                    }
                }
            }

            return itemToBeSelected;
        }

        // Get a path from the modelItem to the root.
        // Path is stored in pathStack as stack datastructure, itemSet is used to avoid loop.
        private static bool GetExpandingPath(ModelItem modelItem, Stack pathStack, HashSet<ModelItem> itemSet)
        {            
            if (modelItem == null || itemSet.Contains(modelItem) || ExtensibilityAccessor.GetAttribute<HidePropertyInOutlineViewAttribute>(modelItem) != null)
            {
                return false;
            }

            itemSet.Add(modelItem);
            pathStack.Push(modelItem);

            if (modelItem.Parents == null || modelItem.Parents.Count() == 0)
            {
                return true;
            }

            if (modelItem.Sources != null && modelItem.Sources.Count() != 0)
            {
                // By design, modelItem's path to the parents is through Sources to their Parent.
                foreach (ModelProperty property in modelItem.Sources)
                {
                    // ModelProperties with HidePropertyInOutlineViewAttribute are also invisible.
                    if (ExtensibilityAccessor.GetAttribute<HidePropertyInOutlineViewAttribute>(property) == null)
                    {
                        // Property is also stored in the path stack because some properties have visible nodes in the designerTree.
                        pathStack.Push(property);
                        if (GetExpandingPath(property.Parent, pathStack, itemSet))
                        {
                            return true;
                        }
                        else
                        {
                            pathStack.Pop();
                        }
                    }
                }
            }
            else
            {
                // If a modelItem is inside an modelItemCollection, its Sources property is null.
                foreach (ModelItem item in modelItem.Parents)
                {
                    if (GetExpandingPath(item, pathStack, itemSet))
                    {
                        return true;
                    }
                }
            }

            itemSet.Remove(modelItem);
            pathStack.Pop();
            return false;
        }
    }
}
