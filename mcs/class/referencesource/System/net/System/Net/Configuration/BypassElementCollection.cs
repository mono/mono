//------------------------------------------------------------------------------
// <copyright file="BypassElementCollection.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{

    using System;
    using System.Configuration;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(BypassElement))]
    public sealed class BypassElementCollection : ConfigurationElementCollection
    {
        public BypassElementCollection() 
        {
        }
        
        public BypassElement this[int index]
        {
            get
            {
                return (BypassElement)BaseGet(index);
            }
            set
            {
                if (BaseGet(index) != null)
                {
                    BaseRemoveAt(index);
                }
                BaseAdd(index,value);
            }
        }

        public new BypassElement this[string name]
        {
            get
            {
                return (BypassElement)BaseGet(name);
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
         
        public void Add(BypassElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() 
        {
            return new BypassElement();
        }

        protected override Object GetElementKey(ConfigurationElement element) 
        {
            if (element == null)
                throw new ArgumentNullException("element");
            return ((BypassElement)element).Key;
        }

        public int IndexOf(BypassElement element)
        {
            return BaseIndexOf(element);
        }
         
        public void Remove(BypassElement element) 
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

        // Since, ByPassElementCollection is a simple list with add/remove functionality, 
        // we now never throw on duplicate entries just for this collection. This also 
        // allows to keep Everett compatibility.
        //
        protected override bool ThrowOnDuplicate
        {
            get
            {
                return false;
            }
        }
    } 

}


