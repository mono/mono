
//
// System.Reflection.Emit/MethodBuilder.cs
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {
	public sealed class MethodBuilder : MethodInfo {
		private RuntimeMethodHandle mhandle;
		private Type rtype;
		private Type[] parameters;
		private MethodAttributes attrs;
		private MethodImplAttributes iattrs;
		private string name;
		private int table_idx;
		private byte[] code;
		private ILGenerator ilgen;
		private TypeBuilder type;
		private ParameterBuilder[] pinfo;
		private MethodInfo override_method;
		private string pi_dll;
		private string pi_entry;
		private CharSet ncharset;
		private CallingConvention native_cc;
		private CallingConventions call_conv;

		internal MethodBuilder (TypeBuilder tb, string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			this.name = name;
			this.attrs = attributes;
			this.call_conv = callingConvention;
			this.rtype = returnType;
			if (parameterTypes != null) {
				this.parameters = new Type [parameterTypes.Length];
				System.Array.Copy (parameterTypes, this.parameters, parameterTypes.Length);
			}
			type = tb;
			table_idx = get_next_table_index (0x06, true);
		}

		internal MethodBuilder (TypeBuilder tb, string name, MethodAttributes attributes, 
			CallingConventions callingConvention, Type returnType, Type[] parameterTypes, 
			String dllName, String entryName, CallingConvention nativeCConv, CharSet nativeCharset) 
			: this (tb, name, attributes, callingConvention, returnType, parameterTypes) {
			pi_dll = dllName;
			pi_entry = entryName;
			native_cc = nativeCConv;
			ncharset = nativeCharset;
		}

		internal TypeBuilder TypeBuilder {
			get {return type;}
		}
		
		public override Type ReturnType {get {return rtype;}}
		public override Type ReflectedType {get {return type;}}
		public override Type DeclaringType {get {return type;}}
		public override string Name {get {return name;}}
		public override RuntimeMethodHandle MethodHandle {get {return mhandle;}}
		public override MethodAttributes Attributes {get {return attrs;}}
		public override ICustomAttributeProvider ReturnTypeCustomAttributes {
			get {return null;}
		}
		public MethodToken GetToken() {
			return new MethodToken(0x06000000 | table_idx);
		}
		
		public override MethodInfo GetBaseDefinition() {
			return null;
		}
		public override MethodImplAttributes GetMethodImplementationFlags() {
			return iattrs;
		}
		public override ParameterInfo[] GetParameters() {
			return null;
		}
		
		public void CreateMethodBody( byte[] il, int count) {
			code = new byte [count];
			System.Array.Copy(il, code, count);
		}
		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			return null;
		}
		public override bool IsDefined (Type attribute_type, bool inherit) {
			return false;
		}
		public override object[] GetCustomAttributes( bool inherit) {
			return null;
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return null;
		}
		public ILGenerator GetILGenerator () {
			return GetILGenerator (256);
		}
		public ILGenerator GetILGenerator (int size) {
			ilgen = new ILGenerator (this, size);
			return ilgen;
		}
		
		[MonoTODO]
		public ParameterBuilder DefineParameter (int position, ParameterAttributes attributes, string strParamName)
		{
			ParameterBuilder pb = new ParameterBuilder (this, position, attributes, strParamName);
			/* FIXME: add it to pinfo */
			return pb;
		}

		internal void fixup () {
			if (ilgen != null)
				ilgen.label_fixup ();
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
		}
		public void SetImplementationFlags( MethodImplAttributes attributes) {
			iattrs = attributes;
		}
		internal override int get_next_table_index (int table, bool inc) {
			return type.get_next_table_index (table, inc);
		}

		internal void set_override (MethodInfo mdecl) {
			override_method = mdecl;
		}
	}
}

