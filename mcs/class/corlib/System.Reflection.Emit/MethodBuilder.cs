
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
		private CustomAttributeBuilder[] cattrs;
		private MethodInfo override_method;
		private string pi_dll;
		private string pi_entry;
		private CharSet ncharset;
		private CallingConvention native_cc;
		private CallingConventions call_conv;
		private bool init_locals = true;

		internal MethodBuilder (TypeBuilder tb, string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			this.name = name;
			this.attrs = attributes;
			this.call_conv = callingConvention;
			this.rtype = returnType;
			// The MSDN docs does not specify this, but the MS MethodBuilder
			// appends a HasThis flag if the method is not static
			if ((attributes & MethodAttributes.Static) == 0)
 				this.call_conv |= CallingConventions.HasThis;
			if (parameterTypes != null) {
				this.parameters = new Type [parameterTypes.Length];
				System.Array.Copy (parameterTypes, this.parameters, parameterTypes.Length);
			}
			type = tb;
			table_idx = get_next_table_index (this, 0x06, true);
			//Console.WriteLine ("index for "+name+" set to "+table_idx.ToString());
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

		public bool InitLocals {
			get {return init_locals;}
			set {init_locals = value;}
		}

		internal TypeBuilder TypeBuilder {
			get {return type;}
		}

		public override RuntimeMethodHandle MethodHandle {
			get {
				throw NotSupported ();
			}
		}

		public override Type ReturnType {get {return rtype;}}
		public override Type ReflectedType {get {return type;}}
		public override Type DeclaringType {get {return type;}}
		public override string Name {get {return name;}}
		public override MethodAttributes Attributes {get {return attrs;}}
		public override ICustomAttributeProvider ReturnTypeCustomAttributes {
			get {return null;}
		}

		public override CallingConventions CallingConvention { 
			get { return call_conv; }
		}

		public MethodToken GetToken() {
			return new MethodToken(0x06000000 | table_idx);
		}
		
		public override MethodInfo GetBaseDefinition() {
			return this;
		}
		public override MethodImplAttributes GetMethodImplementationFlags() {
			return iattrs;
		}
		public override ParameterInfo[] GetParameters() {
			if (parameters == null)
				return null;

			ParameterInfo[] retval = new ParameterInfo [parameters.Length];
			for (int i = 0; i < parameters.Length; i++) {
				retval [i] = new ParameterInfo (pinfo == null ? null : pinfo [i + 1], parameters [i], this, i + 1);
			}
			return retval;
		}

		public Module GetModule () {
			return type.Module;
		}

		public void CreateMethodBody( byte[] il, int count) {
			if ((il != null) && ((count < 0) || (count >= il.Length)))
				throw new ArgumentException ("Index was out of range.  Must be non-negative and less than the size of the collection.");

			if ((code != null) || type.is_created)
				throw new InvalidOperationException ("Type definition of the method is complete.");

			if (il == null)
				code = null;
			else {
				code = new byte [count];
				System.Array.Copy(il, code, count);
			}
		}

		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			throw NotSupported ();
		}
		public override bool IsDefined (Type attribute_type, bool inherit) {
			throw NotSupported ();
		}
		public override object[] GetCustomAttributes( bool inherit) {
			throw NotSupported ();
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			throw NotSupported ();
		}
		public ILGenerator GetILGenerator () {
			return GetILGenerator (64);
		}

		public ILGenerator GetILGenerator (int size) {
			if (((iattrs & MethodImplAttributes.CodeTypeMask) != 
				 MethodImplAttributes.IL) ||
				((iattrs & MethodImplAttributes.ManagedMask) != 
				 MethodImplAttributes.Managed))
				throw new InvalidOperationException ("Method body should not exist.");
			if (ilgen != null)
				return ilgen;
			ilgen = new ILGenerator (this, size);
			return ilgen;
		}
		
		[MonoTODO]
		public ParameterBuilder DefineParameter (int position, ParameterAttributes attributes, string strParamName)
		{
			//
			// Extension: Mono allows position == 0 for the return attribute
			//
			if ((position < 0) || (position > parameters.Length))
				throw new ArgumentOutOfRangeException ("position");

			RejectIfCreated ();

			ParameterBuilder pb = new ParameterBuilder (this, position, attributes, strParamName);
			// check position
			if (pinfo == null)
				pinfo = new ParameterBuilder [parameters.Length + 1];
			pinfo [position] = pb;
			return pb;
		}

		internal void fixup () {
			if (ilgen != null)
				ilgen.label_fixup ();
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
			if (customBuilder == null)
				throw new ArgumentNullException ("customBuilder");
			string attrname = customBuilder.Ctor.ReflectedType.FullName;
			if (attrname == "System.Runtime.CompilerServices.MethodImplAttribute") {
				byte[] data = customBuilder.Data;
				int impla; // the (stupid) ctor takes a short or an int ... 
				impla = (int)data [2];
				impla |= ((int)data [3]) << 8;
				SetImplementationFlags ((MethodImplAttributes)impla);
				return;
			}
			if (cattrs != null) {
				CustomAttributeBuilder[] new_array = new CustomAttributeBuilder [cattrs.Length + 1];
				cattrs.CopyTo (new_array, 0);
				new_array [cattrs.Length] = customBuilder;
				cattrs = new_array;
			} else {
				cattrs = new CustomAttributeBuilder [1];
				cattrs [0] = customBuilder;
			}
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
			if (con == null)
				throw new ArgumentNullException ("con");
			if (binaryAttribute == null)
				throw new ArgumentNullException ("binaryAttribute");
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}
		public void SetImplementationFlags( MethodImplAttributes attributes) {
			RejectIfCreated ();
			iattrs = attributes;
		}
		internal override int get_next_table_index (object obj, int table, bool inc) {
			return type.get_next_table_index (obj, table, inc);
		}

		internal void set_override (MethodInfo mdecl) {
			override_method = mdecl;
		}

		private void RejectIfCreated () {
			if (type.is_created)
				throw new InvalidOperationException ("Type definition of the method is complete.");
		}

		private Exception NotSupported () {
			return new NotSupportedException ("The invoked member is not supported in a dynamic module.");
		}
	}
}

