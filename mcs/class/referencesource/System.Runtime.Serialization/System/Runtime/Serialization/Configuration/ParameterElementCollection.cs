//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization.Configuration
{

    using System;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(ParameterElement), AddItemName = ConfigurationStrings.Parameter, CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class ParameterElementCollection : ConfigurationElementCollection
    {
        public ParameterElementCollection()
        {
            this.AddElementName = ConfigurationStrings.Parameter;
        }

        public ParameterElement this[int index]
        {
            get
            {
                ParameterElement retval = (ParameterElement)BaseGet(index);
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


        public void Add(ParameterElement element)
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

        public bool Contains(string typeName)
        {
            if (String.IsNullOrEmpty(typeName))
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
            }
            return this.BaseGet(typeName) != null;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            ParameterElement retval = new ParameterElement();
            return retval;
        }

        protected override string ElementName
        {
            get { return ConfigurationStrings.Parameter; }
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }

            return ((ParameterElement)element).identity;
        }

        public int IndexOf(ParameterElement element)
        {
            if (element == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return BaseIndexOf(element);
        }

        public void Remove(ParameterElement element)
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
    }

}


