//
// System.Reflection.Emit/MethodBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
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
		private IntPtr generic_container;
#if NET_2_0 || BOOTSTRAP_NET_2_0
		private	GenericTypeParameterBuilder[] generic_params;
#else
		private Object generic_params; /* so offsets are the same */
#endif
		private Type[] returnModReq;
		private Type[] returnModOpt;
		private Type[][] paramModReq;
		private Type[][] paramModOpt;
		private RefEmitPermissionSet[] permissions;

		internal MethodBuilder (TypeBuilder tb, string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnModReq, Type[] returnModOpt, Type[] parameterTypes, Type[][] paramModReq, Type[][] paramModOpt) {
			this.name = name;
			this.attrs = attributes;
			this.call_conv = callingConvention;
			this.rtype = returnType;
			this.returnModReq = returnModReq;
			this.returnModOpt = returnModOpt;
			this.paramModReq = paramModReq;
			this.paramModOpt = paramModOpt;
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
								CallingConventions callingConvention, Type returnType, Type[] returnModReq, Type[] returnModOpt, Type[] parameterTypes, Type[][] paramModReq, Type[][] paramModOpt, 
			String dllName, String entryName, CallingConvention nativeCConv, CharSet nativeCharset) 
			: this (tb, name, attributes, callingConvention, returnType, returnModReq, returnModOpt, parameterTypes, paramModReq, paramModOpt) {
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

		[MonoTODO]
		public string Signature {
			get {
				throw new NotImplementedException ();
			}
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
			if (!type.is_created)
				throw NotSupported ();
			if (parameters == null)
				return null;

			ParameterInfo[] retval = new ParameterInfo [parameters.Length];
			for (int i = 0; i < parameters.Length; i++) {
				retval [i] = new ParameterInfo (pinfo == null ? null : pinfo [i + 1], parameters [i], this, i + 1);
			}
			return retval;
		}
		
		internal override int GetParameterCount ()
		{
			if (parameters == null)
				return 0;
			
			return parameters.Length;
		}

		public Module GetModule () {
			return type.Module;
		}

		public void CreateMethodBody( byte[] il, int count) {
			if ((il != null) && ((count < 0) || (count > il.Length)))
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
			ilgen = new ILGenerator (type.Module, ((ModuleBuilder)type.Module).GetTokenGenerator (), size);
			return ilgen;
		}
		
		public ParameterBuilder DefineParameter (int position, ParameterAttributes attributes, string strParamName)
		{
			RejectIfCreated ();
			
			//
			// Extension: Mono allows position == 0 for the return attribute
			//
			if ((position < 0) || (position > parameters.Length))
				throw new ArgumentOutOfRangeException ("position");

			ParameterBuilder pb = new ParameterBuilder (this, position, attributes, strParamName);
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

		public void AddDeclarativeSecurity( SecurityAction action, PermissionSet pset) {
			if (pset == null)
				throw new ArgumentNullException ("pset");
			if ((action == SecurityAction.RequestMinimum) ||
				(action == SecurityAction.RequestOptional) ||
				(action == SecurityAction.RequestRefuse))
				throw new ArgumentException ("Request* values are not permitted", "action");

			RejectIfCreated ();

			if (permissions != null) {
				/* Check duplicate actions */
				foreach (RefEmitPermissionSet set in permissions)
					if (set.action == action)
						throw new InvalidOperationException ("Multiple permission sets specified with the same SecurityAction.");

				RefEmitPermissionSet[] new_array = new RefEmitPermissionSet [permissions.Length + 1];
				permissions.CopyTo (new_array, 0);
				permissions = new_array;
			}
			else
				permissions = new RefEmitPermissionSet [1];

			permissions [permissions.Length - 1] = new RefEmitPermissionSet (action, pset.ToXml ().ToString ());
			attrs |= MethodAttributes.HasSecurity;
		}

		[MonoTODO]
		public void SetMarshal (UnmanagedMarshal unmanagedMarshal)
		{
			RejectIfCreated ();
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void SetSymCustomAttribute (string name, byte[] data)
		{
			RejectIfCreated ();
			throw new NotImplementedException ();
		}

		public override string ToString()
		{
			return "MethodBuilder [" + type.Name + "::" + name + "]";
		}

		[MonoTODO]
		public override bool Equals (object obj)
		{
			return base.Equals (obj);
		}

		public override int GetHashCode ()
		{
			return name.GetHashCode ();
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

#if NET_2_0 || BOOTSTRAP_NET_2_0
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		public override extern MethodInfo BindGenericParameters (Type [] types);

		public override bool Mono_IsInflatedMethod {
			get {
				return false;
			}
		}

		public override bool HasGenericParameters {
			get {
				return generic_params != null;
			}
		}

		public override Type[] GetGenericArguments ()
		{
			if (generic_params == null)
				return new Type [0];

			Type[] result = new Type [generic_params.Length];
			for (int i = 0; i < generic_params.Length; i++)
				result [i] = generic_params [i];

			return result;
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters (string[] names)
		{
			generic_params = new GenericTypeParameterBuilder [names.Length];
			for (int i = 0; i < names.Length; i++)
				generic_params [i] = new GenericTypeParameterBuilder (
					type, this, names [i], i);

			return generic_params;
		}

		public void SetGenericMethodSignature (MethodAttributes attributes, CallingConventions callingConvention, Type return_type, Type[] parameter_types)
		{
			RejectIfCreated ();

			this.attrs = attributes;
			this.call_conv = callingConvention;
			if ((attributes & MethodAttributes.Static) == 0)
 				this.call_conv |= CallingConventions.HasThis;

			this.rtype = return_type;
			this.parameters = new Type [parameter_types.Length];
			System.Array.Copy (parameter_types, this.parameters, parameter_types.Length);
		}
#endif
	}
}

