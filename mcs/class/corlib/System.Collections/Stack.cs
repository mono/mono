//
// System.Collections.Stack
//
// Author:
//    Garrett Rooney (rooneg@electricjellyfish.net)
//
// (C) 2001 Garrett Rooney
//

namespace System.Collections {

	[Serializable]
	public class Stack : ICollection, IEnumerable, ICloneable {

		// properties
		private object[] contents;
		private int current = -1;
		private int count = 0;
		private int capacity = 16;
		private int modCount = 0;

		private void Resize(int ncapacity) {
			object[] ncontents = new object[ncapacity];

			Array.Copy(contents, ncontents, count);

			capacity = ncapacity;
			contents = ncontents;
		}

		public Stack() {
			contents = new object[capacity];
		}

		public Stack(ICollection collection) {
			capacity = collection.Count;
			contents = new object[capacity];
			current = capacity - 1;
			count = capacity;

			collection.CopyTo(contents, 0);
		}

		public Stack(int c) {
			capacity = c;
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

		public static Stack Synchronized(Stack s) {
			if (s == null) {
				throw new ArgumentNullException();
			}

			return new SyncStack(s);
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
			return stack;
		}

		public virtual bool Contains(object obj) {
			if (count == 0)
				return false;

			for (int i = 0; i < count; i++) {
				if (contents[i].Equals(obj))
					return true; 
			}

			return false;
		}

		public virtual void CopyTo (Array array, int index) {
			if (array == null) {
				throw new ArgumentNullException();
			}

			if (index < 0) {
				throw new ArgumentOutOfRangeException();
			}

			if (array.Rank > 1 || 
			    index >= array.Length || 
			    count > array.Length - index) {
				throw new ArgumentException();
			}

			for (int i = current; i != -1; i--) {
				array.SetValue(contents[i], 
					       count - (i + 1) + index);
			}
		}

		private class Enumerator : IEnumerator {

			Stack stack;
			private int modCount;
			private int current;

			internal Enumerator(Stack s) {
				// this is odd.  it seems that you need to 
				// start one further ahead than current, since 
				// MoveNext() gets called first when using an 
				// Enumeration...
				stack = s;
				modCount = s.modCount;
				current = s.current + 1;
			}

			public virtual object Current {
				get {
					if (modCount != stack.modCount 
					    || current == -1 
					    || current > stack.count)
						throw new InvalidOperationException();
					return stack.contents[current];
				}
			}

			public virtual bool MoveNext() {
				if (modCount != stack.modCount 
				    || current == -1) {
					throw new InvalidOperationException();
				}

				current--;

				if (current == -1) {
					return false;
				} else {
					return true;
				}
			}

			public virtual void Reset() {
				if (modCount != stack.modCount) {
					throw new InvalidOperationException();
				}

				// start one ahead of stack.current, so the 
				// first MoveNext() will put us at the top
				current = stack.current + 1;
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

		public virtual void Push(Object o) {
			modCount++;

			if (capacity == count) {
				Resize(capacity * 2);
			}

			count++;
			current++;

			contents[current] = o;
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
