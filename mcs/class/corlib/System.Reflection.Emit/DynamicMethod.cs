//
// System.Reflection.Emit.DynamicMethod.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//   Zoltan Varga (vargaz@freemail.hu)
//
// (C) 2003 Ximian, Inc.  http://www.ximian.com
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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {

	[ComVisible (true)]
	public sealed class DynamicMethod : MethodInfo {

#pragma warning disable 169, 414
		#region Sync with reflection.h
		private RuntimeMethodHandle mhandle;
		private string name;
		private Type returnType;
		private Type[] parameters;
		private MethodAttributes attributes;
		private CallingConventions callingConvention;
		private Module module;
		private bool skipVisibility;
		private bool init_locals = true;
		private ILGenerator ilgen;
		private int nrefs;
		private object[] refs;
		private IntPtr referenced_by;
		private Type owner;
		#endregion
#pragma warning restore 169, 414
		
		private Delegate deleg;
		private MonoMethod method;
		private ParameterBuilder[] pinfo;
		internal bool creating;

		public DynamicMethod (string name, Type returnType, Type[] parameterTypes, Module m) : this (name, returnType, parameterTypes, m, false) {
		}

		public DynamicMethod (string name, Type returnType, Type[] parameterTypes, Type owner) : this (name, returnType, parameterTypes, owner, false) {
		}

		public DynamicMethod (string name, Type returnType, Type[] parameterTypes, Module m, bool skipVisibility) : this (name, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes, m, skipVisibility) {
		}

		public DynamicMethod (string name, Type returnType, Type[] parameterTypes, Type owner, bool skipVisibility) : this (name, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes, owner, skipVisibility) {
		}

		public DynamicMethod (string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Type owner, bool skipVisibility) : this (name, attributes, callingConvention, returnType, parameterTypes, owner, owner.Module, skipVisibility, false) {
		}

		public DynamicMethod (string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes, Module m, bool skipVisibility) : this (name, attributes, callingConvention, returnType, parameterTypes, null, m, skipVisibility, false) {
		}

		public DynamicMethod (string name, Type returnType, Type[] parameterTypes) : this (name, returnType, parameterTypes, false) {
		}

		[MonoTODO ("Visibility is not restricted")]
		public DynamicMethod (string name, Type returnType, Type[] parameterTypes, bool restrictedSkipVisibility)
			: this (name, MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, returnType, parameterTypes, null, null, restrictedSkipVisibility, true)
		{
		}

		DynamicMethod (string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type [] parameterTypes, Type owner, Module m, bool skipVisibility, bool anonHosted)
		{
			if (name == null)
				throw new ArgumentNullException ("name");
			if (returnType == null)
				returnType = typeof (void);
			if ((m == null) && !anonHosted)
				throw new ArgumentNullException ("m");
			if (returnType.IsByRef)
				throw new ArgumentException ("Return type can't be a byref type", "returnType");
			if (parameterTypes != null) {
				for (int i = 0; i < parameterTypes.Length; ++i)
					if (parameterTypes [i] == null)
						throw new ArgumentException ("Parameter " + i + " is null", "parameterTypes");
			}

			if (m == null)
				m = AnonHostModuleHolder.anon_host_module;

			this.name = name;
			this.attributes = attributes | MethodAttributes.Static;
			this.callingConvention = callingConvention;
			this.returnType = returnType;
			this.parameters = parameterTypes;
			this.owner = owner;
			this.module = m;
			this.skipVisibility = skipVisibility;
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void create_dynamic_method (DynamicMethod m);

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private extern void destroy_dynamic_method (DynamicMethod m);

		private void CreateDynMethod () {
			if (mhandle.Value == IntPtr.Zero) {
				if (ilgen == null || ilgen.ILOffset == 0)
					throw new InvalidOperationException ("Method '" + name + "' does not have a method body.");

				ilgen.label_fixup ();

				// Have to create all DynamicMethods referenced by this one
				try {
					// Used to avoid cycles
					creating = true;
					if (refs != null) {
						for (int i = 0; i < refs.Length; ++i) {
							if (refs [i] is DynamicMethod) {
								DynamicMethod m = (DynamicMethod)refs [i];
								if (!m.creating)
									m.CreateDynMethod ();
							}
						}
					}
				} finally {
					creating = false;
				}

				create_dynamic_method (this);
			}
		}

		~DynamicMethod ()
		{
			destroy_dynamic_method (this);
		}

		[ComVisible (true)]
		public Delegate CreateDelegate (Type delegateType)
		{
			if (delegateType == null)
				throw new ArgumentNullException ("delegateType");
			if (deleg != null)
				return deleg;

			CreateDynMethod ();

			deleg = Delegate.CreateDelegate (delegateType, this);
			return deleg;
		}

		[ComVisible (true)]
		public Delegate CreateDelegate (Type delegateType, object target)
		{
			if (delegateType == null)
				throw new ArgumentNullException ("delegateType");

			CreateDynMethod ();

			/* Can't cache the delegate since it is different for each target */
			return Delegate.CreateDelegate (delegateType, target, this);
		}
		
		public ParameterBuilder DefineParameter (int position, ParameterAttributes attributes, string parameterName)
		{
			//
			// Extension: Mono allows position == 0 for the return attribute
			//
			if ((position < 0) || (position > parameters.Length))
				throw new ArgumentOutOfRangeException ("position");

			RejectIfCreated ();

			ParameterBuilder pb = new ParameterBuilder (this, position, attributes, parameterName);
			if (pinfo == null)
				pinfo = new ParameterBuilder [parameters.Length + 1];
			pinfo [position] = pb;
			return pb;
		}

		public override MethodInfo GetBaseDefinition () {
			return this;
		}

		[MonoTODO("Not implemented")]
		public override object[] GetCustomAttributes (bool inherit) {
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public override object[] GetCustomAttributes (Type attributeType,
													  bool inherit) {
			throw new NotImplementedException ();
		}

		[MonoTODO("Not implemented")]
		public DynamicILInfo GetDynamicILInfo () {
			throw new NotImplementedException ();
		}

		public ILGenerator GetILGenerator () {
			return GetILGenerator (64);
		}

		public ILGenerator GetILGenerator (int streamSize) {
			if (((GetMethodImplementationFlags () & MethodImplAttributes.CodeTypeMask) != 
				 MethodImplAttributes.IL) ||
				((GetMethodImplementationFlags () & MethodImplAttributes.ManagedMask) != 
				 MethodImplAttributes.Managed))
				throw new InvalidOperationException ("Method body should not exist.");
			if (ilgen != null)
				return ilgen;
			ilgen = new ILGenerator (Module, new DynamicMethodTokenGenerator (this), streamSize);
			return ilgen;
		}		

		public override MethodImplAttributes GetMethodImplementationFlags () {
			return MethodImplAttributes.IL | MethodImplAttributes.Managed;
		}

		public override ParameterInfo[] GetParameters () {
			if (parameters == null)
				return new ParameterInfo [0];

			ParameterInfo[] retval = new ParameterInfo [parameters.Length];
			for (int i = 0; i < parameters.Length; i++) {
				retval [i] = new ParameterInfo (pinfo == null ? null : pinfo [i + 1], parameters [i], this, i + 1);
			}
			return retval;
		}
		
		internal override int GetParameterCount ()
		{
			return parameters == null ? 0 : parameters.Length;
		}		

		/*
		public override object Invoke (object obj, object[] parameters) {
			CreateDynMethod ();
			if (method == null)
				method = new MonoMethod (mhandle);
			return method.Invoke (obj, parameters);
		}
		*/

		public override object Invoke (object obj, BindingFlags invokeAttr,
									   Binder binder, object[] parameters,
									   CultureInfo culture)
		{
			try {
				CreateDynMethod ();
				if (method == null)
					method = new MonoMethod (mhandle);

				return method.Invoke (obj, parameters);
			}
			catch (MethodAccessException mae) {
				throw new TargetInvocationException ("Method cannot be invoked.", mae);
			}
		}

		[MonoTODO("Not implemented")]
		public override bool IsDefined (Type attributeType, bool inherit) {
			throw new NotImplementedException ();
		}

		public override string ToString () {
			string parms = String.Empty;
			ParameterInfo[] p = GetParameters ();
			for (int i = 0; i < p.Length; ++i) {
				if (i > 0)
					parms = parms + ", ";
				parms = parms + p [i].ParameterType.Name;
			}
			return ReturnType.Name+" "+Name+"("+parms+")";
		}

		public override MethodAttributes Attributes {
			get {
				return attributes;
			}
		}

		public override CallingConventions CallingConvention {
			get {
				return callingConvention;
			}
		}

		public override Type DeclaringType {
			get {
				return null;
			}
		}

		public bool InitLocals {
			get {
				return init_locals;
			}
			set {
				init_locals = value;
			}
		}

		public override RuntimeMethodHandle MethodHandle {
			get {
				return mhandle;
			}
		}

		public override Module Module {
			get {
				return module;
			}
		}

		public override string Name {
			get {
				return name;
			}
		}

		public override Type ReflectedType {
			get {
				return null;
			}
		}

		[MonoTODO("Not implemented")]
		public override ParameterInfo ReturnParameter {
			get {
				throw new NotImplementedException ();
			}
		}

		public override Type ReturnType {
			get {
				return returnType;
			}
		}

		[MonoTODO("Not implemented")]
		public override ICustomAttributeProvider ReturnTypeCustomAttributes {
			get {
				throw new NotImplementedException ();
			}
		}

/*
		public override int MetadataToken {
			get {
				return 0;
			}
		}
*/

		private void RejectIfCreated () {
			if (mhandle.Value != IntPtr.Zero)
				throw new InvalidOperationException ("Type definition of the method is complete.");
		}

		internal int AddRef (object reference) {
			if (refs == null)
				refs = new object [4];
			if (nrefs >= refs.Length - 1) {
				object [] new_refs = new object [refs.Length * 2];
				System.Array.Copy (refs, new_refs, refs.Length);
				refs = new_refs;
			}
			refs [nrefs] = reference;
			/* Reserved by the runtime */
			refs [nrefs + 1] = null;
			nrefs += 2;
			return nrefs - 1;
		}

		// This class takes care of constructing the module in a thread safe manner
		class AnonHostModuleHolder {
			public static Module anon_host_module;

			static AnonHostModuleHolder () {
				AssemblyName aname = new AssemblyName ();
				aname.Name = "Anonymously Hosted DynamicMethods Assembly";
				AssemblyBuilder ab = AppDomain.CurrentDomain.DefineDynamicAssembly (aname, AssemblyBuilderAccess.Run);

				anon_host_module = ab.GetManifestModule ();
			}
		}
	}

	internal class DynamicMethodTokenGenerator : TokenGenerator {

		private DynamicMethod m;

		public DynamicMethodTokenGenerator (DynamicMethod m) {
			this.m = m;
		}

		public int GetToken (string str) {
			return m.AddRef (str);
		}

		public int GetToken (MethodInfo method, Type[] opt_param_types) {
			throw new InvalidOperationException ();
		}

		public int GetToken (MemberInfo member, bool create_open_instance) {
			return m.AddRef (member);
		}

		public int GetToken (SignatureHelper helper) {
			return m.AddRef (helper);
		}
	}
}

