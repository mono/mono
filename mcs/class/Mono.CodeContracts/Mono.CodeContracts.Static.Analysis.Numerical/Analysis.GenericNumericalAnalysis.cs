// 
// Analysis.GenericNumericalAnalysis.cs
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