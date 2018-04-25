//------------------------------------------------------------------------------
// <copyright file="CustomErrorCollection.cs" company="Microsoft">
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
    using System.Globalization;
    using System.Web.Util;
    using System.Web.Configuration;
    using System.Security.Permissions;

    [ConfigurationCollection(typeof(CustomError), AddItemName = "error",
     CollectionType = ConfigurationElementCollectionType.BasicMap)]
    public sealed class CustomErrorCollection : ConfigurationElementCollection {
        private static ConfigurationPropertyCollection _properties;

        static CustomErrorCollection() {
            _properties = new ConfigurationPropertyCollection();
        }

        public CustomErrorCollection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        // public properties
        public String[] AllKeys {
            get {
                object[] objAllKeys = BaseGetAllKeys();
                String[] stringKeys = new String[objAllKeys.Length];
                for(int x = 0; x < objAllKeys.Length; x++)
                {
                    stringKeys[x] = ((Int32)objAllKeys[x]).ToString(CultureInfo.InvariantCulture);
                }
                return stringKeys;
            }
        }

        public new CustomError this[string statusCode] {
            get {
                return (CustomError)BaseGet((object)Int32.Parse(statusCode, CultureInfo.InvariantCulture));
            }
        }
        
        public CustomError this[int index] {
            get {
                return (CustomError)BaseGet(index);
            }
            set {
                if (BaseGet(index) != null) {
                    BaseRemoveAt(index);
                }
                BaseAdd(index, value);
            }
        }
        
        // Protected Overrides
        protected override ConfigurationElement CreateNewElement() {
            return new CustomError();
        }
        
        protected override Object GetElementKey(ConfigurationElement element) {
            return ((CustomError)element).StatusCode;
        }
        
        protected override string ElementName {
            get {
                return "error";
            }
        }
        
        public override ConfigurationElementCollectionType CollectionType {
            get {
                return ConfigurationElementCollectionType.BasicMap;
            }
        }
        
        // public methods
        public void Add(CustomError customError) {
            BaseAdd(customError);
        }
        
        public void Clear() {
            BaseClear();
        }
        
        public CustomError Get(int index) {
            return (CustomError)BaseGet(index);
        }
        
        public CustomError Get(string statusCode) {
            return (CustomError)BaseGet((object)Int32.Parse(statusCode, CultureInfo.InvariantCulture));
        }
        
        public String GetKey(int index) {
            Int32 key = (Int32) BaseGetKey(index);
            return key.ToString(CultureInfo.InvariantCulture);
        }
        
        public void Remove(string statusCode) {
            BaseRemove((object)Int32.Parse(statusCode, CultureInfo.InvariantCulture));
        }
        
        public void RemoveAt(int index) {
            BaseRemoveAt(index);
        }
        
        public void Set(CustomError customError) {
            BaseAdd(customError, false);
        }
    }
}
