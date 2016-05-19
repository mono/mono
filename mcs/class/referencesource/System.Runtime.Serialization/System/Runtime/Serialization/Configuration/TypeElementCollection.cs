//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization.Configuration
{

    using System;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(TypeElement), CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class TypeElementCollection : ConfigurationElementCollection
    {
        public TypeElementCollection()
        {
        }

        public TypeElement this[int index]
        {
            get
            {
                TypeElement retval = (TypeElement)BaseGet(index);
                return retval;
            }
            set
            {
                // Only validate input if config is not Read-Only, otherwise
                // let BaseAdd throw appropriate exception
                if (!this.IsReadOnly())
                {
                    if (value == null)
                    {
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                    }
                    if (BaseGet(index) != null)
                    {
                        BaseRemoveAt(index);
                    }
                }
                BaseAdd(index, value);
            }
        }

        public void Add(TypeElement element)
        {
            // Only validate input if config is not Read-Only, otherwise
            // let BaseAdd throw appropriate exception
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
            }
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            TypeElement retval = new TypeElement();
            return retval;
        }

        protected override string ElementName
        {
            get { return TypeElementCollection.KnownTypeConfig; }
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return ((TypeElement)element).Key;
        }

        public int IndexOf(TypeElement element)
        {
            if (element == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return BaseIndexOf(element);
        }

        public void Remove(TypeElement element)
        {
            // Only validate input if config is not Read-Only, otherwise
            // let BaseRemove throw appropriate exception
            if (!this.IsReadOnly())
            {
                if (element == null)
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
                }
            }
            BaseRemove(this.GetElementKey(element));
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        const string KnownTypeConfig = "knownType";

    }

}


