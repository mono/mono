//---------------------------------------------------------------------
// <copyright file="LinqExpressionNormalizer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....], [....]
//---------------------------------------------------------------------

using System.Linq.Expressions;
using System.Diagnostics;
using System.Collections.Generic;
using System.Reflection;
namespace System.Data.Objects.ELinq
{
    /// <summary>
    /// Replaces expression patterns produced by the compiler with approximations
    /// used in query translation. For instance, the following VB code:
    /// 
    ///     x = y
    ///     
    /// becomes the expression
    /// 
    ///     Equal(MethodCallExpression(Microsoft.VisualBasic.CompilerServices.Operators.CompareString(x, y, False), 0)
    ///     
    /// which is normalized to
    /// 
    ///     Equal(x, y)
    ///     
    /// Comment convention:
    /// 
    ///     CODE(Lang): _VB or C# coding pattern being simplified_
    ///     ORIGINAL: _original LINQ expression_
    ///     NORMALIZED: _normalized LINQ expression_
    /// </summary>
    internal class LinqExpressionNormalizer : EntityExpressionVisitor
    {
        /// <summary>
        /// If we encounter a MethodCallExpression, we never need to lift to lift to null. This capability
        /// exists to translate certain patterns in the language. In this case, the user (or compiler)
        /// has explicitly asked for a method invocation (at which point, lifting can no longer occur).
        /// </summary>
        private const bool LiftToNull = false;

        /// <summary>
        /// Gets a dictionary mapping from LINQ expressions to matched by those expressions. Used
        /// to identify composite expression patterns.
        /// </summary>
        private readonly Dictionary<Expression, Pattern> _patterns = new Dictionary<Expression, Pattern>();

        /// <summary>
        /// Handle binary patterns:
        /// 
        /// - VB 'Is' operator
        /// - Compare patterns
        /// </summary>
        internal override Expression VisitBinary(BinaryExpression b)
        {
            b = (BinaryExpression)base.VisitBinary(b);

            // CODE(VB): x Is y
            // ORIGINAL: Equal(Convert(x, typeof(object)), Convert(y, typeof(object))
            // NORMALIZED: Equal(x, y)
            if (b.NodeType == ExpressionType.Equal)
            {
                Expression normalizedLeft = UnwrapObjectConvert(b.Left);
                Expression normalizedRight = UnwrapObjectConvert(b.Right);
                if (normalizedLeft != b.Left || normalizedRight != b.Right)
                {
                    b = CreateRelationalOperator(ExpressionType.Equal, normalizedLeft, normalizedRight);
                }
            }

            // CODE(VB): x = y
            // ORIGINAL: Equal(Microsoft.VisualBasic.CompilerServices.Operators.CompareString(x, y, False), 0)
            // NORMALIZED: Equal(x, y)
            Pattern pattern;
            if (_patterns.TryGetValue(b.Left, out pattern) && pattern.Kind == PatternKind.Compare && IsConstantZero(b.Right))
            {
                ComparePattern comparePattern = (ComparePattern)pattern;
                // handle relational operators
                BinaryExpression relationalExpression;
                if (TryCreateRelationalOperator(b.NodeType, comparePattern.Left, comparePattern.Right, out relationalExpression))
                {
                    b = relationalExpression;
                }
            }

            return b;
        }

        /// <summary>
        /// CODE: x
        /// ORIGINAL: Convert(x, typeof(object))
        /// ORIGINAL(Funcletized): Constant(x, typeof(object))
        /// NORMALIZED: x
        /// </summary>
        private static Expression UnwrapObjectConvert(Expression input)
        {
            // recognize funcletized (already evaluated) Converts
            if (input.NodeType == ExpressionType.Constant &&
               input.Type == typeof(object))
            {
                ConstantExpression constant = (ConstantExpression)input;

                // we will handle nulls later, so just bypass those
                if (constant.Value != null &&
                    constant.Value.GetType() != typeof(object))
                {
                    return Expression.Constant(constant.Value, constant.Value.GetType());
                }
            }

            // unwrap object converts
            while (ExpressionType.Convert == input.NodeType && typeof(object) == input.Type)
            {
                input = ((UnaryExpression)input).Operand;
            }
            return input;
        }

