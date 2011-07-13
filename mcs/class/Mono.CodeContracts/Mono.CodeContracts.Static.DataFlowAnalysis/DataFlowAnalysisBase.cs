// 
// DataFlowAnalysisBase.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.DataFlowAnalysis {
	abstract class DataFlowAnalysisBase<AState> :
		IEqualityComparer<APC> {
		protected ICFG CFG;
		protected Dictionary<APC, AState> JoinState;
		protected PriorityQueue<APC> pending;
		private IWidenStrategy widen_strategy;

		protected DataFlowAnalysisBase (ICFG cfg)
		{
			this.CFG = cfg;
			this.pending = new PriorityQueue<APC> (WorkingListComparer);
			this.JoinState = new Dictionary<APC, AState> (this);
			this.widen_strategy = null;
		}

		#region IEqualityComparer<APC> Members
		bool IEqualityComparer<APC>.Equals (APC x, APC y)
		{
			return x.Equals (y);
		}

		int IEqualityComparer<APC>.GetHashCode (APC obj)
		{
			return obj.GetHashCode ();
		}
		#endregion

		public void Initialize (APC entryPoint, AState state)
		{
			this.JoinState.Add (entryPoint, state);
			this.pending.Enqueue (entryPoint);
		}

		public virtual void ComputeFixPoint ()
		{
			this.widen_strategy = new EdgeBasedWidening (20);

			while (this.pending.Count > 0) {
				APC next = this.pending.Dequeue ();
				AState state = MutableVersion (this.JoinState [next], next);

				APC cur;
				bool repeatOuter = false;
				do {
					cur = next;
					if (!IsBottom (cur, state)) {
						state = Transfer (cur, state);
					} else {
						repeatOuter = true;
						break;
					}
				} while (HasSingleSuccessor (cur, out next) && !RequiresJoining (next));

				if (repeatOuter)
					continue;

				foreach (APC successorAPC in Successors (cur)) {
					if (!IsBottom (successorAPC, state))
						PushState (cur, successorAPC, state);
				}
			}
		}

		protected virtual void Dump (AState state)
		{
		}

		public IEnumerable<KeyValuePair<APC, AState>> States ()
		{
			return this.JoinState;
		}

		protected abstract IEnumerable<APC> Successors (APC pc);

		protected virtual void PushState (APC current, APC next, AState state)
		{
			state = ImmutableVersion (state, next);
			if (RequiresJoining (next)) {
				if (!JoinStateAtBlock (new Pair<APC, APC> (current, next), state))
					return;
				this.pending.Enqueue (next);
			} else {
				this.JoinState [next] = state;
				this.pending.Enqueue (next);
			}
		}

		private bool JoinStateAtBlock (Pair<APC, APC> edge, AState state)
		{
			AState existingState;
			if (this.JoinState.TryGetValue (edge.Value, out existingState)) {
				bool widen = this.widen_strategy.WantToWiden (edge.Key, edge.Value, IsBackEdge (edge.Key, edge.Value));
				AState joinedState;
				bool result = Join (edge, state, existingState, out joinedState, widen);
				if (result)
					this.JoinState [edge.Value] = ImmutableVersion (joinedState, edge.Value);
				return result;
			}

			this.JoinState.Add (edge.Value, state);
			return true;
		}

		protected abstract bool IsBackEdge (APC from, APC to);

		protected abstract int WorkingListComparer (APC a, APC b);

		protected abstract bool Join (Pair<APC, APC> edge, AState newState, AState existingState, out AState joinedState, bool widen);

		protected abstract bool RequiresJoining (APC pc);

		protected abstract bool HasSingleSuccessor (APC pc, out APC next);

		protected abstract bool IsBottom (APC pc, AState state);

		protected abstract AState Transfer (APC pc, AState state);

		protected abstract AState MutableVersion (AState state, APC at);
		protected abstract AState ImmutableVersion (AState state, APC at);

		public void PrintStatesAtJoinPoints (TextWriter tw)
		{
			foreach (APC apc in this.JoinState.Keys) {
				string str = this.JoinState [apc].ToString ().Replace (Environment.NewLine, Environment.NewLine + "   ");
				tw.WriteLine ("Block {0}, PC {1}: {2}", apc.Block, apc.Index, str);
			}
		}
	}
}
