// 
// Update.cs
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

using System;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.SymbolicGraph {
	abstract class Update<TFunc, TAbstractDomain>
		where TFunc : IEquatable<TFunc>, IConstantInfo
		where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain> {
		public Update<TFunc, TAbstractDomain> Next { get; private set; }

		public abstract void Replay (MergeInfo<TFunc, TAbstractDomain> merge);
		public abstract void ReplayElimination (MergeInfo<TFunc, TAbstractDomain> merge);

		public static Update<TFunc, TAbstractDomain> Reverse (Sequence<Update<TFunc, TAbstractDomain>> updates,
		                                                      Sequence<Update<TFunc, TAbstractDomain>> common)
		{
			Update<TFunc, TAbstractDomain> last = null;
			for (; updates != common; updates = updates.Tail) {
				Update<TFunc, TAbstractDomain> head = updates.Head;
				head.Next = last;
				last = head;
			}
			return last;
		}
	}
}
