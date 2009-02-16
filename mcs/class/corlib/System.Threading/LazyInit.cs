#if NET_4_0
// LazyInit.cs
//
// Copyright (c) 2008 Jérémie "Garuma" Laval
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
//
//

using System;
using System.Threading;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace System.Threading
{
	//FIXME: This should be a struct. In a perfect world made this a intern class and construct the corresponding struct as a wrapper
	[SerializableAttribute]
	public class LazyInit<T>: IEquatable<LazyInit<T>>, ISerializable
	{
		LazyInitMode mode;
		Func<T>      valueSelector;
		Func<bool>   isInitialized;
		Func<T>      specializedValue;
		T            value;
		
		readonly SpinLock spinLock;
		LocalDataStoreSlot localStore;
		
		static bool StartIsInitialized()
		{
			return false;
		}
		
		static bool FinalIsInitialized()
		{
			return true;
		}
		
		T DefaultFinalValue()
		{
			return value;
		}
		
		class DataSlotWrapper
		{
			public bool Init;
			public Func<T> Getter;
		}
		
		public LazyInit(Func<T> valueSelector): this(valueSelector, LazyInitMode.AllowMultipleExecution)
		{
		}
		
		public LazyInit(Func<T> valueSelector, LazyInitMode mode)
		{
			// Common initialization
			this.valueSelector = valueSelector;
			this.mode = mode;
			specializedValue = null;
			value = default(T);
			spinLock = new SpinLock(false);
			localStore = null;
			isInitialized = StartIsInitialized;
			
			// Depending on mode, do specific initialization
			switch (mode) {
				case LazyInitMode.AllowMultipleExecution:
					InitAllowMultipleExecution();
					break;
				case LazyInitMode.EnsureSingleExecution:
					InitEnsureSingleExecution();
					break;
				case LazyInitMode.ThreadLocal:
					InitThreadLocal();					
					break;
			}
		}
		
		void InitAllowMultipleExecution()
		{
			specializedValue = GetValueMultipleExecution;
		}
		
		void InitEnsureSingleExecution()
		{
			specializedValue = GetValueSingleExecution;
		}
		
		void InitThreadLocal()
		{
			localStore = Thread.AllocateDataSlot();
			
			specializedValue = GetValueThreadLocal;
			
			isInitialized = IsInitializedThreadLocal;
		}
		
		T GetValueMultipleExecution()
		{
			value = valueSelector();
			isInitialized = FinalIsInitialized;
			specializedValue = DefaultFinalValue;
			return value;
		}
		
		T GetValueSingleExecution()
		{
			try {
				spinLock.Enter();
				if (!isInitialized()) {
					isInitialized = FinalIsInitialized;
					value = valueSelector();
					specializedValue = DefaultFinalValue;
				}
			} finally { spinLock.Exit(); }
			
			return value;
		}
		
		T GetValueThreadLocal()
		{
			DataSlotWrapper myWrapper = Thread.GetData(localStore) as DataSlotWrapper;
			// In case it's the first time the Thread access its data
			if (myWrapper == null) {
				myWrapper = DataSlotCreator();
				Thread.SetData(localStore, myWrapper);
			}
				
			return myWrapper.Getter();
		}
		
		bool IsInitializedThreadLocal()
		{
			DataSlotWrapper myWrapper = (DataSlotWrapper)Thread.GetData(localStore);
			if (myWrapper == null) {
				myWrapper = DataSlotCreator();
				Thread.SetData(localStore, myWrapper);
			}
			
			return myWrapper.Init;
		}

		DataSlotWrapper DataSlotCreator()
		{
			DataSlotWrapper wrapper = new DataSlotWrapper();
			Func<T> valSelector = valueSelector;
	
			wrapper.Getter = delegate {
				T val = valSelector();
				wrapper.Init = true;
				wrapper.Getter = delegate { return val; };
				return val;
			};
			
			return wrapper;
		}
		
		void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
		{
			throw new NotImplementedException ();
		}
		
		public bool Equals (LazyInit<T> other)
		{
			// TODO: Find its it's correct or not via unit tests
			return mode == other.mode && valueSelector == other.valueSelector;
		}
		
		public override bool Equals (object other)
		{
			if (!(other is LazyInit<T>))
				return false;
			
			LazyInit<T> temp = (LazyInit<T>)other;
			return Equals(temp);
		}
		
		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}
		
		public override string ToString()
		{
			return Value.ToString();
		}
		
		public T Value {
			get {
				Func<T> temp = specializedValue;
				return temp();
			}
		}
		
		public LazyInitMode Mode {
			get {
				return mode;
			}
		}
		
		public bool IsInitialized {
			get {
				Func<bool> temp = isInitialized;
				return temp();
			}
		}
	}
}
#endif
