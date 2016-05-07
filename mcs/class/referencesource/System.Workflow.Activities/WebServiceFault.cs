using System;
using System.Reflection;
using System.Drawing;
using System.Drawing.Design;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Workflow.ComponentModel;
using System.Workflow.Runtime;
using System.Workflow.ComponentModel.Design;
using System.Workflow.ComponentModel.Compiler;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics;
using System.Globalization;
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities
{
    #region WebServiceFaultActivity
    [SRDescription(SR.WebServiceFaultActivityDescription)]
    [SRCategory(SR.Standard)]
    [ToolboxBitmap(typeof(WebServiceFaultActivity), "Resources.WebServiceOut.png")]
    [Designer(typeof(WebServiceFaultDesigner), typeof(IDesigner))]
    [ActivityValidator(typeof(WebServiceFaultValidator))]
    [DefaultEvent("SendingFault")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WebServiceFaultActivity : Activity, IPropertyValueProvider
    {
        #region Dependency Properties
        public static readonly DependencyProperty InputActivityNameProperty = DependencyProperty.Register("InputActivityName", typeof(string), typeof(WebServiceFaultActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));
        public static readonly DependencyProperty FaultProperty = DependencyProperty.Register("Fault", typeof(Exception), typeof(WebServiceFaultActivity));
        public static readonly DependencyProperty SendingFaultEvent = DependencyProperty.Register("SendingFault", typeof(EventHandler), typeof(WebServiceFaultActivity));
        #endregion

        #region Constructors

        public WebServiceFaultActivity()
        {
        }

        public WebServiceFaultActivity(string name)
            : base(name)
        {
        }

        #endregion

        #region Properties
        [SRCategory(SR.Activity)]
        [SRDescription(SR.ReceiveActivityNameDescription)]
        [TypeConverter(typeof(PropertyValueProviderTypeConverter))]
        [RefreshProperties(RefreshProperties.All)]
        [MergablePropertyAttribute(false)]
        [DefaultValue("")]
        public string InputActivityName
        {
            get
            {
                return base.GetValue(InputActivityNameProperty) as string;
            }

            set
            {
                base.SetValue(InputActivityNameProperty, value);
            }
        }

        [Browsable(true)]
        [SRCategory(SR.Properties)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [DefaultValue(null)]
        [MergablePropertyAttribute(false)]
        public Exception Fault
        {
            get
            {
                return base.GetValue(FaultProperty) as Exception;
            }
            set
            {
                base.SetValue(FaultProperty, value);
            }
        }
        #endregion

        #region Handlers
        [SRDescription(SR.OnBeforeFaultingDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler SendingFault
        {
            add
            {
                base.AddHandler(SendingFaultEvent, value);
            }
            remove
            {
                base.RemoveHandler(SendingFaultEvent, value);
            }
        }
        #endregion

        #region IPropertyValueProvider Implementation
        ICollection IPropertyValueProvider.GetPropertyValues(ITypeDescriptorContext context)
        {
            StringCollection names = new StringCollection();
            if (context.PropertyDescriptor.Name == "InputActivityName")
            {
                foreach (Activity activity in WebServiceActivityHelpers.GetPreceedingActivities(this))
                {
                    if (activity is WebServiceInputActivity)
                    {
                        names.Add(activity.QualifiedName);
                    }
                }
            }
            return names;
        }
        #endregion

        #region Protected Methods
        protected override void Initialize(IServiceProvider provider)
        {
            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            base.Initialize(provider);
        }

        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            if (this.Fault == null)
            {
                throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, SR.Error_PropertyNotSet, FaultProperty.Name));
            }

            WorkflowQueuingService queueService = executionContext.GetService<WorkflowQueuingService>();

            // fire event
            this.RaiseEvent(WebServiceFaultActivity.SendingFaultEvent, this, EventArgs.Empty);

            WebServiceInputActivity webservicereceive = this.GetActivityByName(this.InputActivityName) as WebServiceInputActivity;
            IComparable queueId = new EventQueueName(webservicereceive.InterfaceType, webservicereceive.MethodName, webservicereceive.QualifiedName);
            Debug.Assert(queueService.Exists(queueId));
            IMethodResponseMessage responseMessage = null;
            WorkflowQueue queue = queueService.GetWorkflowQueue(queueId);

            if (queue.Count != 0)
                responseMessage = queue.Dequeue() as IMethodResponseMessage;

            System.Diagnostics.Debug.Assert(responseMessage != null);

            // populate exception & reset the waiting thread
            responseMessage.SendException(this.Fault);

            return ActivityExecutionStatus.Closed;
        }
        #endregion
    }
    #endregion

    #region Validator
    internal sealed class WebServiceFaultValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            WebServiceFaultActivity webServiceFault = obj as WebServiceFaultActivity;

            if (webServiceFault == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(WebServiceFaultActivity).FullName), "obj");

            if (Helpers.IsActivityLocked(webServiceFault))
            {
                return validationErrors;
            }

            WebServiceInputActivity webServiceReceive = null;

            if (String.IsNullOrEmpty(webServiceFault.InputActivityName))
                validationErrors.Add(ValidationError.GetNotSetValidationError("InputActivityName"));
            else
            {
                ITypeProvider typeProvider = (ITypeProvider)manager.GetService(typeof(ITypeProvider));

                if (typeProvider == null)
                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                bool foundMatchingReceive = false;
                foreach (Activity activity in WebServiceActivityHelpers.GetPreceedingActivities(webServiceFault))
                {
                    if ((activity is WebServiceFaultActivity && String.Compare(((WebServiceFaultActivity)activity).InputActivityName, webServiceFault.InputActivityName, StringComparison.Ordinal) == 0) ||
                        (activity is WebServiceOutputActivity && String.Compare(((WebServiceOutputActivity)activity).InputActivityName, webServiceFault.InputActivityName, StringComparison.Ordinal) == 0))
                    {
                        if (activity is WebServiceFaultActivity)
                            validationErrors.Add(new ValidationError(SR.GetString(SR.Error_DuplicateWebServiceFaultFound, activity.Name, webServiceFault.InputActivityName), ErrorNumbers.Error_DuplicateWebServiceFaultFound));
                        else
                            validationErrors.Add(new ValidationError(SR.GetString(SR.Error_DuplicateWebServiceResponseFound, activity.Name, webServiceFault.InputActivityName), ErrorNumbers.Error_DuplicateWebServiceResponseFound));
                        return validationErrors;
                    }
                }

                foreach (Activity activity in WebServiceActivityHelpers.GetPreceedingActivities(webServiceFault))
                {

                    if (String.Compare(activity.QualifiedName, webServiceFault.InputActivityName, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        if (activity is WebServiceInputActivity)
                        {
                            webServiceReceive = activity as WebServiceInputActivity;
                            foundMatchingReceive = true;
                        }
                        else
                        {
                            foundMatchingReceive = false;
                            validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotValid, webServiceFault.InputActivityName), ErrorNumbers.Error_WebServiceReceiveNotValid));
                            return validationErrors;
                        }
                        break;
                    }
                }

                if (!foundMatchingReceive)
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotFound, webServiceFault.InputActivityName), ErrorNumbers.Error_WebServiceReceiveNotFound));
                    return validationErrors;
                }

                Type interfaceType = null;

                if (webServiceReceive.InterfaceType != null)
                    interfaceType = typeProvider.GetType(webServiceReceive.InterfaceType.AssemblyQualifiedName);

                if (interfaceType == null)
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotConfigured, webServiceReceive.Name), ErrorNumbers.Error_WebServiceReceiveNotConfigured));
                    return validationErrors;
                }

                // Validate method
                if (String.IsNullOrEmpty(webServiceReceive.MethodName))
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotConfigured, webServiceReceive.Name), ErrorNumbers.Error_WebServiceReceiveNotConfigured));
                    return validationErrors;
                }

                MethodInfo methodInfo = Helpers.GetInterfaceMethod(interfaceType, webServiceReceive.MethodName);

                if (methodInfo == null)
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotConfigured, webServiceReceive.Name), ErrorNumbers.Error_WebServiceReceiveNotConfigured));
                    return validationErrors;
                }

                List<ParameterInfo> inputParameters, outParameters;
                WebServiceActivityHelpers.GetParameterInfo(methodInfo, out inputParameters, out outParameters);

                if (outParameters.Count == 0)
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceFaultNotNeeded), ErrorNumbers.Error_WebServiceFaultNotNeeded));
                    return validationErrors;
                }
            }
            return validationErrors;
        }
    }
    #endregion
}
