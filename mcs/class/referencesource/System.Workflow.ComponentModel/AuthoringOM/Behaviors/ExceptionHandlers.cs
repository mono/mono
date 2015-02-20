namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.Diagnostics;
    using System.Reflection;
    using System.Drawing;
    using System.Collections;
    using System.CodeDom;
    using System.Globalization;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;
    using System.Collections.Generic;

    #endregion

    [ToolboxItem(false)]
    [Designer(typeof(FaultHandlersActivityDesigner), typeof(IDesigner))]
    [ToolboxBitmap(typeof(FaultHandlersActivity), "Resources.Exceptions.png")]
    [ActivityValidator(typeof(FaultHandlersActivityValidator))]
    [AlternateFlowActivity]
    [SRCategory(SR.Standard)]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class FaultHandlersActivity : CompositeActivity, IActivityEventListener<ActivityExecutionStatusChangedEventArgs>
    {
        public FaultHandlersActivity()
        {
        }

        public FaultHandlersActivity(string name)
            : base(name)
        {
        }

        protected internal override void Initialize(IServiceProvider provider)
        {
            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            base.Initialize(provider);
        }

        protected internal override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            Debug.Assert(this.Parent.GetValue(ActivityExecutionContext.CurrentExceptionProperty) != null, "No Exception contained by parent");

            Exception excep = this.Parent.GetValue(ActivityExecutionContext.CurrentExceptionProperty) as Exception;

            if (excep != null)
            {
                Type exceptionType = excep.GetType();

                foreach (FaultHandlerActivity exceptionHandler in this.EnabledActivities)
                {
                    if (CanHandleException(exceptionHandler, exceptionType))
                    {
                        // remove exception from here, I ate it
                        this.Parent.RemoveProperty(ActivityExecutionContext.CurrentExceptionProperty);
                        exceptionHandler.SetException(excep);
                        exceptionHandler.RegisterForStatusChange(Activity.ClosedEvent, this);
                        executionContext.ExecuteActivity(exceptionHandler);
                        return ActivityExecutionStatus.Executing;
                    }
                }
            }
            return ActivityExecutionStatus.Closed;
        }

        protected internal override ActivityExecutionStatus Cancel(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            for (int i = 0; i < this.EnabledActivities.Count; ++i)
            {
                Activity childActivity = this.EnabledActivities[i];

                if (childActivity.ExecutionStatus == ActivityExecutionStatus.Executing)
                    executionContext.CancelActivity(childActivity);
                if (childActivity.ExecutionStatus == ActivityExecutionStatus.Canceling ||
                    childActivity.ExecutionStatus == ActivityExecutionStatus.Faulting)
                    return this.ExecutionStatus;
            }
            return ActivityExecutionStatus.Closed;
        }

        #region IActivityEventListener<ActivityExecutionStatusChangedEventArgs> Members

        void IActivityEventListener<ActivityExecutionStatusChangedEventArgs>.OnEvent(object sender, ActivityExecutionStatusChangedEventArgs e)
        {
            if (sender == null)
                throw new ArgumentNullException("sender");
            if (e == null)
                throw new ArgumentNullException("e");

            ActivityExecutionContext context = sender as ActivityExecutionContext;
            if (context == null)
                throw new ArgumentException(SR.Error_SenderMustBeActivityExecutionContext, "sender");

            e.Activity.UnregisterForStatusChange(Activity.ClosedEvent, this);
            context.CloseActivity();
        }

        [NonSerialized]
        bool activeChildRemoved = false;

        protected internal override void OnActivityChangeRemove(ActivityExecutionContext executionContext, Activity removedActivity)
        {
            if (removedActivity == null)
                throw new ArgumentNullException("removedActivity");

            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (removedActivity.ExecutionStatus == ActivityExecutionStatus.Closed && this.ExecutionStatus != ActivityExecutionStatus.Closed)
                activeChildRemoved = true;

            base.OnActivityChangeRemove(executionContext, removedActivity);
        }

        protected internal override void OnWorkflowChangesCompleted(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (activeChildRemoved)
            {
                executionContext.CloseActivity();
                activeChildRemoved = false;
            }
            base.OnWorkflowChangesCompleted(executionContext);
        }
        protected override void OnClosed(IServiceProvider provider)
        {
        }
        #endregion

        private bool CanHandleException(FaultHandlerActivity exceptionHandler, Type et)
        {
            Type canHandleType = exceptionHandler.FaultType;
            return (et == canHandleType || et.IsSubclassOf(canHandleType));
        }
    }

    internal sealed class FaultHandlersActivityValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            FaultHandlersActivity exceptionHandlers = obj as FaultHandlersActivity;
            if (exceptionHandlers == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(FaultHandlersActivity).FullName), "obj");

            Hashtable exceptionTypes = new Hashtable();
            ArrayList previousExceptionTypes = new ArrayList();
            bool bFoundNotFaultHandlerActivity = false;
            foreach (Activity activity in exceptionHandlers.EnabledActivities)
            {
                // All child activities must be FaultHandlerActivity
                if (!(activity is FaultHandlerActivity))
                {
                    if (!bFoundNotFaultHandlerActivity)
                    {
                        validationErrors.Add(new ValidationError(SR.GetString(SR.Error_FaultHandlersActivityDeclNotAllFaultHandlerActivityDecl), ErrorNumbers.Error_FaultHandlersActivityDeclNotAllFaultHandlerActivityDecl));
                        bFoundNotFaultHandlerActivity = true;
                    }
                }
                else
                {
                    FaultHandlerActivity exceptionHandler = (FaultHandlerActivity)activity;
                    Type catchType = exceptionHandler.FaultType;
                    if (catchType != null)
                    {
                        if (exceptionTypes[catchType] == null)
                        {
                            exceptionTypes[catchType] = 1;
                            previousExceptionTypes.Add(catchType);
                        }
                        else if ((int)exceptionTypes[catchType] == 1)
                        {
                            /*if (catchType == typeof(System.Exception))
                                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ScopeDuplicateFaultHandlerActivityForAll, exceptionHandlers.EnclosingDataContextActivity.GetType().Name), ErrorNumbers.Error_ScopeDuplicateFaultHandlerActivityForAll));
                            else*/
                            validationErrors.Add(new ValidationError(string.Format(CultureInfo.CurrentCulture, SR.GetString(SR.Error_ScopeDuplicateFaultHandlerActivityFor), new object[] { Helpers.GetEnclosingActivity(exceptionHandlers).GetType().Name, catchType.FullName }), ErrorNumbers.Error_ScopeDuplicateFaultHandlerActivityFor));

                            exceptionTypes[catchType] = 2;
                        }

                        foreach (Type previousType in previousExceptionTypes)
                        {
                            if (previousType != catchType && previousType.IsAssignableFrom(catchType))
                                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_FaultHandlerActivityWrongOrder, catchType.Name, previousType.Name), ErrorNumbers.Error_FaultHandlerActivityWrongOrder));
                        }
                    }
                }
            }

            // fault handlers can not contain fault handlers, compensation handler and cancellation handler
            if (((ISupportAlternateFlow)exceptionHandlers).AlternateFlowActivities.Count > 0)
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ModelingConstructsCanNotContainModelingConstructs), ErrorNumbers.Error_ModelingConstructsCanNotContainModelingConstructs));

            return validationErrors;
        }
    }
}
