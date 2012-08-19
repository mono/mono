using System;

using Mono.CodeContracts.Static.Analysis.Drivers;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        static class Analysers {
                public enum ArithmeticEnvironmentKind {
                        Intervals,
                        DisIntervals
                }

                public class Arithmetic : IMethodAnalysis {
                        public string Name { get { return "Arithmetic"; } }

                        public IMethodResult<TVar> Analyze<TExpr, TVar> (string fullMethodName,
                                                                         IMethodDriver<TExpr, TVar> methodDriver)
                                where TVar : IEquatable<TVar>
                                where TExpr : IEquatable<TExpr>
                        {
                                return AnalysisFacade.RunArithmeticAnalysis (fullMethodName, methodDriver);
                        }
                }
        }
}