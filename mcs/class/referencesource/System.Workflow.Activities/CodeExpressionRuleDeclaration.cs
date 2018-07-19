namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.CodeDom;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Diagnostics;
    using System.Reflection;
    using System.Workflow.ComponentModel;
    using System.Workflow.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.Runtime.DebugEngine;

    [ToolboxItem(false)]
    [ActivityValidator(typeof(CodeConditionValidator))]
    [SRDisplayName(SR.CodeConditionDisplayName)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class CodeCondition : ActivityCondition
    {
        public static readonly DependencyProperty ConditionEvent = DependencyProperty.Register("Condition", typeof(EventHandler<ConditionalEventArgs>), typeof(CodeCondition));

        [SRDescription(SR.ExpressionDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler<ConditionalEventArgs> Condition
        {
            add
            {
                base.AddHandler(ConditionEvent, value);
            }
            remove
            {
                base.RemoveHandler(ConditionEvent, value);
            }
        }

        #region Bind resolution Support

        protected override object GetBoundValue(ActivityBind bind, Type targetType)
        {
            if (bind == null)
                throw new ArgumentNullException("bind");
            if (targetType == null)
                throw new ArgumentNullException("targetType");

            object returnVal = bind;
            Activity activity = this.ParentDependencyObject as Activity;
            if (activity != null)
                returnVal = bind.GetRuntimeValue(activity, targetType);
            return returnVal;
        }

        #endregion

        public override bool Evaluate(Activity ownerActivity, IServiceProvider provider)
        {
            if (provider == null)
                throw new ArgumentNullException("provider");

            ConditionalEventArgs eventArgs = new ConditionalEventArgs();
            EventHandler<ConditionalEventArgs>[] eventHandlers = base.GetInvocationList<EventHandler<ConditionalEventArgs>>(CodeCondition.ConditionEvent);

            IWorkflowDebuggerService workflowDebuggerService = provider.GetService(typeof(IWorkflowDebuggerService)) as IWorkflowDebuggerService;

            if (eventHandlers != null)
            {
                foreach (EventHandler<ConditionalEventArgs> eventHandler in eventHandlers)
                {
                    if (workflowDebuggerService != null)
                        workflowDebuggerService.NotifyHandlerInvoking(eventHandler);

                    eventHandler(ownerActivity, eventArgs);

                    if (workflowDebuggerService != null)
                        workflowDebuggerService.NotifyHandlerInvoked();

                }
            }
            return eventArgs.Result;
        }

        private class CodeConditionValidator : ConditionValidator
        {
            public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
            {
                ValidationErrorCollection errors = new ValidationErrorCollection();
                errors.AddRange(base.Validate(manager, obj));

                CodeCondition codeCondition = obj as CodeCondition;
                if (codeCondition != null)
                {
                    if (codeCondition.GetInvocationList<EventHandler<ConditionalEventArgs>>(CodeCondition.ConditionEvent).Length == 0 &&
                        codeCondition.GetBinding(CodeCondition.ConditionEvent) == null)
                    {
                        Hashtable hashtable = codeCondition.GetValue(WorkflowMarkupSerializer.EventsProperty) as Hashtable;
                        if (hashtable == null || hashtable["Condition"] == null)
                            errors.Add(ValidationError.GetNotSetValidationError(GetFullPropertyName(manager) + ".Condition"));
                    }
                }

                return errors;
            }
        }
    }
}
