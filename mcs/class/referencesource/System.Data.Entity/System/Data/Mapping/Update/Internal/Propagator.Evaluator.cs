//---------------------------------------------------------------------
// <copyright file="Propagator.Evaluator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.Utils;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;

    internal partial class Propagator
    {
        /// <summary>
        /// Helper class supporting the evaluation of highly constrained expressions of the following 
        /// form:
        /// 
        /// P := P AND P | P OR P | NOT P | V is of type | V eq V | V
        /// V := P
        /// V := Property(V) | Constant | CASE WHEN P THEN V ... ELSE V | Row | new Instance | Null
        /// 
        /// The evaluator supports SQL style ternary logic for unknown results (bool? is used, where
        /// null --> unknown, true --> TRUE and false --> FALSE
        /// </summary>
        /// <remarks>
        /// Assumptions:
        /// 
        /// - The node and the row passed in must be type compatible.
        /// 
        /// Any var refs in the node must have the same type as the input row. This is a natural
        /// requirement given the usage of this method in the propagator, since each propagator handler
        /// produces rows of the correct type for its parent. Keep in mind that every var ref in a CQT is
        /// bound specifically to the direct child.
        /// 
        /// - Equality comparisons are CLR culture invariant. Practically, this introduces the following
        /// differences from SQL comparisons:
        /// 
        ///     - String comparisons are not collation sensitive
        ///     - The constants we compare come from a fixed repertoire of scalar types implementing IComparable
        /// 
        /// 
        /// For the purposes of update mapping view evaluation, these assumptions are safe because we
        /// only support mapping of non-null constants to fields (these constants are non-null discriminators)
        /// and key comparisons (where the key values are replicated across a reference).
        /// </remarks>
        private class Evaluator : UpdateExpressionVisitor<PropagatorResult>
        {
            #region Constructors
            /// <summary>
            /// Constructs an evaluator for evaluating expressions for the given row.
            /// </summary>
            /// <param name="row">Row to match</param>
            /// <param name="parent">Propagator context</param>
            private Evaluator(PropagatorResult row, Propagator parent)
            {
                EntityUtil.CheckArgumentNull(row, "row");
                EntityUtil.CheckArgumentNull(parent, "parent");

                m_row = row;
                m_parent = parent;
            }
            #endregion

            #region Fields
            private PropagatorResult m_row;
            private Propagator m_parent;
            private static readonly string s_visitorName = typeof(Evaluator).FullName;
            #endregion

            #region Properties
            override protected string VisitorName
            {
                get { return s_visitorName; }
            }
            #endregion

            #region Methods
            /// <summary>
            /// Utility method filtering out a set of rows given a predicate.
            /// </summary>
            /// <param name="predicate">Match criteria.</param>
            /// <param name="rows">Input rows.</param>
            /// <param name="parent">Propagator context</param>
            /// <returns>Input rows matching criteria.</returns>
            internal static IEnumerable<PropagatorResult> Filter(DbExpression predicate, IEnumerable<PropagatorResult> rows, Propagator parent)
            {
                foreach (PropagatorResult row in rows)
                {
                    if (EvaluatePredicate(predicate, row, parent))
                    {
                        yield return row;
                    }
                }
            }

            /// <summary>
            /// Utility method determining whether a row matches a predicate.
            /// </summary>
            /// <remarks>
            /// See Walker class for an explanation of this coding pattern.
            /// </remarks>
            /// <param name="predicate">Match criteria.</param>
            /// <param name="row">Input row.</param>
            /// <param name="parent">Propagator context</param>
            /// <returns><c>true</c> if the row matches the criteria; <c>false</c> otherwise</returns>
            internal static bool EvaluatePredicate(DbExpression predicate, PropagatorResult row, Propagator parent)
            {
                Evaluator evaluator = new Evaluator(row, parent);
                PropagatorResult expressionResult = predicate.Accept(evaluator);

                bool? result = ConvertResultToBool(expressionResult);

                // unknown --> false at base of predicate
                return result ?? false;
            }

            /// <summary>
            /// Evaluates scalar node.
            /// </summary>
            /// <param name="node">Sub-query returning a scalar value.</param>
            /// <param name="row">Row to evaluate.</param>
            /// <param name="parent">Propagator context.</param>
            /// <returns>Scalar result.</returns>
            static internal PropagatorResult Evaluate(DbExpression node, PropagatorResult row, Propagator parent)
            {
                DbExpressionVisitor<PropagatorResult> evaluator = new Evaluator(row, parent);
                return node.Accept(evaluator);
            }

            /// <summary>
            /// Given an expression, converts to a (nullable) bool. Only boolean constant and null are
            /// supported.
            /// </summary>
            /// <param name="result">Result to convert</param>
            /// <returns>true if true constant; false if false constant; null is null constant</returns>
            private static bool? ConvertResultToBool(PropagatorResult result)
            {
                Debug.Assert(null != result && result.IsSimple, "Must be a simple Boolean result");

                if (result.IsNull)
                {
                    return null;
                }
                else
                {
                    // rely on cast exception to identify invalid cases (CQT validation should already take care of this)
                    return (bool)result.GetSimpleValue();
                }
            }

            /// <summary>
            /// Converts a (nullable) bool to an expression.
            /// </summary>
            /// <param name="booleanValue">Result</param>
            /// <param name="inputs">Inputs contributing to the result</param>
            /// <returns>DbExpression</returns>
            private static PropagatorResult ConvertBoolToResult(bool? booleanValue, params PropagatorResult[] inputs)
            {
                object result;
                if (booleanValue.HasValue)
                {
                    result = booleanValue.Value; ;
                }
                else
                {
                    result = null;
                }
                PropagatorFlags flags = PropagateUnknownAndPreserveFlags(null, inputs);
                return PropagatorResult.CreateSimpleValue(flags, result);
            }

            #region DbExpressionVisitor implementation
            /// <summary>
            /// Determines whether the argument being evaluated has a given type (declared in the IsOfOnly predicate).
            /// </summary>
            /// <param name="predicate">IsOfOnly predicate.</param>
            /// <returns>True if the row being evaluated is of the requested type; false otherwise.</returns>
            public override PropagatorResult Visit(DbIsOfExpression predicate)
            {
                EntityUtil.CheckArgumentNull(predicate, "predicate");

                if (DbExpressionKind.IsOfOnly != predicate.ExpressionKind)
                {
                    throw ConstructNotSupportedException(predicate);
                }

                PropagatorResult childResult = Visit(predicate.Argument);
                bool result;
                if (childResult.IsNull)
                {
                    // Null value expressions are typed, but the semantics of null are slightly different.
                    result = false;
                }
                else
                {
                    result = childResult.StructuralType.EdmEquals(predicate.OfType.EdmType);
                }

                return ConvertBoolToResult(result, childResult);
            }

            /// <summary>
            /// Determines whether the row being evaluated has the given type (declared in the IsOf predicate).
            /// </summary>
            /// <param name="predicate">Equals predicate.</param>
            /// <returns>True if the values being compared are equivalent; false otherwise.</returns>
            public override PropagatorResult Visit(DbComparisonExpression predicate)
            {
                EntityUtil.CheckArgumentNull(predicate, "predicate");

                if (DbExpressionKind.Equals == predicate.ExpressionKind)
                {
                    // Retrieve the left and right hand sides of the equality predicate.
                    PropagatorResult leftResult = Visit(predicate.Left);
                    PropagatorResult rightResult = Visit(predicate.Right);

                    bool? result;

                    if (leftResult.IsNull || rightResult.IsNull)
                    {
                        result = null; // unknown
                    }
                    else
                    {
                        object left = leftResult.GetSimpleValue();
                        object right = rightResult.GetSimpleValue();

                        // Perform a comparison between the sides of the equality predicate using invariant culture.
                        // See assumptions outlined in the documentation for this class for additional information.
                        result = ByValueEqualityComparer.Default.Equals(left, right);
                    }

                    return ConvertBoolToResult(result, leftResult, rightResult);
                }
                else
                {
                    throw ConstructNotSupportedException(predicate);
                }
            }

            /// <summary>
            /// Evaluates an 'and' expression given results of evalating its children.
            /// </summary>
            /// <param name="predicate">And predicate</param>
            /// <returns>True if both child predicates are satisfied; false otherwise.</returns>
            public override PropagatorResult Visit(DbAndExpression predicate)
            {
                EntityUtil.CheckArgumentNull(predicate, "predicate");

                PropagatorResult left = Visit(predicate.Left);
                PropagatorResult right = Visit(predicate.Right);
                bool? leftResult = ConvertResultToBool(left);
                bool? rightResult = ConvertResultToBool(right);
                bool? result;

                // Optimization: if either argument is false, preserved and known, return a
                // result that is false, preserved and known.
                if ((leftResult.HasValue && !leftResult.Value && PreservedAndKnown(left)) ||
                    (rightResult.HasValue && !rightResult.Value && PreservedAndKnown(right)))
                {
                    return CreatePerservedAndKnownResult(false);
                }

                result = EntityUtil.ThreeValuedAnd(leftResult, rightResult);

                return ConvertBoolToResult(result, left, right);
            }

            /// <summary>
            /// Evaluates an 'or' expression given results of evaluating its children.
            /// </summary>
            /// <param name="predicate">'Or' predicate</param>
            /// <returns>True if either child predicate is satisfied; false otherwise.</returns>
            public override PropagatorResult Visit(DbOrExpression predicate)
            {
                EntityUtil.CheckArgumentNull(predicate, "predicate");

                PropagatorResult left = Visit(predicate.Left);
                PropagatorResult right = Visit(predicate.Right);
                bool? leftResult = ConvertResultToBool(left);
                bool? rightResult = ConvertResultToBool(right);
                bool? result;

                // Optimization: if either argument is true, preserved and known, return a
                // result that is true, preserved and known.
                if ((leftResult.HasValue && leftResult.Value && PreservedAndKnown(left)) ||
                    (rightResult.HasValue && rightResult.Value && PreservedAndKnown(right)))
                {
                    return CreatePerservedAndKnownResult(true);
                }

                result = EntityUtil.ThreeValuedOr(leftResult, rightResult);

                return ConvertBoolToResult(result, left, right);
            }

            private static PropagatorResult CreatePerservedAndKnownResult(object value)
            {
                // Known is the default (no explicit flag required)
                return PropagatorResult.CreateSimpleValue(PropagatorFlags.Preserve, value);
            }

            private static bool PreservedAndKnown(PropagatorResult result)
            {
                // Check that the preserve flag is set, and the unknown flag is not set
                return PropagatorFlags.Preserve == (result.PropagatorFlags & (PropagatorFlags.Preserve | PropagatorFlags.Unknown));
            }

            /// <summary>
            /// Evalutes a 'not' expression given results 
            /// </summary>
            /// <param name="predicate">'Not' predicate</param>
            /// <returns>True of the argument to the 'not' predicate evaluator to false; false otherwise</returns>
            public override PropagatorResult Visit(DbNotExpression predicate)
            {
                EntityUtil.CheckArgumentNull(predicate, "predicate");

                PropagatorResult child = Visit(predicate.Argument);
                bool? childResult = ConvertResultToBool(child);

                bool? result = EntityUtil.ThreeValuedNot(childResult);

                return ConvertBoolToResult(result, child);
            }

            /// <summary>
            /// Returns the result of evaluating a case expression.
            /// </summary>
            /// <param name="node">Case expression node.</param>
            /// <returns>Result of evaluating case expression over the input row for this visitor.</returns>
            public override PropagatorResult Visit(DbCaseExpression node)
            {
                Debug.Assert(null != node, "node is not visited when null");

                int match = -1;
                int statementOrdinal = 0;

                List<PropagatorResult> inputs = new List<PropagatorResult>();

                foreach (DbExpression when in node.When)
                {
                    PropagatorResult whenResult = Visit(when);
                    inputs.Add(whenResult);

                    bool matches = ConvertResultToBool(whenResult) ?? false; // ternary logic resolution

                    if (matches)
                    {
                        match = statementOrdinal;
                        break;
                    }

                    statementOrdinal++;
                }

                PropagatorResult matchResult;
                if (-1 == match) { matchResult = Visit(node.Else); } else { matchResult = Visit(node.Then[match]); }
                inputs.Add(matchResult);

                // Clone the result to avoid modifying expressions that may be used elsewhere
                // (design invariant: only set markup for expressions you create)
                PropagatorFlags resultFlags = PropagateUnknownAndPreserveFlags(matchResult, inputs);
                PropagatorResult result = matchResult.ReplicateResultWithNewFlags(resultFlags);

                return result;
            }

            /// <summary>
            /// Evaluates a var ref. In practice, this corresponds to the input row for the visitor (the row is
            /// a member of the referenced input for a projection or filter).
            /// We assert that types are consistent here.
            /// </summary>
            /// <param name="node">Var ref expression node</param>
            /// <returns>Input row for the visitor.</returns>
            public override PropagatorResult Visit(DbVariableReferenceExpression node)
            {
                Debug.Assert(null != node, "node is not visited when null");

                return m_row;
            }

            /// <summary>
            /// Evaluates a property expression given the result of evaluating the property's instance.
            /// </summary>
            /// <param name="node">Property expression node.</param>
            /// <returns>DbExpression resulting from the evaluation of property.</returns>
            public override PropagatorResult Visit(DbPropertyExpression node)
            {
                Debug.Assert(null != node, "node is not visited when null");

                // Retrieve the result of evaluating the instance for the property.
                PropagatorResult instance = Visit(node.Instance);
                PropagatorResult result;

                if (instance.IsNull)
                {
                    result = PropagatorResult.CreateSimpleValue(instance.PropagatorFlags, null);
                }
                else
                {
                    // find member
                    result = instance.GetMemberValue(node.Property);
                }

                // We do not markup the result since the property value already contains the necessary context
                // (determined at record extraction time)
                return result;
            }

            /// <summary>
            /// Evaluates a constant expression (trivial: the result is the constant expression)
            /// </summary>
            /// <param name="node">Constant expression node.</param>
            /// <returns>Constant expression</returns>
            public override PropagatorResult Visit(DbConstantExpression node)
            {
                Debug.Assert(null != node, "node is not visited when null");

                // Flag the expression as 'preserve', since constants (by definition) cannot vary
                PropagatorResult result = PropagatorResult.CreateSimpleValue(PropagatorFlags.Preserve, node.Value);

                return result;
            }

            /// <summary>
            /// Evaluates a ref key expression based on the result of evaluating the argument to the ref.
            /// </summary>
            /// <param name="node">Ref key expression node.</param>
            /// <returns>The structural key of the ref as a new instance (record).</returns>
            public override PropagatorResult Visit(DbRefKeyExpression node)
            {
                Debug.Assert(null != node, "node is not visited when null");

                // Retrieve the result of evaluating the child argument.
                PropagatorResult argument = Visit(node.Argument);

                // Return the argument directly (propagator results treat refs as standard structures)
                return argument;
            }

            /// <summary>
            /// Evaluates a null expression (trivial: the result is the null expression)
            /// </summary>
            /// <param name="node">Null expression node.</param>
            /// <returns>Null expression</returns>
            public override PropagatorResult Visit(DbNullExpression node)
            {
                Debug.Assert(null != node, "node is not visited when null");

                // Flag the expression as 'preserve', since nulls (by definition) cannot vary
                PropagatorResult result = PropagatorResult.CreateSimpleValue(PropagatorFlags.Preserve, null);

                return result;
            }

            /// <summary>
            /// Evaluates treat expression given a result for the argument to the treat.
            /// </summary>
            /// <param name="node">Treat expression</param>
            /// <returns>Null if the argument is of the given type, the argument otherwise</returns>
            public override PropagatorResult Visit(DbTreatExpression node)
            {
                Debug.Assert(null != node, "node is not visited when null");

                PropagatorResult childResult = Visit(node.Argument);
                TypeUsage nodeType = node.ResultType;

                if (MetadataHelper.IsSuperTypeOf(nodeType.EdmType, childResult.StructuralType))
                {
                    // Doing an up cast is not required because all property/ordinal
                    // accesses are unaffected for more derived types (derived members
                    // are appended)
                    return childResult;
                }
                    
                // "Treat" where the result does not implement the given type results in a null
                // result
                PropagatorResult result = PropagatorResult.CreateSimpleValue(childResult.PropagatorFlags, null);
                return result;
            }

            /// <summary>
            /// Casts argument to expression.
            /// </summary>
            /// <param name="node">Cast expression node</param>
            /// <returns>Result of casting argument</returns>
            public override PropagatorResult Visit(DbCastExpression node)
            {
                Debug.Assert(null != node, "node is not visited when null");

                PropagatorResult childResult = Visit(node.Argument);
                TypeUsage nodeType = node.ResultType;

                if (!childResult.IsSimple || BuiltInTypeKind.PrimitiveType != nodeType.EdmType.BuiltInTypeKind)
                {
                    throw EntityUtil.NotSupported(Strings.Update_UnsupportedCastArgument(nodeType.EdmType.Name));
                }

                object resultValue;

                if (childResult.IsNull)
                {
                    resultValue = null;
                }
                else
                {
                    try
                    {
                        resultValue = Cast(childResult.GetSimpleValue(), ((PrimitiveType)nodeType.EdmType).ClrEquivalentType);
                    }
                    catch
                    {
                        Debug.Fail("view generator failed to validate cast in update mapping view");
                        throw;
                    }
                }

                PropagatorResult result = childResult.ReplicateResultWithNewValue(resultValue);
                return result;
            }

            /// <summary>
            /// Casts an object instance to the specified model type.
            /// </summary>
            /// <param name="value">Value to cast</param>
            /// <param name="clrPrimitiveType">clr type to which the value is casted to</param>
            /// <returns>Cast value</returns>
            private static object Cast(object value, Type clrPrimitiveType)
            {
                IFormatProvider formatProvider = CultureInfo.InvariantCulture;

                if (null == value || value == DBNull.Value || value.GetType() == clrPrimitiveType)
                {
                    return value;
                }
                else
                {
                    //Convert is not handling DateTime to DateTimeOffset conversion
                    if ( (value is DateTime) && (clrPrimitiveType == typeof(DateTimeOffset)))
                    {
                        return new DateTimeOffset(((DateTime)value).Ticks, TimeSpan.Zero);
                    }
                    else
                    {
                        return Convert.ChangeType(value, clrPrimitiveType, formatProvider);
                    }
                }
            }

            /// <summary>
            /// Evaluate a null expression.
            /// </summary>
            /// <param name="node">Is null expression</param>
            /// <returns>A boolean expression describing the result of evaluating the Is Null predicate</returns>
            public override PropagatorResult Visit(DbIsNullExpression node)
            {
                Debug.Assert(null != node, "node is not visited when null");

                PropagatorResult argumentResult = Visit(node.Argument);
                bool result = argumentResult.IsNull;

                return ConvertBoolToResult(result, argumentResult);
            }
            #endregion
            /// <summary>
            /// Supports propagation of preserve and unknown values when evaluating expressions. If any input 
            /// to an expression is marked as unknown, the same is true of the result of evaluating
            /// that expression. If all inputs to an expression are marked 'preserve', then the result is also
            /// marked preserve.
            /// </summary>
            /// <param name="result">Result to markup</param>
            /// <param name="inputs">Expressions contributing to the result</param>
            /// <returns>Marked up result.</returns>
            private static PropagatorFlags PropagateUnknownAndPreserveFlags(PropagatorResult result, IEnumerable<PropagatorResult> inputs)
            {
                bool unknown = false;
                bool preserve = true;
                bool noInputs = true;

                // aggregate all flags on the inputs
                foreach (PropagatorResult input in inputs)
                {
                    noInputs = false;
                    PropagatorFlags inputFlags = input.PropagatorFlags;
                    if (PropagatorFlags.NoFlags != (PropagatorFlags.Unknown & inputFlags))
                    {
                        unknown = true;
                    }
                    if (PropagatorFlags.NoFlags == (PropagatorFlags.Preserve & inputFlags))
                    {
                        preserve = false;
                    }
                }
                if (noInputs) { preserve = false; }

                if (null != result)
                {
                    // Merge with existing flags
                    PropagatorFlags flags = result.PropagatorFlags;
                    if (unknown)
                    {
                        flags |= PropagatorFlags.Unknown;
                    }
                    if (!preserve)
                    {
                        flags &= ~PropagatorFlags.Preserve;
                    }

                    return flags;
                }
                else
                {
                    // if there is no input result, create new markup from scratch
                    PropagatorFlags flags = PropagatorFlags.NoFlags;
                    if (unknown)
                    {
                        flags |= PropagatorFlags.Unknown;
                    }
                    if (preserve)
                    {
                        flags |= PropagatorFlags.Preserve;
                    }
                    return flags;
                }
            }
            #endregion
        }
    }
}
