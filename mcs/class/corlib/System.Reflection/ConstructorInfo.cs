using System;
using System.Reflection;

namespace System.Reflection {
	public abstract class ConstructorInfo : MethodBase {
		public override MemberTypes MemberType {
			get {return MemberTypes.Constructor;}
		}
	}
}
