//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Runtime
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.Activities.DynamicUpdate;
    using System.Activities.Statements;
    using System.Collections.ObjectModel;
    
    [DataContract(Name = XD.Runtime.ActivityInstanceMap, Namespace = XD.Runtime.Namespace)]
    class ActivityInstanceMap
    {
        // map from activities to (active) associated activity instances
        IDictionary<Activity, InstanceList> instanceMapping;
        InstanceList[] rawDeserializedLists;

        IList<InstanceListNeedingUpdate> updateList;

        internal ActivityInstanceMap()
        {
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
            Justification = "Called by serialization")]
        [DataMember(EmitDefaultValue = false)]
        internal InstanceList[] SerializedInstanceLists
        {
            get
            {
                if (this.instanceMapping == null || this.instanceMapping.Count == 0)
                {
                    return this.rawDeserializedLists;
                }
                else
                {
                    InstanceList[] lists = new InstanceList[this.instanceMapping.Count];
                    int index = 0;
                    foreach (KeyValuePair<Activity, InstanceList> entry in this.instanceMapping)
                    {
                        entry.Value.ActivityId = entry.Key.QualifiedId.AsByteArray();
                        lists[index] = entry.Value;
                        index++;
                    }

                    return lists;
                }
            }
            set
            {
                Fx.Assert(value != null, "We don't serialize the default value.");

                this.rawDeserializedLists = value;
            }
        }

        IDictionary<Activity, InstanceList> InstanceMapping
        {
            get
            {
                if (this.instanceMapping == null)
                {
                    this.instanceMapping = new Dictionary<Activity, InstanceList>();
                }

                return this.instanceMapping;
            }
        }

        private static void AddBlockingActivity(ref Collection<ActivityBlockingUpdate> updateErrors, DynamicUpdateMap.UpdatedActivity updatedActivity, QualifiedId originalId, string reason, string activityInstanceId)
        {
            if (updatedActivity.NewActivity != null)
            {
                ActivityBlockingUpdate.AddBlockingActivity(ref updateErrors, updatedActivity.NewActivity, originalId.ToString(), reason, activityInstanceId);
            }
            else
            {
                string updatedId = updatedActivity.MapEntry.IsRemoval ? null : updatedActivity.NewId.ToString();
                ActivityBlockingUpdate.AddBlockingActivity(ref updateErrors, updatedId, originalId.ToString(), reason, activityInstanceId);
            }
        }

        public void GetActivitiesBlockingUpdate(DynamicUpdateMap updateMap, List<ActivityInstance> secondaryRootInstances, ref Collection<ActivityBlockingUpdate> updateErrors)
        {
            this.GetInstanceListsNeedingUpdate(updateMap, null, secondaryRootInstances, ref updateErrors);
        }

        // searching secondaryRootInstances list is necessary because instance in InstanceList doesn't have its Parent set until it's fixed up.
        // so the only way to find out if an instance in InstanceList is a secondary root is to lookup in secondaryRootInstances list.
        private static bool IsNonDefaultSecondaryRoot(ActivityInstance instance, List<ActivityInstance> secondaryRootInstances)
        {
            if (secondaryRootInstances != null && secondaryRootInstances.Contains(instance))
            {
                // Non-default secondary roots are CompensationParticipant type, and their environment will always have a non-null parent which is the environment owned by a CompensableActivity.
                // A secondary root whose environment parent is null is the default secondary root, WorkflowCompensationBehavior.
                if (instance.IsEnvironmentOwner && instance.Environment.Parent != null)
                {
                    return true;
                }
            }

            return false;
        }       

        private static bool CanCompensationOrConfirmationHandlerReferenceAddedSymbols(InstanceList instanceList, DynamicUpdateMap rootUpdateMap, IdSpace rootIdSpace, List<ActivityInstance> secondaryRootInstances, ref Collection<ActivityBlockingUpdate> updateErrors)
        {
            for (int j = 0; j < instanceList.Count; j++)
            {
                ActivityInstance activityInstance = instanceList[j] as ActivityInstance;
                if (activityInstance == null || !IsNonDefaultSecondaryRoot(activityInstance, secondaryRootInstances))
                {
                    continue;
                }

                // here, find out if the given non-default secondary root references an environment to which a symbol is to be added via DU.
                // we start from a secondary root instead of starting from the enviroment with the already completed owner that was added symbols.
                // It is becuase for the case of adding symbols to noSymbols activities, the environment doesn't even exist from which we can start looking for referencing secondary root.

                int[] secondaryRootOriginalQID = new QualifiedId(instanceList.ActivityId).AsIDArray();

                Fx.Assert(secondaryRootOriginalQID != null && secondaryRootOriginalQID.Length > 1,
                    "CompensationParticipant is always an implementation child of a CompensableActivity, therefore it's IdSpace must be at least one level deep.");

                int[] parentOfSecondaryRootOriginalQID = new int[secondaryRootOriginalQID.Length - 1];
                Array.Copy(secondaryRootOriginalQID, parentOfSecondaryRootOriginalQID, secondaryRootOriginalQID.Length - 1);

                List<int> currentQIDBuilder = new List<int>();
                for (int i = 0; i < parentOfSecondaryRootOriginalQID.Length; i++)
                {
                    // 
                    // for each iteration of this for-loop, 
                    //  we are finding out if at every IdSpace level the map has any map entry whose activity has the CompensableActivity as an implementation decendant.
                    //  The map may not exist for every IdSpace between the root and the CompensableActivity.
                    //  If the matching map and the entry is found, then we find out if that matching entry's activity is a public decendant of any NoSymbols activity DU is to add variables or arguments to.
                    //
                    // This walk on the definition activity tree determines the hypothetical execution-time chain of instances and environments.
                    // The ultimate goal is to prevent adding variables or arguments to a NoSymbols activity which has already completed,
                    //  but its decendant CompensableActivity's compensation or confirmation handlers in the future may need to reference the added variables or arguments.

                    currentQIDBuilder.Add(parentOfSecondaryRootOriginalQID[i]);                    

                    DynamicUpdateMap.UpdatedActivity updatedActivity = rootUpdateMap.GetUpdatedActivity(new QualifiedId(currentQIDBuilder.ToArray()), rootIdSpace);
                    if (updatedActivity.MapEntry != null)
                    {
                        // the activity of this entry either has the CompensableActivity as an implementation decendant, or is the CompensableActivity itself.

                        // walk the same-IdSpace-parent chain of the entry,
                        // look for an entry whose EnvironmentUpdateMap.IsAdditionToNoSymbols is true.
                        DynamicUpdateMapEntry entry = updatedActivity.MapEntry;
                        do
                        {
                            if (!entry.IsRemoval && entry.HasEnvironmentUpdates && entry.EnvironmentUpdateMap.IsAdditionToNoSymbols)
                            {
                                int[] noSymbolAddActivityIDArray = currentQIDBuilder.ToArray();
                                noSymbolAddActivityIDArray[noSymbolAddActivityIDArray.Length - 1] = entry.OldActivityId;
                                QualifiedId noSymbolAddActivityQID = new QualifiedId(noSymbolAddActivityIDArray);
                                DynamicUpdateMap.UpdatedActivity noSymbolAddUpdatedActivity = rootUpdateMap.GetUpdatedActivity(noSymbolAddActivityQID, rootIdSpace);

                                AddBlockingActivity(ref updateErrors, noSymbolAddUpdatedActivity, noSymbolAddActivityQID, SR.VariableOrArgumentAdditionToReferencedEnvironmentNoDUSupported, null);
                                return true;
                            }

                            entry = entry.Parent;
                        } while (entry != null);
                    }
                }
            }

            return false;
        }

        private static bool IsInvalidEnvironmentUpdate(InstanceList instanceList, DynamicUpdateMap.UpdatedActivity updatedActivity, ref Collection<ActivityBlockingUpdate> updateErrors)
        {           
            if (updatedActivity.MapEntry == null || !updatedActivity.MapEntry.HasEnvironmentUpdates)
            {
                return false;
            }

            for (int j = 0; j < instanceList.Count; j++)
            {
                ActivityInstance activityInstance = instanceList[j] as ActivityInstance;
                if (activityInstance != null)
                {                   
                    string error = null;
                    if (activityInstance.SubState == ActivityInstance.Substate.ResolvingVariables)
                    {
                        // if the entry has Environment update to do when the instance is in the middle of resolving variable, it is an error.
                        error = SR.CannotUpdateEnvironmentInTheMiddleOfResolvingVariables;
                    }
                    else if (activityInstance.SubState == ActivityInstance.Substate.ResolvingArguments)
                    {
                        // if the entry has Environment update to do when the instance is in the middle of resolving arguments, it is an error.
                        error = SR.CannotUpdateEnvironmentInTheMiddleOfResolvingArguments;
                    }

                    if (error != null)
                    {
                        AddBlockingActivity(ref updateErrors, updatedActivity, new QualifiedId(instanceList.ActivityId), error, activityInstance.Id);
                        return true;
                    }
                }
                else
                {
                    LocationEnvironment environment = instanceList[j] as LocationEnvironment;
                    if (environment != null)
                    {
                        //
                        // environment that is referenced by a secondary root
                        // Adding a variable or argument that requires expression scheduling to this instanceless environment is not allowed.
                        //
                        List<int> dummyIndexes;
                        EnvironmentUpdateMap envMap = updatedActivity.MapEntry.EnvironmentUpdateMap;

                        if ((envMap.HasVariableEntries && TryGatherSchedulableExpressions(envMap.VariableEntries, out dummyIndexes)) ||
                            (envMap.HasPrivateVariableEntries && TryGatherSchedulableExpressions(envMap.PrivateVariableEntries, out dummyIndexes)) ||
                            (envMap.HasArgumentEntries && TryGatherSchedulableExpressions(envMap.ArgumentEntries, out dummyIndexes)))
                        {
                            AddBlockingActivity(ref updateErrors, updatedActivity, new QualifiedId(instanceList.ActivityId), SR.VariableOrArgumentAdditionToReferencedEnvironmentNoDUSupported, null);
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private static bool IsRemovalOrRTUpdateBlockedOrBlockedByUser(DynamicUpdateMap.UpdatedActivity updatedActivity, QualifiedId oldQualifiedId, out string error)
        {
            error = null;
            if (updatedActivity.MapEntry.IsRemoval)
            {
                // 

                error = SR.CannotRemoveExecutingActivityUpdateError(oldQualifiedId, updatedActivity.MapEntry.DisplayName);
            }
            else if (updatedActivity.MapEntry.IsRuntimeUpdateBlocked)
            {
                error = updatedActivity.MapEntry.BlockReasonMessage ?? UpdateBlockedReasonMessages.Get(updatedActivity.MapEntry.BlockReason);
            }
            else if (updatedActivity.MapEntry.IsUpdateBlockedByUpdateAuthor)
            {
                error = SR.BlockedUpdateInsideActivityUpdateByUserError;
            }

            return error != null;
        }        

        // targetDefinition argument is optional.
        private IList<InstanceListNeedingUpdate> GetInstanceListsNeedingUpdate(DynamicUpdateMap updateMap, Activity targetDefinition, List<ActivityInstance> secondaryRootInstances, ref Collection<ActivityBlockingUpdate> updateErrors)
        {
            IList<InstanceListNeedingUpdate> instanceListsToUpdate = new List<InstanceListNeedingUpdate>();
            if (this.rawDeserializedLists == null)
            {
                // This instance doesn't have any active instances (it is complete).
                return instanceListsToUpdate;
            }

            IdSpace rootIdSpace = null;
            if (targetDefinition != null)
            {
                rootIdSpace = targetDefinition.MemberOf;
            }

            for (int i = 0; i < this.rawDeserializedLists.Length; i++)
            {
                InstanceList list = this.rawDeserializedLists[i];
                QualifiedId oldQualifiedId = new QualifiedId(list.ActivityId);

                if (updateMap.IsImplementationAsRoot)
                {
                    int[] oldIdArray = oldQualifiedId.AsIDArray();
                    if (oldIdArray.Length == 1 && oldIdArray[0] != 1)
                    {
                        throw FxTrace.Exception.AsError(new InstanceUpdateException(SR.InvalidImplementationAsWorkflowRootForRuntimeState));
                    }
                }

                string error;
                InstanceListNeedingUpdate update;
                DynamicUpdateMap.UpdatedActivity updatedActivity = updateMap.GetUpdatedActivity(oldQualifiedId, rootIdSpace);
                                
                if (CanCompensationOrConfirmationHandlerReferenceAddedSymbols(list, updateMap, rootIdSpace, secondaryRootInstances, ref updateErrors))
                {
                    update = null;
                } 
                else if (updatedActivity.MapEntry == null)
                {
                    if (updatedActivity.IdChanged)
                    {
                        // this newQualifiedId is the new id for those InstanceLists whose IDs shifted by their parents' ID change
                        update = new InstanceListNeedingUpdate
                        {
                            InstanceList = list,
                            NewId = updatedActivity.NewId
                        };                        
                    }
                    else
                    {
                        // nothing changed, no map, no mapEntry
                        update = new InstanceListNeedingUpdate
                        {
                            InstanceList = list,
                            NewId = null,
                        };
                    }
                }
                else if (updatedActivity.MapEntry.IsParentRemovedOrBlocked)
                {
                    update = null;
                }
                else if (IsRemovalOrRTUpdateBlockedOrBlockedByUser(updatedActivity, oldQualifiedId, out error))
                {
                    string instanceId = null;
                    for (int j = 0; j < list.Count; j++)
                    {
                        ActivityInstance activityInstance = list[j] as ActivityInstance;
                        if (activityInstance != null)
                        {
                            instanceId = activityInstance.Id;
                            break;
                        }
                    }
                    AddBlockingActivity(ref updateErrors, updatedActivity, oldQualifiedId, error, instanceId);

                    update = null;
                }
                else if (IsInvalidEnvironmentUpdate(list, updatedActivity, ref updateErrors))
                {
                    update = null;
                }                
                else
                {
                    // no validation error for this InstanceList
                    // add it to the list of InstanceLists to be updated
                    update = new InstanceListNeedingUpdate
                    {
                        InstanceList = list,
                        NewId = updatedActivity.NewId,
                        UpdateMap = updatedActivity.Map,
                        MapEntry = updatedActivity.MapEntry,
                        NewActivity = updatedActivity.NewActivity
                    };
                }

                if (update != null)
                {
                    update.OriginalId = list.ActivityId;
                    instanceListsToUpdate.Add(update);
                }
            }

            return instanceListsToUpdate;
        }

        public void UpdateRawInstance(DynamicUpdateMap updateMap, Activity targetDefinition, List<ActivityInstance> secondaryRootInstances, ref Collection<ActivityBlockingUpdate> updateErrors)
        {         
            this.updateList = GetInstanceListsNeedingUpdate(updateMap, targetDefinition, secondaryRootInstances, ref updateErrors);
            if (updateErrors != null && updateErrors.Count > 0)
            {
                // error found.
                // there is no need to proceed to updating the instances
                return;
            }

            // if UpdateType is either MapEntryExists or ParentIdShiftOnly,
            // update the ActivityIDs and update Environments            
            // also, update the ImplementationVersion.
            foreach (InstanceListNeedingUpdate update in this.updateList)
            {
                Fx.Assert(update.InstanceList != null, "update.InstanceList must not be null.");

                if (update.NothingChanged)
                {
                    continue;
                }

                Fx.Assert(update.NewId != null, "update.NewId must not be null.");

                InstanceList instanceList = update.InstanceList;
                instanceList.ActivityId = update.NewId.AsByteArray();                

                if (update.ParentIdShiftOnly)
                {
                    // this InstanceList must have been one of those whose IDs shifted by their parent's ID change,
                    // but no involvement in DU.
                    continue;
                }

                bool implementationVersionUpdateNeeded = false;
                if (update.MapEntry.ImplementationUpdateMap != null)
                {
                    implementationVersionUpdateNeeded = true;
                }

                if (update.MapEntry.HasEnvironmentUpdates)
                {
                    // update LocationEnvironemnt

                    Fx.Assert(update.NewActivity != null, "TryGetUpdateMapEntryFromRootMap should have thrown if it couldn't map to an activity");
                    instanceList.UpdateEnvironments(update.MapEntry.EnvironmentUpdateMap, update.NewActivity);
                }                

                for (int i = 0; i < instanceList.Count; i++)
                {
                    ActivityInstance activityInstance = instanceList[i] as ActivityInstance;

                    if (implementationVersionUpdateNeeded)
                    {
                        activityInstance.ImplementationVersion = update.NewActivity.ImplementationVersion;
                    }
                }
            }
        }

        private static bool TryGatherSchedulableExpressions(IList<EnvironmentUpdateMapEntry> entries, out List<int> addedLocationReferenceIndexes)
        {
            addedLocationReferenceIndexes = null;

            for (int i = 0; i < entries.Count; i++)
            {
                EnvironmentUpdateMapEntry entry = entries[i];
                if (entry.IsAddition)
                {
                    if (addedLocationReferenceIndexes == null)
                    {
                        addedLocationReferenceIndexes = new List<int>();
                    }
                    addedLocationReferenceIndexes.Add(entry.NewOffset);
                }
            }

            return addedLocationReferenceIndexes != null;
        }

        // this is called after all instances have been loaded and fixedup
        public void UpdateInstanceByActivityParticipation(ActivityExecutor activityExecutor, DynamicUpdateMap rootMap, ref Collection<ActivityBlockingUpdate> updateErrors)
        {
            foreach (InstanceListNeedingUpdate participant in this.updateList)
            {
                if (participant.NothingChanged || participant.ParentIdShiftOnly)
                {
                    Fx.Assert(participant.UpdateMap == null && participant.MapEntry == null, "UpdateMap and MapEntry must be null if we are here.");

                    // create a temporary NoChanges UpdateMap as well as a temporary no change MapEntry
                    // so that we can create a NativeActivityUpdateContext object in order to invoke UpdateInstance() on an activity which
                    // doesn't have a corresponding map and an map entry.  
                    // The scenario enabled here is scheduling a newly added reference branch to a Parallel inside an activity's implementation.                   
                    participant.UpdateMap = DynamicUpdateMap.DummyMap;
                    participant.MapEntry = DynamicUpdateMapEntry.DummyMapEntry;
                }

                // now let activities participate in update
                for (int i = 0; i < participant.InstanceList.Count; i++)
                {
                    ActivityInstance instance = participant.InstanceList[i] as ActivityInstance;
                    if (instance == null)
                    {
                        continue;
                    }

                    IInstanceUpdatable activity = instance.Activity as IInstanceUpdatable;
                    if (activity != null && instance.SubState == ActivityInstance.Substate.Executing)
                    {
                        NativeActivityUpdateContext updateContext = new NativeActivityUpdateContext(this, activityExecutor, instance, participant.UpdateMap, participant.MapEntry, rootMap);
                        try
                        {
                            activity.InternalUpdateInstance(updateContext);

                            if (updateContext.IsUpdateDisallowed)
                            {
                                ActivityBlockingUpdate.AddBlockingActivity(ref updateErrors, instance.Activity, new QualifiedId(participant.OriginalId).ToString(), updateContext.DisallowedReason, instance.Id);
                                continue;
                            }
                        }
                        catch (Exception e)
                        {
                            if (Fx.IsFatal(e))
                            {
                                throw;
                            }

                            throw FxTrace.Exception.AsError(new InstanceUpdateException(SR.NativeActivityUpdateInstanceThrewException(e.Message), e));
                        }
                        finally
                        {
                            updateContext.Dispose();
                        }
                    }                
                }
            }            

            // Schedule evaluation of newly added arguments and newly added variables.
            // This needs to happen after all the invokations of UpdateInstance above, so that newly
            // added arguments and newly added variables get evaluated before any newly added activities get executed.
            // We iterate the list in reverse so that parents are always scheduled after (and thus 
            // execute before) their children, which may depend on the parents.
            for (int i = this.updateList.Count - 1; i >= 0; i--)
            {
                InstanceListNeedingUpdate participant = this.updateList[i];

                if (!participant.MapEntryExists)
                {
                    // if the given InstanceList has no map entry,
                    // then there is no new LocationReferences to resolve. 
                    continue;
                }

                Fx.Assert(participant.MapEntry != null, "MapEntry must be non-null here.");
                if (!participant.MapEntry.HasEnvironmentUpdates)
                {
                    // if there is no environment updates for this MapEntry,
                    // then there is no new LocationReferences to resolve.
                    continue;
                }
                
                for (int j = 0; j < participant.InstanceList.Count; j++)
                {
                    ActivityInstance instance = participant.InstanceList[j] as ActivityInstance;
                    if (instance == null || instance.SubState != ActivityInstance.Substate.Executing)
                    {
                        // if the given ActivityInstance is not in Substate.Executing, 
                        // then, do not try to resolve new LocationReferences
                        continue;
                    }
                    
                    List<int> addedArgumentIndexes;
                    List<int> addedVariableIndexes;
                    List<int> addedPrivateVariableIndexes;

                    EnvironmentUpdateMap envMap = participant.MapEntry.EnvironmentUpdateMap;                    

                    if (envMap.HasVariableEntries && TryGatherSchedulableExpressions(envMap.VariableEntries, out addedVariableIndexes))
                    {
                        // schedule added variable default expressions
                        instance.ResolveNewVariableDefaultsDuringDynamicUpdate(activityExecutor, addedVariableIndexes, false);
                    }

                    if (envMap.HasPrivateVariableEntries && TryGatherSchedulableExpressions(envMap.PrivateVariableEntries, out addedPrivateVariableIndexes))
                    {
                        // schedule added private variable default expressions
                        // HasPrivateMemberChanged() check disallows addition of private variable default that offsets the private IdSpace,
                        // However, the added private variable default expression can be an imported activity, which has no affect on the private IdSpace.
                        // For such case, we want to be able to schedule the imported default expressions here.
                        instance.ResolveNewVariableDefaultsDuringDynamicUpdate(activityExecutor, addedPrivateVariableIndexes, true);
                    }

                    if (envMap.HasArgumentEntries && TryGatherSchedulableExpressions(envMap.ArgumentEntries, out addedArgumentIndexes))
                    {
                        // schedule added arguments
                        instance.ResolveNewArgumentsDuringDynamicUpdate(activityExecutor, addedArgumentIndexes);
                    }
                }                                
            }
        }        

        public void AddEntry(IActivityReference reference, bool skipIfDuplicate)
        {
            Activity activity = reference.Activity;

            InstanceList mappedInstances;
            if (this.InstanceMapping.TryGetValue(activity, out mappedInstances))
            {
                mappedInstances.Add(reference, skipIfDuplicate);
            }
            else
            {
                this.InstanceMapping.Add(activity, new InstanceList(reference));
            }
        }

        public void AddEntry(IActivityReference reference)
        {
            AddEntry(reference, false);
        }

        public void LoadActivityTree(Activity rootActivity, ActivityInstance rootInstance, List<ActivityInstance> secondaryRootInstances, ActivityExecutor executor)
        {
            Fx.Assert(this.rawDeserializedLists != null, "We should always have deserialized some lists.");

            this.instanceMapping = new Dictionary<Activity, InstanceList>(this.rawDeserializedLists.Length);

            for (int i = 0; i < this.rawDeserializedLists.Length; i++)
            {
                InstanceList list = this.rawDeserializedLists[i];
                Activity activity;
                if (!QualifiedId.TryGetElementFromRoot(rootActivity, list.ActivityId, out activity))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.ActivityInstanceFixupFailed));
                }
                this.instanceMapping.Add(activity, list);
                list.Load(activity, this);
            }

            // We need to null this out once we've recreated the dictionary to avoid
            // having out of sync data
            this.rawDeserializedLists = null;

            // then walk our instance list, fixup parent references, and perform basic validation
            Func<ActivityInstance, ActivityExecutor, bool> processInstanceCallback = new Func<ActivityInstance, ActivityExecutor, bool>(OnActivityInstanceLoaded);

            rootInstance.FixupInstance(null, this, executor);
            ActivityUtilities.ProcessActivityInstanceTree(rootInstance, executor, processInstanceCallback);

            if (secondaryRootInstances != null)
            {
                foreach (ActivityInstance instance in secondaryRootInstances)
                {
                    instance.FixupInstance(null, this, executor);
                    ActivityUtilities.ProcessActivityInstanceTree(instance, executor, processInstanceCallback);
                }
            }
        }

        bool OnActivityInstanceLoaded(ActivityInstance activityInstance, ActivityExecutor executor)
        {
            return activityInstance.TryFixupChildren(this, executor);
        }

        public bool RemoveEntry(IActivityReference reference)
        {
            if (this.instanceMapping == null)
            {
                return false;
            }

            Activity activity = reference.Activity;

            InstanceList mappedInstances;
            if (!this.InstanceMapping.TryGetValue(activity, out mappedInstances))
            {
                return false;
            }

            if (mappedInstances.Count == 1)
            {
                this.InstanceMapping.Remove(activity);
            }
            else
            {
                mappedInstances.Remove(reference);
            }

            return true;
        }

        [DataContract]
        internal class InstanceList : HybridCollection<IActivityReference>
        {
            public InstanceList(IActivityReference reference)
                : base(reference)
            {
            }

            [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode,
                Justification = "Called by serialization")]
            [DataMember]
            public byte[] ActivityId
            {
                get;
                set;
            }

            [OnSerializing]
            [SuppressMessage(FxCop.Category.Usage, FxCop.Rule.ReviewUnusedParameters)]
            [SuppressMessage(FxCop.Category.Usage, "CA2238:ImplementSerializationMethodsCorrectly",
                Justification = "Needs to be internal for serialization in partial trust. We have set InternalsVisibleTo(System.Runtime.Serialization) to allow this.")]
            internal void OnSerializing(StreamingContext context)
            {
                base.Compress();
            }

            public void Add(IActivityReference reference, bool skipIfDuplicate)
            {
                Fx.Assert(this.Count >= 1, "instance list should never be empty when we call Add");

                if (skipIfDuplicate)
                {
                    if (base.SingleItem != null)
                    {
                        if (base.SingleItem == reference)
                        {
                            return;
                        }
                    }
                    else
                    {
                        if (base.MultipleItems.Contains(reference))
                        {
                            return;
                        }
                    }
                }

                Add(reference);
            }

            public void Load(Activity activity, ActivityInstanceMap instanceMap)
            {
                Fx.Assert(this.Count >= 1, "instance list should never be empty on load");
                if (base.SingleItem != null)
                {
                    base.SingleItem.Load(activity, instanceMap);
                }
                else
                {
                    for (int i = 0; i < base.MultipleItems.Count; i++)
                    {
                        base.MultipleItems[i].Load(activity, instanceMap);
                    }
                }
            }

            public void UpdateEnvironments(EnvironmentUpdateMap map, Activity activity)
            {
                if (base.SingleItem != null)
                {
                    IActivityReferenceWithEnvironment reference = base.SingleItem as IActivityReferenceWithEnvironment;
                    if (reference != null)
                    {
                        reference.UpdateEnvironment(map, activity);
                    }
                }
                else
                {
                    for (int i = 0; i < base.MultipleItems.Count; i++)
                    {
                        IActivityReferenceWithEnvironment reference = base.MultipleItems[i] as IActivityReferenceWithEnvironment;
                        if (reference != null)
                        {
                            reference.UpdateEnvironment(map, activity);
                        }
                    }
                }
            }

        }

        public interface IActivityReference
        {
            Activity Activity { get; }
            void Load(Activity activity, ActivityInstanceMap instanceMap);
        }

        public interface IActivityReferenceWithEnvironment : IActivityReference
        {
            void UpdateEnvironment(EnvironmentUpdateMap map, Activity activity);
        }

        class InstanceListNeedingUpdate
        {
            // The list of IActivityReferences to be updated
            public InstanceList InstanceList { get; set; }

            public byte[] OriginalId { get; set; }

            // The new ActivityId for these ActivityReferences.
            public QualifiedId NewId { get; set; }

            // The Map & MapEntry for this ActivityId, if there is one.
            // Null if the activity's parent Id was updated, but not the activity itself,
            // Or null if nothing changed.
            public DynamicUpdateMap UpdateMap { get; set; }
            public DynamicUpdateMapEntry MapEntry { get; set; }

            // A pointer to this activity, in the new definition.
            // Null if we don't have the definition loaded.
            public Activity NewActivity { get; set; }

            // 
            // the following three properties are mutual exlusive,
            // meaning, one and only one of them evaluates to TRUE.
            //
            public bool NothingChanged
            {
                get
                {
                    return this.MapEntry == null && this.NewId == null;
                }
            }

            public bool MapEntryExists
            {
                get
                {
                    return this.MapEntry != null;
                }
            }

            public bool ParentIdShiftOnly
            {
                get
                {
                    return this.MapEntry == null && this.NewId != null;
                }
            }        
        }
    }
}
