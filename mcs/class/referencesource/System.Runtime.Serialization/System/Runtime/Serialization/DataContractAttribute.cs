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
        bool isNameSetExplicitly;
        bool isNamespaceSetExplicitly;
        bool isReference;
        bool isReferenceSetExplicitly;

        public DataContractAttribute()
        {
        }

        public bool IsReference
        {
            get { return isReference; }
            set
            {
                isReference = value;
                isReferenceSetExplicitly = true;
            }
        }

        public bool IsReferenceSetExplicitly
        {
            get { return isReferenceSetExplicitly; }
        }

        public string Namespace
        {
            get { return ns; }
            set
            {
                ns = value;
                isNamespaceSetExplicitly = true;
            }
        }

        public bool IsNamespaceSetExplicitly
        {
            get { return isNamespaceSetExplicitly; }
        }

        public string Name
        {
            get { return name; }
            set
            {
                name = value;
                isNameSetExplicitly = true;
            }
        }

        public bool IsNameSetExplicitly
        {
            get { return isNameSetExplicitly; }
        }

    }
}
