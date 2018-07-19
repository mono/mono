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

    [DataContract]
    internal class EnvironmentUpdateMap
    {
        IList<EnvironmentUpdateMapEntry> variableEntries;       
        IList<EnvironmentUpdateMapEntry> privateVariableEntries;       
        IList<EnvironmentUpdateMapEntry> argumentEntries;       
        
        public EnvironmentUpdateMap()
        {
        }

        [DataMember(EmitDefaultValue = false)]
        public int NewArgumentCount
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public int OldArgumentCount
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public int NewVariableCount
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public int OldVariableCount
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public int NewPrivateVariableCount
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public int OldPrivateVariableCount
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public int RuntimeDelegateArgumentCount
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false, Name = "variableEntries")]
        internal IList<EnvironmentUpdateMapEntry> SerializedVariableEntries
        {
            get { return this.variableEntries; }
            set { this.variableEntries = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "privateVariableEntries")]
        internal IList<EnvironmentUpdateMapEntry> SerializedPrivateVariableEntries
        {
            get { return this.privateVariableEntries; }
            set { this.privateVariableEntries = value; }
        }

        [DataMember(EmitDefaultValue = false, Name = "argumentEntries")]
        internal IList<EnvironmentUpdateMapEntry> SerializedArgumentEntries
        {
            get { return this.argumentEntries; }
            set { this.argumentEntries = value; }
        }

        internal bool IsAdditionToNoSymbols
        {
            get
            {
                return (this.OldArgumentCount + this.OldVariableCount + this.OldPrivateVariableCount + this.RuntimeDelegateArgumentCount) == 0 &&
                (this.NewArgumentCount + this.NewVariableCount + this.NewPrivateVariableCount + this.RuntimeDelegateArgumentCount) > 0;
            }
        }

        internal bool HasVariableEntries
        {
            get
            {
                return this.variableEntries != null && this.variableEntries.Count > 0;
            }
        }

        internal bool HasPrivateVariableEntries
        {
            get
            {
                return this.privateVariableEntries != null && this.privateVariableEntries.Count > 0;
            }
        }

        internal bool HasArgumentEntries
        {
            get
            {
                return this.argumentEntries != null && this.argumentEntries.Count > 0;
            }
        }

        public IList<EnvironmentUpdateMapEntry> VariableEntries
        {
            get
            {
                if (this.variableEntries == null)
                {
                    this.variableEntries = new List<EnvironmentUpdateMapEntry>();
                }

                return this.variableEntries;
            }
        }

        public IList<EnvironmentUpdateMapEntry> PrivateVariableEntries
        {
            get
            {
                if (this.privateVariableEntries == null)
                {
                    this.privateVariableEntries = new List<EnvironmentUpdateMapEntry>();
                }

                return this.privateVariableEntries;
            }
        }

        public IList<EnvironmentUpdateMapEntry> ArgumentEntries
        {
            get
            {
                if (this.argumentEntries == null)
                {
                    this.argumentEntries = new List<EnvironmentUpdateMapEntry>();
                }

                return this.argumentEntries;
            }
        }

        internal static EnvironmentUpdateMap Merge(EnvironmentUpdateMap first, EnvironmentUpdateMap second,
            DynamicUpdateMap.MergeErrorContext errorContext)
        {
            if (first == null || second == null)
            {
                return first ?? second;
            }

            ThrowIfMapsIncompatible(first, second, errorContext);

            EnvironmentUpdateMap result = new EnvironmentUpdateMap
            {
                OldArgumentCount = first.OldArgumentCount,
                NewArgumentCount = second.NewArgumentCount,
                OldVariableCount = first.OldVariableCount,
                NewVariableCount = second.NewVariableCount,
                OldPrivateVariableCount = first.OldPrivateVariableCount,
                NewPrivateVariableCount = second.NewPrivateVariableCount,
            };

            result.variableEntries = Merge(result.NewVariableCount, first.VariableEntries, second.VariableEntries);
            result.privateVariableEntries = Merge(result.NewPrivateVariableCount, first.PrivateVariableEntries, second.PrivateVariableEntries);
            result.argumentEntries = Merge(result.NewArgumentCount, first.ArgumentEntries, second.ArgumentEntries);

            if (result.OldArgumentCount != result.NewArgumentCount ||
                result.OldVariableCount != result.NewVariableCount ||
                result.OldPrivateVariableCount != result.NewPrivateVariableCount ||
                result.HasArgumentEntries || result.HasVariableEntries || result.HasPrivateVariableEntries)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        internal int? GetOldVariableIndex(int newIndex)
        {
            EnvironmentUpdateMapEntry environmentEntry = FindByNewIndex(this.VariableEntries, newIndex);
            if (environmentEntry != null)
            {
                return environmentEntry.IsAddition ? (int?)null : environmentEntry.OldOffset;
            }
            return null;
        }

        internal int? GetNewVariableIndex(int oldIndex)
        {
            foreach (EnvironmentUpdateMapEntry environmentEntry in this.VariableEntries)
            {
                if (environmentEntry.OldOffset == oldIndex)
                {
                    return environmentEntry.NewOffset;
                }
            }

            return null;
        }

        internal int? GetNewPrivateVariableIndex(int oldIndex)
        {
            foreach (EnvironmentUpdateMapEntry environmentEntry in this.PrivateVariableEntries)
            {
                if (environmentEntry.OldOffset == oldIndex)
                {
                    return environmentEntry.NewOffset;
                }
            }

            return null;
        }

        static void ThrowIfMapsIncompatible(EnvironmentUpdateMap first, EnvironmentUpdateMap second,
            DynamicUpdateMap.MergeErrorContext errorContext)
        {
            if (first.NewArgumentCount != second.OldArgumentCount ||
                first.NewVariableCount != second.OldVariableCount ||
                first.NewPrivateVariableCount != second.OldPrivateVariableCount)
            {
                errorContext.Throw(SR.InvalidMergeMapEnvironmentCount(
                    first.NewArgumentCount, first.NewVariableCount, first.NewPrivateVariableCount,
                    second.OldArgumentCount, second.OldVariableCount, second.OldPrivateVariableCount));
            }
        }

        static IList<EnvironmentUpdateMapEntry> Merge(int finalCount, IList<EnvironmentUpdateMapEntry> first,
            IList<EnvironmentUpdateMapEntry> second)
        {
            List<EnvironmentUpdateMapEntry> result = new List<EnvironmentUpdateMapEntry>();
            for (int i = 0; i < finalCount; i++)
            {
                EnvironmentUpdateMapEntry resultEntry = MergeEntry(i, first, second);
                if (resultEntry != null)
                {
                    result.Add(resultEntry);
                }
            }

            return result.Count > 0 ? result : null;
        }

        static EnvironmentUpdateMapEntry MergeEntry(int finalIndex, IList<EnvironmentUpdateMapEntry> first,
            IList<EnvironmentUpdateMapEntry> second)
        {
            EnvironmentUpdateMapEntry secondEntry = FindByNewIndex(second, finalIndex);
            EnvironmentUpdateMapEntry firstEntry;
            if (secondEntry != null)
            {
                firstEntry = secondEntry.IsAddition ? null : FindByNewIndex(first, secondEntry.OldOffset);
            }
            else
            {
                firstEntry = FindByNewIndex(first, finalIndex);
            }

            return EnvironmentUpdateMapEntry.Merge(firstEntry, secondEntry);
        }

        static EnvironmentUpdateMapEntry FindByNewIndex(IList<EnvironmentUpdateMapEntry> entries, int newIndex)
        {
            foreach (EnvironmentUpdateMapEntry environmentEntry in entries)
            {
                if (environmentEntry.NewOffset == newIndex)
                {
                    return environmentEntry;
                }
            }

            return null;
        }
    }
}
