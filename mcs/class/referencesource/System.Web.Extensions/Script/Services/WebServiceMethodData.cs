//------------------------------------------------------------------------------
// <copyright file="WebServiceMethodData.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Web.Resources;
using System.Web.Script.Serialization;
using System.Web.Services;

namespace System.Web.Script.Services {

    internal class WebServiceMethodData {
        private MethodInfo _methodInfo;
        private WebMethodAttribute _webMethodAttribute;
        private ScriptMethodAttribute _scriptMethodAttribute;
        private string _methodName;
        private Dictionary<string, WebServiceParameterData> _parameterData;
        private WebServiceData _owner;
        private bool? _useHttpGet;

        internal WebServiceMethodData(WebServiceData owner, MethodInfo methodInfo, WebMethodAttribute webMethodAttribute, ScriptMethodAttribute scriptMethodAttribute) {
            _owner = owner;
            _methodInfo = methodInfo;
            _webMethodAttribute = webMethodAttribute;
            _methodName = _webMethodAttribute.MessageName;
            _scriptMethodAttribute = scriptMethodAttribute;
            if (String.IsNullOrEmpty(_methodName)) {
                _methodName = methodInfo.Name;
            }
        }
        
        // This constructor is only used by WCF. Owener, MethodName, ParameterDataDictionary, ParamterData
        // are the only valid properties in WCF case.
        internal WebServiceMethodData(WebServiceData owner, string methodName, Dictionary<string, WebServiceParameterData> parameterData, bool useHttpGet) {
            _owner = owner;
            _methodName = methodName;
            _parameterData = parameterData;
            _useHttpGet = useHttpGet;
        }

        internal WebServiceData Owner {
            get {
                return _owner;
            }
        }

        private void EnsureParameters() {

            // Build the parameters collection on demand
            if (_parameterData != null)
                return;

            lock (this) {
                Dictionary<string, WebServiceParameterData> parameterData = new Dictionary<string, WebServiceParameterData>();
                int index = 0;
                foreach (ParameterInfo param in _methodInfo.GetParameters()) {
                    parameterData[param.Name] = new WebServiceParameterData(param, index);
                    index++;
                }
                _parameterData = parameterData;
            }
        }

        internal string MethodName {
            get {
                return _methodName;
            }
        }

        internal MethodInfo MethodInfo {
            get {
                return _methodInfo;
            }
        }

        internal IDictionary<string, WebServiceParameterData> ParameterDataDictionary {
            get {
                EnsureParameters();
                return _parameterData;
            }
        }

        internal ICollection<WebServiceParameterData> ParameterDatas {
            get {
                return ParameterDataDictionary.Values;
            }
        }

        internal int CacheDuration {
            get {
                Debug.Assert(_webMethodAttribute != null); // If fails: WebserviceMethodData corrosponding to WCF is being used by ASMX JSON handler
                return _webMethodAttribute.CacheDuration;
            }
        }

        internal bool RequiresSession {
            get {
                Debug.Assert(_webMethodAttribute != null); // If fails: WebserviceMethodData corrosponding to WCF is being used by ASMX JSON handler
                return _webMethodAttribute.EnableSession;
            }
        }

        internal bool IsStatic {
            get {
                Debug.Assert(_methodInfo != null); // If fails: WebserviceMethodData corrosponding to WCF is being used by ASMX JSON handler
                return _methodInfo.IsStatic;
            }
        }

        internal Type ReturnType {
            get {
                Debug.Assert(_methodInfo != null); // If fails: WebserviceMethodData corrosponding to WCF is being used by ASMX JSON handler
                return (_methodInfo == null) ? null : _methodInfo.ReturnType;
            }
        }
        
        internal bool UseXmlResponse {
            get {
                if (_scriptMethodAttribute != null) {
                    return _scriptMethodAttribute.ResponseFormat == ResponseFormat.Xml;
                }
                return false;
            }
        }

        internal bool XmlSerializeString {
            get {
                if (_scriptMethodAttribute != null) {
                    return _scriptMethodAttribute.XmlSerializeString;
                }
                return false;
            }
        }

        internal bool UseGet {
            get {
                if (_useHttpGet != null) {
                    return _useHttpGet.Value;
                }
                if (_scriptMethodAttribute != null) {
                    return _scriptMethodAttribute.UseHttpGet;
                }
                return false;
            }
        }

        internal object CallMethodFromRawParams(object target, IDictionary<string, object> parameters) {
            // Process the 'raw' parameters so that we use strongly typed objects when possible
            parameters = StrongTypeParameters(parameters);
            return CallMethod(target, parameters);
        }

        private object CallMethod(object target, IDictionary<string, object> parameters) {
            // Make sure we have all the data about this method's parameters
            EnsureParameters();

            // Allocate an object array for all the formal parameters (whether passed in or not)
            object[] actualParams = new object[_parameterData.Count];

            // Go through all the actual parameters
            foreach (WebServiceParameterData paramData in _parameterData.Values) {
                object value;
                if (parameters.TryGetValue(paramData.ParameterInfo.Name, out value)) {
                    actualParams[paramData.Index] = value;
                }
                else {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.WebService_MissingArg, paramData.ParameterInfo.Name));
                }
            }

            // Make the actual method call
            return _methodInfo.Invoke(target, actualParams);
        }

        private IDictionary<string, object> StrongTypeParameters(IDictionary<string, object> rawParams) {
            Debug.Assert(ParameterDataDictionary != null);
            IDictionary<string, WebServiceParameterData> paramDataDictionary = ParameterDataDictionary;

            // Allocate a dictionary to hold the processed parameters.
            IDictionary<string, object> result = new Dictionary<string, object>(rawParams.Count);

            // Go through all the raw parameters
            foreach (KeyValuePair<string, object> pair in rawParams) {
                string memberName = pair.Key;
                if (paramDataDictionary.ContainsKey(memberName)) {
                    // Get the type of the formal parameter
                    Type paramType = paramDataDictionary[memberName].ParameterInfo.ParameterType;

                    // Convert the raw parameter based on that type
                    result[memberName] = ObjectConverter.ConvertObjectToType(pair.Value, paramType, Owner.Serializer);
                }
            }
            return result;
        }
    }
}
