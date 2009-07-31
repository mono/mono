#if NET_4_0
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

using System;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Security.Permissions;

namespace System.Threading
{
	[HostProtectionAttribute(SecurityAction.LinkDemand, Synchronization = true, 
	                         ExternalThreading = true)]
	public class ThreadLocal<T>
	{
		readonly Func<T> initializer;
		LocalDataStoreSlot localStore;
		
		class DataSlotWrapper
		{
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
		
		public bool IsValueCreated {
			get {
				return IsInitializedThreadLocal ();
			}
		}
		
		public T Value {
			get {
				return GetValueThreadLocal ();
			}
		}
		
		public override string ToString ()
		{
			return string.Format("[ThreadLocal: IsValueCreated={0}, Value={1}]", IsValueCreated, Value);
		}

		
		T GetValueThreadLocal ()
		{
			DataSlotWrapper myWrapper = Thread.GetData (localStore) as DataSlotWrapper;
			// In case it's the first time the Thread access its data
			if (myWrapper == null) {
				myWrapper = DataSlotCreator ();
				Thread.SetData (localStore, myWrapper);
			}
				
			return myWrapper.Getter();
		}
		
		bool IsInitializedThreadLocal ()
		{
			DataSlotWrapper myWrapper = (DataSlotWrapper)Thread.GetData (localStore);
			if (myWrapper == null) {
				myWrapper = DataSlotCreator ();
				Thread.SetData (localStore, myWrapper);
			}
			
			return myWrapper.Init;
		}

		DataSlotWrapper DataSlotCreator ()
		{
			DataSlotWrapper wrapper = new DataSlotWrapper ();
			Func<T> valSelector = initializer;
	
			wrapper.Getter = delegate {
				T val = valSelector ();
				wrapper.Init = true;
				wrapper.Getter = delegate { return val; };
				return val;
			};
			
			return wrapper;
		}
	}
}
#endif
