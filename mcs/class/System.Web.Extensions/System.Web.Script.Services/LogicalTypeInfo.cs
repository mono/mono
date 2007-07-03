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

namespace System.Web.Script.Services
{
	internal sealed class LogicalTypeInfo
	{
		#region LogicalMethodInfo

		sealed class LogicalMethodInfo
		{
			readonly LogicalTypeInfo _typeInfo;
			readonly MethodInfo _methodInfo;

			readonly string _methodName;

			readonly ScriptMethodAttribute _sma;

			readonly ParameterInfo [] _params;
			readonly Dictionary<string, int> _paramMap;
			readonly XmlSerializer _xmlSer;

			public LogicalMethodInfo (LogicalTypeInfo typeInfo, MethodInfo method) {
				_typeInfo = typeInfo;
				_methodInfo = method;

				WebMethodAttribute wma = (WebMethodAttribute) Attribute.GetCustomAttribute (method, typeof (WebMethodAttribute));
				_methodName = !String.IsNullOrEmpty (wma.MessageName) ? wma.MessageName : method.Name;

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
					&& MethodInfo.ReturnType != typeof(void))
					_xmlSer = new XmlSerializer (MethodInfo.ReturnType);
			}

			public void Invoke (IDictionary<string, object> @params, TextWriter writer) {
				object [] pp = null;
				if (HasParameters) {
					pp = new object [_params.Length];

					foreach (KeyValuePair<string, object> pair in @params) {
						int i = _paramMap [pair.Key];
						pp [i] = LogicalTypeInfo.JSSerializer.ConvertToType (_params [i].ParameterType, pair.Value);
					}
				}

				object target = MethodInfo.IsStatic ? null : Activator.CreateInstance (_typeInfo._type);
				object result = MethodInfo.Invoke (target, pp);
				if (_xmlSer != null)
					_xmlSer.Serialize (writer, result);
				else
					LogicalTypeInfo.JSSerializer.Serialize (result, writer);
			}

			bool HasParameters { get { return _params != null && _params.Length > 0; } }
			public string MethodName { get { return _methodName; } }

			public ScriptMethodAttribute ScriptMethod { get { return _sma; } }
			public MethodInfo MethodInfo { get { return _methodInfo; } }

			public void GenerateMethod (StringBuilder proxy, bool isPrototype) {
				string service = MethodInfo.DeclaringType.FullName;

				string useHttpGet = ScriptMethod.UseHttpGet ? "true" : "false";
				string paramMap = GenerateParameters (true);
				string paramList = GenerateParameters (false);

				if (isPrototype)
					proxy.AppendFormat (
@"
{1}:function({4}succeededCallback, failedCallback, userContext) {{
return this._invoke({0}.get_path(), '{1}',{2},{{{3}}},succeededCallback,failedCallback,userContext); }}",
					service, MethodName, useHttpGet, paramMap, paramList);

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
		static readonly JavaScriptSerializer JSSerializer = new JavaScriptSerializer ();

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
			if (String.IsNullOrEmpty (ns))
				proxy.AppendFormat (
@"var {0}",
	service);
			else
				proxy.AppendFormat (
@"Type.registerNamespace('{0}');
{1}",
	ns, service);
			proxy.AppendFormat (
@"=function() {{
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
				logicalMethods [i].GenerateMethod (proxy, true);
			}

			proxy.AppendFormat (
@"}}
{0}.registerClass('{0}',Sys.Net.WebServiceProxy);
{0}._staticInstance = new {0}();
{0}.set_path = function(value) {{ {0}._staticInstance._path = value; }}
{0}.get_path = function() {{ return {0}._staticInstance._path; }}
{0}.set_timeout = function(value) {{ {0}._staticInstance._timeout = value; }}
{0}.get_timeout = function() {{ return {0}._staticInstance._timeout; }}
{0}.set_defaultUserContext = function(value) {{ {0}._staticInstance._userContext = value; }}
{0}.get_defaultUserContext = function() {{ return {0}._staticInstance._userContext; }}
{0}.set_defaultSucceededCallback = function(value) {{ {0}._staticInstance._succeeded = value; }}
{0}.get_defaultSucceededCallback = function() {{ return {0}._staticInstance._succeeded; }}
{0}.set_defaultFailedCallback = function(value) {{ {0}._staticInstance._failed = value; }}
{0}.get_defaultFailedCallback = function() {{ return {0}._staticInstance._failed; }}
{0}.set_path(""{1}"");",
			service, filePath);

			for (int i = 0; i < logicalMethods.Count; i++)
				logicalMethods[i].GenerateMethod (proxy, false);

			bool gtc = false;

			foreach (MemberInfo gstmi in GetGenerateScriptTypeMembers()) {
				GenerateScriptTypeAttribute [] gstas = (GenerateScriptTypeAttribute []) gstmi.GetCustomAttributes (typeof (GenerateScriptTypeAttribute), true);
				if (gstas == null || gstas.Length == 0)
					continue;

				for (int j = 0; j < gstas.Length; j++) {
					if (!gtc && !gstas [j].Type.IsEnum) {
						proxy.Append (@"var gtc = Sys.Net.WebServiceProxy._generateTypedConstructor;");
						gtc = true;
					}
					GenerateScript (proxy, gstas [j]);
				}
			}

			_proxy = proxy.ToString ();
		}

		IEnumerable<MemberInfo> GetGenerateScriptTypeMembers () {
			foreach (LogicalMethodInfo lmi in _methodMap.Values)
				yield return lmi.MethodInfo;

			yield return _type;
		}

		static void GenerateScript (StringBuilder proxy, GenerateScriptTypeAttribute gsta) {
			proxy.AppendFormat (
@"
if (typeof({0}) === 'undefined') {{", gsta.Type.FullName);
			if (gsta.Type.IsEnum) {
				proxy.AppendFormat (
@"
{0} = function() {{ throw Error.invalidOperation(); }}
{0}.prototype = {1}
{0}.registerEnum('{0}', {2});",
				gsta.Type.FullName,
				JSSerializer.Serialize(new EnumPrototypeSerializer(gsta.Type)),
				Attribute.GetCustomAttribute (gsta.Type, typeof (FlagsAttribute)) != null ? "true" : "false");
				
			}
			else {
				string typeId = String.IsNullOrEmpty (gsta.ScriptTypeId) ? gsta.Type.FullName : gsta.ScriptTypeId;
				proxy.AppendFormat (
@"
{0}=gtc(""{1}"");
{0}.registerClass('{0}');",
				gsta.Type.FullName, typeId);
			}
			proxy.Append ('}');
		}

		sealed class EnumPrototypeSerializer : JavaScriptSerializer.LazyDictionary
		{
			readonly Type _type;
			public EnumPrototypeSerializer (Type type) {
				_type = type;
			}
			protected override IEnumerator<KeyValuePair<string, object>> GetEnumerator () {
				String [] names = Enum.GetNames (_type);
				Array values = Enum.GetValues (_type);
				for (int i = 0; i < names.Length; i++)
					yield return new KeyValuePair<string, object> (names [i], values.GetValue (i));
			}
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
		public void Invoke (string method, IDictionary<string, object> @params, TextWriter writer) {
			((LogicalMethodInfo) _methodMap [method]).Invoke (@params, writer);
		}

		public void Invoke (string method, TextReader reader, TextWriter writer) {
			Invoke (method, (IDictionary<string, object>) JSSerializer.DeserializeObject (reader), writer);
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
	}
}
