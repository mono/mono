//------------------------------------------------------------------------------
// <copyright file="ConvertersCollection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Configuration {
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Resources;
    using System.Web.Script.Serialization;
    using System.Security;

    [
    ConfigurationCollection(typeof(Converter)),
    SuppressMessage("Microsoft.Design", "CA1010:CollectionsShouldImplementGenericInterface",
        Justification="Derives from legacy collection base class.  Base method IsReadOnly() " +
                      "would clash with property ICollection<T>.IsReadOnly.")
    ]
    public class ConvertersCollection : ConfigurationElementCollection {

        private static readonly ConfigurationPropertyCollection _properties =
            new ConfigurationPropertyCollection();

        public ConvertersCollection() {
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public Converter this[int index] {
            get {
                return (Converter)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }

                BaseAdd(index, value);
            }
        }


        public void Add(Converter converter) {
            BaseAdd(converter);
        }

        public void Remove(Converter converter) {
            BaseRemove(GetElementKey(converter));
        }

        public void Clear() {
            BaseClear();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override ConfigurationElement CreateNewElement() {
            return new Converter();
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((Converter)element).Name;
        }

        [SecuritySafeCritical]
        internal JavaScriptConverter[] CreateConverters() {
            List<JavaScriptConverter> list = new List<JavaScriptConverter>();
            foreach (Converter converter in this) {
                Type t = BuildManager.GetType(converter.Type, false /*throwOnError*/);

                if (t == null) {
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.ConvertersCollection_UnknownType, converter.Type));
                }

                if (!typeof(JavaScriptConverter).IsAssignableFrom(t)) {
                    throw new ArgumentException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.ConvertersCollection_NotJavaScriptConverter, t.Name));
                }

                list.Add((JavaScriptConverter)Activator.CreateInstance(t));
            }
            return list.ToArray();
        }
    }
}
