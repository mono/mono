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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {
	internal struct MonoEventInfo {
		public Type parent;
		public String name;
		public MethodInfo add_method;
		public MethodInfo remove_method;
		public MethodInfo raise_method;
		public EventAttributes attrs;
		
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

		public override Type DeclaringType {
			get {
				MonoEventInfo info;
				MonoEventInfo.get_event_info (this, out info);
				
				return info.parent;
			}
		}
		public override Type ReflectedType {
			get {
				MonoEventInfo info;
				MonoEventInfo.get_event_info (this, out info);
				
				return info.parent;
			}
		}
		public override string Name {
			get {
				MonoEventInfo info;
				MonoEventInfo.get_event_info (this, out info);
				
				return info.name;
			}
		}
	}
}
