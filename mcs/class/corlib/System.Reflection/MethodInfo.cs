//
// System.Reflection/MethodInfo.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;

namespace System.Reflection {

	[Serializable]
	public abstract class MethodInfo: MethodBase {

		public abstract MethodInfo GetBaseDefinition();

		protected MethodInfo() {
		}
		public override MemberTypes MemberType { get {return MemberTypes.Method;} }
		public abstract Type ReturnType { get; }
		public abstract ICustomAttributeProvider ReturnTypeCustomAttributes { get; } 
	}

}
