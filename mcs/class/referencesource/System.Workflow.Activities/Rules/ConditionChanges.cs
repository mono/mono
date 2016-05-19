using System;
using System.Collections;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Diagnostics;
using System.Globalization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;

namespace System.Workflow.Activities.Rules
{
    #region ConditionChangeAction
    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    public abstract class RuleConditionChangeAction : WorkflowChangeAction
    {
        public abstract string ConditionName { get; }

        protected override ValidationErrorCollection ValidateChanges(Activity activity)
        {
            // No validations required.
            return new ValidationErrorCollection();
        }
    }
    #endregion

    #region RuleSetChangeAction
    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    public abstract class RuleSetChangeAction : WorkflowChangeAction
    {
        public abstract string RuleSetName { get; }

        protected override ValidationErrorCollection ValidateChanges(Activity activity)
        {
            // No validations can be done since we don't know the context the policy 
            // will execute in (i.e. no idea what the "this" object will be)
            return new ValidationErrorCollection();
        }
    }
    #endregion

    #region AddedConditionAction
    public sealed class AddedConditionAction : RuleConditionChangeAction
    {
        private RuleCondition _conditionDefinition;

        public AddedConditionAction(RuleCondition addedConditionDefinition)
        {
            if (null == addedConditionDefinition)
                throw new ArgumentNullException("addedConditionDefinition");

            _conditionDefinition = addedConditionDefinition;
        }

        public AddedConditionAction()
        {
        }

        public override string ConditionName
        {
            get { return _conditionDefinition.Name; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleCondition ConditionDefinition
        {
            get
            {
                return this._conditionDefinition;
            }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");

                this._conditionDefinition = value;
            }
        }

        protected override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
                return false;

            RuleDefinitions rules = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (rules == null)
            {
                rules = new RuleDefinitions();
                ((Activity)rootActivity).SetValue(RuleDefinitions.RuleDefinitionsProperty, rules);
            }

            // 
            bool setRuntimeMode = false;
            if (rules.Conditions.RuntimeMode)
            {
                rules.Conditions.RuntimeMode = false;
                setRuntimeMode = true;
            }
            try
            {
                rules.Conditions.Add(this.ConditionDefinition);
            }
            finally
            {
                if (setRuntimeMode)
                    rules.Conditions.RuntimeMode = true;
            }
            return true;
        }
    }
    #endregion

    #region RemovedConditionAction
    public sealed class RemovedConditionAction : RuleConditionChangeAction
    {
        private RuleCondition _conditionDefinition;

        public RemovedConditionAction(RuleCondition removedConditionDefinition)
        {
            if (null == removedConditionDefinition)
                throw new ArgumentNullException("removedConditionDefinition");

            _conditionDefinition = removedConditionDefinition;
        }
        public RemovedConditionAction()
        {
        }

        public override string ConditionName
        {
            get { return _conditionDefinition.Name; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleCondition ConditionDefinition
        {
            get
            {
                return this._conditionDefinition;
            }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");

                this._conditionDefinition = value;
            }
        }

        protected override bool ApplyTo(Activity rootActivity)
        {

            if (rootActivity == null)
                return false;

            RuleDefinitions rules = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (rules == null || rules.Conditions == null)
                return false;

            // 
            bool setRuntimeMode = false;
            if (rules.Conditions.RuntimeMode)
            {
                rules.Conditions.RuntimeMode = false;
                setRuntimeMode = true;
            }
            try
            {
                return rules.Conditions.Remove(this.ConditionDefinition.Name);
            }
            finally
            {
                if (setRuntimeMode)
                    rules.Conditions.RuntimeMode = true;
            }
        }
    }
    #endregion

    #region UpdatedConditionAction
    public sealed class UpdatedConditionAction : RuleConditionChangeAction
    {
        private RuleCondition _conditionDefinition;
        private RuleCondition _newConditionDefinition;

        public UpdatedConditionAction(RuleCondition conditionDefinition, RuleCondition newConditionDefinition)
        {
            if (null == conditionDefinition)
                throw new ArgumentNullException("conditionDefinition");
            if (null == newConditionDefinition)
                throw new ArgumentNullException("newConditionDefinition");

            if (newConditionDefinition.Name != conditionDefinition.Name)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.ConditionNameNotIdentical, newConditionDefinition.Name, conditionDefinition.Name);
                throw new ArgumentException(message);
            }

            _conditionDefinition = conditionDefinition;
            _newConditionDefinition = newConditionDefinition;
        }
        public UpdatedConditionAction()
        {
        }

