//
// System.Reflection/EventInfo.cs
//
// Author:
//   Paolo Molaro (lupus@ximian.com)
//
// (C) 2001 Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

using System.Diagnostics;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[ComVisible (true)]
	[ComDefaultInterfaceAttribute (typeof (_EventInfo))]
	[Serializable]
	[ClassInterface(ClassInterfaceType.None)]
	[StructLayout (LayoutKind.Sequential)]
#if MOBILE
	public abstract class EventInfo : MemberInfo {
#else
	public abstract class EventInfo : MemberInfo, _EventInfo {
#endif
		AddEventAdapter cached_add_event;

		public abstract EventAttributes Attributes {get;}

		public
#if NET_4_0
		virtual
#endif
		Type EventHandlerType {
			get {
				ParameterInfo[] p;
				MethodInfo add = GetAddMethod (true);
				p = add.GetParametersInternal ();
				if (p.Length > 0) {
					Type t = p [0].ParameterType;
					/* is it alwasys the first arg?
					if (!t.IsSubclassOf (typeof (System.Delegate)))
						throw new Exception ("no delegate in event");*/
					return t;
				}

				return null;
			}
		}

		public
#if NET_4_0
		virtual
#endif
		bool IsMulticast {get {return true;}}
		public bool IsSpecialName {get {return (Attributes & EventAttributes.SpecialName ) != 0;}}
		public override MemberTypes MemberType {
			get {return MemberTypes.Event;}
		}

		protected EventInfo() {
		}


		[DebuggerHidden]
		[DebuggerStepThrough]
		public
#if NET_4_0
		virtual
#endif
		void AddEventHandler (object target, Delegate handler)
		{
// this optimization cause problems with full AOT
// see bug https://bugzilla.xamarin.com/show_bug.cgi?id=3682
#if FULL_AOT_RUNTIME
			MethodInfo add = GetAddMethod ();
			if (add == null)
				throw new InvalidOperationException ("Cannot add a handler to an event that doesn't have a visible add method");
			if (target == null && !add.IsStatic)
				throw new TargetException ("Cannot add a handler to a non static event with a null target");
			add.Invoke (target, new object [] {handler});
#else
			if (cached_add_event == null) {
				MethodInfo add = GetAddMethod ();
				if (add == null)
					throw new InvalidOperationException ("Cannot add a handler to an event that doesn't have a visible add method");
				if (add.DeclaringType.IsValueType) {
					if (target == null && !add.IsStatic)
						throw new TargetException ("Cannot add a handler to a non static event with a null target");
					add.Invoke (target, new object [] {handler});
					return;
				}
				cached_add_event = CreateAddEventDelegate (add);
			}
			//if (target == null && is_instance)
			//	throw new TargetException ("Cannot add a handler to a non static event with a null target");
			cached_add_event (target, handler);
#endif
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

		public virtual MethodInfo[] GetOtherMethods (bool nonPublic) {
			// implemented by the derived class
			return EmptyArray<MethodInfo>.Value;
		}

		public MethodInfo[] GetOtherMethods () {
			return GetOtherMethods (false);
		}

		[DebuggerHidden]
		[DebuggerStepThrough]
		public
#if NET_4_0
		virtual
#endif
		void RemoveEventHandler (object target, Delegate handler)
		{
			MethodInfo remove = GetRemoveMethod ();
			if (remove == null)
				throw new InvalidOperationException ("Cannot remove a handler to an event that doesn't have a visible remove method");

			remove.Invoke (target, new object [] {handler});
		}

#if NET_4_0
		public override bool Equals (object obj)
		{
			return obj == (object) this;
		}

		public override int GetHashCode ()
		{
			return base.GetHashCode ();
		}

		public static bool operator == (EventInfo left, EventInfo right)
		{
			if ((object)left == (object)right)
				return true;
			if ((object)left == null ^ (object)right == null)
				return false;
			return left.Equals (right);
		}

		public static bool operator != (EventInfo left, EventInfo right)
		{
			if ((object)left == (object)right)
				return false;
			if ((object)left == null ^ (object)right == null)
				return true;
			return !left.Equals (right);
		}
#endif

#if !MOBILE
		void _EventInfo.GetIDsOfNames ([In] ref Guid riid, IntPtr rgszNames, uint cNames, uint lcid, IntPtr rgDispId)
		{
			throw new NotImplementedException ();
		}

		Type _EventInfo.GetType ()
		{
			// Required or object::GetType becomes virtual final
			return base.GetType ();
		}

		void _EventInfo.GetTypeInfo (uint iTInfo, uint lcid, IntPtr ppTInfo)
		{
			throw new NotImplementedException ();
		}

		void _EventInfo.GetTypeInfoCount (out uint pcTInfo)
		{
			throw new NotImplementedException ();
		}

		void _EventInfo.Invoke (uint dispIdMember, [In] ref Guid riid, uint lcid, short wFlags, IntPtr pDispParams, IntPtr pVarResult, IntPtr pExcepInfo, IntPtr puArgErr)
		{
			throw new NotImplementedException ();
		}
#endif

		delegate void AddEventAdapter (object _this, Delegate dele);

// this optimization cause problems with full AOT
// see bug https://bugzilla.xamarin.com/show_bug.cgi?id=3682
// do not revove the above delegate or it's field since it's required by the runtime!
#if !FULL_AOT_RUNTIME
		delegate void AddEvent<T, D> (T _this, D dele);
		delegate void StaticAddEvent<D> (D dele);

#pragma warning disable 169
		// Used via reflection
		static void AddEventFrame<T,D> (AddEvent<T,D> addEvent, object obj, object dele)
		{
			if (obj == null)
				throw new TargetException ("Cannot add a handler to a non static event with a null target");
			if (!(obj is T))
				throw new TargetException ("Object doesn't match target");
			addEvent ((T)obj, (D)dele);
		}

		static void StaticAddEventAdapterFrame<D> (StaticAddEvent<D> addEvent, object obj, object dele)
		{
			addEvent ((D)dele);
		}
#pragma warning restore 169

		/*
		 * The idea behing this optimization is to use a pair of delegates to simulate the same effect of doing a reflection call.
		 * The first delegate performs casting and boxing, the second dispatch to the add_... method.
		 */
		static AddEventAdapter CreateAddEventDelegate (MethodInfo method)
		{
			Type[] typeVector;
			Type addHandlerType;
			object addHandlerDelegate;
			MethodInfo adapterFrame;
			Type addHandlerDelegateType;
			string frameName;

			if (method.IsStatic) {
				typeVector = new Type[] { method.GetParametersInternal () [0].ParameterType };
				addHandlerDelegateType = typeof (StaticAddEvent<>);
				frameName = "StaticAddEventAdapterFrame";
			} else {
				typeVector = new Type[] { method.DeclaringType, method.GetParametersInternal () [0].ParameterType };
				addHandlerDelegateType = typeof (AddEvent<,>);
				frameName = "AddEventFrame";
			}

			addHandlerType = addHandlerDelegateType.MakeGenericType (typeVector);
#if NET_2_1
			// with Silverlight a coreclr failure (e.g. Transparent caller creating a delegate on a Critical method)
			// would normally throw an ArgumentException, so we set throwOnBindFailure to false and check for a null
			// delegate that we can transform into a MethodAccessException
			addHandlerDelegate = Delegate.CreateDelegate (addHandlerType, method, false);
			if (addHandlerDelegate == null)
				throw new MethodAccessException ();
#else
			addHandlerDelegate = Delegate.CreateDelegate (addHandlerType, method);
#endif
			adapterFrame = typeof (EventInfo).GetMethod (frameName, BindingFlags.Static | BindingFlags.NonPublic);
			adapterFrame = adapterFrame.MakeGenericMethod (typeVector);
			return (AddEventAdapter)Delegate.CreateDelegate (typeof (AddEventAdapter), addHandlerDelegate, adapterFrame, true);
		}
#endif

#if NET_4_5
		public virtual MethodInfo AddMethod {
			get { return GetAddMethod (true); }
		}
		public virtual MethodInfo RaiseMethod {
			get { return GetRaiseMethod (true); }
		}
		public virtual MethodInfo RemoveMethod {
			get { return GetRemoveMethod (true); }
		}
#endif
	}
}
