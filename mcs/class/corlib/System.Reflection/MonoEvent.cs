//
// System.Reflection/MonoEvent.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	internal struct MonoEventInfo {
		public Type declaring_type;
		public Type reflected_type;
		public String name;
		public MethodInfo add_method;
		public MethodInfo remove_method;
		public MethodInfo raise_method;
		public EventAttributes attrs;
		public MethodInfo[] other_methods;
		
		[MethodImplAttribute(MethodImplOptions.InternalCall)]
		internal static extern void get_event_info (MonoEvent ev, out MonoEventInfo info);
	}

	internal sealed class MonoEvent: EventInfo {
		IntPtr klass;
		IntPtr handle;

		public override EventAttributes Attributes {
			get {
				MonoEventInfo info;
				MonoEventInfo.get_event_info (this, out info);
				
				return info.attrs;
			}
		}

		public override MethodInfo GetAddMethod(bool nonPublic) {
			MonoEventInfo info;
			MonoEventInfo.get_event_info (this, out info);
				
			return info.add_method;
		}

		public override MethodInfo GetRaiseMethod( bool nonPublic) {
			MonoEventInfo info;
			MonoEventInfo.get_event_info (this, out info);
				
			return info.raise_method;
		}

		public override MethodInfo GetRemoveMethod( bool nonPublic) {
			MonoEventInfo info;
			MonoEventInfo.get_event_info (this, out info);
				
			return info.remove_method;
		}

#if NET_2_0
		public override MethodInfo[] GetOtherMethods (bool nonPublic) {
			MonoEventInfo info;
			MonoEventInfo.get_event_info (this, out info);

			return info.other_methods;
		}
#endif

		public override Type DeclaringType {
			get {
				MonoEventInfo info;
				MonoEventInfo.get_event_info (this, out info);
				
				return info.declaring_type;
			}
		}

		public override Type ReflectedType {
			get {
				MonoEventInfo info;
				MonoEventInfo.get_event_info (this, out info);
				
				return info.reflected_type;
			}
		}

		public override string Name {
			get {
				MonoEventInfo info;
				MonoEventInfo.get_event_info (this, out info);
				
				return info.name;
			}
		}

		public override string ToString () {
			return EventHandlerType + " " + Name;
		}

	}
}
