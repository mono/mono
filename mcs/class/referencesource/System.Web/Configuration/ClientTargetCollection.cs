//------------------------------------------------------------------------------
// <copyright file="ClientTargetCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.Web.Util;
    using System.Diagnostics;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(ClientTarget))]
    public sealed class ClientTargetCollection : ConfigurationElementCollection {
        private static readonly ConfigurationPropertyCollection _properties;

        static ClientTargetCollection() {
            _properties = new ConfigurationPropertyCollection();
        }
        
        public ClientTargetCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public String[] AllKeys {
            get {
                return StringUtil.ObjectArrayToStringArray(BaseGetAllKeys());
            }
        }

        public String GetKey(int index) {
            return (String)BaseGetKey(index);
        }

        public void Add(ClientTarget clientTarget) {
            BaseAdd(clientTarget);
        }
        
        public void Remove(string name) {
            BaseRemove(name);
        }
        
        public void Remove(ClientTarget clientTarget) {
            BaseRemove(GetElementKey(clientTarget));
        }
        
        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }
        
        public new ClientTarget this[string name] {
            get {
                return (ClientTarget)BaseGet(name);
            }
        }
        
        public ClientTarget this[int index] {
            get {
                return (ClientTarget)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }

        public void Clear() {
            BaseClear();
        }

        protected override ConfigurationElement CreateNewElement() {
            return new ClientTarget();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((ClientTarget)element).Alias;
        }
    }
}
