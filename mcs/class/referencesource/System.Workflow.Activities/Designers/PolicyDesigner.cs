using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.CodeDom;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.Activities.Rules;
using System.Globalization;
using System.Windows.Forms.Design;
using System.Workflow.Activities.Rules.Design;
using System.Windows.Forms;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities
{
    [ActivityDesignerTheme(typeof(PolicyDesignerTheme))]
    internal sealed class PolicyDesigner : ActivityDesigner, IServiceProvider
    {
        new public object GetService(Type type)
        {
            return base.GetService(type);
        }

        protected override void DoDefaultAction()
        {
            base.DoDefaultAction();

            // Do not allow editing if in debug mode.
            WorkflowDesignerLoader workflowDesignerLoader = this.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (workflowDesignerLoader != null && workflowDesignerLoader.InDebugMode)
                throw new InvalidOperationException(Messages.DebugModeEditsDisallowed);

            // Do not allow editing if locked
            PolicyActivity activity = (PolicyActivity)this.Activity;
            if (Helpers.IsActivityLocked(activity))
                return;

            RuleDefinitions rules = ConditionHelper.Load_Rules_DT(this, Helpers.GetRootActivity(activity));
            if (rules != null)
            {
                RuleSetCollection ruleSetCollection = rules.RuleSets;
                RuleSetReference ruleSetReference = activity.RuleSetReference;
                RuleSet ruleSet = null;
                string ruleSetName = null;
                if (ruleSetReference != null
                    && !string.IsNullOrEmpty(ruleSetReference.RuleSetName))
                {
                    ruleSetName = ruleSetReference.RuleSetName;
                    if (ruleSetCollection.Contains(ruleSetName))
                    {
                        ruleSet = ruleSetCollection[ruleSetName];
                    }
                }
                else
                {
                    ruleSetName = ruleSetCollection.GenerateRuleSetName();
                }
                using (RuleSetDialog dlg = new RuleSetDialog(activity, ruleSet))
                {
                    if (DialogResult.OK == dlg.ShowDialog())
                    {
                        if (ruleSet != null) // modifying
                        {
                            ruleSetCollection.Remove(ruleSetName);
                        }
                        else // creating
                        {
                            dlg.RuleSet.Name = ruleSetName;
                            activity.RuleSetReference = new RuleSetReference(ruleSetName);
                        }
                        ruleSetCollection.Add(dlg.RuleSet);
                        ConditionHelper.Flush_Rules_DT(this, Helpers.GetRootActivity(activity));
                    }
                }
            }

            // force revalidation by setting a property
            TypeDescriptor.GetProperties(activity)["RuleSetReference"].SetValue(activity, activity.RuleSetReference);
        }
    }


    internal sealed class PolicyDesignerTheme : ActivityDesignerTheme
    {
        public PolicyDesignerTheme(WorkflowTheme theme)
            : base(theme)
        {
            this.ForeColor = Color.FromArgb(0xFF, 0x00, 0x00, 0x00);
            this.BorderColor = Color.FromArgb(0xFF, 0x80, 0x80, 0x80);
            this.BorderStyle = DashStyle.Solid;
            this.BackColorStart = Color.FromArgb(0xFF, 0xF4, 0xF4, 0xF4);
            this.BackColorEnd = Color.FromArgb(0xFF, 0xC0, 0xC0, 0xC0);
            this.BackgroundStyle = LinearGradientMode.Horizontal;
        }
    }
}
