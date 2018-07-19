//------------------------------------------------------------------------------
// <copyright file="HttpHandlerActionCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web.Util;
    using System.Web.Compilation;
    using System.Globalization;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(HttpHandlerAction), 
     CollectionType = ConfigurationElementCollectionType.AddRemoveClearMapAlternate)]
    public sealed class HttpHandlerActionCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static HttpHandlerActionCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        public HttpHandlerActionCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.AddRemoveClearMapAlternate;
            }
        }
        protected override bool ThrowOnDuplicate {
            get {
                return false;
            }
        }

        public HttpHandlerAction this[int index] {
            get {
                return (HttpHandlerAction)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public int IndexOf(HttpHandlerAction action) {
            return BaseIndexOf(action);
        }

        public void Add(HttpHandlerAction httpHandlerAction) {
            BaseAdd(httpHandlerAction, false);
        }

        public void Remove(HttpHandlerAction action) {
            BaseRemove(action.Key);
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        public void Remove(string verb, string path) {
            BaseRemove("verb=" + verb + " | path=" + path);
        }

        protected override ConfigurationElement CreateNewElement() {
            return new HttpHandlerAction();
        }
        
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((HttpHandlerAction)element).Key;
        }

        public void Clear() {
            BaseClear();
        }
    }
}

