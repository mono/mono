
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

namespace System.Reflection.Emit {
	public sealed class MethodBuilder : MethodInfo {
		private RuntimeMethodHandle mhandle;
		private Type rtype;
		private Type[] parameters;
		private MethodAttributes attrs;
		private string name;
		private int table_idx;
		private byte[] code;
		private ILGenerator ilgen;
		internal TypeBuilder type;
		private ParameterBuilder[] pinfo;

		internal MethodBuilder (TypeBuilder tb, string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			this.name = name;
			this.attrs = attributes;
			// call conv
			this.rtype = returnType;
			if (parameterTypes != null) {
				this.parameters = new Type [parameterTypes.Length];
				System.Array.Copy (parameterTypes, this.parameters, parameterTypes.Length);
			}
			type = tb;
			table_idx = tb.module.assemblyb.get_next_table_index (0x06, true);
		}
		
		public override Type ReturnType {get {return rtype;}}
		public override Type ReflectedType {get {return null;}}
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
			return (MethodImplAttributes)0;
		}
		public override ParameterInfo[] GetParameters() {
			return null;
		}
		
		/*
		 * FIXME: this method signature needs to be expanded to handle also
		 * a ILGenerator.
		 */
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

		public ParameterBuilder DefineParameter( int position, ParameterAttributes attributes, string strParamName) {
			ParameterBuilder pb = new ParameterBuilder (this, position, attributes, strParamName);
			/* FIXME: add it to pinfo */
			return pb;
		}

		internal void fixup () {
			if (ilgen != null)
				ilgen.label_fixup ();
		}
	}
}

