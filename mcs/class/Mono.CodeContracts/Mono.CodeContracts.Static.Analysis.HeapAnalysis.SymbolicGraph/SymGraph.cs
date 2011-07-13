// 
// SymGraph.cs
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
using System.IO;
using System.Linq;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Extensions;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis.SymbolicGraph {
	class SymGraph<TFunc, TADomain> : ISymGraph<TFunc, TADomain, SymGraph<TFunc, TADomain>>
		where TFunc : IEquatable<TFunc>, IConstantInfo
		where TADomain : IAbstractDomainForEGraph<TADomain>, IEquatable<TADomain> {
		public const bool DoIncrementalJoin = false;
		private static int egraphIdGenerator;
		private static SymGraph<TFunc, TADomain> BottomValue;
		public readonly SymValue BottomPlaceHolder;
		public readonly SymGraph<TFunc, TADomain> Parent;
		public readonly TADomain UnderlyingTopValue;

		private readonly SymValue const_root;
		private readonly int egraph_id;
		private readonly int history_size;

		private readonly SymGraph<TFunc, TADomain> root_graph;

		private readonly TADomain underlying_bottom_value;
		private IImmutableMap<SymValue, TADomain> abs_map;
		private IImmutableMap<SymValue, SymValue> forw_map;
		private bool is_immutable;

		public SymGraph (TADomain topValue, TADomain bottomValue)
			: this (topValue, bottomValue, false)
		{
			if (BottomValue != null)
				return;
			BottomValue = new SymGraph<TFunc, TADomain> (topValue, bottomValue, false);
		}

		private SymGraph (TADomain topValue, TADomain bottomValue, bool _)
		{
			this.egraph_id = egraphIdGenerator++;
			this.const_root = FreshSymbol ();

			TermMap = DoubleImmutableMap<SymValue, TFunc, SymValue>.Empty (SymValue.GetUniqueKey);
			MultiEdgeMap = DoubleImmutableMap<SymValue, MultiEdge<TFunc, TADomain>, LispList<SymValue>>.Empty (SymValue.GetUniqueKey);
			this.abs_map = ImmutableIntKeyMap<SymValue, TADomain>.Empty (SymValue.GetUniqueKey);
			this.forw_map = ImmutableIntKeyMap<SymValue, SymValue>.Empty (SymValue.GetUniqueKey);
			EqualTermsMap = ImmutableIntKeyMap<SymValue, LispList<SymGraphTerm<TFunc>>>.Empty (SymValue.GetUniqueKey);
			EqualMultiTermsMap = ImmutableIntKeyMap<SymValue, SymGraphTerm<TFunc>>.Empty (SymValue.GetUniqueKey);

			this.BottomPlaceHolder = FreshSymbol ();
			this.abs_map = this.abs_map.Add (this.BottomPlaceHolder, bottomValue);
			this.is_immutable = false;
			this.history_size = 1;
			this.Parent = null;
			this.root_graph = this;
			Updates = null;
			this.UnderlyingTopValue = topValue;
			this.underlying_bottom_value = bottomValue;
		}

		private SymGraph (SymGraph<TFunc, TADomain> from)
		{
			this.egraph_id = egraphIdGenerator++;
			this.const_root = from.const_root;
			this.BottomPlaceHolder = from.BottomPlaceHolder;
			TermMap = from.TermMap;
			MultiEdgeMap = from.MultiEdgeMap;
			IdGenerator = from.IdGenerator;
			this.abs_map = from.abs_map;
			this.forw_map = from.forw_map;
			EqualTermsMap = from.EqualTermsMap;
			EqualMultiTermsMap = from.EqualMultiTermsMap;
			this.UnderlyingTopValue = from.UnderlyingTopValue;
			this.underlying_bottom_value = from.underlying_bottom_value;
			Updates = from.Updates;
			this.Parent = from;
			this.root_graph = from.root_graph;
			this.history_size = from.history_size + 1;

			from.MarkAsImmutable ();
		}

		public IImmutableMap<SymValue, SymGraphTerm<TFunc>> EqualMultiTermsMap { get; private set; }
		public IImmutableMap<SymValue, LispList<SymGraphTerm<TFunc>>> EqualTermsMap { get; private set; }
		public DoubleImmutableMap<SymValue, MultiEdge<TFunc, TADomain>, LispList<SymValue>> MultiEdgeMap { get; private set; }
		public DoubleImmutableMap<SymValue, TFunc, SymValue> TermMap { get; private set; }
		public int IdGenerator { get; private set; }
		public LispList<Update<TFunc, TADomain>> Updates { get; private set; }

		public bool IsImmutable
		{
			get { return this.is_immutable; }
		}

		private int LastSymbolId
		{
			get { return IdGenerator; }
		}

		public SymValue this [SymValue[] args, TFunc function]
		{
			get
			{
				int len = args.Length;
				for (int i = 0; i < len; i++)
					args [i] = Find (args [i]);

				SymValue candidate = FindCandidate (args, function);
				if (candidate != null)
					return candidate;
				candidate = FreshSymbol ();
				for (int i = 0; i < len; i++) {
					var edge = new MultiEdge<TFunc, TADomain> (function, i, len);
					MultiEdgeMap = MultiEdgeMap.Add (args [i], edge, MultiEdgeMap [args [i], edge].Cons (candidate));
				}
				EqualMultiTermsMap = EqualMultiTermsMap.Add (candidate, new SymGraphTerm<TFunc> (function, args));
				AddMultiEdgeUpdate (args, function);
				return candidate;
			}
			set
			{
				int len = args.Length;
				for (int i = 0; i < len; i++)
					args [i] = Find (args [i]);

				bool isTermEqual = true;
				SymGraphTerm<TFunc> term = EqualMultiTermsMap [value];
				if (term.Args != null) {
					for (int i = 0; i < len; i++) {
						if (term.Args [i] != args [i]) {
							isTermEqual = false;
							break;
						}
					}
				}

				for (int i = 0; i < len; i++) {
					var edge = new MultiEdge<TFunc, TADomain> (function, i, len);
					LispList<SymValue> list = MultiEdgeMap [args [i], edge];
					if (isTermEqual && !LispList<SymValue>.Contains (list, value))
						isTermEqual = false;
					if (!isTermEqual)
						MultiEdgeMap = MultiEdgeMap.Add (args [i], edge, list.Cons (value));
				}
				if (isTermEqual)
					return;
				EqualMultiTermsMap = EqualMultiTermsMap.Add (value, new SymGraphTerm<TFunc> (function, args));
				AddMultiEdgeUpdate (args, function);
			}
		}

		public SymValue this [TFunc function, params SymValue[] args]
		{
			get { return this [args, function]; }
		}

		private SymValue this [SymValue source, TFunc function]
		{
			get
			{
				source = Find (source);
				SymValue sv = TermMap [source, function];
				SymValue key;
				if (sv == null) {
					key = FreshSymbol ();
					TermMap = TermMap.Add (source, function, key);
					EqualTermsMap = EqualTermsMap.Add (key, LispList<SymGraphTerm<TFunc>>.Cons (new SymGraphTerm<TFunc> (function, source), null));
					AddEdgeUpdate (source, function);
				} else
					key = Find (sv);

				return key;
			}
			set
			{
				source = Find (source);
				value = Find (value);

				TermMap = TermMap.Add (source, function, value);
				LispList<SymGraphTerm<TFunc>> rest = EqualTermsMap [value];
				if (rest.IsEmpty () || (!rest.Head.Function.Equals (function) || rest.Head.Args [0] != source))
					EqualTermsMap = EqualTermsMap.Add (value, rest.Cons (new SymGraphTerm<TFunc> (function, source)));

				AddEdgeUpdate (source, function);
			}
		}

		public IEnumerable<Pair<SymValue, SymGraphTerm<TFunc>>> ValidMultiTerms {
			get
			{
				foreach (SymValue sv in EqualMultiTermsMap.Keys) {
					SymGraphTerm<TFunc> term = EqualMultiTermsMap [sv];
					if (IsValidMultiTerm (term))
						yield return new Pair<SymValue, SymGraphTerm<TFunc>> (sv, term);
				}
			}
		}

		public SymValue ConstRoot
		{
			get { return this.const_root; }
		}

		#region ISymGraph<TFunc,TADomain,SymGraph<TFunc,TADomain>> Members
		public TADomain this [SymValue symbol]
		{
			get
			{
				symbol = Find (symbol);
				if (this.abs_map.ContainsKey (symbol))
					return this.abs_map [symbol];

				return this.UnderlyingTopValue;
			}
			set
			{
				SymValue newSym = Find (symbol);
				if (this [symbol].Equals (value))
					return;
				AddAbstractValueUpdate (newSym);
				if (value.IsTop)
					this.abs_map = this.abs_map.Remove (newSym);
				else
					this.abs_map = this.abs_map.Add (newSym, value);
			}
		}

		public SymValue this [TFunc function]
		{
			get { return this [this.const_root, function]; }
			set { this [this.const_root, function] = value; }
		}

		public SymValue this [TFunc function, SymValue arg]
		{
			get { return this [arg, function]; }
			set { this [arg, function] = value; }
		}

		public IEnumerable<TFunc> Constants
		{
			get { return TermMap.Keys2 (this.const_root); }
		}

		public IEnumerable<SymValue> Variables
		{
			get { return TermMap.Keys1; }
		}

		public SymGraph<TFunc, TADomain> Top
		{
			get { return new SymGraph<TFunc, TADomain> (this.UnderlyingTopValue, this.underlying_bottom_value); }
		}

		public SymGraph<TFunc, TADomain> Bottom
		{
			get
			{
				if (BottomValue == null) {
					BottomValue = new SymGraph<TFunc, TADomain> (this.UnderlyingTopValue, this.underlying_bottom_value);
					BottomValue.MarkAsImmutable ();
				}
				return BottomValue;
			}
		}

		public bool IsTop
		{
			get { return TermMap.Keys2Count (this.const_root) == 0; }
		}

		public bool IsBottom
		{
			get { return this == BottomValue; }
		}

		public SymValue FreshSymbol ()
		{
			return new SymValue (++IdGenerator);
		}

		public SymValue TryLookup (TFunc function)
		{
			return LookupWithoutManifesting (this.const_root, function);
		}

		public SymValue TryLookup (TFunc function, SymValue arg)
		{
			return LookupWithoutManifesting (arg, function);
		}

		public void Eliminate (TFunc function, SymValue arg)
		{
			SymValue value = Find (arg);
			DoubleImmutableMap<SymValue, TFunc, SymValue> newTermMap = TermMap.Remove (value, function);
			if (newTermMap == TermMap)
				return;
			TermMap = newTermMap;
			AddEliminateEdgeUpdate (value, function);
		}

		public void Eliminate (TFunc function)
		{
			TermMap = TermMap.Remove (this.const_root, function);
			AddEliminateEdgeUpdate (this.const_root, function);
		}

		public void EliminateAll (SymValue arg)
		{
			SymValue value = Find (arg);
			AddEliminateAllUpdate (value);
			TermMap = TermMap.RemoveAll (value);
			this [arg] = this.UnderlyingTopValue;
		}

		public void AssumeEqual (SymValue v1, SymValue v2)
		{
			var workList = new WorkList<EqualityPair<TFunc, TADomain>> ();
			SymValue sv1 = Find (v1);
			SymValue sv2 = Find (v2);

			if (TryPushEquality (workList, sv1, sv2))
				AddEqualityUpdate (sv1, sv2);

			DrainEqualityWorkList (workList);
		}

		public bool IsEqual (SymValue v1, SymValue v2)
		{
			return Find (v1) == Find (v2);
		}

		public IEnumerable<TFunc> Functions (SymValue sv)
		{
			return TermMap.Keys2 (Find (sv));
		}

		public IEnumerable<SymGraphTerm<TFunc>> EqTerms (SymValue sv)
		{
			foreach (var term in EqualTermsMap [Find (sv)].AsEnumerable ()) {
				if (TryLookup (term.Function, term.Args) == sv)
					yield return term;
			}
		}

		public SymGraph<TFunc, TADomain> Clone ()
		{
			return new SymGraph<TFunc, TADomain> (this);
		}

		public SymGraph<TFunc, TADomain> Join (SymGraph<TFunc, TADomain> that, bool widening, out bool weaker)
		{
			IMergeInfo info;
			SymGraph<TFunc, TADomain> join = Join (that, out info, widening);
			weaker = info.Changed;
			return join;
		}

		public SymGraph<TFunc, TADomain> Join (SymGraph<TFunc, TADomain> that, out IMergeInfo mergeInfo, bool widen)
		{
			SymGraph<TFunc, TADomain> egraph = this;
			int updateSize;
			SymGraph<TFunc, TADomain> commonTail = ComputeCommonTail (egraph, that, out updateSize);
			bool hasCommonTail = true;
			if (commonTail == null)
				hasCommonTail = false;

			bool doingIncrementalJoin = hasCommonTail & commonTail != egraph.root_graph & !widen & DoIncrementalJoin;

			//debug

			if (DebugOptions.Debug)
			{
				Console.WriteLine ("SymGraph {0}", widen ? "widen" : "join");
				if (commonTail != null)
					Console.WriteLine ("Last common symbol: {0}", commonTail.LastSymbolId);

				Console.WriteLine ("  Doing {0}", doingIncrementalJoin ? "incremental join" : "full join");
			}

			SymGraph<TFunc, TADomain> result;
			MergeInfo<TFunc, TADomain> mergeState;
			if (doingIncrementalJoin) {
				result = new SymGraph<TFunc, TADomain> (commonTail);
				mergeState = new MergeInfo<TFunc, TADomain> (result, egraph, that, widen);
				mergeState.Replay (commonTail);
				mergeState.Commit ();
			} else {
				result = new SymGraph<TFunc, TADomain> (commonTail);
				mergeState = new MergeInfo<TFunc, TADomain> (result, egraph, that, widen);
				mergeState.ReplayEliminations (commonTail);
				mergeState.AddMapping (egraph.const_root, that.const_root, result.const_root);
				mergeState.JoinSymbolicValue (egraph.const_root, that.const_root, result.const_root);
				mergeState.Commit ();
			}
			mergeInfo = mergeState;

			if (DebugOptions.Debug)
			{
				Console.WriteLine ("  Result update size {0}", result.Updates.Length ());
				Console.WriteLine ("Done with Egraph join: changed = {0}", mergeInfo.Changed ? 1 : 0);
			}

			return result;
		}

		public void Dump (TextWriter tw)
		{
			var set = new HashSet<SymValue> ();
			var workList = new WorkList<SymValue> ();
			IImmutableMap<SymValue, int> triggers = ImmutableIntKeyMap<SymValue, int>.Empty (SymValue.GetUniqueKey);
			tw.WriteLine ("EGraphId: {0}", this.egraph_id);
			tw.WriteLine ("LastSymbolId: {0}", LastSymbolId);

			foreach (TFunc function in TermMap.Keys2 (this.const_root)) {
				SymValue sv = this [this.const_root, function];
				tw.WriteLine ("{0} = {1}", function, sv);
				workList.Add (sv);
			}

			while (!workList.IsEmpty ()) {
				SymValue sv = workList.Pull ();
				if (!set.Add (sv))
					continue;

				foreach (TFunc function in TermMap.Keys2 (sv)) {
					SymValue target = this [sv, function];

					tw.WriteLine ("{0}({2}) = {1})", function, target, sv);
					workList.Add (target);
				}
				foreach (var edge in MultiEdgeMap.Keys2 (sv)) {
					foreach (SymValue target in MultiEdgeMap [sv, edge].AsEnumerable ()) {
						if (!UpdateTrigger (target, edge, ref triggers))
							continue;
						SymGraphTerm<TFunc> term = EqualMultiTermsMap [target];
						if (term.Args != null) {
							tw.WriteLine ("{0}({1}) = {2}",
							              term.Function,
							              term.Args.ToString (", "), target);
							workList.Add (target);
						}
					}
				}
			}

			tw.WriteLine ("**Abstract value map");
			foreach (SymValue sv in set) {
				TADomain abstractValue = this [sv];
				if (!abstractValue.IsTop)
					tw.WriteLine ("{0} -> {1}", sv, abstractValue);
			}
		}
		#endregion

		#region Implementation of IAbstractDomain<SymGraph<Constant,AbstractValue>>
		public SymGraph<TFunc, TADomain> Meet (SymGraph<TFunc, TADomain> that)
		{
			if (this == that || IsBottom || that.IsTop)
				return this;
			if (that.IsBottom || IsTop)
				return that;

			return this;
		}

		public bool LessEqual (SymGraph<TFunc, TADomain> that)
		{
			IImmutableMap<SymValue, LispList<SymValue>> forwardMap;
			IImmutableMap<SymValue, SymValue> backwardMap;

			return LessEqual (that, out forwardMap, out backwardMap);
		}

		public SymGraph<TFunc, TADomain> ImmutableVersion ()
		{
			MarkAsImmutable ();
			return this;
		}

		public bool LessEqual (SymGraph<TFunc, TADomain> that, 
			out IImmutableMap<SymValue, LispList<SymValue>> forward, 
			out IImmutableMap<SymValue, SymValue> backward)
		{
			if (!IsSameEGraph (that))
				return InternalLessEqual (this, that, out forward, out backward);

			forward = null;
			backward = null;
			return true;
		}
		#endregion

		public bool HasAllBottomFields (SymValue sv)
		{
			if (sv == null)
				return false;

			return this [sv].HasAllBottomFields;
		}

		public SymValue LookupOrManifest (TFunc function, SymValue arg, out bool fresh)
		{
			int oldCnt = IdGenerator;
			SymValue result = this [function, arg];

			fresh = oldCnt < IdGenerator;
			return result;
		}

		public SymValue TryLookup (TFunc function, params SymValue[] args)
		{
			if (args.Length == 0 || args.Length == 1)
				return LookupWithoutManifesting (this.const_root, function);
			return LookupWithoutManifesting (args, function);
		}

		public SymValue LookupWithoutManifesting (SymValue sv, TFunc function)
		{
			if (sv == null)
				return null;
			sv = Find (sv);
			SymValue result = TermMap [sv, function];

			if (result == null)
				return null;
			return Find (result);
		}

		public SymValue LookupWithoutManifesting (SymValue[] args, TFunc function)
		{
			int length = args.Length;
			for (int i = 0; i < length; i++)
				args [i] = Find (args [i]);
			return FindCandidate (args, function);
		}

		public SymValue LookupOrBottomPlaceHolder (SymValue arg, TFunc function, out bool isPlaceHolder)
		{
			SymValue result = LookupWithoutManifesting (arg, function);

			isPlaceHolder = result == null;
			return isPlaceHolder ? this.BottomPlaceHolder : result;
		}

		private SymValue Find (SymValue v)
		{
			SymValue forw = this.forw_map [v];
			if (forw == null)
				return v;

			return Find (forw);
		}

		private bool IsOldSymbol (SymValue sv)
		{
			if (this.Parent == null)
				return false;
			return sv.UniqueId <= this.Parent.LastSymbolId;
		}

		private SymValue FindCandidate (SymValue[] args, TFunc function)
		{
			int length = args.Length;
			var multiEdge = new MultiEdge<TFunc, TADomain> (function, 0, length);
			for (LispList<SymValue> list = MultiEdgeMap [args [0], multiEdge]; list != null; list = list.Tail) {
				SymGraphTerm<TFunc> term = EqualMultiTermsMap [list.Head];
				if (term.Args.Length == length) {
					bool found = true;

					for (int i = 0; i < length; ++i) {
						if (Find (term.Args [i]) != args [i]) {
							found = false;
							break;
						}
					}

					if (found)
						return list.Head;
				}
			}
			return null;
		}

		private bool TryPushEquality (WorkList<EqualityPair<TFunc, TADomain>> workList, SymValue sv1, SymValue sv2)
		{
			if (sv1 != sv2) {
				workList.Add (new EqualityPair<TFunc, TADomain> (sv1, sv2));
				return true;
			}

			return false;
		}

		private void DrainEqualityWorkList (WorkList<EqualityPair<TFunc, TADomain>> workList)
		{
			while (!workList.IsEmpty ()) {
				EqualityPair<TFunc, TADomain> equalityPair = workList.Pull ();
				SymValue sv1 = Find (equalityPair.Sv1);
				SymValue sv2 = Find (equalityPair.Sv2);
				if (sv1 != sv2) {
					if (sv1.UniqueId < sv2.UniqueId) {
						SymValue tmp = sv1;
						sv1 = sv2;
						sv2 = tmp;
					}

					foreach (TFunc function in Functions (sv1)) {
						SymValue v2 = LookupWithoutManifesting (sv2, function);
						if (v2 == null)
							this [sv2, function] = this [sv1, function];
						else
							TryPushEquality (workList, this [sv1, function], v2);
					}
					TADomain thisValue = this [sv1];
					TADomain thatValue = this [sv2];
					foreach (var elem in EqualTermsMap [sv1].AsEnumerable ())
						EqualTermsMap = EqualTermsMap.Add (sv2, EqualTermsMap [sv2].Cons (elem));

					this.forw_map = this.forw_map.Add (sv1, sv2);
					this [sv2] = thisValue.Meet (thatValue);
				}
			}
		}

		private IEnumerable<MultiEdge<TFunc, TADomain>> MultiEdges (SymValue sv)
		{
			return MultiEdgeMap.Keys2 (Find (sv));
		}

		public IEnumerable<SymGraphTerm<TFunc>> EqMultiTerms (SymValue sv)
		{
			SymGraphTerm<TFunc> term = EqualMultiTermsMap [sv];
			if (term.Args != null && IsValidMultiTerm (term))
				yield return term;
		}

		public bool IsValidSymbol (SymValue sv)
		{
			return EqualTermsMap.ContainsKey (sv);
		}

		private bool IsValidMultiTerm (SymGraphTerm<TFunc> term)
		{
			return LookupWithoutManifesting (term.Args, term.Function) != null;
		}

		private static SymGraph<TFunc, TADomain> ComputeCommonTail (SymGraph<TFunc, TADomain> g1, SymGraph<TFunc, TADomain> g2, out int updateSize)
		{
			SymGraph<TFunc, TADomain> graph1 = g1;
			SymGraph<TFunc, TADomain> graph2 = g2;
			while (graph1 != graph2) {
				if (graph1 == null)
					break;
				if (graph2 == null) {
					graph1 = null;
					break;
				}
				if (graph1.history_size > graph2.history_size)
					graph1 = graph1.Parent;
				else if (graph2.history_size > graph1.history_size)
					graph2 = graph2.Parent;
				else {
					graph1 = graph1.Parent;
					graph2 = graph2.Parent;
				}
			}
			SymGraph<TFunc, TADomain> tail = graph1;
			int historySize = tail != null ? tail.history_size : 0;
			updateSize = g1.history_size + g2.history_size - 2*historySize;
			return tail;
		}

		private static bool InternalLessEqual (SymGraph<TFunc, TADomain> thisG, SymGraph<TFunc, TADomain> thatG,
		                                       out IImmutableMap<SymValue, LispList<SymValue>> forward,
		                                       out IImmutableMap<SymValue, SymValue> backward)
		{
			int updateSize;
			SymGraph<TFunc, TADomain> commonTail = ComputeCommonTail (thisG, thatG, out updateSize);
			if (thisG.IsImmutable)
				thisG = thisG.Clone ();

			var workList = new WorkList<EqualityPair<TFunc, TADomain>> ();
			workList.Add (new EqualityPair<TFunc, TADomain> (thisG.const_root, thatG.const_root));
			IImmutableSet<SymValue> backwardManifested = ImmutableSet<SymValue>.Empty (SymValue.GetUniqueKey);
			IImmutableMap<SymValue, SymValue> backwardMap = ImmutableIntKeyMap<SymValue, SymValue>.Empty (SymValue.GetUniqueKey);
			IImmutableMap<SymValue, LispList<SymValue>> forwardMap = ImmutableIntKeyMap<SymValue, LispList<SymValue>>.Empty (SymValue.GetUniqueKey);
			IImmutableMap<SymValue, int> triggers = ImmutableIntKeyMap<SymValue, int>.Empty (SymValue.GetUniqueKey);

			while (!workList.IsEmpty ()) {
				EqualityPair<TFunc, TADomain> equalityPair = workList.Pull ();
				SymValue sv1 = equalityPair.Sv1;
				SymValue sv2 = equalityPair.Sv2;

				SymValue s;
				if (VisitedBefore (sv2, backwardManifested, backwardMap, out s)) {
					if (s != null && s == sv1)
						continue;

					if (DebugOptions.Debug)
						Console.WriteLine ("---LessEqual fails due to pre-existing relation: {0} <- {1}", s, sv2);
					forward = null;
					backward = null;
					return false;
				}

				TADomain val1 = sv1 == null ? thisG.UnderlyingTopValue.ForManifestedField () : thisG [sv1];
				TADomain val2 = thatG [sv2];
				if (!val1.LessEqual (val2)) {
					if (DebugOptions.Debug)
						Console.WriteLine ("---LessEqual fails due to abstract values: !({0} <= {1})", val1, val2);
					forward = null;
					backward = null;
					return false;
				}
				if (sv1 != null) {
					backwardMap = backwardMap.Add (sv2, sv1);
					forwardMap = forwardMap.Add (sv1, forwardMap [sv1].Cons (sv2));
				} else
					backwardManifested = backwardManifested.Add (sv2);
				if (thisG.HasAllBottomFields (sv1))
					continue;
				if (thatG.HasAllBottomFields (sv2)) {
					if (DebugOptions.Debug)
					{
						Console.WriteLine ("---LessEqual fails due to bottom field difference");
					}
					forward = null;
					backward = null;
					return false;
				}

				foreach (TFunc function in thatG.Functions (sv2)) {
					SymValue v1 = thisG [function, sv1];
					SymValue v2 = thatG [function, sv2];
					if (DebugOptions.Debug)
						Console.WriteLine ("    {0}-{1}->{2} <=? {3}-{4}->{5}", sv1, function, v1, sv2, function, v2);
					workList.Add (new EqualityPair<TFunc, TADomain> (v1, v2));
				}

				foreach (var e in thatG.MultiEdges (sv2)) {
					foreach (SymValue sv in thatG.MultiEdgeMap [sv2, e].AsEnumerable ()) {
						if (!UpdateTrigger (sv, e, ref triggers))
							continue;

						SymGraphTerm<TFunc> term = thatG.EqualMultiTermsMap [sv];
						var args = new SymValue[term.Args.Length];
						for (int i = 0; i < args.Length; i++)
							args [i] = backwardMap [term.Args [i]];

						SymValue v1 = thisG.LookupWithoutManifesting (args, e.Function);
						if (v1 == null) {
							if (DebugOptions.Debug)
								Console.WriteLine ("---LessEqual fails due to missing multi term {0}({1})",
								                   e.Function,
								                   string.Join (", ", term.Args.Select (it => it.ToString ())));
							forward = null;
							backward = null;
							return false;
						}

						workList.Add (new EqualityPair<TFunc, TADomain> (v1, sv));
					}
				}
			}
			forward = forwardMap;
			backward = CompleteWithCommon (backwardMap, thisG, commonTail.IdGenerator);
			return true;
		}

		private static IImmutableMap<SymValue, SymValue> CompleteWithCommon (IImmutableMap<SymValue, SymValue> map,
		                                                                     SymGraph<TFunc, TADomain> thisGraph, int lastCommonId)
		{
			IEnumerable<SymValue> symValues = thisGraph.EqualTermsMap.Keys.Concat (thisGraph.EqualMultiTermsMap.Keys);
			foreach (SymValue sv in symValues) {
				if (IsCommon (sv, lastCommonId) && !map.ContainsKey (sv))
					map = map.Add (sv, sv);
			}
			return map;
		}

		private static bool IsCommon (SymValue sv, int lastCommonId)
		{
			return sv.UniqueId <= lastCommonId;
		}

		private static bool UpdateTrigger (SymValue sv, MultiEdge<TFunc, TADomain> edge, ref IImmutableMap<SymValue, int> triggers)
		{
			int val = triggers [sv] + 1;
			triggers = triggers.Add (sv, val);
			return (val == edge.Arity);
		}

		private static bool VisitedBefore (SymValue sv2,
		                                   IImmutableSet<SymValue> backwardManifested,
		                                   IImmutableMap<SymValue, SymValue> backward,
		                                   out SymValue sv1)
		{
			sv1 = backward [sv2];
			return sv1 != null || backwardManifested.Contains (sv2);
		}

		private bool IsSameEGraph (SymGraph<TFunc, TADomain> that)
		{
			if (this == that)
				return true;
			if (that.Parent == this)
				return that.Updates == Updates;

			return false;
		}

		private void MarkAsImmutable ()
		{
			this.is_immutable = true;
		}

		#region Merge updates
		private void AddUpdate (Update<TFunc, TADomain> update)
		{
			Updates = Updates.Cons (update);
		}

		private void AddAbstractValueUpdate (SymValue sv)
		{
			if (!IsOldSymbol (sv))
				return;
			AddUpdate (new AbstractDomainUpdate<TFunc, TADomain> (sv));
		}

		private void AddEqualityUpdate (SymValue sv1, SymValue sv2)
		{
			if (!IsOldSymbol (sv1) || !IsOldSymbol (sv2))
				return;
			AddUpdate (new EqualityUpdate<TFunc, TADomain> (sv1, sv2));
		}

		private void AddEdgeUpdate (SymValue from, TFunc function)
		{
			if (!IsOldSymbol (from))
				return;
			AddUpdate (new EdgeUpdate<TFunc, TADomain> (from, function));
		}

		private void AddEliminateAllUpdate (SymValue from)
		{
			if (!IsOldSymbol (from))
				return;
			foreach (TFunc function in TermMap.Keys2 (from))
				AddUpdate (new EliminateEdgeUpdate<TFunc, TADomain> (from, function));
		}

		private void AddEliminateEdgeUpdate (SymValue from, TFunc function)
		{
			if (!IsOldSymbol (from))
				return;
			AddUpdate (new EliminateEdgeUpdate<TFunc, TADomain> (from, function));
		}

		private void AddMultiEdgeUpdate (SymValue[] from, TFunc function)
		{
			for (int i = 0; i < from.Length; i++) {
				if (!IsOldSymbol (from [i]))
					return;
			}

			AddUpdate (new MultiEdgeUpdate<TFunc, TADomain> (from, function));
		}
		#endregion

		public IImmutableMap<SymValue, LispList<SymValue>> GetForwardIdentityMap ()
		{
			var res = ImmutableIntKeyMap<SymValue, LispList<SymValue>>.Empty (SymValue.GetUniqueKey);
			foreach (var sv in this.EqualTermsMap.Keys.Union (this.EqualMultiTermsMap.Keys)) {
				res = res.Add (sv, LispList<SymValue>.Cons (sv, null));
			}
			return res;
		}
	}
}
