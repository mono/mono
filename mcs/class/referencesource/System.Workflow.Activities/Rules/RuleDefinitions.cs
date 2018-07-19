// ---------------------------------------------------------------------------
// Copyright (C) 2006 Microsoft Corporation All Rights Reserved
// ---------------------------------------------------------------------------

#define CODE_ANALYSIS
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Workflow.ComponentModel;

namespace System.Workflow.Activities.Rules
{
    #region class RuleDefinitions

    public sealed class RuleDefinitions : IWorkflowChangeDiff
    {

        [SuppressMessage("Microsoft.Security", "CA2104:DoNotDeclareReadOnlyMutableReferenceTypes")]
        public static readonly DependencyProperty RuleDefinitionsProperty = DependencyProperty.RegisterAttached("RuleDefinitions", typeof(RuleDefinitions), typeof(RuleDefinitions), new PropertyMetadata(null, DependencyPropertyOptions.Metadata, new GetValueOverride(OnGetRuleConditions), null, new Attribute[] { new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden) }));

        private RuleConditionCollection conditions;
        private RuleSetCollection ruleSets;
        private bool runtimeInitialized;
        [NonSerialized]
        private object syncLock = new object();

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleConditionCollection Conditions
        {
            get
            {
                if (this.conditions == null)
                    this.conditions = new RuleConditionCollection();
                return conditions;
            }
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public RuleSetCollection RuleSets
        {
            get
            {
                if (this.ruleSets == null)
                    this.ruleSets = new RuleSetCollection();
                return this.ruleSets;
            }
        }

        internal static object OnGetRuleConditions(DependencyObject dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException("dependencyObject");

            RuleDefinitions rules = dependencyObject.GetValueBase(RuleDefinitions.RuleDefinitionsProperty) as RuleDefinitions;
            if (rules != null)
                return rules;

            Activity rootActivity = dependencyObject as Activity;
            if (rootActivity.Parent == null)
            {
                rules = ConditionHelper.GetRuleDefinitionsFromManifest(rootActivity.GetType());
                if (rules != null)
                    dependencyObject.SetValue(RuleDefinitions.RuleDefinitionsProperty, rules);
            }
            return rules;
        }

        internal void OnRuntimeInitialized()
        {
            lock (syncLock)
            {
                if (runtimeInitialized)
                    return;
                Conditions.OnRuntimeInitialized();
                RuleSets.OnRuntimeInitialized();
                runtimeInitialized = true;
            }
        }

        #region IWorkflowChangeDiff Members

        public IList<WorkflowChangeAction> Diff(object originalDefinition, object changedDefinition)
        {
            RuleDefinitions originalRules = originalDefinition as RuleDefinitions;
            RuleDefinitions changedRules = changedDefinition as RuleDefinitions;
            if ((originalRules == null) || (changedRules == null))
                return new List<WorkflowChangeAction>();

            IList<WorkflowChangeAction> cdiff = Conditions.Diff(originalRules.Conditions, changedRules.Conditions);
            IList<WorkflowChangeAction> rdiff = RuleSets.Diff(originalRules.RuleSets, changedRules.RuleSets);

            // quick optimization -- if no condition changes, simply return the ruleset changes
            if (cdiff.Count == 0)
                return rdiff;

            // merge ruleset changes into condition changes
            for (int i = 0; i < rdiff.Count; ++i)
            {
                cdiff.Add(rdiff[i]);
            }
            return cdiff;
        }
        #endregion

        internal RuleDefinitions Clone()
        {
            RuleDefinitions newRuleDefinitions = new RuleDefinitions();

            if (this.ruleSets != null)
            {
                newRuleDefinitions.ruleSets = new RuleSetCollection();
                foreach (RuleSet r in this.ruleSets)
                    newRuleDefinitions.ruleSets.Add(r.Clone());
            }

            if (this.conditions != null)
            {
                newRuleDefinitions.conditions = new RuleConditionCollection();
                foreach (RuleCondition r in this.conditions)
                    newRuleDefinitions.conditions.Add(r.Clone());
            }

            return newRuleDefinitions;
        }
    }
    #endregion
}
