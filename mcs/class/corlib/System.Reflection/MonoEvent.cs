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
	public sealed class MonoEvent: EventInfo {

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

		/*public override bool IsDefined (Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes( bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}*/
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
