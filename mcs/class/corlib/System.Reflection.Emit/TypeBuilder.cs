//
// System.Reflection.Emit/TypeBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;

namespace System.Reflection.Emit {
	public sealed class TypeBuilder : Type {
		private IntPtr _impl;

	public const int UnspecifiedTypeSize = 1; // FIXME: check the real value
		
		public override Assembly Assembly {get {return null;}}
		public override string AssemblyQualifiedName {get {return null;}}
		public override Type BaseType {get {return null;}}
		public override Type DeclaringType {get {return null;}}
		public override string FullName {get {return null;}}
		//public override Guid GUID {get {return null;}}
		//public override Module Module {get {return null;}}
		public override string Name {get {return null;}}
		//public override string Namespace {get {return null;}}
		public PackingSize PackingSize {get {return (PackingSize)0;}}
		public override Type ReflectedType {get {return null;}}
		public override MemberTypes MemberType { get {return (MemberTypes)0;}}

		public override bool IsDefined( Type attributeType, bool inherit) {
			return false;
		}
		public override object[] GetCustomAttributes(bool inherit) {
			return null;
		}
		public override object[] GetCustomAttributes(Type attributeType, bool inherit) {
			return null;
		}

		public MethodBuilder DefineMethod( string name, MethodAttributes attributes, Type returnType, Type[] parameterTypes) {
			return DefineMethod (name, attributes, CallingConventions.Standard, returnType, parameterTypes);
		}

		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		private static extern MethodBuilder defineMethod (TypeBuilder typeb, string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes);

		public MethodBuilder DefineMethod( string name, MethodAttributes attributes, CallingConventions callingConvention, Type returnType, Type[] parameterTypes) {
			return defineMethod (this, name, attributes, callingConvention, returnType, parameterTypes);
		}

		public Type CreateType() {
			return null;
		}

	}
}
