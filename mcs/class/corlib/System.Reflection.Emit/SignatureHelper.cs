
//
// System.Reflection.Emit/SignatureHelper.cs
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

	public sealed class SignatureHelper {
		internal enum SignatureHelperType {
			HELPER_FIELD,
			HELPER_LOCAL,
			HELPER_METHOD,
			HELPER_PROPERTY
		}

		private ModuleBuilder module;
		private Type[] arguments;
		private SignatureHelperType type;
		private Type returnType;
		private CallingConventions callConv;
		private CallingConvention unmanagedCallConv;

		internal SignatureHelper (ModuleBuilder module, SignatureHelperType type)
		{
			this.type = type;
			this.module = module;
		}

		public static SignatureHelper GetFieldSigHelper (Module mod)
		{
			if (!(mod is ModuleBuilder))
				throw new NotImplementedException ();

			return new SignatureHelper ((ModuleBuilder) mod, SignatureHelperType.HELPER_FIELD);
		}

		public static SignatureHelper GetLocalVarSigHelper (Module mod)
		{
			if (!(mod is ModuleBuilder))
				throw new NotImplementedException ();

			return new SignatureHelper ((ModuleBuilder) mod, SignatureHelperType.HELPER_LOCAL);
		}

		public static SignatureHelper GetMethodSigHelper( Module mod, CallingConventions callingConvention, Type returnType)
		{
			return GetMethodSigHelper (mod, callingConvention, (CallingConvention)0, returnType, null);
		}

		public static SignatureHelper GetMethodSigHelper( Module mod, CallingConvention unmanagedCallingConvention, Type returnType)
		{
			return GetMethodSigHelper (mod, CallingConventions.Standard, unmanagedCallingConvention, returnType, null);
		}

		public static SignatureHelper GetMethodSigHelper( Module mod, Type returnType, Type[] parameterTypes)
		{
			return GetMethodSigHelper (mod, CallingConventions.Standard, 
									   (CallingConvention)0, returnType, 
									   parameterTypes);
		}
		[MonoTODO]
		public static SignatureHelper GetPropertySigHelper( Module mod, Type returnType, Type[] parameterTypes)
		{
			throw new NotImplementedException ();
		}

		public void AddArgument (Type clsArgument)
		{
			if (arguments != null) {
				Type[] new_a = new Type [arguments.Length + 1];
				System.Array.Copy (arguments, new_a, arguments.Length);
				new_a [arguments.Length] = clsArgument;
				arguments = new_a;
			} else {
				arguments = new Type [1];
				arguments [0] = clsArgument;
			}
		}
		[MonoTODO]
		public void AddSentinel ()
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override bool Equals (object obj)
		{
			throw new NotImplementedException ();
		}
		[MonoTODO]
		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern byte[] get_signature_local ();

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal extern byte[] get_signature_field ();

		public byte[] GetSignature ()
		{
			switch (type) {
			case SignatureHelperType.HELPER_LOCAL:
				return get_signature_local ();
			case SignatureHelperType.HELPER_FIELD:
				return get_signature_field ();
			default:
				throw new NotImplementedException ();
			}
		}

		public override string ToString() {
			return "SignatureHelper";
		}

		internal static SignatureHelper GetMethodSigHelper( Module mod, CallingConventions callConv, CallingConvention unmanagedCallConv, Type returnType,
														   Type [] parameters)
		{
			if (!(mod is ModuleBuilder))
				throw new NotImplementedException ();

			SignatureHelper helper = 
				new SignatureHelper ((ModuleBuilder)mod, SignatureHelperType.HELPER_METHOD);
			helper.returnType = returnType;
			helper.callConv = callConv;
			helper.unmanagedCallConv = unmanagedCallConv;

			if (parameters != null) {
				helper.arguments = new Type [parameters.Length];
				for (int i = 0; i < parameters.Length; ++i)
					helper.arguments [i] = parameters [i];
			}

			return helper;
		}


	}
}

