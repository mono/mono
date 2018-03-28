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
using System.Workflow.Activities.Common;

namespace System.Workflow.Activities
{
    [SRDescription(SR.WebServiceResponseActivityDescription)]
    [SRCategory(SR.Standard)]
    [ToolboxBitmap(typeof(WebServiceOutputActivity), "Resources.WebServiceOut.png")]
    [Designer(typeof(WebServiceResponseDesigner), typeof(IDesigner))]
    [ActivityValidator(typeof(WebServiceResponseValidator))]
    [DefaultEvent("SendingOutput")]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WebServiceOutputActivity : Activity, IPropertyValueProvider, IDynamicPropertyTypeProvider
    {
        //metadata properties
        public static readonly DependencyProperty InputActivityNameProperty = DependencyProperty.Register("InputActivityName", typeof(string), typeof(WebServiceOutputActivity), new PropertyMetadata("", DependencyPropertyOptions.Metadata));

        //instance properties
        public static readonly DependencyProperty ParameterBindingsProperty = DependencyProperty.Register("ParameterBindings", typeof(WorkflowParameterBindingCollection), typeof(WebServiceOutputActivity), new PropertyMetadata(DependencyPropertyOptions.Metadata | DependencyPropertyOptions.ReadOnly, new Attribute[] { new BrowsableAttribute(false), new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Content) }));

        //event
        public static readonly DependencyProperty SendingOutputEvent = DependencyProperty.Register("SendingOutput", typeof(EventHandler), typeof(WebServiceOutputActivity));

        #region Constructors

        public WebServiceOutputActivity()
        {
            //
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        public WebServiceOutputActivity(string name)
            : base(name)
        {
            //
            base.SetReadOnlyPropertyValue(ParameterBindingsProperty, new WorkflowParameterBindingCollection(this));
        }

        #endregion

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

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Browsable(false)]
        public WorkflowParameterBindingCollection ParameterBindings
        {
            get
            {
                return base.GetValue(ParameterBindingsProperty) as WorkflowParameterBindingCollection;
            }
        }

        [SRDescription(SR.OnBeforeResponseDescr)]
        [SRCategory(SR.Handlers)]
        [MergableProperty(false)]
        public event EventHandler SendingOutput
        {
            add
            {
                base.AddHandler(SendingOutputEvent, value);
            }
            remove
            {
                base.RemoveHandler(SendingOutputEvent, value);
            }
        }


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
        protected override void Initialize(IServiceProvider provider)
        {
            if (this.Parent == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MustHaveParent));

