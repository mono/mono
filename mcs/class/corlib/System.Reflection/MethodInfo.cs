using System;

namespace System.Reflection {

	public abstract class MethodInfo: MethodBase {

		public abstract MethodInfo GetBaseDefinition();

		protected MethodInfo() {
		}
		public override MemberTypes MemberType { get {return MemberTypes.Method;} }
		public abstract Type ReturnType { get; }
		public abstract ICustomAttributeProvider ReturnTypeCustomAttributes { get; } 
	}

}
