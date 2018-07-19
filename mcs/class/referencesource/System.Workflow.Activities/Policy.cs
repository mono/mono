namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Collections.Generic;
    using System.Workflow.Activities.Rules;
    using System.Workflow.Activities.Rules.Design;
    using System.Workflow.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Drawing.Design;
    using System.Workflow.Activities.Common;

    #region Class Policy

    [SRDescription(SR.PolicyActivityDescription)]
    [ToolboxBitmap(typeof(PolicyActivity), "Resources.Rule.png")]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(PolicyDesigner), typeof(IDesigner))]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class PolicyActivity : Activity
    {
        #region Public Dependency Properties

        public static readonly DependencyProperty RuleSetReferenceProperty = DependencyProperty.Register("RuleSetReference", typeof(RuleSetReference), typeof(PolicyActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata, new Attribute[] { new ValidationOptionAttribute(ValidationOption.Required) }));

        #endregion

        #region Constructors

        public PolicyActivity()
        {
        }

        public PolicyActivity(string name)
            : base(name)
        {
        }

        #endregion

        protected override void Initialize(IServiceProvider provider)
        {
            // if there is no parent, then there will be no validation of RuleSetReference
            // as well, there will be no RuleDefinitions
            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            base.Initialize(provider);
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            Activity declaringActivity = Helpers.GetDeclaringActivity(this);
            if (declaringActivity == null)
                declaringActivity = Helpers.GetRootActivity(this);

            RuleDefinitions ruleDefinitions = (RuleDefinitions)declaringActivity.GetValue(RuleDefinitions.RuleDefinitionsProperty);
            if (ruleDefinitions != null)
            {
                RuleSet ruleSet = ruleDefinitions.RuleSets[this.RuleSetReference.RuleSetName];
                if (ruleSet != null)
                {
                    ruleSet.Execute(declaringActivity, executionContext);
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        [SRDescription(SR.RuleSetDescription)]
        [MergableProperty(false)]
        public RuleSetReference RuleSetReference
        {
            get
            {
                return (RuleSetReference)base.GetValue(RuleSetReferenceProperty);
            }
            set
            {
                base.SetValue(RuleSetReferenceProperty, value);
            }
        }
    }
    #endregion
}
