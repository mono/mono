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
using System.Linq;

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
	class Analysis<E, V> : ILVisitorBase<APC, V, V, NonNullDomain<V>, NonNullDomain<V>>,
		                   IAnalysis<APC, NonNullDomain<V>, IILVisitor<APC, V, V, NonNullDomain<V>, NonNullDomain<V>>, IImmutableMap<V, Sequence<V>>>,
		                   IMethodResult<V>, IFactBase<V> 
            where E : IEquatable<E> 
            where V : IEquatable<V> 
    {
		private readonly Dictionary<APC, NonNullDomain<V>> callSiteCache = new Dictionary<APC, NonNullDomain<V>> ();
		private readonly IMethodDriver<E, V> method_driver;
		private IFixPointInfo<APC, NonNullDomain<V>> fix_point_info;

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

		#region IAnalysis<APC,Domain<V>,IILVisitor<APC,V,V,Domain<V>,Domain<V>>,IImmutableMap<V,Sequence<V>>> Members
		public IILVisitor<APC, V, V, NonNullDomain<V>, NonNullDomain<V>> GetVisitor ()
		{
			return this;
		}

		public NonNullDomain<V> Join (Pair<APC, APC> edge, NonNullDomain<V> newstate, NonNullDomain<V> prevstate, out bool weaker, bool widen)
		{
			bool nonNullWeaker;
			SetDomain<V> nonNulls = prevstate.NonNulls.Join (newstate.NonNulls, widen, out nonNullWeaker);
			bool nullWeaker;
			SetDomain<V> nulls = prevstate.Nulls.Join (newstate.Nulls, widen, out nullWeaker);

			weaker = nonNullWeaker || nullWeaker;
			return new NonNullDomain<V> (nonNulls, nulls);
		}

		public NonNullDomain<V> ImmutableVersion (NonNullDomain<V> state)
		{
			return state;
		}

		public NonNullDomain<V> MutableVersion (NonNullDomain<V> state)
		{
			return state;
		}

		public NonNullDomain<V> EdgeConversion (APC from, APC to, bool isJoinPoint, IImmutableMap<V, Sequence<V>> data, NonNullDomain<V> state)
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
							nonNulls = nonNulls.With (anotherVariable);
						if (nullContains)
							nulls = nulls.With (anotherVariable);
					}
				}
			}

			return new NonNullDomain<V> (nonNulls, nulls);
		}

		public bool IsBottom (APC pc, NonNullDomain<V> state)
		{
			return state.NonNulls.IsBottom;
		}

		public Predicate<APC> SaveFixPointInfo (IFixPointInfo<APC, NonNullDomain<V>> fixPointInfo)
		{
			this.fix_point_info = fixPointInfo;

			//todo: implement this
			return pc => true;
		}

		public void Dump (Pair<NonNullDomain<V>, TextWriter> pair)
		{
			TextWriter tw = pair.Value;
			tw.Write ("NonNulls: ");
			pair.Key.NonNulls.Dump (tw);
			tw.Write ("Nulls: ");
			pair.Key.Nulls.Dump (tw);
		}
		#endregion

		#region IFactBase<V> Members
        public FlatDomain<bool> IsNull(APC pc, V variable)
		{
			if (ContextProvider.ValueContext.IsZero (pc, variable))
				return ProofOutcome.True;

			NonNullDomain<V> domain;
			if (!PreStateLookup (pc, out domain) || domain.NonNulls.IsBottom)
				return ProofOutcome.Bottom;
			if (domain.IsNonNull (variable))
				return ProofOutcome.False;
			if (domain.IsNull (variable))
				return ProofOutcome.True;

			return ProofOutcome.Top;
		}

        public FlatDomain<bool> IsNonNull(APC pc, V variable)
		{
			NonNullDomain<V> domain;
			if (!PreStateLookup (pc, out domain) || domain.NonNulls.IsBottom)
				return ProofOutcome.Bottom;
			if (domain.IsNonNull (variable))
				return ProofOutcome.True;
			if (ContextProvider.ValueContext.IsZero (pc, variable) || domain.IsNull (variable))
				return ProofOutcome.False;

			FlatDomain<TypeNode> aType = ContextProvider.ValueContext.GetType (pc, variable);
			if (aType.IsNormal() && MetaDataProvider.IsManagedPointer (aType.Value))
				return ProofOutcome.True;

			return ProofOutcome.Top;
		}

		public bool IsUnreachable (APC pc)
		{
			NonNullDomain<V> domain;
			if (!PreStateLookup (pc, out domain) || domain.NonNulls.IsBottom)
				return true;

			return false;
		}
		#endregion

		public override NonNullDomain<V> DefaultVisit (APC pc, NonNullDomain<V> data)
		{
			return data;
		}

		public static NonNullDomain<V> AssumeNonNull (V dest, NonNullDomain<V> domain)
		{
			if (!domain.NonNulls.Contains (dest))
				return new NonNullDomain<V> (domain.NonNulls.With (dest), domain.Nulls);

			return domain;
		}

		public static NonNullDomain<V> AssumeNull (V dest, NonNullDomain<V> before)
		{
			if (!before.Nulls.Contains (dest))
				return new NonNullDomain<V> (before.NonNulls, before.Nulls.With (dest));

			return before;
		}

        public override NonNullDomain<V> Assert(APC pc, EdgeTag tag, V condition, NonNullDomain<V> data)
        {
            return ContextProvider.ExpressionContext.Decode
                <Pair<bool, NonNullDomain<V>>, NonNullDomain<V>, ExpressionAssumeDecoder<E, V>>
                (
                    ContextProvider.ExpressionContext.Refine (pc, condition),
                    new ExpressionAssumeDecoder<E, V> (ContextProvider),
                    new Pair<bool, NonNullDomain<V>> (true, data));
        }

	    public override NonNullDomain<V> Assume (APC pc, EdgeTag tag, V condition, NonNullDomain<V> data)
		{
			IExpressionContext<E, V> exprCtx = ContextProvider.ExpressionContext;
			E expr = exprCtx.Refine (pc, condition);

			return exprCtx.Decode<Pair<bool, NonNullDomain<V>>, NonNullDomain<V>, ExpressionAssumeDecoder<E, V>>
				(expr, new ExpressionAssumeDecoder<E, V> (ContextProvider),
				 new Pair<bool, NonNullDomain<V>> (tag != EdgeTag.False, data));
		}

		public override NonNullDomain<V> Unary (APC pc, UnaryOperator op, bool unsigned, V dest, V source, NonNullDomain<V> data)
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

		public override NonNullDomain<V> Call<TypeList, ArgList> (APC pc, Method method, bool virt, TypeList extraVarargs, V dest, ArgList args, NonNullDomain<V> data)
		{
			this.callSiteCache [pc] = data;
			if (!MetaDataProvider.IsStatic (method))
				return AssumeNonNull (args [0], data);

			return data;
		}

		public override NonNullDomain<V> CastClass (APC pc, TypeNode type, V dest, V obj, NonNullDomain<V> data)
		{
			if (data.NonNulls.Contains (obj))
				return AssumeNonNull (dest, data);

			return data;
		}

		public override NonNullDomain<V> Entry (APC pc, Method method, NonNullDomain<V> data)
		{
			APC at = ContextProvider.MethodContext.CFG.Next (pc);
			NonNullDomain<V> domain = data;
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

		public override NonNullDomain<V> LoadStack (APC pc, int offset, V dest, V source, bool isOld, NonNullDomain<V> data)
		{
			NonNullDomain<V> old;
			if (isOld && TryFindOldState (pc, out old)) {
				if (old.IsNonNull (source))
					return AssumeNonNull (dest, data);
				if (old.IsNull (source))
					return AssumeNull (dest, data);
			}

			return data;
		}

		public override NonNullDomain<V> Isinst (APC pc, TypeNode type, V dest, V obj, NonNullDomain<V> data)
		{
			if (data.IsNonNull (obj)) {
				FlatDomain<TypeNode> aType = ContextProvider.ValueContext.GetType (pc, obj);
				if (aType.IsNormal() && MetaDataProvider.DerivesFrom (aType.Value, type))
					return AssumeNonNull (dest, data);
			}
			return data;
		}

		public override NonNullDomain<V> LoadArgAddress (APC pc, Parameter argument, bool isOld, V dest, NonNullDomain<V> data)
		{
			return AssumeNonNull (dest, data);
		}

		public override NonNullDomain<V> LoadConst (APC pc, TypeNode type, object constant, V dest, NonNullDomain<V> data)
		{
			if (constant is string)
				return AssumeNonNull (dest, data);

			return data;
		}

		public override NonNullDomain<V> LoadElement (APC pc, TypeNode type, V dest, V array, V index, NonNullDomain<V> data)
		{
			return AssumeNonNull (array, data);
		}

		public override NonNullDomain<V> LoadField (APC pc, Field field, V dest, V obj, NonNullDomain<V> data)
		{
			NonNullDomain<V> domain = AssumeNonNull (obj, data);
			FlatDomain<TypeNode> aType = ContextProvider.ValueContext.GetType (ContextProvider.MethodContext.CFG.Next (pc), dest);
			if (aType.IsNormal() && MetaDataProvider.IsManagedPointer (aType.Value))
				domain = AssumeNonNull (dest, domain);

			return domain;
		}

		public override NonNullDomain<V> LoadFieldAddress (APC pc, Field field, V dest, V obj, NonNullDomain<V> data)
		{
			NonNullDomain<V> domain = AssumeNonNull (obj, data);
			return AssumeNonNull (dest, domain);
		}

		public override NonNullDomain<V> LoadStaticFieldAddress (APC pc, Field field, V dest, NonNullDomain<V> data)
		{
			return AssumeNonNull (dest, data);
		}

		public override NonNullDomain<V> LoadLength (APC pc, V dest, V array, NonNullDomain<V> data)
		{
			return AssumeNonNull (array, data);
		}

		public override NonNullDomain<V> NewArray<ArgList> (APC pc, TypeNode type, V dest, ArgList lengths, NonNullDomain<V> data)
		{
			return AssumeNonNull (dest, data);
		}

		public override NonNullDomain<V> NewObj<ArgList> (APC pc, Method ctor, V dest, ArgList args, NonNullDomain<V> data)
		{
			return AssumeNonNull (dest, data);
		}

		public override NonNullDomain<V> StoreElement (APC pc, TypeNode type, V array, V index, V value, NonNullDomain<V> data)
		{
			return AssumeNonNull (array, data);
		}

		public override NonNullDomain<V> StoreField (APC pc, Field field, V obj, V value, NonNullDomain<V> data)
		{
			return AssumeNonNull (obj, data);
		}

		private bool TryFindOldState (APC pc, out NonNullDomain<V> old)
		{
		        if (pc.SubroutineContext.AsEnumerable().Any (edge => edge.Tag.Is (EdgeTag.AfterMask))) 
		                return this.callSiteCache.TryGetValue (pc, out old);

		        return false.Without (out old);
		}

	        public NonNullDomain<V> InitialValue (Func<V, int> keyConverter)
		{
			return new NonNullDomain<V> (new SetDomain<V> (keyConverter), new SetDomain<V> (keyConverter));
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

        public FlatDomain<bool> ValidateExplicitAssertion(APC pc, V value)
		{
			NonNullDomain<V> domain;
			if (PreStateLookup (pc, out domain) && !domain.NonNulls.IsBottom) {
				IExpressionContext<E, V> exprCtx = ContextProvider.ExpressionContext;
                return exprCtx.Decode<bool, FlatDomain<bool>, ExpressionAssertDischarger<E, V>>(exprCtx.Refine(pc, value), new ExpressionAssertDischarger<E, V>(this, pc), true);
			}
			return ProofOutcome.Bottom;
		}

		private bool PreStateLookup (APC pc, out NonNullDomain<V> domain)
		{
			return this.fix_point_info.PreStateLookup (pc, out domain);
		}
		#endregion
		}
}