        /// <summary>
        /// Returns true if the given expression is a constant '0'.
        /// </summary>
        private bool IsConstantZero(Expression expression)
        {
            return expression.NodeType == ExpressionType.Constant &&
                ((ConstantExpression)expression).Value.Equals(0);
        }

        /// <summary>
        /// Handles MethodCall patterns:
        /// 
        /// - Operator overloads
        /// - VB operators
        /// </summary>
        internal override Expression VisitMethodCall(MethodCallExpression m)
        {
            m = (MethodCallExpression)base.VisitMethodCall(m);

            if (m.Method.IsStatic)
            {
                // handle operator overloads
                if (m.Method.Name.StartsWith("op_", StringComparison.Ordinal))
                {
                    // handle binary operator overloads
                    if (m.Arguments.Count == 2)
                    {
                        // CODE(C#): x == y
                        // ORIGINAL: MethodCallExpression(<op_Equality>, x, y)
                        // NORMALIZED: Equal(x, y)
                        switch (m.Method.Name)
                        {
                            case "op_Equality":
                                return Expression.Equal(m.Arguments[0], m.Arguments[1], LiftToNull, m.Method);

                            case "op_Inequality":
                                return Expression.NotEqual(m.Arguments[0], m.Arguments[1], LiftToNull, m.Method);

                            case "op_GreaterThan":
                                return Expression.GreaterThan(m.Arguments[0], m.Arguments[1], LiftToNull, m.Method);

                            case "op_GreaterThanOrEqual":
                                return Expression.GreaterThanOrEqual(m.Arguments[0], m.Arguments[1], LiftToNull, m.Method);

                            case "op_LessThan":
                                return Expression.LessThan(m.Arguments[0], m.Arguments[1], LiftToNull, m.Method);

                            case "op_LessThanOrEqual":
                                return Expression.LessThanOrEqual(m.Arguments[0], m.Arguments[1], LiftToNull, m.Method);

                            case "op_Multiply":
                                return Expression.Multiply(m.Arguments[0], m.Arguments[1], m.Method);

                            case "op_Subtraction":
                                return Expression.Subtract(m.Arguments[0], m.Arguments[1], m.Method);

                            case "op_Addition":
                                return Expression.Add(m.Arguments[0], m.Arguments[1], m.Method);

                            case "op_Division":
                                return Expression.Divide(m.Arguments[0], m.Arguments[1], m.Method);

                            case "op_Modulus":
                                return Expression.Modulo(m.Arguments[0], m.Arguments[1], m.Method);

                            case "op_BitwiseAnd":
                                return Expression.And(m.Arguments[0], m.Arguments[1], m.Method);

                            case "op_BitwiseOr":
                                return Expression.Or(m.Arguments[0], m.Arguments[1], m.Method);

                            case "op_ExclusiveOr":
                                return Expression.ExclusiveOr(m.Arguments[0], m.Arguments[1], m.Method);

                            default:
                                break;
                        }
                    }

                    // handle unary operator overloads
                    if (m.Arguments.Count == 1)
                    {
                        // CODE(C#): +x
                        // ORIGINAL: MethodCallExpression(<op_UnaryPlus>, x)
                        // NORMALIZED: UnaryPlus(x)
                        switch (m.Method.Name)
                        {
                            case "op_UnaryNegation":
                                return Expression.Negate(m.Arguments[0], m.Method);

                            case "op_UnaryPlus":
                                return Expression.UnaryPlus(m.Arguments[0], m.Method);

                            case "op_Explicit":
                            case "op_Implicit":
                                return Expression.Convert(m.Arguments[0], m.Type, m.Method);

                            case "op_OnesComplement":
                            case "op_False":
                                return Expression.Not(m.Arguments[0], m.Method);

                            default:
                                break;
                        }
                    }
                }

                // check for static Equals method
                if (m.Method.Name == "Equals" && m.Arguments.Count > 1)
                {
                    // CODE(C#): Object.Equals(x, y)
                    // ORIGINAL: MethodCallExpression(<object.Equals>, x, y)
                    // NORMALIZED: Equal(x, y)
                    return Expression.Equal(m.Arguments[0], m.Arguments[1], false, m.Method);
                }

                // check for Microsoft.VisualBasic.CompilerServices.Operators.CompareString method
                if (m.Method.Name == "CompareString" && m.Method.DeclaringType.FullName == "Microsoft.VisualBasic.CompilerServices.Operators")
                {
                    // CODE(VB): x = y; where x and y are strings, a part of the expression looks like:
                    // ORIGINAL: MethodCallExpression(Microsoft.VisualBasic.CompilerServices.Operators.CompareString(x, y, False)
                    // NORMALIZED: see CreateCompareExpression method
                    return CreateCompareExpression(m.Arguments[0], m.Arguments[1]);
                }

                // check for static Compare method
                if (m.Method.Name == "Compare" && m.Arguments.Count > 1 && m.Method.ReturnType == typeof(int))
                {
                    // CODE(C#): Class.Compare(x, y)
                    // ORIGINAL: MethodCallExpression(<Compare>, x, y)
                    // NORMALIZED: see CreateCompareExpression method
                    return CreateCompareExpression(m.Arguments[0], m.Arguments[1]);
                }
            }
            else
            {
                // check for instance Equals method
                if (m.Method.Name == "Equals" && m.Arguments.Count > 0)
                {
                    // type-specific Equals method on spatial types becomes a call to the 'STEquals' spatial canonical function, so should remain in the expression tree.
                    Type parameterType = m.Method.GetParameters()[0].ParameterType;
                    if (parameterType != typeof(System.Data.Spatial.DbGeography) && parameterType != typeof(System.Data.Spatial.DbGeometry))
                    {
                        // CODE(C#): x.Equals(y)
                        // ORIGINAL: MethodCallExpression(x, <Equals>, y)
                        // NORMALIZED: Equal(x, y)
                        return CreateRelationalOperator(ExpressionType.Equal, m.Object, m.Arguments[0]);
                    }
                }

                // check for instance CompareTo method
                if (m.Method.Name == "CompareTo" && m.Arguments.Count == 1 && m.Method.ReturnType == typeof(int))
                {
                    // CODE(C#): x.CompareTo(y)
                    // ORIGINAL: MethodCallExpression(x.CompareTo(y))
                    // NORMALIZED: see CreateCompareExpression method
                    return CreateCompareExpression(m.Object, m.Arguments[0]);
                }

                // check for List<> instance Contains method
                if (m.Method.Name == "Contains" && m.Arguments.Count == 1) {
                    Type declaringType = m.Method.DeclaringType;
                    if (declaringType.IsGenericType && declaringType.GetGenericTypeDefinition() == typeof(List<>))
                    {
                        // CODE(C#): List<T> x.Contains(y)
                        // ORIGINAL: MethodCallExpression(x.Contains(y))
                        // NORMALIZED: IEnumerable<T>.Contains(x, y)

                        MethodInfo containsMethod;
                        if (ReflectionUtil.TryLookupMethod(SequenceMethod.Contains, out containsMethod))
                        {
                            MethodInfo enumerableContainsMethod = containsMethod.MakeGenericMethod(declaringType.GetGenericArguments());
                            return Expression.Call(enumerableContainsMethod, m.Object, m.Arguments[0]);
                        }
                    }
                }
            }

            // check for coalesce operators added by the VB compiler to predicate arguments
            return NormalizePredicateArgument(m);
        }



