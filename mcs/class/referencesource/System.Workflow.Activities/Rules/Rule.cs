// ---------------------------------------------------------------------------
// Copyright (C) 2005 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.ComponentModel;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules
{
    public enum RuleReevaluationBehavior
    {
        Never,
        Always
    };

    [Serializable]
    public class Rule
    {
        internal string name;
        internal string description;
        internal int priority;
        internal RuleReevaluationBehavior behavior = RuleReevaluationBehavior.Always;
        internal bool active = true;
        internal RuleCondition condition;
        internal IList<RuleAction> thenActions;
        internal IList<RuleAction> elseActions;
        private bool runtimeInitialized;

        public Rule()
        {
        }

        public Rule(string name)
        {
            this.name = name;
        }

        public Rule(string name, RuleCondition condition, IList<RuleAction> thenActions)
        {
            this.name = name;
            this.condition = condition;
            this.thenActions = thenActions;
        }

        public Rule(string name, RuleCondition condition, IList<RuleAction> thenActions, IList<RuleAction> elseActions)
        {
            this.name = name;
            this.condition = condition;
            this.thenActions = thenActions;
            this.elseActions = elseActions;
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

        public int Priority
        {
            get { return priority; }
            set
            {
                if (runtimeInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));
                priority = value;
            }
        }

        public RuleReevaluationBehavior ReevaluationBehavior
        {
            get { return behavior; }
            set
            {
                if (runtimeInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));
                behavior = value;
            }
        }

        public bool Active
        {
            get { return active; }
            set
            {
                if (runtimeInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));
                active = value;
            }
        }

        public RuleCondition Condition
        {
            get { return condition; }
            set
            {
                if (runtimeInitialized)
                    throw new InvalidOperationException(SR.GetString(SR.Error_CanNotChangeAtRuntime));
                condition = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList<RuleAction> ThenActions
        {
            get { if (thenActions == null) thenActions = new List<RuleAction>(); return thenActions; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public IList<RuleAction> ElseActions
        {
            get { if (elseActions == null) elseActions = new List<RuleAction>(); return elseActions; }
        }

        internal void Validate(RuleValidation validation)
        {
            int oldErrorCount = validation.Errors.Count;

            if (string.IsNullOrEmpty(name))
                validation.Errors.Add(new ValidationError(Messages.RuleNameMissing, ErrorNumbers.Error_InvalidConditionName));

            // check the condition
            if (condition == null)
                validation.Errors.Add(new ValidationError(Messages.MissingRuleCondition, ErrorNumbers.Error_MissingRuleCondition));
            else
                condition.Validate(validation);

            // check the optional then actions
            if (thenActions != null)
                ValidateRuleActions(thenActions, validation);

            // check the optional else actions
            if (elseActions != null)
                ValidateRuleActions(elseActions, validation);

            // fix up the error messages by prepending the rule name
            ValidationErrorCollection errors = validation.Errors;
            if (errors.Count > oldErrorCount)
            {
                string prefix = string.Format(CultureInfo.CurrentCulture, Messages.RuleValidationError, name);

                int newErrorCount = errors.Count;
                for (int i = oldErrorCount; i < newErrorCount; ++i)
                {
                    ValidationError oldError = errors[i];

                    ValidationError newError = new ValidationError(prefix + oldError.ErrorText, oldError.ErrorNumber, oldError.IsWarning);
                    foreach (DictionaryEntry de in oldError.UserData)
                        newError.UserData[de.Key] = de.Value;

                    errors[i] = newError;
                }
            }
        }

        private static void ValidateRuleActions(ICollection<RuleAction> ruleActions, RuleValidation validator)
        {
            bool seenHalt = false;
            bool statementsAfterHalt = false;
            foreach (RuleAction action in ruleActions)
            {
                action.Validate(validator);
                if (seenHalt)
                    statementsAfterHalt = true;
                if (action is RuleHaltAction)
                    seenHalt = true;
            }

            if (statementsAfterHalt)
            {
                // one or more actions after Halt
                validator.Errors.Add(new ValidationError(Messages.UnreachableCodeHalt, ErrorNumbers.Warning_UnreachableCode, true));
            }
        }

        public Rule Clone()
        {
            Rule newRule = (Rule)this.MemberwiseClone();
            newRule.runtimeInitialized = false;

            if (this.condition != null)
                newRule.condition = this.condition.Clone();

            if (this.thenActions != null)
            {
                newRule.thenActions = new List<RuleAction>();
                foreach (RuleAction thenAction in this.thenActions)
                    newRule.thenActions.Add(thenAction.Clone());
            }

            if (this.elseActions != null)
            {
                newRule.elseActions = new List<RuleAction>();
                foreach (RuleAction elseAction in this.elseActions)
                    newRule.elseActions.Add(elseAction.Clone());
            }

            return newRule;
        }

        public override bool Equals(object obj)
        {
            Rule other = obj as Rule;
            if (other == null)
                return false;
            if ((this.Name != other.Name)
                || (this.Description != other.Description)
                || (this.Active != other.Active)
                || (this.ReevaluationBehavior != other.ReevaluationBehavior)
                || (this.Priority != other.Priority))
                return false;
            // look similar, compare condition (can be null)
            if (this.Condition == null)
            {
                if (other.Condition != null)
                    return false;
            }
            else
            {
                if (!this.Condition.Equals(other.Condition))
                    return false;
            }
            // compare ThenActions (may be null)
            if (!ActionsEqual(this.thenActions, other.thenActions))
                return false;
            if (!ActionsEqual(this.elseActions, other.elseActions))
                return false;
            return true;
        }

        private static bool ActionsEqual(IList<RuleAction> myActions, IList<RuleAction> otherActions)
        {
            if ((myActions == null) && (otherActions == null))
                return true;
            if ((myActions == null) || (otherActions == null))
                return false;
            if (myActions.Count != otherActions.Count)
                return false;
            for (int i = 0; i < myActions.Count; ++i)
            {
                if (!myActions[i].Equals(otherActions[i]))
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
            runtimeInitialized = true;
        }
    }
}
