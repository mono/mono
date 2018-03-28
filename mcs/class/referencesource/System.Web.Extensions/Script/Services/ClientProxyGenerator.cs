//------------------------------------------------------------------------------
// <copyright file="ClientProxyGenerator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Services {
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using System.Web;
    using System.Web.Script.Serialization;

    internal abstract class ClientProxyGenerator {
        private static string DebugXmlComments = @"/// <param name=""succeededCallback"" type=""Function"" optional=""true"" mayBeNull=""true""></param>
/// <param name=""failedCallback"" type=""Function"" optional=""true"" mayBeNull=""true""></param>
/// <param name=""userContext"" optional=""true"" mayBeNull=""true""></param>
";
        private Hashtable _registeredNamespaces = new Hashtable();
        private Hashtable _ensuredObjectParts = new Hashtable();
        protected StringBuilder _builder;
        protected bool _debugMode;
        // comments are the same in the instance methods as they are in the static methods
        // this cache is used when calculating comments for instance methods, then re-used when
        // writing out static methods.
        private Dictionary<string, string> _docCommentCache;

        internal string GetClientProxyScript(WebServiceData webServiceData) {
            if (webServiceData.MethodDatas.Count == 0) return null;
            _builder = new StringBuilder();

            if (_debugMode) {
                _docCommentCache = new Dictionary<string, string>();
            }

            // Constructor
            GenerateConstructor(webServiceData);

            // Prototype functions
            GeneratePrototype(webServiceData);

            GenerateRegisterClass(webServiceData);
            GenerateStaticInstance(webServiceData);
            GenerateStaticMethods(webServiceData);

            // Generate some client proxy to make some types instantiatable on the client
            GenerateClientTypeProxies(webServiceData);
            GenerateEnumTypeProxies(webServiceData.EnumTypes);
            return _builder.ToString();
        }

        protected void GenerateRegisterClass(WebServiceData webServiceData) {
            // Generate registerClass: Foo.NS.WebService.registerClass('Foo.NS.WebService', Sys.Net.WebServiceProxy);
            string typeName = GetProxyTypeName(webServiceData);
            _builder.Append(typeName).Append(".registerClass('").Append(typeName).Append("',Sys.Net.WebServiceProxy);\r\n");
        }

        protected virtual void GenerateConstructor(WebServiceData webServiceData) {
            GenerateTypeDeclaration(webServiceData, false);
            _builder.Append("function() {\r\n");
            _builder.Append(GetProxyTypeName(webServiceData)).Append(".initializeBase(this);\r\n");
            GenerateFields();
            _builder.Append("}\r\n");
        }

        protected virtual void GeneratePrototype(WebServiceData webServiceData) {
            GenerateTypeDeclaration(webServiceData, true);
            _builder.Append("{\r\n");
            // private method to return the path to be used , returns _path from current instance if set, otherwise returns _path from static instance.
            _builder.Append("_get_path:function() {\r\n var p = this.get_path();\r\n if (p) return p;\r\n else return ");
            _builder.Append(GetProxyTypeName(webServiceData)).Append("._staticInstance.get_path();},\r\n");
            bool first = true;
            foreach (WebServiceMethodData methodData in webServiceData.MethodDatas) {
                if (!first) {
                    _builder.Append(",\r\n");
                }
                first = false;
                GenerateWebMethodProxy(methodData);
            }
            _builder.Append("}\r\n");
        }

        protected virtual void GenerateTypeDeclaration(WebServiceData webServiceData, bool genClass) {
            AppendClientTypeDeclaration(webServiceData.TypeData.TypeNamespace, webServiceData.TypeData.TypeName, genClass, true);
        }

        protected void GenerateFields() {
            _builder.Append("this._timeout = 0;\r\n");
            _builder.Append("this._userContext = null;\r\n");
            _builder.Append("this._succeeded = null;\r\n");
            _builder.Append("this._failed = null;\r\n");
        }

        protected virtual void GenerateMethods() {
        }

        protected void GenerateStaticMethods(WebServiceData webServiceData) {
            string className = GetProxyTypeName(webServiceData);
            // Now generate static methods NS.Service.MyMethod = function()
            foreach (WebServiceMethodData methodData in webServiceData.MethodDatas) {
                string methodName = methodData.MethodName;
                _builder.Append(className).Append('.').Append(methodName).Append("= function(");
                StringBuilder argBuilder = new StringBuilder();
                bool first = true;
                foreach (WebServiceParameterData paramData in methodData.ParameterDatas) {
                    if (!first) argBuilder.Append(',');
                    else first = false;
                    argBuilder.Append(paramData.ParameterName);
                }
                if (!first) argBuilder.Append(',');
                argBuilder.Append("onSuccess,onFailed,userContext");

                _builder.Append(argBuilder.ToString()).Append(") {");

                if (_debugMode) {
                    // doc comments should have been computed already
                    _builder.Append("\r\n");
                    _builder.Append(_docCommentCache[methodName]);
                }

                _builder.Append(className).Append("._staticInstance.").Append(methodName).Append('(');
                _builder.Append(argBuilder.ToString()).Append("); }\r\n");
            }
        }

        protected abstract string GetProxyPath();

        protected virtual string GetJsonpCallbackParameterName() {
            return null;
        }

        protected virtual bool GetSupportsJsonp() {
            return false;
        }

        protected void GenerateStaticInstance(WebServiceData data) {
            string typeName = GetProxyTypeName(data);
            _builder.Append(typeName).Append("._staticInstance = new ").Append(typeName).Append("();\r\n");

            // Generate the static properties
            if (_debugMode) {
                _builder.Append(typeName).Append(".set_path = function(value) {\r\n");
                _builder.Append(typeName).Append("._staticInstance.set_path(value); }\r\n");
                _builder.Append(typeName).Append(".get_path = function() { \r\n/// <value type=\"String\" mayBeNull=\"true\">The service url.</value>\r\nreturn ");
                _builder.Append(typeName).Append("._staticInstance.get_path();}\r\n");
                
                _builder.Append(typeName).Append(".set_timeout = function(value) {\r\n");
                _builder.Append(typeName).Append("._staticInstance.set_timeout(value); }\r\n");
                _builder.Append(typeName).Append(".get_timeout = function() { \r\n/// <value type=\"Number\">The service timeout.</value>\r\nreturn ");
                _builder.Append(typeName).Append("._staticInstance.get_timeout(); }\r\n");
                
                _builder.Append(typeName).Append(".set_defaultUserContext = function(value) { \r\n");
                _builder.Append(typeName).Append("._staticInstance.set_defaultUserContext(value); }\r\n");
                _builder.Append(typeName).Append(".get_defaultUserContext = function() { \r\n/// <value mayBeNull=\"true\">The service default user context.</value>\r\nreturn ");
                _builder.Append(typeName).Append("._staticInstance.get_defaultUserContext(); }\r\n");
                
                _builder.Append(typeName).Append(".set_defaultSucceededCallback = function(value) { \r\n ");
                _builder.Append(typeName).Append("._staticInstance.set_defaultSucceededCallback(value); }\r\n");
                _builder.Append(typeName).Append(".get_defaultSucceededCallback = function() { \r\n/// <value type=\"Function\" mayBeNull=\"true\">The service default succeeded callback.</value>\r\nreturn ");
                _builder.Append(typeName).Append("._staticInstance.get_defaultSucceededCallback(); }\r\n");
                
                _builder.Append(typeName).Append(".set_defaultFailedCallback = function(value) { \r\n");
                _builder.Append(typeName).Append("._staticInstance.set_defaultFailedCallback(value); }\r\n");
                _builder.Append(typeName).Append(".get_defaultFailedCallback = function() { \r\n/// <value type=\"Function\" mayBeNull=\"true\">The service default failed callback.</value>\r\nreturn ");
                _builder.Append(typeName).Append("._staticInstance.get_defaultFailedCallback(); }\r\n");

                _builder.Append(typeName).Append(".set_enableJsonp = function(value) { ");
                _builder.Append(typeName).Append("._staticInstance.set_enableJsonp(value); }\r\n");
                _builder.Append(typeName).Append(".get_enableJsonp = function() { \r\n/// <value type=\"Boolean\">Specifies whether the service supports JSONP for cross domain calling.</value>\r\nreturn ");
                _builder.Append(typeName).Append("._staticInstance.get_enableJsonp(); }\r\n");

                _builder.Append(typeName).Append(".set_jsonpCallbackParameter = function(value) { ");
                _builder.Append(typeName).Append("._staticInstance.set_jsonpCallbackParameter(value); }\r\n");
                _builder.Append(typeName).Append(".get_jsonpCallbackParameter = function() { \r\n/// <value type=\"String\">Specifies the parameter name that contains the callback function name for a JSONP request.</value>\r\nreturn ");
                _builder.Append(typeName).Append("._staticInstance.get_jsonpCallbackParameter(); }\r\n");
            }
            else {
                _builder.Append(typeName).Append(".set_path = function(value) { ");
                _builder.Append(typeName).Append("._staticInstance.set_path(value); }\r\n");
                _builder.Append(typeName).Append(".get_path = function() { return ");
                _builder.Append(typeName).Append("._staticInstance.get_path(); }\r\n");

                _builder.Append(typeName).Append(".set_timeout = function(value) { ");
                _builder.Append(typeName).Append("._staticInstance.set_timeout(value); }\r\n");
                _builder.Append(typeName).Append(".get_timeout = function() { return ");
                _builder.Append(typeName).Append("._staticInstance.get_timeout(); }\r\n");

                _builder.Append(typeName).Append(".set_defaultUserContext = function(value) { ");
                _builder.Append(typeName).Append("._staticInstance.set_defaultUserContext(value); }\r\n");
                _builder.Append(typeName).Append(".get_defaultUserContext = function() { return ");
                _builder.Append(typeName).Append("._staticInstance.get_defaultUserContext(); }\r\n");

                _builder.Append(typeName).Append(".set_defaultSucceededCallback = function(value) { ");
                _builder.Append(typeName).Append("._staticInstance.set_defaultSucceededCallback(value); }\r\n");
                _builder.Append(typeName).Append(".get_defaultSucceededCallback = function() { return ");
                _builder.Append(typeName).Append("._staticInstance.get_defaultSucceededCallback(); }\r\n");

                _builder.Append(typeName).Append(".set_defaultFailedCallback = function(value) { ");
                _builder.Append(typeName).Append("._staticInstance.set_defaultFailedCallback(value); }\r\n");
                _builder.Append(typeName).Append(".get_defaultFailedCallback = function() { return ");
                _builder.Append(typeName).Append("._staticInstance.get_defaultFailedCallback(); }\r\n");

                _builder.Append(typeName).Append(".set_enableJsonp = function(value) { ");
                _builder.Append(typeName).Append("._staticInstance.set_enableJsonp(value); }\r\n");
                _builder.Append(typeName).Append(".get_enableJsonp = function() { return ");
                _builder.Append(typeName).Append("._staticInstance.get_enableJsonp(); }\r\n");

                _builder.Append(typeName).Append(".set_jsonpCallbackParameter = function(value) { ");
                _builder.Append(typeName).Append("._staticInstance.set_jsonpCallbackParameter(value); }\r\n");
                _builder.Append(typeName).Append(".get_jsonpCallbackParameter = function() { return ");
                _builder.Append(typeName).Append("._staticInstance.get_jsonpCallbackParameter(); }\r\n");
            }

            // the path has to be the full absolete path if this is a JSONP enabled service. But it is the responsibility
            // of the caller to GetClientProxyScript to pass the full path if appropriate since determining it may be
            // dependant on the specific technology.
            string proxyPath = GetProxyPath();
            if (!String.IsNullOrEmpty(proxyPath) && (proxyPath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) || proxyPath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))) {
                // DevDiv 91322: avoid url encoding the domain portion of an IDN url
                // find the first "/" after the scheme, and only encode after that.
                int domainStart = proxyPath.IndexOf("://", StringComparison.OrdinalIgnoreCase) + "://".Length;
                int domainEnd = proxyPath.IndexOf("/", domainStart, StringComparison.OrdinalIgnoreCase);
                // if no slash after :// was found, it could be a domain only url, http://[some service].com, don't encode any of it
                if (domainEnd != -1) {
                    proxyPath = proxyPath.Substring(0, domainEnd) + HttpUtility.UrlPathEncode(proxyPath.Substring(domainEnd));
                }
            }
            else {
                // it doesn't appear to be an absolute url, at least not an http or https one. All relative paths
                // and other oddities are safely encoded with UrlPathEncode.
                proxyPath = HttpUtility.UrlPathEncode(proxyPath);
            }
            _builder.Append(typeName).Append(".set_path(\"").Append(proxyPath).Append("\");\r\n");
            if (GetSupportsJsonp()) {
                _builder.Append(typeName).Append(".set_enableJsonp(true);\r\n");
                string jsonpParameterName = GetJsonpCallbackParameterName();
                if (!String.IsNullOrEmpty(jsonpParameterName) && !jsonpParameterName.Equals("callback", StringComparison.Ordinal)) {
                    _builder.Append(typeName).Append(".set_jsonpCallbackParameter(").Append(JavaScriptSerializer.SerializeInternal(jsonpParameterName)).Append(");\r\n");
                }
            }
        }

        private  void BuildArgsDictionary(WebServiceMethodData methodData, StringBuilder args, StringBuilder argsDict, StringBuilder docComments) {
            argsDict.Append('{');
            foreach (WebServiceParameterData paramData in methodData.ParameterDatas) {
                string name = paramData.ParameterName;
                if (docComments != null) {
                    // looks like: /// <param name="foo" type="ClientType">Namespace.ServerType</param>
                    // client type may not match server type for built in js types like date, number, etc.
                    // client type may be omitted for type Object.
                    docComments.Append("/// <param name=\"").Append(name).Append("\"");
                    Type serverType = ServicesUtilities.UnwrapNullableType(paramData.ParameterType);
                    string clientType = GetClientTypeNamespace(ServicesUtilities.GetClientTypeFromServerType(methodData.Owner, serverType));
                    if (!String.IsNullOrEmpty(clientType)) {
                        docComments.Append(" type=\"").Append(clientType).Append("\"");
                    }
                    docComments.Append(">").Append(serverType.FullName).Append("</param>\r\n");
                }
                if (args.Length > 0) {
                    args.Append(',');
                    argsDict.Append(',');
                }
                args.Append(name);
                argsDict.Append(name).Append(':').Append(name);
            }
            if (docComments != null) {
                // append the built-in comments that all methods have (success, failed, usercontext parameters)
                docComments.Append(DebugXmlComments);
            }
            argsDict.Append("}");
            if (args.Length > 0) {
                args.Append(',');
            }
            args.Append("succeededCallback, failedCallback, userContext");
        }

        private void GenerateWebMethodProxy(WebServiceMethodData methodData) {
            string methodName = methodData.MethodName;
            string typeName = GetProxyTypeName(methodData.Owner);
            string useGet = methodData.UseGet ? "true" : "false";

            _builder.Append(methodName).Append(':');
            // e.g. MyMethod : function(param1, param2, ..., OnSuccess, OnFailure)
            StringBuilder args = new StringBuilder();
            StringBuilder argsDict = new StringBuilder();
            StringBuilder docComments = null;
            string docCommentsString = null;
            if (_debugMode) {
                docComments = new StringBuilder();
            }
            BuildArgsDictionary(methodData, args, argsDict, docComments);

            if (_debugMode) {
                // Remember the doc comments for the static instance case
                docCommentsString = docComments.ToString();
                _docCommentCache[methodName] = docCommentsString;
            }

            // Method calls look like this.invoke(FooNS.Sub.Method.get_path(), 'MethodName', true[useGet], {'arg1':'val1', 'arg2':'val2' }, onComplete, onError, userContext, 'FooNS.Sub.Method')
            _builder.Append("function(").Append(args.ToString()).Append(") {\r\n");
            if (_debugMode) {
                // docCommentsString always end in \r\n
                _builder.Append(docCommentsString);
            }
            _builder.Append("return this._invoke(this._get_path(), ");
            _builder.Append("'").Append(methodName).Append("',");
            _builder.Append(useGet).Append(',');
            _builder.Append(argsDict.ToString()).Append(",succeededCallback,failedCallback,userContext); }");
        }

        /* e.g
          var Qqq = function() { this.__type = "Qqq"; }
         */
        private void GenerateClientTypeProxies(WebServiceData data) {
            bool first = true;
            foreach (WebServiceTypeData t in data.ClientTypes) {
                if (first) {
                    _builder.Append("var gtc = Sys.Net.WebServiceProxy._generateTypedConstructor;\r\n");
                    first = false;
                }

                string typeID = data.GetTypeStringRepresentation(t);
                string typeNameWithClientNamespace = GetClientTypeNamespace(t.TypeName);
                string typeName = ServicesUtilities.GetClientTypeName(typeNameWithClientNamespace);
                string clientTypeNamespace = GetClientTypeNamespace(t.TypeNamespace);

                EnsureNamespace(t.TypeNamespace);
                EnsureObjectGraph(clientTypeNamespace, typeName);
                _builder.Append("if (typeof(").Append(typeName).Append(") === 'undefined') {\r\n");
                AppendClientTypeDeclaration(clientTypeNamespace, typeNameWithClientNamespace, false, false);
                // Need to use the _type id, which isn't necessarly the real name
                _builder.Append("gtc(\"");
                _builder.Append(typeID);
                _builder.Append("\");\r\n");
                _builder.Append(typeName).Append(".registerClass('").Append(typeName).Append("');\r\n}\r\n");
            }
        }

        // Create client stubs for all the enums
        private void GenerateEnumTypeProxies(IEnumerable<WebServiceEnumData> enumTypes) {
            foreach (WebServiceEnumData t in enumTypes) {
                EnsureNamespace(t.TypeNamespace);
                string typeNameWithClientNamespace = GetClientTypeNamespace(t.TypeName);
                string typeName = ServicesUtilities.GetClientTypeName(typeNameWithClientNamespace);
                string[] enumNames = t.Names;
                long[] enumValues = t.Values;
                Debug.Assert(enumNames.Length == enumValues.Length);
                EnsureObjectGraph(GetClientTypeNamespace(t.TypeNamespace), typeName);
                _builder.Append("if (typeof(").Append(typeName).Append(") === 'undefined') {\r\n");
                if (typeName.IndexOf('.') == -1) {
                    _builder.Append("var ");
                }
                _builder.Append(typeName).Append(" = function() { throw Error.invalidOperation(); }\r\n");
                _builder.Append(typeName).Append(".prototype = {");
                for (int i = 0; i < enumNames.Length; i++) {
                    if (i > 0) _builder.Append(',');
                    _builder.Append(enumNames[i]);
                    _builder.Append(": ");
                    if (t.IsULong) {
                        _builder.Append((ulong)enumValues[i]);
                    }
                    else {
                        _builder.Append(enumValues[i]);
                    }
                }
                _builder.Append("}\r\n");
                _builder.Append(typeName).Append(".registerEnum('").Append(typeName).Append('\'');
                _builder.Append(", true);\r\n}\r\n");
            }
        }

        protected virtual string GetClientTypeNamespace(string ns) {
            return ns;
        }

        private void AppendClientTypeDeclaration(string ns, string typeName, bool genClass, bool ensureNS) {
            // Register the namespace if any
            // e.g. registerNamespace('MyNS.MySubNS');
            string name = GetClientTypeNamespace(ServicesUtilities.GetClientTypeName(typeName));
            if (!String.IsNullOrEmpty(ns)) {
                if (ensureNS) EnsureNamespace(ns);
            }
            else if (!genClass) {
                // If there is no namespace, we need a var to declare the variable
                if (!name.Contains(".")) {
                    // if name contains '.', an object graph was already ensured and we dont need 'var'.
                    _builder.Append("var ");
                }
            }
            _builder.Append(name);
            if (genClass) {
                _builder.Append(".prototype");
            }
            _builder.Append('=');
            _ensuredObjectParts[name] = null;
        }

        // Normally returns MyNS.MySubNS.MyWebService OR var MyWebService, PageMethods will return PageMethods
        protected virtual string GetProxyTypeName(WebServiceData data) {
            return ServicesUtilities.GetClientTypeName(data.TypeData.TypeName);
        }

        private void EnsureNamespace(string ns) {
            //Give derived proxy generator a chance to transform namespace ( used by WCF)
            ns = GetClientTypeNamespace(ns);

            if (String.IsNullOrEmpty(ns)) return;
         
            // Don't register a given namespace more than once
            if (!_registeredNamespaces.Contains(ns)) {
                _builder.Append("Type.registerNamespace('").Append(ns).Append("');\r\n");
                _registeredNamespaces[ns] = null;
            }
        }

        private void EnsureObjectGraph(string namespacePart, string typeName) {
            // When a type name includes dots, such as 'MyNamespace.MyClass.MyNestedClass',
            // this method writes code that ensures all the parts leading up to the actual class name
            // are either already namespaces or are at least Objects.
            // namespacePart is here so we dont unnecessarily ensure the first part that contains the
            // namespace is checked for. For example, if we have NS1.NS2.NS3.TYPE, the check for
            // _registeredNamespaces will find NS1.NS2.NS3 but not NS1 and NS1.NS2, so we'd insert
            // checks that NS1 and NS1.NS2 are objects, unnecessarily.
            int startFrom = 0;
            bool first = true;
            if (!String.IsNullOrEmpty(namespacePart)) {
                int nsIndex = typeName.IndexOf(namespacePart + ".", StringComparison.Ordinal);
                // in wcf services, the typeName starts with the namespace,
                // in asmx services, it doesnt.
                if (nsIndex > -1) {
                    startFrom = nsIndex + namespacePart.Length + 1;
                    first = false;
                }
            }
            int dotIndex = typeName.IndexOf('.', startFrom);
            while (dotIndex > -1) {
                string fullPath = typeName.Substring(0, dotIndex);
                if (!_registeredNamespaces.Contains(fullPath) && !_ensuredObjectParts.Contains(fullPath)) {
                    _ensuredObjectParts[fullPath] = null;
                    _builder.Append("if (typeof(" + fullPath + ") === \"undefined\") {\r\n   ");
                    if (first) {
                        // var foo = {};
                        _builder.Append("var ");
                        first = false;
                    }
                    // foo.bar = {};
                    _builder.Append(fullPath + " = {};\r\n}\r\n");
                }
                dotIndex = typeName.IndexOf('.', dotIndex + 1);
            }
        }
    }
}
