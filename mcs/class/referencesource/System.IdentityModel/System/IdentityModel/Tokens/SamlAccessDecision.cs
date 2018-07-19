//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.IdentityModel.Tokens
{
    using System.Runtime.Serialization;

    [DataContract]
    public enum SamlAccessDecision
    {
        [EnumMember]
        Permit,
        [EnumMember]
        Deny,
        [EnumMember]
        Indeterminate
    }
}
