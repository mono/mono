//
// System.Collections.Stack
//
// Author:
//    Garrett Rooney (rooneg@electricjellyfish.net)
//
// (C) 2001 Garrett Rooney
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Collections {

	[ComVisible(true)]
	[System.Diagnostics.DebuggerDisplay ("Count={Count}")]
	[System.Diagnostics.DebuggerTypeProxy (typeof (CollectionDebuggerView))]
	[Serializable]
#if INSIDE_CORLIB
	public
#else
	internal
#endif
	class Stack : ICollection, IEnumerable, ICloneable {

		// properties
		private object[] contents;
		private int current = -1;
		private int count;
		private int capacity;
		private int modCount;
			
		const int default_capacity = 16;

		private void Resize(int ncapacity)
		{
			
			ncapacity = Math.Max (ncapacity, default_capacity);
			object[] ncontents = new object[ncapacity];

			Array.Copy(contents, ncontents, count);

			capacity = ncapacity;
			contents = ncontents;
		}

		public Stack ()
		{
			contents = new object[default_capacity];
			capacity = default_capacity;
		}

		public Stack(ICollection col) : this (col == null ? default_capacity : col.Count) {
			if (col == null)
				throw new ArgumentNullException("col");
			
                        // We have to do this because msft seems to call the
                        // enumerator rather than CopyTo. This affects classes
                        // like bitarray.
			foreach (object o in col)
				Push (o);
		}

		public Stack (int initialCapacity)
		{
			if (initialCapacity < 0)
				throw new ArgumentOutOfRangeException ("initialCapacity");
			
			capacity = initialCapacity;
			contents = new object[capacity];
		}

		[Serializable]
		private class SyncStack : Stack {

			Stack stack;

			internal SyncStack(Stack s) {
				stack = s;
			}
			
			public override int Count {
				get { 
					lock (stack) {
						return stack.Count; 
					}
				}
			}
			
/*
			public override bool IsReadOnly {
				get { 
					lock (stack) {
						return stack.IsReadOnly; 
					}
				}
			}
*/
			
			public override bool IsSynchronized {
				get { return true; }
			}
			
			public override object SyncRoot {
				get { return stack.SyncRoot; }
			}

			public override void Clear() {
				lock(stack) { stack.Clear(); }
			}

			public override object Clone() {
				lock (stack) { 
					return Stack.Synchronized((Stack)stack.Clone()); 
				}
			}

			public override bool Contains(object obj) {
				lock (stack) { return stack.Contains(obj); }
			}

			public override void CopyTo(Array array, int index) {
				lock (stack) { stack.CopyTo(array, index); }
			}

			public override IEnumerator GetEnumerator() {
				lock (stack) { 
					return new Enumerator(stack); 
				}
			}

			public override object Peek() {
				lock (stack) { return stack.Peek(); }
			}

			public override object Pop() {
				lock (stack) { return stack.Pop(); }
			}

			public override void Push(object obj) {
				lock (stack) { stack.Push(obj); }
			}

			public override object[] ToArray() {
				lock (stack) { return stack.ToArray(); }
			}
		}

		public static Stack Synchronized (Stack stack)
		{
			if (stack == null)
				throw new ArgumentNullException ("stack");

			return new SyncStack (stack);
		}

		public virtual int Count {
			get { return count; }
		}

/*
		public virtual bool IsReadOnly {
			get { return false; }
		}
*/

		public virtual bool IsSynchronized {
			get { return false; }
		}

		public virtual object SyncRoot {
			get { return this; }
		}

		public virtual void Clear() {
			modCount++;

			for (int i = 0; i < count; i++) {
				contents[i] = null;
			}

			count = 0;
			current = -1;
		}

		public virtual object Clone() {
			Stack stack = new Stack (contents);
			stack.current = current;
			stack.count = count;
			return stack;
		}

		public virtual bool Contains(object obj) {
			if (count == 0)
				return false;
			
			if (obj == null) {
					for (int i = 0; i < count; i++) {
						if (contents[i] == null)
							return true; 
					}
			} else {
					for (int i = 0; i < count; i++) {
						if (obj.Equals (contents[i]))
							return true; 
					}
			}

			return false;
		}

		public virtual void CopyTo (Array array, int index) {
			if (array == null) {
				throw new ArgumentNullException("array");
			}

			if (index < 0) {
				throw new ArgumentOutOfRangeException("index");
			}

			if (array.Rank > 1 || 
			    array.Length > 0 && index >= array.Length || 
			    count > array.Length - index) {
				throw new ArgumentException();
			}

			for (int i = current; i != -1; i--) {
				array.SetValue(contents[i], 
					       count - (i + 1) + index);
			}
		}

		private class Enumerator : IEnumerator, ICloneable {
			
			const int EOF = -1;
			const int BOF = -2;

			Stack stack;
			private int modCount;
			private int current;

			internal Enumerator(Stack s) {
				stack = s;
				modCount = s.modCount;
				current = BOF;
			}
			
			public object Clone ()
			{
				return MemberwiseClone ();
			}

			public virtual object Current {
				get {
					if (modCount != stack.modCount 
					    || current == BOF
					    || current == EOF
					    || current > stack.count)
						throw new InvalidOperationException();
					return stack.contents[current];
				}
			}

			public virtual bool MoveNext() {
				if (modCount != stack.modCount)
					throw new InvalidOperationException();
				
				switch (current) {
				case BOF:
					current = stack.current;
					return current != -1;
				
				case EOF:
					return false;
				
				default:
					current--; 
					return current != -1;
				}
			}

			public virtual void Reset() {
				if (modCount != stack.modCount) {
					throw new InvalidOperationException();
				}

				current = BOF;
			}
		}

		public virtual IEnumerator GetEnumerator() {
			return new Enumerator(this);
		}

		public virtual object Peek() {
			if (current == -1) {
				throw new InvalidOperationException();
			} else {
				return contents[current];
			}
		}

		public virtual object Pop() {
			if (current == -1) {
				throw new InvalidOperationException();
			} else {
				modCount++;

				object ret = contents[current];
				contents [current] = null;
		
				count--;
				current--;

				// if we're down to capacity/4, go back to a 
				// lower array size.  this should keep us from 
				// sucking down huge amounts of memory when 
				// putting large numbers of items in the Stack.
				// if we're lower than 16, don't bother, since 
				// it will be more trouble than it's worth.
				if (count <= (capacity/4) && count > 16) {
					Resize(capacity/2);
				}

				return ret;
			}
		}

		public virtual void Push (Object obj)
		{
			modCount++;

			if (capacity == count) {
				Resize(capacity * 2);
			}

			count++;
			current++;

			contents[current] = obj;
		}

		public virtual object[] ToArray() {
			object[] ret = new object[count];

			Array.Copy(contents, ret, count);

			// ret needs to be in LIFO order
			Array.Reverse(ret);

			return ret;
		}
	}
}
