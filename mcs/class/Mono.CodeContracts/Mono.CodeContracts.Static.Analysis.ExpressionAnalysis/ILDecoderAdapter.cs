// 
// ILDecoderAdapter.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
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
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.HeapAnalysis;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.ExpressionAnalysis {
	/// <summary>
	/// This class performs translation from (source) SymbolicValue to LabeledSymbol
	/// </summary>
	struct ILDecoderAdapter<SymbolicValue, Data, Result, Visitor>
		: IILVisitor<APC, SymbolicValue, SymbolicValue, Data, Result>
		where SymbolicValue : IEquatable<SymbolicValue>
		where Visitor : IILVisitor<APC, LabeledSymbol<APC, SymbolicValue>, SymbolicValue, Data, Result> {
		private readonly Visitor visitor;

		public ILDecoderAdapter (Visitor visitor)
		{
			this.visitor = visitor;
		}

		#region IILVisitor<APC,SymbolicValue,SymbolicValue,Data,Result> Members
		public Result Binary (APC pc, BinaryOperator op, SymbolicValue dest, SymbolicValue operand1, SymbolicValue operand2, Data data)
		{
			return this.visitor.Binary (pc, op, dest, Convert (pc, operand1), Convert (pc, operand2), data);
		}

		public Result Isinst (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue obj, Data data)
		{
			return this.visitor.Isinst (pc, type, dest, Convert (pc, obj), data);
		}

		public Result LoadNull (APC pc, SymbolicValue dest, Data polarity)
		{
			return this.visitor.LoadNull (pc, dest, polarity);
		}

		public Result LoadConst (APC pc, TypeNode type, object constant, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadConst (pc, type, constant, dest, data);
		}

		public Result Sizeof (APC pc, TypeNode type, SymbolicValue dest, Data data)
		{
			return this.visitor.Sizeof (pc, type, dest, data);
		}

		public Result Unary (APC pc, UnaryOperator op, bool unsigned, SymbolicValue dest, SymbolicValue source, Data data)
		{
			return this.visitor.Unary (pc, op, unsigned, dest, Convert (pc, source), data);
		}

		public Result Entry (APC pc, Method method, Data data)
		{
			return this.visitor.Entry (pc, method, data);
		}

		public Result Assume (APC pc, EdgeTag tag, SymbolicValue condition, Data data)
		{
			return this.visitor.Assume (pc, tag, Convert (pc, condition), data);
		}

		public Result Assert (APC pc, EdgeTag tag, SymbolicValue condition, Data data)
		{
			return this.visitor.Assert (pc, tag, Convert (pc, condition), data);
		}

		public Result BeginOld (APC pc, APC matchingEnd, Data data)
		{
			return this.visitor.BeginOld (pc, matchingEnd, data);
		}

		public Result EndOld (APC pc, APC matchingBegin, TypeNode type, SymbolicValue dest, SymbolicValue source, Data data)
		{
			return this.visitor.EndOld (pc, matchingBegin, type, dest, Convert (pc, source), data);
		}

		public Result LoadStack (APC pc, int offset, SymbolicValue dest, SymbolicValue source, bool isOld, Data data)
		{
			return this.visitor.LoadStack (pc, offset, dest, Convert (pc, source), isOld, data);
		}

		public Result LoadStackAddress (APC pc, int offset, SymbolicValue dest, SymbolicValue source, TypeNode type, bool isOld, Data data)
		{
			return this.visitor.LoadStackAddress (pc, offset, dest, Convert (pc, source), type, isOld, data);
		}

		public Result LoadResult (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue source, Data data)
		{
			return this.visitor.LoadResult (pc, type, dest, Convert (pc, source), data);
		}

		public Result Arglist (APC pc, SymbolicValue dest, Data data)
		{
			return this.visitor.Arglist (pc, dest, data);
		}

		public Result Branch (APC pc, APC target, bool leavesExceptionBlock, Data data)
		{
			throw new Exception ("Should not get branches at this level of abstraction.");
		}

		public Result BranchCond (APC pc, APC target, BranchOperator bop, SymbolicValue value1, SymbolicValue value2, Data data)
		{
			throw new Exception ("Should not get branches at this level of abstraction.");
		}

		public Result BranchTrue (APC pc, APC target, SymbolicValue cond, Data data)
		{
			throw new Exception ("Should not get branches at this level of abstraction.");
		}

		public Result BranchFalse (APC pc, APC target, SymbolicValue cond, Data data)
		{
			throw new Exception ("Should not get branches at this level of abstraction.");
		}

		public Result Break (APC pc, Data data)
		{
			return this.visitor.Break (pc, data);
		}

		public Result Call<TypeList, ArgList> (APC pc, Method method, bool virt, TypeList extraVarargs, SymbolicValue dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<SymbolicValue>
		{
			return this.visitor.Call (pc, method, virt, extraVarargs, dest, Convert (pc, args), data);
		}

		public Result Calli<TypeList, ArgList> (APC pc, TypeNode returnType, TypeList argTypes, bool instance, SymbolicValue dest, SymbolicValue functionPointer, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode> where ArgList : IIndexable<SymbolicValue>
		{
			return this.visitor.Calli (pc, returnType, argTypes, instance, dest, Convert (pc, functionPointer), Convert (pc, args), data);
		}

		public Result CheckFinite (APC pc, SymbolicValue dest, SymbolicValue source, Data data)
		{
			return this.visitor.CheckFinite (pc, dest, Convert (pc, source), data);
		}

		public Result CopyBlock (APC pc, SymbolicValue destAddress, SymbolicValue srcAddress, SymbolicValue len, Data data)
		{
			return this.visitor.CopyBlock (pc, Convert (pc, destAddress), Convert (pc, srcAddress), Convert (pc, len), data);
		}

		public Result EndFilter (APC pc, SymbolicValue decision, Data data)
		{
			return this.visitor.EndFilter (pc, Convert (pc, decision), data);
		}

		public Result EndFinally (APC pc, Data data)
		{
			return this.visitor.EndFinally (pc, data);
		}

		public Result Jmp (APC pc, Method method, Data data)
		{
			return this.visitor.Jmp (pc, method, data);
		}

		public Result LoadArg (APC pc, Parameter argument, bool isOld, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadArg (pc, argument, isOld, dest, data);
		}

		public Result LoadArgAddress (APC pc, Parameter argument, bool isOld, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadArgAddress (pc, argument, isOld, dest, data);
		}

		public Result LoadLocal (APC pc, Local local, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadLocal (pc, local, dest, data);
		}

		public Result LoadLocalAddress (APC pc, Local local, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadLocalAddress (pc, local, dest, data);
		}

		public Result Nop (APC pc, Data data)
		{
			return this.visitor.Nop (pc, data);
		}

		public Result Pop (APC pc, SymbolicValue source, Data data)
		{
			return this.visitor.Pop (pc, Convert (pc, source), data);
		}

		public Result Return (APC pc, SymbolicValue source, Data data)
		{
			return this.visitor.Return (pc, Convert (pc, source), data);
		}

		public Result StoreArg (APC pc, Parameter argument, SymbolicValue source, Data data)
		{
			return this.visitor.StoreArg (pc, argument, Convert (pc, source), data);
		}

		public Result StoreLocal (APC pc, Local local, SymbolicValue source, Data data)
		{
			return this.visitor.StoreLocal (pc, local, Convert (pc, source), data);
		}

		public Result Switch (APC pc, TypeNode type, IEnumerable<Pair<object, APC>> cases, SymbolicValue value, Data data)
		{
			throw new Exception ("Should not get branches at this level of abstraction.");
		}

		public Result Box (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue source, Data data)
		{
			return this.visitor.Box (pc, type, dest, Convert (pc, source), data);
		}

		public Result ConstrainedCallvirt<TypeList, ArgList> (APC pc, Method method, TypeNode constraint, TypeList extraVarargs, SymbolicValue dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<SymbolicValue>
		{
			return this.visitor.ConstrainedCallvirt (pc, method, constraint, extraVarargs, dest, Convert (pc, args), data);
		}

		public Result CastClass (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue obj, Data data)
		{
			return this.visitor.CastClass (pc, type, dest, Convert (pc, obj), data);
		}

		public Result CopyObj (APC pc, TypeNode type, SymbolicValue destPtr, SymbolicValue sourcePtr, Data data)
		{
			return this.visitor.CopyObj (pc, type, Convert (pc, destPtr), Convert (pc, sourcePtr), data);
		}

		public Result Initobj (APC pc, TypeNode type, SymbolicValue ptr, Data data)
		{
			return this.visitor.Initobj (pc, type, Convert (pc, ptr), data);
		}

		public Result LoadElement (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue array, SymbolicValue index, Data data)
		{
			return this.visitor.LoadElement (pc, type, dest, Convert (pc, array), Convert (pc, index), data);
		}

		public Result LoadField (APC pc, Field field, SymbolicValue dest, SymbolicValue obj, Data data)
		{
			return this.visitor.LoadField (pc, field, dest, Convert (pc, obj), data);
		}

		public Result LoadFieldAddress (APC pc, Field field, SymbolicValue dest, SymbolicValue obj, Data data)
		{
			return this.visitor.LoadFieldAddress (pc, field, dest, Convert (pc, obj), data);
		}

		public Result LoadLength (APC pc, SymbolicValue dest, SymbolicValue array, Data data)
		{
			return this.visitor.LoadLength (pc, dest, Convert (pc, array), data);
		}

		public Result LoadStaticField (APC pc, Field field, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadStaticField (pc, field, dest, data);
		}

		public Result LoadStaticFieldAddress (APC pc, Field field, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadStaticFieldAddress (pc, field, dest, data);
		}

		public Result LoadTypeToken (APC pc, TypeNode type, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadTypeToken (pc, type, dest, data);
		}

		public Result LoadFieldToken (APC pc, Field type, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadFieldToken (pc, type, dest, data);
		}

		public Result LoadMethodToken (APC pc, Method type, SymbolicValue dest, Data data)
		{
			return this.visitor.LoadMethodToken (pc, type, dest, data);
		}

		public Result NewArray<ArgList> (APC pc, TypeNode type, SymbolicValue dest, ArgList lengths, Data data) where ArgList : IIndexable<SymbolicValue>
		{
			return this.visitor.NewArray (pc, type, dest, Convert (pc, lengths), data);
		}

		public Result NewObj<ArgList> (APC pc, Method ctor, SymbolicValue dest, ArgList args, Data data) where ArgList : IIndexable<SymbolicValue>
		{
			return this.visitor.NewObj (pc, ctor, dest, Convert (pc, args), data);
		}

		public Result MkRefAny (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue obj, Data data)
		{
			return this.visitor.MkRefAny (pc, type, dest, Convert (pc, obj), data);
		}

		public Result RefAnyType (APC pc, SymbolicValue dest, SymbolicValue source, Data data)
		{
			return this.visitor.RefAnyType (pc, dest, Convert (pc, source), data);
		}

		public Result RefAnyVal (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue source, Data data)
		{
			return this.visitor.RefAnyVal (pc, type, dest, Convert (pc, source), data);
		}

		public Result Rethrow (APC pc, Data data)
		{
			return this.visitor.Rethrow (pc, data);
		}

		public Result StoreElement (APC pc, TypeNode type, SymbolicValue array, SymbolicValue index, SymbolicValue value, Data data)
		{
			return this.visitor.StoreElement (pc, type, Convert (pc, array), Convert (pc, index), Convert (pc, value), data);
		}

		public Result StoreField (APC pc, Field field, SymbolicValue obj, SymbolicValue value, Data data)
		{
			return this.visitor.StoreField (pc, field, Convert (pc, obj), Convert (pc, value), data);
		}

		public Result StoreStaticField (APC pc, Field field, SymbolicValue value, Data data)
		{
			return this.visitor.StoreStaticField (pc, field, Convert (pc, value), data);
		}

		public Result Throw (APC pc, SymbolicValue exception, Data data)
		{
			return this.visitor.Throw (pc, Convert (pc, exception), data);
		}

		public Result Unbox (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue obj, Data data)
		{
			return this.visitor.Unbox (pc, type, dest, Convert (pc, obj), data);
		}

		public Result UnboxAny (APC pc, TypeNode type, SymbolicValue dest, SymbolicValue obj, Data data)
		{
			return this.visitor.UnboxAny (pc, type, dest, Convert (pc, obj), data);
		}
		#endregion

		private static LabeledSymbol<APC, SymbolicValue> Convert (APC pc, SymbolicValue value)
		{
			return new LabeledSymbol<APC, SymbolicValue> (pc, value);
		}

		private static ArgumentWrapper<ArgList> Convert<ArgList> (APC pc, ArgList value)
			where ArgList : IIndexable<SymbolicValue>
		{
			return new ArgumentWrapper<ArgList> (value, pc);
		}

		#region Nested type: ArgumentWrapper
		private struct ArgumentWrapper<ArgList> : IIndexable<LabeledSymbol<APC, SymbolicValue>>
			where ArgList : IIndexable<SymbolicValue> {
			private readonly APC readAt;
			private readonly ArgList underlying;

			public ArgumentWrapper (ArgList underlying, APC readAt)
			{
				this.underlying = underlying;
				this.readAt = readAt;
			}

			#region Implementation of IIndexable<ExternalExpression<APC,SymbolicValue>>
			public int Count
			{
				get { return this.underlying.Count; }
			}

			public LabeledSymbol<APC, SymbolicValue> this [int index]
			{
				get { return new LabeledSymbol<APC, SymbolicValue> (this.readAt, this.underlying [index]); }
			}
			#endregion
		}
		#endregion
	}
}
