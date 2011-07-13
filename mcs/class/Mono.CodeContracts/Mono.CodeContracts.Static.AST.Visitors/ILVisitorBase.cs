// 
// ILVisitorBase.cs
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
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.AST.Visitors
{
	/// <summary>
	/// Abstract base implementation of ILVisitor
	/// </summary>
	/// <remarks> Each (non-overriden) operation returns DefaultVisit(pc, data) </remarks>
	abstract class ILVisitorBase<Label, Source, Dest, Data, Result>
		: IILVisitor<Label, Source, Dest, Data, Result>
	{
		public abstract Result DefaultVisit(Label pc, Data data);

		#region Implementation of IExpressionILVisitor<Label,Type,Source,Dest,Data,Result>
		public virtual Result Binary(Label pc, BinaryOperator bop, Dest dest, Source operand1, Source operand2, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Isinst(Label pc, TypeNode type, Dest dest, Source obj, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadNull(Label pc, Dest dest, Data polarity)
		{
			return DefaultVisit (pc, polarity);
		}

		public virtual Result LoadConst(Label pc, TypeNode type, object constant, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Sizeof(Label pc, TypeNode type, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Unary(Label pc, UnaryOperator uop, bool unsigned, Dest dest, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}
		#endregion

		#region Implementation of IILVisitor<Label,Local,Parameter,Method,Field,Type,Source,Dest,Data,Result>
		public virtual Result Arglist(Label pc, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Branch(Label pc, Label target, bool leavesExceptionBlock, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result BranchCond(Label pc, Label target, BranchOperator bop, Source value1, Source value2, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result BranchTrue(Label pc, Label target, Source cond, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result BranchFalse(Label pc, Label target, Source cond, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Break(Label pc, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Call<TypeList, ArgList>(Label pc, Method method, bool virt, TypeList extraVarargs, Dest dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode> where ArgList : IIndexable<Source>
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Calli<TypeList, ArgList>(Label pc, TypeNode returnType, TypeList argTypes, bool instance, Dest dest, Source functionPointer, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode> where ArgList : IIndexable<Source>
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result CheckFinite(Label pc, Dest dest, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result CopyBlock(Label pc, Source destAddress, Source srcAddress, Source len, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result EndFilter(Label pc, Source decision, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result EndFinally(Label pc, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Jmp(Label pc, Method method, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadArg(Label pc, Parameter param, bool isOld, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadArgAddress(Label pc, Parameter param, bool isOld, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadLocal(Label pc, Local local, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadLocalAddress(Label pc, Local local, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Nop(Label pc, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Pop(Label pc, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Return(Label pc, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result StoreArg(Label pc, Parameter param, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result StoreLocal(Label pc, Local local, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Switch(Label pc, TypeNode type, IEnumerable<Pair<object, Label>> cases, Source value, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Box(Label pc, TypeNode type, Dest dest, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result ConstrainedCallvirt<TypeList, ArgList>(Label pc, Method method, TypeNode constraint, TypeList extraVarargs, Dest dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Source>
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result CastClass(Label pc, TypeNode type, Dest dest, Source obj, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result CopyObj(Label pc, TypeNode type, Source destPtr, Source sourcePtr, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Initobj(Label pc, TypeNode type, Source ptr, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadElement(Label pc, TypeNode type, Dest dest, Source array, Source index, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadField(Label pc, Field field, Dest dest, Source obj, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadFieldAddress(Label pc, Field field, Dest dest, Source obj, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadLength(Label pc, Dest dest, Source array, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadStaticField(Label pc, Field field, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadStaticFieldAddress(Label pc, Field field, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadTypeToken(Label pc, TypeNode type, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadFieldToken(Label pc, Field field, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadMethodToken(Label pc, Method method, Dest dest, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result NewArray<ArgList>(Label pc, TypeNode type, Dest dest, ArgList lengths, Data data) 
			where ArgList : IIndexable<Source>
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result NewObj<ArgList>(Label pc, Method ctor, Dest dest, ArgList args, Data data) 
			where ArgList : IIndexable<Source>
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result MkRefAny(Label pc, TypeNode type, Dest dest, Source obj, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result RefAnyType(Label pc, Dest dest, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result RefAnyVal(Label pc, TypeNode type, Dest dest, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Rethrow(Label pc, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result StoreElement(Label pc, TypeNode type, Source array, Source index, Source value, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result StoreField(Label pc, Field field, Source obj, Source value, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result StoreStaticField(Label pc, Field field, Source value, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Throw(Label pc, Source exception, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Unbox(Label pc, TypeNode type, Dest dest, Source obj, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result UnboxAny(Label pc, TypeNode type, Dest dest, Source obj, Data data)
		{
			return DefaultVisit (pc, data);
		}
		#endregion

		#region Implementation of ISyntheticILVisitor<Label,Method,Field,Type,Source,Dest,Data,Result>
		public virtual Result Entry(Label pc, Method method, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Assume(Label pc, EdgeTag tag, Source condition, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result Assert(Label pc, EdgeTag tag, Source condition, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result BeginOld(Label pc, Label matchingEnd, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result EndOld(Label pc, Label matchingBegin, TypeNode type, Dest dest, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadStack(Label pc, int offset, Dest dest, Source source, bool isOld, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadStackAddress(Label pc, int offset, Dest dest, Source source, TypeNode type, bool isOld, Data data)
		{
			return DefaultVisit (pc, data);
		}

		public virtual Result LoadResult(Label pc, TypeNode type, Dest dest, Source source, Data data)
		{
			return DefaultVisit (pc, data);
		}
		#endregion
	}
}