// 
// SimpleSubroutineBuilder.cs
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
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders {
	class SimpleSubroutineBuilder<TLabel> : SubroutineBuilder<TLabel> {
		private readonly HashSet<TLabel> begin_old_start = new HashSet<TLabel> ();
		private readonly HashSet<TLabel> end_old_start = new HashSet<TLabel> ();

		private BlockWithLabels<TLabel> block_prior_to_old;
		private OldValueSubroutine<TLabel> current_old_subroutine;
		private SubroutineBase<TLabel> current_subroutine;

		public SimpleSubroutineBuilder (ICodeProvider<TLabel> codeProvider,
		                                SubroutineFacade subroutineFacade,
		                                TLabel entry)
			: base (codeProvider, subroutineFacade, entry)
		{
			Initialize (entry);
		}

		public override SubroutineBase<TLabel> CurrentSubroutine
		{
			get
			{
				if (this.current_old_subroutine != null)
					return this.current_old_subroutine;
				return this.current_subroutine;
			}
		}

		public BlockWithLabels<TLabel> BuildBlocks (TLabel entry, SubroutineBase<TLabel> subroutine)
		{
			this.current_subroutine = subroutine;

			return base.BuildBlocks (entry);
		}

		public override BlockWithLabels<TLabel> RecordInformationForNewBlock (TLabel currentLabel, BlockWithLabels<TLabel> previousBlock)
		{
			TLabel label;
			if (previousBlock != null && previousBlock.TryGetLastLabel (out label) && this.end_old_start.Contains (label)) {
				OldValueSubroutine<TLabel> oldValueSubroutine = this.current_old_subroutine;
				oldValueSubroutine.Commit (previousBlock);
				this.current_old_subroutine = null;
				BlockWithLabels<TLabel> result = base.RecordInformationForNewBlock (currentLabel, this.block_prior_to_old);
				CurrentSubroutine.AddEdgeSubroutine (this.block_prior_to_old, result, oldValueSubroutine, EdgeTag.Old);
				return result;
			}

			if (!this.begin_old_start.Contains (currentLabel))
				return base.RecordInformationForNewBlock (currentLabel, previousBlock);
			this.current_old_subroutine = new OldValueSubroutine<TLabel> (this.SubroutineFacade,
			                                                             ((MethodContractSubroutine<TLabel>) this.current_subroutine).Method,
			                                                             this, currentLabel);
			this.block_prior_to_old = previousBlock;
			BlockWithLabels<TLabel> newBlock = base.RecordInformationForNewBlock (currentLabel, null);
			this.current_old_subroutine.RegisterBeginBlock (newBlock);
			return newBlock;
		}

		public override void BeginOldHook (TLabel label)
		{
			this.begin_old_start.Add (label);
		}

		public override void EndOldHook (TLabel label)
		{
			this.end_old_start.Add (label);
		}
	}
}
