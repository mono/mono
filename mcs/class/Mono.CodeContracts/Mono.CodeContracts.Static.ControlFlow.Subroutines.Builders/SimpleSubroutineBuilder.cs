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
	class SimpleSubroutineBuilder<Label> : SubroutineBuilder<Label> {
		private readonly HashSet<Label> beginOldStart = new HashSet<Label> ();
		private readonly HashSet<Label> endOldStart = new HashSet<Label> ();

		private BlockWithLabels<Label> block_prior_to_old;
		private OldValueSubroutine<Label> current_old_subroutine;
		private SubroutineBase<Label> current_subroutine;

		public SimpleSubroutineBuilder (ICodeProvider<Label> codeProvider,
		                                SubroutineFacade subroutineFacade,
		                                Label entry)
			: base (codeProvider, subroutineFacade, entry)
		{
			Initialize (entry);
		}

		public override SubroutineBase<Label> CurrentSubroutine
		{
			get
			{
				if (this.current_old_subroutine != null)
					return this.current_old_subroutine;
				return this.current_subroutine;
			}
		}

		public BlockWithLabels<Label> BuildBlocks (Label entry, SubroutineBase<Label> subroutine)
		{
			this.current_subroutine = subroutine;

			return base.BuildBlocks (entry);
		}

		public override BlockWithLabels<Label> RecordInformationForNewBlock (Label currentLabel, BlockWithLabels<Label> previousBlock)
		{
			Label label;
			if (previousBlock != null && previousBlock.TryGetLastLabel (out label) && this.endOldStart.Contains (label)) {
				OldValueSubroutine<Label> oldValueSubroutine = this.current_old_subroutine;
				oldValueSubroutine.Commit (previousBlock);
				this.current_old_subroutine = null;
				BlockWithLabels<Label> result = base.RecordInformationForNewBlock (currentLabel, this.block_prior_to_old);
				CurrentSubroutine.AddEdgeSubroutine (this.block_prior_to_old, result, oldValueSubroutine, EdgeTag.Old);
				return result;
			}

			if (!this.beginOldStart.Contains (currentLabel))
				return base.RecordInformationForNewBlock (currentLabel, previousBlock);
			this.current_old_subroutine = new OldValueSubroutine<Label> (this.SubroutineFacade,
			                                                             ((MethodContractSubroutine<Label>) this.current_subroutine).Method,
			                                                             this, currentLabel);
			this.block_prior_to_old = previousBlock;
			BlockWithLabels<Label> newBlock = base.RecordInformationForNewBlock (currentLabel, null);
			this.current_old_subroutine.RegisterBeginBlock (newBlock);
			return newBlock;
		}

		public override void BeginOldHook (Label label)
		{
			this.beginOldStart.Add (label);
		}

		public override void EndOldHook (Label label)
		{
			this.endOldStart.Add (label);
		}
	}
}
