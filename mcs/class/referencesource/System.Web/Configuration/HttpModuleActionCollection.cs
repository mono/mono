//------------------------------------------------------------------------------
// <copyright file="HttpModuleActionCollection.cs" company="Microsoft">
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
    using System.Web.Configuration;
    using System.Globalization;
    using System.Security.Permissions;

    // class HttpModulesSection

    [ConfigurationCollection(typeof(HttpModuleAction))]
    public sealed class HttpModuleActionCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static HttpModuleActionCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public HttpModuleActionCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }


        public HttpModuleAction this[int index] {
            get {
                return (HttpModuleAction)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }

        public int IndexOf(HttpModuleAction action) {
            return BaseIndexOf(action);
        }

        public void Add(HttpModuleAction httpModule) {
            BaseAdd(httpModule);
        }

        public void Remove(HttpModuleAction action) {
            BaseRemove(action.Key);
        }

        public void Remove(string name) {
            BaseRemove(name);
        }

        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }
        protected override ConfigurationElement CreateNewElement() {
            return new HttpModuleAction();
        }

        protected override Object GetElementKey(ConfigurationElement element) {
            return ((HttpModuleAction)element).Key;
        }

        protected override bool IsElementRemovable(ConfigurationElement element) {
            HttpModuleAction module = (HttpModuleAction)element;
            if (BaseIndexOf(module) == -1) // does it exist?
            {
                if (HttpModuleAction.IsSpecialModuleName(module.Name)) {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Special_module_cannot_be_removed_manually, module.Name),
                                module.FileName, module.LineNumber);
                }
                else {
                    throw new ConfigurationErrorsException(SR.GetString(SR.Module_not_in_app, module.Name),
                                module.FileName, module.LineNumber);
                }
            }
            return true;
        }

        public void Clear() {
            BaseClear();
        }
    }
}
