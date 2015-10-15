//------------------------------------------------------------------------------
// <copyright file="DbConnectionStringBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">Microsoft</owner>
// <owner current="true" primary="false">Microsoft</owner>
//------------------------------------------------------------------------------

namespace System.Data.Common {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Diagnostics.CodeAnalysis;

    public class DbConnectionStringBuilder : System.Collections.IDictionary, ICustomTypeDescriptor {

        // keyword->value currently listed in the connection string
        private Dictionary<string,object>  _currentValues;

        // cached connectionstring to avoid constant rebuilding
        // and to return a user's connectionstring as is until editing occurs
        private string _connectionString = "";

        private PropertyDescriptorCollection _propertyDescriptors;
        private bool _browsableConnectionString = true;
        private readonly bool UseOdbcRules;

        private static int _objectTypeCount; // Bid counter
        internal readonly int _objectID = System.Threading.Interlocked.Increment(ref _objectTypeCount);

        public DbConnectionStringBuilder() {
        }
        
        public DbConnectionStringBuilder(bool useOdbcRules) {
            UseOdbcRules = useOdbcRules;
        }

        private ICollection Collection {
            get { return (ICollection)CurrentValues; }
        }
        private IDictionary Dictionary {
            get { return (IDictionary)CurrentValues; }
        }
        private Dictionary<string,object> CurrentValues {
            get {
                Dictionary<string,object> values = _currentValues;
                if (null == values) {
                    values = new Dictionary<string,object>(StringComparer.OrdinalIgnoreCase);
                    _currentValues = values;
                }
                return values;
            }
        }

        object System.Collections.IDictionary.this[object keyword] {
            // delegate to this[string keyword]
            get { return this[ObjectToString(keyword)]; }
            set { this[ObjectToString(keyword)] = value; }
        }

        [Browsable(false)]
        public virtual object this[string keyword] {
            get {
                Bid.Trace("<comm.DbConnectionStringBuilder.get_Item|API> %d#, keyword='%ls'\n", ObjectID, keyword);
                ADP.CheckArgumentNull(keyword, "keyword");
                object value;
                if (CurrentValues.TryGetValue(keyword, out value)) {
                    return value;
                }
                throw ADP.KeywordNotSupported(keyword);
            }
            set {
                ADP.CheckArgumentNull(keyword, "keyword");
                bool flag = false;
                if (null != value) {
                    string keyvalue = DbConnectionStringBuilderUtil.ConvertToString(value);
                    DbConnectionOptions.ValidateKeyValuePair(keyword, keyvalue);

                    flag = CurrentValues.ContainsKey(keyword);

                    // store keyword/value pair
                    CurrentValues[keyword] = keyvalue;

                }
                else {
                    flag = Remove(keyword);
                }
                _connectionString = null;
                if (flag) {
                    _propertyDescriptors = null;
                }
            }
        }

