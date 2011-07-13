// 
// EdgeUpdate.cs
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
	class EdgeUpdate<TFunc, TAbstractDomain> : Update<TFunc, TAbstractDomain>
		where TFunc : IEquatable<TFunc>, IConstantInfo
		where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain> {
		private readonly SymValue from;
		private readonly TFunc function;

		public EdgeUpdate (SymValue from, TFunc function)
		{
			this.from = from;
			this.function = function;
		}

		#region Overrides of Update
		public override void Replay (MergeInfo<TFunc, TAbstractDomain> merge)
		{
			if (!merge.IsCommon (this.from))
				return;

			SymValue sv1 = merge.Graph1.LookupWithoutManifesting (this.from, this.function);
			SymValue sv2 = merge.Graph2.LookupWithoutManifesting (this.from, this.function);
			if (DebugOptions.Debug)
			{
				Console.WriteLine ("Replay edge update: {0} -{1} -> [ {2}, {3} ]",
				                   this.from, this.function, sv1, sv2);
			}
			if (sv1 == null) {
				if (this.function.KeepAsBottomField && merge.Graph1.HasAllBottomFields (this.from))
					sv1 = merge.Graph1.BottomPlaceHolder;
				else {
					if (sv2 == null || merge.Widen || !this.function.ManifestField)
						return;
					if (DebugOptions.Debug)
					{
						Console.WriteLine ("---SymGraph changed due to manifestation of a top edge in Graph1");
					}
					merge.Changed = true;
				}
			}
			if (sv2 == null) {
				if (this.function.KeepAsBottomField && merge.Graph2.HasAllBottomFields (this.from))
					sv2 = merge.Graph2.BottomPlaceHolder;
				else {
					if (merge.Widen || !this.function.ManifestField)
						return;
					if (DebugOptions.Debug)
					{
						Console.WriteLine ("---SymGraph changed due to manifestation of due to missing target in Graph2");
					}
					merge.Changed = true;
					return;
				}
			}

			SymValue r = merge.AddJointEdge (sv1, sv2, this.function, this.from);
			if (r == null || r.UniqueId <= merge.LastCommonVariable)
				return;

			merge.JoinSymbolicValue (sv1, sv2, r);
		}

		public override void ReplayElimination (MergeInfo<TFunc, TAbstractDomain> merge)
		{
		}
		#endregion
	}
}
