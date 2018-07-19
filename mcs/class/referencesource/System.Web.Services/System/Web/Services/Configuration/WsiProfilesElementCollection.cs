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

    [ConfigurationCollection(typeof(WsiProfilesElement))]
    public sealed class WsiProfilesElementCollection : ConfigurationElementCollection
    {
        public void Add(WsiProfilesElement element)
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
            return new WsiProfilesElement();
        }

        public void CopyTo(WsiProfilesElement[] array, int index)
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

            WsiProfilesElement configElementKey = (WsiProfilesElement)element;
            return configElementKey.Name.ToString();
        }

        public int IndexOf(WsiProfilesElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return BaseIndexOf(element);
        }
         
        public void Remove(WsiProfilesElement element) 
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
            WsiProfilesElement basic10Element = new WsiProfilesElement(WsiProfiles.BasicProfile1_1);
            this.Add(basic10Element);
        }

        public WsiProfilesElement this[object key]
        {
            get
            {
                if (key == null)
                {
                    throw new ArgumentNullException("key");
                }
                WsiProfilesElement retval = (WsiProfilesElement)this.BaseGet(key);
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

        public WsiProfilesElement this[int index]
        {
            get
            {
                return (WsiProfilesElement)BaseGet(index);
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


