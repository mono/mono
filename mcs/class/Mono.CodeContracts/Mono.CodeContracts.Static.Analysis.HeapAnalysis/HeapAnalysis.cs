// 
// HeapAnalysis.cs
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
using System.IO;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis.SymbolicGraph;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataFlowAnalysis;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	class HeapAnalysis : IAnalysis<APC, Domain, IILVisitor<APC, int, int, Domain, Domain>, Dummy> {
		private readonly Dictionary<Pair<APC, APC>, IImmutableMap<SymbolicValue, LispList<SymbolicValue>>> forwardRenamings =
			new Dictionary<Pair<APC, APC>, IImmutableMap<SymbolicValue, LispList<SymbolicValue>>> ();

		public readonly Dictionary<APC, IMergeInfo> MergeInfoCache = new Dictionary<APC, IMergeInfo> ();
		public readonly DoubleDictionary<APC, APC, Dummy> RenamePoints = new DoubleDictionary<APC, APC, Dummy> ();
		private readonly ICodeLayer<int, int, IStackContextProvider, Dummy> stackLayer;
		private IFixPointInfo<APC, Domain> fixPointInfo;

		public HeapAnalysis (ICodeLayer<int, int, IStackContextProvider, Dummy> stackLayer)
		{
			this.stackLayer = stackLayer;
		}

		public IStackContextProvider StackContextProvider
		{
			get { return this.stackLayer.ILDecoder.ContextProvider; }
		}

		public IMetaDataProvider MetaDataProvider
		{
			get { return this.stackLayer.MetaDataProvider; }
		}

		public IContractProvider ContractProvider
		{
			get { return this.stackLayer.ContractProvider; }
		}

		public Method CurrentMethod
		{
			get { return this.StackContextProvider.MethodContext.CurrentMethod; }
		}

		IILVisitor<APC, int, int, Domain, Domain> IAnalysis<APC, Domain, IILVisitor<APC, int, int, Domain, Domain>, Dummy>.
			GetVisitor ()
		{
			return GetVisitor ();
		}

		public Domain Join (Pair<APC, APC> edge, Domain newstate, Domain prevstate, out bool weaker, bool widen)
		{
			if (DebugOptions.Debug)
			{
				Console.WriteLine ("-----OPT Join at {0}", edge);
				Console.WriteLine ("-----Existing state:");
				prevstate.Dump (Console.Out);
				Console.WriteLine ("-----New state:");
				newstate.Dump (Console.Out);
			}

			IMergeInfo mi;
			Domain domain = prevstate.Join (newstate, widen, out weaker, out mi);
			if (weaker) {
				IMergeInfo mi2;
				if (this.MergeInfoCache.TryGetValue (edge.Value, out mi2) && mi2 == null)
					this.MergeInfoCache [edge.Value] = mi;
			} else
				this.MergeInfoCache [edge.Value] = mi;

			if (DebugOptions.Debug)
			{
				Console.WriteLine ("-----Result state: changed = {0} (widen = {1})", weaker ? 1 : 0, widen ? 1 : 0);
				domain.Dump (Console.Out);
				Console.WriteLine ("----------------------------------------------");
			}

			return domain;
		}

		public Domain ImmutableVersion (Domain arg)
		{
			return arg.ImmutableVersion ();
		}

		public Domain MutableVersion (Domain arg)
		{
			if (arg.IsBottom)
				return arg;
			return arg.Clone ();
		}

		public Domain EdgeConversion (APC @from, APC to, bool isJoinPoint, Dummy data, Domain state)
		{
			if (isJoinPoint) {
				this.RenamePoints [from, to] = Dummy.Value;
				if (!this.MergeInfoCache.ContainsKey (to))
					this.MergeInfoCache.Add (to, null);
			}

			if (DebugOptions.Debug)
			{
				Console.WriteLine ("----Edge conversion on {0}->{1}------", from, to);
				state.Dump (Console.Out);
				Console.WriteLine ("-------------------------------------");
			}

			return state;
		}

		public bool IsBottom (APC pc, Domain state)
		{
			return state.IsBottom;
		}

		public Predicate<APC> SaveFixPointInfo (IFixPointInfo<APC, Domain> fixPointInfo)
		{
			this.fixPointInfo = fixPointInfo;
			return pc => true;
		}

		public void Dump (Pair<Domain, TextWriter> pair)
		{
			pair.Key.Dump (pair.Value);
		}

		private IILVisitor<APC, int, int, Domain, Domain> GetVisitor ()
		{
			return new AnalysisDecoder (this);
		}

		public Domain InitialValue ()
		{
			return new Domain (this);
		}

		public IILDecoder<APC, SymbolicValue, SymbolicValue, IValueContextProvider<SymbolicValue>, IImmutableMap<SymbolicValue, LispList<SymbolicValue>>> 
			GetDecoder<Context>(IILDecoder<APC, int, int, Context, Dummy> underlying)
			where Context : IStackContextProvider
		{
			return new ValueDecoder<Context> (this, underlying);
		}

		public bool IsUnreachable (APC pc)
		{
			Domain domain;
			if (!this.fixPointInfo.PreStateLookup (pc, out domain) || domain.IsBottom)
				return true;

			return false;
		}

		public IImmutableMap<SymbolicValue, LispList<SymbolicValue>> EdgeRenaming (Pair<APC, APC> edge, bool isJoinPoint)
		{
			IImmutableMap<SymbolicValue, LispList<SymbolicValue>> forwardRenaming;

			if (this.forwardRenamings.TryGetValue (edge, out forwardRenaming))
				return forwardRenaming;

			IImmutableMap<SymbolicValue, LispList<SymbolicValue>> renaming = null;
			Domain afterBegin;
			PostStateLookup (edge.Key, out afterBegin);
			if (afterBegin == null || afterBegin.IsBottom)
				return null;
			Domain beforeEnd;
			PreStateLookup (edge.Value, out beforeEnd);
			if (beforeEnd != null) {
				IImmutableMap<SymValue, LispList<SymValue>> forward;
				if (!TryComputeFromJoinCache (afterBegin, beforeEnd, edge.Value, out forward)) {
					IImmutableMap<SymValue, SymValue> backward;
					if (!afterBegin.LessEqual (beforeEnd, out forward, out backward))
						throw new InvalidOperationException ("Should never happen");
					if (isJoinPoint && forward == null)
						forward = afterBegin.GetForwardIdentityMap ();
				}
				if (forward != null) {
					renaming = ImmutableIntKeyMap<SymbolicValue, LispList<SymbolicValue>>.Empty (SymbolicValue.GetUniqueKey);
					foreach (SymValue sv in forward.Keys) {
						LispList<SymbolicValue> targets = null;
						foreach (SymValue target in forward [sv].AsEnumerable ())
							targets = targets.Cons (new SymbolicValue (target));
						if (targets != null)
							renaming = renaming.Add (new SymbolicValue (sv), targets);
					}
				}
			}
			this.forwardRenamings.Add (edge, renaming);
			return renaming;
		}

		private bool TryComputeFromJoinCache (Domain inDomain, Domain outDomain, APC joinPoint, out IImmutableMap<SymValue, LispList<SymValue>> forward)
		{
			forward = null;
			IMergeInfo mi;
			if (this.MergeInfoCache.TryGetValue (joinPoint, out mi) && mi != null && outDomain.IsResultEGraph (mi)) {
				if (inDomain.IsGraph1 (mi)) {
					forward = mi.ForwardG1Map;
					return true;
				}
				if (inDomain.IsGraph2 (mi)) {
					forward = mi.ForwardG2Map;
					return true;
				}
			}
			return false;
		}

		public Domain GetPreState (APC pc)
		{
			Domain domain;
			PreStateLookup (pc, out domain);
			return domain;
		}

		public bool PreStateLookup (APC pc, out Domain ifFound)
		{
			return this.fixPointInfo.PreStateLookup (pc, out ifFound);
		}

		public bool PostStateLookup (APC pc, out Domain ifFound)
		{
			return this.fixPointInfo.PostStateLookup (pc, out ifFound);
		}
	}
}
