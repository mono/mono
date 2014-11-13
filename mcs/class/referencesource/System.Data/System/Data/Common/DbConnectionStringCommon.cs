//------------------------------------------------------------------------------
// <copyright file="DbConnectionStringBuilder.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>
// <owner current="true" primary="false">[....]</owner>
//------------------------------------------------------------------------------

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
using System.Data.SqlClient;

namespace System.Data.Common {

/*
    internal sealed class NamedConnectionStringConverter : StringConverter {

        public NamedConnectionStringConverter() {
        }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return true;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {
            // Although theoretically this could be true, some people may want to just type in a name
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            StandardValuesCollection standardValues = null;
            if (null != context) {
                DbConnectionStringBuilder instance = (context.Instance as DbConnectionStringBuilder);
                if (null != instance) {
                    string myProviderName = instance.GetType().Namespace;

                    List<string> myConnectionNames = new List<string>();
                    foreach(System.Configuration.ConnectionStringSetting setting in System.Configuration.ConfigurationManager.ConnectionStrings) {
                        if (myProviderName.EndsWith(setting.ProviderName)) {
                            myConnectionNames.Add(setting.ConnectionName);
                        }
                    }
                    standardValues = new StandardValuesCollection(myConnectionNames);
                }
            }
            return standardValues;
        }
    }
*/

    internal class DbConnectionStringBuilderDescriptor : PropertyDescriptor {
        private Type _componentType;
        private Type _propertyType;
        private bool _isReadOnly;
        private bool _refreshOnChange;

        internal DbConnectionStringBuilderDescriptor(string propertyName, Type componentType, Type propertyType, bool isReadOnly, Attribute[] attributes) : base(propertyName, attributes) {
            //Bid.Trace("<comm.DbConnectionStringBuilderDescriptor|INFO> propertyName='%ls', propertyType='%ls'\n", propertyName, propertyType.Name);
            _componentType = componentType;
            _propertyType = propertyType;
            _isReadOnly = isReadOnly;
        }

        internal bool RefreshOnChange {
            get {
                return _refreshOnChange;
            }
            set {
                _refreshOnChange = value;
            }
        }

        public override Type ComponentType {
            get {
                return _componentType;
            }
        }
        public override bool IsReadOnly {
            get {
                return _isReadOnly;
            }
        }
        public override Type PropertyType {
            get {
                return _propertyType;
            }
        }
        public override bool CanResetValue(object component) {
            DbConnectionStringBuilder builder = (component as DbConnectionStringBuilder);
            return ((null != builder) && builder.ShouldSerialize(DisplayName));
        }
        public override object GetValue(object component) {
            DbConnectionStringBuilder builder = (component as DbConnectionStringBuilder);
            if (null != builder) {
                object value;
                if (builder.TryGetValue(DisplayName, out value)) {
                    return value;
                }
            }
            return null;
        }
        public override void ResetValue(object component) {
            DbConnectionStringBuilder builder = (component as DbConnectionStringBuilder);
            if (null != builder) {
                builder.Remove(DisplayName);

                if (RefreshOnChange) {
                    builder.ClearPropertyDescriptors();
                }
            }
        }
        public override void SetValue(object component, object value) {
            DbConnectionStringBuilder builder = (component as DbConnectionStringBuilder);
            if (null != builder) {
                // via the editor, empty string does a defacto Reset
                if ((typeof(string) == PropertyType) && String.Empty.Equals(value)) {
                    value = null;
                }
                builder[DisplayName] = value;

                if (RefreshOnChange) {
                    builder.ClearPropertyDescriptors();
                }
            }
        }
        public override bool ShouldSerializeValue(object component) {
            DbConnectionStringBuilder builder = (component as DbConnectionStringBuilder);
            return ((null != builder) && builder.ShouldSerialize(DisplayName));
        }
    }

    [Serializable()]
    internal sealed class ReadOnlyCollection<T> : System.Collections.ICollection, ICollection<T> {
        private T[] _items;

        internal ReadOnlyCollection(T[] items) {
            _items = items;
#if DEBUG
            for(int i = 0; i < items.Length; ++i) {
                Debug.Assert(null != items[i], "null item");
            }
#endif
        }

        public void CopyTo(T[] array, int arrayIndex) {
            Array.Copy(_items, 0, array, arrayIndex, _items.Length);
        }

