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
using System.ServiceModel;
using System.ServiceModel.Description;

namespace System.Web.Script.Services
{
	internal abstract class LogicalTypeInfo
	{
		public static LogicalTypeInfo CreateTypeInfo (Type t, string filePath)
		{
			if (t.GetCustomAttributes (typeof (ServiceContractAttribute), false).Length > 0)
				return new WcfLogicalTypeInfo (t, filePath);
			else
				throw new NotSupportedException ("The type " + t.Name + " is not supported");
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

		static Hashtable _type_to_logical_type = Hashtable.Synchronized (new Hashtable ());

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
			bool isPage = false;

			var logicalMethods = GetLogicalMethods (isPage);
			
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
				SerializationHelper.JSSerializer.Serialize(GetEnumPrototypeDictionary (scriptType)),
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

	internal sealed class SerializationHelper 
	{		
		internal static readonly JavaScriptSerializer JSSerializer = new MonoJavaScriptSerializer (null, true);
	}
}
