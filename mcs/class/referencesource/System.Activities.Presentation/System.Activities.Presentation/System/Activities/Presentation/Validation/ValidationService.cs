// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.Activities.Presentation.Validation
{
    using System.Activities.Expressions;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.Services;
    using System.Activities.Presentation.View;
    using System.Activities.Validation;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel.Activities;
    using System.Text;
    using System.Threading;
    using System.Windows.Threading;
    using Microsoft.Activities.Presentation.Xaml;
    using Microsoft.Win32;

    [Fx.Tag.XamlVisible(false)]
    public class ValidationService
    {
        static PropertyInfo parentPropertyInfo;

        Dictionary<Type, IValidationErrorSourceLocator> validationErrorSourceLocators;
        List<Guid> acquiredObjectReferences;

        EditingContext context;
        ModelService modelService;
        ModelSearchServiceImpl modelSearchService;
        ModelTreeManager modelTreeManager;
        WorkflowViewService viewService;
        IValidationErrorService errorService;
        ObjectReferenceService objectReferenceService;
        TaskDispatcher validationTaskDispatcher;
        ValidationSynchronizer validationSynchronizer;

        // Dictionary which maps the object to their error messages and indicators
        // NOTE: Valid objects do not appear in this dictionary,
        // only elements which violated a constraint (Errors or Warnings or Child Validation Issues)
        Dictionary<object, ValidationErrorState> validationErrors;

        // Attached properties for error visuals
        AttachedProperty<ValidationState> validationStateProperty;
        AttachedProperty<string> validationMessageProperty;

        internal event EventHandler ValidationCompleted;

        internal class ErrorsMarkedEventArgs : EventArgs
        {
            ICollection<ValidationError> errors;
            ValidationReason reason;
            ModelTreeManager modelTreeManager;
            EditingContext context;

            public ErrorsMarkedEventArgs(ICollection<ValidationError> errors,
                ValidationReason reason,
                ModelTreeManager modelTreeManager,
                EditingContext context)
            {
                this.errors = errors;
                this.reason = reason;
                this.modelTreeManager = modelTreeManager;
                this.context = context;
            }

            public ICollection<ValidationError> Errors
            {
                get { return this.errors; }
            }

            public ValidationReason Reason
            {
                get { return this.reason; }
            }

            public ModelTreeManager ModelTreeManager
            {
                get { return this.modelTreeManager; }
            }

            public bool Handled
            {
                get;
                set;
            }

            public EditingContext Context
            {
                get { return this.context; }
            }
        }

        internal event EventHandler<ErrorsMarkedEventArgs> ErrorsMarked;

        DynamicActivity dynamicActivityWrapper;

        ValidationSettings settings;

        bool isValidationDisabled = false;
        const string ValidationRegKeyName = "DisableValidateOnModelItemChanged";
        const string ValidationRegKeyInitialPath = "Software\\Microsoft\\.NETFramework\\";
        [ThreadStatic]
        static StringBuilder errorBuilder;

        private static StringBuilder ErrorBuilder
        {
            get
            {
                if (errorBuilder == null)
                {
                    errorBuilder = new StringBuilder();
                }
                return errorBuilder;
            }
        }

        public ValidationService(EditingContext context)
            : this(context, new TaskDispatcher())
        {
        }

        internal ValidationService(EditingContext context, TaskDispatcher validationTaskDispatcher)
        {
            Fx.Assert(validationTaskDispatcher != null, "validationTaskDispatcher cannot be null.");
            this.validationTaskDispatcher = validationTaskDispatcher;
            this.context = context;
            this.settings = new ValidationSettings { SkipValidatingRootConfiguration = true };
            this.context.Services.Subscribe<ModelService>(new SubscribeServiceCallback<ModelService>(OnModelServiceAvailable));
            this.context.Services.Subscribe<ModelSearchService>(new SubscribeServiceCallback<ModelSearchService>(OnModelSearchServiceAvailable));
            this.context.Services.Subscribe<ObjectReferenceService>(new SubscribeServiceCallback<ObjectReferenceService>(OnObjectReferenceServiceAvailable));
            this.context.Services.Subscribe<ModelTreeManager>(new SubscribeServiceCallback<ModelTreeManager>(OnModelTreeManagerAvailable));
            this.context.Services.Subscribe<IValidationErrorService>(new SubscribeServiceCallback<IValidationErrorService>(OnErrorServiceAvailable));
            this.context.Services.Subscribe<AttachedPropertiesService>(new SubscribeServiceCallback<AttachedPropertiesService>(OnAttachedPropertiesServiceAvailable));
            AssemblyName currentAssemblyName = Assembly.GetExecutingAssembly().GetName();
            StringBuilder validationKeyPath = new StringBuilder(90);
            validationKeyPath.Append(ValidationRegKeyInitialPath);
            validationKeyPath.AppendFormat("{0}{1}{2}", "v", currentAssemblyName.Version.ToString(), "\\");
            validationKeyPath.Append(currentAssemblyName.Name);

            RegistryKey validationRegistryKey = Registry.CurrentUser.OpenSubKey(validationKeyPath.ToString());
            if (validationRegistryKey != null)
            {
                object value = validationRegistryKey.GetValue(ValidationRegKeyName);

                this.isValidationDisabled = (value != null && string.Equals("1", value.ToString()));

                validationRegistryKey.Close();
            }
        }

        private ValidationSynchronizer ValidationSynchronizer
        {
            get
            {
                if (this.validationSynchronizer == null)
                {
                    if (DesignerConfigurationServiceUtilities.IsBackgroundValidationEnabled(context))
                    {
                        this.validationSynchronizer = new BackgroundValidationSynchronizer<Tuple<ValidationReason, ValidationResults, Exception>>(validationTaskDispatcher, this.CoreValidationWork, this.OnValidationWorkCompleted);
                    }
                    else
                    {
                        this.validationSynchronizer = new ForegroundValidationSynchronizer<Tuple<ValidationReason, ValidationResults, Exception>>(validationTaskDispatcher, this.CoreValidationWork, this.OnValidationWorkCompleted);
                    }
                }

                return this.validationSynchronizer;
            }
        }

        internal DynamicActivity DynamicActivityWrapper
        {
            get
            {
                if (null == this.dynamicActivityWrapper)
                {
                    this.dynamicActivityWrapper = new DynamicActivity();
                }
                return this.dynamicActivityWrapper;
            }
        }

        public ValidationSettings Settings
        {
            get
            {
                return this.settings;
            }
        }

        WorkflowViewService ViewService
        {
            get
            {
                if (null == this.viewService)
                {
                    this.viewService = (WorkflowViewService)this.context.Services.GetService<ViewService>();
                }
                return this.viewService;
            }
        }

        void OnAttachedPropertiesServiceAvailable(AttachedPropertiesService attachedPropertiesService)
        {
            this.validationStateProperty = new AttachedProperty<ValidationState>()
            {
                Getter = (modelItem) => GetValidationState(modelItem),
                Name = "ValidationState",
                OwnerType = typeof(object)
            };

            attachedPropertiesService.AddProperty(this.validationStateProperty);

            this.validationMessageProperty = new AttachedProperty<string>()
            {
                Getter = (modelItem) => GetValidationMessage(modelItem),
                Name = "ValidationMessage",
                OwnerType = typeof(object)
            };

            attachedPropertiesService.AddProperty(this.validationMessageProperty);
        }

        ValidationState GetValidationState(ModelItem modelItem)
        {
            ValidationState validationState = ValidationState.Valid;
            ValidationErrorState validationError = GetValidationError(modelItem);

            if (validationError != null)
            {
                validationState = validationError.ValidationState;
            }
            return validationState;
        }

        string GetValidationMessage(ModelItem modelItem)
        {
            string errorMessage = string.Empty;
            ValidationErrorState validationError = GetValidationError(modelItem);

            if (validationError != null)
            {
                if (validationError.ErrorMessages != null)
                {
                    ValidationService.ErrorBuilder.Clear();
                    foreach (string message in validationError.ErrorMessages)
                    {
                        ValidationService.ErrorBuilder.AppendLine(message.Trim());
                    }
                    errorMessage = ValidationService.ErrorBuilder.ToString().Trim();
                }
            }
            return errorMessage;
        }

        ValidationErrorState GetValidationError(ModelItem modelItem)
        {
            ValidationErrorState validationError = null;
            this.ValidationErrors.TryGetValue(modelItem.GetCurrentValue(), out validationError);
            return validationError;
        }

        void OnModelServiceAvailable(ModelService modelService)
        {
            if (modelService != null)
            {
                this.modelService = modelService;
            }
        }

        void OnModelSearchServiceAvailable(ModelSearchService modelSearchService)
        {
            if (modelSearchService != null)
            {
                this.modelSearchService = modelSearchService as ModelSearchServiceImpl;
            }
        }

        void OnObjectReferenceServiceAvailable(ObjectReferenceService objectReferenceService)
        {
            if (objectReferenceService != null)
            {
                this.objectReferenceService = objectReferenceService;
            }
        }

        void OnModelTreeManagerAvailable(ModelTreeManager modelTreeManager)
        {
            if (modelTreeManager != null)
            {
                this.modelTreeManager = modelTreeManager;
            }
        }

        void OnErrorServiceAvailable(IValidationErrorService errorService)
        {
            if (errorService != null)
            {
                this.errorService = errorService;
                if (this.isValidationDisabled)
                {
                    this.errorService.ShowValidationErrors(new List<ValidationErrorInfo> { new ValidationErrorInfo(new ValidationError(SR.ValidationDisabledWarning, true)) });
                }
            }
        }

        public void ValidateWorkflow()
        {
            ValidateWorkflow(ValidationReason.Unknown);
        }

        private ValidationRoot GetRootElement()
        {
            Activity rootElement = null;

            Fx.Assert(this.modelService != null, "ModelService is null."); // ModelService should not be null

            ModelItem rootItem = this.modelService.Root;
            object root = rootItem.GetCurrentValue();
            // special case for WorkflowService - it will be returned directly
            WorkflowService workflowService = root as WorkflowService;
            if (workflowService != null)
            {
                return new ValidationRoot(workflowService);
            }
            //special case for ActivityBuilder - its will be converted to a DynamicActivity before validation.
            ActivityBuilder activityBuilder = root as ActivityBuilder;
            if (activityBuilder != null)
            {
                ActivityBuilderExtensions.ConvertActivityBuilderToDynamicActivity(activityBuilder, this.DynamicActivityWrapper);
                rootElement = this.DynamicActivityWrapper;
            }
            else
            {
                rootElement = rootItem.GetRootActivity();
            }

            IList<AssemblyReference> references;
            IList<string> namespaces = NamespaceHelper.GetTextExpressionNamespaces(root, out references);
            NamespaceHelper.SetTextExpressionNamespaces(rootElement, namespaces, references);

            if (rootElement != null)
            {
                return new ValidationRoot(rootElement);
            }
            else
            {
                return null;
            }
        }

        internal void ValidateWorkflow(ValidationReason reason)
        {
            if (this.isValidationDisabled)
            {
                return;
            }

            this.validationTaskDispatcher.DispatchWorkOnUIThread(DispatcherPriority.ApplicationIdle, new Action(() =>
                {
                    this.ValidationSynchronizer.Validate(reason);
                }));
        }

        internal void DeactivateValidation()
        {
            this.ValidationSynchronizer.DeactivateValidation();
        }

        internal void ActivateValidation()
        {
            this.ValidationSynchronizer.ActivateValidation();
        }

        [SuppressMessage(FxCop.Category.Design, FxCop.Rule.DoNotCatchGeneralExceptionTypes)]
        internal Tuple<ValidationReason, ValidationResults, Exception> CoreValidationWork(ValidationReason reason, CancellationToken cancellationToken)
        {
            this.settings.CancellationToken = cancellationToken;
            ValidationResults results = null;
            Exception exception = null;
            try
            {
                ValidationRoot rootElement = this.GetRootElement();
                if (rootElement != null)
                {
                    results = rootElement.Validate(this.Settings);
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e) || e is OperationCanceledException)
                {
                    throw;
                }

                exception = e;
            }

            return Tuple.Create(reason, results, exception);
        }

        private void OnValidationWorkCompleted(Tuple<ValidationReason, ValidationResults, Exception> input)
        {
            ValidationReason reason = input.Item1;
            ValidationResults results = input.Item2;
            Exception exception = input.Item3;

            Fx.Assert(results != null ^ exception != null, "result and exception should not both be null");

            bool needsToMarkValidationErrors = false;
            ValidationErrorInfo validationErrorInfo = null;
            if (exception != null)
            {
                ModelItem rootModelItem = this.modelService.Root;
                Activity rootActivity = rootModelItem.GetRootActivity();

                if (rootActivity != null)
                {
                    // We don't want any crash propagating from here as it causes VS to crash.
                    if (!this.ValidationErrors.ContainsKey(rootActivity))
                    {
                        ValidationErrorState validationError = new ValidationErrorState(new List<string>(), ValidationState.Error);
                        this.ValidationErrors.Add(rootActivity, validationError);
                    }
                    else
                    {
                        this.ValidationErrors[rootActivity].ValidationState = ValidationState.Error;
                    }

                    this.ValidationErrors[rootActivity].ErrorMessages.Add(exception.ToString());

                    // Notify an update to the attached properties
                    this.NotifyValidationPropertiesChanged(rootModelItem);
                }

                validationErrorInfo = new ValidationErrorInfo(exception.ToString());
                needsToMarkValidationErrors = true;
            }

            DesignerPerfEventProvider perfProvider = this.context.Services.GetService<DesignerPerfEventProvider>();
            perfProvider.WorkflowDesignerValidationStart();

            List<ValidationError> validationErrors = null;
            if (results != null)
            {
                validationErrors = new List<ValidationError>(results.Errors);
                validationErrors.AddRange(results.Warnings);
                Activity rootActivity = this.modelService.Root.GetRootActivity();
                needsToMarkValidationErrors = this.MarkErrors(validationErrors, reason, rootActivity);
            }

            if (this.errorService != null && needsToMarkValidationErrors) // Error service could be null if no implementation has been provided
            {
                List<ValidationErrorInfo> errors = new List<ValidationErrorInfo>();

                if (validationErrors != null)
                {
                    foreach (ValidationError validationError in validationErrors)
                    {
                        Activity currentActivity = validationError.Source;
                        ValidationErrorInfo error = new ValidationErrorInfo(validationError);

                        // The acquired activity reference will be release in the Main AppDomain when it clear the error list
                        if (validationError.SourceDetail != null)
                        {
                            error.SourceReferenceId = this.objectReferenceService.AcquireObjectReference(validationError.SourceDetail);
                        }
                        else if (validationError.Source != null)
                        {
                            error.SourceReferenceId = this.objectReferenceService.AcquireObjectReference(validationError.Source);
                        }
                        else
                        {
                            error.SourceReferenceId = Guid.Empty;
                        }
                        errors.Add(error);
                    }
                }

                if (validationErrorInfo != null)
                {
                    errors.Add(validationErrorInfo);
                }

                foreach (Guid acquiredObjectReference in this.AcquiredObjectReferences)
                {
                    this.objectReferenceService.ReleaseObjectReference(acquiredObjectReference);
                }

                this.AcquiredObjectReferences.Clear();

                foreach (ValidationErrorInfo error in errors)
                {
                    if (error.SourceReferenceId != Guid.Empty)
                    {
                        this.AcquiredObjectReferences.Add(error.SourceReferenceId);
                    }
                }

                this.errorService.ShowValidationErrors(errors);
            }

            perfProvider.WorkflowDesignerValidationEnd();
            this.OnValidationCompleted();
        }

        protected virtual void OnValidationCompleted()
        {
            if (this.ValidationCompleted != null)
            {
                this.ValidationCompleted(this, new EventArgs());
            }
        }
        
        //  Find model item and properly create it if necessary.
        internal static ModelItem FindModelItem(ModelTreeManager modelTreeManager, object sourceDetail)
        {
            if (sourceDetail == null)
            {
                return null;
            }

            Fx.Assert(modelTreeManager != null, "modelTreeManager != null");

            Activity element = sourceDetail as Activity;
            object errorTarget = sourceDetail;

            // if source detail is not an Activity, we just expand the model tree to search it.
            if (element == null)
            {
                return ModelTreeManager.FindFirst(modelTreeManager.Root, (modelItem) => (modelItem.GetCurrentValue() == errorTarget));
            }
            else
            {
                return FindActivityModelItem(modelTreeManager, element);
            }
        }
        
        internal static Activity GetParent(Activity childActivity)
        {
            // Obtaining the parent from childActivity using (private) reflection.
            if (parentPropertyInfo == null)
            {
                parentPropertyInfo = typeof(Activity).GetProperty("Parent", BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic);
            }
            Fx.Assert(parentPropertyInfo != null, "Activity.Parent is not defined");
            return parentPropertyInfo.GetValue(childActivity, null) as Activity;
        }

        // Get the parent chain of this activity.
        // Can't use GetParentChain activity because it can only be used in a Constraint.
        internal static List<Activity> GetParentChain(Activity activity)
        {
            List<Activity> parentChain = new List<Activity>();
            while (activity != null)
            {
                activity = GetParent(activity);
                if (activity != null)
                {
                    parentChain.Add(activity);
                }
            }
            return parentChain;
        }

        private List<object> GetParentChainWithSource(Activity activity)
        {
            List<object> parentChain = new List<object>();
            parentChain.Add(activity);
            while (activity != null)
            {
                activity = GetParent(activity);
                if (activity != null)
                {
                    IValidationErrorSourceLocator validationErrorSourceLocator = this.GetValidationErrorSourceLocator(activity.GetType());
                    if (validationErrorSourceLocator != null)
                    {
                        validationErrorSourceLocator.ReplaceParentChainWithSource(activity, parentChain);
                    }
                    else
                    {
                        parentChain.Add(activity);
                    }
                }
            }

            parentChain.RemoveAt(0);
            return parentChain;
        }

        // Mark all the errors including their parent chains
        private bool MarkErrors(ICollection<ValidationError> errors, ValidationReason reason, Activity rootActivity)
        {
            // Clear the previous errors/warnings and update the visuals
            ClearErrors();
            Fx.Assert(this.modelTreeManager != null, "ModelTreeManager is null."); // ModelTreeManager should not be null

            if (this.HandleErrorsMarked(errors, reason))
            {
                return false;
            }

            // Iterate through the new violation list and mark errors/warnings
            foreach (ValidationError error in errors)
            {
                if (error.Source != null)
                {
                    List<object> errorSourcePath = this.GetValidationErrorSourcePath(error.Source, error.SourceDetail);
                    MarkError(error, errorSourcePath);
                }
                else if (error.SourceDetail != null && error.SourceDetail is Receive)
                {
                    // special-case:
                    // WorkflowService.Validate() may produce ValidationError { isWarning = true, Source = null, SourceDetail = Receive activity }

                    List<object> errorSourcePath = this.GetValidationErrorSourcePath((Activity)error.SourceDetail, null);
                    MarkError(error, errorSourcePath);
                }
                else if (rootActivity != null)
                {
                    List<object> errorSourcePath = this.GetValidationErrorSourcePath(rootActivity, error.SourceDetail);
                    MarkError(error, errorSourcePath);
                }
            }

            return true;
        }

        // Mark a single error including its parent chain
        private void MarkError(ValidationError validationError, List<object> errorSourcePath)
        {
            object errorSource = errorSourcePath[0];
            this.MarkCulprit(errorSource, validationError);

            // Intentionally skipping the zeroth errorSourcePath because that is the culprit and is marked above.
            for (int errorSourcePathIndex = 1; errorSourcePathIndex < errorSourcePath.Count; errorSourcePathIndex++)
            {
                this.MarkParent(validationError, errorSourcePath[errorSourcePathIndex]);
            }

            foreach (object parent in this.GetParentChainWithSource(validationError.Source))
            {
                this.MarkParent(validationError, parent);
            }
        }

        // Mark a single error on the culprit
        private void MarkCulprit(object errorSource, ValidationError validationError)
        {
            ValidationErrorState currentError;
            if (!this.ValidationErrors.TryGetValue(errorSource, out currentError))
            {
                currentError = new ValidationErrorState(new List<string>(), ValidationState.Valid);
                this.ValidationErrors.Add(errorSource, currentError);
            }
            MergeValidationError(currentError, validationError);
            this.NotifyValidationPropertiesChanged(errorSource);
        }

        // Mark a single "child has an error" on a parent
        private void MarkParent(ValidationError validationError, object errorParent)
        {
            ValidationState childValidationState = GetValidationState(validationError);
            ValidationErrorState currentError;
            if (!this.ValidationErrors.TryGetValue(errorParent, out currentError))
            {
                currentError = ChildInvalidError();
                this.ValidationErrors.Add(errorParent, currentError);
            }

            if (currentError.ValidationState < childValidationState)
            {
                currentError.ValidationState = childValidationState;
            }

            this.NotifyValidationPropertiesChanged(errorParent);
        }

        private void NotifyValidationPropertiesChanged(object errorItem)
        {
            ModelItem errorModelItem = this.modelTreeManager.GetModelItem(errorItem);
            if (errorModelItem != null)
            {
                this.NotifyValidationPropertiesChanged(errorModelItem);
            }
        }

        private void NotifyValidationPropertiesChanged(ModelItem modelItem)
        {
            // Notify an update to the attached properties
            this.validationStateProperty.NotifyPropertyChanged(modelItem);
            this.validationMessageProperty.NotifyPropertyChanged(modelItem);
        }

        private bool HandleErrorsMarked(ICollection<ValidationError> errors, ValidationReason reason)
        {
            if (this.ErrorsMarked != null)
            {
                ErrorsMarkedEventArgs arg = new ErrorsMarkedEventArgs(errors, reason, this.modelTreeManager, this.context);
                this.ErrorsMarked(this, arg);
                return arg.Handled;
            }

            return false;
        }

        private static ModelItem FindActivityModelItem(ModelTreeManager modelTreeManager, Activity errorTarget)
        {
            // Search the lowest Activity
            ModelItem lowestModelItem = null;
            List<Activity> parentChain = GetParentChain(errorTarget);
            Fx.Assert(parentChain != null, "Cannot find parent chain for " + errorTarget.DisplayName);

            foreach (Activity parent in parentChain)
            {
                lowestModelItem = modelTreeManager.GetModelItem(parent);
                if (lowestModelItem != null)
                {
                    break;
                }
            }

            ModelItem foundItem = null;
            // Find in nearest parent first.
            if (lowestModelItem != null)
            {
                // The foundItem could be null because lowestModelItem is not errorTarget's parent any more.
                // This happens if background validation hasn't finished updating errorTarget's parent.
                foundItem = ModelTreeManager.FindFirst(lowestModelItem, (modelItem) => (modelItem.GetCurrentValue() == errorTarget));
            }

            // Not found, search from root.
            if (foundItem == null)
            {
                foundItem = FindActivityModelItemFromRoot(modelTreeManager, errorTarget);
            }

            return foundItem;
        }

        private static ModelItem FindActivityModelItemFromRoot(ModelTreeManager modelTreeManager, Activity errorTarget)
        {
            ModelItem root = modelTreeManager.Root;
            Fx.Assert(root != null && errorTarget != null, "root != null && errorTarget != null");
            ModelProperty property = root.Properties["Properties"];

            ModelItem propertiesModelItem = property == null ? null : property.Value;
            ModelItem foundItem = null;
            if (propertiesModelItem != null)
            {
                // So,search "Properties" first to delay expanding "Implementation" and other properties.
                foundItem = ModelTreeManager.FindFirst(propertiesModelItem, (modelItem) => (modelItem.GetCurrentValue() == errorTarget));
            }

            // If activity is not in Properties, expand others except Properties.
            foundItem = foundItem ?? ModelTreeManager.FindFirst(
                root, 
                (modelItem) => (modelItem.GetCurrentValue() == errorTarget),
                (modelItem) => { return modelItem != propertiesModelItem; });

            return foundItem;
        }

        private static ValidationState GetValidationState(ValidationError validationError)
        {
            return validationError.IsWarning ? ValidationState.Warning : ValidationState.Error;
        }

        private static ValidationErrorState ChildInvalidError()
        {
            return new ValidationErrorState(new List<string> { SR.ChildValidationError }, ValidationState.ChildInvalid);
        }

        private static void MergeValidationError(ValidationErrorState originalError, ValidationError newError)
        {
            if (originalError.ValidationState == ValidationState.ChildInvalid)
            {
                // If original error is due to child's issue, clear the error list, 
                // as we don't care about its child's issues anymore and want to add its own issues.
                originalError.ErrorMessages.Clear();
            }

            ValidationState errorState = GetValidationState(newError);
            if (originalError.ValidationState < errorState)
            {
                // Promote to the higher level of violation.
                originalError.ValidationState = errorState;
            }

            if (newError.IsWarning)
            {
                originalError.ErrorMessages.Add(string.Format(CultureInfo.CurrentUICulture, SR.WarningFormat, newError.Message));
            }
            else
            {
                originalError.ErrorMessages.Add(newError.Message);
            }
        }

        void ClearErrors()
        {
            // Copy over the previously marked model items before you clear the dictionaries
            object[] oldErrorList = new object[this.ValidationErrors.Count];
            this.ValidationErrors.Keys.CopyTo(oldErrorList, 0);

            this.ValidationErrors.Clear();

            // Iterate through the previously marked model items and notify an update to the attached properties
            ModelItem modelItem;
            foreach (object workflowElement in oldErrorList)
            {
                modelItem = this.modelTreeManager.GetModelItem(workflowElement);
                if (modelItem != null)
                {
                    NotifyValidationPropertiesChanged(modelItem);
                }
            }
        }

        public void NavigateToError(ValidationErrorInfo validationErrorInfo)
        {
            if (validationErrorInfo == null)
            {
                throw FxTrace.Exception.ArgumentNull("validationErrorInfo");
            }

            object sourceDetail = this.GetSourceDetail(validationErrorInfo);
            this.NavigateToErrorOnDispatcherThread(sourceDetail);
        }

        private object GetSourceDetail(ValidationErrorInfo validationErrorInfo)
        {
            Fx.Assert(validationErrorInfo != null, "validationErrorInfo should not be null and is checked by caller.");
            Guid sourceReferenceId = validationErrorInfo.SourceReferenceId;
            object sourceDetail = null;

            if (sourceReferenceId == Guid.Empty)
            {
                if (this.modelTreeManager.Root != null)
                {
                    sourceDetail = modelTreeManager.Root.GetCurrentValue();
                }
            }
            else
            {
                if (!this.objectReferenceService.TryGetObject(sourceReferenceId, out sourceDetail))
                {
                    throw FxTrace.Exception.Argument("validationErrorInfo", string.Format(CultureInfo.CurrentUICulture, SR.SourceReferenceIdNotFoundInWorkflow, sourceReferenceId));
                }
            }
            return sourceDetail;
        }

        private void NavigateToErrorOnDispatcherThread(object sourceDetail)
        {
            this.validationTaskDispatcher.DispatchWorkOnUIThread(DispatcherPriority.ApplicationIdle, new Action(
            () =>
            {
                this.NavigateToError(sourceDetail);
            }));
        }

        public void NavigateToError(string id)
        {
            if (id == null)
            {
                throw FxTrace.Exception.ArgumentNull("id");
            }

            ValidationRoot rootElement = this.GetRootElement();
            if (rootElement != null)
            {
                Activity errorElement = rootElement.Resolve(id);
                this.NavigateToErrorOnDispatcherThread(errorElement);
            }
        }

        void NavigateToError(object sourceDetail)
        {
            Fx.Assert(this.modelTreeManager != null, "ModelTreeManager is null.");
            ModelItem modelItem = this.modelTreeManager.GetModelItem(sourceDetail) ?? FindModelItem(this.modelTreeManager, sourceDetail);

            if (modelItem != null)
            {
                if (this.modelSearchService != null)
                {
                    this.modelSearchService.NavigateTo(modelItem);
                }
                else
                {
                    // For any Expression, need to focus to its parent instead.
                    Activity activity = modelItem.GetCurrentValue() as Activity;
                    if (activity != null && (activity.IsExpression()))
                    {
                        ModelItem parent = modelItem.Parent;
                        while (parent != null)
                        {
                            bool hasDesignerAttribute = this.ViewService.GetDesignerType(parent.ItemType) != null;

                            // ModelItemKeyValuePair type also has DesignerAttribute.
                            // Since we do not want to put a focus on that type, special-casing it here.
                            bool isModelItemKeyValuePair = parent.ItemType.IsGenericType &&
                            parent.ItemType.GetGenericTypeDefinition() == typeof(ModelItemKeyValuePair<,>);

                            if (hasDesignerAttribute && !isModelItemKeyValuePair)
                            {
                                break;
                            }

                            parent = parent.Parent;
                        }

                        if (parent != null)
                        {
                            modelItem = parent;
                        }
                    }
                    modelItem.Focus();
                }
            }
        }

        internal void RegisterValidationErrorSourceLocator(Type activityType, IValidationErrorSourceLocator validationErrorSourceLocator)
        {
            if (validationErrorSourceLocator == null)
            {
                throw FxTrace.Exception.ArgumentNull("validationErrorSourceLocator");
            }
            this.ValidationErrorSourceLocators.Add(activityType, validationErrorSourceLocator);
        }

        List<object> GetValidationErrorSourcePath(Activity violatingActivity, object sourceDetail)
        {
            IValidationErrorSourceLocator validationErrorSourceLocator = GetValidationErrorSourceLocator(violatingActivity.GetType());
            if (validationErrorSourceLocator == null)
            {
                return new List<object> { violatingActivity };
            }
            else
            {
                return validationErrorSourceLocator.FindSourceDetailFromActivity(violatingActivity, sourceDetail);
            }
        }

        IValidationErrorSourceLocator GetValidationErrorSourceLocator(Type typeOfActivityWithValidationError)
        {
            IValidationErrorSourceLocator validationErrorSourceLocator;
            if (this.ValidationErrorSourceLocators.TryGetValue(typeOfActivityWithValidationError, out validationErrorSourceLocator))
            {
                Fx.Assert(validationErrorSourceLocator != null, "Ensured by RegisterValidationErrorSourceLocator");
                return validationErrorSourceLocator;
            }
            else if (typeOfActivityWithValidationError.IsGenericType && !typeOfActivityWithValidationError.IsGenericTypeDefinition)
            {
                return this.GetValidationErrorSourceLocator(typeOfActivityWithValidationError.GetGenericTypeDefinition());
            }
            else
            {
                return null;
            }
        }

        // Properties
        Dictionary<object, ValidationErrorState> ValidationErrors
        {
            get
            {
                if (this.validationErrors == null)
                {
                    this.validationErrors = new Dictionary<object, ValidationErrorState>();
                }
                return this.validationErrors;
            }
        }

        Dictionary<Type, IValidationErrorSourceLocator> ValidationErrorSourceLocators
        {
            get
            {
                if (this.validationErrorSourceLocators == null)
                {
                    this.validationErrorSourceLocators = new Dictionary<Type, IValidationErrorSourceLocator>();
                }
                return this.validationErrorSourceLocators;
            }
        }

        List<Guid> AcquiredObjectReferences
        {
            get
            {
                if (this.acquiredObjectReferences == null)
                {
                    this.acquiredObjectReferences = new List<Guid>();
                }

                return this.acquiredObjectReferences;
            }
        }

        internal AttachedProperty<ValidationState> ValidationStateProperty
        {
            get
            {
                return this.validationStateProperty;
            }
        }

        internal AttachedProperty<string> ValidationMessageProperty
        {
            get
            {
                return this.validationMessageProperty;
            }
        }

        class ValidationErrorState
        {
            internal ValidationErrorState(List<string> errorMessages, ValidationState validationState)
            {
                this.ErrorMessages = errorMessages;
                this.ValidationState = validationState;
            }

            internal List<string> ErrorMessages { get; set; }
            internal ValidationState ValidationState { get; set; }
        }
    }
}
