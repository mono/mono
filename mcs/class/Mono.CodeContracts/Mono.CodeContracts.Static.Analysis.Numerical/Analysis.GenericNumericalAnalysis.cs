using System;

using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis.Drivers;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataFlowAnalysis;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Lattices;
using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        static partial class AnalysisFacade {
                static partial class Bind<TVar, TExpr> where TExpr : IEquatable<TExpr> where TVar : IEquatable<TVar> {
                        class GenericNumericalAnalysis :
                                GenericValueAnalysis<INumericalEnvironmentDomain<BoxedVariable<TVar>, BoxedExpression>> {
                                readonly Analysers.ArithmeticEnvironmentKind env_kind;

                                public GenericNumericalAnalysis (string methodName,
                                                                 IMethodDriver<TExpr, TVar> methodDriver,
                                                                 Analysers.ArithmeticEnvironmentKind envKind)
                                        : base (methodName, methodDriver)
                                {
                                        env_kind = envKind;
                                }

                                public override INumericalEnvironmentDomain<BoxedVariable<TVar>, BoxedExpression>
                                        TopValue ()
                                {
                                        switch (env_kind) {
                                        case Analysers.ArithmeticEnvironmentKind.Intervals:
                                                return
                                                        new IntervalEnvironment<BoxedVariable<TVar>, BoxedExpression> (
                                                                ExpressionDecoder);
                                        case Analysers.ArithmeticEnvironmentKind.DisIntervals:
                                                return
                                                        new DisIntervalEnvironment<BoxedVariable<TVar>, BoxedExpression>
                                                                (ExpressionDecoder);
                                        default:
                                                throw new AbstractInterpretationException (
                                                        "Unknown arithmetic environment kind.");
                                        }
                                }

                                public override IFactQuery<BoxedExpression, TVar> FactQuery
                                        (IFixPointInfo
                                                 <APC, INumericalEnvironmentDomain<BoxedVariable<TVar>, BoxedExpression>
                                                 > fixpoint)
                                {
                                        return new ConstantPropagationFactQuery<TVar> ();
                                }

                                public override INumericalEnvironmentDomain<BoxedVariable<TVar>, BoxedExpression> Entry
                                        (APC pc, Method method,
                                         INumericalEnvironmentDomain<BoxedVariable<TVar>, BoxedExpression> data)
                                {
                                        foreach (var param in MetaDataProvider.Parameters (method).AsEnumerable ()) {
                                                TVar variable;
                                                var readAt = ContextProvider.MethodContext.CFG.Post (pc);
                                                if (!ContextProvider.ValueContext.TryParameterValue (readAt, param, out variable))
                                                        continue;

                                                var abstractType = ContextProvider.ValueContext.GetType (readAt,
                                                                                                         variable);
                                                if (abstractType.IsNormal () && MetaDataProvider.IsPrimitive (abstractType.Value))
                                                        data = SetInitialRange (variable, abstractType.Value, data);
                                        }

                                        return data;
                                }

                                INumericalEnvironmentDomain<BoxedVariable<TVar>, BoxedExpression> SetInitialRange
                                        (TVar variable, TypeNode type,
                                         INumericalEnvironmentDomain<BoxedVariable<TVar>, BoxedExpression> data)
                                {
                                        var interval = Interval.Ranges.GetIntervalForType (type,
                                                                                           MetaDataProvider);
                                        if (interval.IsNormal ())
                                                data = data.AssumeVariableIn (new BoxedVariable<TVar> (variable),
                                                                              interval);
                                        return data;
                                }
                        }
                }
        }
}