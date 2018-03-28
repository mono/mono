using System;
using System.CodeDom;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Security.Permissions;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Drawing.Design;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Design;
using System.Globalization;
using Microsoft.Win32;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities.Rules.Design
{
    internal sealed class LogicalExpressionEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public LogicalExpressionEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object o)
        {
            if (typeDescriptorContext == null)
                throw new ArgumentNullException("typeDescriptorContext");
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            object returnVal = o;

            // Do not allow editing expression if the name is not set.
            RuleConditionReference conditionDeclaration = typeDescriptorContext.Instance as RuleConditionReference;

            if (conditionDeclaration == null || conditionDeclaration.ConditionName == null || conditionDeclaration.ConditionName.Length <= 0)
                throw new ArgumentException(Messages.ConditionNameNotSet);

            Activity baseActivity = null;

            IReferenceService rs = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            if (rs != null)
                baseActivity = rs.GetComponent(typeDescriptorContext.Instance) as Activity;

            RuleConditionCollection conditionDefinitions = null;
            RuleDefinitions rules = ConditionHelper.Load_Rules_DT(serviceProvider, Helpers.GetRootActivity(baseActivity));
            if (rules != null)
                conditionDefinitions = rules.Conditions;

            if (conditionDefinitions != null && !conditionDefinitions.Contains(conditionDeclaration.ConditionName))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.ConditionNotFound, conditionDeclaration.ConditionName);
                throw new ArgumentException(message);
            }

            this.editorService = (IWindowsFormsEditorService)serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                CodeExpression experssion = typeDescriptorContext.PropertyDescriptor.GetValue(typeDescriptorContext.Instance) as CodeExpression;
                try
                {
                    using (RuleConditionDialog dlg = new RuleConditionDialog(baseActivity, experssion))
                    {
                        if (DialogResult.OK == editorService.ShowDialog(dlg))
                            returnVal = dlg.Expression;
                    }
                }
                catch (NotSupportedException)
                {
                    DesignerHelpers.DisplayError(Messages.Error_ExpressionNotSupported, Messages.ConditionEditor, serviceProvider);
                }
            }

            return returnVal;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }

    internal sealed class ConditionNameEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public ConditionNameEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object o)
        {
            if (typeDescriptorContext == null)
                throw new ArgumentNullException("typeDescriptorContext");
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");

            object returnVal = o;

            this.editorService = (IWindowsFormsEditorService)serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                Activity baseActivity = null;

                IReferenceService rs = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                if (rs != null)
                    baseActivity = rs.GetComponent(typeDescriptorContext.Instance) as Activity;

                string conditionName = typeDescriptorContext.PropertyDescriptor.GetValue(typeDescriptorContext.Instance) as string;
                ConditionBrowserDialog dlg = new ConditionBrowserDialog(baseActivity, conditionName);

                if (DialogResult.OK == editorService.ShowDialog(dlg))
                    returnVal = dlg.SelectedName;
            }

            return returnVal;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }

    #region class RuleSetNameEditor

    internal sealed class RuleSetNameEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public RuleSetNameEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object o)
        {
            if (typeDescriptorContext == null)
                throw new ArgumentNullException("typeDescriptorContext");
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");
            
            object returnVal = o;

            this.editorService = (IWindowsFormsEditorService)serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                Activity baseActivity = null;

                IReferenceService rs = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
                if (rs != null)
                    baseActivity = rs.GetComponent(typeDescriptorContext.Instance) as Activity;

                string ruleSetName = null;
                RuleSetReference ruleSetReference = typeDescriptorContext.PropertyDescriptor.GetValue(typeDescriptorContext.Instance) as RuleSetReference;
                if (ruleSetReference != null)
                    ruleSetName = ruleSetReference.RuleSetName;

                RuleSetBrowserDialog dlg = new RuleSetBrowserDialog(baseActivity, ruleSetName);

                if (DialogResult.OK == editorService.ShowDialog(dlg))
                    returnVal = typeDescriptorContext.PropertyDescriptor.Converter.ConvertFrom(typeDescriptorContext, CultureInfo.CurrentUICulture, dlg.SelectedName);
            }

            return returnVal;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }

    #endregion

    #region class RuleSetDefinitionEditor

    internal sealed class RuleSetDefinitionEditor : UITypeEditor
    {
        private IWindowsFormsEditorService editorService;

        public RuleSetDefinitionEditor()
        {
        }

        public override object EditValue(ITypeDescriptorContext typeDescriptorContext, IServiceProvider serviceProvider, object o)
        {
            if (typeDescriptorContext == null)
                throw new ArgumentNullException("typeDescriptorContext");
            if (serviceProvider == null)
                throw new ArgumentNullException("serviceProvider");
            
            object returnVal = o;

            // Do not allow editing if in debug mode.
            WorkflowDesignerLoader workflowDesignerLoader = serviceProvider.GetService(typeof(WorkflowDesignerLoader)) as WorkflowDesignerLoader;
            if (workflowDesignerLoader != null && workflowDesignerLoader.InDebugMode)
                throw new InvalidOperationException(Messages.DebugModeEditsDisallowed);

            // Do not allow editing expression if the name is not set.
            RuleSetReference ruleSetReference = typeDescriptorContext.Instance as RuleSetReference;

            if (ruleSetReference == null || ruleSetReference.RuleSetName == null || ruleSetReference.RuleSetName.Length <= 0)
                throw new ArgumentException(Messages.RuleSetNameNotSet);

            Activity baseActivity = null;

            IReferenceService rs = serviceProvider.GetService(typeof(IReferenceService)) as IReferenceService;
            if (rs != null)
                baseActivity = rs.GetComponent(typeDescriptorContext.Instance) as Activity;

            RuleSetCollection ruleSetCollection = null;
            RuleDefinitions rules = ConditionHelper.Load_Rules_DT(serviceProvider, Helpers.GetRootActivity(baseActivity));
            if (rules != null)
                ruleSetCollection = rules.RuleSets;

            if (ruleSetCollection != null && !ruleSetCollection.Contains(ruleSetReference.RuleSetName))
            {
                string message = string.Format(CultureInfo.CurrentCulture, Messages.RuleSetNotFound, ruleSetReference.RuleSetName);
                throw new ArgumentException(message);
            }

            this.editorService = (IWindowsFormsEditorService)serviceProvider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                RuleSet ruleSet = ruleSetCollection[ruleSetReference.RuleSetName];
                using (RuleSetDialog dlg = new RuleSetDialog(baseActivity, ruleSet))
                {
                    if (DialogResult.OK == editorService.ShowDialog(dlg))
                        returnVal = dlg.RuleSet;
                }
            }
            return returnVal;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext typeDescriptorContext)
        {
            return UITypeEditorEditStyle.Modal;
        }
    }

    #endregion
}
