namespace System.Workflow.Activities
{
    using System;
    using System.Text;
    using System.Reflection;
    using System.Collections;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Serialization;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel.Compiler;

    [SRDescription(SR.CodeActivityDescription)]
    [ToolboxItem(typeof(ActivityToolboxItem))]
    [Designer(typeof(CodeDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(CodeActivity), "Resources.code.png")]
    [DefaultEvent("ExecuteCode")]
    [SRCategory(SR.Standard)]
    [ActivityValidator(typeof(CodeActivityValidator))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class CodeActivity : Activity
    {
        #region Constructors

        public CodeActivity()
        {
        }

        public CodeActivity(string name)
            : base(name)
        {
        }

        #endregion

        public static readonly DependencyProperty ExecuteCodeEvent = DependencyProperty.Register("ExecuteCode", typeof(EventHandler), typeof(CodeActivity));

        protected override sealed ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            base.RaiseEvent(CodeActivity.ExecuteCodeEvent, this, EventArgs.Empty);

            return ActivityExecutionStatus.Closed;
        }

        [SRCategory(SR.Handlers)]
        [SRDescription(SR.UserCodeHandlerDescr)]
        [MergableProperty(false)]
        public event EventHandler ExecuteCode
        {
            add
            {
                base.AddHandler(ExecuteCodeEvent, value);
            }
            remove
            {
                base.RemoveHandler(ExecuteCodeEvent, value);
            }
        }

        private class CodeActivityValidator : ActivityValidator
        {
            public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
            {
                ValidationErrorCollection errors = new ValidationErrorCollection();

                CodeActivity code = obj as CodeActivity;
                if (code == null)
                    throw new InvalidOperationException();

                // This violates the P || C validation condition, but we are compiling with csc.exe here!
                if (code.GetInvocationList<EventHandler>(CodeActivity.ExecuteCodeEvent).Length == 0 &&
                    code.GetBinding(CodeActivity.ExecuteCodeEvent) == null)
                {
                    Hashtable hashtable = code.GetValue(WorkflowMarkupSerializer.EventsProperty) as Hashtable;
                    if (hashtable == null || hashtable["ExecuteCode"] == null)
                        errors.Add(ValidationError.GetNotSetValidationError("ExecuteCode"));
                }

                errors.AddRange(base.Validate(manager, obj));
                return errors;
            }
        }
    }
}
