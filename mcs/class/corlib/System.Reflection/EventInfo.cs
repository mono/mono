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

		public Type EventHandlerType {
			get {
				ParameterInfo[] p;
				MethodInfo add = GetAddMethod (true);
				p = add.GetParameters ();
				if (p.Length > 0) {
					Type t = p [0].ParameterType;
					/* is it alwasys the first arg?
					if (!t.IsSubclassOf (typeof (System.Delegate)))
						throw new Exception ("no delegate in event");*/
					return t;
				} else
					return null;
			}
		}
		public bool IsMulticast {get {return true;}}
		public bool IsSpecialName {get {return false;}}
		public override MemberTypes MemberType {
			get {return MemberTypes.Event;}
		}

		protected EventInfo() {
		}

		public void AddEventHandler (object target, Delegate handler)
		{
			MethodInfo add = GetAddMethod ();
			if (add == null)
				throw new Exception ("No add method!?");

			add.Invoke (target, new object [] {handler});
		}

		public MethodInfo GetAddMethod() {
			return GetAddMethod (false);
		}
		public abstract MethodInfo GetAddMethod(bool nonPublic);
		public MethodInfo GetRaiseMethod() {
			return GetRaiseMethod (false);
		}
		public abstract MethodInfo GetRaiseMethod( bool nonPublic);
		public MethodInfo GetRemoveMethod() {
			return GetRemoveMethod (false);
		}
		public abstract MethodInfo GetRemoveMethod( bool nonPublic);

		public void RemoveEventHandler (object target, Delegate handler)
		{
			MethodInfo remove = GetRemoveMethod ();
			if (remove == null)
				throw new Exception ("No remove method!?");

			remove.Invoke (target, new object [] {handler});
		}

		public override bool IsDefined (Type attributeType, bool inherit) {
			return MonoCustomAttrs.IsDefined (this, attributeType, inherit);
		}

		public override object[] GetCustomAttributes( bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, inherit);
		}
		public override object[] GetCustomAttributes( Type attributeType, bool inherit) {
			return MonoCustomAttrs.GetCustomAttributes (this, attributeType, inherit);
		}
		public override Type ReflectedType {
			get {
				return null;
			}
		}
		public override Type DeclaringType {
			get {
				return null;
			}
		}

		public override String Name {
			get {
				return "Eventname";
			}
		}

	}
}
