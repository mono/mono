// 
// BlockBuilder.cs
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

using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders {
	class BlockBuilder<TLabel> : ILVisitorBase<TLabel, Dummy, Dummy, BlockWithLabels<TLabel>, bool>,
	                             IAggregateVisitor<TLabel, BlockWithLabels<TLabel>, bool> {
		private readonly SubroutineBuilder<TLabel> builder;
		private BlockWithLabels<TLabel> current_block;

		private BlockBuilder (SubroutineBuilder<TLabel> builder)
		{
			this.builder = builder;
		}

		private SubroutineBase<TLabel> CurrentSubroutine
		{
			get { return this.builder.CurrentSubroutine; }
		}

		#region IAggregateVisitor<Label,BlockWithLabels<Label>,bool> Members
		public override bool Branch (TLabel pc, TLabel target, bool leavesExceptionBlock, BlockWithLabels<TLabel> currentBlock)
		{
			currentBlock.AddLabel (pc);
			CurrentSubroutine.AddSuccessor (currentBlock, EdgeTag.Branch, CurrentSubroutine.GetTargetBlock (target));

			return true;
		}

		public override bool BranchCond (TLabel pc, TLabel target, BranchOperator bop, Dummy value1, Dummy value2, BlockWithLabels<TLabel> currentBlock)
		{
			return HandleConditionalBranch (pc, target, true, currentBlock);
		}

		public override bool BranchFalse (TLabel pc, TLabel target, Dummy cond, BlockWithLabels<TLabel> currentBlock)
		{
			return HandleConditionalBranch (pc, target, false, currentBlock);
		}

		public override bool BranchTrue (TLabel pc, TLabel target, Dummy cond, BlockWithLabels<TLabel> currentBlock)
		{
			return HandleConditionalBranch (pc, target, true, currentBlock);
		}

		public override bool Throw (TLabel pc, Dummy exception, BlockWithLabels<TLabel> currentBlock)
		{
			currentBlock.AddLabel (pc);
			return true;
		}

		public override bool Rethrow (TLabel pc, BlockWithLabels<TLabel> currentBlock)
		{
			currentBlock.AddLabel (pc);
			return true;
		}

		public override bool EndFinally (TLabel pc, BlockWithLabels<TLabel> currentBlock)
		{
			currentBlock.AddLabel (pc);
			CurrentSubroutine.AddSuccessor (currentBlock, EdgeTag.EndSubroutine, CurrentSubroutine.Exit);
			return true;
		}

		public override bool Return (TLabel pc, Dummy source, BlockWithLabels<TLabel> currentBlock)
		{
			currentBlock.AddLabel (pc);
			CurrentSubroutine.AddSuccessor (currentBlock, EdgeTag.Return, CurrentSubroutine.Exit);
			CurrentSubroutine.AddReturnBlock (currentBlock);

			return true;
		}

		public override bool Nop (TLabel pc, BlockWithLabels<TLabel> currentBlock)
		{
			return false;
		}

		public override bool LoadField (TLabel pc, Field field, Dummy dest, Dummy obj, BlockWithLabels<TLabel> data)
		{
			if (CurrentSubroutine.IsMethod) {
				var methodInfo = (IMethodInfo) CurrentSubroutine;
				Property property;
				if (this.builder.MetaDataProvider.IsPropertyGetter (methodInfo.Method, out property))
					this.builder.SubroutineFacade.AddReads (methodInfo.Method, field);
			}
			this.current_block.AddLabel (pc);
			return false;
		}

		public override bool StoreField (TLabel pc, Field field, Dummy obj, Dummy value, BlockWithLabels<TLabel> data)
		{
			if (CurrentSubroutine.IsMethod) {
				var methodInfo = (IMethodInfo) CurrentSubroutine;
				Property property;
				if (this.builder.MetaDataProvider.IsPropertySetter (methodInfo.Method, out property))
					this.builder.SubroutineFacade.AddReads (methodInfo.Method, field);
			}
			this.current_block.AddLabel (pc);
			return false;
		}

		public override bool EndOld (TLabel pc, TLabel matchingBegin, TypeNode type, Dummy dest, Dummy source, BlockWithLabels<TLabel> data)
		{
			this.current_block.AddLabel (pc);
			CurrentSubroutine.AddSuccessor (this.current_block, EdgeTag.EndOld, CurrentSubroutine.Exit);
			return false;
		}

		public bool Aggregate (TLabel pc, TLabel aggregateStart, bool canBeTargetOfBranch, BlockWithLabels<TLabel> data)
		{
			TraceAggregateSequentally (aggregateStart);
			return false;
		}
		#endregion

		public static BlockWithLabels<TLabel> BuildBlocks (TLabel entry, SubroutineBuilder<TLabel> subroutineBuilder)
		{
			var blockBuilder = new BlockBuilder<TLabel> (subroutineBuilder);
			blockBuilder.TraceAggregateSequentally (entry);
			if (blockBuilder.current_block == null)
				return null;

			SubroutineBase<TLabel> subroutine = blockBuilder.CurrentSubroutine;

			subroutine.AddSuccessor (blockBuilder.current_block, EdgeTag.FallThroughReturn, subroutine.Exit);
			subroutine.AddReturnBlock (blockBuilder.current_block);

			return blockBuilder.current_block;
		}

		private void TraceAggregateSequentally (TLabel currentLabel)
		{
			do {
				if (this.builder.IsBlockStart (currentLabel))
					this.current_block = this.builder.RecordInformationForNewBlock (currentLabel, this.current_block);
				if (this.builder.CodeProvider.Decode<BlockBuilder<TLabel>, BlockWithLabels<TLabel>, bool> (currentLabel, this, this.current_block))
					this.current_block = null;
			} while (this.builder.CodeProvider.Next (currentLabel, out currentLabel));
		}

		public override bool DefaultVisit (TLabel pc, BlockWithLabels<TLabel> currentBlock)
		{
			currentBlock.AddLabel (pc);
			return false;
		}

		private bool HandleConditionalBranch (TLabel pc, TLabel target, bool isTrueBranch, BlockWithLabels<TLabel> currentBlock)
		{
			currentBlock.AddLabel (pc);
			EdgeTag trueTag = isTrueBranch ? EdgeTag.True : EdgeTag.False;
			EdgeTag falseTag = isTrueBranch ? EdgeTag.False : EdgeTag.True;

			AssumeBlock<TLabel> trueBlock = CurrentSubroutine.NewAssumeBlock (pc, trueTag);
			this.builder.RecordInformationSameAsOtherBlock (trueBlock, this.current_block);
			CurrentSubroutine.AddSuccessor (currentBlock, trueTag, trueBlock);
			CurrentSubroutine.AddSuccessor (trueBlock, EdgeTag.FallThrough, CurrentSubroutine.GetTargetBlock (target));

			AssumeBlock<TLabel> falseBlock = CurrentSubroutine.NewAssumeBlock (pc, falseTag);
			this.builder.RecordInformationSameAsOtherBlock (falseBlock, this.current_block);
			CurrentSubroutine.AddSuccessor (currentBlock, falseTag, falseBlock);
			this.current_block = falseBlock;

			return false;
		}
	}
}
