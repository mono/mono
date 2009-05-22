//
// Lazy.cs
//
// Authors:
//  Zoltan Varga (vargaz@gmail.com)
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

namespace System
{
	[SerializableAttribute]
	[ComVisibleAttribute(false)]
	[HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true, 
							 ExternalThreading = true)]
	public class Lazy<T> : ISerializable
	{
		T value;
		bool inited;
		LazyExecutionMode mode;
		Func<T> factory;
		object monitor;

		public Lazy () : this (LazyExecutionMode.NotThreadSafe) {
		}

		public Lazy (Func<T> valueFactory) : this (valueFactory, LazyExecutionMode.NotThreadSafe) {
		}

		public Lazy (LazyExecutionMode mode) {
			this.mode = mode;
			if (mode != LazyExecutionMode.NotThreadSafe)
				monitor = new Object ();
		}

		public Lazy (Func<T> valueFactory, LazyExecutionMode mode) {
			if (valueFactory == null)
				throw new ArgumentNullException ("valueFactory");
			this.factory = valueFactory;
			this.mode = mode;
			if (mode != LazyExecutionMode.NotThreadSafe)
				monitor = new Object ();
		}

		[MonoTODO]
		protected Lazy (SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void GetObjectData (SerializationInfo info, StreamingContext context) {
			throw new NotImplementedException ();
		}

		public override string ToString () {
			if (!IsValueCreated)
				return "Value is not created";
			return Value.ToString ();
		}

		[MonoTODO]
		public T Value {
			get {
				if (inited)
					return value;

				return InitValue ();
			}
		}

		T InitValue () {
			if (mode == LazyExecutionMode.NotThreadSafe) {
				value = CreateValue ();
				inited = true;
			} else {
				// We treat AllowMultipleThread... as EnsureSingleThread...
				lock (monitor) {
					if (inited)
						return value;
					T v = CreateValue ();
					value = v;
					Thread.MemoryBarrier ();
					inited = true;
				}
			}

			return value;
		}

		T CreateValue () {
			if (factory == null) {
				try {
					return Activator.CreateInstance<T> ();
				} catch (MissingMethodException) {
					throw new MissingMemberException ("The lazily-initialized type does not have a public, parameterless constructor.");
				}
			} else {
				return factory ();
			}
		}			

		public bool IsValueCreated {
			get {
				return inited;
			}
		}
	}		
}
	
#endif