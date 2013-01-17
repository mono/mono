// 
// OldValueSubroutine.cs
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
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines {
	class OldValueSubroutine<TLabel> : MethodContractSubroutine<TLabel> {
		private BlockWithLabels<TLabel> begin_old_block;
		private BlockWithLabels<TLabel> end_old_block;

		public OldValueSubroutine (SubroutineFacade subroutineFacade, Method method,
		                           SimpleSubroutineBuilder<TLabel> builder, TLabel startLabel)
			: base (subroutineFacade, method, builder, startLabel)
		{
		}

		public override SubroutineKind Kind
		{
			get { return SubroutineKind.Old; }
		}

		public override int StackDelta
		{
			get { return 1; }
		}

		public override bool IsOldValue
		{
			get { return true; }
		}

		public override void Initialize ()
		{
		}

		public void Commit (BlockWithLabels<TLabel> endOldBlock)
		{
			this.end_old_block = endOldBlock;
			base.Commit ();
		}

		public void RegisterBeginBlock (BlockWithLabels<TLabel> beginOldBlock)
		{
			this.begin_old_block = beginOldBlock;
		}

		public APC BeginOldAPC (Sequence<Edge<CFGBlock, EdgeTag>> context)
		{
			return new APC (this.begin_old_block, 0, context);
		}

		public APC EndOldAPC (Sequence<Edge<CFGBlock, EdgeTag>> context)
		{
			return new APC (this.end_old_block, this.end_old_block.Count - 1, context);
		}
	}
}
