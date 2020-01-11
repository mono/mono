//
// Mono.Interop.ComInteropProxy
//
// Authors:
//   Jonathan Chambers <joncham@gmail.com>
//
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
#if !FULL_AOT_RUNTIME
using System;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Runtime.InteropServices;


namespace Mono.Interop
{
	[StructLayout (LayoutKind.Sequential)]
	internal class ComInteropProxy : RealProxy, IRemotingTypeInfo
    {
        #region Sync with object-internals.h
		private __ComObject com_object;
		int ref_count = 1; // wrapper ref count
        #endregion
		private string type_name;

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		private extern static void AddProxy (IntPtr pItf, ref ComInteropProxy proxy);

		[MethodImplAttribute (MethodImplOptions.InternalCall)]
		internal extern static void FindProxy (IntPtr pItf, ref ComInteropProxy proxy);

		// Private. Objects must be created with CreateProxy.
		ComInteropProxy (Type t)
			: base (t)
		{
			// object only created here
			// .ctor is called later
			com_object = __ComObject.CreateRCW (t);
		}

		void CacheProxy ()
		{
			// called from unmanaged code after .ctor is invoked
			// we need .ctor to create unmanaged object and thus IUnknown property value
			ComInteropProxy proxy = null;
			FindProxy (com_object.IUnknown, ref proxy);
			if (proxy == null) {
				var self = this;
				AddProxy (com_object.IUnknown, ref self);
			}
			else
				System.Threading.Interlocked.Increment (ref ref_count);
		}

		ComInteropProxy (IntPtr pUnk)
			: this (pUnk, typeof (__ComObject))
		{
		}

		internal ComInteropProxy (IntPtr pUnk, Type t)
			: base (t)
		{
			com_object = new __ComObject (pUnk, this);
			CacheProxy ();
		}

		internal static ComInteropProxy GetProxy (IntPtr pItf, Type t)
		{
			IntPtr ppv;
			Guid iid = __ComObject.IID_IUnknown;
			int hr = Marshal.QueryInterface (pItf, ref iid, out ppv);
			Marshal.ThrowExceptionForHR (hr);
			ComInteropProxy obj = null;
			FindProxy (ppv, ref obj);
			if (obj == null) {
				Marshal.Release (ppv);
				return new ComInteropProxy (ppv);
			}
			else {
				Marshal.Release (ppv);
				System.Threading.Interlocked.Increment (ref obj.ref_count);
				return obj;
			}
		}

		// Gets the proxy of the specified COM type. If the COM type is
		// already known, a cached proxy will be returned.
		internal static ComInteropProxy CreateProxy (Type t)
		{
			IntPtr iunknown = __ComObject.CreateIUnknown (t);
			ComInteropProxy proxy;
			ComInteropProxy cachedProxy = null;
			FindProxy (iunknown, ref cachedProxy);
			if (cachedProxy != null) {
				// check that the COM type of the cached proxy matches
				// the requested type. See 2nd part of bug #520437.
				Type cachedType = cachedProxy.com_object.GetType ();
				if (cachedType != t)
					throw new InvalidCastException (String.Format ("Unable to cast object of type '{0}' to type '{1}'.", cachedType, t));
				proxy = cachedProxy;
				Marshal.Release (iunknown);
			} else {
				proxy = new ComInteropProxy (t);
				proxy.com_object.Initialize (iunknown, proxy);
			}
			return proxy;
		}

		public override IMessage Invoke (IMessage msg)
		{
			throw new Exception ("The method or operation is not implemented.");
		}

		public string TypeName
		{
			get { return type_name; }
			set { type_name = value; }
		}

		public bool CanCastTo (Type fromType, object o)
		{
            __ComObject co = o as __ComObject;
            if (co == null)
                throw new NotSupportedException ("Only RCWs are currently supported");

            if ((fromType.Attributes & TypeAttributes.Import) == 0)
                return false;

            if (co.GetInterface (fromType, false) == IntPtr.Zero)
                return false;
            
            return true;
		}
	}
}
#endif
