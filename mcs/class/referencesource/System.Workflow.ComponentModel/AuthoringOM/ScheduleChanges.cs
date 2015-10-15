#pragma warning disable 1634, 1691
namespace System.Workflow.ComponentModel
{
    #region Imports

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.Reflection;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;
    using System.Workflow.ComponentModel.Design;
    using System.Xml;
    using System.IO;

    #endregion

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowChanges
    {
        #region Data members
        public static readonly DependencyProperty ConditionProperty = DependencyProperty.RegisterAttached("Condition", typeof(ActivityCondition), typeof(WorkflowChanges), new PropertyMetadata(DependencyPropertyOptions.Metadata));
        internal static DependencyProperty WorkflowChangeActionsProperty = DependencyProperty.RegisterAttached("WorkflowChangeActions", typeof(IList), typeof(WorkflowChanges), new PropertyMetadata(DependencyPropertyOptions.NonSerialized));
        internal static DependencyProperty WorkflowChangeVersionProperty = DependencyProperty.RegisterAttached("WorkflowChangeVersion", typeof(Guid), typeof(WorkflowChanges), new PropertyMetadata(Guid.Empty, DependencyPropertyOptions.NonSerialized));

        private Activity originalRootActivity = null;
        private Activity clonedRootActivity = null;

        private List<WorkflowChangeAction> modelChangeActions = new List<WorkflowChangeAction>();
        private bool saved = false;

        #endregion

        #region Constuctor & Destructor

        public WorkflowChanges(Activity rootActivity)
        {
            if (rootActivity == null)
                throw new ArgumentNullException("rootActivity");
            if (!(rootActivity is CompositeActivity) || rootActivity.Parent != null)
                throw new ArgumentException(SR.GetString(SR.Error_RootActivityTypeInvalid2), "rootActivity");
#pragma warning suppress 56506
            if (rootActivity.DesignMode)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            // get the original activity
            this.originalRootActivity = (Activity)((Activity)rootActivity).GetValue(Activity.WorkflowDefinitionProperty);
            if (this.originalRootActivity == null)
                this.originalRootActivity = rootActivity;

            // Work around: for dynamic update create a clone, without calling initialize for runtime
            this.clonedRootActivity = (Activity)CloneRootActivity(originalRootActivity);

            // make the tree readonly
            ApplyDynamicUpdateMode((Activity)this.clonedRootActivity);
        }

        #endregion

        #region Public members
        // WhenConditionProperty Get and Set Accessors
        public static object GetCondition(object dependencyObject)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException("dependencyObject");
            if (!(dependencyObject is DependencyObject))
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(DependencyObject).FullName), "dependencyObject");

            return (dependencyObject as DependencyObject).GetValue(ConditionProperty);
        }

        public static void SetCondition(object dependencyObject, object value)
        {
            if (dependencyObject == null)
                throw new ArgumentNullException("dependencyObject");
            if (!(dependencyObject is DependencyObject))
                throw new ArgumentException(SR.GetString(SR.Error_UnexpectedArgumentType, typeof(DependencyObject).FullName), "dependencyObject");

            (dependencyObject as DependencyObject).SetValue(ConditionProperty, value);
        }

        public CompositeActivity TransientWorkflow
        {
            get
            {
                return this.clonedRootActivity as CompositeActivity;
            }
        }

        public ValidationErrorCollection Validate()
        {
            TypeProvider typeProvider = CreateTypeProvider(this.originalRootActivity);

            // create service provider
            ServiceContainer serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(ITypeProvider), typeProvider);

            ValidationManager validationManager = new ValidationManager(serviceContainer);
            ValidationErrorCollection errors;
            using (WorkflowCompilationContext.CreateScope(validationManager))
            {
                errors = ValidationHelpers.ValidateObject(validationManager, this.clonedRootActivity);
            }
            return XomlCompilerHelper.MorphIntoFriendlyValidationErrors(errors);
        }

        private void Save()
        {
            ValidationErrorCollection errors = Validate();
            if (errors.HasErrors)
                throw new WorkflowValidationFailedException(SR.GetString(SR.Error_CompilerValidationFailed), errors);

            //work around !!!for conditions we do diff 
            object originalConditions = ((Activity)this.originalRootActivity).GetValue(ConditionTypeConverter.DeclarativeConditionDynamicProp);
            object changedConditions = ((Activity)this.clonedRootActivity).GetValue(ConditionTypeConverter.DeclarativeConditionDynamicProp);
            if (null != originalConditions)
                this.modelChangeActions.AddRange(((IWorkflowChangeDiff)originalConditions).Diff(originalConditions, changedConditions));
            else if (null != changedConditions)
                this.modelChangeActions.AddRange(((IWorkflowChangeDiff)changedConditions).Diff(originalConditions, changedConditions));

            // diff the process model
            this.modelChangeActions.AddRange(DiffTrees(this.originalRootActivity as CompositeActivity, this.clonedRootActivity as CompositeActivity));

            // always call it after diff tree, otherwise it turns on the Locked.
            ReleaseDynamicUpdateMode((Activity)this.clonedRootActivity);

            // cache the change actions into the new workflow definition
            ArrayList workflowChanges = (ArrayList)((Activity)this.clonedRootActivity).GetValue(WorkflowChanges.WorkflowChangeActionsProperty);
            if (workflowChanges == null)
            {
                workflowChanges = new ArrayList();
                ((Activity)this.clonedRootActivity).SetValue(WorkflowChanges.WorkflowChangeActionsProperty, workflowChanges);
            }

            workflowChanges.AddRange(this.modelChangeActions);
            ((Activity)this.clonedRootActivity).SetValue(WorkflowChanges.WorkflowChangeVersionProperty, Guid.NewGuid());
            this.saved = true;

            // now initialize for runtime
            ((IDependencyObjectAccessor)this.clonedRootActivity).InitializeDefinitionForRuntime(null);
        }

        internal void ApplyTo(Activity activity)
        {
            if (activity == null)
                throw new ArgumentNullException("activity");

            if (activity.Parent != null)
                throw new ArgumentException(SR.GetString(SR.Error_RootActivityTypeInvalid), "activity");

            if (activity.RootActivity == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_MissingRootActivity));

            if (activity.WorkflowCoreRuntime == null)
                throw new InvalidOperationException(SR.GetString(SR.Error_NoRuntimeAvailable));

            if (this.saved)
                throw new InvalidOperationException(SR.GetString(SR.Error_TransactionAlreadyApplied));

            if (!CompareWorkflowDefinition((Activity)this.originalRootActivity, (Activity)activity.RootActivity.GetValue(Activity.WorkflowDefinitionProperty)))
                throw new ArgumentException(SR.GetString(SR.Error_WorkflowDefinitionModified), "activity");

            this.Save();

            // go up in the chain and then apply changes
            IWorkflowCoreRuntime workflowCoreRuntime = activity.WorkflowCoreRuntime;
            if (workflowCoreRuntime.CurrentAtomicActivity != null)
                throw new InvalidOperationException(SR.GetString(SR.Error_InsideAtomicScope));
            bool suspended = workflowCoreRuntime.SuspendInstance(SR.GetString(SR.SuspendReason_WorkflowChange));
            try
            {
                // collect all context Activities
                List<Activity> contextActivities = new List<Activity>();
                Queue<Activity> contextActivitiesQueue = new Queue<Activity>();
                contextActivitiesQueue.Enqueue(workflowCoreRuntime.RootActivity);
                while (contextActivitiesQueue.Count > 0)
                {
                    Activity contextActivity = contextActivitiesQueue.Dequeue();
                    contextActivities.Add(contextActivity);

                    // enqueue child context Activities
                    IList<Activity> nestedContextActivities = (IList<Activity>)contextActivity.GetValue(Activity.ActiveExecutionContextsProperty);
                    if (nestedContextActivities != null)
                    {
                        foreach (Activity nestedContextActivity in nestedContextActivities)
                            contextActivitiesQueue.Enqueue(nestedContextActivity);
                    }
                }

                // run instance level validations
                ValidationErrorCollection validationErrors = new ValidationErrorCollection();
                foreach (WorkflowChangeAction changeAction in this.modelChangeActions)
                {
                    if (changeAction is ActivityChangeAction)
                    {
                        foreach (Activity contextActivity in contextActivities)
                        {
                            // WinOE 

                            if (changeAction is RemovedActivityAction &&
                                contextActivity.DottedPath == ((RemovedActivityAction)changeAction).OriginalRemovedActivity.DottedPath)
                                validationErrors.AddRange(changeAction.ValidateChanges(contextActivity));

                            // Ask the parent context activity whether or not this child activity can be added or removed.
                            // The call to TraverseDottedPathFromRoot here should return the parent context activity for this change action.
                            if (contextActivity.TraverseDottedPathFromRoot(((ActivityChangeAction)changeAction).OwnerActivityDottedPath) != null)
                                validationErrors.AddRange(changeAction.ValidateChanges(contextActivity));
                        }
                    }
                }

                // if errors then return
                if (validationErrors.HasErrors)
                    throw new WorkflowValidationFailedException(SR.GetString(SR.Error_RuntimeValidationFailed), validationErrors);

                // verify if workflow can be changed
                VerifyWorkflowCanBeChanged(workflowCoreRuntime);

                // inform workflow runtime
                workflowCoreRuntime.OnBeforeDynamicChange(this.modelChangeActions);

                // set the new Workflow Definition
                workflowCoreRuntime.RootActivity.SetValue(Activity.WorkflowDefinitionProperty, this.clonedRootActivity);

                // apply changes to all context Activities
                foreach (Activity contextActivity in contextActivities)
                {
                    // apply change to state reader
                    foreach (WorkflowChangeAction changeAction in this.modelChangeActions)
                    {
                        if (changeAction is ActivityChangeAction)
                        {
                            if (contextActivity.TraverseDottedPathFromRoot(((ActivityChangeAction)changeAction).OwnerActivityDottedPath) != null)
                            {
                                bool result = changeAction.ApplyTo(contextActivity);
                                Debug.Assert(result, "ApplyTo failed");
                            }
                        }
                    }
                    // fixup meta properties and notify changes
                    // if the context activity is the one that's being removed, we do not fixup the meta properties.
                    Activity clonedActivity = ((Activity)this.clonedRootActivity).GetActivityByName(contextActivity.QualifiedName);
                    if (clonedActivity != null)
                        contextActivity.FixUpMetaProperties(clonedActivity);
                    NotifyChangesToChildExecutors(workflowCoreRuntime, contextActivity, this.modelChangeActions);
                    NotifyChangesCompletedToChildExecutors(workflowCoreRuntime, contextActivity);
                }

                // inform workflow runtime
                workflowCoreRuntime.OnAfterDynamicChange(true, this.modelChangeActions);
            }
            catch
            {
                workflowCoreRuntime.OnAfterDynamicChange(false, this.modelChangeActions);
                throw;
            }
            finally
            {
                if (suspended)
                    workflowCoreRuntime.Resume();
            }
        }


        #endregion

        #region Internal Helpers
        private void OnActivityListChanged(object sender, ActivityCollectionChangeEventArgs e)
        {
            if (e.RemovedItems != null)
            {
                foreach (Activity removedActivity in e.RemovedItems)
                {
                    if (removedActivity.Readonly)
                        ReleaseDynamicUpdateMode(removedActivity);
                }
            }
        }
        private void ApplyDynamicUpdateMode(Activity seedActivity)
        {
            Queue<Activity> queue = new Queue<Activity>();
            queue.Enqueue(seedActivity);
            while (queue.Count > 0)
            {
                Activity activity = queue.Dequeue();
                activity.Readonly = true;
                activity.DynamicUpdateMode = true;
                foreach (DependencyProperty dependencyProperty in activity.MetaDependencyProperties)
                {
                    if (activity.IsBindingSet(dependencyProperty))
                    {
                        ActivityBind activityBind = activity.GetBinding(dependencyProperty);
                        if (activityBind != null)
                            activityBind.DynamicUpdateMode = true;
                    }
                }

                if (activity is CompositeActivity)
                {
                    CompositeActivity compositeActivity = activity as CompositeActivity;
                    compositeActivity.Activities.ListChanged += new EventHandler<ActivityCollectionChangeEventArgs>(this.OnActivityListChanged);
                    foreach (Activity activity2 in ((CompositeActivity)activity).Activities)
                        queue.Enqueue(activity2);
                }
            }
        }
        private void ReleaseDynamicUpdateMode(Activity seedActivity)
        {
            Queue<Activity> queue = new Queue<Activity>();
            queue.Enqueue(seedActivity);
            while (queue.Count > 0)
            {
                Activity activity = queue.Dequeue() as Activity;
                activity.Readonly = false;
                activity.DynamicUpdateMode = false;
                foreach (DependencyProperty dependencyProperty in activity.MetaDependencyProperties)
                {
                    if (activity.IsBindingSet(dependencyProperty))
                    {
                        ActivityBind activityBind = activity.GetBinding(dependencyProperty);
                        if (activityBind != null)
                            activityBind.DynamicUpdateMode = false;
                    }
                }
                if (activity is CompositeActivity)
                {
                    CompositeActivity compositeActivity = activity as CompositeActivity;
                    compositeActivity.Activities.ListChanged -= new EventHandler<ActivityCollectionChangeEventArgs>(this.OnActivityListChanged);
                    foreach (Activity activity2 in ((CompositeActivity)activity).Activities)
                        queue.Enqueue(activity2);
                }
            }
        }
        private void VerifyWorkflowCanBeChanged(IWorkflowCoreRuntime workflowCoreRuntime)
        {
            // check if the update is allowed on this root-activity.
            ActivityCondition dynamicUpdateCondition = ((Activity)workflowCoreRuntime.RootActivity).GetValue(WorkflowChanges.ConditionProperty) as ActivityCondition;
            if (dynamicUpdateCondition != null)
            {
                using (workflowCoreRuntime.SetCurrentActivity(workflowCoreRuntime.RootActivity))
                {
                    if (!dynamicUpdateCondition.Evaluate(workflowCoreRuntime.RootActivity, workflowCoreRuntime))
                        throw new InvalidOperationException(SR.GetString(CultureInfo.CurrentCulture, SR.Error_DynamicUpdateEvaluation, new object[] { workflowCoreRuntime.InstanceID.ToString() }));
                }
            }
        }
        private void NotifyChangesCompletedToChildExecutors(IWorkflowCoreRuntime workflowCoreRuntime, Activity contextActivity)
        {
            Queue compositeActivities = new Queue();
            compositeActivities.Enqueue(contextActivity);
            while (compositeActivities.Count > 0)
            {
                CompositeActivity compositeActivity = compositeActivities.Dequeue() as CompositeActivity;
                if (compositeActivity == null || !WorkflowChanges.IsActivityExecutable(compositeActivity))
                    continue;

                ISupportWorkflowChanges compositeActivityExecutor = ActivityExecutors.GetActivityExecutor(compositeActivity) as ISupportWorkflowChanges;
                if (compositeActivityExecutor != null)
                {
                    using (workflowCoreRuntime.SetCurrentActivity(compositeActivity))
                    {
                        using (ActivityExecutionContext executionContext = new ActivityExecutionContext(compositeActivity))
                            compositeActivityExecutor.OnWorkflowChangesCompleted(executionContext);
                    }
                }
                foreach (Activity activity in compositeActivity.Activities)
                {
                    if (activity is CompositeActivity)
                        compositeActivities.Enqueue(activity);
                }
            }
        }

        //
        internal static bool IsActivityExecutable(Activity activity)
        {
            if (!activity.Enabled)
                return false;
            if (activity.Parent != null)
                return IsActivityExecutable(activity.Parent);
            return activity.Enabled;
        }

        private void NotifyChangesToChildExecutors(IWorkflowCoreRuntime workflowCoreRuntime, Activity contextActivity, IList<WorkflowChangeAction> changeActions)
        {
            foreach (WorkflowChangeAction action in changeActions)
            {
                if (!(action is ActivityChangeAction))
                    continue;

                CompositeActivity ownerActivity = contextActivity.TraverseDottedPathFromRoot(((ActivityChangeAction)action).OwnerActivityDottedPath) as CompositeActivity;
                if (ownerActivity == null || !WorkflowChanges.IsActivityExecutable(ownerActivity))
                    continue;

                ISupportWorkflowChanges compositeActivityExecutor = ActivityExecutors.GetActivityExecutor(ownerActivity) as ISupportWorkflowChanges;
                if (compositeActivityExecutor == null)
                    throw new ApplicationException(SR.GetString(SR.Error_WorkflowChangesNotSupported, ownerActivity.GetType().FullName));

                using (workflowCoreRuntime.SetCurrentActivity(ownerActivity))
                {
                    using (ActivityExecutionContext executionContext = new ActivityExecutionContext(ownerActivity))
                    {
                        if (action is AddedActivityAction)
                        {
                            Activity addedActivity = ownerActivity.Activities[((AddedActivityAction)action).Index];
                            if (WorkflowChanges.IsActivityExecutable(addedActivity))
                            {
                                addedActivity.OnActivityExecutionContextLoad(executionContext.Activity.RootActivity.WorkflowCoreRuntime);
                                executionContext.InitializeActivity(addedActivity);
                                compositeActivityExecutor.OnActivityAdded(executionContext, addedActivity);
                            }
                        }
                        else if (action is RemovedActivityAction)
                        {
                            RemovedActivityAction removedActivityAction = (RemovedActivityAction)action;
                            if (WorkflowChanges.IsActivityExecutable(removedActivityAction.OriginalRemovedActivity))
                            {
                                compositeActivityExecutor.OnActivityRemoved(executionContext, removedActivityAction.OriginalRemovedActivity);
                                if (removedActivityAction.OriginalRemovedActivity.ExecutionResult != ActivityExecutionResult.Uninitialized)
                                {
                                    removedActivityAction.OriginalRemovedActivity.Uninitialize(executionContext.Activity.RootActivity.WorkflowCoreRuntime);
                                    removedActivityAction.OriginalRemovedActivity.SetValue(Activity.ExecutionResultProperty, ActivityExecutionResult.Uninitialized);
                                }
                                removedActivityAction.OriginalRemovedActivity.OnActivityExecutionContextUnload(executionContext.Activity.RootActivity.WorkflowCoreRuntime);
                                removedActivityAction.OriginalRemovedActivity.Dispose();
                            }
                        }
                    }
                }
            }
        }
        #endregion

        #region Static helpers

        private static bool CompareWorkflowDefinition(Activity originalWorkflowDefinition, Activity currentWorkflowDefinition)
        {
            if (originalWorkflowDefinition == currentWorkflowDefinition)
                return true;

            if (originalWorkflowDefinition.GetType() != currentWorkflowDefinition.GetType())
                return false;

            Guid originalChangeVersion = (Guid)originalWorkflowDefinition.GetValue(WorkflowChanges.WorkflowChangeVersionProperty);
            Guid currentChangeVersion = (Guid)currentWorkflowDefinition.GetValue(WorkflowChanges.WorkflowChangeVersionProperty);
            return (originalChangeVersion == currentChangeVersion);
        }

        private static List<WorkflowChangeAction> DiffTrees(CompositeActivity originalCompositeActivity, CompositeActivity clonedCompositeActivity)
        {
            List<WorkflowChangeAction> listChanges = new List<WorkflowChangeAction>();
            IEnumerator<Activity> clonedActivitiesEnum = clonedCompositeActivity.Activities.GetEnumerator();
            IEnumerator<Activity> originalActivitiesEnum = originalCompositeActivity.Activities.GetEnumerator();
            int currentRemoveIndex = 0;
            while (originalActivitiesEnum.MoveNext())
            {
                bool foundMatching = false;
                Activity originalActivity = originalActivitiesEnum.Current;
                while (clonedActivitiesEnum.MoveNext())
                {
                    Activity clonedActivity = clonedActivitiesEnum.Current;
                    if (clonedActivity.Readonly)
                    {
                        if (originalActivity.DottedPath == clonedActivity.CachedDottedPath)
                        {
                            currentRemoveIndex++;
                            foundMatching = true;
                            if (originalActivity is CompositeActivity)
                                listChanges.AddRange(DiffTrees(originalActivity as CompositeActivity, clonedActivity as CompositeActivity));
                            break;
                        }
                        else
                        {
                            listChanges.Add(new RemovedActivityAction(currentRemoveIndex, originalActivity, clonedCompositeActivity));
                            while (originalActivitiesEnum.MoveNext())
                            {
                                originalActivity = originalActivitiesEnum.Current;
                                if (originalActivity.DottedPath == clonedActivity.CachedDottedPath)
                                {
                                    currentRemoveIndex++;
                                    foundMatching = true;
                                    if (originalActivity is CompositeActivity)
                                        listChanges.AddRange(DiffTrees(originalActivity as CompositeActivity, clonedActivity as CompositeActivity));
                                    break;
                                }
                                else
                                {
                                    listChanges.Add(new RemovedActivityAction(currentRemoveIndex, originalActivity, clonedCompositeActivity));
                                }
                            }
                        }
                        break;
                    }
                    else
                    {
                        listChanges.Add(new AddedActivityAction(clonedCompositeActivity, clonedActivity));
                        currentRemoveIndex++;
                    }
                }
                if (!foundMatching)
                {
                    listChanges.Add(new RemovedActivityAction(currentRemoveIndex, originalActivity, clonedCompositeActivity));
                }
            }
            while (clonedActivitiesEnum.MoveNext())
                listChanges.Add(new AddedActivityAction(clonedCompositeActivity, clonedActivitiesEnum.Current));
            return listChanges;
        }

        private static Activity CloneRootActivity(Activity originalRootActivity)
        {
            // create new definition root
            string xomlText = originalRootActivity.GetValue(Activity.WorkflowXamlMarkupProperty) as string;
            string rulesText = null;
            Activity clonedRootActivity = null;
            IServiceProvider serviceProvider = originalRootActivity.GetValue(Activity.WorkflowRuntimeProperty) as IServiceProvider;
            Debug.Assert(serviceProvider != null);
            if (!string.IsNullOrEmpty(xomlText))
            {
                rulesText = originalRootActivity.GetValue(Activity.WorkflowRulesMarkupProperty) as string;
                clonedRootActivity = Activity.OnResolveActivityDefinition(null, xomlText, rulesText, true, false, serviceProvider);
            }
            else
                clonedRootActivity = Activity.OnResolveActivityDefinition(originalRootActivity.GetType(), null, null, true, false, serviceProvider);

            if (clonedRootActivity == null)
                throw new NullReferenceException(SR.GetString(SR.Error_InvalidRootForWorkflowChanges));

            // deserialize change history and apply it to new definition tree
            ArrayList workflowChanges = (ArrayList)((Activity)originalRootActivity).GetValue(WorkflowChanges.WorkflowChangeActionsProperty);
            if (workflowChanges != null)
            {
                workflowChanges = CloneWorkflowChangeActions(workflowChanges, originalRootActivity);
                if (workflowChanges != null)
                {
                    // apply changes to the shared schedule Defn to get the instance specific copy
                    foreach (WorkflowChangeAction action in workflowChanges)
                    {
                        bool result = action.ApplyTo((Activity)clonedRootActivity);
                        Debug.Assert(result, "ApplyTo Failed");
                    }
                    ((Activity)clonedRootActivity).SetValue(WorkflowChanges.WorkflowChangeActionsProperty, workflowChanges);
                }
            }
            return clonedRootActivity;
        }

        private static ArrayList CloneWorkflowChangeActions(ArrayList workflowChanges, Activity rootActivity)
        {
            if (workflowChanges == null)
                throw new ArgumentNullException("workflowChanges");

            if (rootActivity == null)
                throw new ArgumentNullException("rootActivity");

            string dynamicUpdateHistory = null;
            TypeProvider typeProvider = CreateTypeProvider(rootActivity);
            ServiceContainer serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(ITypeProvider), typeProvider);
            DesignerSerializationManager manager = new DesignerSerializationManager(serviceContainer);
            WorkflowMarkupSerializer xomlSerializer = new WorkflowMarkupSerializer();

            ArrayList clonedWorkflowChanges = null;
            // serialize dynamic updates
            using (manager.CreateSession())
            {
                using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
                {
                    using (XmlWriter xmlWriter = Helpers.CreateXmlWriter(sw))
                    {
                        WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                        xomlSerializer.Serialize(xomlSerializationManager, xmlWriter, workflowChanges);
                        dynamicUpdateHistory = sw.ToString();
                    }
                }

                // deserialize those
                using (StringReader sr = new StringReader(dynamicUpdateHistory))
                {
                    using (XmlReader xmlReader = XmlReader.Create(sr))
                    {
                        WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                        clonedWorkflowChanges = xomlSerializer.Deserialize(xomlSerializationManager, xmlReader) as ArrayList;
                    }
                }
            }
            return clonedWorkflowChanges;
        }

        internal static TypeProvider CreateTypeProvider(Activity rootActivity)
        {
            TypeProvider typeProvider = new TypeProvider(null);

            Type companionType = rootActivity.GetType();
            typeProvider.SetLocalAssembly(companionType.Assembly);
            typeProvider.AddAssembly(companionType.Assembly);

            foreach (AssemblyName assemblyName in companionType.Assembly.GetReferencedAssemblies())
            {
                Assembly referencedAssembly = null;
                try
                {
                    referencedAssembly = Assembly.Load(assemblyName);
                    if (referencedAssembly != null)
                        typeProvider.AddAssembly(referencedAssembly);
                }
                catch
                {
                }

                if (referencedAssembly == null && assemblyName.CodeBase != null)
                    typeProvider.AddAssemblyReference(assemblyName.CodeBase);
            }
            return typeProvider;
        }
        #endregion
    }

    #region WorkflowChangeAction classes

    [DesignerSerializer(typeof(WorkflowMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class WorkflowChangeAction
    {
        protected internal abstract bool ApplyTo(Activity rootActivity);
        protected internal abstract ValidationErrorCollection ValidateChanges(Activity activity);
    }

    [DesignerSerializer(typeof(ActivityChangeActionMarkupSerializer), typeof(WorkflowMarkupSerializer))]
    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public abstract class ActivityChangeAction : WorkflowChangeAction
    {
        private string ownerActivityDottedPath = string.Empty;

        protected ActivityChangeAction()
        {
        }

        protected ActivityChangeAction(CompositeActivity compositeActivity)
        {
            if (compositeActivity == null)
                throw new ArgumentNullException("compositeActivity");

            this.ownerActivityDottedPath = compositeActivity.DottedPath;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public string OwnerActivityDottedPath
        {
            get
            {
                return this.ownerActivityDottedPath;
            }
            internal set
            {
                this.ownerActivityDottedPath = value;
            }
        }

        protected internal override ValidationErrorCollection ValidateChanges(Activity contextActivity)
        {
            if (contextActivity == null)
                throw new ArgumentNullException("contextActivity");

            ValidationErrorCollection errors = new ValidationErrorCollection();

            CompositeActivity ownerActivity = contextActivity.TraverseDottedPathFromRoot(this.OwnerActivityDottedPath) as CompositeActivity;
            if (ownerActivity != null && WorkflowChanges.IsActivityExecutable(ownerActivity))
            {
                foreach (Validator validator in ComponentDispenser.CreateComponents(ownerActivity.GetType(), typeof(ActivityValidatorAttribute)))
                {
                    ValidationError error = validator.ValidateActivityChange(ownerActivity, this);
                    if (error != null)
                        errors.Add(error);
                }
            }

            return errors;
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class AddedActivityAction : ActivityChangeAction
    {
        private int index = 0;
        private Activity addedActivity = null;

        public AddedActivityAction()
        {
        }

        public AddedActivityAction(CompositeActivity compositeActivity, Activity activityAdded)
            : base(compositeActivity)
        {
            if (compositeActivity == null)
                throw new ArgumentNullException("compositeActivity");

            if (activityAdded == null)
                throw new ArgumentNullException("activityAdded");

            this.index = (compositeActivity.Activities != null) ? compositeActivity.Activities.IndexOf(activityAdded) : -1;
            this.addedActivity = activityAdded;
        }
        public int Index
        {
            get
            {
                return this.index;
            }
            internal set
            {
                this.index = value;
            }
        }

        public Activity AddedActivity
        {
            get
            {
                return this.addedActivity;
            }
            internal set
            {
                this.addedActivity = value;
            }
        }

        protected internal override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
                throw new ArgumentNullException("rootActivity");
            if (!(rootActivity is CompositeActivity))
                throw new ArgumentException(SR.GetString(SR.Error_RootActivityTypeInvalid), "rootActivity");

            CompositeActivity ownerActivity = rootActivity.TraverseDottedPathFromRoot(this.OwnerActivityDottedPath) as CompositeActivity;
            if (ownerActivity == null)
                return false;

            // !!!work around: 
            ownerActivity.DynamicUpdateMode = true;
            CompositeActivity addedActivityOwner = this.addedActivity.Parent;
            try
            {
                this.addedActivity.SetParent(ownerActivity);
                Activity clonedAddedActivity = this.addedActivity;
                if (!this.addedActivity.DesignMode)
                    clonedAddedActivity = this.addedActivity.Clone();
                // We need to serialize and deserialize in order to clone during design mode
                else
                {
                    TypeProvider typeProvider = WorkflowChanges.CreateTypeProvider(rootActivity);
                    ServiceContainer serviceContainer = new ServiceContainer();
                    serviceContainer.AddService(typeof(ITypeProvider), typeProvider);
                    DesignerSerializationManager manager = new DesignerSerializationManager(serviceContainer);
                    WorkflowMarkupSerializer xomlSerializer = new WorkflowMarkupSerializer();
                    string addedActivityText = string.Empty;
                    // serialize dynamic updates
                    using (manager.CreateSession())
                    {
                        using (StringWriter sw = new StringWriter(CultureInfo.InvariantCulture))
                        {
                            using (XmlWriter xmlWriter = Helpers.CreateXmlWriter(sw))
                            {
                                WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                                xomlSerializer.Serialize(xomlSerializationManager, xmlWriter, this.addedActivity);
                                addedActivityText = sw.ToString();
                            }
                        }

                        // deserialize those
                        using (StringReader sr = new StringReader(addedActivityText))
                        {
                            using (XmlReader xmlReader = XmlReader.Create(sr))
                            {
                                WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                                clonedAddedActivity = xomlSerializer.Deserialize(xomlSerializationManager, xmlReader) as Activity;
                            }
                        }
                    }
                    if (clonedAddedActivity == null)
                        throw new InvalidOperationException(SR.GetString(SR.Error_ApplyDynamicChangeFailed));
                }
                if (ownerActivity.WorkflowCoreRuntime != null)
                    ((IDependencyObjectAccessor)clonedAddedActivity).InitializeInstanceForRuntime(ownerActivity.WorkflowCoreRuntime);

                clonedAddedActivity.SetParent(null);
                ownerActivity.Activities.Insert(this.index, clonedAddedActivity);
            }
            finally
            {
                this.addedActivity.SetParent(addedActivityOwner);
                ownerActivity.DynamicUpdateMode = false;
            }
            return true;
        }
    }

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class RemovedActivityAction : ActivityChangeAction
    {
        private int removedActivityIndex = -1;
        private Activity originalRemovedActivity = null;

        public RemovedActivityAction()
        {
        }
        public RemovedActivityAction(int removedActivityIndex, Activity originalActivity, CompositeActivity clonedParentActivity)
            : base(clonedParentActivity)
        {
            if (originalActivity == null)
                throw new ArgumentNullException("originalActivity");
            if (clonedParentActivity == null)
                throw new ArgumentNullException("clonedParentActivity");

            this.originalRemovedActivity = originalActivity;
            this.removedActivityIndex = removedActivityIndex;
        }

        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        public int RemovedActivityIndex
        {
            get
            {
                return this.removedActivityIndex;
            }
            internal set
            {
                this.removedActivityIndex = value;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Activity OriginalRemovedActivity
        {
            get
            {
                return this.originalRemovedActivity;
            }
            internal set
            {
                this.originalRemovedActivity = value;
            }
        }
        protected internal override ValidationErrorCollection ValidateChanges(Activity contextActivity)
        {
            ValidationErrorCollection errors = base.ValidateChanges(contextActivity);
            Activity removedActivityInContext = contextActivity.TraverseDottedPathFromRoot(this.originalRemovedActivity.DottedPath);
            if (WorkflowChanges.IsActivityExecutable(removedActivityInContext) && removedActivityInContext.ExecutionStatus == ActivityExecutionStatus.Executing)
                errors.Add(new ValidationError(SR.GetString(SR.Error_RemoveExecutingActivity, this.originalRemovedActivity.QualifiedName), ErrorNumbers.Error_RemoveExecutingActivity));
            return errors;
        }
        protected internal override bool ApplyTo(Activity rootActivity)
        {
            if (rootActivity == null)
                throw new ArgumentNullException("rootActivity");
            if (!(rootActivity is CompositeActivity))
                throw new ArgumentException(SR.GetString(SR.Error_RootActivityTypeInvalid), "rootActivity");


            CompositeActivity ownerActivity = rootActivity.TraverseDottedPathFromRoot(this.OwnerActivityDottedPath) as CompositeActivity;
            if (ownerActivity == null)
                return false;

            if (this.removedActivityIndex >= ownerActivity.Activities.Count)
                return false;

            // !!!work around: 
            ownerActivity.DynamicUpdateMode = true;
            try
            {
                this.originalRemovedActivity = ownerActivity.Activities[this.removedActivityIndex];
                ownerActivity.Activities.RemoveAt(this.removedActivityIndex);
            }
            finally
            {
                ownerActivity.DynamicUpdateMode = false;
            }
            return true;
        }
    }
    #endregion

    #region Class ActivityChangeActionMarkupSerializer
    internal sealed class ActivityChangeActionMarkupSerializer : WorkflowMarkupSerializer
    {
        protected internal override PropertyInfo[] GetProperties(WorkflowMarkupSerializationManager serializationManager, object obj)
        {
            List<PropertyInfo> properties = new List<PropertyInfo>(base.GetProperties(serializationManager, obj));

            //Collect the internal properties, we do this so that activity change action apis don't need to expose unnecessary setters
            foreach (PropertyInfo property in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy))
            {
                DesignerSerializationVisibility visibility = Helpers.GetSerializationVisibility(property);
                if (visibility != DesignerSerializationVisibility.Hidden && property.GetSetMethod() == null && property.GetSetMethod(true) != null)
                    properties.Add(property);
            }

            return properties.ToArray();
        }
    }
    #endregion
}
