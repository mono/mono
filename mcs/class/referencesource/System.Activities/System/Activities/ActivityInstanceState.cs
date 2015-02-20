//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum ActivityInstanceState
    {
        [EnumMember]
        Executing,

        [EnumMember]
        Closed,

        [EnumMember]
        Canceled,

        [EnumMember]
        Faulted,
    }
}
