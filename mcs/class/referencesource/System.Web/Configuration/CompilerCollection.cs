//------------------------------------------------------------------------------
// <copyright file="CompilerCollection.cs" company="Microsoft">
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
    using System.Web.Compilation;
    using System.Reflection;
    using System.Web.Hosting;
    using System.Web.UI;
    using System.CodeDom.Compiler;
    using System.Web.Util;
    using System.ComponentModel;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(Compiler), AddItemName = "compiler",
     CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class CompilerCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static CompilerCollection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        public CompilerCollection()
            : base(StringComparer.OrdinalIgnoreCase) {
        }
        
        // public properties
        public string[] AllKeys {
            get {
                return StringUtil.ObjectArrayToStringArray(BaseGetAllKeys());
            }
        }
        
        public new Compiler this[string language] {
            get {
                return (Compiler)BaseGet(language);
            }
        }
        
        public Compiler this[int index] {
            get {
                return (Compiler)BaseGet(index);
            }
            //            set
            //            {
            //                throw new ConfigurationErrorsException(SR.GetString(SR.Config_read_only_section_cannot_be_set, "CompilerCollection"));
            //                if (BaseGet(index) != null)
            //                    BaseRemoveAt(index);
            //                BaseAdd(index,value);
            //            }
        }
       
        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new Compiler();
        }
        
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((Compiler)element).Language;
        }
        
        protected override string ElementName {
            get {
                return "compiler";
            }
        }
        
        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }

        // public methods
        //        public void Add(Compiler compiler) {
        //            BaseAdd(compiler);
        //        }

        //        public void Clear() {
        //            BaseClear();
        //        }
        
        public Compiler Get(int index) {
            return (Compiler)BaseGet(index);
        }
        
        public Compiler Get(string language) {
            return (Compiler)BaseGet(language);
        }
        
        public String GetKey(int index) {
            return (String) BaseGetKey(index);
        }
        
        //        public void Remove(string language) {
        //            BaseRemove(language);
        //        }
        
        //        public void RemoveAt(int index) {
        //            BaseRemoveAt(index);
        //        }

        //        public void Set(Compiler compiler) {
        //            BaseAdd(compiler,false);
        //        }
    }
}
