//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Statements
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using System.Runtime;
    using System.Diagnostics.CodeAnalysis;
    using System.ComponentModel;

    [Fx.Tag.XamlVisible(false)]
    [DataContract]
    public sealed class CompensationToken
    {
        internal const string PropertyName = "System.Compensation.CompensationToken";
        internal const long RootCompensationId = 0;
            
        internal CompensationToken(CompensationTokenData tokenData)
        {
            this.CompensationId = tokenData.CompensationId;
        }
        
        [DataMember(EmitDefaultValue = false)]
        internal long CompensationId
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        internal bool CompensateCalled
        {
            get;
            set;
        }

        [DataMember(EmitDefaultValue = false)]
        internal bool ConfirmCalled
        {
            get;
            set;
        }
    }
}
