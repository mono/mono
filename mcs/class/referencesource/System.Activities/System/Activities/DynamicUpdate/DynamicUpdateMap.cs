// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Activities.DynamicUpdate;
    using System.Activities.XamlIntegration;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Globalization;
    using System.Runtime;
    using System.Runtime.Serialization;

    [DataContract]
    [TypeConverter(typeof(DynamicUpdateMapConverter))]
    public class DynamicUpdateMap
    {
        static DynamicUpdateMap noChanges = new DynamicUpdateMap();
        static DynamicUpdateMap dummyMap = new DynamicUpdateMap();

        internal EntryCollection entries;        
        IList<ArgumentInfo> newArguments;
        IList<ArgumentInfo> oldArguments;        
        
        internal DynamicUpdateMap()
        {
        }

        public static DynamicUpdateMap NoChanges
        {
            get
            {
                return noChanges;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "entries")]
        internal EntryCollection SerializedEntries
        {
            get { return this.entries; }
            set { this.entries = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "newArguments")]
        internal IList<ArgumentInfo> SerializedNewArguments
        {
            get { return this.newArguments; }
            set { this.newArguments = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "oldArguments")]
        internal IList<ArgumentInfo> SerializedOldArguments
        {
            get { return this.oldArguments; }
            set { this.oldArguments = value; }
        }

        // this is a dummy map to be used for creating a NativeActivityUpdateContext
        // for calling UpdateInstance() on activities without map entries.
        // this should not be used anywhere except for creating NativeActivityUpdateContext.
        internal static DynamicUpdateMap DummyMap
        {
            get { return dummyMap; }
        }

        internal IList<ArgumentInfo> NewArguments
        {
            get
            {
                if (this.newArguments == null)
                {
                    this.newArguments = new List<ArgumentInfo>();
                }
                return this.newArguments;
            }
            set
            {
                this.newArguments = value;
            }
        }

        internal IList<ArgumentInfo> OldArguments
        {
            get
            {
                if (this.oldArguments == null)
                {
                    this.oldArguments = new List<ArgumentInfo>();
                }
                return this.oldArguments;
            }
            set
            {
                this.oldArguments = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        internal bool ArgumentsAreUnknown
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        internal bool IsImplementationAsRoot
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        internal int NewDefinitionMemberCount
        {
            get;
            set;
        }

        internal int OldDefinitionMemberCount
        {
            get
            {
                return this.Entries.Count;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        internal bool IsForImplementation { get; set; }

        // IdSpaces always have at least one member. So a count of 0 means that this is
        // DynamicUpdateMap.NoChanges, or a serialized equivalent.
        internal bool IsNoChanges
        {
            get
            {
                return this.NewDefinitionMemberCount == 0;
            }
        }

        // use the internal method AddEntry() instead
        private IList<DynamicUpdateMapEntry> Entries
        {
            get
            {
                if (this.entries == null)
                {
                    this.entries = new EntryCollection();
                }

                return this.entries;
            }
        }

        public static IDictionary<object, DynamicUpdateMapItem> CalculateMapItems(Activity workflowDefinitionToBeUpdated)
        {
            return CalculateMapItems(workflowDefinitionToBeUpdated, null);
        }

        public static IDictionary<object, DynamicUpdateMapItem> CalculateMapItems(Activity workflowDefinitionToBeUpdated, LocationReferenceEnvironment environment)
        {
            return InternalCalculateMapItems(workflowDefinitionToBeUpdated, environment, false);
        }

        public static IDictionary<object, DynamicUpdateMapItem> CalculateImplementationMapItems(Activity activityDefinitionToBeUpdated)
        {
            return CalculateImplementationMapItems(activityDefinitionToBeUpdated, null);
        }

        public static IDictionary<object, DynamicUpdateMapItem> CalculateImplementationMapItems(Activity activityDefinitionToBeUpdated, LocationReferenceEnvironment environment)
        {
            return InternalCalculateMapItems(activityDefinitionToBeUpdated, environment, true);
        }

        public static DynamicUpdateMap Merge(params DynamicUpdateMap[] maps)
        {
            return Merge((IEnumerable<DynamicUpdateMap>)maps);
        }

        public static DynamicUpdateMap Merge(IEnumerable<DynamicUpdateMap> maps)
        {
            if (maps == null)
            {
                throw FxTrace.Exception.ArgumentNull("maps");
            }

            // We could try to optimize this by merging the entire set at once, but it's simpler
            // to just do pairwise merging
            int index = 0;
            DynamicUpdateMap result = null;
            foreach (DynamicUpdateMap nextMap in maps)
            {
                result = Merge(result, nextMap, new MergeErrorContext { MapIndex = index });
                index++;
            }

            return result;
        }

        static IDictionary<object, DynamicUpdateMapItem> InternalCalculateMapItems(Activity workflowDefinitionToBeUpdated, LocationReferenceEnvironment environment, bool forImplementation)
        {
            if (workflowDefinitionToBeUpdated == null)
            {
                throw FxTrace.Exception.ArgumentNull("workflowDefinitionToBeUpdated");
            }

            DynamicUpdateMapBuilder.Preparer preparer = new DynamicUpdateMapBuilder.Preparer(workflowDefinitionToBeUpdated, environment, forImplementation);
            return preparer.Prepare();
        }        

        public DynamicUpdateMapQuery Query(Activity updatedWorkflowDefinition, Activity originalWorkflowDefinition)
        {            
            if (this.IsNoChanges)
            {
                throw FxTrace.Exception.AsError(new InvalidOperationException(SR.NoChangesMapQueryNotSupported));
            }

            if (this.IsForImplementation)
            {
                ValidateDefinitionMatchesImplementationMap(updatedWorkflowDefinition, this.NewDefinitionMemberCount, "updatedWorkflowDefinition");
                ValidateDefinitionMatchesImplementationMap(originalWorkflowDefinition, this.OldDefinitionMemberCount, "originalWorkflowDefinition");
            }
            else
            {
                ValidateDefinitionMatchesMap(updatedWorkflowDefinition, this.NewDefinitionMemberCount, "updatedWorkflowDefinition");
                ValidateDefinitionMatchesMap(originalWorkflowDefinition, this.OldDefinitionMemberCount, "originalWorkflowDefinition");
            }            

            return new DynamicUpdateMapQuery(this, updatedWorkflowDefinition, originalWorkflowDefinition);
        }

        internal static bool CanUseImplementationMapAsRoot(Activity workflowDefinition)
        {
            Fx.Assert(workflowDefinition.IsMetadataCached, "This should only be called for cached definition");

            // We can only use the implementation map as a root map if the worklflow has no public children
            return
                workflowDefinition.Children.Count == 0 &&
                workflowDefinition.ImportedChildren.Count == 0 &&
                workflowDefinition.Delegates.Count == 0 &&
                workflowDefinition.ImportedDelegates.Count == 0 &&
                workflowDefinition.RuntimeVariables.Count == 0;
        }

        internal static DynamicUpdateMap Merge(DynamicUpdateMap first, DynamicUpdateMap second, MergeErrorContext errorContext)
        {
            if (first == null || second == null)
            {
                return first ?? second;
            }

            if (first.IsNoChanges || second.IsNoChanges)
            {
                // DynamicUpdateMap.NoChanges has zero members, so we need to special-case it here.
                return first.IsNoChanges ? second : first;
            }

            ThrowIfMapsIncompatible(first, second, errorContext);

            DynamicUpdateMap result = new DynamicUpdateMap
            {
                IsForImplementation = first.IsForImplementation,
                NewDefinitionMemberCount = second.NewDefinitionMemberCount,
                ArgumentsAreUnknown = first.ArgumentsAreUnknown && second.ArgumentsAreUnknown,
                oldArguments = first.ArgumentsAreUnknown ? second.oldArguments : first.oldArguments,
                newArguments = second.ArgumentsAreUnknown ? first.newArguments : second.newArguments
            };

            foreach (DynamicUpdateMapEntry firstEntry in first.Entries)
            {
                DynamicUpdateMapEntry parent = null;
                if (firstEntry.Parent != null)
                {
                    result.TryGetUpdateEntry(firstEntry.Parent.OldActivityId, out parent);
                }

                if (firstEntry.IsRemoval)
                {
                    result.AddEntry(firstEntry.Clone(parent));
                }
                else
                {
                    DynamicUpdateMapEntry secondEntry = second.entries[firstEntry.NewActivityId];
                    result.AddEntry(DynamicUpdateMapEntry.Merge(firstEntry, secondEntry, parent, errorContext));
                }
            }

            return result;
        }        

        internal void AddEntry(DynamicUpdateMapEntry entry)
        {
            this.Entries.Add(entry);
        }

        // Wrap an implementation map in a dummy map. This allows use of an implementation map as the
        // root map in the case when the root is an x:Class with no public children.
        internal DynamicUpdateMap AsRootMap()
        {
            Fx.Assert(this.IsForImplementation, "This should only be called on implementation map");

            if (!ActivityComparer.ListEquals(this.NewArguments, this.OldArguments))
            {
                throw FxTrace.Exception.AsError(new InstanceUpdateException(SR.InvalidImplementationAsWorkflowRootForRuntimeStateBecauseArgumentsChanged));
            }

            DynamicUpdateMap result = new DynamicUpdateMap
            {
                IsImplementationAsRoot = true,
                NewDefinitionMemberCount = 1
            };
            result.AddEntry(new DynamicUpdateMapEntry(1, 1)
            {
                ImplementationUpdateMap = this,
            });
            return result;
        }

        internal void ThrowIfInvalid(Activity updatedDefinition)
        {
            Fx.Assert(updatedDefinition.IsMetadataCached, "Caller should have ensured cached definition");
            Fx.Assert(updatedDefinition.Parent == null && !this.IsForImplementation, "This should only be called on a workflow definition");

            this.ThrowIfInvalid(updatedDefinition.MemberOf);
        }

        // We verify that the count of all IdSpaces is as expected.
        // We could choose to be looser, and only check the IdSpaces that have children active;
        // but realistically, if all provided implementation maps don't match, something is probably wrong.
        // Conversely, we could check the correctness of every environment map, but it doesn't seem worth
        // doing that much work. If we find a mismatch on the environment of an executing activity, we'll
        // throw at that point.
        void ThrowIfInvalid(IdSpace updatedIdSpace)
        {
            if (this.IsNoChanges)
            {
                // 0 means this is NoChanges map, since every workflow has at least one member
                return;
            }

            if (this.NewDefinitionMemberCount != updatedIdSpace.MemberCount)
            {
                throw FxTrace.Exception.AsError(new InstanceUpdateException(SR.InvalidUpdateMap(
                    SR.WrongMemberCount(updatedIdSpace.Owner, updatedIdSpace.MemberCount, this.NewDefinitionMemberCount))));
            }

            foreach (DynamicUpdateMapEntry entry in this.Entries)
            {
                if (entry.ImplementationUpdateMap != null)
                {
                    Activity implementationOwner = updatedIdSpace[entry.NewActivityId];
                    if (implementationOwner == null)
                    {
                        string expectedId = entry.NewActivityId.ToString(CultureInfo.InvariantCulture);
                        if (updatedIdSpace.Owner != null)
                        {
                            expectedId = updatedIdSpace.Owner.Id + "." + expectedId;
                        }
                        throw FxTrace.Exception.AsError(new InstanceUpdateException(SR.InvalidUpdateMap(
                            SR.ActivityNotFound(expectedId))));
                    }

                    if (implementationOwner.ParentOf == null)
                    {
                        throw FxTrace.Exception.AsError(new InstanceUpdateException(SR.InvalidUpdateMap(
                            SR.ActivityHasNoImplementation(implementationOwner))));
                    }

                    entry.ImplementationUpdateMap.ThrowIfInvalid(implementationOwner.ParentOf);
                }
            }
        }

        internal bool TryGetUpdateEntryByNewId(int newId, out DynamicUpdateMapEntry entry)
        {
            Fx.Assert(!this.IsNoChanges, "This method is never supposed to be called on the NoChanges map.");

            entry = null;

            for (int i = 0; i < this.Entries.Count; i++)
            {
                DynamicUpdateMapEntry currentEntry = this.Entries[i];
                if (currentEntry.NewActivityId == newId)
                {
                    entry = currentEntry;
                    return true;
                }
            }
            return false;
        }

        internal bool TryGetUpdateEntry(int oldId, out DynamicUpdateMapEntry entry)
        {
            if (this.entries != null && this.entries.Count > 0)
            {
                if (this.entries.Contains(oldId))
                {
                    entry = this.entries[oldId];
                    return true;
                }
            }

            entry = null;
            return false;
        }

        // rootIdSpace is optional.  if it's null, result.NewActivity will be null
        internal UpdatedActivity GetUpdatedActivity(QualifiedId oldQualifiedId, IdSpace rootIdSpace)
        {
            UpdatedActivity result = new UpdatedActivity();
            int[] oldIdSegments = oldQualifiedId.AsIDArray();
            int[] newIdSegments = null;
            IdSpace currentIdSpace = rootIdSpace;
            DynamicUpdateMap currentMap = this;

            Fx.Assert(!this.IsForImplementation, "This method is never supposed to be called on an implementation map.");

            for (int i = 0; i < oldIdSegments.Length; i++)
            {
                if (currentMap == null || currentMap.Entries.Count == 0)
                {
                    break;
                }

                DynamicUpdateMapEntry entry;
                if (!currentMap.TryGetUpdateEntry(oldIdSegments[i], out entry))
                {
                    // UpdateMap should contain entries for all old activities in the IdSpace
                    int[] subIdSegments = new int[i + 1];
                    Array.Copy(oldIdSegments, subIdSegments, subIdSegments.Length);
                    throw FxTrace.Exception.AsError(new InstanceUpdateException(SR.InvalidUpdateMap(
                        SR.MapEntryNotFound(new QualifiedId(subIdSegments)))));
                }

                if (entry.IsIdChange)
                {
                    if (newIdSegments == null)
                    {
                        newIdSegments = new int[oldIdSegments.Length];
                        Array.Copy(oldIdSegments, newIdSegments, oldIdSegments.Length);
                    }

                    newIdSegments[i] = entry.NewActivityId;
                }

                Activity currentActivity = null;
                if (currentIdSpace != null && !entry.IsRemoval)
                {
                    currentActivity = currentIdSpace[entry.NewActivityId];
                    if (currentActivity == null)
                    {
                        // New Activity pointed to by UpdateMap should exist
                        string activityId = currentIdSpace.Owner.Id + "." + entry.NewActivityId.ToString(CultureInfo.InvariantCulture);
                        throw FxTrace.Exception.AsError(new InstanceUpdateException(SR.InvalidUpdateMap(
                            SR.ActivityNotFound(activityId))));
                    }
                    currentIdSpace = currentActivity.ParentOf;
                }

                if (i == oldIdSegments.Length - 1)
                {
                    result.Map = currentMap;
                    result.MapEntry = entry;
                    result.NewActivity = currentActivity;
                }
                else if (entry.IsRuntimeUpdateBlocked || entry.IsUpdateBlockedByUpdateAuthor)
                {
                    currentMap = null;
                }
                else
                {
                    currentMap = entry.ImplementationUpdateMap;
                }
            }

            result.IdChanged = newIdSegments != null;
            result.NewId = result.IdChanged ? new QualifiedId(newIdSegments) : oldQualifiedId;

            return result;
        }

        static void ThrowIfMapsIncompatible(DynamicUpdateMap first, DynamicUpdateMap second, MergeErrorContext errorContext)
        {
            Fx.Assert(!first.IsNoChanges && !second.IsNoChanges, "This method is never supposed to be called on the NoChanges map.");

            if (first.IsForImplementation != second.IsForImplementation)
            {
                errorContext.Throw(SR.InvalidMergeMapForImplementation(first.IsForImplementation, second.IsForImplementation));
            }
            if (first.NewDefinitionMemberCount != second.OldDefinitionMemberCount)
            {
                errorContext.Throw(SR.InvalidMergeMapMemberCount(first.NewDefinitionMemberCount, second.OldDefinitionMemberCount));
            }
            if (!first.ArgumentsAreUnknown && !second.ArgumentsAreUnknown && first.IsForImplementation && 
                !ActivityComparer.ListEquals(first.newArguments, second.oldArguments))
            {
                if (first.NewArguments.Count != second.OldArguments.Count)
                {
                    errorContext.Throw(SR.InvalidMergeMapArgumentCount(first.NewArguments.Count, second.OldArguments.Count));
                }
                else
                {
                    errorContext.Throw(SR.InvalidMergeMapArgumentsChanged);
                }
            }
        }

        static void ValidateDefinitionMatchesMap(Activity activity, int memberCount, string parameterName)
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull(parameterName);
            }
            if (activity.MemberOf == null)
            {
                throw FxTrace.Exception.Argument(parameterName, SR.ActivityIsUncached);
            }
            if (activity.Parent != null)
            {
                throw FxTrace.Exception.Argument(parameterName, SR.ActivityIsNotRoot);
            }
            if (activity.MemberOf.MemberCount != memberCount)
            {
                throw FxTrace.Exception.Argument(parameterName, SR.InvalidUpdateMap(
                    SR.WrongMemberCount(activity.MemberOf.Owner, activity.MemberOf.MemberCount, memberCount)));
            }
        }

        static void ValidateDefinitionMatchesImplementationMap(Activity activity, int memberCount, string parameterName)
        {
            if (activity == null)
            {
                throw FxTrace.Exception.ArgumentNull(parameterName);
            }
            if (activity.MemberOf == null)
            {
                throw FxTrace.Exception.Argument(parameterName, SR.ActivityIsUncached);
            }
            if (activity.Parent != null)
            {
                throw FxTrace.Exception.Argument(parameterName, SR.ActivityIsNotRoot);
            }
            if (activity.ParentOf == null)
            {
                throw FxTrace.Exception.Argument(parameterName, SR.InvalidUpdateMap(
                    SR.ActivityHasNoImplementation(activity)));
            }
            if (activity.ParentOf.MemberCount != memberCount)
            {
                throw FxTrace.Exception.Argument(parameterName, SR.InvalidUpdateMap(
                    SR.WrongMemberCount(activity.ParentOf.Owner, activity.ParentOf.MemberCount, memberCount)));
            }
            if (!CanUseImplementationMapAsRoot(activity))
            {
                throw FxTrace.Exception.Argument(parameterName, SR.InvalidImplementationAsWorkflowRoot);
            }
        }

        internal struct UpdatedActivity
        {
            // This can be true even if Map & MapEntry are null, if a parent ID changed.
            // It can also be false even when Map & MapEntry are non-null, if the update didn't produce an ID shift.
            public bool IdChanged;

            public QualifiedId NewId;

            // Null if the activity's IDSpace wasn't updated.
            public DynamicUpdateMap Map;
            public DynamicUpdateMapEntry MapEntry;

            // Null when we're dealing with just a serialized instance with no definition.
            public Activity NewActivity;
        }

        internal class MergeErrorContext
        {
            private Stack<int> currentIdSpace;
            public int MapIndex { get; set; }

            public void PushIdSpace(int id)
            {
                if (this.currentIdSpace == null)
                {
                    this.currentIdSpace = new Stack<int>();
                }
                this.currentIdSpace.Push(id);
            }

            public void PopIdSpace()
            {
                this.currentIdSpace.Pop();
            }

            public void Throw(string detail)
            {
                QualifiedId id = null;
                if (this.currentIdSpace != null && this.currentIdSpace.Count > 0)
                {
                    int[] idSegments = new int[this.currentIdSpace.Count];
                    for (int i = idSegments.Length - 1; i >= 0; i--)
                    {
                        idSegments[i] = this.currentIdSpace.Pop();
                    }
                    id = new QualifiedId(idSegments);
                }

                string errorMessage;
                if (id == null)
                {
                    errorMessage = SR.InvalidRootMergeMap(this.MapIndex, detail);
                }
                else
                {
                    errorMessage = SR.InvalidMergeMap(this.MapIndex, id, detail);
                }

                throw FxTrace.Exception.Argument("maps", errorMessage);
            }
        }

        [CollectionDataContract]
        internal class EntryCollection : KeyedCollection<int, DynamicUpdateMapEntry>
        {
            public EntryCollection()
            {
            }

            protected override int GetKeyForItem(DynamicUpdateMapEntry item)
            {
                return item.OldActivityId;
            }
        }
    }
}
