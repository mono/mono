//------------------------------------------------------------------------------
// <copyright file="ClientSettingsProvider.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.ClientServices.Providers
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Configuration;
    using System.Configuration.Provider;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Security;
    using System.Security.Principal;
    using System.Security.Permissions;
    using System.Net;
    using System.ServiceModel;
    using System.ServiceModel.Activation;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Web.ClientServices;
    using System.Web.Resources;
    using System.Web.Security;
    using System.Threading;
    using System.Data;
    using System.Data.Common;
    using System.Reflection;
    using System.Collections.ObjectModel;
    using System.Collections.Generic;
    using System.Web.ApplicationServices;
    using System.Web.Script.Serialization;
    using System.Diagnostics.CodeAnalysis;

    [SecurityCritical]
    [PermissionSet(SecurityAction.InheritanceDemand, Name="FullTrust")]
    public class ClientSettingsProvider : SettingsProvider, IApplicationSettingsProvider
    {
        private string  _ConnectionString           = null;
        private string  _ConnectionStringProvider   = "";
        private bool    _NeedToDoReset              = false;
        private bool    _HonorCookieExpiry          = false;
        private bool    _firstTime                  = true;
        private string  _UserName                   = "";

        private SettingsPropertyValueCollection     _PropertyValues = new SettingsPropertyValueCollection();
        private SettingsPropertyCollection          _Properties     = null;
        private static  Hashtable                   _KnownTypesHashtable = null;
        private static  Type []                     _KnownTypesArray     = null;

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public override string ApplicationName      { 
            [SecuritySafeCritical]
            get { return ""; } 
            [SecuritySafeCritical]
            set { } }
        private static  string _ServiceUri          = "";
        private static  object _lock                = new object();
        private static  bool   _UsingFileSystemStore = false;
        private static  bool   _UsingIsolatedStore   = true;
        private static  bool   _UsingWFCService      = false;
        private static  ApplicationSettingsBase _SettingsBaseClass = null;

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId="0#", Justification="Reviewed and approved by feature crew"),
         SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        public static SettingsPropertyCollection GetPropertyMetadata(string serviceUri)
        {
            CookieContainer                  cookies        = null;
            IIdentity                        id             = Thread.CurrentPrincipal.Identity;
            SettingsPropertyCollection       retColl        = new SettingsPropertyCollection();


            if (id is ClientFormsIdentity)
                cookies = ((ClientFormsIdentity)id).AuthenticationCookies;

            if (serviceUri.EndsWith(".svc", StringComparison.OrdinalIgnoreCase)) {
                throw new NotImplementedException();

//                 CustomBinding                    binding        = ProxyHelper.GetBinding();
//                 ChannelFactory<ProfileService>   channelFactory = new ChannelFactory<ProfileService>(binding, new EndpointAddress(serviceUri));
//                 ProfilePropertyMetadata[]        props          = null;
//                 ProfileService                   clientService  = channelFactory.CreateChannel();

//                 using (new OperationContextScope((IContextChannel)clientService)) {
//                     ProxyHelper.AddCookiesToWCF(cookies, serviceUri, id.Name, null, null);
//                     props = clientService.GetPropertiesMetadata();
//                     ProxyHelper.GetCookiesFromWCF(cookies, serviceUri, id.Name, null, null);
//                 }
//                 if (props == null)
//                     return retColl;

//                 for(int iter=0; iter<props.Length; iter++) {
//                     AddToColl(props[iter], retColl, id.IsAuthenticated);
//                 }

            } else {
                object o = ProxyHelper.CreateWebRequestAndGetResponse(serviceUri + "/GetPropertiesMetadata",
                                                           ref cookies,
                                                           id.Name,
                                                           null,
                                                           null,
                                                           null,
                                                           null,
                                                           typeof(Collection<ProfilePropertyMetadata>));

                Collection<ProfilePropertyMetadata> props2 = (Collection<ProfilePropertyMetadata>) o;
                if (props2 != null)   {
                    foreach(ProfilePropertyMetadata p in props2)
                        AddToColl(p, retColl, id.IsAuthenticated);
                }
            }
            return retColl;
        }
        private static void AddToColl(ProfilePropertyMetadata p, SettingsPropertyCollection retColl, bool isAuthenticated) {
            string propName = p.PropertyName;
            Type   propType = Type.GetType(p.TypeName, false, true);
            bool   allowAnon = p.AllowAnonymousAccess;
            bool   readOnly = p.IsReadOnly;

            if (!allowAnon && !isAuthenticated)
                return;

            SettingsSerializeAs serializeAs = (SettingsSerializeAs)p.SerializeAs;
            SettingsAttributeDictionary dict = new SettingsAttributeDictionary();
            dict.Add("AllowAnonymous", allowAnon);
            retColl.Add(new SettingsProperty(propName, propType, null, readOnly,p.DefaultValue, serializeAs, dict, true, true));
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        [SecuritySafeCritical]
        public override void Initialize(string name, NameValueCollection config) {

            _UsingIsolatedStore = false;

            // Initialize using appSettings section
            string temp = System.Configuration.ConfigurationManager.AppSettings["ClientSettingsProvider.ServiceUri"];
            if (!string.IsNullOrEmpty(temp))
                ServiceUri = temp;

            temp = System.Configuration.ConfigurationManager.AppSettings["ClientSettingsProvider.ConnectionStringName"];
            if (!string.IsNullOrEmpty(temp)) {
                if (ConfigurationManager.ConnectionStrings[temp] != null) {
                    _ConnectionStringProvider = ConfigurationManager.ConnectionStrings[temp].ProviderName;
                    _ConnectionString = ConfigurationManager.ConnectionStrings[temp].ConnectionString;
                } else {
                    _ConnectionString = temp;
                }
            } else {
                _ConnectionString = SqlHelper.GetDefaultConnectionString();
            }


            temp = System.Configuration.ConfigurationManager.AppSettings["ClientSettingsProvider.HonorCookieExpiry"];
            if (!string.IsNullOrEmpty(temp))
                _HonorCookieExpiry = (string.Compare(temp, "true", StringComparison.OrdinalIgnoreCase) == 0);
            if (name == null)
                name = this.GetType().ToString();

            base.Initialize(name, config);

            if (config != null) {
                temp = config["serviceUri"];
                if (!string.IsNullOrEmpty(temp))
                    ServiceUri = temp;

                temp = config["connectionStringName"];
                if (!string.IsNullOrEmpty(temp)) {
                    if (ConfigurationManager.ConnectionStrings[temp] != null) {
                        _ConnectionStringProvider = ConfigurationManager.ConnectionStrings[temp].ProviderName;
                        _ConnectionString = ConfigurationManager.ConnectionStrings[temp].ConnectionString;
                    } else {
                        _ConnectionString = temp;
                    }
                }
                config.Remove("name");
                config.Remove("description");
                config.Remove("connectionStringName");
                config.Remove("serviceUri");
                foreach (string attribUnrecognized in config.Keys)
                    if (!String.IsNullOrEmpty(attribUnrecognized))
                        throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, AtlasWeb.AttributeNotRecognized, attribUnrecognized));
            }

            switch(SqlHelper.IsSpecialConnectionString(_ConnectionString))
            {
            case 1:
                _UsingFileSystemStore = true;
                break;
            case 2:
                _UsingIsolatedStore = true;
                break;
            default:
                break;
            }


        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        [SecuritySafeCritical]
        public override SettingsPropertyValueCollection GetPropertyValues(SettingsContext context, SettingsPropertyCollection propertyCollection) {
            if (propertyCollection == null || propertyCollection.Count < 1)
                return new SettingsPropertyValueCollection();
            lock(_lock) {
                if (_SettingsBaseClass == null && context != null) {
                    Type oType = context["SettingsClassType"] as Type;
                    if (oType != null) {
                        _SettingsBaseClass = oType.InvokeMember("Default", BindingFlags.GetProperty | BindingFlags.Public | BindingFlags.Static,
                                                                null, null, null, CultureInfo.InvariantCulture) as ApplicationSettingsBase;
                    }
                }
                _PropertyValues = new SettingsPropertyValueCollection();
                _Properties = propertyCollection;

                StoreKnownTypes(propertyCollection);
                GetPropertyValuesCore();
                return _PropertyValues;
            }
        }
        private void GetPropertyValuesCore()
        {
            _UserName = Thread.CurrentPrincipal.Identity.Name;
            if (_firstTime) {
                _firstTime = false;
                _NeedToDoReset = GetNeedToReset();
                RegisterForValidateUserEvent();
            }

            if (_NeedToDoReset) {
                _NeedToDoReset = false;
                SetNeedToReset(false);
                _PropertyValues = new SettingsPropertyValueCollection();
                SetRemainingValuesToDefault();
                SetPropertyValuesCore(_PropertyValues, false);
            }

            bool isCacheMoreFresh = GetIsCacheMoreFresh();

            GetPropertyValuesFromSQL(); // Always start with the local copy

            if (!ConnectivityStatus.IsOffline) {
                if (isCacheMoreFresh) {
                    SetPropertyValuesWeb(_PropertyValues, isCacheMoreFresh); // local copy is fresher, so update the web-copy
                } else {
                    GetPropertyValuesFromWeb();
                    SetPropertyValuesSQL(_PropertyValues, false);
                }
            }
            if (_PropertyValues.Count < _Properties.Count)
                SetRemainingValuesToDefault();
        }


        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        [SecuritySafeCritical]
        public override void SetPropertyValues(SettingsContext context, SettingsPropertyValueCollection propertyValueCollection) {
            if (propertyValueCollection == null || propertyValueCollection.Count < 1)
                return;
            lock(_lock) {
                StoreKnownTypes(propertyValueCollection);
                SetPropertyValuesCore(propertyValueCollection, true);
            }
        }
        private void SetPropertyValuesCore(SettingsPropertyValueCollection values, bool raiseEvent) {
            lock(_lock) {
                bool isCacheMoreFresh = GetIsCacheMoreFresh();
                // First store in SQL
                SetPropertyValuesSQL(values, true);

                Collection<string> errorList = null;
                // Store in web if it is offline
                if (!ConnectivityStatus.IsOffline)
                    errorList = SetPropertyValuesWeb(values, isCacheMoreFresh);

                if (raiseEvent && SettingsSaved != null) {
                    if (errorList == null)
                        errorList = new Collection<string>();
                    SettingsSaved(this, new SettingsSavedEventArgs(errorList));
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        [SecuritySafeCritical]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Reset(SettingsContext context) {
            lock(_lock) {
                if (_Properties == null) {
                    SetNeedToReset(true);
                } else {
                    _PropertyValues = new SettingsPropertyValueCollection();
                    SetRemainingValuesToDefault();
                    SetPropertyValues(context, _PropertyValues);
                    _NeedToDoReset = false;
                    SetNeedToReset(false);
                }
            }
        }
        [SecuritySafeCritical]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public void Upgrade(SettingsContext context, SettingsPropertyCollection properties) {
            return;
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        [SecuritySafeCritical]
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase")]
        public SettingsPropertyValue GetPreviousVersion(SettingsContext context, SettingsProperty property) {
            if (_Properties == null)
                _Properties = new SettingsPropertyCollection();
            if (_Properties[property.Name] == null)
                _Properties.Add(property);
            GetPropertyValuesCore();
            return _PropertyValues[property.Name];
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////

        private string GetServiceUri()
        {
            if (string.IsNullOrEmpty(_ServiceUri))
                throw new ArgumentException(AtlasWeb.ServiceUriNotFound);
            return _ServiceUri;
        }

        [SuppressMessage("Microsoft.Design", "CA1056:UriPropertiesShouldNotBeStrings", Justification = "Reviewed and approved by feature crew")]
        public static string ServiceUri
        {
            [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "Reviewed and approved by feature crew")]
            get
            {
                return _ServiceUri;
            }
            [SuppressMessage("Microsoft.Design", "CA1054:UriParametersShouldNotBeStrings", MessageId = "2#", Justification = "Reviewed and approved by feature crew")]
            set {
                _ServiceUri = value;
                if (string.IsNullOrEmpty(_ServiceUri)) {
                    _UsingWFCService = false;
                } else {
                    _UsingWFCService = _ServiceUri.EndsWith(".svc", StringComparison.OrdinalIgnoreCase);
                }
            }
        }

        public event EventHandler<SettingsSavedEventArgs> SettingsSaved;


        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        internal static Type[] GetKnownTypes(ICustomAttributeProvider knownTypeAttributeTarget)
        {
            if (_KnownTypesArray == null)
                InitKnownTypes();
            return _KnownTypesArray;
        }
        static private void InitKnownTypes()
        {
            _KnownTypesHashtable = new Hashtable();
            _KnownTypesArray = new Type[]{ typeof(bool),
                                             typeof(string),
                                             typeof(ArrayList),
                                             typeof(ProfilePropertyMetadata),
                                             typeof(IDictionary<string, object>),
                                             typeof(Collection<string>)
                                           };

            for(int iter=0; iter<_KnownTypesArray.Length; iter++)
                _KnownTypesHashtable.Add(_KnownTypesArray[iter], string.Empty);
        }

        static private void StoreKnownTypes(SettingsPropertyValueCollection propertyValueCollection)
        {
            if (_KnownTypesHashtable == null)
                InitKnownTypes();

            ArrayList al = null;
            foreach(SettingsPropertyValue p in propertyValueCollection) {
                if (!_KnownTypesHashtable.Contains(p.Property.PropertyType)) {
                    _KnownTypesHashtable.Add(p.Property.PropertyType, string.Empty);
                    if (al == null)
                        al = new ArrayList();
                    al.Add(p.Property.PropertyType);
                }
            }

            if (al != null) {
                Type [] temp = new Type[_KnownTypesArray.Length + al.Count];
                _KnownTypesArray.CopyTo(temp, 0);
                al.CopyTo(temp, _KnownTypesArray.Length);
                _KnownTypesArray = temp;
            }
        }

        static private void StoreKnownTypes(SettingsPropertyCollection propertyCollection)
        {
            if (_KnownTypesHashtable == null)
                InitKnownTypes();

            ArrayList al = null;
            foreach(SettingsProperty p in propertyCollection) {
                if (!_KnownTypesHashtable.Contains(p.PropertyType)) {
                    _KnownTypesHashtable.Add(p.PropertyType, string.Empty);
                    if (al == null)
                        al = new ArrayList();
                    al.Add(p.PropertyType);
                }
            }

            if (al != null) {
                Type [] temp = new Type[_KnownTypesArray.Length + al.Count];
                _KnownTypesArray.CopyTo(temp, 0);
                al.CopyTo(temp, _KnownTypesArray.Length);
                _KnownTypesArray = temp;
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private void GetPropertyValuesFromWeb()
        {
            GetPropertyValuesFromWebCore(_HonorCookieExpiry);

            // Any failures?
            bool anyFailures = (_PropertyValues.Count < _Properties.Count);

            if (!_HonorCookieExpiry && anyFailures) {
                // if there were failures, try re-validating the ClientFormsIdentity
                //    and try again

                ClientFormsIdentity id = Thread.CurrentPrincipal.Identity as ClientFormsIdentity;

                if (id != null) {
                    id.RevalidateUser();
                    GetPropertyValuesFromWebCore(true);
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        private void GetPropertyValuesFromWebCore(bool bubbleExceptionFromSvc) {
            string []                        propertyNames  = new string[_Properties.Count];
            int                              iter           = 0;
            CookieContainer                  cookies        = null;
            IIdentity                        id             = Thread.CurrentPrincipal.Identity;

            foreach (SettingsProperty setting in _Properties) {
                propertyNames[iter++] = setting.Name;
            }

            if (id is ClientFormsIdentity)
                cookies = ((ClientFormsIdentity)id).AuthenticationCookies;

            if (_UsingWFCService) {
                throw new NotImplementedException();

//                 CustomBinding                    binding        = ProxyHelper.GetBinding();
//                 ChannelFactory<ProfileService>   channelFactory = new ChannelFactory<ProfileService>(binding, new EndpointAddress(GetServiceUri()));
//                 ProfileService                   clientService  = channelFactory.CreateChannel();
//                 Dictionary<string, object>       propertyValues = null;


//                 using (new OperationContextScope((IContextChannel)clientService)) {
//                     ProxyHelper.AddCookiesToWCF(cookies, GetServiceUri(), id.Name, GetConnectionString(), _ConnectionStringProvider);
//                     propertyValues = clientService.GetPropertiesForCurrentUser(propertyNames, id.IsAuthenticated && (id is ClientFormsIdentity));
//                     ProxyHelper.GetCookiesFromWCF(cookies, GetServiceUri(), id.Name, GetConnectionString(), _ConnectionStringProvider);
//                 }

//                 for(iter = 0; iter<propertyNames.Length; iter++) {
//                     string name = propertyNames[iter];
//                     if (!propertyValues.ContainsKey(name))
//                         continue;
//                     SettingsProperty setting = _Properties[name];
//                     if (setting == null)
//                         continue; // Bad -- why wasn't it found?
//                     bool mustAdd = false;
//                     SettingsPropertyValue value = _PropertyValues[setting.Name];
//                     if (value == null) {
//                         value = new SettingsPropertyValue(setting);
//                         mustAdd = true;
//                     }

//                     //if ((value.SerializedValue is string) && ((string)value.SerializedValue == "(null)")) {
//                     value.PropertyValue = propertyValues[name];
//                     value.Deserialized = true;
//                     //                 } else if (setting.SerializeAs == SettingsSerializeAs.Binary)
//                     //                     value.SerializedValue = Convert.FromBase64String(propertyValues[iter]);
//                     //                 else
//                     //                     value.SerializedValue = propertyValues[iter];
//                     //                 value.Deserialized = false;
//                     value.IsDirty = false;
//                     if (mustAdd)
//                         _PropertyValues.Add(value);
//                 }
            } else {

                string [] paramNames  = new string [] {"properties", "authenticatedUserOnly" };
                object [] paramValues = new object [] {propertyNames, id.IsAuthenticated && (id is ClientFormsIdentity) };
                object    obj         = null;
                try {
                    obj = ProxyHelper.CreateWebRequestAndGetResponse(
                            GetServiceUri() + "/GetPropertiesForCurrentUser",
                            ref cookies,
                            id.Name,
                            _ConnectionString,
                            _ConnectionStringProvider,
                            paramNames,
                            paramValues,
                            typeof(Dictionary<string, object>));

                } catch {
                    if (bubbleExceptionFromSvc)
                        throw;
                }

                if (obj != null) {
                    Dictionary<string, object> ret = (Dictionary<string, object>) obj;

                    foreach(KeyValuePair<string,object> de in ret) {
                        SettingsProperty setting = _Properties[(string) de.Key];
                        if (setting == null)
                            continue; // Bad -- why wasn't it found?
                        bool mustAdd = false;
                        SettingsPropertyValue value = _PropertyValues[setting.Name];
                        if (value == null) {
                            value = new SettingsPropertyValue(setting);
                            mustAdd = true;
                        }

                        if (de.Value != null && !setting.PropertyType.IsAssignableFrom(de.Value.GetType()))
                        {
                            object convertedValue = null;
                            if (!ObjectConverter.TryConvertObjectToType(de.Value,
                                                                        setting.PropertyType,
                                                                        new JavaScriptSerializer(),
                                                                        out convertedValue))
                            {
                                // Failure to convert!
                                continue;
                            }
                            value.PropertyValue = convertedValue;
                        } else {
                            value.PropertyValue = de.Value;

                        }

                        value.Deserialized = true;
                        value.IsDirty = false;
                        if (mustAdd)
                            _PropertyValues.Add(value);
                    }
                }
            }
        }


        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private Collection<string> SetPropertyValuesWeb(SettingsPropertyValueCollection values, bool cacheIsMoreFresh)
        {
            bool anyFailures = false;
            Collection<string> errorList = null;
            ClientFormsIdentity id = Thread.CurrentPrincipal.Identity as ClientFormsIdentity;
            try {
                errorList = SetPropertyValuesWebCore(values, cacheIsMoreFresh);
                anyFailures = (errorList != null && errorList.Count > 0);
            }
            catch (WebException) {
                if (id == null || _HonorCookieExpiry) {
                    throw;
                }
                anyFailures = true;
            }

            if (!_HonorCookieExpiry && anyFailures) {
                // if there were failures, try re-validating the ClientFormsIdentity
                //    and try again
                if (id != null) {
                    id.RevalidateUser();
                    errorList = SetPropertyValuesWebCore(values, cacheIsMoreFresh);
                }
            }
            return errorList;
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        [SuppressMessage("Microsoft.Security", "CA2116:AptcaMethodsShouldOnlyCallAptcaMethods", Justification="Reviewed and approved by feature crew")]
        private Collection<string> SetPropertyValuesWebCore(SettingsPropertyValueCollection values, bool cacheIsMoreFresh)
        {
            Dictionary<string, object>       propertyValues = new Dictionary<string, object>();
            Collection<string> errorList   = null;
            foreach(SettingsPropertyValue value in values) {
                if (cacheIsMoreFresh || value.IsDirty) {
                    propertyValues.Add(value.Property.Name, value.PropertyValue);
                }
            }

            CookieContainer                  cookies           = null;
            IIdentity                        id                = Thread.CurrentPrincipal.Identity;

            if (id is ClientFormsIdentity)
                cookies = ((ClientFormsIdentity)id).AuthenticationCookies;


            if (_UsingWFCService) {
                throw new NotImplementedException();

//                 CustomBinding                    binding           = ProxyHelper.GetBinding();
//                 ChannelFactory<ProfileService>   channelFactory    = new ChannelFactory<ProfileService>(binding, new EndpointAddress(GetServiceUri()));
//                 ProfileService                   clientService     = channelFactory.CreateChannel();
//                 using (new OperationContextScope((IContextChannel)clientService)) {
//                     ProxyHelper.AddCookiesToWCF(cookies, GetServiceUri(), id.Name, GetConnectionString(), _ConnectionStringProvider);
//                     errorList = clientService.SetPropertiesForCurrentUser(propertyValues, id.IsAuthenticated && (id is ClientFormsIdentity));
//                     ProxyHelper.GetCookiesFromWCF(cookies, GetServiceUri(), id.Name, GetConnectionString(), _ConnectionStringProvider);
//                 }
            } else {
                // Collection<string> SetPropertiesForCurrentUser(IDictionary<string, object> values, bool authenticatedUserOnly)
                string [] paramNames = new string [] {"values", "authenticatedUserOnly" };
                object [] paramValues = new object [] {propertyValues, id.IsAuthenticated && (id is ClientFormsIdentity) };

                object o = ProxyHelper.CreateWebRequestAndGetResponse(GetServiceUri() + "/SetPropertiesForCurrentUser",
                                                                      ref cookies,
                                                                      id.Name,
                                                                      _ConnectionString,
                                                                      _ConnectionStringProvider,
                                                                      paramNames,
                                                                      paramValues,
                                                                      typeof(Collection<string>));
                errorList = (Collection<string>) o;
            }
            SetIsCacheMoreFresh(false);
            return errorList;
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private void GetPropertyValuesFromSQL()
        {
            if (_UsingFileSystemStore || _UsingIsolatedStore)
            {
                ClientData cd = ClientDataManager.GetUserClientData(Thread.CurrentPrincipal.Identity.Name, _UsingIsolatedStore);
                if (cd.SettingsNames == null || cd.SettingsValues == null)
                    return;
                int len = cd.SettingsNames.Length;
                if (cd.SettingsNames.Length != cd.SettingsStoredAs.Length || cd.SettingsValues.Length != cd.SettingsStoredAs.Length)
                {
                    return; // Bad!
                }
                for(int iter=0; iter<len; iter++) {
                    AddProperty(cd.SettingsNames[iter], cd.SettingsStoredAs[iter], cd.SettingsValues[iter]);
                }
                return;
            }

            using (DbConnection conn = SqlHelper.GetConnection(Thread.CurrentPrincipal.Identity.Name,
                                                                        GetConnectionString(), _ConnectionStringProvider))
            {
                DbTransaction trans = null;
                try {
                    trans = conn.BeginTransaction();
                    DbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT PropertyName, PropertyStoredAs, PropertyValue FROM Settings";
                    cmd.Transaction = trans;
                    using(DbDataReader reader = cmd.ExecuteReader()) {
                        while(reader.Read()) {
                            string   name       = reader.GetString(0);
                            string   storedAs   = reader.GetString(1);
                            string   propVal    = (reader.IsDBNull(2) ? null : reader.GetString(2));

                            AddProperty(name, storedAs, propVal);
                        }
                    }
                } catch {
                    if (trans != null) {
                        trans.Rollback();
                        trans = null;
                    }
                    throw;
                } finally {
                    if (trans != null)
                        trans.Commit();
                }
            }
        }

        private void AddProperty(string name, string storedAs, string propVal)
        {
            if (storedAs != "S" && storedAs != "B" && storedAs != "N")
                return;

            SettingsProperty prop = _Properties[name];
            if (prop == null)
                return;
            SettingsPropertyValue value    = _PropertyValues[name];
            bool                  mustAdd  = false;
            if (value == null) {
                value = new SettingsPropertyValue(prop);
                mustAdd = true;
            }

            switch(storedAs) {
            case "S":
                value.SerializedValue = propVal;
                break;
            case "B":
                value.SerializedValue = Convert.FromBase64String(propVal);
                break;
            case "N":
                value.SerializedValue = null;
                break;
            }
            value.Deserialized = false;
            value.IsDirty = false;
            if (mustAdd)
                _PropertyValues.Add(value);
        }
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////
        private void SetPropertyValuesSQL(SettingsPropertyValueCollection values, bool updateSaveTime)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;

            if (_UsingFileSystemStore || _UsingIsolatedStore)
            {
                ClientData cd = ClientDataManager.GetUserClientData(username, _UsingIsolatedStore);
                cd.SettingsNames = new string[values.Count];
                cd.SettingsStoredAs = new string[values.Count];
                cd.SettingsValues = new string[values.Count];

                int iter=0;

                foreach(SettingsPropertyValue value in values) {

                    cd.SettingsNames[iter] = value.Property.Name;

                    object val = value.SerializedValue;
                    if (val == null) {
                        cd.SettingsStoredAs[iter] = "N";
                    } else if (val is string) {
                        cd.SettingsStoredAs[iter] = "S";
                        cd.SettingsValues[iter] = (string) val;
                    } else {
                        cd.SettingsStoredAs[iter] = "B";
                        cd.SettingsValues[iter] = Convert.ToBase64String((byte[])val);
                    }

                    iter++;
                }
                if (updateSaveTime)
                    cd.SettingsCacheIsMoreFresh = true;
                cd.Save();
                return;
            }

            using (DbConnection conn = SqlHelper.GetConnection(username, GetConnectionString(), _ConnectionStringProvider)) {
                DbTransaction trans = null;
                try {
                    trans = conn.BeginTransaction();
                    foreach(SettingsPropertyValue value in values) {
                        DbCommand cmd = conn.CreateCommand();
                        cmd.Transaction = trans;
                        cmd.CommandText = "DELETE FROM Settings WHERE PropertyName = @PropName";
                        SqlHelper.AddParameter(conn, cmd, "@PropName", value.Property.Name);
                        cmd.ExecuteNonQuery();

                        cmd = conn.CreateCommand();
                        cmd.Transaction = trans;
                        object val = value.SerializedValue;
                        if (val == null) {
                            cmd.CommandText = "INSERT INTO Settings (PropertyName, PropertyStoredAs, PropertyValue) VALUES (@PropName, 'N', '')";
                            SqlHelper.AddParameter(conn, cmd, "@PropName", value.Property.Name);
                        } else if (val is string) {
                            cmd.CommandText = "INSERT INTO Settings (PropertyName, PropertyStoredAs, PropertyValue) VALUES (@PropName, 'S', @PropVal)";
                            SqlHelper.AddParameter(conn, cmd, "@PropName", value.Property.Name);
                            SqlHelper.AddParameter(conn, cmd, "@PropVal", (string) val);
                        } else {
                            cmd.CommandText = "INSERT INTO Settings (PropertyName, PropertyStoredAs, PropertyValue) VALUES (@PropName, 'B', @PropVal)";
                            SqlHelper.AddParameter(conn, cmd, "@PropName", value.Property.Name);
                            SqlHelper.AddParameter(conn, cmd, "@PropVal", Convert.ToBase64String((byte[])val));
                        }
                        cmd.ExecuteNonQuery();
                    }
                } catch {
                    if (trans != null) {
                        trans.Rollback();
                        trans = null;
                    }
                    throw;
                } finally {
                    if (trans != null)
                        trans.Commit();
                }
            }
            if (updateSaveTime)
                SetIsCacheMoreFresh(true);
        }

        private bool GetNeedToReset()
        {
            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetUserClientData(Thread.CurrentPrincipal.Identity.Name, _UsingIsolatedStore);
                return cd.SettingsNeedReset;
            } else {
                string temp = GetTagValue("NeeedToDoReset");
                return (temp != null && temp == "1");
            }
        }
        private void SetNeedToReset(bool fSet)
        {
            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetUserClientData(Thread.CurrentPrincipal.Identity.Name, _UsingIsolatedStore);
                cd.SettingsNeedReset = fSet;
                cd.Save();
            } else {
                SetTagValue("NeeedToDoReset", fSet ? "1" : "0");
            }
        }
        private bool GetIsCacheMoreFresh()
        {
            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetUserClientData(Thread.CurrentPrincipal.Identity.Name, _UsingIsolatedStore);
                return cd.SettingsCacheIsMoreFresh;
            } else {
                string temp = GetTagValue("IsCacheMoreFresh");
                return (temp != null && temp == "1");
            }
        }
        private void SetIsCacheMoreFresh(bool fSet)
        {
            if (_UsingFileSystemStore || _UsingIsolatedStore) {
                ClientData cd = ClientDataManager.GetUserClientData(Thread.CurrentPrincipal.Identity.Name, _UsingIsolatedStore);
                cd.SettingsCacheIsMoreFresh = fSet;
                cd.Save();
            } else {
                SetTagValue("IsCacheMoreFresh", fSet ? "1" : "0");
            }
        }
        private string GetTagValue(string tagName)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;
            using (DbConnection conn = SqlHelper.GetConnection(username, GetConnectionString(), _ConnectionStringProvider))
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT PropertyValue FROM Settings WHERE PropertyName = @PropName AND PropertyStoredAs='I'";
                SqlHelper.AddParameter(conn, cmd, "@PropName", tagName);
                return cmd.ExecuteScalar() as string;
            }
        }
        private void SetTagValue(string tagName, string tagValue)
        {
            string username = Thread.CurrentPrincipal.Identity.Name;
            using (DbConnection conn = SqlHelper.GetConnection(username, GetConnectionString(), _ConnectionStringProvider)) {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = @"DELETE FROM Settings WHERE PropertyName = @PropName AND PropertyStoredAs='I'";
                SqlHelper.AddParameter(conn, cmd, "@PropName", tagName);
                cmd.ExecuteNonQuery();
                if (tagValue != null) {
                    cmd = conn.CreateCommand();
                    cmd.CommandText = @"INSERT INTO Settings (PropertyName, PropertyStoredAs, PropertyValue) VALUES  (@PropName, 'I', @PropValue)";
                    SqlHelper.AddParameter(conn, cmd, "@PropName", tagName);
                    SqlHelper.AddParameter(conn, cmd, "@PropValue", tagValue);
                    cmd.ExecuteNonQuery();
                 }
            }
        }

        private void RegisterForValidateUserEvent()
        {
            foreach(MembershipProvider provider in Membership.Providers) {
                EventInfo ei = provider.GetType().GetEvent("UserValidated");
                if (ei == null)
                    continue;

                MethodInfo addMethod = ei.GetAddMethod();
                if (addMethod == null)
                    continue;

                ParameterInfo[] addMethodParams = addMethod.GetParameters();
                Delegate handlerDelegate = Delegate.CreateDelegate(addMethodParams[0].ParameterType, this, "OnUserValidated");
                addMethod.Invoke(provider, new Object[1]{handlerDelegate});
            }

        }


        [SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode", Justification="Called via Reflection")]
        private void OnUserValidated(object src, UserValidatedEventArgs e)
        {
            _NeedToDoReset = GetNeedToReset();
            if (_Properties != null && _Properties.Count > 0 && string.Compare(e.UserName, _UserName, StringComparison.OrdinalIgnoreCase) != 0) {
                try {
                    if (_SettingsBaseClass != null)
                        _SettingsBaseClass.Reload();
                    // _PropertyValues = new SettingsPropertyValueCollection();
                    // GetPropertyValuesCore(); // refresh the collection
                } catch {
                    // it's possible that the (new) user doesn't have the same props
                    // if (_PropertyValues.Count < _Properties.Count)
                    //    SetRemainingValuesToDefault();
                }
            }
        }

        private void SetRemainingValuesToDefault()
        {
            foreach (SettingsProperty prop in _Properties) {
                SettingsPropertyValue value = _PropertyValues[prop.Name];
                if (value == null) {
                    value = new SettingsPropertyValue(prop);
                    value.SerializedValue = prop.DefaultValue;
                    value.Deserialized = false;
                    object o = value.PropertyValue;
                    value.PropertyValue = o;
                    _PropertyValues.Add(value);
                }
            }
        }
        private string GetConnectionString()
        {
            if (_ConnectionString == null)
                _ConnectionString = SqlHelper.GetDefaultConnectionString();
            return _ConnectionString;
        }
    }
}
