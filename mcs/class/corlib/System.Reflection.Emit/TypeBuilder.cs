using System;
using System.Reflection;
using System.Reflection.Emit;

namespace System.Reflection.Emit {
	public sealed class TypeBuilder : Type {

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


	}
}
