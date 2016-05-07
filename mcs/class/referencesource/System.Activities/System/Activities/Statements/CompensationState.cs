//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System;
    using System.Runtime.Serialization;

    [DataContract]
    enum CompensationState
    {
        [EnumMember]
        Creating,

        [EnumMember]
        Active,

        [EnumMember]
        Completed,

        [EnumMember]
        Confirming,

        [EnumMember]
        Confirmed,

        [EnumMember]
        Compensating,

        [EnumMember]
        Compensated,

        [EnumMember]
        Canceling,

        [EnumMember]
        Canceled,
    }
}
