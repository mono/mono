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
		private Type[] parameters;
		private MethodAttributes attrs;
		private MethodImplAttributes iattrs;
		private int table_idx;
		private CallingConventions call_conv;
		private TypeBuilder type;

		internal ConstructorBuilder (TypeBuilder tb, MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes) {
			attrs = attributes;
			call_conv = callingConvention;
			if (parameterTypes != null) {
				this.parameters = new Type [parameterTypes.Length];
				System.Array.Copy (parameterTypes, this.parameters, parameterTypes.Length);
			}
			type = tb;
			table_idx = get_next_table_index (0x06, true);
		}
		
		public override MethodImplAttributes GetMethodImplementationFlags() {
			return iattrs;
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
		public override MethodAttributes Attributes { 
			get {return attrs;} 
		}
		public override Type ReflectedType { get {return type;}}
		public override Type DeclaringType { get {return type;}}
		public Type ReturnType { get {return null;}}
		public override string Name { 
			get {return (attrs & MethodAttributes.Static) != 0 ? ".cctor" : ".ctor";}
		}
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
			ParameterBuilder pb = new ParameterBuilder (this, iSequence, attributes, strParamName);
			/* FIXME: add it to pinfo */
			return pb;
		}

		public override bool IsDefined (Type attribute_type, bool inherit) {return false;}

		public override object [] GetCustomAttributes (bool inherit) {return null;}

		public override object [] GetCustomAttributes (Type attribute_type, bool inherit) {return null;}

		public ILGenerator GetILGenerator () {
			return GetILGenerator (256);
		}
		public ILGenerator GetILGenerator (int size) {
			ilgen = new ILGenerator (this, size);
			return ilgen;
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
		}
		public void SetImplementationFlags( MethodImplAttributes attributes) {
			iattrs = attributes;
		}
		public Module GetModule() {
			return null;
		}
		public MethodToken GetToken() {
			return new MethodToken (0x06000000 | table_idx);
		}
		public void SetSymCustomAttribute( string name, byte[] data) {
		}
		public override string ToString() {
			return "constructor";
		}

		internal void fixup () {
			if (ilgen != null)
				ilgen.label_fixup ();
		}
		internal override int get_next_table_index (int table, bool inc) {
			return type.get_next_table_index (table, inc);
		}

	}
}
