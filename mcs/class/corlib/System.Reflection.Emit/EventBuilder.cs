
//
// System.Reflection.Emit/EventBuilder.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection.Emit {
	public sealed class EventBuilder {
		public void AddOtherMethod( MethodBuilder mdBuilder) {
		}
		public EventToken GetEventToken() {
			return new EventToken();
		}
		public void SetAddOnMethod( MethodBuilder mdBuilder) {
		}
		public void SetRaiseMethod( MethodBuilder mdBuilder) {
		}
		public void SetRemoveOnMethod( MethodBuilder mdBuilder) {
		}

		public void SetCustomAttribute( CustomAttributeBuilder customBuilder) {
		}
		public void SetCustomAttribute( ConstructorInfo con, byte[] binaryAttribute) {
		}


	}
}