        void System.Collections.ICollection.CopyTo(Array array, int arrayIndex) {
            Array.Copy(_items, 0, array, arrayIndex, _items.Length);
        }


        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new Enumerator<T>(_items);
        }

        public System.Collections.IEnumerator GetEnumerator() {
            return new Enumerator<T>(_items);
        }

        bool System.Collections.ICollection.IsSynchronized {
            get { return false; }
        }

        Object System.Collections.ICollection.SyncRoot {
            get { return _items; }
        }

        bool ICollection<T>.IsReadOnly {
            get { return true;}
        }

        void ICollection<T>.Add(T value) {
            throw new NotSupportedException();
        }

        void ICollection<T>.Clear() {
            throw new NotSupportedException();
        }

        bool ICollection<T>.Contains(T value) {
            return Array.IndexOf(_items, value) >= 0;
        }

        bool ICollection<T>.Remove(T value) {
            throw new NotSupportedException();
        }

        public int Count {
            get { return _items.Length; }
        }

        [Serializable()]
        internal struct Enumerator<K> : IEnumerator<K>, System.Collections.IEnumerator { // based on List<T>.Enumerator
            private K[] _items;
            private int _index;

            internal Enumerator(K[] items) {
                _items = items;
                _index = -1;
            }

            public void Dispose() {
            }

            public bool MoveNext() {
                return (++_index < _items.Length);
            }

            public K Current {
                get {
                    return _items[_index];
                }
            }

            Object System.Collections.IEnumerator.Current {
                get {
                    return _items[_index];
                }
            }

