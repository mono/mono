// 
// BlockStartGatherer.cs
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
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines.Builders {
	class BlockStartGatherer<Label> : ILVisitorBase<Label, Dummy, Dummy, Dummy, bool>,
	                                           IAggregateVisitor<Label, Dummy, bool> {
		private readonly SubroutineBuilder<Label> parent;

		public BlockStartGatherer (SubroutineBuilder<Label> parent)
		{
			this.parent = parent;
		}

		#region IAggregateVisitor<Label,Dummy,bool> Members
		public override bool Branch (Label pc, Label target, bool leavesExceptionBlock, Dummy data)
		{
			AddTargetLabel (target);
			return true;
		}

		public override bool BranchCond (Label pc, Label target, BranchOperator bop, Dummy value1, Dummy value2, Dummy data)
		{
			AddTargetLabel (target);
			return true;
		}

		public override bool BranchFalse (Label pc, Label target, Dummy cond, Dummy data)
		{
			AddTargetLabel (target);
			return true;
		}

		public override bool BranchTrue (Label pc, Label target, Dummy cond, Dummy data)
		{
			AddTargetLabel (target);
			return true;
		}

		public override bool EndFinally (Label pc, Dummy data)
		{
			return true;
		}

		public override bool Return (Label pc, Dummy source, Dummy data)
		{
			return true;
		}

		public override bool Rethrow (Label pc, Dummy data)
		{
			return true;
		}

		public override bool Throw (Label pc, Dummy exception, Dummy data)
		{
			return true;
		}

		public override bool Call<TypeList, ArgList> (Label pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, Dummy data)
		{
			return CallHelper (pc, method, false, virt);
		}

		public override bool ConstrainedCallvirt<TypeList, ArgList> (Label pc, Method method, TypeNode constraint, TypeList extraVarargs, Dummy dest, ArgList args, Dummy data)
		{
			return CallHelper (pc, method, false, true);
		}

		public override bool NewObj<ArgList> (Label pc, Method ctor, Dummy dest, ArgList args, Dummy data)
		{
			return CallHelper (pc, ctor, true, false);
		}

		public override bool BeginOld (Label pc, Label matchingEnd, Dummy data)
		{
			AddTargetLabel (pc);
			this.parent.BeginOldHook (pc);
			return false;
		}

		public override bool EndOld (Label pc, Label matchingBegin, TypeNode type, Dummy dest, Dummy source, Dummy data)
		{
			this.parent.EndOldHook (pc);
			return true;
		}

		public bool Aggregate (Label pc, Label aggregateStart, bool canBeTargetOfBranch, Dummy data)
		{
			return TraceAggregateSequentally (aggregateStart);
		}
		#endregion

		public override bool DefaultVisit (Label pc, Dummy data)
		{
			return false;
		}

		private bool CallHelper (Label pc, Method method, bool isNewObj, bool isVirtual)
		{
			AddBlockStart (pc);
			if (isNewObj)
				this.parent.AddNewObjSite (pc, method);
			else
				this.parent.AddMethodCallSite (pc, new Pair<Method, bool> (method, isVirtual));

			return true;
		}

		public bool TraceAggregateSequentally (Label current)
		{
			bool isCurrentBranches;
			bool isCurrentHasSuccessor;
			do {
				ICodeProvider<Label> codeProvider = this.parent.CodeProvider;
				isCurrentBranches = codeProvider.Decode<BlockStartGatherer<Label>, Dummy, bool> (current, this, Dummy.Value);
				isCurrentHasSuccessor = codeProvider.Next (current, out current);
				if (isCurrentBranches && isCurrentHasSuccessor)
					AddBlockStart (current);
			} while (isCurrentHasSuccessor);

			return isCurrentBranches;
		}

		private void AddBlockStart (Label target)
		{
			this.parent.AddBlockStart (target);
		}

		private void AddTargetLabel (Label target)
		{
			this.parent.AddTargetLabel (target);
		}
	}
}
