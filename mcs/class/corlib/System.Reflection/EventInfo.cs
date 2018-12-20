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
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace System.Reflection {

	[Serializable]
	public abstract partial class EventInfo : MemberInfo {
		AddEventAdapter cached_add_event;

		[DebuggerHidden]
		[DebuggerStepThrough]
		public virtual void AddEventHandler (object target, Delegate handler)
		{
// this optimization cause problems with full AOT
// see bug https://bugzilla.xamarin.com/show_bug.cgi?id=3682
#if FULL_AOT_RUNTIME
			MethodInfo add = GetAddMethod ();
			if (add == null)
				throw new InvalidOperationException (SR.InvalidOperation_NoPublicAddMethod);
			if (target == null && !add.IsStatic)
				throw new TargetException ("Cannot add a handler to a non static event with a null target");
			add.Invoke (target, new object [] {handler});
#else
			if (cached_add_event == null) {
				MethodInfo add = GetAddMethod ();
				if (add == null)
					throw new InvalidOperationException (SR.InvalidOperation_NoPublicAddMethod);

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
			if (!(dele is D))
				throw new ArgumentException ($"Object of type {dele.GetType ()} cannot be converted to type {typeof (D)}.");
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
#if MOBILE
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

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		static extern EventInfo internal_from_handle_type (IntPtr event_handle, IntPtr type_handle);

		internal static EventInfo GetEventFromHandle (Mono.RuntimeEventHandle handle, RuntimeTypeHandle reflectedType)
		{
			if (handle.Value == IntPtr.Zero)
				throw new ArgumentException ("The handle is invalid.");
			EventInfo ei = internal_from_handle_type (handle.Value, reflectedType.Value);
			if (ei == null)
				throw new ArgumentException ("The event handle and the type handle are incompatible.");
			return ei;
		}
	}
}
