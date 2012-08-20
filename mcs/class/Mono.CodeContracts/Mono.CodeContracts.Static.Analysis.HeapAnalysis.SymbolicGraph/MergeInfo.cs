// 
// MergeInfo.cs
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
using System.Collections.Generic;
using System.Linq;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Extensions;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.SymbolicGraph {
	class MergeInfo<TFunc, TADomain> : IMergeInfo
		where TFunc : IEquatable<TFunc>, IConstantInfo
		where TADomain : IAbstractDomainForEGraph<TADomain>, IEquatable<TADomain> {
		
		public readonly SymGraph<TFunc, TADomain> Result;
		public readonly SymGraph<TFunc, TADomain> Graph1;
		public readonly SymGraph<TFunc, TADomain> Graph2;

		public readonly int LastCommonVariable;
		public readonly bool Widen;

		private readonly HashSet<SymValue> manifested;
		private readonly DoubleDictionary<SymValue, SymValue, int> pending_counts;
		private readonly HashSet<Tuple<SymValue, SymValue, MultiEdge<TFunc, TADomain>>> visited_multi_edges;

		private DoubleImmutableMap<SymValue, SymValue, SymValue> mappings;
		private IImmutableSet<SymValue> visited_key1;
		private Sequence<Tuple<SymValue, SymValue, SymValue>> merge_triples;

		public MergeInfo (SymGraph<TFunc, TADomain> result,
		                  SymGraph<TFunc, TADomain> g1,
		                  SymGraph<TFunc, TADomain> g2, bool widen)
		{
			this.mappings = DoubleImmutableMap<SymValue, SymValue, SymValue>.Empty (SymValue.GetUniqueKey);
			this.visited_key1 = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
			this.visited_multi_edges = new HashSet<Tuple<SymValue, SymValue, MultiEdge<TFunc, TADomain>>> ();
			this.pending_counts = new DoubleDictionary<SymValue, SymValue, int> ();
			this.manifested = new HashSet<SymValue> ();
			
			this.LastCommonVariable = result.IdGenerator;
			this.Widen = widen;
			this.Result = result;
			this.Graph1 = g1;
			this.Graph2 = g2;

			this.Changed = false;
		}

		#region IMergeInfo Members
		public bool Changed { get; set; }

		public IEnumerable<Tuple<SymValue, SymValue, SymValue>> MergeTriples
		{
			get { return this.merge_triples.AsEnumerable (); }
		}

		public IImmutableMap<SymValue, Sequence<SymValue>> ForwardG1Map
		{
			get { return GetForwardGraphMap ((t) => t.Item1); }
		}

		public IImmutableMap<SymValue, Sequence<SymValue>> ForwardG2Map
		{
			get { return GetForwardGraphMap ((t) => t.Item2); }
		}

		public bool IsResultGraph<TFunc1, TAbstractDomain> (SymGraph<TFunc1, TAbstractDomain> graph)
			where TFunc1 : IEquatable<TFunc1>, IConstantInfo
			where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain>
		{
			return Equals (graph, this.Result);
		}

		public bool IsGraph1<TFunc1, TAbstractDomain> (SymGraph<TFunc1, TAbstractDomain> graph)
			where TFunc1 : IEquatable<TFunc1>, IConstantInfo
			where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain>
		{
			return (Equals (this.Graph1, graph) || Equals (this.Graph1.Parent, graph) && Equals (this.Graph1.Updates, graph.Updates));
		}

		public bool IsGraph2<TFunc1, TAbstractDomain> (SymGraph<TFunc1, TAbstractDomain> graph)
			where TFunc1 : IEquatable<TFunc1>, IConstantInfo
			where TAbstractDomain : IAbstractDomainForEGraph<TAbstractDomain>, IEquatable<TAbstractDomain>
		{
			return (Equals (this.Graph2, graph) || Equals (this.Graph2.Parent, graph) && Equals (this.Graph2.Updates, graph.Updates));
		}
		#endregion

		public void AddMapping (SymValue v1, SymValue v2, SymValue result)
		{
			if (v1 != null && v2 != null)
				this.mappings = this.mappings.Add (v1, v2, result);
			else if (v2 == null)
				this.visited_key1 = this.visited_key1.Add (v1);
			else
				this.visited_key1 = this.visited_key1.Add (v2);

			AddMergeTriple (v1, v2, result);
		}

		public SymValue AddJointEdge (SymValue v1Target, SymValue v2Target, TFunc function, SymValue resultArg)
		{
			SymValue result = LookupMapping (v1Target, v2Target);
			bool newEdge = false;
			if (result == null)
			{
				if (IsMappingAlreadyAdded (v1Target, v2Target))
				{
					if (DebugOptions.Debug)
						Console.WriteLine ("---SymGraph changed due to pre-existing mapping in G1 of {0}", v1Target);
					Changed = true;
					if (v1Target == null)
					{
						if (this.manifested.Contains (v2Target))
							return null;
						this.manifested.Add (v2Target);
					}
					if (v2Target == null)
					{
						if (this.manifested.Contains (v1Target))
							return null;
						this.manifested.Add (v1Target);
					}
				}
				newEdge = true;
				result = v1Target == null || v1Target.UniqueId > this.LastCommonVariable || v1Target != v2Target ? this.Result.FreshSymbol () : v1Target;
				AddMapping (v1Target, v2Target, result);
			}
			else if (this.Result.LookupWithoutManifesting (resultArg, function) == result)
				return null;
			this.Result[function, resultArg] = result;
			TADomain val1 = Graph1ADomain (v1Target);
			TADomain val2 = Graph2ADomain (v2Target);

			bool weaker;
			TADomain join = val1.Join (val2, this.Widen, out weaker);
			this.Result[result] = join;

			if (weaker)
			{
				if (DebugOptions.Debug)
				{
					Console.WriteLine ("----SymGraph changed due to join of abstract values of [{0}, {1}] " +
					                   "(prev {2}, new {3}, join {4}", v1Target, v2Target, val1, val2, join);
				}
				Changed = true;
			}

			return newEdge ? result : null;
		}

		public SymValue AddJointEdge (SymValue v1Target, SymValue v2Target, TFunc function, SymValue[] resultArgs)
		{
			SymValue result = LookupMapping (v1Target, v2Target);
			bool newEdge = false;
			if (result == null)
			{
				if (IsMappingAlreadyAdded (v1Target, v2Target))
				{
					if (DebugOptions.Debug)
						Console.WriteLine ("---SymGraph changed due to pre-existing mapping in G1 of {0}", v1Target);
					Changed = true;
					if (v1Target == null || v2Target == null)
						return null;
				}
				newEdge = true;
				result = v1Target == null || v1Target.UniqueId > this.LastCommonVariable || v1Target != v2Target ? this.Result.FreshSymbol () : v1Target;
				AddMapping (v1Target, v2Target, result);
			}
			else if (this.Result.LookupWithoutManifesting (resultArgs, function) == result)
				return null;
			this.Result[resultArgs, function] = result;
			TADomain val1 = Graph1ADomain (v1Target);
			TADomain val2 = Graph2ADomain (v2Target);

			bool weaker;
			TADomain joinValue = val1.Join (val2, this.Widen, out weaker);

			this.Result[result] = joinValue;
			if (weaker)
			{
				if (DebugOptions.Debug)
					Console.WriteLine ("----SymGraph changed due to join of abstract values of [{0}, {1}] (prev {2}, new {3}, join {4}",
					                   v1Target, v2Target,
					                   val1, val2, joinValue);
				Changed = true;
			}

			if (DebugOptions.Debug)
			{
				Console.WriteLine ("AddJointEdge: ({0}) -{1} -> [{2},{3},{4}]",
				                   resultArgs.ToString (", "), function,
				                   v1Target, v2Target, result);
			}
			return newEdge ? result : null;
		}

		public bool IsCommon (SymValue sv)
		{
			return sv.UniqueId <= this.LastCommonVariable;
		}

		public bool AreCommon (SymValue[] svs)
		{
			return svs.All (sv => IsCommon (sv));
		}

		public void JoinSymbolicValue (SymValue sv1, SymValue sv2, SymValue r)
		{
			if (this.Graph2.HasAllBottomFields (sv2)) {
				if (sv1 != null) {
					foreach (TFunc function in this.Graph1.TermMap.Keys2 (sv1)) {
						SymValue v1 = this.Graph1.LookupWithoutManifesting (sv1, function);
						bool isPlaceHolder;
						SymValue v2 = this.Graph2.LookupOrBottomPlaceHolder (sv2, function, out isPlaceHolder);
						if (!isPlaceHolder || function.KeepAsBottomField) {
							SymValue r1 = AddJointEdge (v1, v2, function, r);
							if (r1 != null)
								JoinSymbolicValue (v1, v2, r1);
						}
					}
				}
			} else if (!this.Widen && this.Graph1.HasAllBottomFields (sv1)) {
				if (DebugOptions.Debug)
					Console.WriteLine ("---SymGraph changed due to an all bottom field value in G1 changing to non-bottom");
				Changed = true;
				if (sv2 != null) {
					foreach (TFunc function in this.Graph2.TermMap.Keys2 (sv2)) {
						bool isPlaceHolder;
						SymValue v1 = this.Graph1.LookupOrBottomPlaceHolder (sv1, function, out isPlaceHolder);
						SymValue v2 = this.Graph2.LookupWithoutManifesting (sv2, function);
						if (!isPlaceHolder || function.KeepAsBottomField) {
							SymValue r1 = AddJointEdge (v1, v2, function, r);
							if (r1 != null)
								JoinSymbolicValue (v1, v2, r1);
						}
					}
				}
			} else {
				IEnumerable<TFunc> functions;
				if (this.Widen) {
					if (this.Graph1.TermMap.Keys2Count (sv1) <= this.Graph2.TermMap.Keys2Count (sv2))
						functions = this.Graph1.TermMap.Keys2 (sv1);
					else {
						functions = this.Graph2.TermMap.Keys2 (sv2);
						if (DebugOptions.Debug)
							Console.WriteLine ("---SymGraph changed because G2 has fewer keys for {0} than {1} in G1", sv2, sv1);
						Changed = true;
					}
				} else {
					if (this.Graph1.TermMap.Keys2Count (sv1) < this.Graph2.TermMap.Keys2Count (sv2)) {
						functions = this.Graph2.TermMap.Keys2 (sv2);
						if (DebugOptions.Debug)
							Console.WriteLine ("---SymGraph changed because G1 has fewer keys for {0} than {1} in G2", sv1, sv2);
						Changed = true;
					} else
						functions = this.Graph1.TermMap.Keys2 (sv1);
				}

				foreach (TFunc function in functions) {
					SymValue v1 = this.Graph1.LookupWithoutManifesting (sv1, function);
					SymValue v2 = this.Graph2.LookupWithoutManifesting (sv2, function);

					if (v1 == null) {
						if (!this.Widen && function.ManifestField) {
							if (DebugOptions.Debug)
								Console.WriteLine ("---SymGraph changed due to manifestation of a top edge in G1");
							Changed = true;

						} else
							continue;
					}
					if (v2 == null && (this.Widen || !function.ManifestField)) {
						if (DebugOptions.Debug)
							Console.WriteLine ("---SymGraph changed due to absence of map {0}-{1} -> in G2", sv2, function);
						Changed = true;
					}

					if (v1 != null && v2 != null)
					{
						//we have to joint ends of edges
						SymValue r1 = AddJointEdge (v1, v2, function, r);
						if (r1 != null)
							JoinSymbolicValue (v1, v2, r1);
					}
				}
			}

			JoinMultiEdges (sv1, sv2);
		}

		public void JoinMultiEdge (SymValue sv1, SymValue sv2, MultiEdge<TFunc, TADomain> edge)
		{
			var key = new Tuple<SymValue, SymValue, MultiEdge<TFunc, TADomain>> (sv1, sv2, edge);
			if (!this.visited_multi_edges.Add (key))
				return;

			Sequence<SymValue> list1 = this.Graph1.MultiEdgeMap [sv1, edge];
			Sequence<SymValue> list2 = this.Graph2.MultiEdgeMap [sv2, edge];
			if (list2.IsEmpty ())
				return;
			foreach (SymValue v1 in list1.AsEnumerable ()) {
				foreach (SymValue v2 in list2.AsEnumerable ()) {
					if (UpdatePendingCount (v1, v2, edge.Arity)) {
						SymGraphTerm<TFunc> term1 = this.Graph1.EqualMultiTermsMap [v1];
						SymGraphTerm<TFunc> term2 = this.Graph2.EqualMultiTermsMap [v2];
						if (term1.Args != null && term2.Args != null) {
							var resultRoots = new SymValue[term1.Args.Length];
							for (int i = 0; i < resultRoots.Length; i++)
								resultRoots [i] = this.mappings [term1.Args [i], term2.Args [i]];
							SymValue r = AddJointEdge (v1, v2, edge.Function, resultRoots);
							if (r != null)
								JoinSymbolicValue (sv1, sv2, r);
						} else
							break;
					}
				}
			}
		}

		public void Replay (SymGraph<TFunc, TADomain> common)
		{
			PrimeMapWithCommon ();
			Replay (this.Graph1.Updates, common.Updates);
			Replay (this.Graph2.Updates, common.Updates);
		}

		public void ReplayEliminations (SymGraph<TFunc, TADomain> common)
		{
			ReplayEliminations (this.Graph1.Updates, common.Updates);
			ReplayEliminations (this.Graph2.Updates, common.Updates);
		}

		public void Commit ()
		{
			if (Changed)
				return;

			bool needContinue = false;
			foreach (var edge in this.Graph1.ValidMultiTerms)
			{
				SymGraphTerm<TFunc> term = edge.Value;
				var args = new SymValue[term.Args.Length];

				for (int i = 0; i < args.Length; ++i)
				{
					SymValue sv = term.Args[i];
					if (IsMappingAlreadyAdded (sv, null))
					{
						if (this.mappings.Keys2 (sv) != null && this.mappings.Keys2 (sv).Count () == 1)
							args[i] = this.mappings[sv, this.mappings.Keys2 (sv).First ()];
					}
					else
					{
						needContinue = true;
						break;
					}

					if (args[i] == null)
					{
						Changed = true;
						return;
					}
				}

				if (needContinue)
					continue;

				SymValue symbol = this.Result.LookupWithoutManifesting (args, term.Function);
				if (symbol != null)
				{
					SymValue key = edge.Key;
					if (this.mappings.Keys2 (key) != null && this.mappings.Keys2 (key).Count () == 1 && this.mappings[key, this.mappings.Keys2 (key).First ()] == symbol)
						continue;
				}

				Changed = true;
				return;
			}
		}

		private IImmutableMap<SymValue, Sequence<SymValue>> GetForwardGraphMap (Func<Tuple<SymValue, SymValue, SymValue>, SymValue> sourceSelector)
		{
			IImmutableMap<SymValue, Sequence<SymValue>> res = ImmutableIntKeyMap<SymValue, Sequence<SymValue>>.Empty (SymValue.GetUniqueKey);
			foreach (var tuple in this.merge_triples.AsEnumerable ()) {
				SymValue sv = sourceSelector (tuple);
				if (sv != null)
					res = res.Add (sv, res [sv].Cons (tuple.Item3));
			}
			return res;
		}

		private bool UpdatePendingCount (SymValue xi, SymValue yi, int arity)
		{
			int result;
			this.pending_counts.TryGetValue (xi, yi, out result);
			result = result + 1;

			this.pending_counts [xi, yi] = result;
			if (result == arity)
				return true;

			return false;
		}

		private void JoinMultiEdges (SymValue sv1, SymValue sv2)
		{
			if (sv1 == null || sv2 == null)
				return;

			IEnumerable<MultiEdge<TFunc, TADomain>> edges =
				this.Graph1.MultiEdgeMap.Keys2Count (sv1) > this.Graph2.MultiEdgeMap.Keys2Count (sv2)
					? this.Graph2.MultiEdgeMap.Keys2 (sv2)
					: this.Graph1.MultiEdgeMap.Keys2 (sv1);
			foreach (var edge in edges)
				JoinMultiEdge (sv1, sv2, edge);
		}

		private TADomain Graph1ADomain (SymValue sv)
		{
			if (sv != null)
				return this.Graph1 [sv];
			return this.Graph1.UnderlyingTopValue.ForManifestedField ();
		}

		private TADomain Graph2ADomain (SymValue sv)
		{
			if (sv != null)
				return this.Graph2 [sv];
			return this.Graph2.UnderlyingTopValue.ForManifestedField ();
		}

		private void AddMergeTriple (SymValue v1, SymValue v2, SymValue result)
		{
			this.merge_triples = this.merge_triples.Cons (new Tuple<SymValue, SymValue, SymValue> (v1, v2, result));
		}

		private bool IsMappingAlreadyAdded (SymValue v1, SymValue v2)
		{
			if (v1 != null)
				return this.visited_key1.Contains (v1) || this.mappings.ContainsKey1 (v1);
			
			return this.visited_key1.Contains (v2);
		}

		private SymValue LookupMapping (SymValue v1, SymValue v2)
		{
			if (v1 == null || v2 == null)
				return null;

			return this.mappings [v1, v2];
		}

		private void PrimeMapWithCommon ()
		{
			Sequence<SymValue> rest = null;
			foreach (SymValue sv in this.Graph1.EqualTermsMap.Keys) {
				if (IsCommon (sv) && (this.Graph2.EqualTermsMap.ContainsKey (sv) || this.Graph2.EqualMultiTermsMap.ContainsKey (sv))) {
					if (this.Graph1.MultiEdgeMap.ContainsKey1 (sv))
						rest = rest.Cons (sv);
					AddMapping (sv, sv, sv);
				}
			}
			foreach (SymValue sv in this.Graph1.EqualMultiTermsMap.Keys) {
				if (IsCommon (sv) && (this.Graph2.EqualTermsMap.ContainsKey (sv) || this.Graph2.EqualMultiTermsMap.ContainsKey (sv)) && this.mappings [sv, sv] == null) {
					if (this.Graph1.MultiEdgeMap.ContainsKey1 (sv))
						rest = rest.Cons (sv);
					AddMapping (sv, sv, sv);
				}
			}
			while (rest != null) {
				SymValue sv = rest.Head;
				rest = rest.Tail;
				foreach (var edge in this.Graph1.MultiEdgeMap.Keys2 (sv))
					JoinMultiEdge (sv, sv, edge);
			}
		}

		private void Replay (Sequence<Update<TFunc, TADomain>> updates, Sequence<Update<TFunc, TADomain>> common)
		{
			for (Update<TFunc, TADomain> update = Update<TFunc, TADomain>.Reverse (updates, common); update != null; update = update.Next)
				update.Replay (this);
		}

		private void ReplayEliminations (Sequence<Update<TFunc, TADomain>> updates, Sequence<Update<TFunc, TADomain>> common)
		{
			for (Update<TFunc, TADomain> update = Update<TFunc, TADomain>.Reverse (updates, common); update != null; update = update.Next)
				update.ReplayElimination (this);
		}
	}
}