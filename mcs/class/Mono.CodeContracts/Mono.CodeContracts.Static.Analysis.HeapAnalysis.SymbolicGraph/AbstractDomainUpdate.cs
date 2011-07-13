// 
// AbstractDomainUpdate.cs
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
	class AbstractDomainUpdate<TFunc, TAbstractDomain> : Update<TFunc, TAbstractDomain>
		where TFunc : IEquatable<TFunc>, IConstantInfo
		where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain> {
		private readonly SymValue sv;

		public AbstractDomainUpdate (SymValue sv)
		{
			this.sv = sv;
		}

		#region Overrides of Update
		public override void Replay (MergeInfo<TFunc, TAbstractDomain> merge)
		{
			if (!merge.IsCommon (this.sv))
				return;

			TAbstractDomain val1 = merge.Graph1 [this.sv];
			TAbstractDomain val2 = merge.Graph2 [this.sv];
			bool weaker;
			TAbstractDomain join = val1.Join (val2, merge.Widen, out weaker);

			TAbstractDomain wasInResult = merge.Result [this.sv];
			if (weaker) {
				if (DebugOptions.Debug)
				{
					Console.WriteLine ("----SymGraph changed during AbstractDomainUpdate of {3} " +
					                   "due to weaker abstractValue join (val1 = {0}, val2 = {1}, wasInResult = {2}",
					                   val1, val2, wasInResult, this.sv);
				}
				merge.Changed = true;
			}

			if (join.Equals (wasInResult))
				return;

			merge.Result [this.sv] = join;
		}

		public override void ReplayElimination (MergeInfo<TFunc, TAbstractDomain> merge)
		{
			if (!merge.IsCommon (this.sv))
				return;

			TAbstractDomain val1 = merge.Graph1 [this.sv];

			if (val1.IsTop)
				merge.Result [this.sv] = val1;
			else {
				TAbstractDomain val2 = merge.Graph2 [this.sv];
				if (val2.IsTop)
					merge.Result [this.sv] = val2;
			}
		}
		#endregion
	}
}
