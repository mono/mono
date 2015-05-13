//
// AssemblyRef
//
// Author:
//	Bruno Lauze     (brunolauze@msn.com)
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2015 Microsoft (http://www.microsoft.com)
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
using System.Linq;

namespace System.Management
{
	internal class UnixWbemClass : IUnixWbemClassHandler
	{
		private Dictionary<string, UnixCimMethodInfo> _methods;
		private readonly Dictionary<string, object> _systemProps;
		private readonly Dictionary<string, UnixWbemPropertyInfo> _systemPropInfos;
		private readonly Dictionary<string, UnixWbemQualiferInfo> _qualifiers;
		private IEnumerator<UnixCimMethodInfo> _methodEnumerator;
		private bool _isInstance = false;
		private System.Management.Internal.CimInstance nativeInstance;
		private System.Management.Internal.CimInstancePath nativeInstancePath;

		public UnixWbemClass (System.Management.Internal.CimInstancePath nativeInstancePath)
		{
			_isInstance = true;
			this.nativeInstancePath = nativeInstancePath;
			this.nativeInstance = nativeInstancePath.Instance;
			_systemProps = new Dictionary<string, object> ();
			_systemPropInfos = new Dictionary<string, UnixWbemPropertyInfo> ();
			RegisterSystemProperties ();
			AddSystemProperties ();
		}

		internal bool IsMetaImplementation {
			get;
			set;
		}


		public IUnixWbemClassHandler New()
		{
			return new UnixWbemClass (null);
		}

		/// <summary>
		/// Get this instance.
		/// </summary>
		public IEnumerable<object> Get(string strQuery)
		{
			return WbemClientFactory.Get ("root/cimv2", strQuery);
		}

		/// <summary>
		/// Get the specified nativeObj.
		/// </summary>
		/// <param name='nativeObj'>
		/// Native object.
		/// </param>
		public object Get(object nativeObj)
		{
			return nativeObj;
		}

		/// <summary>
		/// Adds the property.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='obj'>
		/// Object.
		/// </param>
		public void AddProperty (string key, object obj)
		{

		}

		/// <summary>
		/// Gets the property.
		/// </summary>
		/// <returns>
		/// The property.
		/// </returns>
		/// <param name='key'>
		/// Key.
		/// </param>
		public object GetProperty(string key)
		{
			if (_systemPropInfos.ContainsKey (key)) {
				return null;
			}
			if (nativeInstance.Properties.Contains (new System.Management.Internal.CimName (key))) {
				return nativeInstance.Properties [key].Value;
			}
			return null;
		}

		/// <summary>
		/// Invokes the method.
		/// </summary>
		/// <returns>
		/// The method.
		/// </returns>
		/// <param name='obj'>
		/// Object.
		/// </param>
		public IUnixWbemClassHandler InvokeMethod(string methodName, IUnixWbemClassHandler obj)
		{

			return this;
		}

		public IUnixWbemClassHandler WithProperty(string key, object obj)
		{
			//
			return this;
		}

		public IUnixWbemClassHandler WithMethod(string key, UnixCimMethodInfo methodInfo)
		{
			this._methods.Add (key, methodInfo);
			return this;
		}

		/// <summary>
		/// Adds the method.
		/// </summary>
		/// <param name='key'>
		/// Key.
		/// </param>
		/// <param name='method'>
		/// Method.
		/// </param>
		public void AddMethod (string key, UnixCimMethodInfo method)
		{

		}

		internal IUnixWbemClassHandler Format (IUnixWbemClassHandler obj)
		{
			return obj.WithProperty ("__RELATIVE_PATH", string.Format ("{0}/{1}={2}", obj.Properties ["__CLASS"], PathField, FormatPropertyValue(obj.Properties [PathField])))
				.WithProperty ("__PATH",string.Format ("//{0}/{1}/{2}/{3}={4}", obj.Properties ["__SERVER"], obj.Properties ["__NAMESPACE"], obj.Properties ["__CLASS"], PathField, FormatPropertyValue(obj.Properties [PathField])))
				.WithProperty ("__PROPERTY_COUNT", obj.Properties.Count + 1);
		}

		private static string FormatPropertyValue(object obj)
		{
			if (obj is String)
			{
				if (obj == null)
				{
					return "\"\"";
				}
				return "\"" + obj.ToString () + "\"";
			}
			if (obj == null) return "";
			return obj.ToString ();
		}
			

		private void RegisterSystemProperties ()
		{
			RegisterSystemProperty ("__GENUS", CimType.SInt32, 0);
			RegisterSystemProperty ("__UID", CimType.Object, 0);
			RegisterSystemProperty ("__SERVER", CimType.String, 0);
			RegisterSystemProperty ("__NAMESPACE", CimType.String, 0);
			RegisterSystemProperty ("__CLASS", CimType.String, 0);
			RegisterSystemProperty ("__DYNASTY", CimType.String, 0);
			RegisterSystemProperty ("__DERIVATION", CimType.Object, 0);
			RegisterSystemProperty ("__RELATIVE_PATH", CimType.String, 0);
			RegisterSystemProperty ("__PATH", CimType.String, 0);
			RegisterSystemProperty ("__KEY_FIELD", CimType.String, 0);
		}

		private void AddSystemProperties()
		{
			this._systemProps.Add ("__GENUS", _isInstance ? 0 : 1);
			this._systemProps.Add ("__UID", GetUid ());
			this._systemProps.Add ("__SERVER", nativeInstancePath.Host.ToString().ToLower());
			this._systemProps.Add ("__NAMESPACE", nativeInstancePath.Namespace.ToString());
			this._systemProps.Add ("__DYNASTY", GetDynasty());
			this._systemProps.Add ("__CLASS", nativeInstance.ClassName);
			this._systemProps.Add ("__DERIVATION", GetDerivations());
			this._systemProps.Add ("__PATH", string.Format ("//{0}/{1}/{2}", this._systemProps["__SERVER"],this._systemProps["__NAMESPACE"],this._systemProps["__CLASS"])); 
			this._systemProps.Add ("__KEY_FIELD", PathField);
		}

		object GetDynasty ()
		{
			return "";
		}

		private Guid GetUid ()
		{
			return Guid.Empty;
		}

		protected virtual void RegisterSystemProperty(string name, CimType type, int flavor)
		{	
			if (_systemPropInfos.ContainsKey (name))
				_systemPropInfos[name] = new UnixWbemPropertyInfo { Name = name, Type = type, Flavor = flavor};
			else 
				_systemPropInfos.Add (name, new UnixWbemPropertyInfo { Name = name, Type = type, Flavor = flavor});
		}

		public virtual IDictionary<string, object> Properties 
		{ 
			get 
			{ 
				Dictionary<string, object> ret = new Dictionary<string, object> ();
				foreach (System.Management.Internal.CimProperty p in nativeInstance.Properties) {
					ret.Add (p.Name.ToString(), p.Value);
				}
				return ret;
			} 
		}

		public virtual IEnumerable<string> PropertyNames { get { return nativeInstance.Properties.Select(x => x.Name.ToString()); } }

		public virtual IEnumerable<UnixWbemPropertyInfo> PropertyInfos { 
			get 
			{ 
				return nativeInstance.Properties.Select(x => new UnixWbemPropertyInfo {
					Name = x.Name.ToString(),
					Type = GetCimType(x.Type),
					Flavor = x.IsPropagated.ToBoolOrDefault() ? (int)tag_WBEM_FLAVOR_TYPE.WBEM_FLAVOR_ORIGIN_PROPAGATED : 0
				});
			} 
		}

		private CimType GetCimType (System.Management.Internal.NullableCimType type)
		{
			CimType ret;
			if (Enum.TryParse<CimType>(type.ToString (), true, out ret)) {
				return ret;	
			}
			return CimType.None;
		}

		public virtual IEnumerable<UnixWbemPropertyInfo> SystemPropertyInfos { get { return _systemPropInfos.Values; } }

		public virtual IEnumerable<string> SystemPropertyNames { get { return _systemPropInfos.Keys; } }

		public virtual IEnumerable<string> QualifierNames { get { return null; } }

		public IEnumerable<string> MethodNames { get { return _methods.Keys; } }

		public IEnumerable<UnixCimMethodInfo> Methods { get { return _methods.Values; } }

		public UnixCimMethodInfo NextMethod()
		{
			return default(UnixCimMethodInfo);
		}


		public UnixWbemQualiferInfo GetQualifier(string name)
		{
			var obj = _qualifiers[name];
			return null; // ShouldSendQualifier (obj) ? obj : null;
		}

		public UnixWbemQualiferInfo GetQualifier (int index)
		{
			return null;
		}

		private string[] GetDerivations ()
		{
			return new string[0];
			/*
			var list = new List<string> ();
			Type type = IsMetaImplementation ? this.GetType ().BaseType : this.GetType ();
			if (type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(CIMWbemClassBase)) {

				do {
					list.Add (type.BaseType.Name);
					type = type.BaseType;
				} while(type.BaseType != null && type.BaseType != typeof(object) && type.BaseType != typeof(CIMWbemClassBase));
			}
			return list.ToArray ();
			**/
		}

		public string PathField { get { return null; } }
	}
}