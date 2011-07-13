// 
// MultiEdgeUpdate.cs
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

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.SymbolicGraph {
	class MultiEdgeUpdate<TFunc, TAbstractDomain> : Update<TFunc, TAbstractDomain>
		where TFunc : IEquatable<TFunc>, IConstantInfo
		where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain> {
		private readonly SymValue[] from;
		private readonly TFunc function;

		public MultiEdgeUpdate (SymValue[] from, TFunc function)
		{
			this.function = function;
			this.from = from;
		}

		#region Overrides of Update
		public override void Replay (MergeInfo<TFunc, TAbstractDomain> merge)
		{
			int len = this.from.Length;
			for (int i = 0; i < len; i++) {
				SymValue sv = this.from [i];
				if (merge.IsCommon (sv))
					merge.JoinMultiEdge (sv, sv, new MultiEdge<TFunc, TAbstractDomain> (this.function, i, len));
			}
		}

		public override void ReplayElimination (MergeInfo<TFunc, TAbstractDomain> merge)
		{
		}
		#endregion
	}
}