        /// <summary>
        /// Identifies and normalizes any predicate argument in the given call expression. If no changes
        /// are needed, returns the existing expression. Otherwise, returns a new call expression
        /// with a normalized predicate argument.
        /// </summary>
        private static MethodCallExpression NormalizePredicateArgument(MethodCallExpression callExpression)
        {
            MethodCallExpression result;

            int argumentOrdinal;
            Expression normalizedArgument;
            if (HasPredicateArgument(callExpression, out argumentOrdinal) &&
                TryMatchCoalescePattern(callExpression.Arguments[argumentOrdinal], out normalizedArgument))
            {
                List<Expression> normalizedArguments = new List<Expression>(callExpression.Arguments);

                // replace the predicate argument with the normalized version
                normalizedArguments[argumentOrdinal] = normalizedArgument;

                result = Expression.Call(callExpression.Object, callExpression.Method, normalizedArguments);
            }
            else
            {
                // nothing has changed
                result = callExpression;
            }

            return result;
        }

        /// <summary>
        /// Determines whether the given call expression has a 'predicate' argument (e.g. Where(source, predicate)) 
        /// and returns the ordinal for the predicate.
        /// </summary>
        /// <remarks>
        /// Obviously this method will need to be replaced if we ever encounter a method with multiple predicates.
        /// </remarks>
        private static bool HasPredicateArgument(MethodCallExpression callExpression, out int argumentOrdinal)
        {
            argumentOrdinal = default(int);
            bool result = false;

            // It turns out all supported methods taking a predicate argument have it as the second
            // argument. As a result, we always set argumentOrdinal to 1 when there is a match and
            // we can safely ignore all methods taking fewer than 2 arguments
            SequenceMethod sequenceMethod;
            if (2 <= callExpression.Arguments.Count &&
                ReflectionUtil.TryIdentifySequenceMethod(callExpression.Method, out sequenceMethod))
            {
                switch (sequenceMethod)
                {
                    case SequenceMethod.FirstPredicate:
                    case SequenceMethod.FirstOrDefaultPredicate:
                    case SequenceMethod.SinglePredicate:
                    case SequenceMethod.SingleOrDefaultPredicate:
                    case SequenceMethod.LastPredicate:
                    case SequenceMethod.LastOrDefaultPredicate:
                    case SequenceMethod.Where:
                    case SequenceMethod.WhereOrdinal:
                    case SequenceMethod.CountPredicate:
                    case SequenceMethod.LongCountPredicate:
                    case SequenceMethod.AnyPredicate:
                    case SequenceMethod.All:
                    case SequenceMethod.SkipWhile:
                    case SequenceMethod.SkipWhileOrdinal:
                    case SequenceMethod.TakeWhile:
                    case SequenceMethod.TakeWhileOrdinal:
                        argumentOrdinal = 1; // the second argument is always the one
                        result = true;
                        break;
                }
            }

            return result;
        }

