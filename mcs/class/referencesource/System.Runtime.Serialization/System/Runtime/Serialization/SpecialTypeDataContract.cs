//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Xml;
    using System.Security;

    sealed class SpecialTypeDataContract : DataContract
    {
        [Fx.Tag.SecurityNote(Critical = "Holds instance of CriticalHelper which keeps state that is cached statically for serialization."
            + " Static fields are marked SecurityCritical or readonly to prevent data from being modified or leaked to other components in appdomain.")]
        [SecurityCritical]
        SpecialTypeDataContractCriticalHelper helper;

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        public SpecialTypeDataContract(Type type)
            : base(new SpecialTypeDataContractCriticalHelper(type))
        {
            helper = base.Helper as SpecialTypeDataContractCriticalHelper;
        }

        [Fx.Tag.SecurityNote(Critical = "Initializes SecurityCritical field 'helper'.",
            Safe = "Doesn't leak anything.")]
        [SecuritySafeCritical]
        public SpecialTypeDataContract(Type type, XmlDictionaryString name, XmlDictionaryString ns)
            : base(new SpecialTypeDataContractCriticalHelper(type, name, ns))
        {
            helper = base.Helper as SpecialTypeDataContractCriticalHelper;
        }

        internal override bool IsBuiltInDataContract
        {
            get
            {
                return true;
            }
        }

        [Fx.Tag.SecurityNote(Critical = "Holds all state used for for (de)serializing known types like System.Enum, System.ValueType, etc."
            + " Since the data is cached statically, we lock down access to it.")]
        [SecurityCritical(SecurityCriticalScope.Everything)]
        class SpecialTypeDataContractCriticalHelper : DataContract.DataContractCriticalHelper
        {
            internal SpecialTypeDataContractCriticalHelper(Type type)
                : base(type)
            {
            }

            internal SpecialTypeDataContractCriticalHelper(Type type, XmlDictionaryString name, XmlDictionaryString ns)
                : base(type)
            {
                SetDataContractName(name, ns);
            }
        }
    }
}

