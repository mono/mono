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
	internal struct ComInteropProxyEntry
	{
		public int refcount;
		public WeakReference weakref;

		public ComInteropProxyEntry (int refcount, WeakReference weak)
		{
			this.refcount = refcount;
			this.weakref = weak;
		}
	}

	internal class ComInteropProxy : RealProxy, IRemotingTypeInfo
    {
        #region Sync with object-internals.h
		private __ComObject com_object;
        #endregion
		private string type_name;
		static Hashtable iunknown_hashtable;

		static ComInteropProxy ()
		{
			iunknown_hashtable = new Hashtable ();
		}

		public ComInteropProxy (Type t)
			: base (t)
		{
			// object only created here
			// .ctor is called later
			com_object = __ComObject.CreateRCW (t);
		}

		internal void CacheProxy ()
		{
			// called from unmanaged code after .ctor is invoked
			// we need .ctor to create unmanaged object and thus IUnknown property value
			iunknown_hashtable.Add (com_object.IUnknown, new ComInteropProxyEntry (1, new WeakReference (this)));
		}

        internal ComInteropProxy (IntPtr pUnk)
            : this (pUnk, typeof (__ComObject))
        {
		}

		internal ComInteropProxy (IntPtr pUnk, Type t)
			: base (t)
		{
			com_object = new __ComObject (pUnk);
			iunknown_hashtable.Add (com_object.IUnknown, new ComInteropProxyEntry (1, new WeakReference (this)));
		}

		internal static int ReleaseComObject (__ComObject co)
		{
			if (co == null)
				throw new ArgumentNullException ("co");
			int refcount = -1;
			IntPtr pUnk = co.IUnknown;
			object obj = iunknown_hashtable[pUnk];
			if (obj != null) {
				ComInteropProxyEntry entry = (ComInteropProxyEntry)obj;
				refcount = entry.refcount - 1;
				if (refcount == 0) {
					iunknown_hashtable.Remove (pUnk);
					ComInteropProxy proxy = (ComInteropProxy)entry.weakref.Target;
					if (proxy != null && proxy.com_object != null)
						proxy.com_object.Finalizer ();
				}
				else {
					iunknown_hashtable[pUnk] = new ComInteropProxyEntry (refcount, entry.weakref);
				}
			}
			return refcount;
		}

		internal static ComInteropProxy GetProxy (IntPtr pItf, Type t)
		{
			IntPtr ppv;
			Guid iid = __ComObject.IID_IUnknown;
			int hr = Marshal.QueryInterface (pItf, ref iid, out ppv);
			Marshal.ThrowExceptionForHR (hr);
			object obj = iunknown_hashtable[ppv];
			if (obj == null) {
				return new ComInteropProxy (ppv);
			}
			else {
				ComInteropProxyEntry entry = ((ComInteropProxyEntry)obj);
				WeakReference weak_ref = entry.weakref;
				object target = weak_ref.Target;
				if (target == null) {
					return new ComInteropProxy (ppv);
				}
				iunknown_hashtable[ppv] = new ComInteropProxyEntry (entry.refcount + 1, weak_ref);
				return ((ComInteropProxy)target);
			}
		}

		public override IMessage Invoke (IMessage msg)
		{
			Console.WriteLine ("Invoke");

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

            if (co.GetInterface (fromType) == IntPtr.Zero)
                return false;
            
            return true;
		}
	}
}
