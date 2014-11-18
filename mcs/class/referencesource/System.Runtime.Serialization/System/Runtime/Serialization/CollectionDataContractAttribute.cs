//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Runtime.Serialization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, Inherited = false, AllowMultiple = false)]
    public sealed class CollectionDataContractAttribute : Attribute
    {
        string name;
        string ns;
        string itemName;
        string keyName;
        string valueName;
        bool isReference;
        bool isNameSetExplicit;
        bool isNamespaceSetExplicit;
        bool isReferenceSetExplicit;
        bool isItemNameSetExplicit;
        bool isKeyNameSetExplicit;
        bool isValueNameSetExplicit;

        public CollectionDataContractAttribute()
        {
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

        public string ItemName
        {
            get { return itemName; }
            set
            {
                itemName = value;
                isItemNameSetExplicit = true;
            }
        }

        internal bool IsItemNameSetExplicit
        {
            get { return isItemNameSetExplicit; }
        }

        public string KeyName
        {
            get { return keyName; }
            set
            {
                keyName = value;
                isKeyNameSetExplicit = true;
            }
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

        internal bool IsKeyNameSetExplicit
        {
            get { return isKeyNameSetExplicit; }
        }

        public string ValueName
        {
            get { return valueName; }
            set
            {
                valueName = value;
                isValueNameSetExplicit = true;
            }
        }

        internal bool IsValueNameSetExplicit
        {
            get { return isValueNameSetExplicit; }
        }

    }
}
