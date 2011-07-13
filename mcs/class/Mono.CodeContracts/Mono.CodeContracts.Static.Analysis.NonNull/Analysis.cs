// 
// Analysis.cs
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

using System;
using System.Collections.Generic;
using System.IO;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Analysis.Drivers;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataFlowAnalysis;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Providers;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Analysis.NonNull {
	class Analysis<E, V> :
		ILVisitorBase<APC, V, V, Domain<E, V>, Domain<E, V>>,
		IAnalysis<APC, Domain<E, V>, IILVisitor<APC, V, V, Domain<E, V>, Domain<E, V>>, IImmutableMap<V, LispList<V>>>,
		IMethodResult<V>, IFactBase<V> where E : IEquatable<E> where V : IEquatable<V> {
		private readonly Dictionary<APC, Domain<E, V>> callSiteCache = new Dictionary<APC, Domain<E, V>> ();
		private readonly IMethodDriver<E, V> method_driver;
		private IFixPointInfo<APC, Domain<E, V>> fix_point_info;

		public Analysis (IMethodDriver<E, V> mdriver)
		{
			this.method_driver = mdriver;
		}

		protected internal IExpressionContextProvider<E, V> ContextProvider
		{
			get { return this.method_driver.ContextProvider; }
		}

		protected IMetaDataProvider MetaDataProvider
		{
			get { return this.method_driver.MetaDataProvider; }
		}

		#region IAnalysis<APC,Domain<E,V>,IILVisitor<APC,V,V,Domain<E,V>,Domain<E,V>>,IImmutableMap<V,LispList<V>>> Members
		public IILVisitor<APC, V, V, Domain<E, V>, Domain<E, V>> GetVisitor ()
		{
			return this;
		}

		public Domain<E, V> Join (Pair<APC, APC> edge, Domain<E, V> newstate, Domain<E, V> prevstate, out bool weaker, bool widen)
		{
			bool nonNullWeaker;
			SetDomain<V> nonNulls = prevstate.NonNulls.Join (newstate.NonNulls, widen, out nonNullWeaker);
			bool nullWeaker;
			SetDomain<V> nulls = prevstate.Nulls.Join (newstate.Nulls, widen, out nullWeaker);

			weaker = nonNullWeaker || nullWeaker;
			return new Domain<E, V> (nonNulls, nulls);
		}

		public Domain<E, V> ImmutableVersion (Domain<E, V> state)
		{
			return state;
		}

		public Domain<E, V> MutableVersion (Domain<E, V> state)
		{
			return state;
		}

		public Domain<E, V> EdgeConversion (APC from, APC to, bool isJoinPoint, IImmutableMap<V, LispList<V>> data, Domain<E, V> state)
		{
			if (data == null)
				return state;
			SetDomain<V> oldNonNulls = state.NonNulls;
			SetDomain<V> nonNulls = SetDomain<V>.TopValue;

			SetDomain<V> oldNulls = state.Nulls;
			SetDomain<V> nulls = SetDomain<V>.TopValue;
			foreach (V variable in data.Keys) {
				bool nonNullContains = oldNonNulls.Contains (variable);
				bool nullContains = oldNulls.Contains (variable);

				if (nonNullContains || nullContains) {
					foreach (V anotherVariable in data [variable].AsEnumerable ()) {
						if (nonNullContains)
							nonNulls = nonNulls.Add (anotherVariable);
						if (nullContains)
							nulls = nulls.Add (anotherVariable);
					}
				}
			}

			return new Domain<E, V> (nonNulls, nulls);
		}

		public bool IsBottom (APC pc, Domain<E, V> state)
		{
			return state.NonNulls.IsBottom;
		}

		public Predicate<APC> SaveFixPointInfo (IFixPointInfo<APC, Domain<E, V>> fixPointInfo)
		{
			this.fix_point_info = fixPointInfo;

			//todo: implement this
			return pc => true;
		}

		public void Dump (Pair<Domain<E, V>, TextWriter> pair)
		{
			TextWriter tw = pair.Value;
			tw.Write ("NonNulls: ");
			pair.Key.NonNulls.Dump (tw);
			tw.Write ("Nulls: ");
			pair.Key.Nulls.Dump (tw);
		}
		#endregion

		#region IFactBase<V> Members
		public ProofOutcome IsNull (APC pc, V variable)
		{
			if (ContextProvider.ValueContext.IsZero (pc, variable))
				return ProofOutcome.True;

			Domain<E, V> domain;
			if (!PreStateLookup (pc, out domain) || domain.NonNulls.IsBottom)
				return ProofOutcome.Bottom;
			if (domain.IsNonNull (variable))
				return ProofOutcome.False;
			if (domain.IsNull (variable))
				return ProofOutcome.True;

			return ProofOutcome.Top;
		}

		public ProofOutcome IsNonNull (APC pc, V variable)
		{
			Domain<E, V> domain;
			if (!PreStateLookup (pc, out domain) || domain.NonNulls.IsBottom)
				return ProofOutcome.Bottom;
			if (domain.IsNonNull (variable))
				return ProofOutcome.True;
			if (ContextProvider.ValueContext.IsZero (pc, variable) || domain.IsNull (variable))
				return ProofOutcome.False;

			FlatDomain<TypeNode> aType = ContextProvider.ValueContext.GetType (pc, variable);
			if (aType.IsNormal && MetaDataProvider.IsManagedPointer (aType.Concrete))
				return ProofOutcome.True;

			return ProofOutcome.Top;
		}

		public bool IsUnreachable (APC pc)
		{
			Domain<E, V> domain;
			if (!PreStateLookup (pc, out domain) || domain.NonNulls.IsBottom)
				return true;

			return false;
		}
		#endregion

		public override Domain<E, V> DefaultVisit (APC pc, Domain<E, V> data)
		{
			return data;
		}

		public static Domain<E, V> AssumeNonNull (V dest, Domain<E, V> before)
		{
			if (!before.NonNulls.Contains (dest))
				return new Domain<E, V> (before.NonNulls.Add (dest), before.Nulls);
			return before;
		}

		public static Domain<E, V> AssumeNull (V dest, Domain<E, V> before)
		{
			if (!before.Nulls.Contains (dest))
				return new Domain<E, V> (before.NonNulls, before.Nulls.Add (dest));
			return before;
		}

		public override Domain<E, V> Assert (APC pc, EdgeTag tag, V condition, Domain<E, V> data)
		{
			return ContextProvider.ExpressionContext.
				Decode<Pair<bool, Domain<E, V>>, Domain<E, V>, ExpressionAssumeDecoder<E, V>>
				(
				 ContextProvider.ExpressionContext.Refine (pc, condition),
				 new ExpressionAssumeDecoder<E, V> (ContextProvider),
				 new Pair<bool, Domain<E, V>> (true, data));
		}

		public override Domain<E, V> Assume (APC pc, EdgeTag tag, V condition, Domain<E, V> data)
		{
			IExpressionContext<E, V> exprCtx = ContextProvider.ExpressionContext;
			E expr = exprCtx.Refine (pc, condition);

			return exprCtx.Decode<Pair<bool, Domain<E, V>>, Domain<E, V>, ExpressionAssumeDecoder<E, V>>
				(expr, new ExpressionAssumeDecoder<E, V> (ContextProvider),
				 new Pair<bool, Domain<E, V>> (tag != EdgeTag.False, data));
		}

		public override Domain<E, V> Unary (APC pc, UnaryOperator op, bool unsigned, V dest, V source, Domain<E, V> data)
		{
			switch (op) {
			case UnaryOperator.Conv_i:
			case UnaryOperator.Conv_u:
				if (data.IsNonNull (source))
					return AssumeNonNull (dest, data);
				break;
			}
			return data;
		}

		public override Domain<E, V> Call<TypeList, ArgList> (APC pc, Method method, bool virt, TypeList extraVarargs, V dest, ArgList args, Domain<E, V> data)
		{
			this.callSiteCache [pc] = data;
			if (!MetaDataProvider.IsStatic (method))
				return AssumeNonNull (args [0], data);

			return data;
		}

		public override Domain<E, V> CastClass (APC pc, TypeNode type, V dest, V obj, Domain<E, V> data)
		{
			if (data.NonNulls.Contains (obj))
				return AssumeNonNull (dest, data);

			return data;
		}

		public override Domain<E, V> Entry (APC pc, Method method, Domain<E, V> data)
		{
			APC at = ContextProvider.MethodContext.CFG.Next (pc);
			Domain<E, V> domain = data;
			IIndexable<Parameter> parameters = MetaDataProvider.Parameters (method);
			TypeNode eventArgsType;
			bool systemType = MetaDataProvider.TryGetSystemType ("System.EventArgs", out eventArgsType);
			for (int i = 0; i < parameters.Count; i++) {
				Parameter p = parameters [i];
				TypeNode pType = MetaDataProvider.ParameterType (p);
				if (MetaDataProvider.IsManagedPointer (pType)) {
					V sv;
					if (ContextProvider.ValueContext.TryParameterValue (at, p, out sv))
						domain = AssumeNonNull (sv, domain);
				} else {
					V sv;
					if (i == 0 && parameters.Count == 1 && MetaDataProvider.IsArray (pType)
					    && MetaDataProvider.Name (method) == "Main" && MetaDataProvider.IsStatic (method) &&
					    ContextProvider.ValueContext.TryParameterValue (pc, p, out sv))
						domain = AssumeNonNull (sv, domain);
				}
			}
			V sv1;
			if (systemType && parameters.Count == 2 && MetaDataProvider.Equal (MetaDataProvider.System_Object, MetaDataProvider.ParameterType (parameters [0])) &&
			    MetaDataProvider.DerivesFrom (MetaDataProvider.ParameterType (parameters [1]), eventArgsType)
			    && ContextProvider.ValueContext.TryParameterValue (pc, parameters [1], out sv1))
				domain = AssumeNonNull (sv1, domain);
			if (!MetaDataProvider.IsStatic (method) && ContextProvider.ValueContext.TryParameterValue (pc, MetaDataProvider.This (method), out sv1))
				domain = AssumeNonNull (sv1, domain);

			return domain;
		}

		public override Domain<E, V> LoadStack (APC pc, int offset, V dest, V source, bool isOld, Domain<E, V> data)
		{
			Domain<E, V> old;
			if (isOld && TryFindOldState (pc, out old)) {
				if (old.IsNonNull (source))
					return AssumeNonNull (dest, data);
				if (old.IsNull (source))
					return AssumeNull (dest, data);
			}

			return data;
		}

		public override Domain<E, V> Isinst (APC pc, TypeNode type, V dest, V obj, Domain<E, V> data)
		{
			if (data.IsNonNull (obj)) {
				FlatDomain<TypeNode> aType = ContextProvider.ValueContext.GetType (pc, obj);
				if (aType.IsNormal && MetaDataProvider.DerivesFrom (aType.Concrete, type))
					return AssumeNonNull (dest, data);
			}
			return data;
		}

		public override Domain<E, V> LoadArgAddress (APC pc, Parameter argument, bool isOld, V dest, Domain<E, V> data)
		{
			return AssumeNonNull (dest, data);
		}

		public override Domain<E, V> LoadConst (APC pc, TypeNode type, object constant, V dest, Domain<E, V> data)
		{
			if (constant is string)
				return AssumeNonNull (dest, data);

			return data;
		}

		public override Domain<E, V> LoadElement (APC pc, TypeNode type, V dest, V array, V index, Domain<E, V> data)
		{
			return AssumeNonNull (array, data);
		}

		public override Domain<E, V> LoadField (APC pc, Field field, V dest, V obj, Domain<E, V> data)
		{
			Domain<E, V> domain = AssumeNonNull (obj, data);
			FlatDomain<TypeNode> aType = ContextProvider.ValueContext.GetType (ContextProvider.MethodContext.CFG.Next (pc), dest);
			if (aType.IsNormal && MetaDataProvider.IsManagedPointer (aType.Concrete))
				domain = AssumeNonNull (dest, domain);

			return domain;
		}

		public override Domain<E, V> LoadFieldAddress (APC pc, Field field, V dest, V obj, Domain<E, V> data)
		{
			Domain<E, V> domain = AssumeNonNull (obj, data);
			return AssumeNonNull (dest, domain);
		}

		public override Domain<E, V> LoadStaticFieldAddress (APC pc, Field field, V dest, Domain<E, V> data)
		{
			return AssumeNonNull (dest, data);
		}

		public override Domain<E, V> LoadLength (APC pc, V dest, V array, Domain<E, V> data)
		{
			return AssumeNonNull (array, data);
		}

		public override Domain<E, V> NewArray<ArgList> (APC pc, TypeNode type, V dest, ArgList lengths, Domain<E, V> data)
		{
			return AssumeNonNull (dest, data);
		}

		public override Domain<E, V> NewObj<ArgList> (APC pc, Method ctor, V dest, ArgList args, Domain<E, V> data)
		{
			return AssumeNonNull (dest, data);
		}

		public override Domain<E, V> StoreElement (APC pc, TypeNode type, V array, V index, V value, Domain<E, V> data)
		{
			return AssumeNonNull (array, data);
		}

		public override Domain<E, V> StoreField (APC pc, Field field, V obj, V value, Domain<E, V> data)
		{
			return AssumeNonNull (obj, data);
		}

		private bool TryFindOldState (APC pc, out Domain<E, V> old)
		{
			for (LispList<Edge<CFGBlock, EdgeTag>> flist = pc.SubroutineContext; flist != null; flist = flist.Tail) {
				Edge<CFGBlock, EdgeTag> head = flist.Head;
				if (head.Tag.Is (EdgeTag.AfterMask))
					return this.callSiteCache.TryGetValue (pc, out old);
			}
			old = new Domain<E, V> ();
			return false;
		}

		public Domain<E, V> InitialValue (Func<V, int> keyConverter)
		{
			return new Domain<E, V> (new SetDomain<V> (keyConverter), new SetDomain<V> (keyConverter));
		}

		#region Implementation of IMethodResult<Variable>
		public IMethodAnalysis MethodAnalysis { get; set; }

		public void ValidateImplicitAssertions (IFactQuery<BoxedExpression, V> facts, List<string> proofResults)
		{
		}

		public IFactQuery<BoxedExpression, V> FactQuery
		{
			get { return new SimpleLogicInference<E, V> (ContextProvider, this, this.method_driver.BasicFacts.IsUnreachable); }
		}

		public ProofOutcome ValidateExplicitAssertion (APC pc, V value)
		{
			Domain<E, V> domain;
			if (PreStateLookup (pc, out domain) && !domain.NonNulls.IsBottom) {
				IExpressionContext<E, V> exprCtx = ContextProvider.ExpressionContext;
				return exprCtx.Decode<bool, ProofOutcome, ExpressionAssertDischarger<E, V>> (exprCtx.Refine (pc, value), new ExpressionAssertDischarger<E, V> (this, pc), true);
			}
			return ProofOutcome.Bottom;
		}

		private bool PreStateLookup (APC pc, out Domain<E, V> domain)
		{
			return this.fix_point_info.PreStateLookup (pc, out domain);
		}
		#endregion
		}
}
