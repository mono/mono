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
		private RuntimeMethodHandle mhandle;
		private ILGenerator ilgen;
		private Type[] parameters;
		private MethodAttributes attrs;
		private MethodImplAttributes iattrs;
		private int table_idx;
		private CallingConventions call_conv;
		private TypeBuilder type;
		private ParameterBuilder[] pinfo;
		private CustomAttributeBuilder[] cattrs;
		private bool init_locals = true;

		internal ConstructorBuilder (TypeBuilder tb, MethodAttributes attributes, CallingConventions callingConvention, Type[] parameterTypes) {
			attrs = attributes | MethodAttributes.SpecialName;
			call_conv = callingConvention;
			if (parameterTypes != null) {
				this.parameters = new Type [parameterTypes.Length];
				System.Array.Copy (parameterTypes, this.parameters, parameterTypes.Length);
			}
			type = tb;
			table_idx = get_next_table_index (this, 0x06, true);
		}
		
		public bool InitLocals {
			get {return init_locals;}
			set {init_locals = value;}
		}

		internal TypeBuilder TypeBuilder {
			get {return type;}
		}
		
		public override MethodImplAttributes GetMethodImplementationFlags() {
			return iattrs;
		}
		public override ParameterInfo[] GetParameters() {
			if (parameters == null)
				return null;

			ParameterInfo[] retval = new ParameterInfo [parameters.Length];
			for (int i = 0; i < parameters.Length; i++) {
				retval [i] = new ParameterInfo (pinfo == null ? null : pinfo [i+1], parameters [i], this, i + 1);
			}

			return retval;
		}
		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			throw not_supported ();
		}
		public override object Invoke(BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture) {
			throw not_supported ();
		}

		public override RuntimeMethodHandle MethodHandle { 
			get {
				throw not_supported ();
			}
		}

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

		public void AddDeclarativeSecurity( SecurityAction action, PermissionSet pset) {
		}

		[MonoTODO]
		public ParameterBuilder DefineParameter(int iSequence, ParameterAttributes attributes, string strParamName)
		{
			if ((iSequence < 1) || (iSequence > parameters.Length))
				throw new ArgumentOutOfRangeException ("iSequence");
			if (type.is_created)
				throw not_after_created ();

			ParameterBuilder pb = new ParameterBuilder (this, iSequence, attributes, strParamName);
			// check iSequence
			if (pinfo == null)
				pinfo = new ParameterBuilder [parameters.Length + 1];
			pinfo [iSequence] = pb;
			return pb;
		}

		public override bool IsDefined (Type attribute_type, bool inherit) {
			throw not_supported ();
		}

		public override object [] GetCustomAttributes (bool inherit) {
			throw not_supported ();
		}

		public override object [] GetCustomAttributes (Type attribute_type, bool inherit) {
			throw not_supported ();
		}

		public ILGenerator GetILGenerator () {
			return GetILGenerator (64);
		}
		internal ILGenerator GetILGenerator (int size) {
			ilgen = new ILGenerator (this, size);
			return ilgen;
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
			if (type.is_created)
				throw not_after_created ();

			iattrs = attributes;
		}
		public Module GetModule() {
			return type.Module;
		}
		public MethodToken GetToken() {
			return new MethodToken (0x06000000 | table_idx);
		}

		[MonoTODO]
		public void SetSymCustomAttribute( string name, byte[] data) {
			if (type.is_created)
				throw not_after_created ();
		}

		public override string ToString() {
			return "constructor";
		}

		internal void fixup () {
			if (ilgen != null)
				ilgen.label_fixup ();
		}
		internal override int get_next_table_index (object obj, int table, bool inc) {
			return type.get_next_table_index (obj, table, inc);
		}

		private Exception not_supported () {
			return new NotSupportedException ("The invoked member is not supported in a dynamic module.");
		}

		private Exception not_after_created () {
			return new InvalidOperationException ("Unable to change after type has been created.");
		}
	}
}
