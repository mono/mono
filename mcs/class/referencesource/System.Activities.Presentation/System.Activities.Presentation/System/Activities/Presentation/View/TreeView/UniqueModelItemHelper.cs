//-----------------------------------------------------------------------
// <copyright file="UniqueModelItemHelper.cs" company="Microsoft Corporation">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.Activities.Presentation.View.TreeView
{
    using System.Activities.Presentation.Internal.PropertyEditing;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.View.OutlineView;
    using System.Collections.Generic;
    using System.Linq;
    
    internal static class UniqueModelItemHelper
    {
        // Return HashSet containing ModelItems found only through given property: Cannot reach property.Parent without
        // hitting property from these immediate descendents of property.Value
        // Set may contain some internal duplication -- all nodes, not just the root, of a linked tree will be included
        //
        // Caveat 1: Due to problems removing Parents (e.g. Case content sometimes holds references to FlowSwitch after
        // Case removed), this method is not entirely reliable -- customers may occasionally need to reopen Designer
        // Caveat 2: Due to lazy loading of Properties (and therefore the back-pointing Parents collection), may
        // temporarily include non-unique ModelItems in returned set -- cleared as tree or designer views expand
        //
        // (Throughout, cannot use ModelItem.GetParentEnumerator because that does not check all Sources and Parents)
        internal static HashSet<ModelItem> FindUniqueChildren(ModelProperty property)
        {
            HashSet<ModelItem> retval = new HashSet<ModelItem>();
            if (null != property && null != property.Parent && null != property.Value)
            {
                ModelItem target = property.Parent;
                ModelItem expected = property.Value;
                HashSet<ModelItem> visited = new HashSet<ModelItem>();

                // Check all immediate children of property.Value
                ModelItemCollection collection = expected as ModelItemCollection;
                if (null == collection)
                {
                    ModelItemDictionary dictionary = expected as ModelItemDictionary;
                    if (null == dictionary)
                    {
                        // ModelItem
                        // Can't use UniqueRoute because we're starting at expected
                        // Can't use EnqueueParents because we need to special-case given property
                        // Instead confirm property.Value is not referenced anywhere else
                        ModelItemImpl expectedImpl = expected as ModelItemImpl;
                        if (null != expectedImpl)
                        {
                            bool justThisSource = true;

                            // expectedImpl.InternalParents does not include ModelItems that are just Sources
                            if (0 == expectedImpl.InternalParents.Count)
                            {
                                // expectedImpl.InternalSources would be similar but adds a wrapper we don't need here
                                foreach (ModelProperty source in expected.Sources)
                                {
                                    if (null != source.Parent && !visited.Contains(source.Parent))
                                    {
                                        visited.Add(source.Parent);
                                        if (!property.Equals(source) && !expected.Equals(source) &&
                                            null == ExtensibilityAccessor.GetAttribute<HidePropertyInOutlineViewAttribute>(source))
                                        {
                                            // Found a non-ignored property from somewhere else referencing expected
                                            justThisSource = false;
                                            break;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                // Found a Parent that's not a Source.Parent: property.Value is in some collection
                                justThisSource = false;
                            }

                            if (justThisSource)
                            {
                                retval.Add(expected);
                            }
                        }
                    }
                    else
                    {
                        // ModelItemDictionary
                        foreach (KeyValuePair<ModelItem, ModelItem> child in dictionary)
                        {
                            if (null != child.Key && !visited.Contains(child.Key))
                            {
                                visited.Add(child.Key);
                                if (UniqueModelItemHelper.UniqueRoute(child.Key, target, expected))
                                {
                                    retval.Add(child.Key);
                                }
                            }

                            if (null != child.Value && !visited.Contains(child.Value))
                            {
                                visited.Add(child.Value);
                                if (UniqueModelItemHelper.UniqueRoute(child.Value, target, expected))
                                {
                                    retval.Add(child.Value);
                                }
                            }
                        }
                    }
                }
                else
                {
                    // ModelItemCollection
                    foreach (ModelItem child in collection)
                    {
                        if (null != child && !visited.Contains(child))
                        {
                            visited.Add(child);
                            if (UniqueModelItemHelper.UniqueRoute(child, target, expected))
                            {
                                retval.Add(child);
                            }
                        }
                    }
                }
            }

            return retval;
        }

        // Enqueue Parents of given ModelItem
        // Do not enqueue source Properties with a ViewIgnore attribute
        private static void EnqueueParents(ModelItem item, Queue<ModelItem> queue)
        {
            Dictionary<ModelItem, int> nonSources = new Dictionary<ModelItem, int>();
            if (null != item)
            {
                // Initialize nonSources dictionary to hold all Parents
                foreach (ModelItem parent in item.Parents)
                {
                    if (nonSources.ContainsKey(parent))
                    {
                        ++nonSources[parent];
                    }
                    else
                    {
                        nonSources.Add(parent, 1);
                    }
                }

                // Enqueue Sources and remove found items from nonSources
                foreach (ModelProperty source in item.Sources)
                {
                    if (null != source.Parent)
                    {
                        if (null == ExtensibilityAccessor.GetAttribute<HidePropertyInOutlineViewAttribute>(source))
                        {
                            queue.Enqueue(source.Parent);
                        }

                        if (nonSources.ContainsKey(source.Parent))
                        {
                            --nonSources[source.Parent];
                        }
                    }
                }

                // Deal with the collections that contain this ModelItem
                foreach (KeyValuePair<ModelItem, int> kvp in nonSources.Where((kvp) => 0 < kvp.Value))
                {
                    queue.Enqueue(kvp.Key);
                }
            }
        }

        // Determine if targetParent is only reachable from item via expectedParent
        // Return true if the only routes from item to targetParent include expectedParent; false otherwise
        // Do not search past source Properties with a ViewIgnore attribute but continue looking for targetParent
        private static bool UniqueRoute(ModelItem item, ModelItem targetParent, ModelItem expectedParent)
        {
            bool retval = true;
            if (null == item)
            {
                retval = false;
            }
            else
            {
                HashSet<ModelItem> visited = new HashSet<ModelItem>();
                Queue<ModelItem> todo = new Queue<ModelItem>();
                todo.Enqueue(item);

                while (0 < todo.Count)
                {
                    ModelItem parent = todo.Dequeue();
                    if (null != parent && !visited.Contains(parent))
                    {
                        visited.Add(parent);

                        if (parent.Equals(targetParent))
                        {
                            // Failure: Route was not unique, have reached target without passing expectedParent
                            retval = false;
                            break;
                        }
                        else if (!parent.Equals(expectedParent))
                        {
                            UniqueModelItemHelper.EnqueueParents(parent, todo);
                        }
                    }
                }
            }

            return retval;
        }
    }
}