        public override string ConditionName
        {
            get { return _conditionDefinition.Name; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleCondition ConditionDefinition
        {
            get
            {
                return this._conditionDefinition;
            }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");

                this._conditionDefinition = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleCondition NewConditionDefinition
        {
            get
            {
                return this._newConditionDefinition;
            }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");

                this._newConditionDefinition = value;
            }
        }

        protected override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
                return false;

            RuleDefinitions rules = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (rules == null || rules.Conditions == null)
                return false;

            if (rules.Conditions[this.ConditionDefinition.Name] == null)
                return false;

            // 
            bool setRuntimeMode = false;
            if (rules.Conditions.RuntimeMode)
            {
                rules.Conditions.RuntimeMode = false;
                setRuntimeMode = true;
            }
            try
            {
                rules.Conditions.Remove(this.ConditionDefinition.Name);
                rules.Conditions.Add(this.NewConditionDefinition);
            }
            finally
            {
                if (setRuntimeMode)
                    rules.Conditions.RuntimeMode = true;
            }
            return true;
        }
    }
    #endregion

    #region AddedRuleSetAction
    public sealed class AddedRuleSetAction : RuleSetChangeAction
    {
        private RuleSet ruleset;

        public AddedRuleSetAction(RuleSet addedRuleSetDefinition)
        {
            if (addedRuleSetDefinition == null)
                throw new ArgumentNullException("addedRuleSetDefinition");
            ruleset = addedRuleSetDefinition;
        }

        public AddedRuleSetAction()
        {
        }

        public override string RuleSetName
        {
            get { return ruleset.Name; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleSet RuleSetDefinition
        {
            get { return ruleset; }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");
                ruleset = value;
            }
        }

        protected override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
                return false;

            RuleDefinitions rules = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (rules == null)
            {
                rules = new RuleDefinitions();
                ((Activity)rootActivity).SetValue(RuleDefinitions.RuleDefinitionsProperty, rules);
            }

            // 
            bool setRuntimeMode = false;
            if (rules.RuleSets.RuntimeMode)
            {
                rules.RuleSets.RuntimeMode = false;
                setRuntimeMode = true;
            }
            try
            {
                rules.RuleSets.Add(ruleset);
            }
            finally
            {
                if (setRuntimeMode)
                    rules.RuleSets.RuntimeMode = true;
            }
            return true;
        }
    }
    #endregion

    #region RemovedRuleSetAction
    public sealed class RemovedRuleSetAction : RuleSetChangeAction
    {
        private RuleSet ruleset;

        public RemovedRuleSetAction(RuleSet removedRuleSetDefinition)
        {
            if (removedRuleSetDefinition == null)
                throw new ArgumentNullException("removedRuleSetDefinition");
            ruleset = removedRuleSetDefinition;
        }

        public RemovedRuleSetAction()
        {
        }

        public override string RuleSetName
        {
            get { return ruleset.Name; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleSet RuleSetDefinition
        {
            get { return ruleset; }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");
                ruleset = value;
            }
        }

        protected override bool ApplyTo(Activity rootActivity)
        {

            if (rootActivity == null)
                return false;

            RuleDefinitions rules = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (rules == null || rules.RuleSets == null)
                return false;

            // 
            bool setRuntimeMode = false;
            if (rules.RuleSets.RuntimeMode)
            {
                rules.RuleSets.RuntimeMode = false;
                setRuntimeMode = true;
            }
            try
            {
                return rules.RuleSets.Remove(ruleset.Name);
            }
            finally
            {
                if (setRuntimeMode)
                    rules.RuleSets.RuntimeMode = true;
            }
        }
    }
    #endregion

    #region UpdatedRuleSetAction
    public sealed class UpdatedRuleSetAction : RuleSetChangeAction
    {
        private RuleSet original;
        private RuleSet updated;

        public UpdatedRuleSetAction(RuleSet originalRuleSetDefinition, RuleSet updatedRuleSetDefinition)
        {
            if (originalRuleSetDefinition == null)
                throw new ArgumentNullException("originalRuleSetDefinition");
            if (updatedRuleSetDefinition == null)
                throw new ArgumentNullException("updatedRuleSetDefinition");

            if (originalRuleSetDefinition.Name != updatedRuleSetDefinition.Name)
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.ConditionNameNotIdentical, originalRuleSetDefinition.Name, updatedRuleSetDefinition.Name);
                throw new ArgumentException(message);
            }
            original = originalRuleSetDefinition;
            updated = updatedRuleSetDefinition;
        }

        public UpdatedRuleSetAction()
        {
        }

        public override string RuleSetName
        {
            get { return original.Name; }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleSet OriginalRuleSetDefinition
        {
            get { return original; }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");
                original = value;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleSet UpdatedRuleSetDefinition
        {
            get { return updated; }
            set
            {
                if (null == value)
                    throw new ArgumentNullException("value");
                updated = value;
            }
        }

        protected override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
                return false;

            RuleDefinitions rules = rootActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (rules == null || rules.RuleSets == null)
                return false;

            if (rules.RuleSets[RuleSetName] == null)
                return false;

            // 
            bool setRuntimeMode = false;
            if (rules.Conditions.RuntimeMode)
            {
                rules.Conditions.RuntimeMode = false;
                setRuntimeMode = true;
            }
            try
            {
                rules.RuleSets.Remove(RuleSetName);
                rules.RuleSets.Add(updated);
            }
            finally
            {
                if (setRuntimeMode)
                    rules.RuleSets.RuntimeMode = true;
            }
            return true;
        }
    }
    #endregion
}
