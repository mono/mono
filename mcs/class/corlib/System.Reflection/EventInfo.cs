//
// System.Reflection/EventInfo.cs
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
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[ClassInterface(ClassInterfaceType.AutoDual)]
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
		public bool IsSpecialName {get {return (Attributes & EventAttributes.SpecialName ) != 0;}}
		public override MemberTypes MemberType {
			get {return MemberTypes.Event;}
		}

		protected EventInfo() {
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
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

#if NET_2_0
		public virtual MethodInfo[] GetOtherMethods (bool nonPublic) {
			// Shouldn't this be abstract, like the other methods ?
			throw new NotImplementedException ();
		}

		public MethodInfo[] GetOtherMethods () {
			return GetOtherMethods (false);
		}
#endif		

		[DebuggerHidden]
		[DebuggerStepThrough]
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
