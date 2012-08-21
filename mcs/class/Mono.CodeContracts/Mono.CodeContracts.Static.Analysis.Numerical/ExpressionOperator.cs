// 
// ExpressionOperator.cs
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

using System.Collections.Generic;

namespace Mono.CodeContracts.Static.Analysis.Numerical {
        public enum ExpressionOperator {
                Constant,
                Variable,
                Not,
                And,
                Or,
                Xor,
                LogicalAnd,
                LogicalOr,
                LogicalNot,
                Equal,
                Equal_Obj,
                NotEqual,
                LessThan,
                LessEqualThan,
                GreaterThan,
                GreaterEqualThan,
                Add,
                Sub,
                Mult,
                Div,
                Mod,
                UnaryMinus,
                SizeOf,
                Unknown,

                ConvertToInt32
        }

        static class ExpressionOperatorExtensions {
                static readonly HashSet<ExpressionOperator> relationalOperators = new HashSet<ExpressionOperator> ();

                static ExpressionOperatorExtensions ()
                {
                        relationalOperators.Add (ExpressionOperator.LessThan);
                        relationalOperators.Add (ExpressionOperator.LessEqualThan);
                        relationalOperators.Add (ExpressionOperator.GreaterThan);
                        relationalOperators.Add (ExpressionOperator.GreaterEqualThan);
                        relationalOperators.Add (ExpressionOperator.Equal);
                        relationalOperators.Add (ExpressionOperator.Equal_Obj);
                        relationalOperators.Add (ExpressionOperator.NotEqual);
                }

                public static bool IsUnary (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.UnaryMinus || op == ExpressionOperator.Not;
                }

                public static bool IsZerary (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.Constant || op == ExpressionOperator.Variable;
                }

                public static bool IsBinary (this ExpressionOperator op)
                {
                        return !op.IsUnary () && !op.IsZerary ();
                }

                public static bool IsGreaterThan (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.GreaterThan;
                }

                public static bool IsGreaterEqualThan (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.GreaterEqualThan;
                }

                public static bool IsLessThan (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.LessThan;
                }

                public static bool IsLessEqualThan (this ExpressionOperator op)
                {
                        return op == ExpressionOperator.LessEqualThan;
                }

                public static bool IsRelational (this ExpressionOperator op)
                {
                        return relationalOperators.Contains (op);
                }
        }
}