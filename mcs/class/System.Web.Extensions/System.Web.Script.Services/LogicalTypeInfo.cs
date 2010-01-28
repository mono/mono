//
// ScriptHandlerFactory.cs
//
// Author:
//   Konstantin Triger <kostat@mainsoft.com>
//
// (C) 2007 Mainsoft, Inc.  http://www.mainsoft.com
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
using System.Text;
using System.Web.Services;
using System.Reflection;
using System.Collections;
using System.Web.Script.Serialization;
using System.IO;
using System.Xml.Serialization;
using System.Xml;

namespace System.Web.Script.Services
{
	internal sealed class LogicalTypeInfo
	{
		#region LogicalMethodInfo

		public sealed class LogicalMethodInfo
		{
			readonly LogicalTypeInfo _typeInfo;
			readonly MethodInfo _methodInfo;

			readonly WebMethodAttribute _wma;

			readonly ScriptMethodAttribute _sma;

			readonly ParameterInfo [] _params;
			readonly Dictionary<string, int> _paramMap;
			readonly XmlSerializer _xmlSer;

			public LogicalMethodInfo (LogicalTypeInfo typeInfo, MethodInfo method) {
				_typeInfo = typeInfo;
				_methodInfo = method;

				_wma = (WebMethodAttribute) Attribute.GetCustomAttribute (method, typeof (WebMethodAttribute));

				_sma = (ScriptMethodAttribute) Attribute.GetCustomAttribute (method, typeof (ScriptMethodAttribute));
				if (_sma == null)
					_sma = ScriptMethodAttribute.Default;

				_params = MethodInfo.GetParameters ();

				if (HasParameters) {
					_paramMap = new Dictionary<string, int> (_params.Length, StringComparer.Ordinal);
					for (int i = 0; i < _params.Length; i++)
						_paramMap.Add(_params[i].Name, i);
				}

				if (ScriptMethod.ResponseFormat == ResponseFormat.Xml
					&& MethodInfo.ReturnType != typeof (void)) {
					Type retType = MethodInfo.ReturnType;
					if (Type.GetTypeCode (retType) != TypeCode.String || ScriptMethod.XmlSerializeString)
						_xmlSer = new XmlSerializer (retType);
				}
			}

			public void Invoke (IDictionary<string, object> @params, TextWriter writer) {
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
							pp [i] = LogicalTypeInfo.JSSerializer.ConvertToType (ptype, value);
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
					LogicalTypeInfo.JSSerializer.Serialize (result, writer);
				}
			}

			bool HasParameters { get { return _params != null && _params.Length > 0; } }
			public string MethodName { get { return String.IsNullOrEmpty (WebMethod.MessageName) ? MethodInfo.Name : WebMethod.MessageName; } }

			public ScriptMethodAttribute ScriptMethod { get { return _sma; } }
			public MethodInfo MethodInfo { get { return _methodInfo; } }
			public WebMethodAttribute WebMethod { get { return _wma; } }
			public IEnumerable<Type> GetParameterTypes () {
				if (HasParameters)
					for (int i = 0; i < _params.Length; i++)
						yield return _params [i].ParameterType;

				yield return MethodInfo.ReturnType;
			}

			public void GenerateMethod (StringBuilder proxy, bool isPrototype, bool isPage) {
				string service = isPage ? "PageMethods" : MethodInfo.DeclaringType.FullName;

				string useHttpGet = ScriptMethod.UseHttpGet ? "true" : "false";
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
		}

		#endregion

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

		//readonly LogicalMethodInfo [] _logicalMethods;
		readonly Hashtable _methodMap;
		readonly Type _type;
		readonly string _proxy;
		static readonly JavaScriptSerializer JSSerializer = new JavaScriptSerializer (null, true);

