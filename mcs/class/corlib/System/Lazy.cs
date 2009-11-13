//
// Lazy.cs
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
//  Marek Safar (marek.safar@gmail.com)
//
// Copyright (C) 2009 Novell
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

#if NET_4_0

using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Threading;
using System.Diagnostics;

namespace System
{
	[SerializableAttribute]
	[ComVisibleAttribute(false)]
	[HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
	public class Lazy<T> 
	{
		T value;
		bool inited;
		Func<T> factory;
		object monitor;

		public Lazy ()
			: this (true)
		{
		}

		public Lazy (Func<T> valueFactory)
			: this (valueFactory, true)
		{
		}

		public Lazy (bool isThreadSafe)
			: this (() => Activator.CreateInstance<T> (), isThreadSafe)
		{
		}
		
		public Lazy (Func<T> valueFactory, bool isThreadSafe)
		{
			if (valueFactory == null)
				throw new ArgumentNullException ("valueFactory");
			this.factory = valueFactory;
			if (isThreadSafe)
				monitor = new object ();
		}

		// Don't trigger expensive initialization
		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		public T Value {
			get {
				if (inited)
					return value;

				return InitValue ();
			}
		}

		T InitValue () {
			if (monitor == null) {
				value = factory ();
				inited = true;
			} else {
				lock (monitor) {
					if (inited)
						return value;

					if (factory == null)
						throw new InvalidOperationException ("The initialization function tries to access Value on this instance");

					var init_factory = factory;
					try {
						factory = null;
						T v = init_factory ();
						value = v;
						Thread.MemoryBarrier ();
						inited = true;
					} catch {
						factory = init_factory;
						throw;
					}
				}
			}

			return value;
		}

		public bool IsValueCreated {
			get {
				return inited;
			}
		}

		public override string ToString ()
		{
			if (inited)
				return value.ToString ();
			else
				return "Value is not created";
		}
	}		
}
	
#endif
