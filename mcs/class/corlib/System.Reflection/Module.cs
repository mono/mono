//
// System.Reflection/Module.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace System.Reflection {
	public class Module : ISerializable, ICustomAttributeProvider {

		public static readonly TypeFilter FilterTypeName;
		public static readonly TypeFilter FilterTypeNameIgnoreCase;

		private IntPtr _impl; /* a pointer to a MonoImage */
		internal Assembly assembly;
		internal string fqname;
		internal string name;
		internal string scopename;

		public Assembly Assembly {get {return assembly;}}
		public virtual string FullyQualifiedName {get {return fqname;}}
		public string Name {get {return name;}}
		public string ScopeName {get {return scopename;}}


		public virtual Type[] FindTypes(TypeFilter filter, object filterCriteria) {
			return null;
		}
		public virtual object[] GetCustomAttributes(bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}
		public virtual object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}
		public FieldInfo GetField(string name) {
			return null;
		}
		public FieldInfo GetField(string name, BindingFlags flags) {
			return null;
		}
		public FieldInfo[] GetFields() {
			return null;
		}

		public MethodInfo GetMethod(string name) {
			return null;
		}
		public MethodInfo GetMethod(string name, Type[] types) {
			return null;
		}
		public MethodInfo GetMethod( string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			return null;
		}
		protected virtual MethodInfo GetMethodImpl( string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers) {
			return null;
		}
		public MethodInfo[] GetMethods() {
			return null;
		}
		public virtual void GetObjectData( SerializationInfo info, StreamingContext context) {}
		public X509Certificate GetSignerCertificate() {
			return null;
		}
		public virtual Type GetType(string className) {
			return null;
		}
		public virtual Type GetType(string className, bool ignoreCase) {
			return null;
		}
		public virtual Type GetType(string className, bool throwOnError, bool ignoreCase) {
			return null;
		}
		public virtual Type[] GetTypes() {
			return null;
		}
		public virtual bool IsDefined( Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}
		public bool IsResource() {
			return false;
		}
		public override string ToString() {
			return "Reflection.Module: " + name;
		}






	}

}
