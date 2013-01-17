// 
// StackToSymbolicAdapter.cs
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

using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Analysis.HeapAnalysis {
	struct StackToSymbolicAdapter<Data, Result, Visitor> : IILVisitor<APC, int, int, Data, Result>
		where Visitor : IILVisitor<APC, SymbolicValue, SymbolicValue, Data, Result> {
		private readonly Visitor delegatee;
		private readonly HeapAnalysis parent;

		public StackToSymbolicAdapter (HeapAnalysis parent, Visitor delegatee)
		{
			this.parent = parent;
			this.delegatee = delegatee;
		}

		private bool PreStateLookup (APC pc, out Domain domain)
		{
			return this.parent.PreStateLookup (pc, out domain);
		}

		private bool PostStateLookup (APC pc, out Domain domain)
		{
			return this.parent.PostStateLookup (pc, out domain);
		}

		private SymbolicValue ConvertSource (APC pc, int source)
		{
			Domain domain;
			if (!PreStateLookup (pc, out domain) || domain.IsBottom)
				return new SymbolicValue ();
			if (source < 0)
				return new SymbolicValue (domain.VoidAddr);

			SymbolicValue sv;
			domain.TryGetCorrespondingValueAbstraction (source, out sv);
			return sv;
		}

		private SymbolicValue ConvertOldSource (APC pc, int source)
		{
			Domain domain;
			if (!PreStateLookup (pc, out domain) || domain.IsBottom)
				return new SymbolicValue ();

			if (source < 0)
				return new SymbolicValue (domain.VoidAddr);

			Domain oldDomain = AnalysisDecoder.FindOldState (pc, domain);
			if (oldDomain == null)
				return new SymbolicValue (domain.VoidAddr);

			SymbolicValue sv;
			oldDomain.TryGetCorrespondingValueAbstraction (source, out sv);
			return sv;
		}

		private SymbolicValue ConvertDest (APC pc, int dest)
		{
			Domain domain;
			if (!PostStateLookup (pc, out domain))
				return new SymbolicValue ();

			SymbolicValue sv;
			domain.TryGetCorrespondingValueAbstraction (dest, out sv);
			return sv;
		}

		private SymbolicValue ConvertOldDest (APC pc, int dest)
		{
			SymbolicValue sv = default(SymbolicValue);
			Domain domain;
			if (!PostStateLookup (pc, out domain))
				return sv;

			domain.OldDomain.TryGetCorrespondingValueAbstraction (dest, out sv);
			return sv;
		}

		private SymbolicValue ConvertSourceDeref (APC pc, int source)
		{
			Domain domain;
			if (!PreStateLookup (pc, out domain))
				return new SymbolicValue ();
			if (source < 0)
				return new SymbolicValue (domain.VoidAddr);

			SymValue addr = domain.LoadValue (source);
			if (!PostStateLookup (pc, out domain))
				return new SymbolicValue ();

			return domain.TryLoadValue (addr);
		}

		private SymbolicValue TryConvertUnbox (APC pc, int source)
		{
			Domain domain;
			if (!PreStateLookup (pc, out domain))
				return new SymbolicValue ();

			if (source < 0)
				return new SymbolicValue (domain.VoidAddr);

			SymbolicValue sv;
			if (!domain.TryGetUnboxedValue (source, out sv))
				domain.TryGetCorrespondingValueAbstraction (source, out sv);
			return sv;
		}

		private ArgumentSourceWrapper<ArgList> ConvertSources<ArgList> (APC pc, ArgList args)
			where ArgList : IIndexable<int>
		{
			return new ArgumentSourceWrapper<ArgList> (args, this.parent.GetPreState (pc));
		}

		private bool InsideOld (APC pc)
		{
			Domain domain;
			return PreStateLookup (pc, out domain) && domain.OldDomain != null;
		}

		#region Implementation of IExpressionILVisitor<APC,int,int,Data,Result>
		public Result Binary (APC pc, BinaryOperator op, int dest, int operand1, int operand2, Data data)
		{
			if (op != BinaryOperator.Cobjeq) {
				return this.delegatee.Binary (pc, op, ConvertDest (pc, dest),
				                              ConvertSource (pc, operand1), ConvertSource (pc, operand2),
				                              data);
			}

			SymbolicValue op1 = TryConvertUnbox (pc, operand1);
			SymbolicValue op2 = TryConvertUnbox (pc, operand2);

			return this.delegatee.Binary (pc, op, ConvertDest (pc, dest), op1, op2, data);
		}

		public Result Isinst (APC pc, TypeNode type, int dest, int obj, Data data)
		{
			return this.delegatee.Isinst (pc, type, ConvertDest (pc, dest), ConvertSource (pc, obj), data);
		}

		public Result LoadNull (APC pc, int dest, Data polarity)
		{
			return this.delegatee.LoadNull (pc, ConvertDest (pc, dest), polarity);
		}

		public Result LoadConst (APC pc, TypeNode type, object constant, int dest, Data data)
		{
			return this.delegatee.LoadConst (pc, type, constant, ConvertDest (pc, dest), data);
		}

		public Result Sizeof (APC pc, TypeNode type, int dest, Data data)
		{
			return this.delegatee.Sizeof (pc, type, ConvertDest (pc, dest), data);
		}

		public Result Unary (APC pc, UnaryOperator op, bool unsigned, int dest, int source, Data data)
		{
			return this.delegatee.Unary (pc, op, unsigned, ConvertDest (pc, dest), ConvertSource (pc, source), data);
		}
		#endregion

		#region Implementation of ISyntheticILVisitor<APC,int,int,Data,Result>
		public Result Entry (APC pc, Method method, Data data)
		{
			return this.delegatee.Entry (pc, method, data);
		}

		public Result Assume (APC pc, EdgeTag tag, int source, Data data)
		{
			return this.delegatee.Assume (pc, tag, ConvertSource (pc, source), data);
		}

		public Result Assert (APC pc, EdgeTag tag, int source, Data data)
		{
			return this.delegatee.Assert (pc, tag, ConvertSource (pc, source), data);
		}

		public Result BeginOld (APC pc, APC matchingEnd, Data data)
		{
			return this.delegatee.BeginOld (pc, matchingEnd, data);
		}

		public Result EndOld (APC pc, APC matchingBegin, TypeNode type, int dest, int source, Data data)
		{
			return this.delegatee.Nop (pc, data);
		}

		public Result LoadStack (APC pc, int offset, int dest, int source, bool isOld, Data data)
		{
			SymbolicValue src = isOld ? ConvertOldSource (pc, source) : ConvertSource (pc, source);
			return this.delegatee.LoadStack (pc, offset, ConvertDest (pc, dest), src, isOld, data);
		}

		public Result LoadStackAddress (APC pc, int offset, int dest, int source, TypeNode type, bool isOld, Data data)
		{
			return this.delegatee.LoadStackAddress (pc, offset, ConvertDest (pc, dest), ConvertSource (pc, source), type, isOld, data);
		}

		public Result LoadResult (APC pc, TypeNode type, int dest, int source, Data data)
		{
			return this.delegatee.LoadResult (pc, type, ConvertDest (pc, dest), ConvertSource (pc, source), data);
		}
		#endregion

		#region Implementation of IILVisitor<APC,int,int,Data,Result>
		public Result Arglist (APC pc, int dest, Data data)
		{
			return this.delegatee.Arglist (pc, ConvertDest (pc, dest), data);
		}

		public Result Branch (APC pc, APC target, bool leavesExceptionBlock, Data data)
		{
			return this.delegatee.Branch (pc, target, leavesExceptionBlock, data);
		}

		public Result BranchCond (APC pc, APC target, BranchOperator bop, int value1, int value2, Data data)
		{
			return this.delegatee.BranchCond (pc, target, bop, ConvertSource (pc, value1), ConvertSource (pc, value2), data);
		}

		public Result BranchTrue (APC pc, APC target, int cond, Data data)
		{
			return this.delegatee.BranchTrue (pc, target, ConvertSource (pc, cond), data);
		}

		public Result BranchFalse (APC pc, APC target, int cond, Data data)
		{
			return this.delegatee.BranchFalse (pc, target, ConvertSource (pc, cond), data);
		}

		public Result Break (APC pc, Data data)
		{
			return this.delegatee.Break (pc, data);
		}

		public Result Call<TypeList, ArgList> (APC pc, Method method, bool virt, TypeList extraVarargs, int dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<int>
		{
			if (!this.parent.MetaDataProvider.IsVoidMethod (method) && InsideOld (pc))
				return this.delegatee.LoadStack (pc, 0, ConvertDest (pc, dest), ConvertOldDest (pc, dest), true, data);

			return DelegateCall (pc, method, virt, extraVarargs, dest, args, data);
		}

		public Result Calli<TypeList, ArgList> (APC pc, TypeNode returnType, TypeList argTypes, bool instance, int dest, int functionPointer, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<int>
		{
			if (!this.parent.MetaDataProvider.IsVoid (returnType) && InsideOld (pc))
				return this.delegatee.LoadStack (pc, 0, ConvertDest (pc, dest), ConvertOldDest (pc, dest), true, data);

			return this.delegatee.Calli (pc, returnType, argTypes, instance,
			                             ConvertDest (pc, dest), ConvertSource (pc, functionPointer),
			                             ConvertSources (pc, args), data);
		}

		public Result CheckFinite (APC pc, int dest, int source, Data data)
		{
			return this.delegatee.CheckFinite (pc, ConvertDest (pc, dest), ConvertSource (pc, source), data);
		}

		public Result CopyBlock (APC pc, int destAddress, int srcAddress, int len, Data data)
		{
			return this.delegatee.CopyBlock (pc, ConvertSource (pc, destAddress), ConvertSource (pc, srcAddress), ConvertSource (pc, len), data);
		}

		public Result EndFilter (APC pc, int decision, Data data)
		{
			return this.delegatee.EndFilter (pc, ConvertSource (pc, decision), data);
		}

		public Result EndFinally (APC pc, Data data)
		{
			return this.delegatee.EndFinally (pc, data);
		}

		public Result Jmp (APC pc, Method method, Data data)
		{
			return this.delegatee.Jmp (pc, method, data);
		}

		public Result LoadArg (APC pc, Parameter argument, bool isOld, int dest, Data data)
		{
			return this.delegatee.LoadArg (pc, argument, isOld, ConvertDest (pc, dest), data);
		}

		public Result LoadArgAddress (APC pc, Parameter argument, bool isOld, int dest, Data data)
		{
			return this.delegatee.LoadArgAddress (pc, argument, isOld, ConvertDest (pc, dest), data);
		}

		public Result LoadLocal (APC pc, Local local, int dest, Data data)
		{
			return this.delegatee.LoadLocal (pc, local, ConvertDest (pc, dest), data);
		}

		public Result LoadLocalAddress (APC pc, Local local, int dest, Data data)
		{
			return this.delegatee.LoadLocalAddress (pc, local, ConvertDest (pc, dest), data);
		}

		public Result Nop (APC pc, Data data)
		{
			return this.delegatee.Nop (pc, data);
		}

		public Result Pop (APC pc, int source, Data data)
		{
			return this.delegatee.Pop (pc, ConvertSource (pc, source), data);
		}

		public Result Return (APC pc, int source, Data data)
		{
			return this.delegatee.Return (pc, ConvertSource (pc, source), data);
		}

		public Result StoreArg (APC pc, Parameter argument, int source, Data data)
		{
			return this.delegatee.StoreArg (pc, argument, ConvertSource (pc, source), data);
		}

		public Result StoreLocal (APC pc, Local local, int source, Data data)
		{
			return this.delegatee.StoreLocal (pc, local, ConvertSource (pc, source), data);
		}

		public Result Switch (APC pc, TypeNode type, IEnumerable<Pair<object, APC>> cases, int value, Data data)
		{
			return this.delegatee.Switch (pc, type, cases, ConvertSource (pc, value), data);
		}

		public Result Box (APC pc, TypeNode type, int dest, int source, Data data)
		{
			return this.delegatee.Box (pc, type, ConvertDest (pc, dest), ConvertSource (pc, source), data);
		}

		public Result ConstrainedCallvirt<TypeList, ArgList> (APC pc, Method method, TypeNode constraint,
		                                                      TypeList extraVarargs, int dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<int>
		{
			return this.delegatee.ConstrainedCallvirt (pc, method, constraint, extraVarargs,
			                                           ConvertDest (pc, dest), ConvertSources (pc, args), data);
		}

		public Result CastClass (APC pc, TypeNode type, int dest, int obj, Data data)
		{
			return this.delegatee.CastClass (pc, type, ConvertDest (pc, dest), ConvertSource (pc, obj), data);
		}

		public Result CopyObj (APC pc, TypeNode type, int destPtr, int sourcePtr, Data data)
		{
			return this.delegatee.CopyObj (pc, type, ConvertSource (pc, destPtr), ConvertSource (pc, sourcePtr), data);
		}

		public Result Initobj (APC pc, TypeNode type, int ptr, Data data)
		{
			return this.delegatee.Initobj (pc, type, ConvertSource (pc, ptr), data);
		}

		public Result LoadElement (APC pc, TypeNode type, int dest, int array, int index, Data data)
		{
			if (InsideOld (pc))
				return this.delegatee.LoadStack (pc, 0, ConvertDest (pc, dest), ConvertOldDest (pc, dest), true, data);
			return this.delegatee.LoadElement (pc, type, ConvertDest (pc, dest), ConvertSource (pc, array), ConvertSource (pc, index), data);
		}

		public Result LoadField (APC pc, Field field, int dest, int obj, Data data)
		{
			if (InsideOld (pc))
				return this.delegatee.LoadStack (pc, 0, ConvertDest (pc, dest), ConvertOldDest (pc, dest), true, data);

			return this.delegatee.LoadField (pc, field, ConvertDest (pc, dest), ConvertSource (pc, obj), data);
		}

		public Result LoadFieldAddress (APC pc, Field field, int dest, int obj, Data data)
		{
			return this.delegatee.LoadFieldAddress (pc, field, ConvertDest (pc, dest), ConvertSource (pc, obj), data);
		}

		public Result LoadLength (APC pc, int dest, int array, Data data)
		{
			return this.delegatee.LoadLength (pc, ConvertDest (pc, dest), ConvertSource (pc, array), data);
		}

		public Result LoadStaticField (APC pc, Field field, int dest, Data data)
		{
			if (InsideOld (pc))
				return this.delegatee.LoadStack (pc, 0, ConvertDest (pc, dest), ConvertOldDest (pc, dest), true, data);

			return this.delegatee.LoadStaticField (pc, field, ConvertDest (pc, dest), data);
		}

		public Result LoadStaticFieldAddress (APC pc, Field field, int dest, Data data)
		{
			return this.delegatee.LoadStaticFieldAddress (pc, field, ConvertDest (pc, dest), data);
		}

		public Result LoadTypeToken (APC pc, TypeNode type, int dest, Data data)
		{
			return this.delegatee.LoadTypeToken (pc, type, ConvertDest (pc, dest), data);
		}

		public Result LoadFieldToken (APC pc, Field type, int dest, Data data)
		{
			return this.delegatee.LoadFieldToken (pc, type, ConvertDest (pc, dest), data);
		}

		public Result LoadMethodToken (APC pc, Method type, int dest, Data data)
		{
			return this.delegatee.LoadMethodToken (pc, type, ConvertDest (pc, dest), data);
		}

		public Result NewArray<ArgList> (APC pc, TypeNode type, int dest, ArgList lengths, Data data) where ArgList : IIndexable<int>
		{
			return this.delegatee.NewArray (pc, type, ConvertDest (pc, dest), ConvertSources (pc, lengths), data);
		}

		public Result NewObj<ArgList> (APC pc, Method ctor, int dest, ArgList args, Data data) where ArgList : IIndexable<int>
		{
			return this.delegatee.NewObj (pc, ctor, ConvertDest (pc, dest), ConvertSources (pc, args), data);
		}

		public Result MkRefAny (APC pc, TypeNode type, int dest, int obj, Data data)
		{
			return this.delegatee.MkRefAny (pc, type, ConvertDest (pc, dest), ConvertSource (pc, obj), data);
		}

		public Result RefAnyType (APC pc, int dest, int source, Data data)
		{
			return this.delegatee.RefAnyType (pc, ConvertDest (pc, dest), ConvertSource (pc, source), data);
		}

		public Result RefAnyVal (APC pc, TypeNode type, int dest, int source, Data data)
		{
			return this.delegatee.RefAnyVal (pc, type, ConvertDest (pc, dest), ConvertSource (pc, source), data);
		}

		public Result Rethrow (APC pc, Data data)
		{
			return this.delegatee.Rethrow (pc, data);
		}

		public Result StoreElement (APC pc, TypeNode type, int array, int index, int value, Data data)
		{
			return this.delegatee.StoreElement (pc, type, ConvertSource (pc, array), ConvertSource (pc, index), ConvertSource (pc, value), data);
		}

		public Result StoreField (APC pc, Field field, int obj, int value, Data data)
		{
			return this.delegatee.StoreField (pc, field, ConvertSource (pc, obj), ConvertSource (pc, value), data);
		}

		public Result StoreStaticField (APC pc, Field field, int value, Data data)
		{
			return this.delegatee.StoreStaticField (pc, field, ConvertSource (pc, value), data);
		}

		public Result Throw (APC pc, int exception, Data data)
		{
			return this.delegatee.Throw (pc, ConvertSource (pc, exception), data);
		}

		public Result Unbox (APC pc, TypeNode type, int dest, int obj, Data data)
		{
			return this.delegatee.Unbox (pc, type, ConvertDest (pc, dest), ConvertSource (pc, obj), data);
		}

		public Result UnboxAny (APC pc, TypeNode type, int dest, int obj, Data data)
		{
			return this.delegatee.UnboxAny (pc, type, ConvertDest (pc, dest), ConvertSource (pc, obj), data);
		}

		private Result DelegateCall<TypeList, ArgList> (APC pc, Method method, bool virt, TypeList extraVarargs, int dest, ArgList args, Data data)
			where TypeList : IIndexable<TypeNode>
			where ArgList : IIndexable<int>
		{
			TypeNode declaringType = this.parent.MetaDataProvider.DeclaringType (method);
			if (args.Count == 2) {
				string name = this.parent.MetaDataProvider.Name (method);
				if (name == "Equals") {
					return this.delegatee.Binary (pc, BinaryOperator.Cobjeq,
					                              ConvertDest (pc, dest),
					                              TryConvertUnbox (pc, args [0]), TryConvertUnbox (pc, args [1]), data);
				}

				if (this.parent.MetaDataProvider.IsReferenceType (declaringType)) {
					if (name == "op_Inequality") {
						return this.delegatee.Binary (pc, BinaryOperator.Cne_Un,
						                              ConvertDest (pc, dest),
						                              ConvertSource (pc, args [0]), ConvertSource (pc, args [1]), data);
					}
					if (name == "op_Equality") {
						return this.delegatee.Binary (pc, BinaryOperator.Cobjeq,
						                              ConvertDest (pc, dest),
						                              ConvertSource (pc, args [0]), ConvertSource (pc, args [1]), data);
					}
				}
			}

			return this.delegatee.Call (pc, method, virt, extraVarargs, ConvertDest (pc, dest), ConvertSources (pc, args), data);
		}
		#endregion

		#region Nested type: ArgumentSourceWrapper
		private struct ArgumentSourceWrapper<ArgList> : IIndexable<SymbolicValue>
			where ArgList : IIndexable<int> {
			private readonly Domain state;
			private readonly ArgList underlying;

			public ArgumentSourceWrapper (ArgList underlying, Domain state)
			{
				this.underlying = underlying;
				this.state = state;
			}

			#region Implementation of IIndexable<SymbolicValue>
			public int Count
			{
				get { return this.underlying.Count; }
			}

			public SymbolicValue this [int index]
			{
				get { return Map (this.underlying [index]); }
			}

			private SymbolicValue Map (int i)
			{
				SymbolicValue sv = default(SymbolicValue);
				if (this.state == null)
					return sv;

				this.state.TryGetCorrespondingValueAbstraction (i, out sv);
				return sv;
			}
			#endregion
			}
		#endregion
	}
}