            base.Initialize(provider);
        }

        #region Execute
        protected override ActivityExecutionStatus Execute(ActivityExecutionContext executionContext)
        {
            if (executionContext == null)
                throw new ArgumentNullException("executionContext");

            WorkflowQueuingService queueService = executionContext.GetService<WorkflowQueuingService>();

            // fire event
            this.RaiseEvent(WebServiceOutputActivity.SendingOutputEvent, this, EventArgs.Empty);

            WebServiceInputActivity webservicereceive = this.GetActivityByName(this.InputActivityName) as WebServiceInputActivity;
            if (webservicereceive == null)
            {
                Activity parent = this.Parent;
                while (parent != null)
                {
                    //typically if defined inside a custom activity
                    string qualifiedName = parent.QualifiedName + "." + this.InputActivityName;
                    webservicereceive = this.GetActivityByName(qualifiedName) as WebServiceInputActivity;
                    if (webservicereceive != null)
                        break;
                    parent = this.Parent;
                }
            }
            if (webservicereceive == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_CannotResolveWebServiceInput, this.QualifiedName, this.InputActivityName));

            IComparable queueId = new EventQueueName(webservicereceive.InterfaceType, webservicereceive.MethodName, webservicereceive.QualifiedName);

            MethodInfo mInfo = webservicereceive.InterfaceType.GetMethod(webservicereceive.MethodName);
            if (!queueService.Exists(queueId))
            {
                // determine if no response is required,
                // compiler did not catch it, do the runtime check and return
                if (mInfo.ReturnType == typeof(void))
                {
                    return ActivityExecutionStatus.Closed;
                }

                bool isresponseRequired = false;
                foreach (ParameterInfo formalParameter in mInfo.GetParameters())
                {
                    if (formalParameter.ParameterType.IsByRef || (formalParameter.IsIn && formalParameter.IsOut))
                    {
                        isresponseRequired = true;
                    }
                }

                if (isresponseRequired)
                {
                    return ActivityExecutionStatus.Closed;
                }
            }

            if (!queueService.Exists(queueId))
                throw new InvalidOperationException(SR.GetString(SR.Error_WebServiceInputNotProcessed, webservicereceive.QualifiedName));

            IMethodResponseMessage responseMessage = null;
            WorkflowQueue queue = queueService.GetWorkflowQueue(queueId);

            if (queue.Count != 0)
                responseMessage = queue.Dequeue() as IMethodResponseMessage;

            IMethodMessage message = responseMessage as IMethodMessage;

            WorkflowParameterBindingCollection parameterBindings = this.ParameterBindings;
            ArrayList outArgs = new ArrayList();
            // populate result
            if (this.ParameterBindings.Contains("(ReturnValue)"))
            {
                WorkflowParameterBinding retBind = this.ParameterBindings["(ReturnValue)"];
                if (retBind != null)
                {
                    outArgs.Add(retBind.Value);
                }
            }

            foreach (ParameterInfo formalParameter in mInfo.GetParameters())
            {
                // update out and byref values
                if (formalParameter.ParameterType.IsByRef || (formalParameter.IsIn && formalParameter.IsOut))
                {
                    WorkflowParameterBinding binding = parameterBindings[formalParameter.Name];
                    outArgs.Add(binding.Value);
                }
            }

            // reset the waiting thread
            responseMessage.SendResponse(outArgs);

            return ActivityExecutionStatus.Closed;
        }
        #endregion



        #region IDynamicPropertyTypeProvider

        Type IDynamicPropertyTypeProvider.GetPropertyType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            Dictionary<string, object> parameters = new Dictionary<string, object>();
            this.GetParameterPropertyDescriptors(parameters);
            if (parameters.ContainsKey(propertyName))
            {
                ParameterInfoBasedPropertyDescriptor descriptor = parameters[propertyName] as ParameterInfoBasedPropertyDescriptor;
                if (descriptor != null)
                    return descriptor.ParameterType;
            }

            return null;
        }

        AccessTypes IDynamicPropertyTypeProvider.GetAccessType(IServiceProvider serviceProvider, string propertyName)
        {
            if (propertyName == null)
                throw new ArgumentNullException("propertyName");

            return AccessTypes.Read;
        }

        internal void GetParameterPropertyDescriptors(IDictionary properties)
        {
            if (((IComponent)this).Site == null)
                return;

            ITypeProvider typeProvider = (ITypeProvider)((IComponent)this).Site.GetService(typeof(ITypeProvider));
            if (typeProvider == null)
                throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

            if (this.InputActivityName != null && !String.IsNullOrEmpty(this.InputActivityName.Trim()))
            {
                WebServiceInputActivity webServiceReceive = Helpers.ParseActivity(Helpers.GetRootActivity(this), this.InputActivityName) as WebServiceInputActivity;
                if (webServiceReceive != null)
                {
                    Type type = null;
                    if (webServiceReceive.InterfaceType != null)
                        type = typeProvider.GetType(webServiceReceive.InterfaceType.AssemblyQualifiedName);

                    if (type != null)
                    {

                        MethodInfo method = Helpers.GetInterfaceMethod(type, webServiceReceive.MethodName);
                        if (method != null && WebServiceActivityHelpers.ValidateParameterTypes(method).Count == 0)
                        {
                            List<ParameterInfo> inputParameters, outParameters;
                            WebServiceActivityHelpers.GetParameterInfo(method, out inputParameters, out outParameters);

                            foreach (ParameterInfo paramInfo in outParameters)
                            {
                                PropertyDescriptor prop = null;
                                if (paramInfo.Position == -1)
                                    prop = new ParameterInfoBasedPropertyDescriptor(typeof(WebServiceOutputActivity), paramInfo, false, DesignOnlyAttribute.Yes);
                                else
                                    prop = new ParameterInfoBasedPropertyDescriptor(typeof(WebServiceOutputActivity), paramInfo, true, DesignOnlyAttribute.Yes);

                                if (prop != null)
                                    properties[prop.Name] = prop;
                            }
                        }
                    }
                }
            }
        }
        #endregion

    }

    internal sealed class WebServiceResponseValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            WebServiceOutputActivity webServiceResponse = obj as WebServiceOutputActivity;
            if (webServiceResponse == null)
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(WebServiceOutputActivity).FullName), "obj");

            if (Helpers.IsActivityLocked(webServiceResponse))
            {
                return validationErrors;
            }

            WebServiceInputActivity webServiceReceive = null;

            if (String.IsNullOrEmpty(webServiceResponse.InputActivityName))
                validationErrors.Add(ValidationError.GetNotSetValidationError("InputActivityName"));
            else
            {
                ITypeProvider typeProvider = (ITypeProvider)manager.GetService(typeof(ITypeProvider));
                if (typeProvider == null)
                    throw new InvalidOperationException(SR.GetString(SR.General_MissingService, typeof(ITypeProvider).FullName));

                bool foundMatchingReceive = false;
                foreach (Activity activity in WebServiceActivityHelpers.GetPreceedingActivities(webServiceResponse))
                {
                    if ((activity is WebServiceOutputActivity && String.Compare(((WebServiceOutputActivity)activity).InputActivityName, webServiceResponse.InputActivityName, StringComparison.Ordinal) == 0) ||
                        (activity is WebServiceFaultActivity && String.Compare(((WebServiceFaultActivity)activity).InputActivityName, webServiceResponse.InputActivityName, StringComparison.Ordinal) == 0))
                    {
                        if (activity is WebServiceOutputActivity)
                            validationErrors.Add(new ValidationError(SR.GetString(SR.Error_DuplicateWebServiceResponseFound, activity.QualifiedName, webServiceResponse.InputActivityName), ErrorNumbers.Error_DuplicateWebServiceResponseFound));
                        else
                            validationErrors.Add(new ValidationError(SR.GetString(SR.Error_DuplicateWebServiceFaultFound, activity.QualifiedName, webServiceResponse.InputActivityName), ErrorNumbers.Error_DuplicateWebServiceFaultFound));
                        return validationErrors;
                    }
                }

                foreach (Activity activity in WebServiceActivityHelpers.GetPreceedingActivities(webServiceResponse))
                {
                    if (String.Compare(activity.QualifiedName, webServiceResponse.InputActivityName, StringComparison.Ordinal) == 0)
                    {
                        if (activity is WebServiceInputActivity)
                        {
                            webServiceReceive = activity as WebServiceInputActivity;
                            foundMatchingReceive = true;
                        }
                        else
                        {
                            foundMatchingReceive = false;
                            validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotValid, webServiceResponse.InputActivityName), ErrorNumbers.Error_WebServiceReceiveNotValid));
                            return validationErrors;
                        }
                        break;
                    }
                }

                if (!foundMatchingReceive)
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotFound, webServiceResponse.InputActivityName), ErrorNumbers.Error_WebServiceReceiveNotFound));
                    return validationErrors;
                }
                else
                {
                    Type interfaceType = null;
                    if (webServiceReceive.InterfaceType != null)
                        interfaceType = typeProvider.GetType(webServiceReceive.InterfaceType.AssemblyQualifiedName);

                    if (interfaceType == null)
                    {
                        validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotConfigured, webServiceReceive.Name), ErrorNumbers.Error_WebServiceReceiveNotConfigured));
                    }
                    else
                    {
                        // Validate method
                        if (String.IsNullOrEmpty(webServiceReceive.MethodName))
                            validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotConfigured, webServiceReceive.Name), ErrorNumbers.Error_WebServiceReceiveNotConfigured));
                        else
                        {
                            MethodInfo methodInfo = Helpers.GetInterfaceMethod(interfaceType, webServiceReceive.MethodName);

                            if (methodInfo == null)
                            {
                                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceReceiveNotConfigured, webServiceReceive.Name), ErrorNumbers.Error_WebServiceReceiveNotConfigured));
                            }
                            else
                            {
                                ValidationErrorCollection parameterTypeErrors = WebServiceActivityHelpers.ValidateParameterTypes(methodInfo);
                                if (parameterTypeErrors.Count > 0)
                                {
                                    foreach (ValidationError parameterTypeError in parameterTypeErrors)
                                    {
                                        parameterTypeError.PropertyName = "InputActivityName";
                                    }
                                    validationErrors.AddRange(parameterTypeErrors);
                                }
                                else
                                {
                                    List<ParameterInfo> inputParameters, outParameters;
                                    WebServiceActivityHelpers.GetParameterInfo(methodInfo, out inputParameters, out outParameters);

                                    if (outParameters.Count == 0)
                                    {
                                        validationErrors.Add(new ValidationError(SR.GetString(SR.Error_WebServiceResponseNotNeeded), ErrorNumbers.Error_WebServiceResponseNotNeeded));
                                    }
                                    else
                                    {
                                        // Check to see if all output parameters have a valid bindings.
                                        foreach (ParameterInfo paramInfo in outParameters)
                                        {
                                            string paramName = paramInfo.Name;
                                            Type paramType = paramInfo.ParameterType.IsByRef ? paramInfo.ParameterType.GetElementType() : paramInfo.ParameterType;

                                            if (paramInfo.Position == -1)
                                                paramName = "(ReturnValue)";

                                            object paramValue = null;
                                            if (webServiceResponse.ParameterBindings.Contains(paramName))
                                            {
                                                if (webServiceResponse.ParameterBindings[paramName].IsBindingSet(WorkflowParameterBinding.ValueProperty))
                                                    paramValue = webServiceResponse.ParameterBindings[paramName].GetBinding(WorkflowParameterBinding.ValueProperty);
                                                else
                                                    paramValue = webServiceResponse.ParameterBindings[paramName].GetValue(WorkflowParameterBinding.ValueProperty);
                                            }

                                            if (!paramType.IsPublic || !paramType.IsSerializable)
                                            {
                                                ValidationError validationError = new ValidationError(SR.GetString(SR.Error_TypeNotPublicSerializable, paramName, paramType.FullName), ErrorNumbers.Error_TypeNotPublicSerializable);
                                                validationError.PropertyName = (String.Compare(paramName, "(ReturnValue)", StringComparison.Ordinal) == 0) ? paramName : ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(webServiceReceive.GetType(), paramName);
                                                validationErrors.Add(validationError);
                                            }
                                            else if (!webServiceResponse.ParameterBindings.Contains(paramName) || paramValue == null)
                                            {
                                                ValidationError validationError = ValidationError.GetNotSetValidationError(paramName);
                                                validationError.PropertyName = (String.Compare(paramName, "(ReturnValue)", StringComparison.Ordinal) == 0) ? paramName : ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(webServiceReceive.GetType(), paramName);
                                                validationErrors.Add(validationError);
                                            }
                                            else
                                            {
                                                AccessTypes access = AccessTypes.Read;
                                                if (paramInfo.IsOut || paramInfo.IsRetval || paramInfo.Position == -1)
                                                    access = AccessTypes.Write;

                                                ValidationErrorCollection variableErrors = ValidationHelpers.ValidateProperty(manager, webServiceResponse, paramValue,
                                                                                                new PropertyValidationContext(webServiceResponse.ParameterBindings[paramName], null, paramName), new BindValidationContext(paramInfo.ParameterType.IsByRef ? paramInfo.ParameterType.GetElementType() : paramInfo.ParameterType, access));
                                                foreach (ValidationError variableError in variableErrors)
                                                {
                                                    if (String.Compare(paramName, "(ReturnValue)", StringComparison.Ordinal) != 0)
                                                        variableError.PropertyName = ParameterInfoBasedPropertyDescriptor.GetParameterPropertyName(webServiceReceive.GetType(), paramName);
                                                }
                                                validationErrors.AddRange(variableErrors);
                                            }
                                        }

                                        if (webServiceResponse.ParameterBindings.Count > outParameters.Count)
                                            validationErrors.Add(new ValidationError(SR.GetString(SR.Warning_AdditionalBindingsFound), ErrorNumbers.Warning_AdditionalBindingsFound, true));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return validationErrors;
        }
    }

    internal static class WebServiceActivityHelpers
    {
        private static IEnumerable GetContainedActivities(CompositeActivity activity)
        {
            if (!activity.Enabled)
                yield break;

            foreach (Activity containedActivity in activity.Activities)
            {
                if (containedActivity is CompositeActivity && !Helpers.IsCustomActivity((CompositeActivity)containedActivity))
                {
                    foreach (Activity nestedActivity in WebServiceActivityHelpers.GetContainedActivities((CompositeActivity)containedActivity))
                    {
                        if (nestedActivity.Enabled)
                            yield return nestedActivity;
                    }
                }
                else
                {
                    if (containedActivity.Enabled)
                        yield return containedActivity;
                }
            }
            yield break;
        }

        internal static IEnumerable GetPreceedingActivities(Activity startActivity)
        {
            return GetPreceedingActivities(startActivity, false);
        }

        internal static IEnumerable GetPreceedingActivities(Activity startActivity, bool crossOverLoop)
        {
            Activity currentActivity = null;
            Stack<Activity> activityStack = new Stack<Activity>();
            activityStack.Push(startActivity);

            while ((currentActivity = activityStack.Pop()) != null)
            {
                if (currentActivity is CompositeActivity && Helpers.IsCustomActivity((CompositeActivity)currentActivity))
                    break;

                if (currentActivity.Parent != null)
                {
                    foreach (Activity siblingActivity in currentActivity.Parent.Activities)
                    {
                        // 
                        if (siblingActivity == currentActivity && ((currentActivity.Parent is ParallelActivity && !Helpers.IsFrameworkActivity(currentActivity)) || (currentActivity.Parent is StateActivity && !Helpers.IsFrameworkActivity(currentActivity))))
                            continue;

                        // 
                        if (currentActivity.Parent is IfElseActivity && !Helpers.IsFrameworkActivity(currentActivity))
                            continue;

                        //For Listen Case.
                        if (currentActivity.Parent is ListenActivity && !Helpers.IsFrameworkActivity(currentActivity))
                            continue;

                        // State Machine logic:
                        // If startActivity was in the InitialState, then
                        // there are no preceeding activities. 
                        // Otherwise, we just return the parent state as
                        // the preceeding activity. 
                        StateActivity currentState = currentActivity.Parent as StateActivity;
                        if (currentState != null)
                        {
                            StateActivity enclosingState = StateMachineHelpers.FindEnclosingState(startActivity);
                            //If we are at Initial State there is no preceeding above us.
                            if (StateMachineHelpers.IsInitialState(enclosingState))
                                yield break;
                            else
                                yield return currentState;
                        }

                        if (siblingActivity == currentActivity)
                            break;

                        if (siblingActivity.Enabled)
                        {
                            if (siblingActivity is CompositeActivity && !Helpers.IsCustomActivity((CompositeActivity)siblingActivity) && (crossOverLoop || !IsLoopActivity(siblingActivity)))
                            {
                                foreach (Activity containedActivity in WebServiceActivityHelpers.GetContainedActivities((CompositeActivity)siblingActivity))
                                    yield return containedActivity;
                            }
                            else
                            {
                                yield return siblingActivity;
                            }
                        }
                    }
                }

                if (!crossOverLoop && IsLoopActivity(currentActivity.Parent))
                    break;
                else
                    activityStack.Push(currentActivity.Parent);

            }
            yield break;
        }

        internal static bool IsLoopActivity(Activity activity)
        {
            //
            if (activity is WhileActivity || activity is ReplicatorActivity || activity is ConditionedActivityGroup)
                return true;

            return false;
        }

        internal static bool IsInsideLoop(Activity webServiceActivity, Activity searchBoundary)
        {
            IEnumerable<String> searchBoundaryPath = GetActivityPath(searchBoundary);
            IEnumerable<String> currentActivityPath = GetActivityPath(webServiceActivity);

            String leastCommonParent = FindLeastCommonParent(searchBoundaryPath, currentActivityPath);

            Activity currentActivity = webServiceActivity;

            while (currentActivity.Parent != null && currentActivity.Parent.QualifiedName != leastCommonParent)
            {
                if (IsLoopActivity(currentActivity))
                    return true;

                currentActivity = currentActivity.Parent;
            }

            return false;
        }

        static IEnumerable<String> GetActivityPath(Activity activity)
        {
            if (activity != null)
            {
                foreach (String path in GetActivityPath(activity.Parent))
                    yield return path;

                yield return activity.QualifiedName;
            }
        }

        static String FindLeastCommonParent(IEnumerable<String> source, IEnumerable<String> dest)
        {
            IEnumerator srcEnum = source.GetEnumerator();
            IEnumerator destEnum = dest.GetEnumerator();

            String leastCommonParent = null;

            while (srcEnum.MoveNext() && destEnum.MoveNext())
            {
                if (srcEnum.Current.Equals(destEnum.Current))
                    leastCommonParent = (String)srcEnum.Current;
                else
                    return leastCommonParent;
            }

            return leastCommonParent;
        }

        internal static IEnumerable GetSucceedingActivities(Activity startActivity)
        {
            Activity currentActivity = null;
            Stack<Activity> activityStack = new Stack<Activity>();
            activityStack.Push(startActivity);

            while ((currentActivity = activityStack.Pop()) != null)
            {
                if (currentActivity is CompositeActivity && Helpers.IsCustomActivity((CompositeActivity)currentActivity))
                    break;

                if (currentActivity.Parent != null)
                {
                    bool pastCurrentActivity = false;

                    foreach (Activity siblingActivity in currentActivity.Parent.Activities)
                    {
                        if (siblingActivity == currentActivity)
                        {
                            pastCurrentActivity = true;
                            continue;
                        }

                        if (!pastCurrentActivity)
                            continue;

                        if (siblingActivity.Enabled)
                        {
                            if (siblingActivity is CompositeActivity && !Helpers.IsCustomActivity((CompositeActivity)siblingActivity))
                            {
                                foreach (Activity containedActivity in WebServiceActivityHelpers.GetContainedActivities((CompositeActivity)siblingActivity))
                                    yield return containedActivity;
                            }
                            else
                            {
                                yield return siblingActivity;
                            }
                        }
                    }
                }
                activityStack.Push(currentActivity.Parent);
            }
            yield break;
        }

        internal static void GetParameterInfo(MethodInfo methodInfo, out List<ParameterInfo> inParameters, out List<ParameterInfo> outParameters)
        {
            inParameters = new List<ParameterInfo>(); outParameters = new List<ParameterInfo>();
            foreach (ParameterInfo paramInfo in methodInfo.GetParameters())
            {
                if (paramInfo.IsOut || paramInfo.IsRetval || paramInfo.ParameterType.IsByRef)
                    outParameters.Add(paramInfo);

                if (!paramInfo.IsOut && !paramInfo.IsRetval)
                    inParameters.Add(paramInfo);
            }

            if (methodInfo.ReturnType != typeof(void))
                outParameters.Add(methodInfo.ReturnParameter);

            return;
        }

        internal static ValidationErrorCollection ValidateParameterTypes(MethodInfo methodInfo)
        {
            ValidationErrorCollection validationErrors = new ValidationErrorCollection();
            if (methodInfo == null)
                return validationErrors;
            foreach (ParameterInfo paramInfo in methodInfo.GetParameters())
            {
                if (paramInfo.ParameterType == null)
                {
                    validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ParameterTypeNotFound, methodInfo.Name, paramInfo.Name), ErrorNumbers.Error_ParameterTypeNotFound));
                }
            }

            if (methodInfo.ReturnType != typeof(void) && methodInfo.ReturnParameter.ParameterType == null)
            {
                validationErrors.Add(new ValidationError(SR.GetString(SR.Error_ReturnTypeNotFound, methodInfo.Name), ErrorNumbers.Error_ReturnTypeNotFound));
            }

            return validationErrors;
        }
    }
}
