// 
// StackDecoder.cs
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
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.StackAnalysis {
	struct StackDecoder<TContext, TData, TResult, TVisitor> : IILVisitor<APC, Dummy, Dummy, TData, TResult>
		where TContext : IMethodContextProvider
		where TVisitor : IILVisitor<APC, int, int, TData, TResult>
	{
		private readonly StackDepthProvider<TContext> parent;
		private readonly TVisitor visitor;

		public StackDecoder(StackDepthProvider<TContext> parent,
		                    TVisitor visitor)
		{
			this.parent = parent;
			this.visitor = visitor;
		}

		private int Pop(APC label, int count)
		{
			return this.parent.GlobalStackDepth (label) - 1 - count;
		}

		private SequenceGenerator PopSequence(APC pc, int args, int offset)
		{
			return new SequenceGenerator (this.parent.GlobalStackDepth (pc) - args - offset, args);
		}

		private int Push(APC label, int count)
		{
			return this.parent.GlobalStackDepth (label) - count;
		}

		private int Push(APC label, int args, TypeNode returnType)
		{
			if (this.parent.MetaDataProvider.IsVoid (returnType))
				return -1;

			return Push (label, args);
		}

		private int GetParametersCount(Method ctor, int extraVarargs)
		{
			int result = extraVarargs + this.parent.MetaDataProvider.Parameters (ctor).Count;
			if (!this.parent.MetaDataProvider.IsStatic (ctor))
				++result;
			return result;
		}

		private bool RemapParameterToLoadStack(APC pc, ref Parameter param, out bool isLoadResult, out int loadStackOffset, out bool isOld, out APC lookupPC)
		{
			if (pc.SubroutineContext == null) {
				isLoadResult = false;
				loadStackOffset = 0;
				isOld = false;
				lookupPC = pc;
				return false;
			}

			if (pc.Block.Subroutine.IsRequires) {
				isLoadResult = false;
				isOld = false;
				lookupPC = pc;
				for (LispList<Edge<CFGBlock, EdgeTag>> list = pc.SubroutineContext; list != null; list = list.Tail) {
					EdgeTag edgeTag = list.Head.Tag;
					if (edgeTag == EdgeTag.Entry) {
						param = RemapParameter (param, list.Head.From, pc.Block);
						loadStackOffset = 0;
						return false;
					}
					if (edgeTag.Is (EdgeTag.BeforeMask)) {
						int stackDepth = this.parent.LocalStackDepth (pc);
						loadStackOffset = this.parent.MetaDataProvider.ParameterStackIndex (param) + stackDepth;
						return true;
					}
				}
				throw new InvalidOperationException ();
			}

			if (pc.Block.Subroutine.IsEnsuresOrOldValue) {
				isOld = true;
				for (LispList<Edge<CFGBlock, EdgeTag>> ctx = pc.SubroutineContext; ctx != null; ctx = ctx.Tail) {
					EdgeTag tag = ctx.Head.Tag;
					if (tag == EdgeTag.Exit) {
						param = RemapParameter (param, ctx.Head.From, pc.Block);
						isLoadResult = false;
						loadStackOffset = 0;
						lookupPC = pc;
						return false;
					}

					if (tag == EdgeTag.AfterCall) {
						loadStackOffset = this.parent.MetaDataProvider.ParameterStackIndex (param);
						isLoadResult = false;
						lookupPC = new APC (ctx.Head.From, 0, ctx.Tail);
						return true;
					}

					if (tag == EdgeTag.AfterNewObj) {
						if (this.parent.MetaDataProvider.ParameterIndex (param) == 0) {
							loadStackOffset = this.parent.LocalStackDepth (pc);
							isLoadResult = true;
							lookupPC = pc;
							isOld = false;
							return false;
						}

						loadStackOffset = this.parent.MetaDataProvider.ParameterIndex (param);
						isLoadResult = false;
						lookupPC = new APC (ctx.Head.From, 0, ctx.Tail);
						return true;
					}
					if (tag == EdgeTag.OldManifest) {
						param = RemapParameter (param, ctx.Tail.Head.From, pc.Block);
						isOld = false;
						isLoadResult = false;
						loadStackOffset = 0;
						lookupPC = pc;
						return false;
					}
				}
				throw new InvalidOperationException ();
			}

			if (pc.Block.Subroutine.IsInvariant) {
				for (LispList<Edge<CFGBlock, EdgeTag>> list = pc.SubroutineContext; list != null; list = list.Tail) {
					EdgeTag tag = list.Head.Tag;
					if (tag == EdgeTag.Entry || tag == EdgeTag.Exit) {
						Method method;
						if (pc.TryGetContainingMethod (out method)) {
							param = this.parent.MetaDataProvider.This (method);
							isLoadResult = false;
							loadStackOffset = 0;
							isOld = tag == EdgeTag.Exit;
							lookupPC = pc;
							return false;
						}
						isLoadResult = false;
						loadStackOffset = 0;
						isOld = false;
						lookupPC = pc;
						return false;
					}
					if (tag == EdgeTag.AfterCall) {
						Method calledMethod;
						bool isNewObj;
						bool isVirtual;
						list.Head.From.IsMethodCallBlock (out calledMethod, out isNewObj, out isVirtual);
						int count = this.parent.MetaDataProvider.Parameters (calledMethod).Count;
						loadStackOffset = count;
						isLoadResult = false;
						isOld = true;
						lookupPC = new APC (list.Head.From, 0, list.Tail);
						return true;
					}
					if (tag == EdgeTag.AfterNewObj) {
						isLoadResult = true;
						loadStackOffset = this.parent.LocalStackDepth (pc);
						isOld = false;
						lookupPC = pc;
						return false;
					}
					if (tag.Is (EdgeTag.BeforeMask))
						throw new InvalidOperationException ("this should never happen");
				}
				throw new InvalidOperationException ("this should never happen");
			}

			isLoadResult = false;
			loadStackOffset = 0;
			isOld = false;
			lookupPC = pc;
			return false;
		}

		private Parameter RemapParameter(Parameter p, CFGBlock parentMethodBlock, CFGBlock subroutineBlock)
		{
			Method parentMethod = ((IMethodInfo) parentMethodBlock.Subroutine).Method;
			Method method = ((IMethodInfo) subroutineBlock.Subroutine).Method;

			if (this.parent.MetaDataProvider.Equal (method, parentMethod))
				return p;

			int index = this.parent.MetaDataProvider.ParameterIndex (p);
			if (this.parent.MetaDataProvider.IsStatic (parentMethod) || index != 0)
				return this.parent.MetaDataProvider.Parameters (parentMethod)[index];

			return this.parent.MetaDataProvider.This (parentMethod);
		}

		private bool IsReferenceType(APC pc, TypeNode type)
		{
			return this.parent.MetaDataProvider.IsReferenceType (type);
		}

		private TypeNode GetSpecializedType(APC pc, TypeNode type)
		{
			var methodInfo = pc.Block.Subroutine as IMethodInfo;
			if (methodInfo == null)
				return type;

			throw new NotImplementedException ();
		}

		#region Implementation of IExpressionILVisitor<APC,Type,Dummy,Dummy,Data,Result>
		public TResult Binary(APC pc, BinaryOperator op, Dummy dest, Dummy operand1, Dummy operand2, TData data)
		{
			return this.visitor.Binary (pc, op, Push (pc, 2), Pop (pc, 1), Pop (pc, 0), data);
		}

		public TResult Isinst(APC pc, TypeNode type, Dummy dest, Dummy obj, TData data)
		{
			return this.visitor.Isinst (pc, type, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult LoadNull(APC pc, Dummy dest, TData polarity)
		{
			return this.visitor.LoadNull (pc, Push (pc, 0), polarity);
		}

		public TResult LoadConst(APC pc, TypeNode type, object constant, Dummy dest, TData data)
		{
			return this.visitor.LoadConst (pc, type, constant, Push (pc, 0), data);
		}

		public TResult Sizeof(APC pc, TypeNode type, Dummy dest, TData data)
		{
			return this.visitor.Sizeof (pc, type, Push (pc, 0), data);
		}

		public TResult Unary(APC pc, UnaryOperator op, bool unsigned, Dummy dest, Dummy source, TData data)
		{
			return this.visitor.Unary (pc, op, unsigned, Push (pc, 1), Pop (pc, 0), data);
		}
		#endregion

		#region Implementation of ISyntheticILVisitor<APC,Method,Field,Type,Dummy,Dummy,Data,Result>
		public TResult Entry(APC pc, Method method, TData data)
		{
			return this.visitor.Entry (pc, method, data);
		}

		public TResult Assume(APC pc, EdgeTag tag, Dummy condition, TData data)
		{
			return this.visitor.Assume (pc, tag, Pop (pc, 0), data);
		}

		public TResult Assert(APC pc, EdgeTag tag, Dummy condition, TData data)
		{
			return this.visitor.Assert (pc, tag, Pop (pc, 0), data);
		}

		public TResult BeginOld(APC pc, APC matchingEnd, TData data)
		{
			return this.visitor.BeginOld (pc, matchingEnd, data);
		}

		public TResult EndOld(APC pc, APC matchingBegin, TypeNode type, Dummy dest, Dummy source, TData data)
		{
			if (pc.InsideOldManifestation)
				return this.visitor.LoadStack (pc, 1, Push (matchingBegin, 0), Pop (pc, 0), false, data);

			return this.visitor.EndOld (pc, matchingBegin, type, Push (matchingBegin, 0), Pop (pc, 0), data);
		}

		public TResult LoadStack(APC pc, int offset, Dummy dest, Dummy source, bool isOld, TData data)
		{
			return this.visitor.LoadStack (pc, offset, Push (pc, 0), Pop (pc, offset), isOld, data);
		}

		public TResult LoadStackAddress(APC pc, int offset, Dummy dest, Dummy source, TypeNode type, bool isOld, TData data)
		{
			return this.visitor.LoadStackAddress (pc, offset, Push (pc, 0), Pop (pc, offset), type, isOld, data);
		}

		public TResult LoadResult(APC pc, TypeNode type, Dummy dest, Dummy source, TData data)
		{
			int offset = this.parent.LocalStackDepth (pc);
			return this.visitor.LoadResult (pc, type, Push (pc, 0), Pop (pc, offset), data);
		}
		#endregion

		#region Implementation of IILVisitor<APC,Local,Parameter,Method,Field,Type,Dummy,Dummy,Data,Result>
		public TResult Arglist(APC pc, Dummy dest, TData data)
		{
			return this.visitor.Arglist (pc, Push (pc, 0), data);
		}

		public TResult Branch(APC pc, APC target, bool leavesExceptionBlock, TData data)
		{
			return this.visitor.Branch (pc, target, leavesExceptionBlock, data);
		}

		public TResult BranchCond(APC pc, APC target, BranchOperator bop, Dummy value1, Dummy value2, TData data)
		{
			return this.visitor.BranchCond (pc, target, bop, Pop (pc, 1), Pop (pc, 0), data);
		}

		public TResult BranchTrue(APC pc, APC target, Dummy cond, TData data)
		{
			return this.visitor.BranchTrue (pc, target, Pop (pc, 0), data);
		}

		public TResult BranchFalse(APC pc, APC target, Dummy cond, TData data)
		{
			return this.visitor.BranchFalse (pc, target, Pop (pc, 0), data);
		}

		public TResult Break(APC pc, TData data)
		{
			return this.visitor.Break (pc, data);
		}

		public TResult Call<TypeList, ArgList>(APC pc, Method method, bool virt, TypeList extraVarargs, Dummy dest, ArgList args, TData data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			int argsCount = GetParametersCount (method, extraVarargs == null ? 0 : extraVarargs.Count);
			return this.visitor.Call (pc, method, virt, extraVarargs, Push (pc, argsCount, this.parent.MetaDataProvider.ReturnType (method)), PopSequence (pc, argsCount, 0), data);
		}

		public TResult Calli<TypeList, ArgList>(APC pc, TypeNode returnType, TypeList argTypes, bool instance, Dummy dest, Dummy functionPointer, ArgList args, TData data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			int argsCount = argTypes.Count + (instance ? 1 : 0);
			return this.visitor.Calli (pc, returnType, argTypes, instance, Push (pc, argsCount + 1, returnType), Pop (pc, 0), PopSequence (pc, argsCount, 1), data);
		}

		public TResult CheckFinite(APC pc, Dummy dest, Dummy source, TData data)
		{
			return this.visitor.CheckFinite (pc, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult CopyBlock(APC pc, Dummy destAddress, Dummy srcAddress, Dummy len, TData data)
		{
			return this.visitor.CopyBlock (pc, Pop (pc, 2), Pop (pc, 1), Pop (pc, 0), data);
		}

		public TResult EndFilter(APC pc, Dummy decision, TData data)
		{
			return this.visitor.EndFilter (pc, Pop (pc, 0), data);
		}

		public TResult EndFinally(APC pc, TData data)
		{
			return this.visitor.EndFinally (pc, data);
		}

		public TResult Jmp(APC pc, Method method, TData data)
		{
			return this.visitor.Jmp (pc, method, data);
		}

		public TResult LoadArg(APC pc, Parameter argument, bool dummyOld, Dummy dest, TData data)
		{
			Parameter p = argument;
			bool isLdResult;
			int loadStackOffset;
			bool isOld;
			APC lookupPC;
			if (RemapParameterToLoadStack (pc, ref argument, out isLdResult, out loadStackOffset, out isOld, out lookupPC))
				return this.visitor.LoadStack (pc, loadStackOffset, Push (pc, 0), Pop (lookupPC, loadStackOffset), isOld, data);

			if (argument == null)
				argument = p;

			if (isLdResult) {
				if (this.parent.MetaDataProvider.IsStruct (this.parent.MetaDataProvider.DeclaringType (this.parent.MetaDataProvider.DeclaringMethod (argument))))
					return this.visitor.LoadStackAddress (pc, loadStackOffset, Push (pc, 0), Pop (pc, loadStackOffset), this.parent.MetaDataProvider.ParameterType (argument), isOld, data);

				return this.visitor.LoadResult (pc, this.parent.MetaDataProvider.ParameterType (argument), Push (pc, 0), Pop (pc, loadStackOffset), data);
			}

			return this.visitor.LoadArg (pc, argument, isOld, Push (pc, 0), data);
		}

		public TResult LoadArgAddress(APC pc, Parameter argument, bool dummyOld, Dummy dest, TData data)
		{
			bool isLoadResult;
			int loadStackOffset;
			bool isOld;
			APC lookupPC;
			if (RemapParameterToLoadStack (pc, ref argument, out isLoadResult, out loadStackOffset, out isOld, out lookupPC))
				return this.visitor.LoadStackAddress (pc, loadStackOffset, Push (pc, 0), Pop (lookupPC, loadStackOffset), this.parent.MetaDataProvider.ParameterType (argument), isOld, data);

			if (isLoadResult)
				throw new InvalidOperationException ();

			return this.visitor.LoadArgAddress (pc, argument, isOld, Push (pc, 0), data);
		}

		public TResult LoadLocal(APC pc, Local local, Dummy dest, TData data)
		{
			return this.visitor.LoadLocal (pc, local, Push (pc, 0), data);
		}

		public TResult LoadLocalAddress(APC pc, Local local, Dummy dest, TData data)
		{
			return this.visitor.LoadLocalAddress (pc, local, Push (pc, 0), data);
		}

		public TResult Nop(APC pc, TData data)
		{
			return this.visitor.Nop (pc, data);
		}

		public TResult Pop(APC pc, Dummy source, TData data)
		{
			return this.visitor.Pop (pc, Pop (pc, 0), data);
		}

		public TResult Return(APC pc, Dummy source, TData data)
		{
			return this.visitor.Nop (pc, data);
		}

		public TResult StoreArg(APC pc, Parameter argument, Dummy source, TData data)
		{
			return this.visitor.StoreArg (pc, argument, Pop (pc, 0), data);
		}

		public TResult StoreLocal(APC pc, Local local, Dummy source, TData data)
		{
			return this.visitor.StoreLocal (pc, local, Pop (pc, 0), data);
		}

		public TResult Switch(APC pc, TypeNode type, IEnumerable<Pair<object, APC>> cases, Dummy value, TData data)
		{
			return this.visitor.Switch (pc, type, cases, Pop (pc, 0), data);
		}

		public TResult Box(APC pc, TypeNode type, Dummy dest, Dummy source, TData data)
		{
			type = GetSpecializedType (pc, type);
			if (IsReferenceType (pc, type))
				return this.visitor.Nop (pc, data);

			return this.visitor.Box (pc, type, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult ConstrainedCallvirt<TypeList, ArgList>(APC pc, Method method, TypeNode constraint, TypeList extraVarargs, Dummy dest, ArgList args, TData data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Dummy>
		{
			int argsCount = GetParametersCount (method, extraVarargs == null ? 0 : extraVarargs.Count);
			return this.visitor.ConstrainedCallvirt (pc, method, constraint, extraVarargs, Push (pc, argsCount, this.parent.MetaDataProvider.ReturnType (method)), PopSequence (pc, argsCount, 0), data);
		}

		public TResult CastClass(APC pc, TypeNode type, Dummy dest, Dummy obj, TData data)
		{
			return this.visitor.CastClass (pc, type, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult CopyObj(APC pc, TypeNode type, Dummy destPtr, Dummy sourcePtr, TData data)
		{
			return this.visitor.CopyObj (pc, type, Pop (pc, 1), Pop (pc, 0), data);
		}

		public TResult Initobj(APC pc, TypeNode type, Dummy ptr, TData data)
		{
			return this.visitor.Initobj (pc, type, Pop (pc, 0), data);
		}

		public TResult LoadElement(APC pc, TypeNode type, Dummy dest, Dummy array, Dummy index, TData data)
		{
			return this.visitor.LoadElement (pc, type, Push (pc, 2), Pop (pc, 1), Pop (pc, 0), data);
		}

		public TResult LoadField(APC pc, Field field, Dummy dest, Dummy obj, TData data)
		{
			return this.visitor.LoadField (pc, field, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult LoadFieldAddress(APC pc, Field field, Dummy dest, Dummy obj, TData data)
		{
			return this.visitor.LoadFieldAddress (pc, field, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult LoadLength(APC pc, Dummy dest, Dummy array, TData data)
		{
			return this.visitor.LoadLength (pc, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult LoadStaticField(APC pc, Field field, Dummy dest, TData data)
		{
			return this.visitor.LoadStaticField (pc, field, Push (pc, 0), data);
		}

		public TResult LoadStaticFieldAddress(APC pc, Field field, Dummy dest, TData data)
		{
			return this.visitor.LoadStaticFieldAddress (pc, field, Push (pc, 0), data);
		}

		public TResult LoadTypeToken(APC pc, TypeNode type, Dummy dest, TData data)
		{
			return this.visitor.LoadTypeToken (pc, type, Push (pc, 0), data);
		}

		public TResult LoadFieldToken(APC pc, Field field, Dummy dest, TData data)
		{
			return this.visitor.LoadFieldToken (pc, field, Push (pc, 0), data);
		}

		public TResult LoadMethodToken(APC pc, Method method, Dummy dest, TData data)
		{
			return this.visitor.LoadMethodToken (pc, method, Push (pc, 0), data);
		}

		public TResult NewArray<ArgList>(APC pc, TypeNode type, Dummy dest, ArgList lengths, TData data)
			where ArgList : IIndexable<Dummy>
		{
			return this.visitor.NewArray (pc, type, Push (pc, 1), PopSequence (pc, lengths.Count, 0), data);
		}

		public TResult NewObj<ArgList>(APC pc, Method ctor, Dummy dest, ArgList args, TData data)
			where ArgList : IIndexable<Dummy>
		{
			int argsCount = GetParametersCount (ctor, 0) - 1;
			return this.visitor.NewObj (pc, ctor, Push (pc, argsCount), PopSequence (pc, argsCount, 0), data);
		}

		public TResult MkRefAny(APC pc, TypeNode type, Dummy dest, Dummy obj, TData data)
		{
			return this.visitor.MkRefAny (pc, type, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult RefAnyType(APC pc, Dummy dest, Dummy source, TData data)
		{
			return this.visitor.RefAnyType (pc, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult RefAnyVal(APC pc, TypeNode type, Dummy dest, Dummy source, TData data)
		{
			return this.visitor.RefAnyVal (pc, type, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult Rethrow(APC pc, TData data)
		{
			return this.visitor.Rethrow (pc, data);
		}

		public TResult StoreElement(APC pc, TypeNode type, Dummy array, Dummy index, Dummy value, TData data)
		{
			return this.visitor.StoreElement (pc, type, Pop (pc, 2), Pop (pc, 1), Pop (pc, 0), data);
		}

		public TResult StoreField(APC pc, Field field, Dummy obj, Dummy value, TData data)
		{
			return this.visitor.StoreField (pc, field, Pop (pc, 1), Pop (pc, 0), data);
		}

		public TResult StoreStaticField(APC pc, Field field, Dummy value, TData data)
		{
			return this.visitor.StoreStaticField (pc, field, Pop (pc, 0), data);
		}

		public TResult Throw(APC pc, Dummy exception, TData data)
		{
			return this.visitor.Throw (pc, Pop (pc, 0), data);
		}

		public TResult Unbox(APC pc, TypeNode type, Dummy dest, Dummy obj, TData data)
		{
			return this.visitor.Unbox (pc, type, Push (pc, 1), Pop (pc, 0), data);
		}

		public TResult UnboxAny(APC pc, TypeNode type, Dummy dest, Dummy obj, TData data)
		{
			return this.visitor.UnboxAny (pc, type, Push (pc, 1), Pop (pc, 0), data);
		}
		#endregion
	}
}