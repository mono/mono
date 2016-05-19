namespace System.Workflow.Activities
{
    #region Imports

    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.Activities.Common;

    #endregion


    [Designer(typeof(IfElseBranchDesigner), typeof(IDesigner))]
    [ToolboxItem(false)]
    [ActivityValidator(typeof(IfElseBranchValidator))]
    [ToolboxBitmap(typeof(IfElseBranchActivity), "Resources.DecisionBranch.bmp")]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class IfElseBranchActivity : SequenceActivity
    {
        public IfElseBranchActivity()
        {
        }

        public IfElseBranchActivity(string name)
            : base(name)
        {
        }

        //metadata properties go here
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.Register("Condition", typeof(ActivityCondition), typeof(IfElseBranchActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata));


        [SRCategory(SR.Conditions)]
        [SRDescription(SR.ConditionDescr)]
        [RefreshProperties(RefreshProperties.Repaint)]
        [DefaultValue(null)]
        public ActivityCondition Condition
        {
            get
            {
                return base.GetValue(ConditionProperty) as ActivityCondition;
            }
            set
            {
                base.SetValue(ConditionProperty, value);
            }
        }
    }

    internal sealed class IfElseBranchValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            IfElseBranchActivity ifElseBranch = obj as IfElseBranchActivity;
            if (ifElseBranch == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(IfElseBranchActivity).FullName), "obj");

            // parent must be conditional
            IfElseActivity ifElse = ifElseBranch.Parent as IfElseActivity;
            if (ifElse == null)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ConditionalBranchParentNotConditional), ErrorNumbers.Error_ConditionalBranchParentNotConditional));

            bool isLastBranch = (ifElse != null && ifElse.EnabledActivities.Count > 1 && (ifElse.EnabledActivities[ifElse.EnabledActivities.Count - 1] == ifElseBranch));
            if (!isLastBranch || ifElseBranch.Condition != null)
            {
                if (ifElseBranch.Condition == null)
                    validationErrors.Add(ValidationError.GetNotSetValidationError("Condition"));
            }
            return validationErrors;
        }
    }
}
