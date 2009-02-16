#if NET_4_0
// WriteOnce.cs
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

namespace System.Threading
{
	[SerializableAttribute]
	public struct WriteOnce<T>: IEquatable<WriteOnce<T>>
	{
		T value;
		AtomicBoolean setFlag;
		bool hasValue;
		
		public WriteOnce(T value)
		{
			this.value = value;
			this.setFlag = true;
			this.hasValue = true;
		}
		
		public bool HasValue {
			get {
				return hasValue;
			}
		}
		
		public T Value {
			get {
				if (!HasValue) {
					if (setFlag.Value) {
						SpinWait wait = new SpinWait();
						while (!HasValue) wait.SpinOnce();
					} else {
						throw new InvalidOperationException("An attempt was made to retrieve the value, but no value had been set.");
					}
				}
				return value;
			}
			set {
				bool result = setFlag.Exchange(true);
				if (result)
					throw new InvalidOperationException("An attempt was made to set the value when the value was already set.");
				this.value = value;
				hasValue = true;
			}
		}
		
		public bool TryGetValue(out T val)
		{
			bool result = HasValue;
			val = result ? value : default(T);
			return result;
		}
		
		public bool TrySetValue(T val)
		{
			bool result = setFlag.Exchange(true);
			if (result)
				return false;
			value = val;
			hasValue = true;
			return true;
		}
		
		public bool Equals(WriteOnce<T> other)
		{
			return value == null ? other.value == null : value.Equals(other.value);
		}
		
		public override bool Equals(object other)
		{
			return (other is WriteOnce<T>) ? Equals((WriteOnce<T>)other) : false;
		}
		
		public override int GetHashCode()
		{
			return (HasValue) ? value.GetHashCode() : base.GetHashCode();
		}
		
		public override string ToString()
		{
			return (HasValue) ? value.ToString() : base.ToString();
		}
	}
}
#endif
