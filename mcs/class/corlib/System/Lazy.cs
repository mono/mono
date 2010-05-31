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

#if NET_4_0 || MOONLIGHT

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
		LazyThreadSafetyMode mode;
		Func<T> factory;
		object monitor;
		Exception exception;

		public Lazy ()
			: this (LazyThreadSafetyMode.ExecutionAndPublication)
		{
		}

		public Lazy (Func<T> valueFactory)
			: this (valueFactory, LazyThreadSafetyMode.ExecutionAndPublication)
		{
		}

		public Lazy (bool isThreadSafe)
			: this (() => Activator.CreateInstance<T> (), isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
		{
		}
		
		public Lazy (Func<T> valueFactory, bool isThreadSafe)
			: this (valueFactory, isThreadSafe ? LazyThreadSafetyMode.ExecutionAndPublication : LazyThreadSafetyMode.None)
		{
		}

		public Lazy (LazyThreadSafetyMode mode)
			: this (() => Activator.CreateInstance<T> (), mode)
		{
		}

		

		public Lazy (Func<T> valueFactory, LazyThreadSafetyMode mode)
		{
			if (valueFactory == null)
				throw new ArgumentNullException ("valueFactory");
			this.factory = valueFactory;
			if (mode != LazyThreadSafetyMode.None)
				monitor = new object ();
			this.mode = mode;
		}

		// Don't trigger expensive initialization
		[DebuggerBrowsable (DebuggerBrowsableState.Never)]
		public T Value {
			get {
				if (inited)
					return value;
				if (exception != null)
					throw exception;

				return InitValue ();
			}
		}

		T InitValue ()
		{
			Func<T> init_factory;
			T v;
			
			switch (mode) {
			case LazyThreadSafetyMode.None:
				init_factory = factory;
				if (init_factory == null) 
					throw exception = new InvalidOperationException ("The initialization function tries to access Value on this instance");
				try {
					factory = null;
					v = init_factory ();
					value = v;
					Thread.MemoryBarrier ();
					inited = true;
				} catch (Exception ex) {
					exception = ex;
					throw;
				}
				break;

			case LazyThreadSafetyMode.PublicationOnly:
				init_factory = factory;

				//exceptions are ignored
				if (init_factory != null)
					v = init_factory ();
				else
					v = default (T);

				lock (monitor) {
					if (inited)
						return value;
					value = v;
					Thread.MemoryBarrier ();
					inited = true;
					factory = null;
				}
				break;

			case LazyThreadSafetyMode.ExecutionAndPublication:
				lock (monitor) {
					if (inited)
						return value;

					if (factory == null)
						throw exception = new InvalidOperationException ("The initialization function tries to access Value on this instance");

					init_factory = factory;
					try {
						factory = null;
						v = init_factory ();
						value = v;
						Thread.MemoryBarrier ();
						inited = true;
					} catch (Exception ex) {
						exception = ex;
						throw;
					}
				}
				break;

			default:
				throw new InvalidOperationException ("Invalid LazyThreadSafetyMode " + mode);
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
