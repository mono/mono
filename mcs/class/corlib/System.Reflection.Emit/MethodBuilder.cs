using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;

namespace System.Reflection.Emit {
	public sealed class MethodBuilder : MethodInfo {
		private Type rtype;
		private Type[] paremeters;
		private MethodAttributes attrs;
		private string name;
		private RuntimeMethodHandle mhandle;
		
		public override Type ReturnType {get {return rtype;}}
		public override Type ReflectedType {get {return null;}}
		public override Type DeclaringType {get {return null;}}
		public override string Name {get {return name;}}
		public override RuntimeMethodHandle MethodHandle {get {return mhandle;}}
		public override MethodAttributes Attributes {get {return attrs;}}
		public override ICustomAttributeProvider ReturnTypeCustomAttributes {
			get {return null;}
		}

		
		public override MethodInfo GetBaseDefinition() {
			return null;
		}
		public override MethodImplAttributes GetMethodImplementationFlags() {
			return (MethodImplAttributes)0;
		}
		public override ParameterInfo[] GetParameters() {
			return null;
		}
		public void CreateMethodBody( byte[] il, int count) {
			
		}
		public override Object Invoke(Object obj, BindingFlags invokeAttr, Binder binder, Object[] parameters, CultureInfo culture) {
			return null;
		}
		public override bool IsDefined (Type attribute_type, bool inherit) {
			return false;
		}
		public override object[] GetCustomAttributes( bool inherit) {
			return null;
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return null;
		}




	}
}

