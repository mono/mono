// -*- Mode: C; tab-width: 8; c-basic-offset: 8 -*-
//
// System.Collections.Stack
//
// Author:
//    Garrett Rooney (rooneg@electricjellyfish.net)
//
// (C) 2001 Garrett Rooney
//

namespace System.Collections {

	public class Stack : ICollection, IEnumerable, ICloneable {

		// properties
		private object[] contents;
		private int current = -1;
		private int count = 0;
		private int capacity = 16;

		private bool readOnly = false;
		private bool synchronized = false;

		private void Resize(int ncapacity) {
			object[] ncontents = new object[ncapacity];

			for (int i = 0; i < capacity; i++) {
				ncontents[i] = contents[i];
			}

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

			int i = 0;
			foreach (object o in collection) {
				contents[i++] = o;
			}
		}

		public Stack(int c) {
			capacity = c;
			contents = new object[capacity];
		}

		// The Synchronized version of Stack uses lock(this) to make 
		// it thread safe.  This should be ok, even though we wrap an 
		// Array, which is a Collection, since there is no way for the 
		// outside world to get a reference to the Array.  If I'm 
		// wrong about this, then we should change lock(this) to 
		// lock(contents.SyncRoot).
		private class SyncStack : Stack {

			public SyncStack(Stack s) {
				contents = s.contents;
				current = s.current;
				count = s.count;
				capacity = s.capacity;
				readOnly = s.readOnly;
				synchronized = true;
			}
			
			public override int Count {
				get { lock(this) { return count; } }
			}
			
			public override bool IsReadOnly {
				get { lock(this) { return readOnly; } }
			}

			public override bool IsSynchronized {
				get { lock(this) { return synchronized; } }
			}
			
			public override object SyncRoot {
				get { lock(this) { return this; } }
			}

			public override void Clear() {
				lock(this) { base.Clear(); }
			}

			public override object Clone() {
				lock (this) { return base.Clone(); }
			}

			public override bool Contains(object obj) {
				lock (this) { return base.Contains(obj); }
			}

			public override void CopyTo(Array array, int index) {
				lock (this) { base.CopyTo(array, index); }
			}

			// As noted above, this uses lock(this), and if that 
			// turns out to be unsafe, it should be changed to 
			// lock(contents.SyncRoot).
			private class SyncEnumerator : Enumerator {

				internal SyncEnumerator(Stack s) : base(s) {}

				public override object Current {
					get {
						lock (this) {
							return base.Current; 
						}
					}
				}

				public override bool MoveNext() {
					lock (this) { return base.MoveNext(); }
				}

				public override void Reset() {
					lock (this) { base.Reset(); }
				}
			}
			
			public override IEnumerator GetEnumerator() {
				lock (this) { 
					return new SyncEnumerator(this); 
				}
			}

			public override object Peek() {
				lock (this) { return base.Peek(); }
			}

			public override object Pop() {
				lock (this) { return base.Pop(); }
			}

			public override void Push(object obj) {
				lock (this) { base.Push(obj); }
			}

			public override object[] ToArray() {
				lock (this) { return base.ToArray(); }
			}
		}

		public static Stack Syncronized(Stack s) {
			if (s == null) {
				throw new ArgumentNullException();
			}

			return new SyncStack(s);
		}

		public virtual int Count {
			get { return count; }
		}

		public virtual bool IsReadOnly {
			get { return readOnly; }
		}

		public virtual bool IsSynchronized {
			get { return synchronized; }
		}

		// If using this for the SyncRoot is unsafe, we should use 
		// contents.SyncRoot instead.  I think this is ok though.
		public virtual object SyncRoot {
			get { return this; }
		}

		public virtual void Clear() {
			for (int i = 0; i < count; i++) {
				contents[i] = null;
			}

			count = 0;
			current = -1;
		}

		public virtual object Clone() {
			Stack stack;

			if (synchronized == false) {
				stack = new Stack();

				stack.current = current;
				stack.contents = contents;
				stack.count = count;
				stack.capacity = capacity;
				stack.readOnly = readOnly;
				stack.synchronized = synchronized;
			} else {
				stack = new SyncStack(this);
			}

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

		// I made several methods of this class virtual, so that they 
		// could be overriden by a thread safe version of the 
		// Enumerator for use by SyncStack.  I don't know if MS does 
		// that in their implimentation, but it seemed like one should 
                // reasonably be able to expect a thread safe Collection to 
		// return a thread safe Enumerator.  If this is a problem, it 
		// could be ripped out.  Realistically speaking, I doubt if 
		// many people would ever notice if the Enumerator was thread 
		// safe, as I cannot concieve of a situation where an 
		// Enumerator would be accessed by more than one thread.
		private class Enumerator : IEnumerator {

			private Array contents;
			private int current;
			private int count;
			private int begin;

			internal Enumerator(Stack s) {
				// this is odd.  it seems that you need to 
				// start one further ahead than current, since 
				// MoveNext() gets called first when using an 
				// Enumeration...
				begin = s.current + 1;
				current = begin;
				count = s.count;
				contents = (Array) s.contents.Clone();
			}

			public virtual object Current {
				get {
					if (current == -1 || current > count)
						throw new InvalidOperationException();
					return contents.GetValue(current);
				}
			}

			public virtual bool MoveNext() {
				if (current == -1) {
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
				// start one ahead of stack.current, so the 
				// first MoveNext() will put us at the top
				current = begin;
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
				object ret = contents[current];
		
				count--;
				current--;
		
				return ret;
			}
		}

		// FIXME: We should probably be a bit smarter about our 
		// resizing.  After a certain point, doubling isn't that smart.
		// We just need to find out what that point is...
		public virtual void Push(Object o) {
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

