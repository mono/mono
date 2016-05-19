//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Debug
{
    using System;
    using System.Activities;
    using System.Activities.Debugger;
    using System.Activities.Presentation;
    using System.Activities.Presentation.Hosting;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.Validation;
    using System.Activities.Presentation.View;
    using System.Activities.Presentation.Xaml;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime;
    using System.Windows.Documents;
    using System.Windows.Threading;

    [Fx.Tag.XamlVisible(false)]
    public class DebuggerService : IDesignerDebugView
    {
        EditingContext context;
        ModelItem selectedModelItem;
        SourceLocation currentLocation;
        SourceLocation currentContext;
        ModelItem currentModelItem;
        ModelItem currentModelItemContext;
        WorkflowViewService viewService;
        ModelSearchServiceImpl modelSearchService;
        ModelTreeManager modelTreeManager;
        const string unresolvedPrefix = "unresolved:";

        AttachedProperty<bool> isBreakpointEnabledProperty;
        AttachedProperty<bool> isBreakpointBoundedProperty;
        AttachedProperty<bool> isBreakpointConditionalProperty;
        AttachedProperty<bool> isCurrentLocationProperty;
        AttachedProperty<bool> isCurrentContextProperty;

        Dictionary<object, SourceLocation> instanceToSourceLocationMapping;
        Dictionary<ModelItem, SourceLocation> modelItemToSourceLocation;
        Dictionary<SourceLocation, ModelItem> sourceLocationToModelItem;

        Dictionary<ModelItem, BreakpointTypes> breakpoints;              // The map contains breakpoint that has its ModelItem on the modelTree.
        Dictionary<ModelItem, BreakpointTypes> transientBreakpoints;     // The map contains breakpoint that has its ModelItem not on the modelTree.
        Dictionary<SourceLocation, BreakpointTypes> unmappedBreakpoints; // The map contains breakpoint that has no ModelItem

        // This is used to generate unique source line no when the view element does not have a source location
        int lastSourceLineNo = 1;

        bool isReadOnly = false;
        bool isDebugging = false;
        private string fileName;
        private bool requiresUpdateSourceLocation;

        // Storing background BringToViewCurrentLocation operation.
        DispatcherOperation bringToViewCurrentLocationOperation = null;

        public DebuggerService(EditingContext context)
        {
            if (context == null)
            {
                throw FxTrace.Exception.ArgumentNull("context");
            }

            this.context = context;
            this.modelItemToSourceLocation = new Dictionary<ModelItem, SourceLocation>();
            this.sourceLocationToModelItem = new Dictionary<SourceLocation, ModelItem>();
            this.breakpoints = new Dictionary<ModelItem, BreakpointTypes>();
            // Breakpoints transiently removed from the document (copy/paste/undo/redo).
            this.transientBreakpoints = new Dictionary<ModelItem, BreakpointTypes>();
            this.unmappedBreakpoints = new Dictionary<SourceLocation, BreakpointTypes>(4);
            this.instanceToSourceLocationMapping = new Dictionary<object, SourceLocation>();

            this.context.Items.Subscribe<Selection>(new SubscribeContextCallback<Selection>(this.SelectionChanged));
            this.context.Services.Subscribe<ViewService>(new SubscribeServiceCallback<ViewService>(this.OnViewServiceAvailable));
            this.context.Services.Subscribe<ModelSearchService>(new SubscribeServiceCallback<ModelSearchService>(this.OnModelSearchServiceAvailable));
            this.context.Services.Subscribe<AttachedPropertiesService>(new SubscribeServiceCallback<AttachedPropertiesService>(this.OnAttachedPropertiesServiceAvailable));
            this.context.Services.Subscribe<ModelTreeManager>(new SubscribeServiceCallback<ModelTreeManager>(this.OnModelTreeManagerServiceAvailable));

            this.requiresUpdateSourceLocation = true;
        }

        // IDesignerDebugView

        // Get the currently selected location from the designer
        // generally this is the location of the object currently selected by the user
        public SourceLocation SelectedLocation
        {
            get
            {
                return (this.selectedModelItem != null && AllowBreakpointAttribute.IsBreakpointAllowed(this.selectedModelItem.ItemType)) ?
                    this.GetSourceLocationFromModelItem(this.selectedModelItem) : null;
            }
        }

        // Set current location of execution.
        // The location to shown the "yellow" arrow.
        public SourceLocation CurrentLocation
        {
            get
            {
                return this.currentLocation;
            }

            set
            {
                this.currentLocation = value;
                ModelItem previousModelItem = this.currentModelItem;
                UpdateCurrentModelItem();
                if (this.currentLocation != null && this.currentModelItem == null)
                {   // This is a rare case but it happens when the designer is not all done with bringing up the view but
                    // Debugger already set this location.
                    PostBringToViewCurrentLocation(previousModelItem);
                }
                else
                {
                    BringToViewCurrentLocation(previousModelItem);
                }
            }
        }

        public void EnsureVisible(SourceLocation sourceLocation)
        {
            SourceLocation exactLocation = GetExactLocation(sourceLocation);
            ModelItem mi = this.GetModelItemFromSourceLocation(exactLocation, /* forceCreate */ true);
            if (mi != null)
            {
                BringToView(mi);
            }
        }

        void BringToViewCurrentLocation(ModelItem previousModelItem)
        {
            SetPropertyValue(previousModelItem, isCurrentLocationProperty, this.currentModelItem);
            if (this.currentModelItem != this.currentModelItemContext)
            {
                BringToView(this.currentModelItem);
            }
        }

        // Post new BringToViewCurrentLocation operation
        void PostBringToViewCurrentLocation(ModelItem previousModelItem)
        {
            // Abort pending operation.
            if (this.bringToViewCurrentLocationOperation != null)
            {
                this.bringToViewCurrentLocationOperation.Abort();
                this.bringToViewCurrentLocationOperation = null;
            }

            // Post a new background operation.
            this.bringToViewCurrentLocationOperation = Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                (DispatcherOperationCallback)delegate(object arg)
                {
                    this.UpdateCurrentModelItem();
                    this.BringToViewCurrentLocation(previousModelItem);
                    this.bringToViewCurrentLocationOperation = null;
                    return null;
                },
                null);
        }


        // Set current context (stack frame scope).
        // The highlighted scope of execution.
        public SourceLocation CurrentContext
        {
            get
            {
                return this.currentContext;
            }
            set
            {
                this.currentContext = value;
                ModelItem previousModelItem = this.currentModelItemContext;
                UpdateCurrentModelItemContext();
                SetPropertyValue(previousModelItem, this.isCurrentContextProperty, this.currentModelItemContext);
                BringToView(this.currentModelItemContext);
            }
        }

        // Set to true while debugging
        public bool IsDebugging
        {
            get
            {
                return this.isDebugging;
            }

            set
            {
                ReadOnlyState readOnlyState = this.context.Items.GetValue<ReadOnlyState>();
                if (readOnlyState != null)
                {
                    // start debugging
                    if (value && !this.isDebugging)
                    {
                        this.isDebugging = true;
                        // backup the read-only state
                        this.isReadOnly = readOnlyState.IsReadOnly;
                        readOnlyState.IsReadOnly = true;
                    }
                    // finish debugging
                    else if (!value && this.isDebugging)
                    {
                        this.isDebugging = false;
                        // restore to previous state before debugging
                        readOnlyState.IsReadOnly = this.isReadOnly;
                    }
                    this.context.Items.SetValue(new ReadOnlyState() { IsReadOnly = readOnlyState.IsReadOnly });
                }
            }
        }

        public bool HideSourceFileName
        {
            get;
            set;
        }

        void UpdateCurrentModelItem()
        {
            this.currentModelItem = this.GetModelItemFromSourceLocation(this.currentLocation, /* forceCreate */ true);
        }

        void UpdateCurrentModelItemContext()
        {
            this.currentModelItemContext = this.GetModelItemFromSourceLocation(this.currentContext, /* forceCreate */ true);
        }

        void BringToView(ModelItem modelItem)
        {
            if (modelItem != null)
            {
                modelItem.Focus();
            }
        }

        void OnAttachedPropertiesServiceAvailable(AttachedPropertiesService attachedPropertiesService)
        {
            this.isBreakpointEnabledProperty = new AttachedProperty<bool>()
                {
                    Getter = (modelItem) => IsBreakpointOfType(modelItem, BreakpointTypes.Enabled),
                    Name = "IsBreakpointEnabled",
                    OwnerType = typeof(object)
                };

            this.isBreakpointBoundedProperty = new AttachedProperty<bool>()
                {
                    Getter = (modelItem) => IsBreakpointOfType(modelItem, BreakpointTypes.Bounded),
                    Name = "IsBreakpointBounded",
                    OwnerType = typeof(object)
                };

            this.isBreakpointConditionalProperty = new AttachedProperty<bool>()
                {
                    Getter = (modelItem) => IsBreakpointOfType(modelItem, BreakpointTypes.Conditional),
                    Name = "IsBreakpointConditional",
                    OwnerType = typeof(object)
                };

            this.isCurrentLocationProperty = new AttachedProperty<bool>()
                {
                    Getter = (modelItem) => IsCurrentLocation(modelItem),
                    Name = "IsCurrentLocation",
                    OwnerType = typeof(object)
                };

            this.isCurrentContextProperty = new AttachedProperty<bool>()
                {
                    Getter = (modelItem) => IsCurrentContext(modelItem),
                    Name = "IsCurrentContext",
                    OwnerType = typeof(object)
                };

            attachedPropertiesService.AddProperty(isBreakpointEnabledProperty);
            attachedPropertiesService.AddProperty(isBreakpointBoundedProperty);
            attachedPropertiesService.AddProperty(isBreakpointConditionalProperty);
            attachedPropertiesService.AddProperty(isCurrentLocationProperty);
            attachedPropertiesService.AddProperty(isCurrentContextProperty);
        }

        void OnModelTreeManagerServiceAvailable(ModelTreeManager modelTreeManager)
        {
            this.modelTreeManager = modelTreeManager;
            this.modelTreeManager.EditingScopeCompleted += OnEditingScopeCompleted;
        }

        private void OnEditingScopeCompleted(object sender, EditingScopeEventArgs e)
        {
            Fx.Assert(e.EditingScope != null, "e.EditingScope should not be null.");
            foreach (ModelItem removedModelItem in e.EditingScope.ItemsRemoved)
            {
                DeleteModelItem(removedModelItem);
            }
        }

        private void DeleteModelItem(ModelItem modelItem)
        {
            if (modelItem != null)
            {
                BreakpointTypes breakpointType;
                if (this.breakpoints.TryGetValue(modelItem, out breakpointType))
                {
                    this.transientBreakpoints[modelItem] = breakpointType;   // cache it in case it's added later (move case).
                    SetBreakpointType(modelItem, BreakpointTypes.None); // clear breakpoint
                }

                DeleteFromMapping(modelItem.GetCurrentValue());
            }
        }

        // Delete a single object from the mappings.
        // We only delete unresolved object, i.e. object that never been 
        // saved.  We leave the resolved object untouch (otherwise undoing
        // the removed object will make it as "unresolved" the next time around).
        void DeleteFromMapping(object unresolvedObject)
        {
            SourceLocation sourceLocation;
            if (this.instanceToSourceLocationMapping.TryGetValue(unresolvedObject, out sourceLocation))
            {
                if (IsUnresolved(sourceLocation))
                {
                    this.instanceToSourceLocationMapping.Remove(unresolvedObject);
                    ModelItem modelItem;
                    if (this.sourceLocationToModelItem.TryGetValue(sourceLocation, out modelItem))
                    {
                        this.sourceLocationToModelItem.Remove(sourceLocation);
                        this.modelItemToSourceLocation.Remove(modelItem);
                    }
                }
            }
        }

        bool IsCurrentLocation(ModelItem modelItem)
        {
            UpdateCurrentModelItem();
            return this.currentModelItem == modelItem;
        }

        bool IsCurrentContext(ModelItem modelItem)
        {
            UpdateCurrentModelItemContext();
            return this.currentModelItemContext == modelItem;
        }

        void SelectionChanged(Selection selection)
        {
            this.selectedModelItem = selection.PrimarySelection;
        }

        // Check if unmapped breakpoint exists for the given sourceLocation,
        // if so, mapped it to the given model item & remove it from unmapped breakpoints.
        void TryActivateUnmappedBreakpoint(SourceLocation sourceLocation, ModelItem modelItem)
        {
            BreakpointTypes breakpointType;
            if (this.unmappedBreakpoints.TryGetValue(sourceLocation, out breakpointType))
            {
                this.SetBreakpointType(modelItem, breakpointType);
                this.unmappedBreakpoints.Remove(sourceLocation);
            }
        }

        void TryActivateAllUnmappedBreakpoints()
        {
            if (this.unmappedBreakpoints.Count > 0)
            {
                List<SourceLocation> unmappedLocations = new List<SourceLocation>();
                unmappedLocations.AddRange(this.unmappedBreakpoints.Keys);
                foreach (SourceLocation unmappedLocation in unmappedLocations)
                {
                    ModelItem modelItem = this.GetModelItemFromSourceLocation(unmappedLocation);
                    if (modelItem != null)
                    {
                        TryActivateUnmappedBreakpoint(unmappedLocation, modelItem);
                    }
                }
            }
        }

        bool IsBreakpointOfType(ModelItem modelItem, BreakpointTypes breakpointType)
        {
            bool result = false;
            BreakpointTypes actualBreakpointType;
            TryActivateAllUnmappedBreakpoints();
            if (this.breakpoints.TryGetValue(modelItem, out actualBreakpointType))
            {
                result = (actualBreakpointType & breakpointType) > 0;
            }
            return result;
        }

        void SetBreakpointType(ModelItem modelItem, BreakpointTypes newBreakpointType)
        {
            BreakpointTypes oldBreakpointType = BreakpointTypes.None;
            if (this.breakpoints.TryGetValue(modelItem, out oldBreakpointType))
            {
                Fx.Assert(oldBreakpointType != BreakpointTypes.None, "Should not store BreakpointType.None");
                if (newBreakpointType == BreakpointTypes.None)
                {
                    this.breakpoints.Remove(modelItem);
                }
                else
                {
                    this.breakpoints[modelItem] = newBreakpointType;
                }
            }
            else if (newBreakpointType != BreakpointTypes.None)
            {
                this.breakpoints.Add(modelItem, newBreakpointType);
            }

            // Now notifying corresponding properties.
            if ((oldBreakpointType & BreakpointTypes.Bounded) !=
                (newBreakpointType & BreakpointTypes.Bounded))
            {
                this.isBreakpointBoundedProperty.NotifyPropertyChanged(modelItem);
            }

            if ((oldBreakpointType & BreakpointTypes.Enabled) !=
                (newBreakpointType & BreakpointTypes.Enabled))
            {
                this.isBreakpointEnabledProperty.NotifyPropertyChanged(modelItem);
            }

            if ((oldBreakpointType & BreakpointTypes.Conditional) !=
                (newBreakpointType & BreakpointTypes.Conditional))
            {
                this.isBreakpointConditionalProperty.NotifyPropertyChanged(modelItem);
            }
        }

        // Return exact source location given approximate location.
        public SourceLocation GetExactLocation(SourceLocation approximateLocation)
        {
            this.EnsureSourceLocationUpdated();

            if (approximateLocation == null)
            {
                throw FxTrace.Exception.ArgumentNull("approximateLocation");
            }

            SourceLocation exactLocation = null;

            foreach (SourceLocation sourceLocation in this.instanceToSourceLocationMapping.Values)
            {
                if (sourceLocation.StartLine == approximateLocation.StartLine)
                {
                    exactLocation = sourceLocation;
                    break;
                }
            }

            if (exactLocation == null)
            {
                exactLocation = FindClosestSourceLocation(approximateLocation, this.instanceToSourceLocationMapping.Values);
            }

            return exactLocation;
        }

        // This method tries to find the inner most outer source location from a list
        // The outer source locations of a source location is ones that contains it
        // The inner most outer source location is the one nested most deeply, right outside of the source location being contained.
        private static SourceLocation FindInnerMostContainer(SourceLocation approximateLocation, IEnumerable<SourceLocation> availableSourceLocations)
        {
            Fx.Assert(approximateLocation != null && availableSourceLocations != null, "Argument should not be null");

            SourceLocation innerMostOuterSourceLocation = null;

            foreach (SourceLocation sourceLocation in availableSourceLocations)
            {
                if (sourceLocation.Contains(approximateLocation))
                {
                    if (innerMostOuterSourceLocation == null)
                    {
                        innerMostOuterSourceLocation = sourceLocation;
                    }
                    else
                    {
                        if (innerMostOuterSourceLocation.Contains(sourceLocation))
                        {
                            innerMostOuterSourceLocation = sourceLocation;
                        }
                    }
                }
            }

            return innerMostOuterSourceLocation;
        }

        internal static SourceLocation FindClosestSourceLocation(SourceLocation approximateLocation, IEnumerable<SourceLocation> availableSourceLocations)
        {
            Fx.Assert(approximateLocation != null && availableSourceLocations != null, "Argument should not be null");

            SourceLocation exactLocation = null;
            SourceLocation innerMostOuterSourceLocation =
                FindInnerMostContainer(approximateLocation, availableSourceLocations);

            if (innerMostOuterSourceLocation != null)
            {
                exactLocation = innerMostOuterSourceLocation;
            }
            else
            {
                // Find the next line of the approximateLocation.
                int minimumDistance = int.MaxValue;
                foreach (SourceLocation sourceLocation in availableSourceLocations)
                {
                    int lineDistance = sourceLocation.StartLine - approximateLocation.StartLine;
                    if ((lineDistance > 0) &&
                        ((lineDistance < minimumDistance) ||
                         ((lineDistance == minimumDistance) && (sourceLocation.StartColumn < exactLocation.StartColumn))))  // if same distance, then compare the start column
                    {
                        exactLocation = sourceLocation;
                        minimumDistance = lineDistance;
                    }
                }
            }

            return exactLocation;
        }

        // Called after a Save by AddIn to update breakpoints with new locations 
        public IDictionary<SourceLocation, BreakpointTypes> GetBreakpointLocations()
        {
            IDictionary<SourceLocation, BreakpointTypes> breakpointLocations = new Dictionary<SourceLocation, BreakpointTypes>();

            // Collect source locations of model items with breakpoints
            if (this.breakpoints.Count > 0 || this.unmappedBreakpoints.Count > 0)
            {
                foreach (KeyValuePair<ModelItem, BreakpointTypes> entry in this.breakpoints)
                {
                    SourceLocation breakpointLocation = this.GetSourceLocationFromModelItem(entry.Key);
                    // BreakpointLocation can be null, if the model item is deleted but without notification
                    // through OnModelChanged.  This happens when the breakpoint is located inside child
                    // of a deleted object.
                    if (breakpointLocation != null)
                    {
                        breakpointLocations.Add(breakpointLocation, entry.Value);
                    }
                }
                foreach (KeyValuePair<SourceLocation, BreakpointTypes> entry in this.unmappedBreakpoints)
                {
                    breakpointLocations.Add(entry.Key, entry.Value);
                }
            }
            return breakpointLocations;
        }

        // Inserting a new breakpoint of a given type.
        public void InsertBreakpoint(SourceLocation sourceLocation, BreakpointTypes breakpointType)
        {
            this.UpdateBreakpoint(sourceLocation, breakpointType);
        }

        // Update the appearance of a given breakpoint to show the given type.
        public void UpdateBreakpoint(SourceLocation sourceLocation, BreakpointTypes newBreakpointType)
        {
            ModelItem modelItem = this.GetModelItemFromSourceLocation(sourceLocation);
            if (modelItem != null)
            {
                SetBreakpointType(modelItem, newBreakpointType);
            }
            else
            {
                BreakpointTypes oldBreakpointType;
                if (this.unmappedBreakpoints.TryGetValue(sourceLocation, out oldBreakpointType))
                {
                    if (newBreakpointType == BreakpointTypes.None)
                    {
                        this.unmappedBreakpoints.Remove(sourceLocation);
                    }
                    else
                    {
                        this.unmappedBreakpoints[sourceLocation] = newBreakpointType;
                    }
                }
                else if (newBreakpointType != BreakpointTypes.None)
                {
                    this.unmappedBreakpoints.Add(sourceLocation, newBreakpointType);
                }
            }
        }

        // Delete a breakpoint.
        public void DeleteBreakpoint(SourceLocation sourceLocation)
        {
            UpdateBreakpoint(sourceLocation, BreakpointTypes.None);
        }

        // Reset breakpoints: delete and prepare for breakpoint refresh.
        public void ResetBreakpoints()
        {
            ModelItem[] oldModelItems = new ModelItem[this.breakpoints.Keys.Count];
            this.breakpoints.Keys.CopyTo(oldModelItems, 0);
            this.breakpoints.Clear();
            this.unmappedBreakpoints.Clear();

            // Now notifying update to corresponding properties.
            foreach (ModelItem modelItem in oldModelItems)
            {
                this.isBreakpointBoundedProperty.NotifyPropertyChanged(modelItem);
                this.isBreakpointEnabledProperty.NotifyPropertyChanged(modelItem);
                this.isBreakpointConditionalProperty.NotifyPropertyChanged(modelItem);
            }
        }

        public void UpdateSourceLocations(Dictionary<object, SourceLocation> newSourceLocationMapping)
        {
            if (newSourceLocationMapping == null)
            {
                throw FxTrace.Exception.ArgumentNull("newSourceLocationMapping");
            }

            // Update unmappedBreakpoints before refreshing the instanceToSourceLocationMapping.
            if (this.unmappedBreakpoints.Count > 0)
            {
                Dictionary<SourceLocation, BreakpointTypes> newUnmappedBreakpoints = new Dictionary<SourceLocation, BreakpointTypes>(this.unmappedBreakpoints.Count);
                foreach (KeyValuePair<object, SourceLocation> kvpEntry in this.instanceToSourceLocationMapping)
                {
                    if (this.unmappedBreakpoints.ContainsKey(kvpEntry.Value))
                    {
                        if (newSourceLocationMapping.ContainsKey(kvpEntry.Key))
                        {
                            newUnmappedBreakpoints.Add(newSourceLocationMapping[kvpEntry.Key], this.unmappedBreakpoints[kvpEntry.Value]);
                        }
                    }
                }
                this.unmappedBreakpoints = newUnmappedBreakpoints;
            }

            // It is possible that after InvalidateSourceLocationMapping, before UpdateSourceLocations, we introduced new unresolvedEntries. 
            // These entries should not be dropped, or we will not be able to add breakpoint before UpdateSourceLocation.
            List<KeyValuePair<object, SourceLocation>> unresolvedEntries = this.instanceToSourceLocationMapping.Where(entry => IsUnresolved(entry.Value)).ToList();

            this.instanceToSourceLocationMapping = newSourceLocationMapping;
            this.sourceLocationToModelItem.Clear();
            this.modelItemToSourceLocation.Clear();
            this.transientBreakpoints.Clear();

            if (this.modelTreeManager != null)
            {
                foreach (KeyValuePair<object, SourceLocation> kvp in newSourceLocationMapping)
                {
                    ModelItem modelItem = this.modelTreeManager.GetModelItem(kvp.Key);
                    if (modelItem != null)
                    {
                        SourceLocation sourceLocation = kvp.Value;
                        this.modelItemToSourceLocation.Add(modelItem, sourceLocation);
                        this.sourceLocationToModelItem.Add(sourceLocation, modelItem);
                    }
                }

                foreach (KeyValuePair<object, SourceLocation> unresolvedEntry in unresolvedEntries)
                {
                    object unresolvedObject = unresolvedEntry.Key;
                    SourceLocation sourceLocation = unresolvedEntry.Value;
                    if (!this.instanceToSourceLocationMapping.ContainsKey(unresolvedObject))
                    {
                        this.instanceToSourceLocationMapping.Add(unresolvedObject, sourceLocation);
                        ModelItem modelItem = this.modelTreeManager.GetModelItem(unresolvedObject);
                        if (modelItem != null)
                        {
                            this.modelItemToSourceLocation.Add(modelItem, sourceLocation);
                            this.sourceLocationToModelItem.Add(sourceLocation, modelItem);
                        }
                    }
                }
            }

            TryActivateAllUnmappedBreakpoints();
        }

        // Called by View Service when a new view element is created 
        private void ViewCreated(object sender, ViewCreatedEventArgs e)
        {
            if (e.View != null)
            {
                ModelItem modelItem = e.View.ModelItem;
                object addedObject = modelItem.GetCurrentValue();

                // Create a mapping between SourceLocation and this View Element
                SourceLocation sourceLocation = this.GetSourceLocationFromModelItemInstance(addedObject);
                if (sourceLocation == null)
                {
                    // The current view element has not been saved yet to the Xaml file
                    sourceLocation = GenerateUnresolvedLocation();
                    this.instanceToSourceLocationMapping.Add(addedObject, sourceLocation);
                }

                this.modelItemToSourceLocation[modelItem] = sourceLocation;
                this.sourceLocationToModelItem[sourceLocation] = modelItem;

                BreakpointTypes breakpointType;
                // check if it's in the transient breakpoint list.
                if (this.transientBreakpoints.TryGetValue(modelItem, out breakpointType))
                {
                    this.transientBreakpoints.Remove(modelItem);
                    SetBreakpointType(modelItem, breakpointType);
                }
                else
                {
                    TryActivateUnmappedBreakpoint(sourceLocation, modelItem);
                }
            }
        }

        private SourceLocation GenerateUnresolvedLocation()
        {
            return new SourceLocation(unresolvedPrefix + this.context.Items.GetValue<WorkflowFileItem>().LoadedFile, this.lastSourceLineNo++);
        }

        private static bool IsUnresolved(SourceLocation sourceLocation)
        {
            return !string.IsNullOrEmpty(sourceLocation.FileName) && sourceLocation.FileName.StartsWith(unresolvedPrefix, StringComparison.OrdinalIgnoreCase);
        }

        // This method is called during Load/Save - the resolved mapping should be invalidated.
        internal void InvalidateSourceLocationMapping(string fileName)
        {
            this.fileName = fileName;
            this.requiresUpdateSourceLocation = true;

            // Remove, from the SourceLocationMappings, the entries with resolved SourceLocation - they are no longer valid and should be refreshed.
            List<KeyValuePair<ModelItem, SourceLocation>> resolvedEntries = this.modelItemToSourceLocation.Where(entry => !IsUnresolved(entry.Value)).ToList();
            foreach (KeyValuePair<ModelItem, SourceLocation> resolvedEntry in resolvedEntries)
            {
                this.modelItemToSourceLocation.Remove(resolvedEntry.Key);
                this.sourceLocationToModelItem.Remove(resolvedEntry.Value);
                this.instanceToSourceLocationMapping.Remove(resolvedEntry.Key.GetCurrentValue());
            }

            // All breakpoint should simply stay - unmappedBreakpoint will get updated to newSourceLocation when we have the newSourceLocation.
        }

        private void EnsureSourceLocationUpdated()
        {
            Fx.Assert(this.modelSearchService != null, "ModelSearchService should be available and is ensured in WorkflowDesigner constructor");
            if (this.requiresUpdateSourceLocation)
            {
                Dictionary<object, SourceLocation> updatedSourceLocations = new Dictionary<object, SourceLocation>();
                foreach (ModelItem key in this.modelSearchService.GetObjectsWithSourceLocation())
                {
                    // disallow expressions
                    if (AllowBreakpointAttribute.IsBreakpointAllowed(key.ItemType) && !typeof(IValueSerializableExpression).IsAssignableFrom(key.ItemType))
                    {
                        SourceLocation sourceLocationWithoutFileName = this.modelSearchService.FindSourceLocation(key);

                        // Appending the fileName
                        SourceLocation sourceLocationWithFileName = new SourceLocation(this.fileName,
                            sourceLocationWithoutFileName.StartLine,
                            sourceLocationWithoutFileName.StartColumn,
                            sourceLocationWithoutFileName.EndLine,
                            sourceLocationWithoutFileName.EndColumn);
                        updatedSourceLocations.Add(key.GetCurrentValue(), sourceLocationWithFileName);
                    }
                }

                this.UpdateSourceLocations(updatedSourceLocations);
                this.requiresUpdateSourceLocation = false;
            }
        }

        private void OnModelSearchServiceAvailable(ModelSearchService modelSearchService)
        {
            this.modelSearchService = (ModelSearchServiceImpl)modelSearchService;
        }

        private void OnViewServiceAvailable(ViewService viewService)
        {
            this.viewService = (WorkflowViewService)viewService;
            this.viewService.ViewCreated += this.ViewCreated;
        }

        private SourceLocation GetSourceLocationFromModelItemInstance(object instance)
        {
            SourceLocation sourceLocation;

            // instanceToSourceLocationMapping contains source locations for all instances 
            // immediately after a Load or save.  For instances that have been just dropped into
            // the Designer from the Toolbox, we want to return null from here and treat them
            // as "Unresolved" in the caller.
            if (this.instanceToSourceLocationMapping.TryGetValue(instance, out sourceLocation))
            {
                return sourceLocation;
            }
            else
            {
                return null;
            }
        }

        private void SetPropertyValue(ModelItem oldModelItem, AttachedProperty property, ModelItem newModelItem)
        {
            // update the previous ModelItem (what was current before)
            if (oldModelItem != null)
            {
                property.NotifyPropertyChanged(oldModelItem);
            }

            // update the current Modelitem
            if (newModelItem != null)
            {
                property.NotifyPropertyChanged(newModelItem);
            }
        }

        private SourceLocation GetSourceLocationFromModelItem(ModelItem modelItem)
        {
            this.EnsureSourceLocationUpdated();
            SourceLocation sourceLocation = null;
            if (modelItem != null)
            {
                this.modelItemToSourceLocation.TryGetValue(modelItem, out sourceLocation);
            }
            return sourceLocation;
        }

        private ModelItem GetModelItemFromSourceLocation(SourceLocation sourceLocation)
        {
            return GetModelItemFromSourceLocation(sourceLocation, /* forceCreate = */ false);
        }

        private ModelItem GetModelItemFromSourceLocation(SourceLocation sourceLocation, bool forceCreate)
        {
            ModelItem modelItem = null;
            if (sourceLocation != null)
            {
                if (!this.sourceLocationToModelItem.TryGetValue(sourceLocation, out modelItem))
                {
                    if (forceCreate)
                    {
                        object foundElement = null;
                        foreach (KeyValuePair<object, SourceLocation> kvp in this.instanceToSourceLocationMapping)
                        {
                            if (kvp.Value.Equals(sourceLocation))
                            {
                                foundElement = kvp.Key;
                                break;
                            }
                        }

                        if (foundElement != null)
                        {
                            modelItem = Validation.ValidationService.FindModelItem(this.modelTreeManager, foundElement);
                            
                            if (modelItem != null)
                            {
                                this.modelItemToSourceLocation.Add(modelItem, sourceLocation);
                                this.sourceLocationToModelItem.Add(sourceLocation, modelItem);
                            }
                        }
                    }
                }
            }

            return modelItem;
        }
    }
}
