// 
// LabelAdapter.cs
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
using Mono.CodeContracts.Static.ControlFlow.Subroutines;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.ControlFlow.Blocks {
	struct LabelAdapter<Label, Data, Result, Visitor> :
		IAggregateVisitor<Label, Data, Result>
		where Visitor : IILVisitor<APC, Dummy, Dummy, Data, Result> {
		private readonly APC original_pc;
		private readonly Visitor visitor;

		public LabelAdapter (Visitor visitor, APC pc)
		{
			this.visitor = visitor;
			this.original_pc = pc;
		}

		#region IAggregateVisitor<Label,Data,Result> Members
		public Result Binary (Label pc, BinaryOperator op, Dummy dest, Dummy operand1, Dummy operand2, Data data)
		{
			return this.visitor.Binary (ConvertLabel (pc), op, dest, operand1, operand2, data);
		}

		public Result Isinst (Label pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.Isinst (ConvertLabel (pc), type, dest, obj, data);
		}

		public Result LoadNull (Label pc, Dummy dest, Data polarity)
		{
			return this.visitor.LoadNull (ConvertLabel (pc), dest, polarity);
		}

		public Result LoadConst (Label pc, TypeNode type, object constant, Dummy dest, Data data)
		{
			return this.visitor.LoadConst (ConvertLabel (pc), type, constant, dest, data);
		}

		public Result Sizeof (Label pc, TypeNode type, Dummy dest, Data data)
		{
			return this.visitor.Sizeof (ConvertLabel (pc), type, dest, data);
		}

		public Result Unary (Label pc, UnaryOperator op, bool unsigned, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.Unary (ConvertLabel (pc), op, unsigned, dest, source, data);
		}

		public Result Arglist (Label pc, Dummy dest, Data data)
		{
			return this.visitor.Arglist (ConvertLabel (pc), dest, data);
		}

		public Result Branch (Label pc, Label target, bool leavesExceptionBlock, Data data)
		{
			return this.visitor.Branch (ConvertLabel (pc), ConvertLabel (target), leavesExceptionBlock, data);
		}

		public Result BranchCond (Label pc, Label target, BranchOperator bop, Dummy value1, Dummy value2, Data data)
		{
			return this.visitor.BranchCond (ConvertLabel (pc), ConvertLabel (target), bop, value1, value2, data);
		}

		public Result BranchTrue (Label pc, Label target, Dummy cond, Data data)
		{
			return this.visitor.BranchTrue (ConvertLabel (pc), ConvertLabel (target), cond, data);
		}

		public Result BranchFalse (Label pc, Label target, Dummy cond, Data data)
		{
			return this.visitor.BranchFalse (ConvertLabel (pc), ConvertLabel (target), cond, data);
		}

		public Result Break (Label pc, Data data)
		{
			return this.visitor.Break (ConvertLabel (pc), data);
		}

		public Result Call<TypeList, ArgList> (Label pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			return this.visitor.Call (ConvertLabel (pc), method, virt, extraVarargs, dest, args, data);
		}

		public Result Calli<TypeList, ArgList> (Label pc, TypeNode returnType, TypeList argTypes, bool instance, Dummy dest, Dummy functionPointer, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			return this.visitor.Calli (ConvertLabel (pc), returnType, argTypes, instance, dest, functionPointer, args, data);
		}

		public Result CheckFinite (Label pc, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.CheckFinite (ConvertLabel (pc), dest, source, data);
		}

		public Result CopyBlock (Label pc, Dummy destAddress, Dummy srcAddress, Dummy len, Data data)
		{
			return this.visitor.CopyBlock (ConvertLabel (pc), destAddress, srcAddress, len, data);
		}

		public Result EndFilter (Label pc, Dummy decision, Data data)
		{
			return this.visitor.EndFilter (ConvertLabel (pc), decision, data);
		}

		public Result EndFinally (Label pc, Data data)
		{
			return this.visitor.EndFinally (ConvertLabel (pc), data);
		}

		public Result Jmp (Label pc, Method method, Data data)
		{
			return this.visitor.Jmp (ConvertLabel (pc), method, data);
		}

		public Result LoadArg (Label pc, Parameter argument, bool isOld, Dummy dest, Data data)
		{
			return this.visitor.LoadArg (ConvertLabel (pc), argument, isOld, dest, data);
		}

		public Result LoadArgAddress (Label pc, Parameter argument, bool isOld, Dummy dest, Data data)
		{
			return this.visitor.LoadArgAddress (ConvertLabel (pc), argument, isOld, dest, data);
		}

		public Result LoadLocal (Label pc, Local local, Dummy dest, Data data)
		{
			return this.visitor.LoadLocal (ConvertLabel (pc), local, dest, data);
		}

		public Result LoadLocalAddress (Label pc, Local local, Dummy dest, Data data)
		{
			return this.visitor.LoadLocalAddress (ConvertLabel (pc), local, dest, data);
		}

		public Result Nop (Label pc, Data data)
		{
			return this.visitor.Nop (ConvertLabel (pc), data);
		}

		public Result Pop (Label pc, Dummy source, Data data)
		{
			return this.visitor.Pop (ConvertLabel (pc), source, data);
		}

		public Result Return (Label pc, Dummy source, Data data)
		{
			return this.visitor.Return (ConvertLabel (pc), source, data);
		}

		public Result StoreArg (Label pc, Parameter argument, Dummy source, Data data)
		{
			return this.visitor.StoreArg (ConvertLabel (pc), argument, source, data);
		}

		public Result StoreLocal (Label pc, Local local, Dummy source, Data data)
		{
			return this.visitor.StoreLocal (ConvertLabel (pc), local, source, data);
		}

		public Result Switch (Label pc, TypeNode type, IEnumerable<Pair<object, Label>> cases, Dummy value, Data data)
		{
			return this.visitor.Nop (ConvertLabel (pc), data);
		}

		public Result Box (Label pc, TypeNode type, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.Box (ConvertLabel (pc), type, dest, source, data);
		}

		public Result ConstrainedCallvirt<TypeList, ArgList> (Label pc, Method method, TypeNode constraint, TypeList extraVarargs, Dummy dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			return this.visitor.ConstrainedCallvirt (ConvertLabel (pc), method, constraint, extraVarargs, dest, args, data);
		}

		public Result CastClass (Label pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.CastClass (ConvertLabel (pc), type, dest, obj, data);
		}

		public Result CopyObj (Label pc, TypeNode type, Dummy destPtr, Dummy sourcePtr, Data data)
		{
			return this.visitor.CopyObj (ConvertLabel (pc), type, destPtr, sourcePtr, data);
		}

		public Result Initobj (Label pc, TypeNode type, Dummy ptr, Data data)
		{
			return this.visitor.Initobj (ConvertLabel (pc), type, ptr, data);
		}

		public Result LoadElement (Label pc, TypeNode type, Dummy dest, Dummy array, Dummy index, Data data)
		{
			return this.visitor.LoadElement (ConvertLabel (pc), type, dest, array, index, data);
		}

		public Result LoadField (Label pc, Field field, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.LoadField (ConvertLabel (pc), field, dest, obj, data);
		}

		public Result LoadFieldAddress (Label pc, Field field, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.LoadFieldAddress (ConvertLabel (pc), field, dest, obj, data);
		}

		public Result LoadLength (Label pc, Dummy dest, Dummy array, Data data)
		{
			return this.visitor.LoadLength (ConvertLabel (pc), dest, array, data);
		}

		public Result LoadStaticField (Label pc, Field field, Dummy dest, Data data)
		{
			return this.visitor.LoadStaticField (ConvertLabel (pc), field, dest, data);
		}

		public Result LoadStaticFieldAddress (Label pc, Field field, Dummy dest, Data data)
		{
			return this.visitor.LoadStaticFieldAddress (ConvertLabel (pc), field, dest, data);
		}

		public Result LoadTypeToken (Label pc, TypeNode type, Dummy dest, Data data)
		{
			return this.visitor.LoadTypeToken (ConvertLabel (pc), type, dest, data);
		}

		public Result LoadFieldToken (Label pc, Field type, Dummy dest, Data data)
		{
			return this.visitor.LoadFieldToken (ConvertLabel (pc), type, dest, data);
		}

		public Result LoadMethodToken (Label pc, Method type, Dummy dest, Data data)
		{
			return this.visitor.LoadMethodToken (ConvertLabel (pc), type, dest, data);
		}

		public Result NewArray<ArgList> (Label pc, TypeNode type, Dummy dest, ArgList lengths, Data data) where ArgList : IIndexable<Dummy>
		{
			return this.visitor.NewArray (ConvertLabel (pc), type, dest, lengths, data);
		}

		public Result NewObj<ArgList> (Label pc, Method ctor, Dummy dest, ArgList args, Data data) where ArgList : IIndexable<Dummy>
		{
			return this.visitor.NewObj (ConvertLabel (pc), ctor, dest, args, data);
		}

		public Result MkRefAny (Label pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.MkRefAny (ConvertLabel (pc), type, dest, obj, data);
		}

		public Result RefAnyType (Label pc, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.RefAnyType (ConvertLabel (pc), dest, source, data);
		}

		public Result RefAnyVal (Label pc, TypeNode type, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.RefAnyVal (ConvertLabel (pc), type, dest, source, data);
		}

		public Result Rethrow (Label pc, Data data)
		{
			return this.visitor.Rethrow (ConvertLabel (pc), data);
		}

		public Result StoreElement (Label pc, TypeNode type, Dummy array, Dummy index, Dummy value, Data data)
		{
			return this.visitor.StoreElement (ConvertLabel (pc), type, array, index, value, data);
		}

		public Result StoreField (Label pc, Field field, Dummy obj, Dummy value, Data data)
		{
			return this.visitor.StoreField (ConvertLabel (pc), field, obj, value, data);
		}

		public Result StoreStaticField (Label pc, Field field, Dummy value, Data data)
		{
			return this.visitor.StoreStaticField (ConvertLabel (pc), field, value, data);
		}

		public Result Throw (Label pc, Dummy exception, Data data)
		{
			return this.visitor.Throw (ConvertLabel (pc), exception, data);
		}

		public Result Unbox (Label pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.Unbox (ConvertLabel (pc), type, dest, obj, data);
		}

		public Result UnboxAny (Label pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.UnboxAny (ConvertLabel (pc), type, dest, obj, data);
		}

		public Result Aggregate (Label pc, Label aggregateStart, bool canBeTargetOfBranch, Data data)
		{
			return this.visitor.Nop (ConvertLabel (pc), data);
		}

		public Result Entry (Label pc, Method method, Data data)
		{
			return this.visitor.Entry (ConvertLabel (pc), method, data);
		}

		public Result Assume (Label pc, EdgeTag tag, Dummy condition, Data data)
		{
			return this.visitor.Assume (ConvertLabel (pc), tag, condition, data);
		}

		public Result Assert (Label pc, EdgeTag tag, Dummy condition, Data data)
		{
			return this.visitor.Assert (ConvertLabel (pc), tag, condition, data);
		}

		public Result BeginOld (Label pc, Label matchingEnd, Data data)
		{
			if (this.original_pc.InsideOldManifestation)
				return this.visitor.Nop (ConvertLabel (pc), data);

			return this.visitor.BeginOld (ConvertLabel (pc), ConvertMatchingEndLabel (matchingEnd), data);
		}

		public Result EndOld (Label pc, Label matchingBegin, TypeNode type, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.EndOld (ConvertLabel (pc), ConvertMatchingBeginLabel (matchingBegin), type, dest, source, data);
		}

		public Result LoadStack (Label pc, int offset, Dummy dest, Dummy source, bool isOld, Data data)
		{
			return this.visitor.LoadStack (ConvertLabel (pc), offset, dest, source, isOld, data);
		}

		public Result LoadStackAddress (Label pc, int offset, Dummy dest, Dummy source, TypeNode type, bool isOld, Data data)
		{
			return this.visitor.LoadStackAddress (ConvertLabel (pc), offset, dest, source, type, isOld, data);
		}

		public Result LoadResult (Label pc, TypeNode type, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.LoadResult (ConvertLabel (pc), type, dest, source, data);
		}
		#endregion

		private APC ConvertLabel (Label pc)
		{
			return this.original_pc;
		}

		private APC ConvertMatchingEndLabel (Label matchingEnd)
		{
			return ((OldValueSubroutine<Label>) this.original_pc.Block.Subroutine).EndOldAPC (this.original_pc.SubroutineContext);
		}

		private APC ConvertMatchingBeginLabel (Label matchingBegin)
		{
			return ((OldValueSubroutine<Label>) this.original_pc.Block.Subroutine).BeginOldAPC (this.original_pc.SubroutineContext);
		}
	}
}
