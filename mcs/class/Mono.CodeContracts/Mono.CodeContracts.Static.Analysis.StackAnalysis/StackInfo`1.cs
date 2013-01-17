// 
// StackInfo`1.cs
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
	struct StackInfo<T> {
		private readonly T[] stack;
		private int depth;

		public StackInfo (int depth, int capacity)
		{
			this.depth = depth;
			this.stack = new T[capacity];
		}

		public StackInfo (StackInfo<T> that)
		{
			this.depth = that.depth;
			this.stack = (T[]) that.stack.Clone ();
		}

		public int Depth { 
			get { return this.depth; }
		}

		public T this [int offset] {
			get {
				int index = this.depth - 1 - offset;
				if (index >= 0 && index < this.stack.Length)
					return this.stack [index];
				return default(T);
			}
		}

		public StackInfo<T> Pop (int slots)
		{
			for (int i = this.depth - slots; i < this.depth; ++i) {
				if (i < this.stack.Length)
					this.stack [i] = default(T);
			}
			this.depth -= slots;
			return this;
		}

		public void Push (T info)
		{
			int index = this.depth;
			if (index < this.stack.Length)
				this.stack [index] = info;
			++this.depth;
		}

		public override string ToString ()
		{
			return this.depth.ToString ();
		}
	}
}
