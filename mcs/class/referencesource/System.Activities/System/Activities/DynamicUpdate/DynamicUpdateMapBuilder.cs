// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Activities.Validation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Runtime;
    using System.Runtime.CompilerServices;

    public class DynamicUpdateMapBuilder
    {
        private HashSet<Activity> disallowUpdateInside;

        public DynamicUpdateMapBuilder()
        {
        }

        public bool ForImplementation 
        { 
            get; 
            set; 
        }

        public ISet<Activity> DisallowUpdateInside
        {
            get
            {
                if (this.disallowUpdateInside == null)
                {
                    this.disallowUpdateInside = new HashSet<Activity>(ReferenceEqualityComparer.Instance);
                }

                return this.disallowUpdateInside;
            }
        }

        public Func<object, DynamicUpdateMapItem> LookupMapItem
        {
            get;
            set;
        }

        public Func<Activity, DynamicUpdateMap> LookupImplementationMap 
        { 
            get; 
            set; 
        }

        public LocationReferenceEnvironment UpdatedEnvironment
        {
            get;
            set;
        }

        public Activity UpdatedWorkflowDefinition
        {
            get;
            set;
        }

        public LocationReferenceEnvironment OriginalEnvironment
        {
            get;
            set;
        }

        public Activity OriginalWorkflowDefinition
        {
            get;
            set;
        }

        // Internal hook to allow DynamicUpdateServices to surface a customized error message when
        // there is an invalid activity in the disallowUpdateInsideActivities list
        internal Func<Activity, Exception> OnInvalidActivityToBlockUpdate
        {
            get;
            set;
        }

        // Internal hook to allow DynamicUpdateServices to surface a customized error message when
        // there is an invalid activity in the disallowUpdateInsideActivities list
        internal Func<Activity, Exception> OnInvalidImplementationMapAssociation
        {
            get;
            set;
        }

        public DynamicUpdateMap CreateMap()
        {
            IList<ActivityBlockingUpdate> activitiesBlockingUpdate;
            return CreateMap(out activitiesBlockingUpdate);
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.AvoidOutParameters, Justification = "Approved Design. Need to return the map and the block list.")]
        public DynamicUpdateMap CreateMap(out IList<ActivityBlockingUpdate> activitiesBlockingUpdate)
        {
            RequireProperty(this.LookupMapItem, "LookupMapItem");
            RequireProperty(this.UpdatedWorkflowDefinition, "UpdatedWorkflowDefinition");
            RequireProperty(this.OriginalWorkflowDefinition, "OriginalWorkflowDefinition");

            Finalizer finalizer = new Finalizer(this);
            DynamicUpdateMap result = finalizer.FinalizeUpdate(out activitiesBlockingUpdate);
            return result;
        }

        private static void CacheMetadata(Activity workflowDefinition, LocationReferenceEnvironment environment, ActivityUtilities.ProcessActivityCallback callback, bool forImplementation)
        {
            IList<ValidationError> validationErrors = null;
            ActivityUtilities.CacheRootMetadata(workflowDefinition, environment, ProcessTreeOptions(forImplementation), callback, ref validationErrors);
            ActivityValidationServices.ThrowIfViolationsExist(validationErrors);
        }

        static DynamicUpdateMapEntry GetParentEntry(Activity originalActivity, DynamicUpdateMap updateMap)
        {
            if (originalActivity.Parent != null && originalActivity.Parent.MemberOf == originalActivity.MemberOf)
            {
                DynamicUpdateMapEntry parentEntry;
                updateMap.TryGetUpdateEntry(originalActivity.Parent.InternalId, out parentEntry);
                Fx.Assert(parentEntry != null, "We process in IdSpace order, so we always process parents before their children");
                return parentEntry;
            }
            return null;
        }

        static IEnumerable<Activity> GetPublicDeclaredChildren(Activity activity, bool includeExpressions)
        {
            IEnumerable<Activity> result = activity.Children.Concat(
                activity.ImportedChildren).Concat(
                activity.Delegates.Select(d => d.Handler)).Concat(
                activity.ImportedDelegates.Select(d => d.Handler));
            if (includeExpressions)
            {
                result = result.Concat(
                    activity.RuntimeVariables.Select(v => v.Default)).Concat(
                    activity.RuntimeArguments.Select(a => a.IsBound ? a.BoundArgument.Expression : null));
            }

            return result.Where(a => a != null && a.Parent == activity);
        }

        private static ProcessActivityTreeOptions ProcessTreeOptions(bool forImplementation)
        {
            return forImplementation ? ProcessActivityTreeOptions.DynamicUpdateOptionsForImplementation : ProcessActivityTreeOptions.DynamicUpdateOptions;
        }

        private static void RequireProperty(object value, string name)
        {
            if (value == null)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.UpdateMapBuilderRequiredProperty(name)));
            }
        }

        internal class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly IEqualityComparer<object> Instance = new ReferenceEqualityComparer();

            ReferenceEqualityComparer()
            {
            }

            public new bool Equals(object x, object y)
            {
                return object.ReferenceEquals(x, y);
            }

            public int GetHashCode(object obj)
            {
                return RuntimeHelpers.GetHashCode(obj);
            }
        }

        // Preparer walks the tree and identifies, for each object in the tree, an ID that can be
        // attached to an object in the new definition to match it to the equivalent object in the
        // old definition.
        internal class Preparer
        {
            private Dictionary<object, DynamicUpdateMapItem> updateableObjects;
            private Activity originalProgram;
            private LocationReferenceEnvironment originalEnvironment;
            private bool forImplementation;

            public Preparer(Activity originalProgram, LocationReferenceEnvironment originalEnvironment, bool forImplementation)
            {
                this.originalProgram = originalProgram;
                this.originalEnvironment = originalEnvironment;
                this.forImplementation = forImplementation;
            }

            public Dictionary<object, DynamicUpdateMapItem> Prepare()
            {
                this.updateableObjects = new Dictionary<object, DynamicUpdateMapItem>(ReferenceEqualityComparer.Instance);
                CacheMetadata(this.originalProgram, this.originalEnvironment, null, this.forImplementation);

                IdSpace idSpace = GetIdSpace();
                if (idSpace != null)
                {
                    for (int i = 1; i <= idSpace.MemberCount; i++)
                    {
                        ProcessElement(idSpace[i]);
                    }
                }

                return this.updateableObjects;
            }

            IdSpace GetIdSpace()
            {
                return this.forImplementation ? this.originalProgram.ParentOf : this.originalProgram.MemberOf;
            }

            void ProcessElement(Activity currentElement)
            {
                // Attach the original Activity ID to the activity
                // The origin of a variable default is the same as the origin of the variable itself.
                // So we don't attach match info for the default, since that would conflict with 
                // the match info for the variable.
                if (currentElement.RelationshipToParent != Activity.RelationshipType.VariableDefault || currentElement.Origin == null)
                {
                    ValidateOrigin(currentElement.Origin, currentElement);
                    this.updateableObjects[currentElement.Origin ?? currentElement] = new DynamicUpdateMapItem(currentElement.InternalId);
                }

                // Attach the original variable index to the variable
                IList<Variable> variables = currentElement.RuntimeVariables;
                for (int i = 0; i < variables.Count; i++)
                {
                    Variable variable = variables[i];
                    if (string.IsNullOrEmpty(variable.Name))
                    {
                        ValidateOrigin(variable.Origin, variable);
                        this.updateableObjects[variable.Origin ?? variable] = new DynamicUpdateMapItem(currentElement.InternalId, i);
                    }                    
                }
            }

            void ValidateOrigin(object origin, object element)
            {
                if (origin != null)
                {
                    DynamicUpdateMapItem mapItem;
                    if (this.updateableObjects.TryGetValue(origin, out mapItem))
                    {
                        string error = null;
                        if (mapItem.IsVariableMapItem)
                        {
                            Variable dupe = GetVariable(mapItem);
                            Variable elementVar = element as Variable;
                            if (elementVar != null)
                            {
                                error = SR.DuplicateOriginVariableVariable(origin, dupe.Name, elementVar.Name);
                            }
                            else
                            {
                                error = SR.DuplicateOriginActivityVariable(origin, element, dupe.Name);
                            }
                        }
                        else
                        {
                            Activity dupe = GetActivity(mapItem);
                            Variable elementVar = element as Variable;
                            if (elementVar != null)
                            {
                                error = SR.DuplicateOriginActivityVariable(origin, dupe, elementVar.Name);
                            }
                            else
                            {
                                error = SR.DuplicateOriginActivityActivity(origin, dupe, element);
                            }
                        }

                        throw FxTrace.Exception.AsError(new InvalidWorkflowException(error));
                    }
                }
            }

            Activity GetActivity(DynamicUpdateMapItem mapItem)
            {
                return GetIdSpace()[mapItem.OriginalId];
            }

            Variable GetVariable(DynamicUpdateMapItem mapItem)
            {
                return GetIdSpace()[mapItem.OriginalVariableOwnerId].RuntimeVariables[mapItem.OriginalId];
            }
        }

        // Builds an Update Map given an old and new definition, and matches between them
        internal class Finalizer
        {
            BitArray foundOriginalElements;
            DynamicUpdateMapBuilder builder;
            DynamicUpdateMap updateMap;
            Dictionary<Activity, object> savedOriginalValues;
            bool savedOriginalValuesForReferencedChildren;
            IList<ActivityBlockingUpdate> blockList;
            // dictionary from expression root to the activity that can make it go idle
            Dictionary<Activity, Activity> expressionRootsThatCanInduceIdle;

            public Finalizer(DynamicUpdateMapBuilder builder)
            {
                this.builder = builder;
                this.savedOriginalValues = new Dictionary<Activity, object>(ReferenceEqualityComparer.Instance);
                this.Matcher = new DefinitionMatcher(builder.LookupMapItem);
            }

            public DynamicUpdateMap FinalizeUpdate(out IList<ActivityBlockingUpdate> blockList)
            {
                this.updateMap = new DynamicUpdateMap();
                this.blockList = new List<ActivityBlockingUpdate>();

                // cache metadata of originalProgram
                CacheMetadata(this.builder.OriginalWorkflowDefinition, this.builder.OriginalEnvironment, null, this.builder.ForImplementation);
                IdSpace originalIdSpace = this.builder.ForImplementation ? this.builder.OriginalWorkflowDefinition.ParentOf : this.builder.OriginalWorkflowDefinition.MemberOf;
                if (originalIdSpace == null)
                {
                    Fx.Assert(this.builder.ForImplementation, "An activity must be a member of an IdSpace");
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidOriginalWorkflowDefinitionForImplementationMapCreation));
                }
                this.Matcher.OldIdSpace = originalIdSpace;
                this.foundOriginalElements = new BitArray(originalIdSpace.MemberCount);

                // cache metadata of modifiedProgram before iterative ProcessElement()
                CacheMetadata(this.builder.UpdatedWorkflowDefinition, this.builder.UpdatedEnvironment, CheckCanArgumentOrVariableDefaultInduceIdle, this.builder.ForImplementation);
                IdSpace idSpace = this.builder.ForImplementation ? this.builder.UpdatedWorkflowDefinition.ParentOf : this.builder.UpdatedWorkflowDefinition.MemberOf;
                if (idSpace == null)
                {
                    Fx.Assert(this.builder.ForImplementation, "An activity must be a member of an IdSpace");
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidUpdatedWorkflowDefinitionForImplementationMapCreation));
                }
                this.Matcher.NewIdSpace = idSpace;

                // check if any of the activities or variables from the original definition 
                // were reused in the updated definition
                for (int i = 1; i < originalIdSpace.MemberCount + 1; i++)
                {
                    CheckForReusedActivity(originalIdSpace[i]);
                }
                
                // most of the updatemap construction processing                 
                for (int i = 1; i < idSpace.MemberCount + 1; i++)
                {
                    ProcessElement(idSpace[i]);
                }

                // if an activity doesn't have an entry by this point, that means it was removed
                for (int i = 0; i < this.foundOriginalElements.Count; i++)
                {
                    if (!this.foundOriginalElements[i])
                    {
                        DynamicUpdateMapEntry removalEntry = new DynamicUpdateMapEntry(i + 1, 0);
                        Activity originalActivity = originalIdSpace[i + 1];
                        removalEntry.Parent = GetParentEntry(originalActivity, this.updateMap);
                        if (!removalEntry.IsParentRemovedOrBlocked)
                        {
                            removalEntry.DisplayName = originalActivity.DisplayName;
                        }
                        this.updateMap.AddEntry(removalEntry);
                    }
                }

                if (this.builder.ForImplementation)
                {
                    this.updateMap.IsForImplementation = true;

                    // gather arguments diff between new and old activity definitions
                    this.updateMap.OldArguments = ArgumentInfo.List(builder.OriginalWorkflowDefinition);
                    this.updateMap.NewArguments = ArgumentInfo.List(builder.UpdatedWorkflowDefinition);
                }

                // Validate the Disallow entries
                foreach (Activity disallowActivity in this.builder.DisallowUpdateInside)
                {
                    if (disallowActivity == null)
                    {
                        continue;
                    }

                    if (disallowActivity.MemberOf != idSpace)
                    {
                        ThrowInvalidActivityToBlockUpdate(disallowActivity);
                    }
                }

                this.updateMap.NewDefinitionMemberCount = idSpace.MemberCount;
                blockList = this.blockList;
                return this.updateMap;
            }

            internal bool? AllowUpdateInsideCurrentActivity
            {
                get;
                set;
            }

            internal string UpdateDisallowedReason
            {
                get;
                set;
            }

            internal Dictionary<string, object> SavedOriginalValuesForCurrentActivity
            {
                get;
                set;
            }

            internal DefinitionMatcher Matcher
            {
                get;
                private set;
            }

            internal Dictionary<Activity, Activity> ExpressionRootsThatCanInduceIdle
            {
                get
                {
                    return this.expressionRootsThatCanInduceIdle;
                }
            }

            void BlockUpdate(Activity activity, UpdateBlockedReason reason, DynamicUpdateMapEntry entry, string message = null)
            {
                Fx.Assert(activity.MemberOf == (this.builder.ForImplementation ? activity.RootActivity.ParentOf : activity.RootActivity.MemberOf), "Should have called other overload of BlockUpdate");
                BlockUpdate(activity, entry.OldActivityId.ToString(CultureInfo.InvariantCulture), reason, entry, message);
            }

            internal void BlockUpdate(Activity activity, string originalActivityId, UpdateBlockedReason reason, DynamicUpdateMapEntry entry, string message = null)
            {
                Fx.Assert(reason != UpdateBlockedReason.NotBlocked, "Invalid block reason");
                if (!entry.IsRuntimeUpdateBlocked)
                {
                    entry.BlockReason = reason;
                    if (reason == UpdateBlockedReason.Custom)
                    {
                        entry.BlockReasonMessage = message;
                    }
                    entry.ImplementationUpdateMap = null;

                    this.blockList.Add(new ActivityBlockingUpdate(activity, originalActivityId, message ?? UpdateBlockedReasonMessages.Get(reason)));
                }
            }

            internal void SetOriginalValue(Activity key, object value, bool isReferencedChild)
            {
                if (isReferencedChild)
                {
                    this.savedOriginalValuesForReferencedChildren = true;
                }
                else
                {
                    this.savedOriginalValues[key] = value;
                }
            }

            internal object GetSavedOriginalValueFromParent(Activity key)
            {
                object result = null;
                this.savedOriginalValues.TryGetValue(key, out result);
                return result;
            }

            void ProcessElement(Activity currentElement)
            {
                Activity originalElement = this.Matcher.GetMatch(currentElement);
                if (originalElement != null)
                {
                    // this means it's an existing one                                      

                    DynamicUpdateMapEntry mapEntry = this.CreateMapEntry(currentElement, originalElement);
                    mapEntry.Parent = GetParentEntry(originalElement, this.updateMap);
                    if (this.builder.DisallowUpdateInside.Contains(currentElement))
                    {
                        mapEntry.IsUpdateBlockedByUpdateAuthor = true;
                    }
                    if (originalElement.GetType() != currentElement.GetType())
                    {
                        // returned matching activity's type doesn't really match the currentElement
                        BlockUpdate(currentElement, UpdateBlockedReason.TypeChange, mapEntry,
                            SR.DUActivityTypeMismatch(currentElement.GetType(), originalElement.GetType()));
                    }
                    if (this.DelegateArgumentsChanged(currentElement, originalElement))
                    {
                        this.BlockUpdate(currentElement, UpdateBlockedReason.DelegateArgumentChange, mapEntry);
                    }

                    DynamicUpdateMap implementationMap = null;
                    if (this.builder.LookupImplementationMap != null)
                    {
                        implementationMap = this.builder.LookupImplementationMap(currentElement);
                    }

                    // fill ArgumentEntries
                    // get arguments diff info from implementation map if it exists
                    // we do this before user participation, so that we don't call into user code
                    // if the update is invalid
                    IList<ArgumentInfo> oldArguments = GetOriginalArguments(mapEntry, implementationMap, currentElement, originalElement);
                    if (oldArguments != null)
                    {
                        CreateArgumentEntries(mapEntry, currentElement.RuntimeArguments, oldArguments);
                    }

                    // Capture any saved original value associated with this activity by its parent
                    mapEntry.SavedOriginalValueFromParent = GetSavedOriginalValueFromParent(currentElement);

                    if (mapEntry.IsRuntimeUpdateBlocked)
                    {
                        // don't allow activity to participate if update isn't possible anyway
                        mapEntry.EnvironmentUpdateMap = null;
                        return;
                    }

                    OnCreateDynamicUpdateMap(currentElement, originalElement, mapEntry, this.Matcher);
                    if (mapEntry.IsRuntimeUpdateBlocked)
                    {
                        // if the activity disabled update, we can't rely on the variable matches,
                        // so no point in proceeding
                        mapEntry.EnvironmentUpdateMap = null;
                        return;
                    }

                    // variable entries need to be calculated after activity participation, since
                    // the activity can participate in matching them
                    CreateVariableEntries(false, mapEntry, currentElement.RuntimeVariables, originalElement.RuntimeVariables, originalElement);
                    CreateVariableEntries(true, mapEntry, currentElement.ImplementationVariables, originalElement.ImplementationVariables, originalElement);

                    if (mapEntry.HasEnvironmentUpdates)
                    {
                        FillEnvironmentMapMemberCounts(mapEntry.EnvironmentUpdateMap, currentElement, originalElement, oldArguments);
                    }
                    else
                    {
                        Fx.Assert(originalElement.SymbolCount == currentElement.SymbolCount || 
                            originalElement.ImplementationVariables.Count != currentElement.ImplementationVariables.Count,
                            "Should have environment update if symbol count changed");
                    }

                    if (!mapEntry.IsParentRemovedOrBlocked && !mapEntry.IsUpdateBlockedByUpdateAuthor)
                    {
                        NestedIdSpaceFinalizer nestedFinalizer = new NestedIdSpaceFinalizer(this, implementationMap, currentElement, originalElement, null);
                        nestedFinalizer.ValidateOrCreateImplementationMap(mapEntry);
                    }
                }
            }

            internal static void FillEnvironmentMapMemberCounts(EnvironmentUpdateMap envMap, Activity currentElement, Activity originalElement, IList<ArgumentInfo> oldArguments)
            {
                envMap.NewVariableCount = currentElement.RuntimeVariables != null ? currentElement.RuntimeVariables.Count : 0;
                envMap.NewPrivateVariableCount = currentElement.ImplementationVariables != null ? currentElement.ImplementationVariables.Count : 0;
                envMap.NewArgumentCount = currentElement.RuntimeArguments != null ? currentElement.RuntimeArguments.Count : 0;

                envMap.OldVariableCount = originalElement.RuntimeVariables.Count;
                envMap.OldPrivateVariableCount = originalElement.ImplementationVariables.Count;
                envMap.OldArgumentCount = oldArguments != null ? oldArguments.Count : 0;

                Fx.Assert((originalElement.HandlerOf == null && currentElement.HandlerOf == null)
                    || (originalElement.HandlerOf.RuntimeDelegateArguments.Count == currentElement.HandlerOf.RuntimeDelegateArguments.Count),
                    "RuntimeDelegateArguments count must not have changed.");
                envMap.RuntimeDelegateArgumentCount = originalElement.HandlerOf == null ? 0 : originalElement.HandlerOf.RuntimeDelegateArguments.Count;
            }

            DynamicUpdateMapEntry CreateMapEntry(Activity currentActivity, Activity matchingOriginal)
            {
                Fx.Assert(currentActivity != null && matchingOriginal != null, "this entry creation is only for existing activity's ID change.");
                
                this.foundOriginalElements[matchingOriginal.InternalId - 1] = true;

                DynamicUpdateMapEntry entry = new DynamicUpdateMapEntry(matchingOriginal.InternalId, currentActivity.InternalId);
                this.updateMap.AddEntry(entry);
                return entry;
            }

            internal void OnCreateDynamicUpdateMap(Activity currentElement, Activity originalElement, 
                DynamicUpdateMapEntry mapEntry, IDefinitionMatcher matcher)
            {
                this.AllowUpdateInsideCurrentActivity = null;
                this.UpdateDisallowedReason = null;
                this.SavedOriginalValuesForCurrentActivity = null;
                this.savedOriginalValuesForReferencedChildren = false;
                currentElement.OnInternalCreateDynamicUpdateMap(this, matcher, originalElement);
                if (this.AllowUpdateInsideCurrentActivity == false)
                {
                    this.BlockUpdate(currentElement, originalElement.Id, UpdateBlockedReason.Custom, mapEntry, this.UpdateDisallowedReason);
                }
                if (this.SavedOriginalValuesForCurrentActivity != null && this.SavedOriginalValuesForCurrentActivity.Count > 0)
                {
                    mapEntry.SavedOriginalValues = this.SavedOriginalValuesForCurrentActivity;
                }
                if (this.savedOriginalValuesForReferencedChildren)
                {
                    this.BlockUpdate(currentElement, originalElement.Id, UpdateBlockedReason.SavedOriginalValuesForReferencedChildren, mapEntry);
                }
            }

            void CreateVariableEntries(bool forImplementationVariables, DynamicUpdateMapEntry mapEntry, IList<Variable> newVariables, IList<Variable> oldVariables, Activity originalElement)
            {
                if (newVariables != null && newVariables.Count > 0)
                {
                    for (int i = 0; i < newVariables.Count; i++)
                    {
                        Variable newVariable = newVariables[i];
                        int originalIndex = this.Matcher.GetMatchIndex(newVariable, originalElement, forImplementationVariables);
                        
                        if (originalIndex != i)
                        {
                            EnsureEnvironmentUpdateMap(mapEntry);
                            EnvironmentUpdateMapEntry environmentEntry = new EnvironmentUpdateMapEntry
                            {
                                OldOffset = originalIndex,
                                NewOffset = i,
                            };

                            if (forImplementationVariables)
                            {
                                mapEntry.EnvironmentUpdateMap.PrivateVariableEntries.Add(environmentEntry);
                            }
                            else
                            {
                                mapEntry.EnvironmentUpdateMap.VariableEntries.Add(environmentEntry);
                            }                            

                            if (originalIndex == EnvironmentUpdateMapEntry.NonExistent)
                            {
                                Activity idleActivity = GetIdleActivity(newVariable.Default);
                                if (idleActivity != null)
                                {
                                    // If an variable default expression goes idle, the activity it is declared on can potentially
                                    // resume execution before the default expression is evaluated. We can't allow that.
                                    this.BlockUpdate(newVariable.Owner, UpdateBlockedReason.AddedIdleExpression, mapEntry, 
                                        SR.AddedIdleVariableDefaultBlockDU(newVariable.Name, idleActivity));
                                }
                                else if (newVariable.IsHandle)
                                {
                                    this.BlockUpdate(newVariable.Owner, UpdateBlockedReason.NewHandle, mapEntry);
                                }
                                environmentEntry.IsNewHandle = newVariable.IsHandle;
                            }
                        }
                    }
                }

                // We don't normally create entries for removals, but we need to ensure that
                // environment update happens if there are only removals.
                if (oldVariables != null && (newVariables == null || newVariables.Count < oldVariables.Count))
                {
                    EnsureEnvironmentUpdateMap(mapEntry);
                }
            }

            internal void CreateArgumentEntries(DynamicUpdateMapEntry mapEntry, IList<RuntimeArgument> newArguments, IList<ArgumentInfo> oldArguments)
            {                
                RuntimeArgument newIdleArgument;
                Activity idleActivity;
                if (!CreateArgumentEntries(mapEntry, newArguments, oldArguments, this.expressionRootsThatCanInduceIdle, out newIdleArgument, out idleActivity))
                {
                    // If an argument expression goes idle, the activity it is declared on can potentially
                    // resume execution before the argument is evaluated. We can't allow that.
                    this.BlockUpdate(newIdleArgument.Owner, UpdateBlockedReason.AddedIdleExpression, mapEntry,
                        SR.AddedIdleArgumentBlockDU(newIdleArgument.Name, idleActivity));
                    return;
                }
            }

            // if it detects any added argument whose Expression can induce idle, it returns FALSE along with newIdleArgument and idleActivity.  Return true otherwise.
            internal static bool CreateArgumentEntries(DynamicUpdateMapEntry mapEntry, IList<RuntimeArgument> newArguments, IList<ArgumentInfo> oldArguments, Dictionary<Activity, Activity> expressionRootsThatCanInduceIdle, out RuntimeArgument newIdleArgument, out Activity idleActivity)
            {
                newIdleArgument = null;
                idleActivity = null;

                if (newArguments != null && newArguments.Count > 0)
                {
                    for (int i = 0; i < newArguments.Count; i++)
                    {
                        RuntimeArgument newArgument = newArguments[i];
                        int oldIndex = oldArguments.IndexOf(new ArgumentInfo(newArgument));
                        Fx.Assert(oldIndex >= 0 || oldIndex == EnvironmentUpdateMapEntry.NonExistent, "NonExistent constant should be consistent with IndexOf");

                        if (oldIndex != i)
                        {
                            EnsureEnvironmentUpdateMap(mapEntry);
                            mapEntry.EnvironmentUpdateMap.ArgumentEntries.Add(new EnvironmentUpdateMapEntry
                            {
                                OldOffset = oldIndex,
                                NewOffset = i
                            });

                            if (oldIndex == EnvironmentUpdateMapEntry.NonExistent && newArgument.IsBound)
                            {
                                Activity expressionRoot = newArgument.BoundArgument.Expression;
                                if (expressionRoot != null && expressionRootsThatCanInduceIdle != null && expressionRootsThatCanInduceIdle.TryGetValue(expressionRoot, out idleActivity))
                                {
                                    newIdleArgument = newArgument;
                                    return false;
                                }
                            }
                        }
                    }
                }

                // We don't normally create entries for removals, but we need to ensure that
                // environment update happens if there are only removals.
                if (oldArguments != null && (newArguments == null || newArguments.Count < oldArguments.Count))
                {
                    EnsureEnvironmentUpdateMap(mapEntry);
                }

                return true;
            }

            IList<ArgumentInfo> GetOriginalArguments(DynamicUpdateMapEntry mapEntry, DynamicUpdateMap implementationMap, Activity updatedActivity, Activity originalActivity)
            {
                bool argumentsChangedFromImplementationMap = false;

                if (implementationMap != null && !implementationMap.ArgumentsAreUnknown)
                {
                    argumentsChangedFromImplementationMap = !ActivityComparer.ListEquals(implementationMap.NewArguments, implementationMap.OldArguments);
                    bool dynamicArgumentsDetected = !ActivityComparer.ListEquals(ArgumentInfo.List(updatedActivity), implementationMap.NewArguments);
                    if (argumentsChangedFromImplementationMap && dynamicArgumentsDetected)
                    {
                        // this is to ensure no dynamic arguments were added, removed or rearranged as the arguments owning activity was being consumed
                        // at the same time the activity has arguments changed from its implementation map.
                        // the list of RuntimeArguments obtained from the configured activity and the list of ArgumentInfos obtained from
                        // the implementation map must match exactly.  Otherwise this activity is blocked for update.
                        this.BlockUpdate(updatedActivity, UpdateBlockedReason.DynamicArguments, mapEntry, SR.NoDynamicArgumentsInActivityDefinitionChange);
                        return null;
                    }
                }

                return argumentsChangedFromImplementationMap ? implementationMap.OldArguments : ArgumentInfo.List(originalActivity);
            }

            Activity GetIdleActivity(Activity expressionRoot)
            {
                Activity result = null;
                if (expressionRoot != null && this.expressionRootsThatCanInduceIdle != null)
                {
                    this.expressionRootsThatCanInduceIdle.TryGetValue(expressionRoot, out result);
                }
                return result;
            }

            static void EnsureEnvironmentUpdateMap(DynamicUpdateMapEntry mapEntry)
            {
                if (!mapEntry.HasEnvironmentUpdates)
                {
                    mapEntry.EnvironmentUpdateMap = new EnvironmentUpdateMap();
                }   
            }
            
            void CheckForReusedActivity(Activity activity)
            {
                if (activity.RootActivity != this.builder.OriginalWorkflowDefinition)
                {
                    throw FxTrace.Exception.AsError(new InvalidWorkflowException(SR.OriginalActivityReusedInModifiedDefinition(activity)));
                }

                IList<Variable> variables = activity.RuntimeVariables;
                for (int i = 0; i < variables.Count; i++)
                {
                    if (variables[i].Owner.RootActivity != this.builder.OriginalWorkflowDefinition)
                    {
                        throw FxTrace.Exception.AsError(new InvalidWorkflowException(SR.OriginalVariableReusedInModifiedDefinition(variables[i].Name)));
                    }
                }                
            }

            void CheckCanArgumentOrVariableDefaultInduceIdle(ActivityUtilities.ChildActivity childActivity, ActivityUtilities.ActivityCallStack parentChain)
            {
                Activity activity = childActivity.Activity;
                if (!(activity.IsExpressionRoot || activity.RelationshipToParent == Activity.RelationshipType.VariableDefault))
                {
                    return;
                }                

                if (activity.HasNonEmptySubtree)
                {
                    ActivityUtilities.FinishCachingSubtree(
                        childActivity, parentChain, ProcessTreeOptions(this.builder.ForImplementation),
                        (a, c) => CheckCanActivityInduceIdle(activity, a.Activity));
                }
                else
                {
                    CheckCanActivityInduceIdle(activity, activity);
                }
            }

            void CheckCanActivityInduceIdle(Activity activity, Activity expressionRoot)
            {
                if (activity.InternalCanInduceIdle)
                {
                    if (this.expressionRootsThatCanInduceIdle == null)
                    {
                        this.expressionRootsThatCanInduceIdle = new Dictionary<Activity, Activity>(ReferenceEqualityComparer.Instance);
                    }
                    if (!this.expressionRootsThatCanInduceIdle.ContainsKey(expressionRoot))
                    {
                        this.expressionRootsThatCanInduceIdle.Add(expressionRoot, activity);
                    }
                }
            }

            bool DelegateArgumentsChanged(Activity newActivity, Activity oldActivity)
            {
                // check DelegateArguments of ActivityDelegate owning the handler 

                if (newActivity.HandlerOf == null)
                {
                    Fx.Assert(oldActivity.HandlerOf == null, "Once two activities have been matched, either both must be handlers or both must not be handlers.");
                    return false;
                }

                Fx.Assert(oldActivity.HandlerOf != null, "Once two activities have been matched, either both must be handlers or both must not be handlers.");

                return !ActivityComparer.ListEquals(newActivity.HandlerOf.RuntimeDelegateArguments, oldActivity.HandlerOf.RuntimeDelegateArguments);
            }

            void ThrowInvalidActivityToBlockUpdate(Activity activity)
            {
                Exception exception;
                if (builder.OnInvalidActivityToBlockUpdate != null)
                {
                    exception = builder.OnInvalidActivityToBlockUpdate(activity);
                }
                else
                {
                    exception = new InvalidOperationException(SR.InvalidActivityToBlockUpdate(activity));
                }
                throw FxTrace.Exception.AsError(exception);
            }

            internal void ThrowInvalidImplementationMapAssociation(Activity activity)
            {
                Exception exception;
                if (builder.OnInvalidImplementationMapAssociation != null)
                {
                    exception = builder.OnInvalidImplementationMapAssociation(activity);
                }
                else
                {
                    exception = new InvalidOperationException(SR.InvalidImplementationMapAssociation(activity));
                }
                throw FxTrace.Exception.AsError(exception);
            }
        }

        internal interface IDefinitionMatcher
        {
            void AddMatch(Activity newChild, Activity oldChild, Activity source);

            void AddMatch(Variable newVariable, Variable oldVariable, Activity source);

            Activity GetMatch(Activity newActivity);

            Variable GetMatch(Variable newVariable);
        }

        internal class DefinitionMatcher : IDefinitionMatcher
        {
            Dictionary<object, object> newToOldMatches;
            Func<object, DynamicUpdateMapItem> matchInfoLookup;

            internal DefinitionMatcher(Func<object, DynamicUpdateMapItem> matchInfoLookup)
            {
                this.matchInfoLookup = matchInfoLookup;
                this.newToOldMatches = new Dictionary<object, object>(ReferenceEqualityComparer.Instance);
            }

            internal IdSpace NewIdSpace
            {
                get;
                set;
            }

            internal IdSpace OldIdSpace
            {
                get;
                set;
            }

            // The following methods are intended to be called by the activity author
            // (via UpdateMapMetadata), and should validate accordingly
            public void AddMatch(Activity newChild, Activity oldChild, Activity source)
            {
                Fx.Assert(source != null, "source cannot be null.");

                if (newChild.Parent != source)
                {
                    throw FxTrace.Exception.Argument("newChild", SR.AddMatchActivityNewParentMismatch(
                        source, newChild, newChild.Parent));
                }
                if (newChild.MemberOf != newChild.Parent.MemberOf)
                {
                    throw FxTrace.Exception.Argument("newChild", SR.AddMatchActivityPrivateChild(newChild));
                }
                if (oldChild.Parent != null && oldChild.MemberOf != oldChild.Parent.MemberOf)
                {
                    throw FxTrace.Exception.Argument("oldChild", SR.AddMatchActivityPrivateChild(oldChild));
                }
                if (!ParentsMatch(newChild, oldChild))
                {
                    throw FxTrace.Exception.Argument("oldChild", SR.AddMatchActivityNewAndOldParentMismatch(
                        newChild, oldChild, newChild.Parent, oldChild.Parent));
                }               

                // Only one updated activity can match a given original activity
                foreach (Activity newSibling in GetPublicDeclaredChildren(newChild.Parent, true))
                {
                    if (GetMatch(newSibling) == oldChild)
                    {
                        this.newToOldMatches[newSibling] = null;
                        break;
                    }
                }

                this.newToOldMatches[newChild] = oldChild;
            }

            public void AddMatch(Variable newVariable, Variable oldVariable, Activity source)
            {
                if (!ActivityComparer.SignatureEquals(newVariable, oldVariable))
                {
                    throw FxTrace.Exception.Argument("newVariable", SR.AddMatchVariableSignatureMismatch(
                        source, newVariable.Name, newVariable.Type, newVariable.Modifiers, oldVariable.Name, oldVariable.Type, oldVariable.Modifiers));
                }

                if (newVariable.Owner != source)
                {
                    throw FxTrace.Exception.Argument("newVariable", SR.AddMatchVariableNewParentMismatch(
                        source, newVariable.Name, newVariable.Owner));
                }
                if (GetMatch(newVariable.Owner) != oldVariable.Owner)
                {
                    throw FxTrace.Exception.Argument("oldVariable", SR.AddMatchVariableNewAndOldParentMismatch(
                        newVariable.Name, oldVariable.Name, newVariable.Owner, oldVariable.Owner));
                }
                if (!newVariable.IsPublic)
                {
                    throw FxTrace.Exception.Argument("newVariable", SR.AddMatchVariablePrivateChild(newVariable.Name));
                }
                if (!oldVariable.IsPublic)
                {
                    throw FxTrace.Exception.Argument("oldVariable", SR.AddMatchVariablePrivateChild(oldVariable.Name));
                }

                // Only one updated variable can match a given original variable
                foreach (Variable newSibling in newVariable.Owner.RuntimeVariables)
                {
                    if (GetMatch(newSibling) == oldVariable)
                    {
                        this.newToOldMatches[newSibling] = EnvironmentUpdateMapEntry.NonExistent;
                        break;
                    }
                }

                this.newToOldMatches[newVariable] = oldVariable.Owner.RuntimeVariables.IndexOf(oldVariable);
            }

            public Activity GetMatch(Activity newChild)
            {
                object result;
                if (this.newToOldMatches.TryGetValue(newChild, out result))
                {
                    return (Activity)result;
                }

                if (newChild.MemberOf != this.NewIdSpace)
                {
                    // We can only match the IdSpace being updated.
                    return null;
                }

                if (newChild.Origin != null && newChild.RelationshipToParent == Activity.RelationshipType.VariableDefault)
                {
                    // Auto-generated variable defaults have the same origin as the variable itself,
                    // so the match info comes from the variable.
                    foreach (Variable variable in newChild.Parent.RuntimeVariables)
                    {
                        if (variable.Default == newChild)
                        {
                            Variable originalVariable = GetMatch(variable);
                            if (originalVariable != null && originalVariable.Origin != null)
                            {
                                return originalVariable.Default;
                            }
                        }
                    }

                    return null;
                }

                DynamicUpdateMapItem matchInfo = this.matchInfoLookup(newChild.Origin ?? newChild);
                if (matchInfo == null || matchInfo.IsVariableMapItem)
                {
                    return null;
                }

                Activity originalActivity = this.OldIdSpace[matchInfo.OriginalId];
                if (originalActivity != null && ParentsMatch(newChild, originalActivity))
                {
                    this.newToOldMatches.Add(newChild, originalActivity);
                    return originalActivity;
                }
                else
                {
                    return null;
                }
            }

            public Variable GetMatch(Variable newVariable)
            {
                Activity matchingOwner = GetMatch(newVariable.Owner);
                if (matchingOwner == null)
                {
                    return null;
                }

                int index = GetMatchIndex(newVariable, matchingOwner, false);
                if (index >= 0)
                {
                    return matchingOwner.RuntimeVariables[index];
                }

                return null;
            }

            // return -1 if there is no match
            internal int GetMatchIndex(Variable newVariable, Activity matchingOwner, bool forImplementation)
            {
                object result;
                if (this.newToOldMatches.TryGetValue(newVariable, out result))
                {
                    return (int)result;
                }

                IList<Variable> originalVariables;
                if (forImplementation)
                {
                    originalVariables = matchingOwner.ImplementationVariables;
                }
                else
                {
                    originalVariables = matchingOwner.RuntimeVariables;
                }

                int oldIndex = -1;
                if (String.IsNullOrEmpty(newVariable.Name))
                {
                    if (forImplementation)
                    {
                        // HasPrivateMemberChanged must have detected any presence of nameless private variable in advance.
                        oldIndex = newVariable.Owner.ImplementationVariables.IndexOf(newVariable);
                    }
                    else
                    {
                        // only for those variables without names, we attempt to match by MapItem tag
                        DynamicUpdateMapItem matchInfo = this.matchInfoLookup(newVariable.Origin ?? newVariable);
                        if (matchInfo != null && matchInfo.IsVariableMapItem && matchingOwner.InternalId == matchInfo.OriginalVariableOwnerId)
                        {
                            // "matchingOwner.InternalId != matchInfo.OriginalVariableOwnerId" means the variable has been moved to a different owner,
                            // and it is treated as a new variable addition.
                            oldIndex = matchInfo.OriginalId;
                        }
                    }
                }
                else
                {
                    // named variables are matched by their Name, Type and Modifiers

                    for (int i = 0; i < originalVariables.Count; i++)
                    {
                        if (ActivityComparer.SignatureEquals(newVariable, originalVariables[i]))
                        {
                            // match by sig----ure(Name, Type, Modifier) found
                            oldIndex = i;
                            break;
                        }
                    }                        
                }

                if (oldIndex >= 0 && oldIndex < originalVariables.Count)
                {
                    this.newToOldMatches.Add(newVariable, oldIndex);
                    return oldIndex;
                }

                return EnvironmentUpdateMapEntry.NonExistent;
            }

            bool ParentsMatch(Activity currentActivity, Activity originalActivity)
            {
                if (currentActivity.Parent == null)
                {
                    return originalActivity.Parent == null;
                }
                else
                {
                    if (currentActivity.RelationshipToParent != originalActivity.RelationshipToParent ||
                        (currentActivity.HandlerOf != null && currentActivity.HandlerOf.ParentCollectionType != originalActivity.HandlerOf.ParentCollectionType))
                    {
                        return false;
                    }

                    if (currentActivity.Parent == currentActivity.MemberOf.Owner)
                    {
                        return originalActivity.Parent == this.OldIdSpace.Owner;
                    }

                    return originalActivity.Parent != null &&
                        GetMatch(currentActivity.Parent) == originalActivity.Parent;
                }
            }           
        }

        internal class NestedIdSpaceFinalizer : IDefinitionMatcher
        {
            Finalizer finalizer;
            DynamicUpdateMap userProvidedMap;
            DynamicUpdateMap generatedMap;
            Activity updatedActivity;
            Activity originalActivity;
            bool invalidMatchInCurrentActivity;
            NestedIdSpaceFinalizer parent;

            public NestedIdSpaceFinalizer(Finalizer finalizer, DynamicUpdateMap implementationMap, Activity updatedActivity, Activity originalActivity, NestedIdSpaceFinalizer parent)
            {
                this.finalizer = finalizer;
                this.userProvidedMap = implementationMap;
                this.updatedActivity = updatedActivity;
                this.originalActivity = originalActivity;
                this.parent = parent;
            }

            public void ValidateOrCreateImplementationMap(DynamicUpdateMapEntry mapEntry)
            {
                // check applicability of the provided implementation map
                if (this.userProvidedMap != null)
                {
                    IdSpace privateIdSpace = updatedActivity.ParentOf;
                    if (privateIdSpace == null)
                    {
                        this.finalizer.ThrowInvalidImplementationMapAssociation(updatedActivity);
                    }
                    if (!this.userProvidedMap.IsNoChanges && privateIdSpace.MemberCount != this.userProvidedMap.NewDefinitionMemberCount)
                    {
                        BlockUpdate(updatedActivity, UpdateBlockedReason.InvalidImplementationMap, mapEntry,
                            SR.InvalidImplementationMap(this.userProvidedMap.NewDefinitionMemberCount, privateIdSpace.MemberCount));
                        return;
                    }
                }

                // The only difference between updatedActivity and originalActivity is changes in the outer IdSpace.
                // The implementation IdSpace should never change in response to outer IdSpace changes.
                // The only exception is addition/removal/rearrangement of RuntimeArguments and their Expressions in the private IdSpace for the sake of supporting Receive Content Parameter change.
                // If any argument change is detected and nothing else changed in the private IdSpace, 
                //  HasPrivateMemberOtherThanArgumentsChanged will return FALSE as well as returning a generated implementation Map.
                // Also, when userProvidedMap exists, we don't allow changes to arguments inside the private IdSpace
                DynamicUpdateMap argumentChangesMap;
                if (ActivityComparer.HasPrivateMemberOtherThanArgumentsChanged(this, updatedActivity, originalActivity, this.parent == null, out argumentChangesMap) || 
                    (argumentChangesMap != null && this.userProvidedMap != null))
                {
                    // either of the following two must have occured here.
                    // A.
                    // members in the private IdSpace(or nested IdSpaces) must have changed.
                    // addition/removal/rearrangement of arguments and their expressions in the private IdSpace are not considered as change.
                    //
                    // B.
                    // addition/removal/rearrangement of arguments or their expressions in the private IdSpace occured and no other members in the private IdSpace(or nested IdSpaces) changed except their id shift.
                    // Due to the id shift caused by argument change, an implementation map("argumentChangesMap") was created.
                    // In addition to "argumentChangesMap", there is also a user provided map.  This blocks DU.

                    // generate a warning and block update inside this activity
                    BlockUpdate(updatedActivity, UpdateBlockedReason.PrivateMembersHaveChanged, mapEntry);
                    return;
                }                

                if (updatedActivity.ParentOf != null)
                {
                    GenerateMap(argumentChangesMap);
                    if (this.generatedMap == null)
                    {
                        mapEntry.ImplementationUpdateMap = this.userProvidedMap;
                    }
                    else
                    {
                        if (this.userProvidedMap == null || this.userProvidedMap.IsNoChanges)
                        {
                            FillGeneratedMap();
                        }
                        else
                        {
                            MergeProvidedMapIntoGeneratedMap();
                        }
                        mapEntry.ImplementationUpdateMap = this.generatedMap;
                    }
                }
            }

            // AddMatch is a no-op, since any matches come from the provided implementation map.
            // However an invalid match can still cause us to disallow update
            public void AddMatch(Activity newChild, Activity oldChild, Activity source)
            {
                if (newChild.Parent != source || newChild.MemberOf != source.MemberOf || GetMatch(newChild) != oldChild)
                {
                    this.invalidMatchInCurrentActivity = true;
                }
            }

            public void AddMatch(Variable newVariable, Variable oldVariable, Activity source)
            {
                if (newVariable.Owner != source || !newVariable.IsPublic || GetMatch(newVariable) != oldVariable)
                {
                    this.invalidMatchInCurrentActivity = true;
                }
            }

            public Activity GetMatch(Activity newActivity)
            {
                NestedIdSpaceFinalizer owningFinalizer = this;
                do
                {
                    // The original definition being updated still needs to reference the updated implementation.
                    // So even if we have a provided impl map, there should be no ID changes between updatedActivity and original activity.
                    if (newActivity.MemberOf == owningFinalizer.updatedActivity.ParentOf)
                    {
                        return owningFinalizer.originalActivity.ParentOf[newActivity.InternalId];
                    }
                    owningFinalizer = owningFinalizer.parent;
                }
                while (owningFinalizer != null);

                return this.finalizer.Matcher.GetMatch(newActivity);
            }

            public Variable GetMatch(Variable newVariable)
            {
                Fx.Assert(newVariable.Owner.MemberOf == this.updatedActivity.ParentOf, "Should only call GetMatch for variables owned by the participating activity");
                int index = newVariable.Owner.RuntimeVariables.IndexOf(newVariable);
                if (index >= 0)
                {
                    Activity matchingOwner = GetMatch(newVariable.Owner);
                    if (matchingOwner != null && matchingOwner.RuntimeVariables.Count > index)
                    {
                        return matchingOwner.RuntimeVariables[index];
                    }
                }

                return null;
            }

            public void CreateArgumentEntries(DynamicUpdateMapEntry mapEntry, IList<RuntimeArgument> newArguments, IList<ArgumentInfo> oldArguments)
            {
                RuntimeArgument newIdleArgument;
                Activity idleActivity;
                if (!DynamicUpdateMapBuilder.Finalizer.CreateArgumentEntries(mapEntry, newArguments, oldArguments, this.finalizer.ExpressionRootsThatCanInduceIdle, out newIdleArgument, out idleActivity))
                {
                    // If an argument expression goes idle, the activity it is declared on can potentially
                    // resume execution before the argument is evaluated. We can't allow that.
                    this.BlockUpdate(newIdleArgument.Owner, UpdateBlockedReason.AddedIdleExpression, mapEntry,
                        SR.AddedIdleArgumentBlockDU(newIdleArgument.Name, idleActivity));
                    return;
                }
            }

            void BlockUpdate(Activity updatedActivity, UpdateBlockedReason reason, DynamicUpdateMapEntry entry, string message = null)
            {
                Activity originalActivity = GetMatch(updatedActivity);
                Fx.Assert(originalActivity != null, "Cannot block update inside an added activity");
                this.finalizer.BlockUpdate(updatedActivity, originalActivity.Id, reason, entry, message);
            }

            // This method allows activities in the implementation IdSpace to participate in map creation.
            // This is necessary because they may need to save original values for properties whose value
            // may be set by referencing activity properties. (E.g. <Receive OperationName='{PropertyReference OpName}' />)
            // They may also disable update based on observed property values. However they are not allowed
            // to change or add any matches, because the implementation IdSpace should not be changing
            // based on public property changes.
            // if argumentChangesMap is non-null, it will be used as the initial generatedMap onto which original values are saved.
            void GenerateMap(DynamicUpdateMap argumentChangesMap)
            {
                IdSpace updatedIdSpace = this.updatedActivity.ParentOf;
                IdSpace originalIdSpace = this.originalActivity.ParentOf;

                for (int i = 1; i <= updatedIdSpace.MemberCount; i++)
                {
                    DynamicUpdateMapEntry providedEntry = null;
                    if (this.userProvidedMap != null && !this.userProvidedMap.IsNoChanges)
                    {
                        bool isNewlyAdded = !this.userProvidedMap.TryGetUpdateEntryByNewId(i, out providedEntry);
                        if (isNewlyAdded || providedEntry.IsRuntimeUpdateBlocked ||
                            providedEntry.IsUpdateBlockedByUpdateAuthor || providedEntry.IsParentRemovedOrBlocked)
                        {
                            // No need to save original values or block update
                            continue;
                        }
                    }

                    DynamicUpdateMapEntry argumentChangesMapEntry = null;
                    if (argumentChangesMap != null)
                    {
                        Fx.Assert(!argumentChangesMap.IsNoChanges, "argumentChangesMap will never be NoChanges map because it is automatically created only when there is argument changes.");
                        bool isNewlyAdded = !argumentChangesMap.TryGetUpdateEntryByNewId(i, out argumentChangesMapEntry);
                        if (isNewlyAdded)
                        {
                            // No need to save original values or block update
                            continue;
                        }
                    }

                    // We only need to save this map entry if it has some non-default value.
                    DynamicUpdateMapEntry generatedEntry = GenerateEntry(argumentChangesMapEntry, providedEntry, i);
                    DynamicUpdateMap providedImplementationMap = providedEntry != null ? providedEntry.ImplementationUpdateMap : null;
                    if (generatedEntry.IsRuntimeUpdateBlocked ||
                        generatedEntry.SavedOriginalValues != null ||
                        generatedEntry.SavedOriginalValueFromParent != null ||
                        generatedEntry.ImplementationUpdateMap != providedImplementationMap ||
                        generatedEntry.IsIdChange ||
                        generatedEntry.HasEnvironmentUpdates)
                    {
                        EnsureGeneratedMap();
                        this.generatedMap.AddEntry(generatedEntry);
                    }
                }

                if (argumentChangesMap != null && argumentChangesMap.entries != null)
                {
                    // add all IsRemoved entries
                    foreach (DynamicUpdateMapEntry entry in argumentChangesMap.entries)
                    {
                        if (entry.IsRemoval)
                        {
                            EnsureGeneratedMap();                            
                            this.generatedMap.AddEntry(entry);
                        }                        
                    }                    
                }
            }

            void EnsureGeneratedMap()
            {
                if (this.generatedMap == null)
                {
                    this.generatedMap = new DynamicUpdateMap
                    {
                        IsForImplementation = true,
                        NewDefinitionMemberCount = this.updatedActivity.ParentOf.MemberCount
                    };
                }
            }

            DynamicUpdateMapEntry GenerateEntry(DynamicUpdateMapEntry argumentChangesMapEntry, DynamicUpdateMapEntry providedEntry, int id)
            {
                DynamicUpdateMapEntry generatedEntry;
                Activity updatedChild;
                Activity originalChild;

                // argumentChangesMapEntry and providedEntry are mutually exclusive.
                // both cannot be non-null at the same time although both may be null at the same time.
                if (argumentChangesMapEntry == null)
                {
                    int originalIndex = providedEntry != null ? providedEntry.OldActivityId : id;
                    generatedEntry = new DynamicUpdateMapEntry(originalIndex, id);

                    // we assume nothing has changed in the private IdSpace
                    updatedChild = this.updatedActivity.ParentOf[id];
                    originalChild = this.originalActivity.ParentOf[id];
                }
                else
                {
                    generatedEntry = argumentChangesMapEntry;

                    // activity IDs in the private IdSpace has changed due to arguments change inside the private IdSpace
                    updatedChild = this.updatedActivity.ParentOf[argumentChangesMapEntry.NewActivityId];
                    originalChild = this.originalActivity.ParentOf[argumentChangesMapEntry.OldActivityId];
                }                

                // Allow the activity to participate                
                this.invalidMatchInCurrentActivity = false;
                this.finalizer.OnCreateDynamicUpdateMap(updatedChild, originalChild, generatedEntry, this);
                if (this.invalidMatchInCurrentActivity && !generatedEntry.IsRuntimeUpdateBlocked)
                {
                    BlockUpdate(updatedChild, UpdateBlockedReason.ChangeMatchesInImplementation, generatedEntry);
                }

                // Fill in the rest of the map entry;
                generatedEntry.SavedOriginalValueFromParent = this.finalizer.GetSavedOriginalValueFromParent(updatedChild);
                DynamicUpdateMap childImplementationMap = providedEntry != null ? providedEntry.ImplementationUpdateMap : null;
                if (!generatedEntry.IsRuntimeUpdateBlocked)
                {
                    NestedIdSpaceFinalizer nestedFinalizer = new NestedIdSpaceFinalizer(this.finalizer, childImplementationMap, updatedChild, originalChild, this);
                    nestedFinalizer.ValidateOrCreateImplementationMap(generatedEntry);
                }

                return generatedEntry;
            }

            // The generated map only contains entries that have some non-default value. For it to be a valid
            // implementation map, we need to fill in all the unchanged entries.
            void FillGeneratedMap()
            {
                Fx.Assert(this.generatedMap != null, "If there were no generated entries then we don't need a generated map.");
                this.generatedMap.ArgumentsAreUnknown = true;                
                for (int i = 1; i <= this.originalActivity.ParentOf.MemberCount; i++)
                {
                    DynamicUpdateMapEntry entry;
                    if (!this.generatedMap.TryGetUpdateEntry(i, out entry))
                    {
                        entry = new DynamicUpdateMapEntry(i, i);
                        this.generatedMap.AddEntry(entry);
                    }
                    entry.Parent = GetParentEntry(this.originalActivity.ParentOf[i], this.generatedMap);
                }
            }

            void MergeProvidedMapIntoGeneratedMap()
            {
                this.generatedMap.OldArguments = this.userProvidedMap.OldArguments;
                this.generatedMap.NewArguments = this.userProvidedMap.NewArguments;

                for (int i = 1; i <= this.userProvidedMap.OldDefinitionMemberCount; i++)
                {
                    // Get/create the matching generated entry
                    DynamicUpdateMapEntry providedEntry;
                    this.userProvidedMap.TryGetUpdateEntry(i, out providedEntry);
                    DynamicUpdateMapEntry generatedEntry = GetOrCreateGeneratedEntry(providedEntry);
                    if (generatedEntry.IsRemoval || generatedEntry.IsRuntimeUpdateBlocked || generatedEntry.IsUpdateBlockedByUpdateAuthor || generatedEntry.IsParentRemovedOrBlocked)
                    {
                        continue;
                    }

                    // Disable update if there's a conflict
                    int newActivityId = providedEntry.NewActivityId;
                    if (HasOverlap(providedEntry.SavedOriginalValues, generatedEntry.SavedOriginalValues) ||
                        (HasSavedOriginalValuesForChildren(newActivityId, this.userProvidedMap) && HasSavedOriginalValuesForChildren(newActivityId, this.generatedMap)))
                    {
                        Activity updatedChild = this.updatedActivity.ParentOf[generatedEntry.NewActivityId];
                        BlockUpdate(updatedChild, UpdateBlockedReason.GeneratedAndProvidedMapConflict, generatedEntry, SR.GeneratedAndProvidedMapConflict);
                    }
                    else
                    {
                        generatedEntry.SavedOriginalValues = DynamicUpdateMapEntry.Merge(generatedEntry.SavedOriginalValues, providedEntry.SavedOriginalValues);
                    }
                }
            }

            DynamicUpdateMapEntry GetOrCreateGeneratedEntry(DynamicUpdateMapEntry providedEntry)
            {
                // Get or create the matching entry
                DynamicUpdateMapEntry generatedEntry;
                if (!this.generatedMap.TryGetUpdateEntry(providedEntry.OldActivityId, out generatedEntry))
                {
                    generatedEntry = new DynamicUpdateMapEntry(providedEntry.OldActivityId, providedEntry.NewActivityId)
                    {
                        DisplayName = providedEntry.DisplayName,
                        BlockReason = providedEntry.BlockReason,
                        BlockReasonMessage = providedEntry.BlockReasonMessage,
                        IsUpdateBlockedByUpdateAuthor = providedEntry.IsUpdateBlockedByUpdateAuthor,
                    };
                    this.generatedMap.AddEntry(generatedEntry);
                }
                else
                {
                    Fx.Assert(providedEntry.NewActivityId == generatedEntry.NewActivityId &&
                        providedEntry.DisplayName == generatedEntry.DisplayName &&
                        !providedEntry.IsRuntimeUpdateBlocked && !providedEntry.IsUpdateBlockedByUpdateAuthor,
                        "GeneratedEntry should be created with correct ID, and should not be created for an entry that has blocked update");
                }

                // Copy/fill additional values
                generatedEntry.EnvironmentUpdateMap = providedEntry.EnvironmentUpdateMap;
                if (providedEntry.Parent != null)
                {
                    DynamicUpdateMapEntry parentEntry;
                    this.generatedMap.TryGetUpdateEntry(providedEntry.Parent.OldActivityId, out parentEntry);
                    Fx.Assert(parentEntry != null, "We process in IdSpace order, so we always process parents before their children");
                    generatedEntry.Parent = parentEntry;
                }
                if (generatedEntry.SavedOriginalValueFromParent == null)
                {
                    generatedEntry.SavedOriginalValueFromParent = providedEntry.SavedOriginalValueFromParent;
                }
                if (generatedEntry.ImplementationUpdateMap == null)
                {
                    generatedEntry.ImplementationUpdateMap = providedEntry.ImplementationUpdateMap;
                }

                return generatedEntry;
            }

            bool HasOverlap(IDictionary<string, object> providedValues, IDictionary<string, object> generatedValues)
            {
                return providedValues != null && generatedValues != null &&
                    providedValues.Keys.Any(k => generatedValues.ContainsKey(k));
            }

            bool HasSavedOriginalValuesForChildren(int parentNewActivityId, DynamicUpdateMap map)
            {
                foreach (Activity child in GetPublicDeclaredChildren(this.updatedActivity.ParentOf[parentNewActivityId], false))
                {
                    DynamicUpdateMapEntry childEntry;
                    if (map.TryGetUpdateEntryByNewId(child.InternalId, out childEntry) &&
                        childEntry.SavedOriginalValueFromParent != null)
                    {
                        return true;
                    }
                }

                return false;
            }            
        }
    }
}
