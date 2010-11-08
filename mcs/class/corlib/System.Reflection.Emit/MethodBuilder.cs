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
using System.Diagnostics.SymbolStore;

namespace System.Reflection.Emit
{
	[ComVisible (true)]
	[ComDefaultInterface (typeof (_MethodBuilder))]
	[ClassInterface (ClassInterfaceType.None)]
	public sealed class MethodBuilder : MethodInfo, _MethodBuilder
	{
#pragma warning disable 169, 414
		private RuntimeMethodHandle mhandle;
		private Type rtype;
		internal Type[] parameters;
		private MethodAttributes attrs;	/* It's used directly by MCS */
		private MethodImplAttributes iattrs;
		private string name;
		private int table_idx;
		private byte[] code;
		private ILGenerator ilgen;
		private TypeBuilder type;
		internal ParameterBuilder[] pinfo;
		private CustomAttributeBuilder[] cattrs;
		private MethodInfo override_method;
		private string pi_dll;
		private string pi_entry;
		private CharSet charset;
		private uint extra_flags; /* this encodes set_last_error etc */
		private CallingConvention native_cc;
		private CallingConventions call_conv;
		private bool init_locals = true;
		private IntPtr generic_container;
		internal GenericTypeParameterBuilder[] generic_params;
		private Type[] returnModReq;
		private Type[] returnModOpt;
		private Type[][] paramModReq;
		private Type[][] paramModOpt;
		private RefEmitPermissionSet[] permissions;
#pragma warning restore 169, 414

		internal MethodBuilder (TypeBuilder tb, string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] returnModReq, Type[] returnModOpt, Type[] parameterTypes, Type[][] paramModReq, Type[][] paramModOpt)
		{
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
				for (int i = 0; i < parameterTypes.Length; ++i)
					if (parameterTypes [i] == null)
						throw new ArgumentException ("Elements of the parameterTypes array cannot be null", "parameterTypes");

				this.parameters = new Type [parameterTypes.Length];
				System.Array.Copy (parameterTypes, this.parameters, parameterTypes.Length);
			}
			type = tb;
			table_idx = get_next_table_index (this, 0x06, true);

