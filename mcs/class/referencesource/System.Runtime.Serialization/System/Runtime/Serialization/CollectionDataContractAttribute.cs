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
        bool isNameSetExplicitly;
        bool isNamespaceSetExplicitly;
        bool isReferenceSetExplicitly;
        bool isItemNameSetExplicitly;
        bool isKeyNameSetExplicitly;
        bool isValueNameSetExplicitly;

        public CollectionDataContractAttribute()
        {
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

        public string ItemName
        {
            get { return itemName; }
            set
            {
                itemName = value;
                isItemNameSetExplicitly = true;
            }
        }

        public bool IsItemNameSetExplicitly
        {
            get { return isItemNameSetExplicitly; }
        }

        public string KeyName
        {
            get { return keyName; }
            set
            {
                keyName = value;
                isKeyNameSetExplicitly = true;
            }
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

        public bool IsKeyNameSetExplicitly
        {
            get { return isKeyNameSetExplicitly; }
        }

        public string ValueName
        {
            get { return valueName; }
            set
            {
                valueName = value;
                isValueNameSetExplicitly = true;
            }
        }

        public bool IsValueNameSetExplicitly
        {
            get { return isValueNameSetExplicitly; }
        }

    }
}
