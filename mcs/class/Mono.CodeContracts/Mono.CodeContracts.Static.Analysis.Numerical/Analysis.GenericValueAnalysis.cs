// 
// Analysis.GenericValueAnalysis.cs
// 
// Authors:
//	Alexander Chebaturkin (chebaturkin@gmail.com)
// 
// Copyright (C) 2012 Alexander Chebaturkin
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

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        static partial class AnalysisFacade {
                static partial class Bind<TVar, TExpr> where TExpr : IEquatable<TExpr> where TVar : IEquatable<TVar> {
                        abstract class GenericValueAnalysis<TDomain> :
                                ILVisitorBase<APC, TVar, TVar, TDomain, TDomain>,
                                IAbstractAnalysis<TDomain, TVar>,
                                IMethodResult<TVar>
                                where TDomain : IEnvironmentDomain<TDomain, BoxedVariable<TVar>, BoxedExpression> {
                                readonly IMethodDriver<TExpr, TVar> method_driver;
                                //readonly string method_name;

                                protected ConstantEvaluator EvaluatorOfConstants;

                                protected IFixPointInfo<APC, TDomain> FixPointInfo;
                                BoxedExpressionDecoder<TVar, TExpr> expression_decoder;

                                protected GenericValueAnalysis (string methodName,
                                                                IMethodDriver<TExpr, TVar> methodDriver)
                                {
                                        ThresholdDB.Reset ();
                                        BoxedVariable<TVar>.ResetFreshVariableCounter ();

                                //        method_name = methodName;
                                        method_driver = methodDriver;

                                        EvaluatorOfConstants = new ConstantEvaluator (ContextProvider,
                                                                                      MetaDataProvider);
                                }

                                protected IExpressionContextProvider<TExpr, TVar> ContextProvider { get { return method_driver.ContextProvider; } }
                                protected IMetaDataProvider MetaDataProvider { get { return method_driver.MetaDataProvider; } }

                                protected BoxedExpressionDecoder<TVar, TExpr> ExpressionDecoder
                                {
                                        get
                                        {
                                                if (expression_decoder == null)
                                                        expression_decoder =
                                                                new BoxedExpressionDecoder<TVar, TExpr> (
                                                                        new ValueExpressionDecoder<TVar, TExpr> (
                                                                                MetaDataProvider,
                                                                                ContextProvider));
                                                return expression_decoder;
                                        }
                                }

                                public IILVisitor<APC, TVar, TVar, TDomain, TDomain> GetVisitor ()
                                {
                                        return this;
                                }

                                public TDomain Join (Pair<APC, APC> edge, TDomain newstate, TDomain prevstate,
                                                     out bool weaker, bool widen)
                                {
                                        TDomain result;
                                        if (!widen) {
                                                result = Join (newstate, prevstate, edge);
                                                weaker = true;
                                        }
                                        else {
                                                result = Widen (newstate, prevstate, edge);
                                                weaker = !result.LessEqual (prevstate);
                                        }

                                        return result;
                                }

                                public TDomain ImmutableVersion (TDomain arg)
                                {
                                        return arg;
                                }

                                public TDomain MutableVersion (TDomain arg)
                                {
                                        return arg.Clone ();
                                }

                                public virtual TDomain EdgeConversion (APC @from, APC to, bool isJoinPoint,
                                                                       IImmutableMap<TVar, Sequence<TVar>>
                                                                               sourceTargetMap, TDomain state)
                                {
                                        return state;
                                }

                                public bool IsBottom (APC pc, TDomain state)
                                {
                                        return state.IsBottom;
                                }

                                public Predicate<APC> SaveFixPointInfo (IFixPointInfo<APC, TDomain> fixPointInfo)
                                {
                                        FixPointInfo = fixPointInfo;
                                        return a => false;
                                }

                                public void Dump (Pair<TDomain, TextWriter> pair)
                                {
                                        pair.Value.WriteLine (pair.Key.ToString ());
                                }

                                public abstract TDomain TopValue ();

                                public TDomain BottomValue ()
                                {
                                        return TopValue ().Bottom;
                                }

                                public abstract IFactQuery<BoxedExpression, TVar> FactQuery (
                                        IFixPointInfo<APC, TDomain> fixpoint);

                                IFactQuery<BoxedExpression, TVar> IMethodAnalysisFixPoint<TVar>.FactQuery { get { return FactQuery (FixPointInfo); } }

                                public FlatDomain<bool> ValidateExplicitAssertion (APC pc, TVar value)
                                {
                                        return FlatDomain<bool>.TopValue;
                                }

                                public IMethodAnalysis MethodAnalysis { get; set; }

                                public void ValidateImplicitAssertions (IFactQuery<BoxedExpression, TVar> facts,
                                                                        List<string> proofResults)
                                {
                                }

                                protected virtual TDomain Widen (TDomain newState, TDomain prevState,
                                                                 Pair<APC, APC> edge)
                                {
                                        return newState.Widen (prevState);
                                }

                                protected virtual TDomain Join (TDomain newState, TDomain prevState, Pair<APC, APC> edge)
                                {
                                        return newState.Join (prevState);
                                }

                                public override TDomain Assume (APC pc, EdgeTag tag, TVar condition, TDomain data)
                                {
                                        var boxed = ToBoxedExpression (pc, condition);
                                        if (tag != EdgeTag.False) {
                                                bool value;
                                                if (boxed.IsTrivialCondition (out value))
                                                        return !value ? data.Bottom : data;
                                        }

                                        List<int> thresholds;
                                        if (ThresholdDB.TryGetAThreshold (boxed, expression_decoder, out thresholds))
                                                ThresholdDB.Add (thresholds);

                                        TDomain result;
                                        switch (tag) {
                                        case EdgeTag.True:
                                        case EdgeTag.Requires:
                                        case EdgeTag.Assume:
                                        case EdgeTag.Invariant:
                                                result = data.AssumeTrue (boxed);
                                                break;
                                        case EdgeTag.False:
                                                result = data.AssumeFalse (boxed);
                                                break;
                                        default:
                                                result = data;
                                                break;
                                        }

                                        if (tag != EdgeTag.False) {
                                                var abstractType =
                                                        ContextProvider.ValueContext.GetType (
                                                                ContextProvider.MethodContext.CFG.Post (pc), condition);
                                                if (abstractType.IsNormal () &&
                                                    MetaDataProvider.Equal (abstractType.Value,
                                                                            MetaDataProvider.System_Boolean)) {
                                                        var guard =
                                                                BoxedExpression.Binary (BinaryOperator.Ceq, boxed,
                                                                                        BoxedExpression.Const (1,
                                                                                                               MetaDataProvider
                                                                                                                       .
                                                                                                                       System_Int32));

                                                        result = result.AssumeTrue (guard);
                                                }
                                        }

                                        return result;
                                }

                                public override TDomain Assert (APC pc, EdgeTag tag, TVar condition, TDomain data)
                                {
                                        var boxed = ToBoxedExpression (pc, condition);

                                        bool result;
                                        if (boxed.IsTrivialCondition (out result))
                                                return result ? data : data.Bottom;

                                        data = data.AssumeTrue (boxed);

                                        var type =
                                                ContextProvider.ValueContext.GetType (
                                                        ContextProvider.MethodContext.CFG.Post (pc), condition);
                                        if (type.IsNormal () &&
                                            MetaDataProvider.Equal (type.Value, MetaDataProvider.System_Boolean)) {
                                                var guard =
                                                        BoxedExpression.Binary (BinaryOperator.Ceq, boxed,
                                                                                BoxedExpression.Const (1,
                                                                                                       MetaDataProvider.
                                                                                                               System_Int32));

                                                data = data.AssumeTrue (guard);
                                        }

                                        return data;
                                }

                                public override TDomain DefaultVisit (APC pc, TDomain data)
                                {
                                        return data;
                                }

                                protected BoxedExpression ToBoxedExpression (APC pc, TVar condition)
                                {
                                        return
                                                BoxedExpression.For (
                                                        ContextProvider.ExpressionContext.Refine (pc, condition),
                                                        ExpressionDecoder.ExternalDecoder);
                                }

                                public IFactQuery<BoxedExpression, TVar> FactQuery ()
                                {
                                        return FactQuery (FixPointInfo);
                                }
                        }
                }
        }
}