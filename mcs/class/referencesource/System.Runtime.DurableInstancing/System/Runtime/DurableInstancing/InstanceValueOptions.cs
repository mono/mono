//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Runtime.DurableInstancing
{
    using System;
    using System.Runtime.Serialization;

    [Flags]
    [DataContract]
    public enum InstanceValueOptions
    {
        [EnumMember]
        None = 0,

        [EnumMember]
        Optional = 1,

        [EnumMember]
        WriteOnly = 2,
    }
}
