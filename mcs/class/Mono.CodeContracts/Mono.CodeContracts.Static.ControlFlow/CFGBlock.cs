// 
// CFGBlock.cs
// 
// Authors:
// 	Alexander Chebaturkin (chebaturkin@gmail.com)
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

using System.Collections.Generic;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow {
	abstract class CFGBlock {
		public int Index;

		protected CFGBlock (Subroutine subroutine, ref int idGen)
		{
			this.Index = idGen++;
			Subroutine = subroutine;
		}

		public abstract int Count { get; }
		public Subroutine Subroutine { get; private set; }
		public int ReversePostOrderIndex { get; set; }

		public APC First
		{
			get { return APC.ForStart (this, null); }
		}

		public APC Last
		{
			get { return APC.ForEnd (this, null); }
		}

		public virtual bool IsMethodCallBlock<TMethod> (out TMethod calledMethod, out bool isNewObj, out bool isVirtual)
		{
			calledMethod = default(TMethod);
			isNewObj = false;
			isVirtual = false;

			return false;
		}

		public void Renumber (ref int idGen)
		{
			this.Index = idGen++;
		}

		public abstract int GetILOffset (APC pc);

		public IEnumerable<APC> APCs ()
		{
			return APCs (null);
		}

		private IEnumerable<APC> APCs (Sequence<Edge<CFGBlock, EdgeTag>> context)
		{
			for (int i = 0; i < Count; i++)
				yield return new APC (this, i, context);
		}
	}
}
