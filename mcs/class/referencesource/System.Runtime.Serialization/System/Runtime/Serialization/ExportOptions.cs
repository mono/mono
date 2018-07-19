//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    using System.Security;
    using System.Security.Permissions;
    using System.Collections.ObjectModel;

    public class ExportOptions
    {
        Collection<Type> knownTypes;
        IDataContractSurrogate dataContractSurrogate;

        public IDataContractSurrogate DataContractSurrogate
        {
            get { return dataContractSurrogate; }
            set { dataContractSurrogate = value; }
        }

        internal IDataContractSurrogate GetSurrogate()
        {
            return dataContractSurrogate;
        }

        public Collection<Type> KnownTypes
        {
            get
            {
                if (knownTypes == null)
                {
                    knownTypes = new Collection<Type>();
                }
                return knownTypes;
            }
        }
    }
}

