
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
		public static SignatureHelper GetFieldSigHelper (Module mod) {
			return null;
		}
		public static SignatureHelper GetLocalVarSigHelper( Module mod) {
			return null;
		}
		public static SignatureHelper GetMethodSigHelper( Module mod, CallingConventions callingConvention, Type returnType) {
			return null;
		}
		public static SignatureHelper GetMethodSigHelper( Module mod, Type returnType, Type[] parameterTypes) {
			return null;
		}
		public static SignatureHelper GetPropertySigHelper( Module mod, Type returnType, Type[] parameterTypes) {
			return null;
		}
		public void AddArgument( Type clsArgument) {
		}
		public void AddSentinel() {
		}
		public override bool Equals( object obj) {
			return false;
		}
		public override int GetHashCode() {
			return 0;
		}
		public byte[] GetSignature() {
			return null;
		}
		public override string ToString() {
			return "SignatureHelper";
		}



	}
}

