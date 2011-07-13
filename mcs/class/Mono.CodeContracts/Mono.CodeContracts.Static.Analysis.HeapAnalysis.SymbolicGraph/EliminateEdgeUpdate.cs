// 
// EliminateEdgeUpdate.cs
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
	class EliminateEdgeUpdate<TFunc, TAbstractDomain> : Update<TFunc, TAbstractDomain>
		where TFunc : IEquatable<TFunc>, IConstantInfo
		where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain> {
		private readonly SymValue from;
		private readonly TFunc function;

		public EliminateEdgeUpdate (SymValue sv, TFunc function)
		{
			this.from = sv;
			this.function = function;
		}

		#region Overrides of Update
		public override void ReplayElimination (MergeInfo<TFunc, TAbstractDomain> merge)
		{
			if (!merge.IsCommon (this.from))
				return;

			merge.Result.Eliminate (this.function, this.from);
		}

		public override void Replay (MergeInfo<TFunc, TAbstractDomain> merge)
		{
			if (!merge.IsCommon (this.from))
				return;

			SymValue sv1 = merge.Graph1.LookupWithoutManifesting (this.from, this.function);
			SymValue sv2 = merge.Graph2.LookupWithoutManifesting (this.from, this.function);
			if (sv1 != null && sv2 != null)
				return;
			if (sv1 != null) {
				if (DebugOptions.Debug)
				{
					Console.WriteLine ("---SymGraph changed due to EliminateEdgeUpdate {0}-{1} " +
					                   "-> that is only in G1", this.from, this.function);
				}
				merge.Changed = true;
			}

			bool noEdgeInResult = merge.Result.LookupWithoutManifesting (this.from, this.function) == null;
			if (noEdgeInResult)
				return;

			merge.Result.Eliminate (this.function, this.from);
		}
		#endregion
	}
}
