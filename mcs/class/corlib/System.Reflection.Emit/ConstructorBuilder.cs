using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;

namespace System.Reflection.Emit {
	public sealed class ConstructorBuilder : ConstructorInfo {
		public override MethodImplAttributes GetMethodImplementationFlags() {
			return (MethodImplAttributes)0;
		}
		public override ParameterInfo[] GetParameters() {
			return null;
		}
		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			return null;
		}
		public override RuntimeMethodHandle MethodHandle { get {return new RuntimeMethodHandle ();} }
		public override MethodAttributes Attributes { get {return (MethodAttributes)0;} }
		public override Type ReflectedType { get {return null;}}
		public override Type DeclaringType { get {return null;}}
		public override string Name { get {return ".ctor";}}

		public override bool IsDefined (Type attribute_type, bool inherit) {return false;}

		public override object [] GetCustomAttributes (bool inherit) {return null;}

		public override object [] GetCustomAttributes (Type attribute_type, bool inherit) {return null;}

	}
}
