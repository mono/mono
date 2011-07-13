// 
// OldScanStateMachine.cs
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
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow.Blocks;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Subroutines {
	class OldScanStateMachine<Label> : ILVisitorBase<Label, Dummy, Dummy, int, bool>,
	                                        IAggregateVisitor<Label, int, bool> {
		private readonly Dictionary<CFGBlock, ScanState> block_start_state = new Dictionary<CFGBlock, ScanState> ();
		private readonly EnsuresSubroutine<Label> subroutine;
		private EnsuresBlock<Label> current_block;
		private TypeNode next_end_old_type;
		private ScanState state;

		public OldScanStateMachine (EnsuresSubroutine<Label> ensuresSubroutine)
		{
			this.subroutine = ensuresSubroutine;
			this.state = ScanState.OutsideOld;
		}

		#region Overrides of ILVisitorBase<Label,Local,Parameter,Method,Field,Type,Dummy,Dummy,int,bool>
		public override bool DefaultVisit (Label pc, int index)
		{
			if (this.state != ScanState.InsertingOld)
				return this.current_block.UsesOverriding;

			this.state = ScanState.OutsideOld;
			this.current_block.EndOldWithoutInstruction (this.subroutine.SubroutineFacade.MetaDataProvider.ManagedPointer (this.next_end_old_type));
			return true;
		}
		#endregion

		#region Implementation of ICodeQuery<Label,Local,Parameter,Method,Field,Type,int,bool>
		public bool Aggregate (Label pc, Label aggregateStart, bool canBeTargetOfBranch, int data)
		{
			return Nop (pc, data);
		}
		#endregion

		#region IAggregateVisitor<Label,int,bool> Members
		public override bool Nop (Label pc, int data)
		{
			return this.current_block.UsesOverriding;
		}

		public override bool BeginOld (Label pc, Label matchingEnd, int data)
		{
			this.state = ScanState.InsideOld;
			return this.current_block.UsesOverriding;
		}

		public override bool EndOld (Label pc, Label matchingEnd, TypeNode type, Dummy dest, Dummy source, int data)
		{
			this.state = ScanState.OutsideOld;
			return this.current_block.UsesOverriding;
		}

		public override bool LoadField (Label pc, Field field, Dummy dest, Dummy obj, int data)
		{
			if (this.state != ScanState.InsertingOld)
				return this.current_block.UsesOverriding;

			this.state = ScanState.OutsideOld;
			this.current_block.EndOld (data, this.subroutine.SubroutineFacade.MetaDataProvider.FieldType (field));
			return false;
		}

		public override bool LoadFieldAddress (Label pc, Field field, Dummy dest, Dummy obj, int data)
		{
			if (this.state == ScanState.InsertingOld)
				this.next_end_old_type = this.subroutine.SubroutineFacade.MetaDataProvider.FieldType (field);
			return this.current_block.UsesOverriding;
		}

		public override bool Call<TypeList, ArgList> (Label pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, int data)
		{
			return false;
		}
		#endregion

		public void HandlePotentialCallBlock (MethodCallBlock<Label> block, EnsuresBlock<Label> priorBlock)
		{
			if (block == null || this.state != ScanState.InsertingOld)
				return;

			int count = this.subroutine.SubroutineFacade.MetaDataProvider.Parameters (block.CalledMethod).Count;
			if (!this.subroutine.SubroutineFacade.MetaDataProvider.IsStatic (block.CalledMethod))
				++count;
			if (count > 1) {
				this.state = ScanState.OutsideOld;
				TypeNode mp = this.subroutine.SubroutineFacade.MetaDataProvider.ManagedPointer (this.next_end_old_type);
				priorBlock.EndOldWithoutInstruction (mp);
			} else {
				this.state = ScanState.InsertingOldAfterCall;
				this.next_end_old_type = this.subroutine.SubroutineFacade.MetaDataProvider.ReturnType (block.CalledMethod);
			}
		}

		public void StartBlock (EnsuresBlock<Label> block)
		{
			if (!this.block_start_state.TryGetValue (block, out this.state)) {
				this.state = ScanState.OutsideOld;
				this.block_start_state.Add (block, this.state);
			}
			if (this.state == ScanState.InsertingOld)
				block.StartOverridingLabels ();
			if (this.state == ScanState.InsertingOldAfterCall) {
				block.StartOverridingLabels ();
				block.EndOldWithoutInstruction (this.next_end_old_type);
				this.state = ScanState.OutsideOld;
			}
			this.current_block = block;
		}

		public void SetStartState (CFGBlock succ)
		{
			ScanState sstate;
			if (!this.block_start_state.TryGetValue (succ, out sstate))
				this.block_start_state.Add (succ, this.state);
		}

		#region Nested type: ScanState
		private enum ScanState {
			OutsideOld,
			InsideOld,
			InsertingOld,
			InsertingOldAfterCall
		}
		#endregion
	                                        }
}