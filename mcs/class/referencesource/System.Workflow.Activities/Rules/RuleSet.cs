// ---------------------------------------------------------------------------
// Copyright (C) 2005 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

using System.Collections.Generic;
using System.ComponentModel;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    public enum RuleChainingBehavior
    {
        None,
        UpdateOnly,
        Full
    };

    [Serializable]
    public class RuleSet
    {
        internal const string RuleSetTrackingKey = "RuleSet.";
        internal string name;
        internal string description;
        internal List<Rule> rules;
        internal RuleChainingBehavior behavior = RuleChainingBehavior.Full;
        private bool runtimeInitialized;
        private object syncLock = new object();

        // keep track of cached data
        [NonSerialized]
        private RuleEngine cachedEngine;
        [NonSerialized]
        private RuleValidation cachedValidation;

        public RuleSet()
        {
            this.rules = new List<Rule>();
        }

        public RuleSet(string name)
            : this()
        {
            this.name = name;
        }

        public RuleSet(string name, string description)
            : this(name)
        {
            this.description = description;
        }

        public string Name
        {
            get { return name; }
            set
            {
                if (runtimeInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));
                name = value;
            }
        }

        public string Description
        {
            get { return description; }
            set
            {
                if (runtimeInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));
                description = value;
            }
        }

        public RuleChainingBehavior ChainingBehavior
        {
            get { return behavior; }
            set
            {
                if (runtimeInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));
                behavior = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ICollection<Rule> Rules
        {
            get { return rules; }
        }

        public bool Validate(RuleValidation validation)
        {
            if (validation == null)
                throw new ArgumentNullException("validation");

            // Validate each rule.
            Dictionary<string, object> ruleNames = new Dictionary<string, object>();
            foreach (Rule r in rules)
            {
                if (!string.IsNullOrEmpty(r.Name))  // invalid names caught when validating the rule
                {
                    if (ruleNames.ContainsKey(r.Name))
                    {
                        // Duplicate rule name found.
                        ValidationError error = new ValidationError(Messages.Error_DuplicateRuleName, ErrorNumbers.Error_DuplicateConditions);
                        error.UserData[RuleUserDataKeys.ErrorObject] = r;
                        validation.AddError(error);
                    }
                    else
                    {
                        ruleNames.Add(r.Name, null);
                    }
                }

                r.Validate(validation);
            }

            if (validation.Errors == null || validation.Errors.Count == 0)
                return true;

            return false;
        }

        public void Execute(RuleExecution ruleExecution)
        {
            // we have no way of knowing if the ruleset has been changed, so no caching done
            if (ruleExecution == null)
                throw new ArgumentNullException("ruleExecution");
            if (ruleExecution.Validation == null)
                throw new ArgumentException(SR.GetString(SR.Error_MissingValidationProperty), "ruleExecution");

            RuleEngine engine = new RuleEngine(this, ruleExecution.Validation, ruleExecution.ActivityExecutionContext);
            engine.Execute(ruleExecution);
        }

        internal void Execute(Activity activity, ActivityExecutionContext executionContext)
        {
            // this can be called from multiple threads if multiple workflows are 
            // running at the same time (only a single workflow is single-threaded)
            // we want to only lock around the validation and preprocessing, so that
            // execution can run in parallel.

            if (activity == null)
                throw new ArgumentNullException("activity");

            Type activityType = activity.GetType();
            RuleEngine engine = null;
            lock (syncLock)
            {
                // do we have something useable cached?
                if ((cachedEngine == null) || (cachedValidation == null) || (cachedValidation.ThisType != activityType))
                {
                    // no cache (or its invalid)
                    RuleValidation validation = new RuleValidation(activityType, null);
                    engine = new RuleEngine(this, validation, executionContext);
                    cachedValidation = validation;
                    cachedEngine = engine;
                }
                else
                {
                    // this will happen if the ruleset has already been processed
                    // we can simply use the previously processed engine
                    engine = cachedEngine;
                }
            }

            // when we get here, we have a local RuleEngine all ready to go
            // we are outside the lock, so these can run in parallel
            engine.Execute(activity, executionContext);
        }

        public RuleSet Clone()
        {
            RuleSet newRuleSet = (RuleSet)this.MemberwiseClone();
            newRuleSet.runtimeInitialized = false;

            if (this.rules != null)
            {
                newRuleSet.rules = new List<Rule>();
                foreach (Rule r in this.rules)
                    newRuleSet.rules.Add(r.Clone());
            }

            return newRuleSet;
        }

        public override bool Equals(object obj)
        {
            RuleSet other = obj as RuleSet;
            if (other == null)
                return false;
            if ((this.Name != other.Name)
                || (this.Description != other.Description)
                || (this.ChainingBehavior != other.ChainingBehavior)
                || (this.Rules.Count != other.Rules.Count))
                return false;
            // look similar, compare each rule
            for (int i = 0; i < this.rules.Count; ++i)
            {
                if (!this.rules[i].Equals(other.rules[i]))
                    return false;
            }
            return true;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        internal void OnRuntimeInitialized()
        {
            lock (syncLock)
            {
                if (runtimeInitialized)
                    return;

                foreach (Rule rule in rules)
                {
                    rule.OnRuntimeInitialized();
                }
                runtimeInitialized = true;
            }
        }
    }
}
