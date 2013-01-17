// 
// RemoveBranchDelegator.cs
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
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Providers;

namespace Mono.CodeContracts.Static.ControlFlow {
	/// <summary>
	/// This class wraps underlying visitor.
	/// Replaces: branches to nop; branchCond to binary.
	/// 
	/// EdgeTag.Requires: (inside method) => assume, (outside method) => assert
	/// EdgeTag.Ensures:  (inside method) => assert, (outside method) => assume
	/// </summary>
	struct RemoveBranchDelegator<Data, Result, Visitor> : IILVisitor<APC, Dummy, Dummy, Data, Result>
		where Visitor : IILVisitor<APC, Dummy, Dummy, Data, Result> {
		private readonly IMetaDataProvider meta_data_provider;
		private readonly Visitor visitor;

		public RemoveBranchDelegator (Visitor visitor,
		                              IMetaDataProvider metaDataProvider)
		{
			this.visitor = visitor;
			this.meta_data_provider = metaDataProvider;
		}

		#region IILVisitor<APC,Dummy,Dummy,Data,Result> Members
		public Result Binary (APC pc, BinaryOperator op, Dummy dest, Dummy operand1, Dummy operand2, Data data)
		{
			return this.visitor.Binary (pc, op, dest, operand1, operand2, data);
		}

		public Result Isinst (APC pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.Isinst (pc, type, dest, obj, data);
		}

		public Result LoadNull (APC pc, Dummy dest, Data polarity)
		{
			return this.visitor.LoadNull (pc, dest, polarity);
		}

		public Result LoadConst (APC pc, TypeNode type, object constant, Dummy dest, Data data)
		{
			return this.visitor.LoadConst (pc, type, constant, dest, data);
		}

		public Result Sizeof (APC pc, TypeNode type, Dummy dest, Data data)
		{
			return this.visitor.Sizeof (pc, type, dest, data);
		}

		public Result Unary (APC pc, UnaryOperator op, bool unsigned, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.Unary (pc, op, unsigned, dest, source, data);
		}

		public Result Entry (APC pc, Method method, Data data)
		{
			return this.visitor.Entry (pc, method, data);
		}

		public Result Assume (APC pc, EdgeTag tag, Dummy condition, Data data)
		{
			if (tag == EdgeTag.Requires && pc.InsideRequiresAtCall || tag == EdgeTag.Invariant && pc.InsideInvariantOnExit)
				return this.visitor.Assert (pc, tag, condition, data);

			return this.visitor.Assume (pc, tag, condition, data);
		}

		public Result Assert (APC pc, EdgeTag tag, Dummy condition, Data data)
		{
			if (pc.InsideEnsuresAtCall)
				return this.visitor.Assume (pc, tag, condition, data);

			return this.visitor.Assert (pc, tag, condition, data);
		}

		public Result BeginOld (APC pc, APC matchingEnd, Data data)
		{
			return this.visitor.BeginOld (pc, matchingEnd, data);
		}

		public Result EndOld (APC pc, APC matchingBegin, TypeNode type, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.EndOld (pc, matchingBegin, type, dest, source, data);
		}

		public Result LoadStack (APC pc, int offset, Dummy dest, Dummy source, bool isOld, Data data)
		{
			return this.visitor.LoadStack (pc, offset, dest, source, isOld, data);
		}

		public Result LoadStackAddress (APC pc, int offset, Dummy dest, Dummy source, TypeNode type, bool isOld, Data data)
		{
			return this.visitor.LoadStackAddress (pc, offset, dest, source, type, isOld, data);
		}

		public Result LoadResult (APC pc, TypeNode type, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.LoadResult (pc, type, dest, source, data);
		}

		public Result Arglist (APC pc, Dummy dest, Data data)
		{
			return this.visitor.Arglist (pc, dest, data);
		}

		public Result Branch (APC pc, APC target, bool leavesExceptionBlock, Data data)
		{
			return this.visitor.Nop (pc, data);
		}

		public Result BranchCond (APC pc, APC target, BranchOperator bop, Dummy value1, Dummy value2, Data data)
		{
			Dummy dest = Dummy.Value;
			switch (bop) {
			case BranchOperator.Beq:
				return this.visitor.Binary (pc, BinaryOperator.Ceq, dest, value1, value2, data);
			case BranchOperator.Bge:
				return this.visitor.Binary (pc, BinaryOperator.Cge, dest, value1, value2, data);
			case BranchOperator.Bge_Un:
				return this.visitor.Binary (pc, BinaryOperator.Cge_Un, dest, value1, value2, data);
			case BranchOperator.Bgt:
				return this.visitor.Binary (pc, BinaryOperator.Cgt, dest, value1, value2, data);
			case BranchOperator.Bgt_Un:
				return this.visitor.Binary (pc, BinaryOperator.Cgt_Un, dest, value1, value2, data);
			case BranchOperator.Ble:
				return this.visitor.Binary (pc, BinaryOperator.Cle, dest, value1, value2, data);
			case BranchOperator.Ble_Un:
				return this.visitor.Binary (pc, BinaryOperator.Cle_Un, dest, value1, value2, data);
			case BranchOperator.Blt:
				return this.visitor.Binary (pc, BinaryOperator.Clt, dest, value1, value2, data);
			case BranchOperator.Blt_Un:
				return this.visitor.Binary (pc, BinaryOperator.Clt_Un, dest, value1, value2, data);
			case BranchOperator.Bne_un:
				return this.visitor.Binary (pc, BinaryOperator.Cne_Un, dest, value1, value2, data);
			default:
				return this.visitor.Nop (pc, data);
			}
		}

		public Result BranchTrue (APC pc, APC target, Dummy cond, Data data)
		{
			return this.visitor.Nop (pc, data);
		}

		public Result BranchFalse (APC pc, APC target, Dummy cond, Data data)
		{
			return this.visitor.Nop (pc, data);
		}

		public Result Break (APC pc, Data data)
		{
			return this.visitor.Break (pc, data);
		}

		public Result Call<TypeList, ArgList> (APC pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			TypeNode declaringType = this.meta_data_provider.DeclaringType (method);
			if (MethodIsReferenceEquals (method, args, declaringType))
				return this.visitor.Binary (pc, BinaryOperator.Ceq, dest, args [0], args [1], data);

			return this.visitor.Call (pc, method, virt, extraVarargs, dest, args, data);
		}

		public Result Calli<TypeList, ArgList> (APC pc, TypeNode returnType, TypeList argTypes, bool instance, Dummy dest, Dummy functionPointer, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			return this.visitor.Calli (pc, returnType, argTypes, instance, dest, functionPointer, args, data);
		}

		public Result CheckFinite (APC pc, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.CheckFinite (pc, dest, source, data);
		}

		public Result CopyBlock (APC pc, Dummy destAddress, Dummy srcAddress, Dummy len, Data data)
		{
			return this.visitor.CopyBlock (pc, destAddress, srcAddress, len, data);
		}

		public Result EndFilter (APC pc, Dummy decision, Data data)
		{
			return this.visitor.EndFilter (pc, decision, data);
		}

		public Result EndFinally (APC pc, Data data)
		{
			return this.visitor.EndFinally (pc, data);
		}

		public Result Jmp (APC pc, Method method, Data data)
		{
			return this.visitor.Jmp (pc, method, data);
		}

		public Result LoadArg (APC pc, Parameter argument, bool isOld, Dummy dest, Data data)
		{
			return this.visitor.LoadArg (pc, argument, isOld, dest, data);
		}

		public Result LoadArgAddress (APC pc, Parameter argument, bool isOld, Dummy dest, Data data)
		{
			return this.visitor.LoadArgAddress (pc, argument, isOld, dest, data);
		}

		public Result LoadLocal (APC pc, Local local, Dummy dest, Data data)
		{
			return this.visitor.LoadLocal (pc, local, dest, data);
		}

		public Result LoadLocalAddress (APC pc, Local local, Dummy dest, Data data)
		{
			return this.visitor.LoadLocalAddress (pc, local, dest, data);
		}

		public Result LoadElement (APC pc, TypeNode type, Dummy dest, Dummy array, Dummy index, Data data)
		{
			return this.visitor.LoadElement (pc, type, dest, array, index, data);
		}

		public Result LoadField (APC pc, Field field, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.LoadField (pc, field, dest, obj, data);
		}

		public Result LoadFieldAddress (APC pc, Field field, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.LoadFieldAddress (pc, field, dest, obj, data);
		}

		public Result LoadLength (APC pc, Dummy dest, Dummy array, Data data)
		{
			return this.visitor.LoadLength (pc, dest, array, data);
		}

		public Result LoadStaticField (APC pc, Field field, Dummy dest, Data data)
		{
			return this.visitor.LoadStaticField (pc, field, dest, data);
		}

		public Result LoadStaticFieldAddress (APC pc, Field field, Dummy dest, Data data)
		{
			return this.visitor.LoadStaticFieldAddress (pc, field, dest, data);
		}

		public Result LoadTypeToken (APC pc, TypeNode type, Dummy dest, Data data)
		{
			return this.visitor.LoadTypeToken (pc, type, dest, data);
		}

		public Result LoadFieldToken (APC pc, Field type, Dummy dest, Data data)
		{
			return this.visitor.LoadFieldToken (pc, type, dest, data);
		}

		public Result LoadMethodToken (APC pc, Method type, Dummy dest, Data data)
		{
			return this.visitor.LoadMethodToken (pc, type, dest, data);
		}

		public Result Nop (APC pc, Data data)
		{
			return this.visitor.Nop (pc, data);
		}

		public Result Pop (APC pc, Dummy source, Data data)
		{
			return this.visitor.Pop (pc, source, data);
		}

		public Result Return (APC pc, Dummy source, Data data)
		{
			return this.visitor.Return (pc, source, data);
		}

		public Result StoreArg (APC pc, Parameter argument, Dummy source, Data data)
		{
			return this.visitor.StoreArg (pc, argument, source, data);
		}

		public Result StoreLocal (APC pc, Local local, Dummy source, Data data)
		{
			return this.visitor.StoreLocal (pc, local, source, data);
		}

		public Result StoreElement (APC pc, TypeNode type, Dummy array, Dummy index, Dummy value, Data data)
		{
			return this.visitor.StoreElement (pc, type, array, index, value, data);
		}

		public Result StoreField (APC pc, Field field, Dummy obj, Dummy value, Data data)
		{
			return this.visitor.StoreField (pc, field, obj, value, data);
		}

		public Result StoreStaticField (APC pc, Field field, Dummy value, Data data)
		{
			return this.visitor.StoreStaticField (pc, field, value, data);
		}

		public Result Switch (APC pc, TypeNode type, IEnumerable<Pair<object, APC>> cases, Dummy value, Data data)
		{
			return this.visitor.Nop (pc, data);
		}

		public Result Box (APC pc, TypeNode type, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.Box (pc, type, dest, source, data);
		}

		public Result ConstrainedCallvirt<TypeList, ArgList> (APC pc, Method method, TypeNode constraint, TypeList extraVarargs, Dummy dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			return this.visitor.ConstrainedCallvirt (pc, method, constraint, extraVarargs, dest, args, data);
		}

		public Result CastClass (APC pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.CastClass (pc, type, dest, obj, data);
		}

		public Result CopyObj (APC pc, TypeNode type, Dummy destPtr, Dummy sourcePtr, Data data)
		{
			return this.visitor.CopyObj (pc, type, destPtr, sourcePtr, data);
		}

		public Result Initobj (APC pc, TypeNode type, Dummy ptr, Data data)
		{
			return this.visitor.Initobj (pc, type, ptr, data);
		}

		public Result NewArray<ArgList> (APC pc, TypeNode type, Dummy dest, ArgList lengths, Data data) where ArgList : IIndexable<Dummy>
		{
			return this.visitor.NewArray (pc, type, dest, lengths, data);
		}

		public Result NewObj<ArgList> (APC pc, Method ctor, Dummy dest, ArgList args, Data data) where ArgList : IIndexable<Dummy>
		{
			return this.visitor.NewObj (pc, ctor, dest, args, data);
		}

		public Result MkRefAny (APC pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.MkRefAny (pc, type, dest, obj, data);
		}

		public Result RefAnyType (APC pc, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.RefAnyType (pc, dest, source, data);
		}

		public Result RefAnyVal (APC pc, TypeNode type, Dummy dest, Dummy source, Data data)
		{
			return this.visitor.RefAnyVal (pc, type, dest, source, data);
		}

		public Result Rethrow (APC pc, Data data)
		{
			return this.visitor.Rethrow (pc, data);
		}

		public Result Throw (APC pc, Dummy exception, Data data)
		{
			return this.visitor.Throw (pc, exception, data);
		}

		public Result Unbox (APC pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.Unbox (pc, type, dest, obj, data);
		}

		public Result UnboxAny (APC pc, TypeNode type, Dummy dest, Dummy obj, Data data)
		{
			return this.visitor.UnboxAny (pc, type, dest, obj, data);
		}
		#endregion

		private bool MethodIsReferenceEquals<ArgList> (Method method, ArgList args, TypeNode declaringType)
			where ArgList : IIndexable<Dummy>
		{
			return args.Count == 2 && this.meta_data_provider.IsStatic (method)
			       && this.meta_data_provider.Equal (declaringType, this.meta_data_provider.System_Object)
			       && this.meta_data_provider.Name (method) == "ReferenceEquals";
		}
		}
}
