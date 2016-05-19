//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Runtime.Serialization.Configuration
{

    using System;
    using System.Configuration;
    using System.Globalization;

    [ConfigurationCollection(typeof(DeclaredTypeElement))]
    public sealed class DeclaredTypeElementCollection : ConfigurationElementCollection
    {
        public DeclaredTypeElementCollection()
        {
        }

        public DeclaredTypeElement this[int index]
        {
            get
            {
                DeclaredTypeElement retval = (DeclaredTypeElement)BaseGet(index);
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

        public new DeclaredTypeElement this[string typeName]
        {
            get
            {
                if (String.IsNullOrEmpty(typeName))
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
                }
                DeclaredTypeElement retval = (DeclaredTypeElement)BaseGet(typeName);
                return retval;
            }
            set
            {
                // Only validate input if config is not Read-Only, otherwise
                // let Add throw appropriate exception
                if (!this.IsReadOnly())
                {
                    if (String.IsNullOrEmpty(typeName))
                    {
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
                    }
                    if (value == null)
                    {
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                    }
                    if (BaseGet(typeName) != null)
                    {
                        BaseRemove(typeName);
                    }
                    else
                    {
                        throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperError(new IndexOutOfRangeException(SR.GetString(SR.ConfigIndexOutOfRange,
                            typeName)));
                    }
                }
                Add(value);
            }
        }

        public void Add(DeclaredTypeElement element)
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
            DeclaredTypeElement retval = new DeclaredTypeElement();
            return retval;
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return ((DeclaredTypeElement)element).Type;
        }

        public int IndexOf(DeclaredTypeElement element)
        {
            if (element == null)
            {
                throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("element");
            }
            return BaseIndexOf(element);
        }

        public void Remove(DeclaredTypeElement element)
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

        public void Remove(string typeName)
        {
            // Only validate input if config is not Read-Only, otherwise
            // let BaseRemove throw appropriate exception
            if (!this.IsReadOnly())
            {
                if (String.IsNullOrEmpty(typeName))
                {
                    throw System.Runtime.Serialization.DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("typeName");
                }
            }
            BaseRemove(typeName);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

    }

}


