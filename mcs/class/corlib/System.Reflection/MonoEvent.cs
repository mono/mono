//
// System.Reflection/MonoEvent.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;

namespace System.Reflection {
	internal sealed class MonoEvent: EventInfo {
		IntPtr klass;
		IntPtr handle;

		public override EventAttributes Attributes {
			get { return (EventAttributes)0;}
		}

		public override MethodInfo GetAddMethod(bool nonPublic) {
			return null;
		}
		public override MethodInfo GetRaiseMethod( bool nonPublic) {
			return null;
		}
		public override MethodInfo GetRemoveMethod( bool nonPublic) {
			return null;
		}

		public override Type DeclaringType {
			get {
				return null;
			}
		}
		public override string Name {
			get {
				return null;
			}
		}
	}
}
