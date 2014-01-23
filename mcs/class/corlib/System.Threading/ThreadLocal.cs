// 
// ThreadLocal.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
//       Rewritten by Paolo Molaro (lupus@ximian.com)
// 
// Copyright (c) 2009 Jérémie "Garuma" Laval
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

#if NET_4_0
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
	[HostProtection (SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	[System.Diagnostics.DebuggerDisplay ("IsValueCreated={IsValueCreated}, Value={ValueForDebugDisplay}")]
	[System.Diagnostics.DebuggerTypeProxy ("System.Threading.SystemThreading_ThreadLocalDebugView`1")]
	public class ThreadLocal<T> : IDisposable
	{
		struct TlsDatum {
			internal sbyte state; /* 0 uninitialized, < 0 initializing, > 0 inited */
			internal Exception cachedException; /* this is per-thread */
			internal T data;
		}

		Func<T> valueFactory;
		/* The tlsdata field is handled magically by the JIT
		 * It must be a struct and it is always accessed by ldflda: the JIT, instead of
		 * computing the address inside the instance, will return the address of the variable
		 * for the current thread (based on tls_offset). This magic wouldn't be needed if C#
		 * let us declare an icall with a TlsDatum& return type...
		 * For this same reason, we must check tls_offset for != 0 to make sure it's valid before accessing tlsdata
		 * The address of the tls var is cached per method at the first IL ldflda instruction, so care must be taken
		 * not to cause it to be conditionally executed.
		 */
		uint tls_offset;
		TlsDatum tlsdata;
		
		public ThreadLocal ()
		{
			tls_offset = Thread.AllocTlsData (typeof (TlsDatum));
		}

		public ThreadLocal (Func<T> valueFactory) : this ()
		{
			if (valueFactory == null)
				throw new ArgumentNullException ("valueFactory");
			this.valueFactory = valueFactory;
		}

#if NET_4_5
		public ThreadLocal (bool trackAllValues) : this () {
			if (trackAllValues)
				throw new NotImplementedException ();
		}

		public ThreadLocal (Func<T> valueFactory, bool trackAllValues) : this (valueFactory) {
			if (trackAllValues)
				throw new NotImplementedException ();
		}
#endif

		public void Dispose ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool disposing)
		{
			if (tls_offset != 0) {
				uint o = tls_offset;
				tls_offset = 0;
				if (disposing)
					valueFactory = null;
				Thread.DestroyTlsData (o);
				GC.SuppressFinalize (this);
			}
		}

		~ThreadLocal ()
		{
			Dispose (false);
		}
		
		public bool IsValueCreated {
			get {
				if (tls_offset == 0)
					throw new ObjectDisposedException ("ThreadLocal object");
				/* ALERT! magic tlsdata JIT access redirects to TLS value instead of instance field */
				return tlsdata.state > 0;
			}
		}

		T GetSlowPath () {
			/* ALERT! magic tlsdata JIT access redirects to TLS value instead of instance field */
			if (tlsdata.cachedException != null)
				throw tlsdata.cachedException;
			if (tlsdata.state < 0)
				throw new InvalidOperationException ("The initialization function attempted to reference Value recursively");
			tlsdata.state = -1;
			if (valueFactory != null) {
				try {
					tlsdata.data = valueFactory ();
				} catch (Exception ex) {
					tlsdata.cachedException = ex;
					throw ex;
				}
			} else {
				tlsdata.data = default (T);
			}
			tlsdata.state = 1;
			return tlsdata.data;
		}

		[System.Diagnostics.DebuggerBrowsableAttribute (System.Diagnostics.DebuggerBrowsableState.Never)]
		public T Value {
			get {
				if (tls_offset == 0)
					throw new ObjectDisposedException ("ThreadLocal object");
				/* ALERT! magic tlsdata JIT access redirects to TLS value instead of instance field */
				if (tlsdata.state > 0)
					return tlsdata.data;
				return GetSlowPath ();
			}
			set {
				if (tls_offset == 0)
					throw new ObjectDisposedException ("ThreadLocal object");
				/* ALERT! magic tlsdata JIT access redirects to TLS value instead of instance field */
				tlsdata.state = 1;
				tlsdata.data = value;
			}
		}

#if NET_4_5
		public IList<T> Values {
			get {
				if (tls_offset == 0)
					throw new ObjectDisposedException ("ThreadLocal object");
				throw new NotImplementedException ();
			}
		}
#endif

		public override string ToString ()
		{
			return string.Format ("[ThreadLocal: IsValueCreated={0}, Value={1}]", IsValueCreated, Value);
		}
		
	}
}
#endif
