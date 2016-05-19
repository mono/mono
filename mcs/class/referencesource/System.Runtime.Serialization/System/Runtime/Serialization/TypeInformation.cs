//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    sealed class TypeInformation
    {
        string fullTypeName;
        string assemblyString;
        bool hasTypeForwardedFrom;

        internal TypeInformation(string fullTypeName, string assemblyString, bool hasTypeForwardedFrom)
        {
            this.fullTypeName = fullTypeName;
            this.assemblyString = assemblyString;
            this.hasTypeForwardedFrom = hasTypeForwardedFrom;
        }

        internal string FullTypeName
        {
            get
            {
                return this.fullTypeName;
            }
        }

        internal string AssemblyString
        {
            get
            {
                return this.assemblyString;
            }
        }

        internal bool HasTypeForwardedFrom
        {
            get
            {
                return this.hasTypeForwardedFrom;
            }
        }
    }
}
