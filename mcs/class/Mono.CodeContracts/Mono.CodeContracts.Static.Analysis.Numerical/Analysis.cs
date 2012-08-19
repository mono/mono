using System;

using Mono.CodeContracts.Static.Analysis.Drivers;
using Mono.CodeContracts.Static.DataFlowAnalysis;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        static partial class AnalysisFacade {
                public static IMethodResult<TVar> RunArithmeticAnalysis<TVar, TExpr> (string methodName,
                                                                                      IMethodDriver<TExpr, TVar>
                                                                                              methodDriver)
                        where TVar : IEquatable<TVar>
                        where TExpr : IEquatable<TExpr>
                {
                        return Bind<TVar, TExpr>.RunArithmeticAnalysis (methodName, methodDriver);
                }

                static partial class Bind<TVar, TExpr> where TExpr : IEquatable<TExpr> where TVar : IEquatable<TVar> {
                        public static IMethodResult<TVar> RunArithmeticAnalysis (string methodName,
                                                                                 IMethodDriver<TExpr, TVar> methodDriver)
                        {
                                var analysis = new GenericNumericalAnalysis (methodName, methodDriver,
                                                                             Analysers.ArithmeticEnvironmentKind.
                                                                                     DisIntervals);
                                return RunAnalysis (methodName, methodDriver, analysis);
                        }

                        public static IMethodResult<TVar> RunAnalysis<TDomain> (string methodName,
                                                                                IMethodDriver<TExpr, TVar> methodDriver,
                                                                                IAbstractAnalysis<TDomain, TVar>
                                                                                        analysis)
                        {
                                methodDriver.HybridLayer.CreateForward (analysis).Invoke (analysis.TopValue ());

                                return analysis as IMethodResult<TVar>;
                        }
                }
        }
}