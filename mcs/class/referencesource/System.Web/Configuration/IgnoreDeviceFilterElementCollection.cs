//------------------------------------------------------------------------------
// <copyright file="IgnoreDeviceFilterElementCollection .cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Util;
    using System.Web.UI;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Xml;

    [ConfigurationCollection(typeof(IgnoreDeviceFilterElement), AddItemName = "filter",
     CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class IgnoreDeviceFilterElementCollection : ConfigurationElementCollection {
        private static readonly ConfigurationPropertyCollection _properties;

        static IgnoreDeviceFilterElementCollection() {
            _properties = new ConfigurationPropertyCollection();
        }

        public IgnoreDeviceFilterElementCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public void Add(IgnoreDeviceFilterElement deviceFilter) {
            BaseAdd(deviceFilter);
        }

        public void Remove(string name) {
            BaseRemove(name);
        }

        public void Remove(IgnoreDeviceFilterElement deviceFilter) {
            BaseRemove(GetElementKey(deviceFilter));
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }

        public new IgnoreDeviceFilterElement this[string name] {
            get {
                return (IgnoreDeviceFilterElement)BaseGet(name);
            }
        }

        public IgnoreDeviceFilterElement this[int index] {
            get {
                return (IgnoreDeviceFilterElement)BaseGet(index);
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

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "Can't modify the base class.")]
        protected override ConfigurationElement CreateNewElement() {
            return new IgnoreDeviceFilterElement();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "Can't modify the base class.")]
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((IgnoreDeviceFilterElement)element).Name;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "Can't modify the base class.")]
        protected override string ElementName {
            get {
                return "filter";
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "Can't modify the base class.")]
        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
    }
}
