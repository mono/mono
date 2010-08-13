// 
// ThreadLocal.cs
//  
// Author:
//       Jérémie "Garuma" Laval <jeremie.laval@gmail.com>
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
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
	[HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true, 
	                         ExternalThreading = true)]
	public class ThreadLocal<T> : IDisposable
	{
		readonly Func<T> initializer;
		LocalDataStoreSlot localStore;
		Exception cachedException;
		
		class DataSlotWrapper
		{
			public bool Creating;
			public bool Init;
			public Func<T> Getter;
		}
		
		public ThreadLocal () : this (LazyInitializer.GetDefaultCtorValue<T>)
		{
		}

		public ThreadLocal (Func<T> initializer)
		{
			if (initializer == null)
				throw new ArgumentNullException ("initializer");
			
			localStore = Thread.AllocateDataSlot ();
			this.initializer = initializer;
		}
		
		public void Dispose ()
		{
			Dispose (true);
		}
		
		protected virtual void Dispose (bool dispManagedRes)
		{
			
		}
		
		public bool IsValueCreated {
			get {
				ThrowIfNeeded ();
				return IsInitializedThreadLocal ();
			}
		}
		
		public T Value {
			get {
				ThrowIfNeeded ();
				return GetValueThreadLocal ();
			}
			set {
				ThrowIfNeeded ();

				DataSlotWrapper w = GetWrapper ();
				w.Init = true;
				w.Getter = () => value;
			}
		}
		
		public override string ToString ()
		{
			return string.Format ("[ThreadLocal: IsValueCreated={0}, Value={1}]", IsValueCreated, Value);
		}

		
		T GetValueThreadLocal ()
		{
			DataSlotWrapper myWrapper = GetWrapper ();
			if (myWrapper.Creating)
				throw new InvalidOperationException ("The initialization function attempted to reference Value recursively");

			return myWrapper.Getter ();
		}
		
		bool IsInitializedThreadLocal ()
		{
			DataSlotWrapper myWrapper = GetWrapper ();

			return myWrapper.Init;
		}

		DataSlotWrapper GetWrapper ()
		{
			DataSlotWrapper myWrapper = (DataSlotWrapper)Thread.GetData (localStore);
			if (myWrapper == null) {
				myWrapper = DataSlotCreator ();
				Thread.SetData (localStore, myWrapper);
			}

			return myWrapper;
		}

		void ThrowIfNeeded ()
		{
			if (cachedException != null)
				throw cachedException;
		}

		DataSlotWrapper DataSlotCreator ()
		{
			DataSlotWrapper wrapper = new DataSlotWrapper ();
			Func<T> valSelector = initializer;
	
			wrapper.Getter = delegate {
				wrapper.Creating = true;
				try {
					T val = valSelector ();
					wrapper.Creating = false;
					wrapper.Init = true;
					wrapper.Getter = () => val;
					return val;
				} catch (Exception e) {
					cachedException = e;
					throw e;
				}
			};
			
			return wrapper;
		}
	}
}
#endif