        [Browsable(false)]
        [DesignOnly(true)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [EditorBrowsableAttribute(EditorBrowsableState.Never)]
        public bool BrowsableConnectionString {
            get {
                return _browsableConnectionString;
            }
            set {
                _browsableConnectionString = value;
                _propertyDescriptors = null;
            }
        }

        [RefreshPropertiesAttribute(RefreshProperties.All)]
        [ResCategoryAttribute(Res.DataCategory_Data)]
        [ResDescriptionAttribute(Res.DbConnectionString_ConnectionString)]
        public string ConnectionString {
            get {
                Bid.Trace("<comm.DbConnectionStringBuilder.get_ConnectionString|API> %d#\n", ObjectID);
                string connectionString = _connectionString;
                if (null == connectionString) {
                    StringBuilder builder = new StringBuilder();
                    foreach(string keyword in Keys) {
                        object value;
                        if (ShouldSerialize(keyword) && TryGetValue(keyword, out value)) {
                            string keyvalue = ConvertValueToString(value);
                            AppendKeyValuePair(builder, keyword, keyvalue, UseOdbcRules);
                        }
                    }
                    connectionString = builder.ToString();
                    _connectionString = connectionString;
                }
                return connectionString;
            }
            set {
                Bid.Trace("<comm.DbConnectionStringBuilder.set_ConnectionString|API> %d#\n", ObjectID);
                DbConnectionOptions constr = new DbConnectionOptions(value, null, UseOdbcRules);
                string originalValue = ConnectionString;
                Clear();
                try {
                    for(NameValuePair pair = constr.KeyChain; null != pair; pair = pair.Next) {
                        if (null != pair.Value) {
                            this[pair.Name] = pair.Value;
                        }
                        else {
                            Remove(pair.Name);
                        }
                    }
                    _connectionString = null;
                }
                catch(ArgumentException) { // restore original string
                    ConnectionString = originalValue;
                    _connectionString = originalValue;
                    throw;
                }
            }
        }

        [Browsable(false)]
        public virtual int Count {
            get { return CurrentValues.Count; }
        }

        [Browsable(false)]
        public bool IsReadOnly {
            get { return false; }
        }

        [Browsable(false)]
        public virtual bool IsFixedSize {
            get { return false; }
        }
        bool System.Collections.ICollection.IsSynchronized {
            get { return Collection.IsSynchronized; }
        }

        [Browsable(false)]
        public virtual ICollection Keys {
            get {
                Bid.Trace("<comm.DbConnectionStringBuilder.Keys|API> %d#\n", ObjectID);
                return Dictionary.Keys;
            }
        }

        internal int ObjectID {
            get {
                return _objectID;
            }
        }

        object System.Collections.ICollection.SyncRoot {
            get { return Collection.SyncRoot; }
        }

        [Browsable(false)]
        public virtual ICollection Values {
             get {
                Bid.Trace("<comm.DbConnectionStringBuilder.Values|API> %d#\n", ObjectID);
                System.Collections.Generic.ICollection<string> keys = (System.Collections.Generic.ICollection<string>)Keys;
                System.Collections.Generic.IEnumerator<string> keylist = keys.GetEnumerator();
                object[] values = new object[keys.Count];
                for(int i = 0; i < values.Length; ++i) {
                    keylist.MoveNext();
                    values[i] = this[keylist.Current];
                    Debug.Assert(null != values[i], "null value " + keylist.Current);
                }
                return new System.Data.Common.ReadOnlyCollection<object>(values);
            }
        }

        internal virtual string ConvertValueToString(object value) {
            return (value == null) ? (string)null : Convert.ToString(value, CultureInfo.InvariantCulture);
        }

        void System.Collections.IDictionary.Add(object keyword, object value) {
            Add(ObjectToString(keyword), value);
        }
        public void Add(string keyword, object value) {
            this[keyword] = value;
        }

        public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value) {
            DbConnectionOptions.AppendKeyValuePairBuilder(builder, keyword, value, false);
        }
        
        public static void AppendKeyValuePair(StringBuilder builder, string keyword, string value, bool useOdbcRules) {
            DbConnectionOptions.AppendKeyValuePairBuilder(builder, keyword, value, useOdbcRules);
        }

        public virtual void Clear() {
            Bid.Trace("<comm.DbConnectionStringBuilder.Clear|API>\n");
            _connectionString = "";
            _propertyDescriptors = null;
            CurrentValues.Clear();
        }

        protected internal void ClearPropertyDescriptors() {
            _propertyDescriptors = null;
        }

        // does the keyword exist as a strongly typed keyword or as a stored value
        bool System.Collections.IDictionary.Contains(object keyword) {
            return ContainsKey(ObjectToString(keyword));
        }
        public virtual bool ContainsKey(string keyword) {
            ADP.CheckArgumentNull(keyword, "keyword");
            return CurrentValues.ContainsKey(keyword);
        }

        void ICollection.CopyTo(Array array, int index) {
            Bid.Trace("<comm.DbConnectionStringBuilder.ICollection.CopyTo|API> %d#\n", ObjectID);
            Collection.CopyTo(array, index);
        }

        public virtual bool EquivalentTo(DbConnectionStringBuilder connectionStringBuilder) {
            ADP.CheckArgumentNull(connectionStringBuilder, "connectionStringBuilder");


            Bid.Trace("<comm.DbConnectionStringBuilder.EquivalentTo|API> %d#, connectionStringBuilder=%d#\n", ObjectID, connectionStringBuilder.ObjectID);
            if ((GetType() != connectionStringBuilder.GetType()) || (CurrentValues.Count != connectionStringBuilder.CurrentValues.Count)) {
                return false;
            }
            object value;
            foreach(KeyValuePair<string, object> entry in CurrentValues) {
                if (!connectionStringBuilder.CurrentValues.TryGetValue(entry.Key, out value) || !entry.Value.Equals(value)) {
                    return false;
                }
            }
            return true;
        }

        IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            Bid.Trace("<comm.DbConnectionStringBuilder.IEnumerable.GetEnumerator|API> %d#\n", ObjectID);
            return Collection.GetEnumerator();
        }
        IDictionaryEnumerator System.Collections.IDictionary.GetEnumerator() {
            Bid.Trace("<comm.DbConnectionStringBuilder.IDictionary.GetEnumerator|API> %d#\n", ObjectID);
            return Dictionary.GetEnumerator();
        }

        [SuppressMessage("Microsoft.Usage", "CA2208:InstantiateArgumentExceptionsCorrectly", Justification = "See Dev11 bug 875012")]
        private string ObjectToString(object keyword) {
            try {
                return (string)keyword;
            }
            catch(InvalidCastException) {
                // 



                throw new ArgumentException("keyword", "not a string");
            }
        }

        void System.Collections.IDictionary.Remove(object keyword) {
            Remove(ObjectToString(keyword));
        }
        public virtual bool Remove(string keyword) {
            Bid.Trace("<comm.DbConnectionStringBuilder.Remove|API> %d#, keyword='%ls'\n", ObjectID, keyword);
            ADP.CheckArgumentNull(keyword, "keyword");
            if (CurrentValues.Remove(keyword)) {
                _connectionString = null;
                _propertyDescriptors = null;
                return true;
            }
            return false;
        }

        // does the keyword exist as a stored value or something that should always be persisted
        public virtual bool ShouldSerialize(string keyword) {
            ADP.CheckArgumentNull(keyword, "keyword");
            return CurrentValues.ContainsKey(keyword);
        }

        public override string ToString() {
            return ConnectionString;
        }

        public virtual bool TryGetValue(string keyword, out object value) {
            ADP.CheckArgumentNull(keyword, "keyword");
            return CurrentValues.TryGetValue(keyword, out value);
        }

        internal Attribute[] GetAttributesFromCollection(AttributeCollection collection) {
            Attribute[] attributes = new Attribute[collection.Count];
            collection.CopyTo(attributes, 0);
            return attributes;
        }

        private PropertyDescriptorCollection GetProperties() {
            PropertyDescriptorCollection propertyDescriptors = _propertyDescriptors;
            if (null == propertyDescriptors) {
                IntPtr hscp;
                Bid.ScopeEnter(out hscp, "<comm.DbConnectionStringBuilder.GetProperties|INFO> %d#", ObjectID);
                try {
                    Hashtable descriptors = new Hashtable(StringComparer.OrdinalIgnoreCase);

                    GetProperties(descriptors);

                    PropertyDescriptor[] properties = new PropertyDescriptor[descriptors.Count];
                    descriptors.Values.CopyTo(properties, 0);
                    propertyDescriptors = new PropertyDescriptorCollection(properties);
                    _propertyDescriptors = propertyDescriptors;
                }
                finally {
                    Bid.ScopeLeave(ref hscp);
                }
            }
            return propertyDescriptors;
        }

