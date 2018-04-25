//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration
{
    using System;
    using System.Collections;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(ProtocolElement))]
    public sealed class ProtocolElementCollection : ConfigurationElementCollection
    {
        public void Add(ProtocolElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public bool ContainsKey(object key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return this.BaseGet(key) != null;
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ProtocolElement();
        }

        public void CopyTo(ProtocolElement[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array");
            }

            ((ICollection)this).CopyTo(array, index);
        }

        protected override Object GetElementKey(ConfigurationElement element) 
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            ProtocolElement configElementKey = (ProtocolElement)element;
            return configElementKey.Name.ToString();
        }

        public int IndexOf(ProtocolElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return BaseIndexOf(element);
        }
         
        public void Remove(ProtocolElement element) 
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            BaseRemove(GetElementKey(element));
        }

        public void RemoveAt(object key) 
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            BaseRemove(key);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }

        internal void SetDefaults()
        {
            ProtocolElement httpSoap12Element = new ProtocolElement(WebServiceProtocols.HttpSoap12);
            ProtocolElement httpSoapElement = new ProtocolElement(WebServiceProtocols.HttpSoap);
            ProtocolElement httpPostLocalhostElement = new ProtocolElement(WebServiceProtocols.HttpPostLocalhost);
            ProtocolElement documentationElement = new ProtocolElement(WebServiceProtocols.Documentation);

            this.Add(httpSoap12Element);
            this.Add(httpSoapElement);
            this.Add(httpPostLocalhostElement);
            this.Add(documentationElement);
        }

        public ProtocolElement this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                ProtocolElement retval = (ProtocolElement)this.BaseGet(key);
                if (retval == null)
                {
                    throw new System.Collections.Generic.KeyNotFoundException(
                        string.Format(CultureInfo.InvariantCulture, 
                        Res.GetString(Res.ConfigKeyNotFoundInElementCollection),
                        key.ToString()));
                }
                return retval;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                // NOTE [ivelin : integration fix] The change bellow have the issue that it wont use the collection comparer
                // if one is specified. We ( System.Configuration ) usually avoid having set_item[ key ] when the element contains
                // the key and instead provide an Add( element ) method only.
                if (this.GetElementKey(value).Equals(key))
                {
                    if (BaseGet(key) != null)
                    {
                        BaseRemove(key);
                    }
                    Add(value);
                }
                else
                {
                    throw new ArgumentException(string.Format(CultureInfo.InvariantCulture,
                        Res.GetString(Res.ConfigKeysDoNotMatch), this.GetElementKey(value).ToString(),
                        key.ToString()));
                }
            }
        }

        public ProtocolElement this[int index]
        {
            get
            {
                return (ProtocolElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
    }
}


