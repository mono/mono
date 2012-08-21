// 
// ForwardAnalysis.cs
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
using System.IO;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.DataFlowAnalysis {
	class ForwardAnalysis<AbstractState, EdgeData> :
		ForwardDataFlowAnalysisBase<AbstractState>,
		IFixPointInfo<APC, AbstractState> {
		private readonly Action<Pair<AbstractState, TextWriter>> dumper;
		private readonly EdgeConverter<APC, AbstractState, EdgeData> edge_converter;
		private readonly Func<APC, APC, EdgeData> edge_data_getter;
		private readonly Func<AbstractState, AbstractState> immutable_version;
		private readonly Func<APC, AbstractState, bool> is_bottom;
		private readonly Joiner<APC, AbstractState> joiner;
		private readonly Func<AbstractState, AbstractState> mutable_version;
		private readonly Func<APC, AbstractState, AbstractState> transfer;

		public ForwardAnalysis (ICFG cfg,
		                        Func<APC, AbstractState, AbstractState> transfer,
		                        Joiner<APC, AbstractState> joiner,
		                        Func<AbstractState, AbstractState> immutableVersion,
		                        Func<AbstractState, AbstractState> mutableVersion,
		                        EdgeConverter<APC, AbstractState, EdgeData> edgeConverter,
		                        Func<APC, APC, EdgeData> edgeDataGetter,
		                        Func<APC, AbstractState, bool> isBottom,
		                        Action<Pair<AbstractState, TextWriter>> dumper) : base (cfg)
		{
			this.transfer = transfer;
			this.joiner = joiner;
			this.immutable_version = immutableVersion;
			this.mutable_version = mutableVersion;
			this.edge_converter = edgeConverter;
			this.edge_data_getter = edgeDataGetter;
			this.is_bottom = isBottom;
			this.dumper = dumper;
		}

		#region IFixPointInfo<APC,AbstractState> Members
		public bool PreStateLookup (APC pc, out AbstractState state)
		{
			return GetPreState (pc, out state);
		}

		public bool PostStateLookup (APC pc, out AbstractState state)
		{
			return GetPostState (pc, out state);
		}
		#endregion

		public static ForwardAnalysis<AbstractState, EdgeData> Make<Source, Dest, Context> (
			IILDecoder<APC, Source, Dest, Context, EdgeData> decoder,
			IAnalysis<APC, AbstractState, IILVisitor<APC, Source, Dest, AbstractState, AbstractState>, EdgeData> analysis)
			where Context : IMethodContextProvider
		{
			IILVisitor<APC, Source, Dest, AbstractState, AbstractState> visitor = analysis.GetVisitor ();
			var forwardAnalysisSolver = new ForwardAnalysis<AbstractState, EdgeData> (
				decoder.ContextProvider.MethodContext.CFG,
				(pc, state) => decoder.ForwardDecode<AbstractState, AbstractState, IILVisitor<APC, Source, Dest, AbstractState, AbstractState>> (pc, visitor, state),
				analysis.Join,
				analysis.ImmutableVersion,
				analysis.MutableVersion,
				analysis.EdgeConversion,
				decoder.EdgeData,
				(pc, state) => {
					if (!decoder.IsUnreachable (pc))
						return analysis.IsBottom (pc, state);

					return true;
				}, 
				analysis.Dump
				);

			analysis.SaveFixPointInfo (forwardAnalysisSolver);
			return forwardAnalysisSolver;
		}

		protected override void Dump (AbstractState state)
		{
			this.dumper (new Pair<AbstractState, TextWriter> (state, Console.Out));
		}

		protected override void PushState (APC from, APC next, AbstractState state)
		{
			EdgeData data = this.edge_data_getter (from, next);
			AbstractState pushState = this.edge_converter (from, next, RequiresJoining (next), data, state);
			base.PushState (from, next, pushState);
		}

		protected override bool Join (Pair<APC, APC> edge, AbstractState newState, AbstractState existingState, out AbstractState joinedState, bool widen)
		{
			bool weaker;
			joinedState = this.joiner (edge, newState, existingState, out weaker, widen);

			return weaker;
		}

		protected override bool IsBottom (APC pc, AbstractState state)
		{
			return this.is_bottom (pc, state);
		}

		protected override AbstractState Transfer (APC pc, AbstractState state)
		{
			AbstractState resultState = this.transfer (pc, state);

			return resultState;
		}

		protected override AbstractState MutableVersion (AbstractState state, APC at)
		{
			return this.mutable_version (state);
		}

		protected override AbstractState ImmutableVersion (AbstractState state, APC at)
		{
			return this.immutable_version (state);
		}
	}
}
