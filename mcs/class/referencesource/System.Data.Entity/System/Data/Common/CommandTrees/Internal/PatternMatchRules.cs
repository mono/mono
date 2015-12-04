//---------------------------------------------------------------------
// <copyright file="PatternMatchRules.cs" company="Microsoft">
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
    /// PatternMatchRule is a specialization of <see cref="DbExpressionRule"/> that uses a Func&lt;DbExpression, bool&gt; 'pattern'
    /// to implement <see cref="DbExpressionRule.ShouldProcess"/> and a Func&lt;DbExpression, DbExpression&gt; 'processor' to implement
    /// <see cref="DbExpressionRule.TryProcess"/>. The 'processor' should return <c>null</c> to indicate that the expression was not
    /// successfully processed, otherwise it should return the new result expression.
    /// </summary>
    internal class PatternMatchRule : DbExpressionRule
    {
        private readonly Func<DbExpression, bool> isMatch;
        private readonly Func<DbExpression, DbExpression> process;
        private readonly ProcessedAction processed;

        private PatternMatchRule(Func<DbExpression, bool> matchFunc, Func<DbExpression, DbExpression> processor, ProcessedAction onProcessed)
        {
            this.isMatch = matchFunc;
            this.process = processor;
            this.processed = onProcessed;
        }

        internal override bool ShouldProcess(DbExpression expression)
        {
            return this.isMatch(expression);
        }

        internal override bool TryProcess(DbExpression expression, out DbExpression result)
        {
            result = this.process(expression);
            return (result != null);
        }

        internal override ProcessedAction OnExpressionProcessed
        {
            get { return this.processed; }
        }

        /// <summary>
        /// Constructs a new PatternMatch rule with the specified pattern, processor and default <see cref="DbExpressionRule.ProcessedAction"/> of <see cref="DbExpressionRule.ProcessedAction.Reset"/>
        /// </summary>
        internal static PatternMatchRule Create(Func<DbExpression, bool> matchFunc, Func<DbExpression, DbExpression> processor)
        {
            return PatternMatchRule.Create(matchFunc, processor, ProcessedAction.Reset);
        }

        /// <summary>
        /// Constructs a new PatternMatchRule with the specified pattern, processor and <see cref="DbExpressionRule.ProcessedAction"/>
        /// </summary>
        internal static PatternMatchRule Create(Func<DbExpression, bool> matchFunc, Func<DbExpression, DbExpression> processor, ProcessedAction onProcessed)
        {
            EntityUtil.CheckArgumentNull(matchFunc, "matchFunc");
            EntityUtil.CheckArgumentNull(processor, "processor");

            return new PatternMatchRule(matchFunc, processor, onProcessed);
        }
    }

    /// <summary>
    /// PatternMatchRuleProcessor is a specialization of <see cref="DbExpressionRuleProcessingVisitor"/> that uses a collection of <see cref="PatternMatchRule"/>s 
    /// as its ruleset. The static Create methods can be used to construct a new PatternMatchRuleProcessor that applies the specified PatternMatchRules, which is
    /// returned as a Func&lt;DbExpression, DbExpression&gt; that can be invoked directly on an expression to apply the ruleset to it.
    /// </summary>
    internal class PatternMatchRuleProcessor : DbExpressionRuleProcessingVisitor
    {
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<PatternMatchRule> ruleSet;

        private PatternMatchRuleProcessor(System.Collections.ObjectModel.ReadOnlyCollection<PatternMatchRule> rules)
        {
            Debug.Assert(rules.Count() != 0, "At least one PatternMatchRule is required");
            Debug.Assert(rules.Where(r => r == null).Count() == 0, "Individual PatternMatchRules must not be null");
            
            this.ruleSet = rules;
        }

        private DbExpression Process(DbExpression expression)
        {
            EntityUtil.CheckArgumentNull(expression, "expression");
            expression = this.VisitExpression(expression);
            return expression;
        }

        protected override IEnumerable<DbExpressionRule> GetRules()
        {
            return this.ruleSet;
        }

        internal static Func<DbExpression, DbExpression> Create(params PatternMatchRule[] rules)
        {
            EntityUtil.CheckArgumentNull(rules, "rules");

            return new PatternMatchRuleProcessor(new System.Collections.ObjectModel.ReadOnlyCollection<PatternMatchRule>(rules)).Process;
        }
    }

    /// <summary>
    /// Provides a means of constructing Func&lt;DbExpression, bool&gt; 'patterns' for use with <see cref="PatternMatchRule"/>s.
    /// </summary>
    internal static class Patterns
    {
        #region Pattern Combinators

        /// <summary>
        /// Constructs a new pattern that is matched iff both <paramref name="pattern1"/> and <paramref name="pattern2"/> are matched. Does NOT return a pattern that matches <see cref="DbAndExpression"/>. Use <see cref="MatchKind"/> with an argument of <see cref="DbExpressionKind.And"/> to match an AND expression
        /// </summary>
        internal static Func<DbExpression, bool> And(Func<DbExpression, bool> pattern1, Func<DbExpression, bool> pattern2)
        {
            return (e => pattern1(e) && pattern2(e));
        }

        /// <summary>
        /// Constructs a new pattern that is matched iff all of <paramref name="pattern1"/>, <paramref name="pattern2"/> and <paramref name="pattern3"/> are matched. Does NOT return a pattern that matches <see cref="DbAndExpression"/>. Use <see cref="MatchKind"/> with an argument of <see cref="DbExpressionKind.And"/> to match an AND expression
        /// </summary>        
        internal static Func<DbExpression, bool> And(Func<DbExpression, bool> pattern1, Func<DbExpression, bool> pattern2, Func<DbExpression, bool> pattern3)
        {
            return (e => pattern1(e) && pattern2(e) && pattern3(e));
        }

        /// <summary>
        /// Constructs a new pattern that is matched if either <paramref name="pattern1"/> or <paramref name="pattern2"/> are matched. Does NOT return a pattern that matches <see cref="DbOrExpression"/>. Use <see cref="MatchKind"/> with an argument of <see cref="DbExpressionKind.Or"/> to match an OR expression
        /// </summary>
        internal static Func<DbExpression, bool> Or(Func<DbExpression, bool> pattern1, Func<DbExpression, bool> pattern2)
        {
            return (e => pattern1(e) || pattern2(e));
        }

        /// <summary>
        /// Constructs a new pattern that is matched if either <paramref name="pattern1"/>, <paramref name="pattern2"/> or <paramref name="pattern3"/> are matched. Does NOT return a pattern that matches <see cref="DbOrExpression"/>. Use <see cref="MatchKind"/> with an argument of <see cref="DbExpressionKind.Or"/> to match an OR expression
        /// </summary>
        internal static Func<DbExpression, bool> Or(Func<DbExpression, bool> pattern1, Func<DbExpression, bool> pattern2, Func<DbExpression, bool> pattern3)
        {
            return (e => pattern1(e) || pattern2(e) || pattern3(e));
        }

#if _ENABLE_UNUSED_PATTERNS_
        /// <summary>
        /// Constructs a new pattern that is matched iff the argument pattern is not matched. Does NOT return a pattern that matches <see cref="DbNotExpression"/>. Use <see cref="MatchKind"/> with an argument of <see cref="DbExpressionKind.Not"/> to match a NOT expression
        /// </summary>
        internal static Func<DbExpression, bool> Not(Func<DbExpression, bool> pattern)
        {
            return (e => !pattern(e));
        }
#endif
        #endregion

        #region Constant Patterns

        /// <summary>
        /// Returns a pattern that will match any expression, returning <c>true</c> for any argument, including null.
        /// </summary>
        internal static Func<DbExpression, bool> AnyExpression { get { return (e => true); } }

        /// <summary>
        /// Returns a pattern that will match any collection of expressions, returning <c>true</c> for any argument, including a null or empty enumerable.
        /// </summary>
        internal static Func<IEnumerable<DbExpression>, bool> AnyExpressions { get { return (elems => true); } }

        #endregion

        #region Result Type Patterns

#if _ENABLE_UNUSED_PATTERNS_
        /// <summary>
        /// Returns a pattern that is matched if the the argument has a Boolean result type
        /// </summary>
        internal static Func<DbExpression, bool> MatchBooleanType { get { return (e => TypeSemantics.IsBooleanType(e.ResultType)); } }
#endif
        /// <summary>
        /// Returns a pattern that is matched if the argument has a complex result type
        /// </summary>
        internal static Func<DbExpression, bool> MatchComplexType { get { return (e => TypeSemantics.IsComplexType(e.ResultType)); } }

        /// <summary>
        /// Returns a pattern that is matched if the argument has an entity result type
        /// </summary>
        internal static Func<DbExpression, bool> MatchEntityType { get { return (e => TypeSemantics.IsEntityType(e.ResultType)); } }

        /// <summary>
        /// Returns a pattern that is matched if the argument has a row result type
        /// </summary>
        internal static Func<DbExpression, bool> MatchRowType { get { return (e => TypeSemantics.IsRowType(e.ResultType)); } }

        #endregion

        #region General Patterns

        /// <summary>
        /// Constructs a new pattern that will match an expression with the specified <see cref="DbExpressionKind"/>.
        /// </summary>
        internal static Func<DbExpression, bool> MatchKind(DbExpressionKind kindToMatch)
        {
            return (e => e.ExpressionKind == kindToMatch);
        }

        /// <summary>
        /// Constructs a new pattern that will match iff the specified pattern argument is matched for all expressions in the collection argument.
        /// </summary>
        internal static Func<IEnumerable<DbExpression>, bool> MatchForAll(Func<DbExpression, bool> elementPattern)
        {
            return (elems => elems.FirstOrDefault(e => !elementPattern(e)) == null);
        }

#if _ENABLE_UNUSED_PATTERNS_
        /// <summary>
        /// Constructs a new pattern that will match if the specified pattern argument is matched for any expression in the collection argument.
        /// </summary>
        internal static Func<IEnumerable<DbExpression>, bool> MatchForAny(Func<DbExpression, bool> elementPattern)
        {
            return (elems => elems.FirstOrDefault(e => elementPattern(e)) != null);
        }
#endif
        #endregion

        #region Type-specific Patterns
#if _ENABLE_UNUSED_PATTERNS_
        /// <summary>
        /// Returns a pattern that is matched if the argument expression is a <see cref="DbUnaryExpression"/>
        /// </summary>
        internal static Func<DbExpression, bool> MatchUnary()
        {
            return (e => e is DbUnaryExpression);
        }

        /// <summary>
        /// Constructs a new pattern that is matched iff the argument expression is a <see cref="DbUnaryExpression"/> and matches <paramref name="argumentPattern"/>
        /// </summary>
        internal static Func<DbExpression, bool> MatchUnary(Func<DbExpression, bool> argumentPattern)
        {
            return (e => (e is DbUnaryExpression) && argumentPattern(((DbUnaryExpression)e).Argument));
        }
#endif
        /// <summary>
        /// Returns a pattern that is matched if the argument expression is a <see cref="DbBinaryExpression"/>
        /// </summary>
        internal static Func<DbExpression, bool> MatchBinary()
        {
            return (e => e is DbBinaryExpression);
        }
#if _ENABLE_UNUSED_PATTERNS_
        /// <summary>
        /// Constructs a new pattern that is matched iff the argument expression is a <see cref="DbBinaryExpression"/> with left and right subexpressions that match the corresponding <paramref name="leftPattern"/> and <paramref name="rightPattern"/> patterns
        /// </summary>
        internal static Func<DbExpression, bool> MatchBinary(Func<DbExpression, bool> leftPattern, Func<DbExpression, bool> rightPattern)
        {
            return (e => { DbBinaryExpression binEx = (e as DbBinaryExpression); return (binEx != null && leftPattern(binEx.Left) && rightPattern(binEx.Right)); });
        }
#endif

        /// <summary>
        /// Constructs a new pattern that is matched iff the argument expression is a <see cref="DbFilterExpression"/> with input and predicate subexpressions that match the corresponding <paramref name="inputPattern"/> and <paramref name="predicatePattern"/> patterns
        /// </summary>
        internal static Func<DbExpression, bool> MatchFilter(Func<DbExpression, bool> inputPattern, Func<DbExpression, bool> predicatePattern)
        {
            return (e => { if (e.ExpressionKind != DbExpressionKind.Filter) { return false; } else { DbFilterExpression filterEx = (DbFilterExpression)e; return inputPattern(filterEx.Input.Expression) && predicatePattern(filterEx.Predicate); } });
        }

        /// <summary>
        /// Constructs a new pattern that is matched iff the argument expression is a <see cref="DbProjectExpression"/> with input and projection subexpressions that match the corresponding <paramref name="inputPattern"/> and <paramref name="projectionPattern"/> patterns
        /// </summary>
        internal static Func<DbExpression, bool> MatchProject(Func<DbExpression, bool> inputPattern, Func<DbExpression, bool> projectionPattern)
        {
            return (e => { if (e.ExpressionKind != DbExpressionKind.Project) { return false; } else { DbProjectExpression projectEx = (DbProjectExpression)e; return inputPattern(projectEx.Input.Expression) && projectionPattern(projectEx.Projection); } });
        }

        /// <summary>
        /// Constructs a new pattern that is matched iff the argument expression is a <see cref="DbCaseExpression"/> with 'when' and 'then' subexpression lists that match the specified <paramref name="whenPattern"/> and <paramref name="thenPattern"/> collection patterns and an 'else' subexpression that matches the specified <paramref name="elsePattern"/> expression pattern
        /// </summary>
        internal static Func<DbExpression, bool> MatchCase(Func<IEnumerable<DbExpression>, bool> whenPattern, Func<IEnumerable<DbExpression>, bool> thenPattern, Func<DbExpression, bool> elsePattern)
        {
            return (e => { if (e.ExpressionKind != DbExpressionKind.Case) { return false; } else { DbCaseExpression caseEx = (DbCaseExpression)e; return whenPattern(caseEx.When) && thenPattern(caseEx.Then) && elsePattern(caseEx.Else); } });
        }

        /// <summary>
        /// Gets a pattern that is matched if the argument expression is a <see cref="DbCaseExpression"/>. This property can be used instead of repeated calls to <see cref="MatchKind"/> with an argument of <see cref="DbExpressionKind.Case"/>
        /// </summary>
        internal static Func<DbExpression, bool> MatchNewInstance() { return (e => e.ExpressionKind == DbExpressionKind.NewInstance); }

        /// <summary>
        /// Constructs a new pattern that is matched iff the argument expression is a <see cref="DbNewInstanceExpression"/> with arguments that match the specified collection pattern
        /// </summary>
        internal static Func<DbExpression, bool> MatchNewInstance(Func<IEnumerable<DbExpression>, bool> argumentsPattern)
        {
            return (e => { if (e.ExpressionKind != DbExpressionKind.NewInstance) { return false; } else { DbNewInstanceExpression newInst = (DbNewInstanceExpression)e; return argumentsPattern(newInst.Arguments); } });
        }

        #endregion
    }
}
