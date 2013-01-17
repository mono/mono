//
// LogicalTypeInfo.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//   Atsushi Enomoto <atsushi@ximian.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
// Copyright (C) 2009 Novell, Inc. http://novell.com
//
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.Web.Services;
using System.Reflection;
using System.Collections;
using System.Web.Script.Serialization;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
#if NET_3_5
using System.ServiceModel;
using System.ServiceModel.Description;
#endif

namespace System.Web.Script.Services
{
	internal sealed class JsonResult
	{
		public readonly object d;
		public JsonResult (object result) {
			d = result;
		}
	}

	internal abstract class LogicalTypeInfo
	{
		public static LogicalTypeInfo CreateTypeInfo (Type t, string filePath)
		{
#if NET_3_5
			if (t.GetCustomAttributes (typeof (ServiceContractAttribute), false).Length > 0)
				return new WcfLogicalTypeInfo (t, filePath);
			else
#endif
				return new AsmxLogicalTypeInfo (t, filePath);
		}

		internal abstract class LogicalMethodInfo
		{
			readonly MethodInfo _methodInfo;
			internal readonly ParameterInfo [] _params;
			internal readonly Dictionary<string, int> _paramMap;
			LogicalTypeInfo _typeInfo;

			protected LogicalMethodInfo (LogicalTypeInfo typeInfo, MethodInfo method)
			{
				_methodInfo = method;
				_params = MethodInfo.GetParameters ();
				_typeInfo = typeInfo;

				if (HasParameters) {
					_paramMap = new Dictionary<string, int> (_params.Length, StringComparer.Ordinal);
					for (int i = 0; i < _params.Length; i++)
						_paramMap.Add(_params[i].Name, i);
				}

			}

			public abstract bool UseHttpGet { get; }
			public abstract bool EnableSession { get; }
			public abstract ResponseFormat ResponseFormat { get; }
			public abstract string MethodName { get; }
			public MethodInfo MethodInfo { get { return _methodInfo; } }
			public bool HasParameters { get { return _params != null && _params.Length > 0; } }
			public IEnumerable<Type> GetParameterTypes () {
				if (HasParameters)
					for (int i = 0; i < _params.Length; i++)
						yield return _params [i].ParameterType;

				yield return MethodInfo.ReturnType;
			}

			public void GenerateMethod (StringBuilder proxy, bool isPrototype, bool isPage) {
				string ns;
				string service;// = isPage ? "PageMethods" : MethodInfo.DeclaringType.FullName;

				_typeInfo.GetNamespaceAndServiceName (MethodInfo.DeclaringType, isPage, out ns, out service);
				string useHttpGet = UseHttpGet ? "true" : "false";
				string paramMap = GenerateParameters (true);
				string paramList = GenerateParameters (false);

				if (isPrototype){
					proxy.AppendFormat (
@"
{1}:function({4}succeededCallback, failedCallback, userContext) {{
return this._invoke({0}.get_path(), '{1}',{2},{{{3}}},succeededCallback,failedCallback,userContext); }}",
					service, MethodName, useHttpGet, paramMap, paramList);
				}
				else
					proxy.AppendFormat (
@"
{0}.{1}= function({2}onSuccess,onFailed,userContext) {{{0}._staticInstance.{1}({2}onSuccess,onFailed,userContext); }}",
					service, MethodName, paramList);
			}

			string GenerateParameters (bool isMap) {
				if (!HasParameters)
					return null;

				StringBuilder builder = new StringBuilder ();

				for (int i = 0; i < _params.Length; i++) {
					builder.AppendFormat (isMap ? "{0}:{0}" : "{0}", _params [i].Name);
					builder.Append (',');
				}

				if (isMap)
					builder.Length--;

				return builder.ToString ();
			}