        protected virtual void GetProperties(Hashtable propertyDescriptors) {
            IntPtr hscp;
            Bid.ScopeEnter(out hscp, "<comm.DbConnectionStringBuilder.GetProperties|API> %d#", ObjectID);
            try {
                // show all strongly typed properties (not already added)
                // except ConnectionString iff BrowsableConnectionString
                Attribute[] attributes;
                foreach(PropertyDescriptor reflected in TypeDescriptor.GetProperties(this, true)) {

                    if (ADP.ConnectionString != reflected.Name) {
                        string displayName = reflected.DisplayName;
                        if (!propertyDescriptors.ContainsKey(displayName)) {
                            attributes = GetAttributesFromCollection(reflected.Attributes);
                            PropertyDescriptor descriptor = new DbConnectionStringBuilderDescriptor(reflected.Name,
                                    reflected.ComponentType, reflected.PropertyType, reflected.IsReadOnly, attributes);
                            propertyDescriptors[displayName] = descriptor;
                        }
                        // else added by derived class first
                    }
                    else if (BrowsableConnectionString) {
                        propertyDescriptors[ADP.ConnectionString] = reflected;
                    }
                    else {
                        propertyDescriptors.Remove(ADP.ConnectionString);
                    }
                }

                // all keywords in Keys list that do not have strongly typed property, ODBC case
                // ignore 'Workaround Oracle 
                if (!IsFixedSize) {
                    attributes = null;
                    foreach(string keyword in Keys) {

                        if (!propertyDescriptors.ContainsKey(keyword)) {
                            object value = this[keyword];

                            Type vtype;
                            if (null != value) {
                                vtype = value.GetType();
                                if (typeof(string) == vtype) {
                                    int tmp1;
                                    if (Int32.TryParse((string)value, out tmp1)) {
                                        vtype = typeof(Int32);
                                    }
                                    else {
                                        bool tmp2;
                                        if (Boolean.TryParse((string)value, out tmp2)) {
                                            vtype = typeof(Boolean);
                                        }
                                    }
                                }
                            }
                            else {
                                vtype = typeof(string);
                            }

                            Attribute[] useAttributes = attributes;
                            if (StringComparer.OrdinalIgnoreCase.Equals(DbConnectionStringKeywords.Password, keyword) ||
                                StringComparer.OrdinalIgnoreCase.Equals(DbConnectionStringSynonyms.Pwd, keyword)) {
                                useAttributes = new Attribute[] {
                                    BrowsableAttribute.Yes,
                                    PasswordPropertyTextAttribute.Yes,
                                    new ResCategoryAttribute(Res.DataCategory_Security),
                                    RefreshPropertiesAttribute.All,
                                };
                            }
                            else if (null == attributes) {
                                attributes = new Attribute[] {
                                    BrowsableAttribute.Yes,
                                    RefreshPropertiesAttribute.All,
                                };
                                useAttributes = attributes;
                            }

                            PropertyDescriptor descriptor = new DbConnectionStringBuilderDescriptor(keyword,
                                                                    this.GetType(), vtype, false, useAttributes);
                            propertyDescriptors[keyword] = descriptor;
                        }
                    }
                }
            }
            finally {
                Bid.ScopeLeave(ref hscp);
            }
        }

        private PropertyDescriptorCollection GetProperties(Attribute[] attributes) {
            PropertyDescriptorCollection propertyDescriptors = GetProperties();
            if ((null == attributes) || (0 == attributes.Length)) {
                // Basic case has no filtering
                return propertyDescriptors;
            }

            // Create an array that is guaranteed to hold all attributes
            PropertyDescriptor[] propertiesArray = new PropertyDescriptor[propertyDescriptors.Count];

            // Create an index to reference into this array
            int index = 0;

            // Iterate over each property
            foreach (PropertyDescriptor property in propertyDescriptors) {
                // Identify if this property's attributes match the specification
                bool match = true;
                foreach (Attribute attribute in attributes) {
                    Attribute attr = property.Attributes[attribute.GetType()];
                    if ((attr == null && !attribute.IsDefaultAttribute()) || !attr.Match(attribute)) {
                        match = false;
                        break;
                    }
                }

                // If this property matches, add it to the array
                if (match) {
                    propertiesArray[index] = property;
                    index++;
                }
            }

            // Create a new array that only contains the filtered properties
            PropertyDescriptor[] filteredPropertiesArray = new PropertyDescriptor[index];
            Array.Copy(propertiesArray, filteredPropertiesArray, index);

            return new PropertyDescriptorCollection(filteredPropertiesArray);
        }

        string ICustomTypeDescriptor.GetClassName() {
            return TypeDescriptor.GetClassName(this, true);
        }
        string ICustomTypeDescriptor.GetComponentName() {
            return TypeDescriptor.GetComponentName(this, true);
        }
        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return TypeDescriptor.GetAttributes(this, true);
        }
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }
        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return TypeDescriptor.GetConverter(this, true);
        }
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return GetProperties();
        }
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            return GetProperties(attributes);
        }
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return TypeDescriptor.GetEvents(this, true);
        }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }
        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return this;
        }
    }
}

