//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.DynamicUpdate
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.Serialization;
    using System.ComponentModel;

    [DataContract(IsReference = true)]
    internal class DynamicUpdateMapEntry
    {
        static DynamicUpdateMapEntry dummyMapEntry = new DynamicUpdateMapEntry(-1, -1);

        DynamicUpdateMap implementationUpdateMap;
        int oldActivityId;
        int newActivityId;        
               
        public DynamicUpdateMapEntry(int oldActivityId, int newActivityId)
        {
            this.OldActivityId = oldActivityId;
            this.NewActivityId = newActivityId;
        }
        
        // this is a dummy map entry to be used for creating a NativeActivityUpdateContext
        // for calling UpdateInstance() on activities without map entries.
        // the OldActivityId and NewActivityId of this dummy map entry are invalid, 
        // and should not be used anywhere except for creating NativeActivityUpdateContext.
        internal static DynamicUpdateMapEntry DummyMapEntry
        {
            get { return dummyMapEntry; }
        }        

        public int OldActivityId
        {
            get
            {
                return this.oldActivityId;
            }
            private set
            {
                this.oldActivityId = value;
            }
        }        

        public int NewActivityId
        {
            get
            {
                return this.newActivityId;
            }
            private set
            {
                this.newActivityId = value;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public DynamicUpdateMapEntry Parent
        {
            get;
            set;
        }

        // Only set when IsRemoval == true && IsParentRemovedOrBlock == false
        [DataMember(EmitDefaultValue = false)]
        public string DisplayName
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public UpdateBlockedReason BlockReason { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string BlockReasonMessage { get; set; }

        public bool IsRuntimeUpdateBlocked
        {
            get
            {
                return BlockReason != UpdateBlockedReason.NotBlocked;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public bool IsUpdateBlockedByUpdateAuthor { get; set; }

        public bool IsParentRemovedOrBlocked
        {
            get
            {
                for (DynamicUpdateMapEntry parent = this.Parent; parent != null; parent = parent.Parent)
                {
                    if (parent.IsRemoval || parent.IsRuntimeUpdateBlocked || parent.IsUpdateBlockedByUpdateAuthor)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        [DataMember(EmitDefaultValue = false)]
        public IDictionary<string, object> SavedOriginalValues { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object SavedOriginalValueFromParent { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public EnvironmentUpdateMap EnvironmentUpdateMap
        {
            get;
            set;
        }

        public DynamicUpdateMap ImplementationUpdateMap
        {
            get
            {
                return this.implementationUpdateMap;
            }
            internal set
            {
                this.implementationUpdateMap = value;
            }
        }

        [DataMember(EmitDefaultValue = false, Name = "implementationUpdateMap")]
        internal DynamicUpdateMap SerializedImplementationUpdateMap
        {
            get { return this.implementationUpdateMap; }
            set { this.implementationUpdateMap = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "OldActivityId")]
        internal int SerializedOldActivityId
        {
            get { return this.OldActivityId; }
            set { this.OldActivityId = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "NewActivityId")]
        internal int SerializedNewActivityId
        {
            get { return this.NewActivityId; }
            set { this.NewActivityId = value; }
        }
        
        internal bool IsIdChange
        {
            get
            {
                return this.NewActivityId > 0 && this.OldActivityId > 0 && this.NewActivityId != this.OldActivityId;
            }
        }

        internal bool IsRemoval
        {
            get
            {
                return this.NewActivityId <= 0 && this.OldActivityId > 0;
            }
        }

        internal bool HasEnvironmentUpdates
        {
            get
            {
                return this.EnvironmentUpdateMap != null;
            }
        }

        internal static DynamicUpdateMapEntry Merge(DynamicUpdateMapEntry first, DynamicUpdateMapEntry second,
            DynamicUpdateMapEntry newParent, DynamicUpdateMap.MergeErrorContext errorContext)
        {
            Fx.Assert(first.NewActivityId == second.OldActivityId, "Merging mismatched entries");
            Fx.Assert((first.Parent == null && second.Parent == null) || (first.Parent.NewActivityId == second.Parent.OldActivityId), "Merging mismatched parents");

            DynamicUpdateMapEntry result = new DynamicUpdateMapEntry(first.OldActivityId, second.NewActivityId)
            {
                Parent = newParent
            };

            if (second.IsRemoval)
            {
                if (!result.IsParentRemovedOrBlocked)
                {
                    result.DisplayName = second.DisplayName;
                }
            }
            else
            {
                result.SavedOriginalValues = Merge(first.SavedOriginalValues, second.SavedOriginalValues);
                result.SavedOriginalValueFromParent = first.SavedOriginalValueFromParent ?? second.SavedOriginalValueFromParent;
                if (first.BlockReason == UpdateBlockedReason.NotBlocked)
                {
                    result.BlockReason = second.BlockReason;
                    result.BlockReasonMessage = second.BlockReasonMessage;
                }
                else
                {
                    result.BlockReason = first.BlockReason;
                    result.BlockReasonMessage = second.BlockReasonMessage;
                }
                result.IsUpdateBlockedByUpdateAuthor = first.IsUpdateBlockedByUpdateAuthor || second.IsUpdateBlockedByUpdateAuthor;

                errorContext.PushIdSpace(result.NewActivityId);
                result.EnvironmentUpdateMap = EnvironmentUpdateMap.Merge(first.EnvironmentUpdateMap, second.EnvironmentUpdateMap, errorContext);
                if (!result.IsRuntimeUpdateBlocked && !result.IsUpdateBlockedByUpdateAuthor && !result.IsParentRemovedOrBlocked)
                {
                    result.ImplementationUpdateMap = DynamicUpdateMap.Merge(first.ImplementationUpdateMap, second.ImplementationUpdateMap, errorContext);
                }
                errorContext.PopIdSpace();
            };

            return result;
        }

        internal static IDictionary<string, object> Merge(IDictionary<string, object> first, IDictionary<string, object> second)
        {
            if (first == null || second == null)
            {
                return first ?? second;
            }

            Dictionary<string, object> result = new Dictionary<string, object>(first);
            foreach (KeyValuePair<string, object> pair in second)
            {
                if (!result.ContainsKey(pair.Key))
                {
                    result.Add(pair.Key, pair.Value);
                }
            }

            return result;
        }

        internal DynamicUpdateMapEntry Clone(DynamicUpdateMapEntry newParent)
        {
            return new DynamicUpdateMapEntry(this.OldActivityId, this.NewActivityId)
            {
                DisplayName = this.DisplayName,
                EnvironmentUpdateMap = this.EnvironmentUpdateMap,
                ImplementationUpdateMap = this.ImplementationUpdateMap,
                BlockReason = this.BlockReason,
                BlockReasonMessage = this.BlockReasonMessage,
                IsUpdateBlockedByUpdateAuthor = this.IsUpdateBlockedByUpdateAuthor,
                Parent = newParent,
                SavedOriginalValues = this.SavedOriginalValues,
                SavedOriginalValueFromParent = this.SavedOriginalValueFromParent
            };
        }
    }
}