            void System.Collections.IEnumerator.Reset() {
                _index = -1;
            }
        }
    }

    internal static class DbConnectionStringBuilderUtil {

        internal static bool ConvertToBoolean(object value) {
            Debug.Assert(null != value, "ConvertToBoolean(null)");
            string svalue = (value as string);
            if (null != svalue) {
                if (StringComparer.OrdinalIgnoreCase.Equals(svalue, "true") || StringComparer.OrdinalIgnoreCase.Equals(svalue, "yes"))
                    return true;
                else if (StringComparer.OrdinalIgnoreCase.Equals(svalue, "false") || StringComparer.OrdinalIgnoreCase.Equals(svalue, "no"))
                    return false;
                else {
                    string tmp = svalue.Trim();  // Remove leading & trailing white space.
                    if (StringComparer.OrdinalIgnoreCase.Equals(tmp, "true") || StringComparer.OrdinalIgnoreCase.Equals(tmp, "yes"))
                        return true;
                    else if (StringComparer.OrdinalIgnoreCase.Equals(tmp, "false") || StringComparer.OrdinalIgnoreCase.Equals(tmp, "no"))
                        return false;
                }
                return Boolean.Parse(svalue);
            }
            try {
                return ((IConvertible)value).ToBoolean(CultureInfo.InvariantCulture);
            }
            catch(InvalidCastException e) {
                throw ADP.ConvertFailed(value.GetType(), typeof(Boolean), e);
            }
        }

        internal static bool ConvertToIntegratedSecurity(object value) {
            Debug.Assert(null != value, "ConvertToIntegratedSecurity(null)");
            string svalue = (value as string);
            if (null != svalue) {
                if (StringComparer.OrdinalIgnoreCase.Equals(svalue, "sspi") || StringComparer.OrdinalIgnoreCase.Equals(svalue, "true") || StringComparer.OrdinalIgnoreCase.Equals(svalue, "yes"))
                    return true;
                else if (StringComparer.OrdinalIgnoreCase.Equals(svalue, "false") || StringComparer.OrdinalIgnoreCase.Equals(svalue, "no"))
                    return false;
                else {
                    string tmp = svalue.Trim();  // Remove leading & trailing white space.
                    if (StringComparer.OrdinalIgnoreCase.Equals(tmp, "sspi") || StringComparer.OrdinalIgnoreCase.Equals(tmp, "true") || StringComparer.OrdinalIgnoreCase.Equals(tmp, "yes"))
                        return true;
                    else if (StringComparer.OrdinalIgnoreCase.Equals(tmp, "false") || StringComparer.OrdinalIgnoreCase.Equals(tmp, "no"))
                        return false;
                }
                return Boolean.Parse(svalue);
            }
            try {
                return ((IConvertible)value).ToBoolean(CultureInfo.InvariantCulture);
            }
            catch(InvalidCastException e) {
                throw ADP.ConvertFailed(value.GetType(), typeof(Boolean), e);
            }
        }

        internal static int ConvertToInt32(object value) {
            try {
                return ((IConvertible)value).ToInt32(CultureInfo.InvariantCulture);
            }
            catch(InvalidCastException e) {
                throw ADP.ConvertFailed(value.GetType(), typeof(Int32), e);
            }
        }

        internal static string ConvertToString(object value) {
            try {
                return ((IConvertible)value).ToString(CultureInfo.InvariantCulture);
            }
            catch(InvalidCastException e) {
                throw ADP.ConvertFailed(value.GetType(), typeof(String), e);
            }
        }

        const string ApplicationIntentReadWriteString = "ReadWrite";
        const string ApplicationIntentReadOnlyString = "ReadOnly";

        internal static bool TryConvertToApplicationIntent(string value, out ApplicationIntent result) {
            Debug.Assert(Enum.GetNames(typeof(ApplicationIntent)).Length == 2, "ApplicationIntent enum has changed, update needed");
            Debug.Assert(null != value, "TryConvertToApplicationIntent(null,...)");

            if (StringComparer.OrdinalIgnoreCase.Equals(value, ApplicationIntentReadOnlyString)) {
                result = ApplicationIntent.ReadOnly;
                return true;
            }
            else if (StringComparer.OrdinalIgnoreCase.Equals(value, ApplicationIntentReadWriteString)) {
                result = ApplicationIntent.ReadWrite;
                return true;
            }
            else {
                result = DbConnectionStringDefaults.ApplicationIntent;
                return false;
            }
        }

        internal static bool IsValidApplicationIntentValue(ApplicationIntent value) {
            Debug.Assert(Enum.GetNames(typeof(ApplicationIntent)).Length == 2, "ApplicationIntent enum has changed, update needed");
            return value == ApplicationIntent.ReadOnly || value == ApplicationIntent.ReadWrite;
        }

        internal static string ApplicationIntentToString(ApplicationIntent value) {
            Debug.Assert(IsValidApplicationIntentValue(value));
            if (value == ApplicationIntent.ReadOnly) {
                return ApplicationIntentReadOnlyString;
            }
            else {
                return ApplicationIntentReadWriteString;
            }
        }

        /// <summary>
        /// This method attempts to convert the given value tp ApplicationIntent enum. The algorithm is:
        /// * if the value is from type string, it will be matched against ApplicationIntent enum names only, using ordinal, case-insensitive comparer
        /// * if the value is from type ApplicationIntent, it will be used as is
        /// * if the value is from integral type (SByte, Int16, Int32, Int64, Byte, UInt16, UInt32, or UInt64), it will be converted to enum
        /// * if the value is another enum or any other type, it will be blocked with an appropriate ArgumentException
        /// 
        /// in any case above, if the conerted value is out of valid range, the method raises ArgumentOutOfRangeException.
        /// </summary>
        /// <returns>applicaiton intent value in the valid range</returns>
        internal static ApplicationIntent ConvertToApplicationIntent(string keyword, object value) {
            Debug.Assert(null != value, "ConvertToApplicationIntent(null)");
            string sValue = (value as string);
            ApplicationIntent result;
            if (null != sValue) {
                // We could use Enum.TryParse<ApplicationIntent> here, but it accepts value combinations like
                // "ReadOnly, ReadWrite" which are unwelcome here
                // Also, Enum.TryParse is 100x slower than plain StringComparer.OrdinalIgnoreCase.Equals method.

                if (TryConvertToApplicationIntent(sValue, out result)) {
                    return result;
                }

                // try again after remove leading & trailing whitespaces.
                sValue = sValue.Trim();
                if (TryConvertToApplicationIntent(sValue, out result)) {
                    return result;
                }

                // string values must be valid
                throw ADP.InvalidConnectionOptionValue(keyword);
            }
            else {
                // the value is not string, try other options
                ApplicationIntent eValue;

                if (value is ApplicationIntent) {
                    // quick path for the most common case
                    eValue = (ApplicationIntent)value;
                }
                else if (value.GetType().IsEnum) {
                    // explicitly block scenarios in which user tries to use wrong enum types, like:
                    // builder["ApplicationIntent"] = EnvironmentVariableTarget.Process;
                    // workaround: explicitly cast non-ApplicationIntent enums to int
                    throw ADP.ConvertFailed(value.GetType(), typeof(ApplicationIntent), null);
                }
                else {
                    try {
                        // Enum.ToObject allows only integral and enum values (enums are blocked above), rasing ArgumentException for the rest
                        eValue = (ApplicationIntent)Enum.ToObject(typeof(ApplicationIntent), value);
                    }
                    catch (ArgumentException e) {
                        // to be consistent with the messages we send in case of wrong type usage, replace 
                        // the error with our exception, and keep the original one as inner one for troubleshooting
                        throw ADP.ConvertFailed(value.GetType(), typeof(ApplicationIntent), e);
                    }
                }

                // ensure value is in valid range
                if (IsValidApplicationIntentValue(eValue)) {
                    return eValue;
                }
                else {
                    throw ADP.InvalidEnumerationValue(typeof(ApplicationIntent), (int)eValue);
                }
            }
        }
    }

    internal static class DbConnectionStringDefaults {
        // all
//        internal const string NamedConnection           = "";

        // Odbc
        internal const string Driver                    = "";
        internal const string Dsn                       = "";

        // OleDb
        internal const bool   AdoNetPooler              = false;
        internal const string FileName                  = "";
        internal const int    OleDbServices             = ~(/*DBPROPVAL_OS_AGR_AFTERSESSION*/0x00000008 | /*DBPROPVAL_OS_CLIENTCURSOR*/0x00000004); // -13
        internal const string Provider                  = "";

        // OracleClient
        internal const bool   Unicode                   = false;
        internal const bool   OmitOracleConnectionName  = false;

        // SqlClient
        internal const ApplicationIntent ApplicationIntent = System.Data.SqlClient.ApplicationIntent.ReadWrite;
        internal const string ApplicationName           = ".Net SqlClient Data Provider";
        internal const bool   AsynchronousProcessing    = false;
        internal const string AttachDBFilename          = "";
        internal const int    ConnectTimeout            = 15;
        internal const bool   ConnectionReset           = true;
        internal const bool   ContextConnection         = false;
        internal const string CurrentLanguage           = "";
        internal const string DataSource                = "";
        internal const bool   Encrypt                   = false;
        internal const bool   Enlist                    = true;
        internal const string FailoverPartner           = "";
        internal const string InitialCatalog            = "";
        internal const bool   IntegratedSecurity        = false;
        internal const int    LoadBalanceTimeout        = 0; // default of 0 means don't use
        internal const bool   MultipleActiveResultSets  = false;
        internal const bool   MultiSubnetFailover       = false;
        internal const int    MaxPoolSize               = 100;
        internal const int    MinPoolSize               = 0;
        internal const string NetworkLibrary            = "";
        internal const int    PacketSize                = 8000;
        internal const string Password                  = "";
        internal const bool   PersistSecurityInfo       = false;
        internal const bool   Pooling                   = true;
        internal const bool   TrustServerCertificate    = false;
        internal const string TypeSystemVersion         = "Latest";
        internal const string UserID                    = "";
        internal const bool   UserInstance              = false;
        internal const bool   Replication               = false;
        internal const string WorkstationID             = "";
        internal const string TransactionBinding        = "Implicit Unbind";
        internal const int    ConnectRetryCount         = 1;
        internal const int    ConnectRetryInterval      = 10;
    }

    internal static class DbConnectionOptionKeywords {
        // Odbc
        internal const string Driver                    = "driver";
        internal const string Pwd                       = "pwd";
        internal const string UID                       = "uid";

        // OleDb
        internal const string DataProvider              = "data provider";
        internal const string ExtendedProperties        = "extended properties";
        internal const string FileName                  = "file name";
        internal const string Provider                  = "provider";
        internal const string RemoteProvider            = "remote provider";

        // common keywords (OleDb, OracleClient, SqlClient)
        internal const string Password                  = "password";
        internal const string UserID                    = "user id";
    }

    internal static class DbConnectionStringKeywords {
        // all
//        internal const string NamedConnection           = "Named Connection";

        // Odbc
        internal const string Driver                    = "Driver";
        internal const string Dsn                       = "Dsn";
        internal const string FileDsn                   = "FileDsn";
        internal const string SaveFile                  = "SaveFile";

        // OleDb
        internal const string FileName                  = "File Name";
        internal const string OleDbServices             = "OLE DB Services";
        internal const string Provider                  = "Provider";

        // OracleClient
        internal const string Unicode                   = "Unicode";
        internal const string OmitOracleConnectionName  = "Omit Oracle Connection Name";

        // SqlClient
        internal const string ApplicationIntent         = "ApplicationIntent";
        internal const string ApplicationName           = "Application Name";
        internal const string AsynchronousProcessing    = "Asynchronous Processing";
        internal const string AttachDBFilename          = "AttachDbFilename";
        internal const string ConnectTimeout            = "Connect Timeout";
        internal const string ConnectionReset           = "Connection Reset";
        internal const string ContextConnection         = "Context Connection";
        internal const string CurrentLanguage           = "Current Language";
        internal const string Encrypt                   = "Encrypt";
        internal const string FailoverPartner           = "Failover Partner";
        internal const string InitialCatalog            = "Initial Catalog";
        internal const string MultipleActiveResultSets  = "MultipleActiveResultSets";
        internal const string MultiSubnetFailover       = "MultiSubnetFailover";
        internal const string NetworkLibrary            = "Network Library";
        internal const string PacketSize                = "Packet Size";
        internal const string Replication               = "Replication";
        internal const string TransactionBinding        = "Transaction Binding";
        internal const string TrustServerCertificate    = "TrustServerCertificate";
        internal const string TypeSystemVersion         = "Type System Version";
        internal const string UserInstance              = "User Instance";
        internal const string WorkstationID             = "Workstation ID";
        internal const string ConnectRetryCount         = "ConnectRetryCount";
        internal const string ConnectRetryInterval      = "ConnectRetryInterval";

        // common keywords (OleDb, OracleClient, SqlClient)
        internal const string DataSource                = "Data Source";
        internal const string IntegratedSecurity        = "Integrated Security";
        internal const string Password                  = "Password";
        internal const string PersistSecurityInfo       = "Persist Security Info";
        internal const string UserID                    = "User ID";

        // managed pooling (OracleClient, SqlClient)
        internal const string Enlist                    = "Enlist";
        internal const string LoadBalanceTimeout        = "Load Balance Timeout";
        internal const string MaxPoolSize               = "Max Pool Size";
        internal const string Pooling                   = "Pooling";
        internal const string MinPoolSize               = "Min Pool Size";
    }

    internal static class DbConnectionStringSynonyms {
        //internal const string AsynchronousProcessing = Async;
        internal const string Async                  = "async";

        //internal const string ApplicationName        = APP;
        internal const string APP                    = "app";

        //internal const string AttachDBFilename       = EXTENDEDPROPERTIES+","+INITIALFILENAME;
        internal const string EXTENDEDPROPERTIES     = "extended properties";
        internal const string INITIALFILENAME        = "initial file name";

        //internal const string ConnectTimeout         = CONNECTIONTIMEOUT+","+TIMEOUT;
        internal const string CONNECTIONTIMEOUT      = "connection timeout";
        internal const string TIMEOUT                = "timeout";

        //internal const string CurrentLanguage        = LANGUAGE;
        internal const string LANGUAGE               = "language";

        //internal const string OraDataSource          = SERVER;
        //internal const string SqlDataSource          = ADDR+","+ADDRESS+","+SERVER+","+NETWORKADDRESS;
        internal const string ADDR                   = "addr";
        internal const string ADDRESS                = "address";
        internal const string SERVER                 = "server";
        internal const string NETWORKADDRESS         = "network address";

        //internal const string InitialCatalog         = DATABASE;
        internal const string DATABASE               = "database";

        //internal const string IntegratedSecurity     = TRUSTEDCONNECTION;
        internal const string TRUSTEDCONNECTION      = "trusted_connection"; // underscore introduced in everett

        //internal const string LoadBalanceTimeout     = ConnectionLifetime;
        internal const string ConnectionLifetime     = "connection lifetime";

        //internal const string NetworkLibrary         = NET+","+NETWORK;
        internal const string NET                    = "net";
        internal const string NETWORK                = "network";

        internal const string WorkaroundOracleBug914652 = "Workaround Oracle Bug 914652";

        //internal const string Password               = Pwd;
        internal const string Pwd                    = "pwd";

        //internal const string PersistSecurityInfo    = PERSISTSECURITYINFO;
        internal const string PERSISTSECURITYINFO    = "persistsecurityinfo";

        //internal const string UserID                 = UID+","+User;
        internal const string UID                    = "uid";
        internal const string User                   = "user";

        //internal const string WorkstationID          = WSID;
        internal const string WSID                   = "wsid";
    }
}
