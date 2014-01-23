// 
// ISymGraph.cs
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

using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.SymbolicGraph;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	interface ISymGraph<TFunc, TADomain, TGraph> : IAbstractDomain<TGraph>
		where TFunc : IEquatable<TFunc>, IConstantInfo
		where TADomain : IAbstractDomainForEGraph<TADomain> {
		SymValue this [TFunc function, SymValue arg] { get; set; }
		SymValue this [TFunc function] { get; set; }
		TADomain this [SymValue symbol] { get; set; }
		IEnumerable<TFunc> Constants { get; }
		IEnumerable<SymValue> Variables { get; }

		SymValue FreshSymbol ();

		SymValue TryLookup (TFunc function, SymValue arg);
		SymValue TryLookup (TFunc function);

		IEnumerable<TFunc> Functions (SymValue symbol);
		IEnumerable<SymGraphTerm<TFunc>> EqTerms (SymValue symbol);

		void AssumeEqual (SymValue v1, SymValue v2);
		bool IsEqual (SymValue v1, SymValue v2);
		void Eliminate (TFunc function, SymValue arg);
		void Eliminate (TFunc function);
		void EliminateAll (SymValue arg);

		TGraph Join (TGraph that, out IMergeInfo mergeInfo, bool widen);

		bool LessEqual (TGraph that, out IImmutableMap<SymValue, Sequence<SymValue>> forward,
		                out IImmutableMap<SymValue, SymValue> backward);
	}
}
