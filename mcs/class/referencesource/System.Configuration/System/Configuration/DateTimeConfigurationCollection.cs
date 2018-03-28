//------------------------------------------------------------------------------
// <copyright file="DateTimeConfigurationCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * ImmutableCollections
 *
 * Copyright (c) 2004 Microsoft Corporation
 */

using System;
using System.Collections;
using System.Collections.Specialized;
using System.Net;
using System.Configuration;

//
// This file contains configuration collections that are used by multiple sections
//
#if ORCAS
namespace System.Configuration {

    [ConfigurationCollection(typeof(DateTimeConfigurationElement))]
    public sealed class DateTimeConfigurationCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static DateTimeConfigurationCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        //
        // Constructor
        //
        public DateTimeConfigurationCollection() {
        }

        //
        // Accessors
        //

        public DateTime this[int index] {
            get {
                return (DateTime)((DateTimeConfigurationElement)BaseGet(index)).Value;
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, new DateTimeConfigurationElement(value));
            }
        }

        public object[] AllKeys {
            get {
                return BaseGetAllKeys();
            }
        }

        //
        // Methods
        //

        public void Add(DateTime dateTime) {
            BaseAdd(new DateTimeConfigurationElement(dateTime));
        }

        public void Remove(DateTime dateTime) {
            BaseRemove(dateTime);
        }
        public void Clear() {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() {
            return new DateTimeConfigurationElement();
        }

        protected override object GetElementKey(ConfigurationElement element) {
            return ((DateTimeConfigurationElement)element).Value;
        }
    }
}
#endif