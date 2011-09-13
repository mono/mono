//
// System.__ComObject
//
// Authors:
//   Sebastien Pouliot <sebastien@ximian.com>
//   Kornél Pál <http://www.kornelpal.hu/>
//   Jonathan Chambers <joncham@gmail.com>
//
// Copyright (C) 2004 Novell (http://www.novell.com)
// Copyright (C) 2005 Kornél Pál
// Copyright (C) 2006 Jonathan Chambers
//

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

using Mono.Interop;
using System.Collections;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System
{
	// This is a private class that is used as a generic wrapper class
	// for COM objects that have no specific wrapper class.
	//
	// It has no public methods, it's functionality is exposed trough
	// System.Runtime.InteropServices.Marshal class and can be casted to
	// any interface that is implemented by the wrapped COM object.
	//
	// This class is referenced in .NET Framework SDK Documentation so
	// many times that obj.GetType().FullName == "System.__ComObject" and
	// Type.GetType("System.__ComObject") may be used.

	internal class __ComObject : MarshalByRefObject
	{
#pragma warning disable 169	
		#region Sync with object-internals.h
		IntPtr iunknown;
		IntPtr hash_table;
		SynchronizationContext synchronization_context;
		#endregion
#pragma warning restore 169

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal static extern __ComObject CreateRCW (Type t);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern void ReleaseInterfaces ();

		~__ComObject ()
		{	
			if (synchronization_context != null)
				synchronization_context.Post ((state) => ReleaseInterfaces (), this);
			else
				ReleaseInterfaces ();				
		}

		public __ComObject ()
		{
			Initialize (GetType ());
		}

		internal __ComObject (Type t) {
			Initialize (t);
		}

		internal __ComObject (IntPtr pItf)
		{
			InitializeApartmentDetails ();
			Guid iid = IID_IUnknown;
			int hr = Marshal.QueryInterface (pItf, ref iid, out iunknown);
			Marshal.ThrowExceptionForHR (hr);
		}

		internal void Initialize (Type t)
		{
			InitializeApartmentDetails ();
			// Guard multiple invocation.
			if (iunknown != IntPtr.Zero)
				return;
			
			ObjectCreationDelegate ocd = ExtensibleClassFactory.GetObjectCreationCallback (t);
			if (ocd != null) {
				iunknown = ocd (IntPtr.Zero);
				if (iunknown == IntPtr.Zero)
					throw new COMException (string.Format("ObjectCreationDelegate for type {0} failed to return a valid COM object", t));
			}
			else {
				int hr = CoCreateInstance (GetCLSID (t), IntPtr.Zero, 0x1 | 0x4 | 0x10, IID_IUnknown, out iunknown);
				Marshal.ThrowExceptionForHR (hr);
			}
		}

		private void InitializeApartmentDetails ()
		{
			// Only synchronization_context if thread is STA.
			if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
				return;
			
			synchronization_context = SynchronizationContext.Current;

			// Check whether the current context is a plain SynchronizationContext object
			// and handle this as if no context was set at all.
			if (synchronization_context != null &&
				synchronization_context.GetType () == typeof(SynchronizationContext))
				synchronization_context = null;			
		}

		private static Guid GetCLSID (Type t)
		{
			if (t.IsImport)
				return t.GUID;

			// look at supertypes
			Type super = t.BaseType;
			while (super != typeof (object)) {
				if (super.IsImport)
					return super.GUID;
				super = super.BaseType;
			}
			throw new COMException ("Could not find base COM type for type " + t.ToString());
		}

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern IntPtr GetInterfaceInternal (Type t, bool throwException);

		internal IntPtr GetInterface (Type t, bool throwException) {
			CheckIUnknown ();
			return GetInterfaceInternal (t, throwException);
		}

		internal IntPtr GetInterface(Type t)
		{
			return GetInterface (t, true);
		}

		private void CheckIUnknown ()
		{
			if (iunknown == IntPtr.Zero)
				throw new InvalidComObjectException ("COM object that has been separated from its underlying RCW cannot be used.");
		}

		internal IntPtr IUnknown
		{
			get
			{
				if (iunknown == IntPtr.Zero)
					throw new InvalidComObjectException ("COM object that has been separated from its underlying RCW cannot be used.");
				return iunknown;
			}
		}

		internal IntPtr IDispatch
		{
			get
			{
				IntPtr pUnk = GetInterface (typeof (IDispatch));
				if (pUnk == IntPtr.Zero)
					throw new InvalidComObjectException ("COM object that has been separated from its underlying RCW cannot be used.");
				return pUnk;
			}
		}

		internal static Guid IID_IUnknown
		{
			get
			{
				return new Guid("00000000-0000-0000-C000-000000000046");
			}
		}

		internal static Guid IID_IDispatch
		{
			get
			{
				return new Guid ("00020400-0000-0000-C000-000000000046");
			}
		}

		public override bool Equals (object obj)
		{
			CheckIUnknown ();
			if (obj == null)
				return false;

			__ComObject co = obj as __ComObject;
			if ((object)co == null)
				return false;
			return (iunknown == co.IUnknown);
		}

		public override int GetHashCode ()
		{
			CheckIUnknown ();
			// not what MS seems to do, 
			// but IUnknown is identity in COM
			return iunknown.ToInt32 ();
		}

		[DllImport ("ole32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true, PreserveSig = true)]
		static extern int CoCreateInstance (
		   [In, MarshalAs (UnmanagedType.LPStruct)] Guid rclsid,
		   IntPtr pUnkOuter,
		   uint dwClsContext,
		  [In, MarshalAs (UnmanagedType.LPStruct)] Guid riid,
			out IntPtr pUnk);
	}
}