        /// <summary>
        /// Determines whether the given expression of the form Lambda(Coalesce(left, Constant(false)), ...), a pattern
        /// introduced by the VB compiler for predicate arguments. Returns the 'normalized' version of the expression
        /// Lambda((bool)left, ...)
        /// </summary>
        private static bool TryMatchCoalescePattern(Expression expression, out Expression normalized)
        {
            normalized = null;
            bool result = false;

            if (expression.NodeType == ExpressionType.Quote)
            {
                // try to normalize the quoted expression
                UnaryExpression quote = (UnaryExpression)expression;
                if (TryMatchCoalescePattern(quote.Operand, out normalized))
                {
                    result = true;
                    normalized = Expression.Quote(normalized);
                }
            }
            else if (expression.NodeType == ExpressionType.Lambda)
            {
                LambdaExpression lambda = (LambdaExpression)expression;

                // collapse coalesce lambda expressions
                // CODE(VB): where a.NullableInt = 1
                // ORIGINAL: Lambda(Coalesce(expr, Constant(false)), a)
                // NORMALIZED: Lambda(expr, a)
                if (lambda.Body.NodeType == ExpressionType.Coalesce && lambda.Body.Type == typeof(bool))
                {
                    BinaryExpression coalesce = (BinaryExpression)lambda.Body;
                    if (coalesce.Right.NodeType == ExpressionType.Constant && false.Equals(((ConstantExpression)coalesce.Right).Value))
                    {
                        normalized = Expression.Lambda(lambda.Type, Expression.Convert(coalesce.Left, typeof(bool)), lambda.Parameters);
                        result = true;
                    }
                }
            }

            return result;
        }

        private static readonly MethodInfo s_relationalOperatorPlaceholderMethod = typeof(LinqExpressionNormalizer).GetMethod("RelationalOperatorPlaceholder", BindingFlags.Static | BindingFlags.NonPublic);
        /// <summary>
        /// This method exists solely to support creation of valid relational operator LINQ expressions that are not natively supported
        /// by the CLR (e.g. String > String). This method must not be invoked.
        /// </summary>
        private static bool RelationalOperatorPlaceholder<TLeft, TRight>(TLeft left, TRight right)
        {
            Debug.Fail("This method should never be called. It exists merely to support creation of relational LINQ expressions.");
            return object.ReferenceEquals(left, right);
        }

