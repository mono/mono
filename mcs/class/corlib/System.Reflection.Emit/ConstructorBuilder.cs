//
// System.Reflection.Emit/ConstructorBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Security;
using System.Security.Permissions;

namespace System.Reflection.Emit {
	public sealed class ConstructorBuilder : ConstructorInfo {
		private ILGenerator ilgen;

		public override MethodImplAttributes GetMethodImplementationFlags() {
			return (MethodImplAttributes)0;
		}
		public override ParameterInfo[] GetParameters() {
			return null;
		}
		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			return null;
		}
		public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
			return null;
		}

		public override RuntimeMethodHandle MethodHandle { get {return new RuntimeMethodHandle ();} }
		public override MethodAttributes Attributes { get {return (MethodAttributes)0;} }
		public override Type ReflectedType { get {return null;}}
		public override Type DeclaringType { get {return null;}}
		public Type ReturnType { get {return null;}}
		public override string Name { get {return ".ctor";}}
		public string Signature {
			get {return "constructor signature";}
		}
		
		public bool InitLocals { /* FIXME */
			get {return false;} 
			set {return;}
		}

		public void AddDeclarativeSecurity( SecurityAction action, PermissionSet pset) {
		}

		public ParameterBuilder DefineParameter(int iSequence, ParameterAttributes attributes, string strParamName) {
			return null;
		}
		

		public override bool IsDefined (Type attribute_type, bool inherit) {return false;}

		public override object [] GetCustomAttributes (bool inherit) {return null;}

		public override object [] GetCustomAttributes (Type attribute_type, bool inherit) {return null;}

		public ILGenerator GetILGenerator () {
			return GetILGenerator (256);
		}
		public ILGenerator GetILGenerator (int size) {
			//ilgen = new ILGenerator (this, size);
			return null;
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
		}
		public void SetImplementationFlags( MethodImplAttributes attributes) {
		}
		public Module GetModule() {
			return null;
		}
		public MethodToken GetToken() {
			return new MethodToken();
		}
		public void SetSymCustomAttribute( string name, byte[] data) {
		}
		public override string ToString() {
			return "constructor";
		}

	}
}
