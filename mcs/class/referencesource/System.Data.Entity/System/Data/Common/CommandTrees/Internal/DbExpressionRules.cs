//---------------------------------------------------------------------
// <copyright file="DbExpressionRules.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Data.Common.CommandTrees;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Data.Common.Utils;
using System.Linq;
using System.Globalization;
using System.Data.Common.CommandTrees.ExpressionBuilder;

namespace System.Data.Common.CommandTrees.Internal
{
    /// <summary>
    /// Enacapsulates the logic that defines an expression 'rule' which is capable of transforming a candidate <see cref="DbExpression"/>
    /// into a result DbExpression, and indicating what action should be taken on that result expression by the rule application logic.
    /// </summary>
    internal abstract class DbExpressionRule
    {
        /// <summary>
        /// Indicates what action the rule processor should take if the rule successfully processes an expression.
        /// </summary>
        internal enum ProcessedAction
        {
            /// <summary>
            /// Continue to apply rules, from the rule immediately following this rule, to the result expression
            /// </summary>
            Continue = 0,

            /// <summary>
            /// Going back to the first rule, apply all rules to the result expression
            /// </summary>
            Reset,

            /// <summary>
            /// Stop all rule processing and return the result expression as the final result expression
            /// </summary>
            Stop
        }

        /// <summary>
        /// Indicates whether <see cref="TryProcess"/> should be called on the specified argument expression.
        /// </summary>
        /// <param name="expression">The <see cref="DbExpression"/> that the rule should inspect and determine if processing is possible</param>
        /// <returns><c>true</c> if the rule can attempt processing of the expression via the <see cref="TryProcess"/> method; otherwise <c>false</c></returns>
        internal abstract bool ShouldProcess(DbExpression expression);
        
        /// <summary>
        /// Attempts to process the input <paramref name="expression"/> to produce a <paramref name="result"/> <see cref="DbExpression"/>.
        /// </summary>
        /// <param name="expression">The input expression that the rule should process</param>
        /// <param name="result">The result expression produced by the rule if processing was successful</param>
        /// <returns><c>true</c> if the rule was able to successfully process the input expression and produce a result expression; otherwise <c>false</c></returns>
        internal abstract bool TryProcess(DbExpression expression, out DbExpression result);
        
        /// <summary>
        /// Indicates what action - as a <see cref="ProcessedAction"/> value - the rule processor should take if <see cref="TryProcess"/> returns true.
        /// </summary>
        internal abstract ProcessedAction OnExpressionProcessed { get; }
    }

    /// <summary>
    /// Abstract base class for a DbExpression visitor that can apply a collection of <see cref="DbExpressionRule"/>s during the visitor pass, returning the final result expression.
    /// This class encapsulates the rule application logic that applies regardless of how the ruleset - modelled as the abstract <see cref="GetRules"/> method - is provided.
    /// </summary>
    internal abstract class DbExpressionRuleProcessingVisitor : DefaultExpressionVisitor
    {
        protected DbExpressionRuleProcessingVisitor() { }

        protected abstract IEnumerable<DbExpressionRule> GetRules();

        private static Tuple<DbExpression, DbExpressionRule.ProcessedAction> ProcessRules(DbExpression expression, List<DbExpressionRule> rules)
        {
            // Considering each rule in the rule set in turn, if the rule indicates that it can process the
            // input expression, call TryProcess to attempt processing. If successful, take the action specified
            // by the rule's OnExpressionProcessed action, which may involve returning the action and the result
            // expression so that processing can be reset or halted.

            for (int idx = 0; idx < rules.Count; idx++)
            {
                DbExpressionRule currentRule = rules[idx];
                if (currentRule.ShouldProcess(expression))
                {
                    DbExpression result;
                    if (currentRule.TryProcess(expression, out result))
                    {
                        if (currentRule.OnExpressionProcessed != DbExpressionRule.ProcessedAction.Continue)
                        {
                            return Tuple.Create(result, currentRule.OnExpressionProcessed);
                        }
                        else
                        {
                            expression = result;
                        }
                    }
                }
            }
            return Tuple.Create(expression, DbExpressionRule.ProcessedAction.Continue);
        }

        private bool _stopped;

        private DbExpression ApplyRules(DbExpression expression)
        {
            // Driver loop to apply rules while the status of processing is 'Reset',
            // or correctly set the _stopped flag if status is 'Stopped'.

            List<DbExpressionRule> currentRules = this.GetRules().ToList();
            var ruleResult = ProcessRules(expression, currentRules);
            while (ruleResult.Item2 == DbExpressionRule.ProcessedAction.Reset)
            {
                currentRules = this.GetRules().ToList();
                ruleResult = ProcessRules(ruleResult.Item1, currentRules);
            }
            if (ruleResult.Item2 == DbExpressionRule.ProcessedAction.Stop)
            {
                _stopped = true;
            }
            return ruleResult.Item1;
        }

        protected override DbExpression VisitExpression(DbExpression expression)
        {
            // Pre-process this visitor's rules
            DbExpression result = ApplyRules(expression);
            if (_stopped)
            {
                // If rule processing was stopped, the result expression must be returned immediately
                return result;
            }

            // Visit the expression to recursively apply rules to subexpressions
            result = base.VisitExpression(result);
            if (_stopped)
            {
                // If rule processing was stopped, the result expression must be returned immediately
                return result;
            }

            // Post-process the rules over the resulting expression and return the result.
            // This is done so that rules that did not match the original structure of the
            // expression have an opportunity to examine the structure of the result expression.
            result = ApplyRules(result);
            return result;
        }
    }
}
