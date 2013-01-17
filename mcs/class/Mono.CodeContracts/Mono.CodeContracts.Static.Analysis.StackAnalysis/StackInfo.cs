// 
// StackInfo.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2011 Alexander Chebaturkin
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

namespace Mono.CodeContracts.Static.Analysis.StackAnalysis {
	struct StackInfo {
		private StackInfo<object> stack;

		public int Depth {
			get { return this.stack.Depth; }
		}

		public object this [int offset] {
			get { return this.stack [offset]; }
		}

		public StackInfo (int depth, int capacity)
		{
			this.stack = new StackInfo<object> (depth, capacity);
		}

		private StackInfo (StackInfo<object> copy)
		{
			this.stack = copy;
		}

		public StackInfo Pop (int slots)
		{
			return new StackInfo (this.stack.Pop (slots));
		}

		public StackInfo Push ()
		{
			this.stack.Push (null);
			return this;
		}

		public StackInfo PushThis ()
		{
			this.stack.Push (true);
			return this;
		}

		public StackInfo Push<T> (T target)
		{
			this.stack.Push (target);
			return this;
		}

		public void Adjust (int delta)
		{
			if (delta == 0)
				return;
			if (delta < 0)
				this.stack.Pop (-delta);
			for (int i = 0; i < delta; ++i)
				Push ();
		}

		public bool IsThis (int offset)
		{
			return As<bool> (offset);
		}

		public bool TryGet<T> (int offset, out T target)
		{
			object o = this [offset];
			if (o is T) {
				target = (T) o;
				return true;
			}
			target = default(T);
			return false;
		}

		private T As<T> (int offset)
		{
			T res;
			TryGetTarget (offset, out res);
			return res;
		}

		public StackInfo Clone ()
		{
			return new StackInfo (new StackInfo<object> (this.stack));
		}

		public override string ToString ()
		{
			return this.stack.ToString ();
		}

		public bool TryGetTarget<T> (int offset, out T target)
		{
			if (this [offset] is T) {
				target = (T) this [offset];
				return true;
			}
			target = default(T);
			return false;
		}
	}
}
