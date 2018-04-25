// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Workflow.ComponentModel;

namespace System.Workflow.Activities.Rules
{
    internal class Tracer
    {
        private string tracePrefix;

        // get the localized trace messages once
        private static string traceRuleIdentifier = Messages.TraceRuleIdentifier;
        private static string traceRuleHeader = Messages.TraceRuleHeader;
        private static string traceRuleSetEvaluate = Messages.TraceRuleSetEvaluate;
        private static string traceRuleEvaluate = Messages.TraceRuleEvaluate;
        private static string traceRuleResult = Messages.TraceRuleResult;
        private static string traceRuleActions = Messages.TraceRuleActions;
        private static string traceCondition = Messages.Condition;
        private static string traceThen = Messages.Then;
        private static string traceElse = Messages.Else;
        private static string traceUpdate = Messages.TraceUpdate;
        private static string traceRuleTriggers = Messages.TraceRuleTriggers;
        private static string traceRuleConditionDependency = Messages.TraceRuleConditionDependency;
        private static string traceRuleActionSideEffect = Messages.TraceRuleActionSideEffect;

        internal Tracer(string name, ActivityExecutionContext activityExecutionContext)
        {
            if (activityExecutionContext != null)
                tracePrefix = string.Format(CultureInfo.CurrentCulture, traceRuleIdentifier, name, activityExecutionContext.ContextGuid.ToString());
            else
                tracePrefix = string.Format(CultureInfo.CurrentCulture, traceRuleHeader, name);
        }

        internal void StartRuleSet()
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Information, 0, traceRuleSetEvaluate, tracePrefix);
        }

        internal void StartRule(string ruleName)
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, traceRuleEvaluate, tracePrefix, ruleName);
        }

        internal void RuleResult(string ruleName, bool result)
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Information, 0, traceRuleResult, tracePrefix, ruleName, result.ToString());
        }

        internal void StartActions(string ruleName, bool result)
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, traceRuleActions, tracePrefix,
                (result ? traceThen : traceElse), ruleName);
        }

        internal void TraceUpdate(string ruleName, string otherName)
        {
            WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, traceUpdate, tracePrefix, ruleName, otherName);
        }

        internal void TraceConditionSymbols(string ruleName, ICollection<string> symbols)
        {
            TraceRuleSymbols(traceRuleConditionDependency, traceCondition, ruleName, symbols);
        }

        internal void TraceThenSymbols(string ruleName, ICollection<string> symbols)
        {
            TraceRuleSymbols(traceRuleActionSideEffect, traceThen, ruleName, symbols);
        }

        internal void TraceElseSymbols(string ruleName, ICollection<string> symbols)
        {
            TraceRuleSymbols(traceRuleActionSideEffect, traceElse, ruleName, symbols);
        }

        private void TraceRuleSymbols(string message, string clause, string ruleName, ICollection<string> symbols)
        {
            foreach (string symbol in symbols)
                WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, message, tracePrefix, ruleName, clause, symbol);
        }

        internal void TraceThenTriggers(string currentRuleName, ICollection<int> triggeredRules, List<RuleState> ruleStates)
        {
            TraceRuleTriggers(traceThen, currentRuleName, triggeredRules, ruleStates);
        }

        internal void TraceElseTriggers(string currentRuleName, ICollection<int> triggeredRules, List<RuleState> ruleStates)
        {
            TraceRuleTriggers(traceElse, currentRuleName, triggeredRules, ruleStates);
        }

        private void TraceRuleTriggers(string thenOrElse, string currentRuleName, ICollection<int> triggeredRules, List<RuleState> ruleStates)
        {
            foreach (int r in triggeredRules)
                WorkflowActivityTrace.Rules.TraceEvent(TraceEventType.Verbose, 0, traceRuleTriggers, tracePrefix, currentRuleName, thenOrElse, ruleStates[r].Rule.Name);
        }
    }
}