        /// <summary>
        /// Create an operator relating 'left' and 'right' given a relational operator.
        /// </summary>
        private static BinaryExpression CreateRelationalOperator(ExpressionType op, Expression left, Expression right)
        {
            BinaryExpression result;
            if (!TryCreateRelationalOperator(op, left, right, out result))
            {
                Debug.Fail("CreateRelationalOperator has unknown op " + op);
            }
            return result;
        }

        /// <summary>
        /// Try to create an operator relating 'left' and 'right' using the given operator. If the given operator
        /// does not define a known relation, returns false.
        /// </summary>
        private static bool TryCreateRelationalOperator(ExpressionType op, Expression left, Expression right, out BinaryExpression result)
        {
            MethodInfo relationalOperatorPlaceholderMethod = s_relationalOperatorPlaceholderMethod.MakeGenericMethod(left.Type, right.Type);

            switch (op)
            {
                case ExpressionType.Equal:
                    result = Expression.Equal(left, right, LiftToNull, relationalOperatorPlaceholderMethod);
                    return true;

                case ExpressionType.NotEqual:
                    result = Expression.NotEqual(left, right, LiftToNull, relationalOperatorPlaceholderMethod);
                    return true;

                case ExpressionType.LessThan:
                    result = Expression.LessThan(left, right, LiftToNull, relationalOperatorPlaceholderMethod);
                    return true;

                case ExpressionType.LessThanOrEqual:
                    result = Expression.LessThanOrEqual(left, right, LiftToNull, relationalOperatorPlaceholderMethod);
                    return true;

                case ExpressionType.GreaterThan:
                    result = Expression.GreaterThan(left, right, LiftToNull, relationalOperatorPlaceholderMethod);
                    return true;

                case ExpressionType.GreaterThanOrEqual:
                    result = Expression.GreaterThanOrEqual(left, right, LiftToNull, relationalOperatorPlaceholderMethod);
                    return true;
 
                default:
                    result = null;
                    return false;
            }
        }

        /// <summary>
        /// CODE(C#): Class.Compare(left, right)
        /// ORIGINAL: MethodCallExpression(Compare, left, right)
        /// NORMALIZED: Condition(Equal(left, right), 0, Condition(left > right, 1, -1))
        /// 
        /// Why is this an improvement? We know how to evaluate Condition in the store, but we don't
        /// know how to evaluate MethodCallExpression... Where the CompareTo appears within a larger expression,
        /// e.g. left.CompareTo(right) > 0, we can further simplify to left > right (we register the "ComparePattern"
        /// to make this possible).
        /// </summary>
        private Expression CreateCompareExpression(Expression left, Expression right)
        {
            Expression result = Expression.Condition(
                CreateRelationalOperator(ExpressionType.Equal, left, right),
                Expression.Constant(0),
                Expression.Condition(
                    CreateRelationalOperator(ExpressionType.GreaterThan, left, right),
                    Expression.Constant(1),
                    Expression.Constant(-1)));

            // Remember that this node matches the pattern
            _patterns[result] = new ComparePattern(left, right);

            return result;
        }

        /// <summary>
        /// Encapsulates an expression matching some pattern.
        /// </summary>
        private abstract class Pattern
        {
            /// <summary>
            /// Gets pattern kind.
            /// </summary>
            internal abstract PatternKind Kind { get; }
        }

        /// <summary>
        /// Gets pattern kind.
        /// </summary>
        private enum PatternKind
        {
            Compare,
        }

        /// <summary>
        /// Matches expression of the form x.CompareTo(y) or Class.CompareTo(x, y)
        /// </summary>
        private sealed class ComparePattern : Pattern
        {
            internal ComparePattern(Expression left, Expression right)
            {
                this.Left = left;
                this.Right = right;
            }

            /// <summary>
            /// Gets left-hand argument to Compare operation.
            /// </summary>
            internal readonly Expression Left;

            /// <summary>
            /// Gets right-hand argument to Compare operation.
            /// </summary>
            internal readonly Expression Right;


            internal override PatternKind Kind
            {
                get { return PatternKind.Compare; }
            }
        }
    }
}
