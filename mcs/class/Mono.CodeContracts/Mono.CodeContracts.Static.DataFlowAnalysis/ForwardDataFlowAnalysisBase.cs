// 
// ForwardDataFlowAnalysisBase.cs
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
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.DataFlowAnalysis {
	abstract class ForwardDataFlowAnalysisBase<AState> : DataFlowAnalysisBase<AState> {
		private readonly Dictionary<APC, AState> post_state = new Dictionary<APC, AState> ();

		protected ForwardDataFlowAnalysisBase (ICFG cfg) : base (cfg)
		{
		}

		public bool GetPreState (APC pc, out AState state)
		{
			bool noInfo;
			state = GetPreState (pc, default(AState), out noInfo);
			return !noInfo;
		}

		public AState GetPreStateWithDefault (APC apc, AState ifMissing)
		{
			bool noInfo;
			AState preState = GetPreState (apc, default(AState), out noInfo);
			return noInfo ? ifMissing : preState;
		}

		private AState GetPreState (APC apc, AState ifMissing, out bool noInfo)
		{
			Sequence<APC> rest = null;
			APC tmp = apc;
			APC singlePredecessor;
			AState state;
			bool weHaveState;
			while (!(weHaveState = this.JoinState.TryGetValue (tmp, out state)) &&
			       !RequiresJoining (tmp) && this.CFG.HasSinglePredecessor (tmp, out singlePredecessor)) {
				tmp = singlePredecessor;

				rest = rest.Cons (tmp);
			}

			if (!weHaveState) {
				noInfo = true;
				return ifMissing;
			}

			bool listWasNotEmpty = rest != null;
			while (rest != null) {
				if (IsBottom (rest.Head, state)) {
					noInfo = false;
					return state;
				}
				state = MutableVersion (state, rest.Head);
				state = Transfer (rest.Head, state);
				if (IsBottom (rest.Head, state)) {
					noInfo = false;
					return state;
				}

				rest = rest.Tail;
				if (rest != null)
					this.JoinState.Add (rest.Head, ImmutableVersion (state, rest.Head));
			}

			if (listWasNotEmpty)
				this.JoinState.Add (apc, ImmutableVersion (state, apc));

			noInfo = false;
			return state;
		}

		public bool GetPostState (APC apc, out AState result)
		{
			if (this.post_state.TryGetValue (apc, out result))
				return true;

			APC singleSuccessor;
			if (apc.Block.Count <= apc.Index)
				return GetPreState (apc, out result);

			if (this.CFG.HasSingleSuccessor (apc, out singleSuccessor) && !RequiresJoining (singleSuccessor))
				return GetPreState (singleSuccessor, out result);

			AState ifFound;
			if (!GetPreState (apc, out ifFound))
				return false;

			result = MutableVersion (ifFound, apc);
			result = Transfer (apc, result);

			this.post_state.Add (apc, result);
			return true;
		}

		public void Run (AState startState)
		{
			Initialize (this.CFG.Entry, startState);
			ComputeFixPoint ();
		}

		protected override int WorkingListComparer (APC a, APC b)
		{
			return b.Block.ReversePostOrderIndex - a.Block.ReversePostOrderIndex;
		}

		protected override bool RequiresJoining (APC pc)
		{
			return this.CFG.IsJoinPoint (pc);
		}

		protected override bool HasSingleSuccessor (APC pc, out APC next)
		{
			return this.CFG.HasSingleSuccessor (pc, out next);
		}

		protected override IEnumerable<APC> Successors (APC pc)
		{
			return this.CFG.Successors (pc);
		}

		protected override bool IsBackEdge (APC from, APC to)
		{
		    return CFG.IsForwardBackEdge (from, to);
		}
	}
}
