// 
// IILVisitor.cs
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
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.AST.Visitors {
	interface IILVisitor<TLabel, TSource, TDest, TData, TResult> :
		IExpressionILVisitor<TLabel, TSource, TDest, TData, TResult>,
		ISyntheticILVisitor<TLabel, TSource, TDest, TData, TResult> {
		TResult Arglist (TLabel pc, TDest dest, TData data);
		TResult Branch (TLabel pc, TLabel target, bool leavesExceptionBlock, TData data);
		TResult BranchCond (TLabel pc, TLabel target, BranchOperator bop, TSource value1, TSource value2, TData data);
		TResult BranchTrue (TLabel pc, TLabel target, TSource cond, TData data);
		TResult BranchFalse (TLabel pc, TLabel target, TSource cond, TData data);
		TResult Break (TLabel pc, TData data);

		TResult Call<TypeList, ArgList> (TLabel pc, Method method, bool virt, TypeList extraVarargs, TDest dest, ArgList args, TData data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<TSource>;

		TResult Calli<TypeList, ArgList> (TLabel pc, TypeNode returnType, TypeList argTypes, bool instance, TDest dest, TSource functionPointer, ArgList args, TData data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<TSource>;

		TResult CheckFinite (TLabel pc, TDest dest, TSource source, TData data);
		TResult CopyBlock (TLabel pc, TSource destAddress, TSource srcAddress, TSource len, TData data);
		TResult EndFilter (TLabel pc, TSource decision, TData data);
		TResult EndFinally (TLabel pc, TData data);
		TResult Jmp (TLabel pc, Method method, TData data);
		TResult LoadArg (TLabel pc, Parameter argument, bool isOld, TDest dest, TData data);
		TResult LoadArgAddress (TLabel pc, Parameter argument, bool isOld, TDest dest, TData data);
		TResult LoadLocal (TLabel pc, Local local, TDest dest, TData data);
		TResult LoadLocalAddress (TLabel pc, Local local, TDest dest, TData data);
		TResult Nop (TLabel pc, TData data);
		TResult Pop (TLabel pc, TSource source, TData data);
		TResult Return (TLabel pc, TSource source, TData data);
		TResult StoreArg (TLabel pc, Parameter argument, TSource source, TData data);
		TResult StoreLocal (TLabel pc, Local local, TSource source, TData data);
		TResult Switch (TLabel pc, TypeNode type, IEnumerable<Pair<object, TLabel>> cases, TSource value, TData data);
		TResult Box (TLabel pc, TypeNode type, TDest dest, TSource source, TData data);

		TResult ConstrainedCallvirt<TypeList, ArgList> (TLabel pc, Method method, TypeNode constraint, TypeList extraVarargs, TDest dest, ArgList args, TData data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<TSource>;

		TResult CastClass (TLabel pc, TypeNode type, TDest dest, TSource obj, TData data);
		TResult CopyObj (TLabel pc, TypeNode type, TSource destPtr, TSource sourcePtr, TData data);
		TResult Initobj (TLabel pc, TypeNode type, TSource ptr, TData data);
		TResult LoadElement (TLabel pc, TypeNode type, TDest dest, TSource array, TSource index, TData data);
		TResult LoadField (TLabel pc, Field field, TDest dest, TSource obj, TData data);
		TResult LoadFieldAddress (TLabel pc, Field field, TDest dest, TSource obj, TData data);
		TResult LoadLength (TLabel pc, TDest dest, TSource array, TData data);
		TResult LoadStaticField (TLabel pc, Field field, TDest dest, TData data);
		TResult LoadStaticFieldAddress (TLabel pc, Field field, TDest dest, TData data);
		TResult LoadTypeToken (TLabel pc, TypeNode type, TDest dest, TData data);
		TResult LoadFieldToken (TLabel pc, Field type, TDest dest, TData data);
		TResult LoadMethodToken (TLabel pc, Method type, TDest dest, TData data);

		TResult NewArray<ArgList> (TLabel pc, TypeNode type, TDest dest, ArgList lengths, TData data)
			where ArgList : IIndexable<TSource>;

		TResult NewObj<ArgList> (TLabel pc, Method ctor, TDest dest, ArgList args, TData data)
			where ArgList : IIndexable<TSource>;

		TResult MkRefAny (TLabel pc, TypeNode type, TDest dest, TSource obj, TData data);
		TResult RefAnyType (TLabel pc, TDest dest, TSource source, TData data);
		TResult RefAnyVal (TLabel pc, TypeNode type, TDest dest, TSource source, TData data);
		TResult Rethrow (TLabel pc, TData data);
		TResult StoreElement (TLabel pc, TypeNode type, TSource array, TSource index, TSource value, TData data);
		TResult StoreField (TLabel pc, Field field, TSource obj, TSource value, TData data);
		TResult StoreStaticField (TLabel pc, Field field, TSource value, TData data);
		TResult Throw (TLabel pc, TSource exception, TData data);
		TResult Unbox (TLabel pc, TypeNode type, TDest dest, TSource obj, TData data);
		TResult UnboxAny (TLabel pc, TypeNode type, TDest dest, TSource obj, TData data);
	}
}
