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
    internal class EnvironmentUpdateMapEntry
    {
        internal const int NonExistent = -1;

        public EnvironmentUpdateMapEntry()
        {
        }

        [DataMember(EmitDefaultValue = false)]
        public int OldOffset
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public int NewOffset
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        public bool IsNewHandle
        {
            get;
            set;
        }

        internal bool IsAddition
        {
            get
            {
                return this.OldOffset == EnvironmentUpdateMapEntry.NonExistent;
            }
        }

        internal static EnvironmentUpdateMapEntry Merge(EnvironmentUpdateMapEntry first, EnvironmentUpdateMapEntry second)
        {
            if (first == null || second == null)
            {
                return first ?? second;
            }

            Fx.Assert(first.NewOffset == second.OldOffset && !second.IsAddition, "Merging mismatched entries");
            if (first.OldOffset == second.NewOffset)
            {
                return null;
            }

            return new EnvironmentUpdateMapEntry
            {
                OldOffset = first.OldOffset,
                NewOffset = second.NewOffset,
                IsNewHandle = first.IsNewHandle
            };
        }
    }
}
