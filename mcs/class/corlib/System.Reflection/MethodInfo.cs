using System;

namespace System.Reflection {

	public abstract class MethodInfo: MethodBase {

		private MemberTypes member_type;

		public abstract MethodInfo GetBaseDefinition();

		protected MethodInfo() {
		}
		public override MemberTypes MemberType { get {return member_type;} }
		public abstract Type ReturnType { get; }
		public abstract ICustomAttributeProvider ReturnTypeCustomAttributes { get; } 
	}

}
