// 
// Branch.cs
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

namespace Mono.CodeContracts.Static.AST {
	class Branch : Statement {
		public readonly bool LeavesExceptionBlock;
		public Expression Condition;
		public Block Target;
		public bool IsShortOffset;
		public bool Unsigned;

		public Branch (Expression condition, Block target, bool isShortOffset, bool unsigned, bool leavesExceptionBlock) : base (NodeType.Branch)
		{
			this.Condition = condition;
			this.Target = target;
			this.IsShortOffset = isShortOffset;
			this.Unsigned = unsigned;
			this.LeavesExceptionBlock = leavesExceptionBlock;
		}

		public override string ToString ()
		{
			return string.Format ("Branch({0}, {1})", this.Condition == null ? "<no cond>" : this.Condition.ToString (), this.Target == null ? "<no target>" : this.Target.ToString ());
		}
	}

	enum BranchOperator {
		Beq,
		Bge,
		Bge_Un,
		Bgt,
		Bgt_Un,
		Ble,
		Ble_Un,
		Blt,
		Blt_Un,
		Bne_un
	}
}
