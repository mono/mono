using System;
using System.CodeDom;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Serialization;
using System.Text;

namespace System.Workflow.Activities.Rules.Design
{
    /// <summary>
    /// Summary description for DesignerHelpers.
    /// </summary>
    internal static class DesignerHelpers
    {
        internal static void DisplayError(string message, string messageBoxTitle, IServiceProvider serviceProvider)
        {
            IUIService uis = null;
            if (serviceProvider != null)
                uis = (IUIService)serviceProvider.GetService(typeof(IUIService));

            if (uis != null)
                uis.ShowError(message);
            else
                MessageBox.Show(message, messageBoxTitle, MessageBoxButtons.OK, MessageBoxIcon.Exclamation, MessageBoxDefaultButton.Button1, 0);
        }

        static internal string GetRulePreview(Rule rule)
        {
            StringBuilder rulePreview = new StringBuilder();

            if (rule != null)
            {
                rulePreview.Append("IF ");
                if (rule.Condition != null)
                    rulePreview.Append(rule.Condition.ToString() + " ");
                rulePreview.Append("THEN ");

                foreach (RuleAction action in rule.ThenActions)
                {
                    rulePreview.Append(action.ToString());
                    rulePreview.Append(' ');
                }

                if (rule.ElseActions.Count > 0)
                {
                    rulePreview.Append("ELSE ");
                    foreach (RuleAction action in rule.ElseActions)
                    {
                        rulePreview.Append(action.ToString());
                        rulePreview.Append(' ');
                    }
                }
            }

            return rulePreview.ToString();
        }

        static internal string GetRuleSetPreview(RuleSet ruleSet)
        {
            StringBuilder preview = new StringBuilder();
            bool first = true;
            if (ruleSet != null)
            {
                foreach (Rule rule in ruleSet.Rules)
                {
                    if (first)
                        first = false;
                    else
                        preview.Append("\n");

                    preview.Append(rule.Name);
                    preview.Append(": ");
                    preview.Append(DesignerHelpers.GetRulePreview(rule));
                }
            }

            return preview.ToString();
        }
    }
}
