//
// System.Reflection/ConstructorInfo.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;

namespace System.Reflection {
	public abstract class ConstructorInfo : MethodBase {
		public override MemberTypes MemberType {
			get {return MemberTypes.Constructor;}
		}

		public object Invoke (object[] parameters)
		{
			//FIXME
			return null;
		}
	}
}
