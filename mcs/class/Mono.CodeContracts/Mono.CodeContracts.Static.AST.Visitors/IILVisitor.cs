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
	interface IILVisitor<Label, Source, Dest, Data, Result> :
		IExpressionILVisitor<Label, Source, Dest, Data, Result>,
		ISyntheticILVisitor<Label, Source, Dest, Data, Result> {
		Result Arglist (Label pc, Dest dest, Data data);
		Result Branch (Label pc, Label target, bool leavesExceptionBlock, Data data);
		Result BranchCond (Label pc, Label target, BranchOperator bop, Source value1, Source value2, Data data);
		Result BranchTrue (Label pc, Label target, Source cond, Data data);
		Result BranchFalse (Label pc, Label target, Source cond, Data data);
		Result Break (Label pc, Data data);

		Result Call<TypeList, ArgList> (Label pc, Method method, bool virt, TypeList extraVarargs, Dest dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Source>;

		Result Calli<TypeList, ArgList> (Label pc, TypeNode returnType, TypeList argTypes, bool instance, Dest dest, Source functionPointer, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Source>;

		Result CheckFinite (Label pc, Dest dest, Source source, Data data);
		Result CopyBlock (Label pc, Source destAddress, Source srcAddress, Source len, Data data);
		Result EndFilter (Label pc, Source decision, Data data);
		Result EndFinally (Label pc, Data data);
		Result Jmp (Label pc, Method method, Data data);
		Result LoadArg (Label pc, Parameter argument, bool isOld, Dest dest, Data data);
		Result LoadArgAddress (Label pc, Parameter argument, bool isOld, Dest dest, Data data);
		Result LoadLocal (Label pc, Local local, Dest dest, Data data);
		Result LoadLocalAddress (Label pc, Local local, Dest dest, Data data);
		Result Nop (Label pc, Data data);
		Result Pop (Label pc, Source source, Data data);
		Result Return (Label pc, Source source, Data data);
		Result StoreArg (Label pc, Parameter argument, Source source, Data data);
		Result StoreLocal (Label pc, Local local, Source source, Data data);
		Result Switch (Label pc, TypeNode type, IEnumerable<Pair<object, Label>> cases, Source value, Data data);
		Result Box (Label pc, TypeNode type, Dest dest, Source source, Data data);

		Result ConstrainedCallvirt<TypeList, ArgList> (Label pc, Method method, TypeNode constraint, TypeList extraVarargs, Dest dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<Source>;

		Result CastClass (Label pc, TypeNode type, Dest dest, Source obj, Data data);
		Result CopyObj (Label pc, TypeNode type, Source destPtr, Source sourcePtr, Data data);
		Result Initobj (Label pc, TypeNode type, Source ptr, Data data);
		Result LoadElement (Label pc, TypeNode type, Dest dest, Source array, Source index, Data data);
		Result LoadField (Label pc, Field field, Dest dest, Source obj, Data data);
		Result LoadFieldAddress (Label pc, Field field, Dest dest, Source obj, Data data);
		Result LoadLength (Label pc, Dest dest, Source array, Data data);
		Result LoadStaticField (Label pc, Field field, Dest dest, Data data);
		Result LoadStaticFieldAddress (Label pc, Field field, Dest dest, Data data);
		Result LoadTypeToken (Label pc, TypeNode type, Dest dest, Data data);
		Result LoadFieldToken (Label pc, Field type, Dest dest, Data data);
		Result LoadMethodToken (Label pc, Method type, Dest dest, Data data);

		Result NewArray<ArgList> (Label pc, TypeNode type, Dest dest, ArgList lengths, Data data)
			where ArgList : IIndexable<Source>;

		Result NewObj<ArgList> (Label pc, Method ctor, Dest dest, ArgList args, Data data)
			where ArgList : IIndexable<Source>;

		Result MkRefAny (Label pc, TypeNode type, Dest dest, Source obj, Data data);
		Result RefAnyType (Label pc, Dest dest, Source source, Data data);
		Result RefAnyVal (Label pc, TypeNode type, Dest dest, Source source, Data data);
		Result Rethrow (Label pc, Data data);
		Result StoreElement (Label pc, TypeNode type, Source array, Source index, Source value, Data data);
		Result StoreField (Label pc, Field field, Source obj, Source value, Data data);
		Result StoreStaticField (Label pc, Field field, Source value, Data data);
		Result Throw (Label pc, Source exception, Data data);
		Result Unbox (Label pc, TypeNode type, Dest dest, Source obj, Data data);
		Result UnboxAny (Label pc, TypeNode type, Dest dest, Source obj, Data data);
		}
}
