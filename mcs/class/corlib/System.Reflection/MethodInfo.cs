//
// System.Reflection/MethodInfo.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[Serializable]
	[ClassInterface(ClassInterfaceType.AutoDual)]
	public abstract class MethodInfo: MethodBase {

		public abstract MethodInfo GetBaseDefinition();

		protected MethodInfo() {
		}
		public override MemberTypes MemberType { get {return MemberTypes.Method;} }
		public abstract Type ReturnType { get; }
		public abstract ICustomAttributeProvider ReturnTypeCustomAttributes { get; } 
	}

}
