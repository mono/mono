//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Xaml
{
    using System.Activities.Debugger;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.ViewState;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.Collections;

    // This class is to create and hold the mapping of Activity to SourceLocation and SourceLocation to ModelItem.
    // First, XamlReader write in activity object and its SourceLocation. Then model search service can pass in the
    // ModelItem list to create a source location to model item mapping.
    internal class ObjectToSourceLocationMapping
    {
        private IDictionary<object, SourceLocation> deserializedObjectToSourceLocationMapping;
        private IDictionary<SourceLocation, ModelItem> sourceLocationToModelItemMapping;
        private IDictionary<SourceLocation, ModelItem> viewStateSourceLocationToModelItemMapping;
        private ModelSearchServiceImpl modelSearchService;

        // Is the Object to SourceLocation mapping updated? generally means the file was reloaded.
        private bool updateRequired;

        internal ObjectToSourceLocationMapping(ModelSearchServiceImpl modelSearchService)
        {
            this.modelSearchService = modelSearchService;
            this.deserializedObjectToSourceLocationMapping = new OrderedDictionary<object, SourceLocation>();
            this.sourceLocationToModelItemMapping = new Dictionary<SourceLocation, ModelItem>();
            this.viewStateSourceLocationToModelItemMapping = new Dictionary<SourceLocation, ModelItem>();
        }

        internal void Clear()
        {
            this.deserializedObjectToSourceLocationMapping.Clear();
            this.updateRequired = true;
        }

        internal Dictionary<object, object> SourceLocationObjectToModelItemObjectMapping
        {
            get;
            set;
        }

        internal Dictionary<string, SourceLocation> ViewStateDataSourceLocationMapping
        {
            get;
            set;
        }

        internal void UpdateMap(object key, SourceLocation sourceLocation)
        {
            if (this.deserializedObjectToSourceLocationMapping.ContainsKey(key))
            {
                this.deserializedObjectToSourceLocationMapping.Remove(key);
            }

            this.deserializedObjectToSourceLocationMapping.Add(key, new SourceLocation(/* fileName = */ null,
                sourceLocation.StartLine, sourceLocation.StartColumn,
                sourceLocation.EndLine, sourceLocation.EndColumn));
        }

        // create a SourceLocation to ModelItem mapping based on the current activity to SourceLocation mapping.
        private void UpdateSourceLocationToModelItemMapping(IEnumerable<ModelItem> modelItemsOnDesigner)
        {
            Dictionary<object, SourceLocation> validMapping = GetValidSourceLocationMapping();
            this.sourceLocationToModelItemMapping.Clear();
            this.viewStateSourceLocationToModelItemMapping.Clear();
            foreach (ModelItem modelItem in modelItemsOnDesigner)
            {
                SourceLocation srcLocation = FindMatchSrcLocation(modelItem, validMapping);
                if (srcLocation != null)
                {
                    sourceLocationToModelItemMapping.Add(srcLocation, modelItem);
                }
                string workflowViewStateIdRef = WorkflowViewState.GetIdRef(modelItem.GetCurrentValue());
                if (!String.IsNullOrEmpty(workflowViewStateIdRef))
                {
                    SourceLocation viewStateSrcLocation = this.FindViewStateDataSrcLocationByViewStateIdRef(workflowViewStateIdRef);
                    if (viewStateSrcLocation != null)
                    {
                        // In some cases duplicated key is possible, use indexer instead of Add() to avoid throw. 
                        // See TFS bug 523908 for detailed information
                        viewStateSourceLocationToModelItemMapping[viewStateSrcLocation] = modelItem;
                    }
                }
            }

            this.updateRequired = false;
        }

        // find a modelitem whose SourceLocation contains the srcLocation passed in.
        internal ModelItem FindModelItem(SourceLocation srcLocation)
        {
            this.EnsureUpdated();
            return FindModelItemInMap(srcLocation, this.sourceLocationToModelItemMapping);
        }

        internal ModelItem FindModelItemOfViewState(SourceLocation srcLocation)
        {
            this.EnsureUpdated();
            return FindModelItemInMap(srcLocation, this.viewStateSourceLocationToModelItemMapping);
        }

        internal SourceLocation FindSourceLocation(ModelItem modelItem)
        {
            this.EnsureUpdated();
            KeyValuePair<SourceLocation, ModelItem>? matchingMappingRecord = sourceLocationToModelItemMapping.SingleOrDefault(kvp => object.ReferenceEquals(kvp.Value, modelItem));
            if (matchingMappingRecord.HasValue)
            {
                return matchingMappingRecord.Value.Key;
            }
            else
            {
                return null;
            }
        }

        private static ModelItem FindModelItemInMap(SourceLocation sourceLocation, IDictionary<SourceLocation, ModelItem> map)
        {
            SourceLocation exactSourceLocation = GetExactLocation(sourceLocation, map);
            if (exactSourceLocation == null)
            {
                return null;
            }

            return map[exactSourceLocation];
        }

        internal IEnumerable<ModelItem> GetObjectsWithSourceLocation()
        {
            this.EnsureUpdated();
            return this.sourceLocationToModelItemMapping.Values;
        }

        private static SourceLocation FindSrcLocation(Dictionary<object, SourceLocation> mapping, Predicate<object> predicate)
        {
            object foundedObject = null;
            if (mapping == null)
            {
                return null;
            }

            foreach (object key in mapping.Keys)
            {
                if (predicate(key))
                {
                    foundedObject = key;
                    break;
                }
            }

            if (foundedObject != null)
            {
                SourceLocation result = mapping[foundedObject];
                mapping.Remove(foundedObject);
                return result;
            }

            return null;
        }

        private static SourceLocation FindMatchSrcLocation(ModelItem modelItem, Dictionary<object, SourceLocation> mapping)
        {
            object modelObject = modelItem.GetCurrentValue();
            return FindSrcLocation(mapping, (key) =>
                {
                    return object.ReferenceEquals(modelObject, key);
                });
        }

        private SourceLocation FindViewStateDataSrcLocationByViewStateIdRef(string workflowViewStateIdRef)
        {
            if (this.ViewStateDataSourceLocationMapping == null)
            {
                return null;
            }

            SourceLocation sourceLocation = null;
            this.ViewStateDataSourceLocationMapping.TryGetValue(workflowViewStateIdRef, out sourceLocation);
            return sourceLocation;
        }

        // get the minimum source location which contains this source location and is in the mapping store.
        private static SourceLocation GetExactLocation(SourceLocation approximateLocation, IDictionary<SourceLocation, ModelItem> mapping)
        {
            SourceLocation candidate = null;
            foreach (SourceLocation srcLocation in mapping.Keys)
            {
                // in the scope?
                if (srcLocation.Contains(approximateLocation))
                {
                    if (candidate != null)
                    {
                        // More approximate?
                        if (candidate.Contains(srcLocation))
                        {
                            candidate = srcLocation;
                        }
                    }
                    else
                    {
                        candidate = srcLocation;
                    }
                }
            }

            return candidate;
        }

        private void EnsureUpdated()
        {
            if (this.updateRequired)
            {
                IEnumerable<ModelItem> itemsOnDesigner = this.modelSearchService.GetItemsOnDesigner(preOrder: false, excludeRoot: false, excludeErrorActivity: false, excludeExpression: false, includeOtherObjects: true);
                this.UpdateSourceLocationToModelItemMapping(itemsOnDesigner);
            }
        }

        private Dictionary<object, SourceLocation> GetValidSourceLocationMapping()
        {
            Dictionary<object, SourceLocation> validSrcLocMapping = new Dictionary<object, SourceLocation>();
            foreach (KeyValuePair<object, SourceLocation> entry in deserializedObjectToSourceLocationMapping)
            {
                if (IsValidRange(entry.Value.StartLine, entry.Value.StartColumn, entry.Value.EndLine, entry.Value.EndColumn))
                {
                    object sourceLocationObject = entry.Key;
                    object modelItemObject;
                    if (this.SourceLocationObjectToModelItemObjectMapping == null)
                    {
                        modelItemObject = sourceLocationObject;
                    }
                    else
                    {
                        this.SourceLocationObjectToModelItemObjectMapping.TryGetValue(sourceLocationObject, out modelItemObject);
                    }

                    if (modelItemObject != null)
                    {
                        validSrcLocMapping.Add(modelItemObject, entry.Value);
                    }
                }
            }
            return validSrcLocMapping;

        }

        private static bool IsValidRange(int startLine, int startColumn, int endLine, int endColumn)
        {
            return
                (startLine > 0) && (startColumn > 0) && (endLine > 0) && (endColumn > 0) &&
                ((startLine < endLine) || (startLine == endLine && startColumn < endColumn));
        }
    }
}