			public abstract void Invoke (HttpRequest request, HttpResponse response);
		}

#if !TARGET_J2EE
		static Hashtable _type_to_logical_type = Hashtable.Synchronized (new Hashtable ());
#else
		const string type_to_logical_type_key = "System.Web.Script.Services.LogicalTypeInfo";
		static Hashtable _type_to_logical_type {
			get {
				Hashtable hash = (Hashtable) AppDomain.CurrentDomain.GetData (type_to_logical_type_key);

				if (hash != null)
					return hash;

				AppDomain.CurrentDomain.SetData (type_to_logical_type_key, Hashtable.Synchronized (new Hashtable ()));


				return (Hashtable) AppDomain.CurrentDomain.GetData (type_to_logical_type_key);
			}
		}
#endif

		static internal LogicalTypeInfo GetLogicalTypeInfo (Type t, string filePath) {
			Hashtable type_to_manager = _type_to_logical_type;
			LogicalTypeInfo tm = (LogicalTypeInfo) type_to_manager [t];

			if (tm != null)
				return tm;

			tm = CreateTypeInfo (t, filePath);
			type_to_manager [t] = tm;

			return tm;
		}

		protected static string EnsureNamespaceRegistered (string ns, string name, StringBuilder proxy, List<string> registeredNamespaces) {
			if (String.IsNullOrEmpty (ns))
				return "var " + name;

			if (!registeredNamespaces.Contains (ns)) {
				registeredNamespaces.Add (ns);
				proxy.AppendFormat (
@"
Type.registerNamespace('{0}');",
								   ns);
			}
			return name;
		}

		protected virtual void GetNamespaceAndServiceName (Type type, bool isPage, out string ns, out string service)
		{
			ns = isPage ? String.Empty : type.Namespace;
			service = isPage ? "PageMethods" : type.FullName;
		}

		// instance members

		internal readonly Type _type;
		readonly string _proxy;
		internal readonly Hashtable _methodMap;