			((ModuleBuilder)tb.Module).RegisterToken (this, GetToken ().Token);
		}

		internal MethodBuilder (TypeBuilder tb, string name, MethodAttributes attributes, 
								CallingConventions callingConvention, Type returnType, Type[] returnModReq, Type[] returnModOpt, Type[] parameterTypes, Type[][] paramModReq, Type[][] paramModOpt, 
			String dllName, String entryName, CallingConvention nativeCConv, CharSet nativeCharset) 
			: this (tb, name, attributes, callingConvention, returnType, returnModReq, returnModOpt, parameterTypes, paramModReq, paramModOpt)
		{
			pi_dll = dllName;
			pi_entry = entryName;
			native_cc = nativeCConv;
			charset = nativeCharset;
		}

		public override bool ContainsGenericParameters {
			get { throw new NotSupportedException (); }
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

		public override Type ReturnType {
			get { return rtype; }
		}

		public override Type ReflectedType {
			get { return type; }
		}

		public override Type DeclaringType {
			get { return type; }
		}

		public override string Name {
			get { return name; }
		}

		public override MethodAttributes Attributes {
			get { return attrs; }
		}

		public override ICustomAttributeProvider ReturnTypeCustomAttributes {
			get { return null; }
		}

		public override CallingConventions CallingConvention {
			get { return call_conv; }
		}

		[MonoTODO("Not implemented")]
		public string Signature {
			get {
				throw new NotImplementedException ();
			}
		}

		/* Used by mcs */
		internal bool BestFitMapping {
			set {
				extra_flags = (uint) ((extra_flags & ~0x30) | (uint)(value ? 0x10 : 0x20));
			}
		}

		/* Used by mcs */
		internal bool ThrowOnUnmappableChar {
			set {
				extra_flags = (uint) ((extra_flags & ~0x3000) | (uint)(value ? 0x1000 : 0x2000));
			}
		}

		/* Used by mcs */
		internal bool ExactSpelling {
			set {
				extra_flags = (uint) ((extra_flags & ~0x01) | (uint)(value ? 0x01 : 0x00));
			}
		}

		/* Used by mcs */
		internal bool SetLastError {
			set {
				extra_flags = (uint) ((extra_flags & ~0x40) | (uint)(value ? 0x40 : 0x00));
			}
		}

		public MethodToken GetToken()
		{
			return new MethodToken(0x06000000 | table_idx);
		}
		
		public override MethodInfo GetBaseDefinition()
		{
			return this;
		}

		public override MethodImplAttributes GetMethodImplementationFlags()
		{
			return iattrs;
		}

		public override ParameterInfo[] GetParameters()
		{
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

		public Module GetModule ()
		{
			return type.Module;
		}

		public void CreateMethodBody (byte[] il, int count)
		{
			if ((il != null) && ((count < 0) || (count > il.Length)))
				throw new ArgumentOutOfRangeException ("Index was out of range.  Must be non-negative and less than the size of the collection.");

			if ((code != null) || type.is_created)
				throw new InvalidOperationException ("Type definition of the method is complete.");

			if (il == null)
				code = null;
			else {
				code = new byte [count];
				System.Array.Copy(il, code, count);
			}
		}

		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture)
		{
			throw NotSupported ();
		}

		public override bool IsDefined (Type attributeType, bool inherit)
		{
			throw NotSupported ();
		}

		public override object[] GetCustomAttributes (bool inherit)
		{
			/*
			 * On MS.NET, this always returns not_supported, but we can't do this
			 * since there would be no way to obtain custom attributes of 
			 * dynamically created ctors.
			 */
			if (type.is_created)
				return MonoCustomAttrs.GetCustomAttributes (this, inherit);
			else
				throw NotSupported ();
		}

		public override object[] GetCustomAttributes (Type attributeType, bool inherit)
		{
			if (type.is_created)
				return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
			else
				throw NotSupported ();
		}

		public ILGenerator GetILGenerator ()
		{
			return GetILGenerator (64);
		}

		public ILGenerator GetILGenerator (int size)
		{
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

		internal void check_override ()
		{
			if (override_method != null && override_method.IsVirtual && !IsVirtual)
				throw new TypeLoadException (String.Format("Method '{0}' override '{1}' but it is not virtual", name, override_method));
		}

		internal void fixup ()
		{
			if (((attrs & (MethodAttributes.Abstract | MethodAttributes.PinvokeImpl)) == 0) && ((iattrs & (MethodImplAttributes.Runtime | MethodImplAttributes.InternalCall)) == 0)) {
				// do not allow zero length method body on MS.NET 2.0 (and higher)
				if (((ilgen == null) || (ilgen.ILOffset == 0)) && (code == null || code.Length == 0))
					throw new InvalidOperationException (
									     String.Format ("Method '{0}.{1}' does not have a method body.",
											    DeclaringType.FullName, Name));
			}
			if (ilgen != null)
				ilgen.label_fixup ();
		}
		
		internal void GenerateDebugInfo (ISymbolWriter symbolWriter)
		{
			if (ilgen != null && ilgen.HasDebugInfo) {
				SymbolToken token = new SymbolToken (GetToken().Token);
				symbolWriter.OpenMethod (token);
				symbolWriter.SetSymAttribute (token, "__name", System.Text.Encoding.UTF8.GetBytes (Name));
				ilgen.GenerateDebugInfo (symbolWriter);
				symbolWriter.CloseMethod ();
			}
		}

		public void SetCustomAttribute (CustomAttributeBuilder customBuilder)
		{
			if (customBuilder == null)
				throw new ArgumentNullException ("customBuilder");

			switch (customBuilder.Ctor.ReflectedType.FullName) {
				case "System.Runtime.CompilerServices.MethodImplAttribute":
					byte[] data = customBuilder.Data;
					int impla; // the (stupid) ctor takes a short or an int ... 
					impla = (int)data [2];
					impla |= ((int)data [3]) << 8;
					iattrs |= (MethodImplAttributes)impla;
					return;

				case "System.Runtime.InteropServices.DllImportAttribute":
					CustomAttributeBuilder.CustomAttributeInfo attr = CustomAttributeBuilder.decode_cattr (customBuilder);
					bool preserveSig = true;

					/*
					 * It would be easier to construct a DllImportAttribute from
					 * the custom attribute builder, but the DllImportAttribute 
					 * does not contain all the information required here, ie.
					 * - some parameters, like BestFitMapping has three values
					 *   ("on", "off", "missing"), but DllImportAttribute only
					 *   contains two (on/off).
					 * - PreserveSig is true by default, while it is false by
					 *   default in DllImportAttribute.
					 */

					pi_dll = (string)attr.ctorArgs[0];
					if (pi_dll == null || pi_dll.Length == 0)
						throw new ArgumentException ("DllName cannot be empty");

					native_cc = System.Runtime.InteropServices.CallingConvention.Winapi;

					for (int i = 0; i < attr.namedParamNames.Length; ++i) {
						string name = attr.namedParamNames [i];
						object value = attr.namedParamValues [i];

						if (name == "CallingConvention")
							native_cc = (CallingConvention)value;
						else if (name == "CharSet")
							charset = (CharSet)value;
						else if (name == "EntryPoint")
							pi_entry = (string)value;
						else if (name == "ExactSpelling")
							ExactSpelling = (bool)value;
						else if (name == "SetLastError")
							SetLastError = (bool)value;
						else if (name == "PreserveSig")
							preserveSig = (bool)value;
					else if (name == "BestFitMapping")
						BestFitMapping = (bool)value;
					else if (name == "ThrowOnUnmappableChar")
						ThrowOnUnmappableChar = (bool)value;
					}

					attrs |= MethodAttributes.PinvokeImpl;
					if (preserveSig)
						iattrs |= MethodImplAttributes.PreserveSig;
					return;

				case "System.Runtime.InteropServices.PreserveSigAttribute":
					iattrs |= MethodImplAttributes.PreserveSig;
					return;
				case "System.Runtime.CompilerServices.SpecialNameAttribute":
					attrs |= MethodAttributes.SpecialName;
					return;
				case "System.Security.SuppressUnmanagedCodeSecurityAttribute":
					attrs |= MethodAttributes.HasSecurity;
					break;
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

		[ComVisible (true)]
		public void SetCustomAttribute (ConstructorInfo con, byte[] binaryAttribute)
		{
			if (con == null)
				throw new ArgumentNullException ("con");
			if (binaryAttribute == null)
				throw new ArgumentNullException ("binaryAttribute");
			SetCustomAttribute (new CustomAttributeBuilder (con, binaryAttribute));
		}

		public void SetImplementationFlags (MethodImplAttributes attributes)
		{
			RejectIfCreated ();
			iattrs = attributes;
		}

		public void AddDeclarativeSecurity (SecurityAction action, PermissionSet pset)
		{
#if !NET_2_1
			if (pset == null)
				throw new ArgumentNullException ("pset");
			if ((action == SecurityAction.RequestMinimum) ||
				(action == SecurityAction.RequestOptional) ||
				(action == SecurityAction.RequestRefuse))
				throw new ArgumentOutOfRangeException ("Request* values are not permitted", "action");

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
#endif
		}

		[Obsolete ("An alternate API is available: Emit the MarshalAs custom attribute instead.")]
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

		internal override int get_next_table_index (object obj, int table, bool inc)
		{
			return type.get_next_table_index (obj, table, inc);
		}

		internal void set_override (MethodInfo mdecl)
		{
			override_method = mdecl;
		}

		private void RejectIfCreated ()
		{
			if (type.is_created)
				throw new InvalidOperationException ("Type definition of the method is complete.");
		}

		private Exception NotSupported ()
		{
			return new NotSupportedException ("The invoked member is not supported in a dynamic module.");
		}

		public override MethodInfo MakeGenericMethod (params Type [] typeArguments)
		{
			if (!IsGenericMethodDefinition)
				throw new InvalidOperationException ("Method is not a generic method definition");
			if (typeArguments == null)
				throw new ArgumentNullException ("typeArguments");
			if (generic_params.Length != typeArguments.Length)
				throw new ArgumentException ("Incorrect length", "typeArguments");
			foreach (Type type in typeArguments) {
				if (type == null)
					throw new ArgumentNullException ("typeArguments");
			}

			return new MethodOnTypeBuilderInst (this, typeArguments);
		}

		public override bool IsGenericMethodDefinition {
			get {
				return generic_params != null;
			}
		}

		public override bool IsGenericMethod {
			get {
				return generic_params != null;
			}
		}

		public override MethodInfo GetGenericMethodDefinition ()
		{
			if (!IsGenericMethodDefinition)
				throw new InvalidOperationException ();

			return this;
		}

		public override Type[] GetGenericArguments ()
		{
			if (generic_params == null)
				return null;

			Type[] result = new Type [generic_params.Length];
			for (int i = 0; i < generic_params.Length; i++)
				result [i] = generic_params [i];

			return result;
		}

		public GenericTypeParameterBuilder[] DefineGenericParameters (params string[] names)
		{
			if (names == null)
				throw new ArgumentNullException ("names");
			if (names.Length == 0)
				throw new ArgumentException ("names");

			generic_params = new GenericTypeParameterBuilder [names.Length];
			for (int i = 0; i < names.Length; i++) {
				string item = names [i];
				if (item == null)
					throw new ArgumentNullException ("names");
				generic_params [i] = new GenericTypeParameterBuilder (type, this, item, i);
			}

			return generic_params;
		}

		public void SetReturnType (Type returnType)
		{
			rtype = returnType;
		}

		public void SetParameters (params Type[] parameterTypes)
		{
			if (parameterTypes != null) {
				for (int i = 0; i < parameterTypes.Length; ++i)
					if (parameterTypes [i] == null)
						throw new ArgumentException ("Elements of the parameterTypes array cannot be null", "parameterTypes");

				this.parameters = new Type [parameterTypes.Length];
				System.Array.Copy (parameterTypes, this.parameters, parameterTypes.Length);
			}
		}

		public void SetSignature (Type returnType, Type[] returnTypeRequiredCustomModifiers, Type[] returnTypeOptionalCustomModifiers, Type[] parameterTypes, Type[][] parameterTypeRequiredCustomModifiers, Type[][] parameterTypeOptionalCustomModifiers)
		{
			SetReturnType (returnType);
			SetParameters (parameterTypes);
			this.returnModReq = returnTypeRequiredCustomModifiers;
			this.returnModOpt = returnTypeOptionalCustomModifiers;
			this.paramModReq = parameterTypeRequiredCustomModifiers;
			this.paramModOpt = parameterTypeOptionalCustomModifiers;
		}

		public override Module Module {
			get {
				return base.Module;
			}
		}

		void _MethodBuilder.GetIDsOfNames([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		void _MethodBuilder.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MethodBuilder.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _MethodBuilder.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
	}
}
