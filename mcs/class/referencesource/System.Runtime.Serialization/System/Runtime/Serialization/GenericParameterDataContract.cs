//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Security;

    sealed class GenericParameterDataContract : DataContract
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        GenericParameterDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        internal GenericParameterDataContract(Type type)
            : base(new GenericParameterDataContractCriticalHelper(type))
        {
            helper = base.Helper as GenericParameterDataContractCriticalHelper;
        }

        internal int ParameterPosition
        {
            [Fx.Tag.SecurityNote(Critical = "Fetches the critical ParameterPosition property.",
                Safe = "ParameterPosition only needs to be protected for write.")]
            [SecuritySafeCritical]
            get { return helper.ParameterPosition; }
        }

        internal override bool IsBuiltInDataContract
        {
            get
            {
                return true;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds state used for deaing with generic parameters."
            + " Since the data is cached statically, we lock down access to it.")]
        [SecurityCritical(SecurityCriticalScope.Everything)]
        class GenericParameterDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            int parameterPosition;

            internal GenericParameterDataContractCriticalHelper(Type type)
                : base(type)
            {
                SetDataContractName(DataContract.GetStableName(type));
                this.parameterPosition = type.GenericParameterPosition;
            }

            internal int ParameterPosition
            {
                get { return parameterPosition; }
            }
        }

        internal override DataContract BindGenericParameters(DataContract[] paramContracts, Dictionary<DataContract, DataContract> boundContracts)
        {
            return paramContracts[ParameterPosition];
        }
    }
}