		private LogicalTypeInfo (Type t, string filePath) {
			_type = t;
			bool isPage = _type.IsSubclassOf (typeof (System.Web.UI.Page));
			BindingFlags bindingAttr = isPage ? (BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public) : (BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			MethodInfo [] all_type_methods = _type.GetMethods (bindingAttr);
			List<LogicalMethodInfo> logicalMethods = new List<LogicalMethodInfo> (all_type_methods.Length);
			foreach (MethodInfo mi in all_type_methods) {
				if (mi.IsPublic && 
					mi.GetCustomAttributes (typeof (WebMethodAttribute), false).Length > 0)
					logicalMethods.Add (new LogicalMethodInfo (this, mi));
				else {
					foreach (Type ifaceType in _type.GetInterfaces ()) {
						if (ifaceType.GetCustomAttributes (typeof (WebServiceBindingAttribute), false).Length > 0) {
							MethodInfo found = FindInInterface (ifaceType, mi);
							if (found != null) {
								if (found.GetCustomAttributes (typeof (WebMethodAttribute), false).Length > 0)
									logicalMethods.Add (new LogicalMethodInfo (this, found));

								break;
							}
						}
					}
				}
			}

			//_logicalMethods = (LogicalMethodInfo []) list.ToArray (typeof (LogicalMethodInfo));

			_methodMap = new Hashtable (logicalMethods.Count);
			for (int i = 0; i < logicalMethods.Count; i++)
				_methodMap.Add (logicalMethods [i].MethodName, logicalMethods [i]);

			string ns = isPage ? String.Empty : t.Namespace;
			string service = isPage ? "PageMethods" : t.FullName;
			
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

			bool gtc = false;

			foreach (GenerateScriptTypeAttribute gsta in GetGenerateScriptTypeAttributes ()) {
				if (!gtc && !gsta.Type.IsEnum) {
					proxy.Append (
@"
var gtc = Sys.Net.WebServiceProxy._generateTypedConstructor;");
					gtc = true;
				}
				GenerateScript (proxy, gsta, registeredNamespaces);
			}

			proxy.AppendLine ();
			_proxy = proxy.ToString ();
		}

		IEnumerable<MemberInfo> GetGenerateScriptTypes () {
			foreach (LogicalMethodInfo lmi in _methodMap.Values)
				yield return lmi.MethodInfo;

			yield return _type;
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

		static readonly Type typeOfIEnumerable = typeof (IEnumerable);
		static readonly Type typeOfIDictionary = typeof (IDictionary);

		static bool ShouldGenerateScript (Type type, bool throwIfNot) {
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

		static void GenerateScript (StringBuilder proxy, GenerateScriptTypeAttribute gsta, List<string> registeredNamespaces) {
			string className = gsta.Type.FullName.Replace ('+', '_');
			string ns = gsta.Type.Namespace;
			string scriptTypeDeclaration = EnsureNamespaceRegistered (ns, className, proxy, registeredNamespaces);
			proxy.AppendFormat (
@"
if (typeof({0}) === 'undefined') {{", className);
			if (gsta.Type.IsEnum) {
				proxy.AppendFormat (
@"
{0} = function() {{ throw Error.invalidOperation(); }}
{0}.prototype = {1}
{0}.registerEnum('{0}', {2});",
				className,
				JSSerializer.Serialize(GetEnumPrototypeDictionary (gsta.Type)),
				Attribute.GetCustomAttribute (gsta.Type, typeof (FlagsAttribute)) != null ? "true" : "false");
				
			}
			else {
				string typeId = String.IsNullOrEmpty (gsta.ScriptTypeId) ? gsta.Type.FullName : gsta.ScriptTypeId;
				proxy.AppendFormat (
@"
" + scriptTypeDeclaration + @"=gtc(""{1}"");
{0}.registerClass('{0}');",
				className, typeId);
			}
			proxy.Append ('}');
		}

		static string EnsureNamespaceRegistered (string ns, string name, StringBuilder proxy, List<string> registeredNamespaces) {
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

		static IDictionary <string, object> GetEnumPrototypeDictionary (Type type)
		{
			var ret = new Dictionary <string, object> ();
			string [] names = Enum.GetNames (type);
			Array values = Enum.GetValues (type);
			for (int i = 0; i < names.Length; i++)
				ret.Add (names [i], values.GetValue (i));

			return ret;
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

		public string Proxy { get { return _proxy; } }

		public LogicalMethodInfo this [string method] {
			get { return (LogicalMethodInfo) _methodMap [method]; }
		}

		static internal LogicalTypeInfo GetLogicalTypeInfo (Type t, string filePath) {
			Hashtable type_to_manager = _type_to_logical_type;
			LogicalTypeInfo tm = (LogicalTypeInfo) type_to_manager [t];

			if (tm != null)
				return tm;

			tm = new LogicalTypeInfo (t, filePath);
			type_to_manager [t] = tm;

			return tm;
		}
		
		sealed class JsonResult
		{
			public readonly object d;
			public JsonResult (object result) {
				d = result;
			}
		}
	}
}
