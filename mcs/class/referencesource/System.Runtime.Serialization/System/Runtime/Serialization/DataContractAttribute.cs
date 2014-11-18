//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public sealed class DataContractAttribute : Attribute
    {
        string name;
        string ns;
        bool isNameSetExplicit;
        bool isNamespaceSetExplicit;
        bool isReference;
        bool isReferenceSetExplicit;

        public DataContractAttribute()
        {
        }

        public bool IsReference
        {
            get { return isReference; }
            set
            {
                isReference = value;
                isReferenceSetExplicit = true;
            }
        }

        internal bool IsReferenceSetExplicit
        {
            get { return isReferenceSetExplicit; }
        }

        public string Namespace
        {
            get { return ns; }
            set
            {
                ns = value;
                isNamespaceSetExplicit = true;
            }
        }

        internal bool IsNamespaceSetExplicit
        {
            get { return isNamespaceSetExplicit; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                isNameSetExplicit = true;
            }
        }

        internal bool IsNameSetExplicit
        {
            get { return isNameSetExplicit; }
        }

    }
}
