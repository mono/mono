// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Workflow.ComponentModel;

namespace System.Workflow.Activities.Rules
{
    public class RuleEngine
    {
        private string name;
        private RuleValidation validation;
        private IList<RuleState> analyzedRules;

        public RuleEngine(RuleSet ruleSet, RuleValidation validation)
            : this(ruleSet, validation, null)
        {
        }

        public RuleEngine(RuleSet ruleSet, Type objectType)
            : this(ruleSet, new RuleValidation(objectType, null), null)
        {
        }

        internal RuleEngine(RuleSet ruleSet, RuleValidation validation, ActivityExecutionContext executionContext)
        {
            // now validate it
            if (!ruleSet.Validate(validation))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.RuleSetValidationFailed, ruleSet.name);
                throw new RuleSetValidationException(message, validation.Errors);
            }

            this.name = ruleSet.Name;
            this.validation = validation;
            Tracer tracer = null;
            if (WorkflowActivityTrace.Rules.Switch.ShouldTrace(TraceEventType.Information))
                tracer = new Tracer(ruleSet.Name, executionContext);
            this.analyzedRules = Executor.Preprocess(ruleSet.ChainingBehavior, ruleSet.Rules, validation, tracer);
        }


        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
        public void Execute(object thisObject, ActivityExecutionContext executionContext)
        {
            Execute(new RuleExecution(validation, thisObject, executionContext));
        }


        [SuppressMessage("Microsoft.Naming", "CA1720:AvoidTypeNamesInParameters", MessageId = "0#")]
        public void Execute(object thisObject)
        {
            Execute(new RuleExecution(validation, thisObject, null));
        }

        internal void Execute(RuleExecution ruleExecution)
        {
            Tracer tracer = null;
            if (WorkflowActivityTrace.Rules.Switch.ShouldTrace(TraceEventType.Information))
            {
                tracer = new Tracer(name, ruleExecution.ActivityExecutionContext);
                tracer.StartRuleSet();
            }
            Executor.ExecuteRuleSet(analyzedRules, ruleExecution, tracer, RuleSet.RuleSetTrackingKey + name);
        }
    }
}
