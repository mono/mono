//
// System.Reflection/EventInfo.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
//

using System;
using System.Reflection;

namespace System.Reflection {
	public abstract class EventInfo : MemberInfo {

		public abstract EventAttributes Attributes {get;}

		public Type EventHandlerType {get {return null;}}
		public bool IsMulticast {get {return true;}}
		public bool IsSpecialName {get {return false;}}
		public override MemberTypes MemberType {
			get {return MemberTypes.Event;}
		}

		protected EventInfo() {
		}

		public void AddEventHandler( object target, Delegate handler) {
		}
		public MethodInfo GetAddMethod() {
			return null;
		}
		public abstract MethodInfo GetAddMethod(bool nonPublic);
		public MethodInfo GetRaiseMethod() {
			return null;
		}
		public abstract MethodInfo GetRaiseMethod( bool nonPublic);
		public MethodInfo GetRemoveMethod() {
			return null;
		}
		public abstract MethodInfo GetRemoveMethod( bool nonPublic);
		public void RemoveEventHandler( object target, Delegate handler) {
		}

	}
}