		protected LogicalTypeInfo (Type t, string filePath)
		{
			_type = t;
			bool isPage = _type.IsSubclassOf (typeof (System.Web.UI.Page));

			var logicalMethods = GetLogicalMethods (isPage);
			//_logicalMethods = (LogicalMethodInfo []) list.ToArray (typeof (LogicalMethodInfo));

			_methodMap = new Hashtable (logicalMethods.Count);
			for (int i = 0; i < logicalMethods.Count; i++)
				_methodMap.Add (logicalMethods [i].MethodName, logicalMethods [i]);

			string ns;
			string service;
			GetNamespaceAndServiceName (t, isPage, out ns, out service);
			
			StringBuilder proxy = new StringBuilder ();
			List<string> registeredNamespaces = new List<string> ();
			string scriptTypeDeclaration = EnsureNamespaceRegistered (ns, service, proxy, registeredNamespaces);
			proxy.AppendFormat (
@"
" + scriptTypeDeclaration + @"=function() {{
{0}.initializeBase(this);
this._timeout = 0;
this._userContext = null;
this._succeeded = null;
this._failed = null;
}}
{0}.prototype={{",
			service);

			for (int i = 0; i < logicalMethods.Count; i++) {
				if (i > 0)
					proxy.Append (',');
				logicalMethods [i].GenerateMethod (proxy, true, isPage);
			}

			proxy.AppendFormat (
@"}}
{0}.registerClass('{0}',Sys.Net.WebServiceProxy);
{0}._staticInstance = new {0}();
{0}.set_path = function(value) {{ {0}._staticInstance.set_path(value); }}
{0}.get_path = function() {{ return {0}._staticInstance.get_path(); }}
{0}.set_timeout = function(value) {{ {0}._staticInstance.set_timeout(value); }}
{0}.get_timeout = function() {{ return {0}._staticInstance.get_timeout(); }}
{0}.set_defaultUserContext = function(value) {{ {0}._staticInstance.set_defaultUserContext(value); }}
{0}.get_defaultUserContext = function() {{ return {0}._staticInstance.get_defaultUserContext(); }}
{0}.set_defaultSucceededCallback = function(value) {{ {0}._staticInstance.set_defaultSucceededCallback(value); }}
{0}.get_defaultSucceededCallback = function() {{ return {0}._staticInstance.get_defaultSucceededCallback(); }}
{0}.set_defaultFailedCallback = function(value) {{ {0}._staticInstance.set_defaultFailedCallback(value); }}
{0}.get_defaultFailedCallback = function() {{ return {0}._staticInstance.get_defaultFailedCallback(); }}
{0}.set_path(""{1}"");",
			service, filePath);

			for (int i = 0; i < logicalMethods.Count; i++)
				logicalMethods [i].GenerateMethod (proxy, false, isPage);

			GenerateTypeRegistrationScript (proxy, registeredNamespaces);

			proxy.AppendLine ();
			_proxy = proxy.ToString ();
		}

		protected IEnumerable<MemberInfo> GetGenerateScriptTypes () {
			foreach (LogicalMethodInfo lmi in _methodMap.Values)
				yield return lmi.MethodInfo;

			yield return _type;
		}

		protected static void GenerateTypeRegistrationScript (StringBuilder proxy, Type scriptType, string scriptTypeId, List<string> registeredNamespaces) {
			string className = scriptType.FullName.Replace ('+', '_');
			string ns = scriptType.Namespace;
			string scriptTypeDeclaration = EnsureNamespaceRegistered (ns, className, proxy, registeredNamespaces);
			proxy.AppendFormat (
@"
if (typeof({0}) === 'undefined') {{", className);
			if (scriptType.IsEnum) {
				proxy.AppendFormat (
@"
{0} = function() {{ throw Error.invalidOperation(); }}
{0}.prototype = {1}
{0}.registerEnum('{0}', {2});",
				className,
				// This method is also used for WCF, but for enum this should work ...
				AsmxLogicalTypeInfo.JSSerializer.Serialize(GetEnumPrototypeDictionary (scriptType)),
				Attribute.GetCustomAttribute (scriptType, typeof (FlagsAttribute)) != null ? "true" : "false");
				
			}
			else {
				string typeId = String.IsNullOrEmpty (scriptTypeId) ? scriptType.FullName : scriptTypeId;
				proxy.AppendFormat (
@"
" + scriptTypeDeclaration + @"=gtc(""{1}"");
{0}.registerClass('{0}');",
				className, typeId);
			}
			proxy.Append ('}');
		}

		static IDictionary <string, object> GetEnumPrototypeDictionary (Type type)
		{
			var ret = new Dictionary <string, object> ();
			string [] names = Enum.GetNames (type);
			Array values = Enum.GetValues (type);
			for (int i = 0; i < names.Length; i++)
				ret.Add (names [i], values.GetValue (i));

			return ret;
		}

		static readonly Type typeOfIEnumerable = typeof (IEnumerable);
		static readonly Type typeOfIDictionary = typeof (IDictionary);

		protected static bool ShouldGenerateScript (Type type, bool throwIfNot) {
			if (type.IsEnum)
				return true;

			if (Type.GetTypeCode (type) != TypeCode.Object)
				return false;

			if (type == typeof (void))
				return false;

			if (typeOfIEnumerable.IsAssignableFrom (type) ||
				typeOfIDictionary.IsAssignableFrom (type) ||
				type.IsAbstract || type.IsInterface) {
				if (throwIfNot)
					ThrowOnIncorrectGenerateScriptAttribute ();
				return false;
			}

			// LAMESPEC: MS never create proxies for GenericTypes
			//&& type.GetGenericTypeDefinition ().GetGenericArguments ().Length > 1
			if (type.IsGenericType)
				return false;

			ConstructorInfo ci = type.GetConstructor (Type.EmptyTypes);
			if (ci == null || !ci.IsPublic) {
				if (throwIfNot)
					ThrowOnIncorrectGenerateScriptAttribute ();
				return false;
			}

			return true;
		}

		static void ThrowOnIncorrectGenerateScriptAttribute () {
			throw new InvalidOperationException (
				"Using the GenerateScriptTypes attribute is not supported for types in the following categories: primitive types; DateTime; generic types taking more than one parameter; types implementing IEnumerable or IDictionary; interfaces; Abstract classes; classes without a public default constructor.");
		}
		
		protected abstract void GenerateTypeRegistrationScript (StringBuilder proxy, List<string> registeredNamespaces);

		protected abstract List<LogicalMethodInfo> GetLogicalMethods (bool isPage);

		public string Proxy { get { return _proxy; } }

		public LogicalMethodInfo this [string method] {
			get { return (LogicalMethodInfo) _methodMap [method]; }
		}
	}

	internal sealed class AsmxLogicalTypeInfo : LogicalTypeInfo
	{
		#region LogicalMethodInfo

		public sealed class AsmxLogicalMethodInfo : LogicalTypeInfo.LogicalMethodInfo
		{
			readonly LogicalTypeInfo _typeInfo;

			readonly WebMethodAttribute _wma;

			readonly ScriptMethodAttribute _sma;

			readonly XmlSerializer _xmlSer;

			public AsmxLogicalMethodInfo (LogicalTypeInfo typeInfo, MethodInfo method)
				: base (typeInfo, method)
			{
				_typeInfo = typeInfo;

				_wma = (WebMethodAttribute) Attribute.GetCustomAttribute (method, typeof (WebMethodAttribute));

				_sma = (ScriptMethodAttribute) Attribute.GetCustomAttribute (method, typeof (ScriptMethodAttribute));
				if (_sma == null)
					_sma = ScriptMethodAttribute.Default;

				if (ScriptMethod.ResponseFormat == ResponseFormat.Xml
					&& MethodInfo.ReturnType != typeof (void)) {
					Type retType = MethodInfo.ReturnType;
					if (Type.GetTypeCode (retType) != TypeCode.String || ScriptMethod.XmlSerializeString)
						_xmlSer = new XmlSerializer (retType);
				}
			}

			IDictionary<string,object> BuildInvokeParameters (HttpRequest request)
			{
				return "GET".Equals (request.RequestType, StringComparison.OrdinalIgnoreCase) ?
					GetNameValueCollectionDictionary (request.QueryString) :
					(IDictionary<string, object>) JavaScriptSerializer.DefaultSerializer.DeserializeObjectInternal (new StreamReader (request.InputStream, request.ContentEncoding));
			}

			IDictionary <string, object> GetNameValueCollectionDictionary (NameValueCollection nvc)
			{
				var ret = new Dictionary <string, object> ();

				for (int i = nvc.Count - 1; i >= 0; i--)
					ret.Add (nvc.GetKey (i), JavaScriptSerializer.DefaultSerializer.DeserializeObjectInternal (nvc.Get (i)));

				return ret;
			}

			public override void Invoke (HttpRequest request, HttpResponse response) {
				var writer = response.Output;
				IDictionary<string, object> @params = BuildInvokeParameters (request);

				object [] pp = null;
				if (HasParameters) {
					Type ptype;
					int i;
					object value;
					pp = new object [_params.Length];

					foreach (KeyValuePair<string, object> pair in @params) {
						if (!_paramMap.TryGetValue (pair.Key, out i))
							continue;

						value = pair.Value;
						ptype = _params [i].ParameterType;
						if (ptype == typeof (System.Object))
							pp [i] = value;
						else
							pp [i] = AsmxLogicalTypeInfo.JSSerializer.ConvertToType (value, ptype);
					}
				}

				object target = MethodInfo.IsStatic ? null : Activator.CreateInstance (_typeInfo._type);
				object result = MethodInfo.Invoke (target, pp);
				if (_xmlSer != null) {
					XmlTextWriter xwriter = new XmlTextWriter (writer);
					xwriter.Formatting = Formatting.None;
					_xmlSer.Serialize (xwriter, result);
				}
				else
				{
					result = new JsonResult (result);
					AsmxLogicalTypeInfo.JSSerializer.Serialize (result, writer);
				}
			}

			public override string MethodName { get { return String.IsNullOrEmpty (WebMethod.MessageName) ? MethodInfo.Name : WebMethod.MessageName; } }

			public ScriptMethodAttribute ScriptMethod { get { return _sma; } }
			public WebMethodAttribute WebMethod { get { return _wma; } }
			public override bool UseHttpGet { get { return ScriptMethod.UseHttpGet; } }
			public override bool EnableSession { get { return WebMethod.EnableSession; } }
			public override ResponseFormat ResponseFormat { get { return ScriptMethod.ResponseFormat; } }
		}

		#endregion

		//readonly LogicalMethodInfo [] _logicalMethods;
		internal static readonly JavaScriptSerializer JSSerializer = new JavaScriptSerializer (null, true);

		protected override List<LogicalMethodInfo> GetLogicalMethods (bool isPage)
		{
			BindingFlags bindingAttr = isPage ? (BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public) : (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MethodInfo [] all_type_methods = _type.GetMethods (bindingAttr);
			List<LogicalMethodInfo> logicalMethods = new List<LogicalMethodInfo> (all_type_methods.Length);
			foreach (MethodInfo mi in all_type_methods) {
				if (mi.IsPublic && 
					mi.GetCustomAttributes (typeof (WebMethodAttribute), false).Length > 0)
					logicalMethods.Add (new AsmxLogicalMethodInfo (this, mi));
				else {
					foreach (Type ifaceType in _type.GetInterfaces ()) {
						if (ifaceType.GetCustomAttributes (typeof (WebServiceBindingAttribute), false).Length > 0) {
							MethodInfo found = FindInInterface (ifaceType, mi);
							if (found != null) {
								if (found.GetCustomAttributes (typeof (WebMethodAttribute), false).Length > 0)
									logicalMethods.Add (new AsmxLogicalMethodInfo (this, found));

								break;
							}
						}
					}
				}
			}
			return logicalMethods;
		}

		internal AsmxLogicalTypeInfo (Type t, string filePath) 
			: base (t, filePath)
		{
		}

		IEnumerable<GenerateScriptTypeAttribute> GetGenerateScriptTypeAttributes () {
			Hashtable generatedTypes = new Hashtable ();

			foreach (MemberInfo mi in GetGenerateScriptTypes ()) {
				GenerateScriptTypeAttribute [] gstas = (GenerateScriptTypeAttribute []) mi.GetCustomAttributes (typeof (GenerateScriptTypeAttribute), true);
				if (gstas == null || gstas.Length == 0)
					continue;

				for (int i = 0; i < gstas.Length; i++) {
					if (!generatedTypes.Contains (gstas [i].Type)) {
						if (ShouldGenerateScript (gstas [i].Type, true)) {
							generatedTypes [gstas [i].Type] = gstas [i].Type;
							yield return gstas [i];
						}
					}
				}
			}

			foreach (LogicalMethodInfo lmi in _methodMap.Values) {
				foreach (Type t in lmi.GetParameterTypes ()) {
					Type param = GetTypeToGenerate (t);
					if (!generatedTypes.Contains (param)) {
						if (ShouldGenerateScript (param, false)) {
							generatedTypes [param] = param;
							yield return new GenerateScriptTypeAttribute (param);
						}
					}
				}
			}
		}

		static Type GetTypeToGenerate (Type type) {
			if (type.IsArray)
				return type.GetElementType ();
			if (type.IsGenericType) {
				while (type.IsGenericType && type.GetGenericArguments ().Length == 1)
					type = type.GetGenericArguments () [0];
				return type;
			}
			return type;
		}

		static MethodInfo FindInInterface (Type ifaceType, MethodInfo method) {
			int nameStartIndex = 0;
			if (method.IsPrivate) {
				nameStartIndex = method.Name.LastIndexOf ('.');
				if (nameStartIndex < 0)
					nameStartIndex = 0;
				else {
					if (String.CompareOrdinal (
						ifaceType.FullName.Replace ('+', '.'), 0, method.Name, 0, nameStartIndex) != 0)
						return null;

					nameStartIndex++;
				}
			}
			foreach (MethodInfo mi in ifaceType.GetMembers ()) {
				if (method.ReturnType == mi.ReturnType &&
					String.CompareOrdinal (method.Name, nameStartIndex, mi.Name, 0, mi.Name.Length) == 0) {
					ParameterInfo [] rpi = method.GetParameters ();
					ParameterInfo [] lpi = mi.GetParameters ();
					if (rpi.Length == lpi.Length) {
						bool match = true;
						for (int i = 0; i < rpi.Length; i++) {
							if (rpi [i].ParameterType != lpi [i].ParameterType) {
								match = false;
								break;
							}
						}

						if (match)
							return mi;
					}
				}
			}

			return null;
		}

		protected override void GenerateTypeRegistrationScript (StringBuilder proxy, List<string> registeredNamespaces)
		{
			bool gtc = false;

			foreach (GenerateScriptTypeAttribute gsta in GetGenerateScriptTypeAttributes ()) {
				if (!gtc && !gsta.Type.IsEnum) {
					proxy.Append (
@"
var gtc = Sys.Net.WebServiceProxy._generateTypedConstructor;");
					gtc = true;
				}
				GenerateTypeRegistrationScript (proxy, gsta.Type, gsta.ScriptTypeId, registeredNamespaces);
			}
		}
	}

#if NET_3_5
	internal class WcfLogicalTypeInfo : LogicalTypeInfo
	{
		ContractDescription cd;

		public WcfLogicalTypeInfo (Type type, string filePath)
			: base (type, filePath)
		{
		}

		ContractDescription Contract {
			get {
				if (cd == null)
					cd = ContractDescription.GetContract (_type);
				return cd;
			}
		}

		IEnumerable<KeyValuePair<Type,string>> GetDataContractTypeInfos ()
		{
			foreach (var od in Contract.Operations) {
				foreach (var md in od.Messages) {
					foreach (var pd in md.Body.Parts) {
						if (ShouldGenerateScript (pd.Type, false))
							yield return new KeyValuePair<Type,string> (pd.Type, null);
					}
					if (md.Body.ReturnValue != null && ShouldGenerateScript (md.Body.ReturnValue.Type, false))
						yield return new KeyValuePair<Type,string> (md.Body.ReturnValue.Type, null);
				}
			}
			yield break;
		}

		protected override void GetNamespaceAndServiceName (Type type, bool isPage, out string ns, out string service)
		{
			string name = type.Namespace;
			int dot = name.LastIndexOf ('.');
			if (dot > -1)
				name = name.Substring (dot + 1);
			ns = name;
			service = name + "." + type.Name;
		}

		protected override void GenerateTypeRegistrationScript (StringBuilder proxy, List<string> registeredNamespaces)
		{
			bool gtc = false;

			foreach (KeyValuePair<Type,string> pair in GetDataContractTypeInfos ()) {
				if (!gtc && !pair.Key.IsEnum) {
					proxy.Append (
@"
var gtc = Sys.Net.WebServiceProxy._generateTypedConstructor;");
					gtc = true;
				}
				GenerateTypeRegistrationScript (proxy, pair.Key, pair.Value, registeredNamespaces);
			}
		}

		protected override List<LogicalMethodInfo> GetLogicalMethods (bool isPage)
		{
			if (isPage)
				throw new NotSupportedException ();

			var l = new List<LogicalMethodInfo> ();
			foreach (var od in Contract.Operations)
				l.Add (new WcfLogicalMethodInfo (this, od));
			return l;
		}

		internal class WcfLogicalMethodInfo : LogicalMethodInfo
		{
			OperationDescription od;

			public WcfLogicalMethodInfo (LogicalTypeInfo typeInfo, OperationDescription od)
				: base (typeInfo, od.SyncMethod)
			{
				this.od = od;
			}

			public override bool UseHttpGet { get { return true; } } // always

			// FIXME: could this be enabled?
			public override bool EnableSession {
				get { return false; }
			}

			public override ResponseFormat ResponseFormat {
				get { return ResponseFormat.Json; } // always
			}

			public override string MethodName {
				get { return od.Name; }
			}

			public override void Invoke (HttpRequest request, HttpResponse response)
			{
				// invocation is done in WCF part.
				throw new NotSupportedException ();
			}
		}
	}
#endif
}
