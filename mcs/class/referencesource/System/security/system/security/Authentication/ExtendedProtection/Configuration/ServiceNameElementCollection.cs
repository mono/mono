//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//------------------------------------------------------------------------------

using System.Configuration;

namespace System.Security.Authentication.ExtendedProtection.Configuration
{
    [ConfigurationCollection(typeof(ServiceNameElement))]
    public sealed class ServiceNameElementCollection : ConfigurationElementCollection
    {
        public ServiceNameElementCollection()
        {
        }

        public ServiceNameElement this[int index]
        {
            get
            {
                return (ServiceNameElement)BaseGet(index);
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

        public new ServiceNameElement this[string name]
        {
            get
            {
                return (ServiceNameElement)BaseGet(name);
            }
            set
            {
                if (BaseGet(name) != null)
                {
                    BaseRemove(name);
                }
                BaseAdd(value);
            }
        }

        public void Add(ServiceNameElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new ServiceNameElement();
        }

        protected override Object GetElementKey(ConfigurationElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return ((ServiceNameElement)element).Key;
        }

        public int IndexOf(ServiceNameElement element)
        {
            return BaseIndexOf(element);
        }

        public void Remove(ServiceNameElement element)
        {
            if (element == null)
                throw new ArgumentNullException("element");
            BaseRemove(element.Key);
        }

        public void Remove(string name)
        {
            BaseRemove(name);
        }

        public void RemoveAt(int index)
        {
            BaseRemoveAt(index);
        }
    }
}
