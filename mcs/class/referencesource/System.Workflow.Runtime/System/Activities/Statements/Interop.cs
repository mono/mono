//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Statements
{
    using System.Activities;
    using System.Activities.Expressions;
    using System.Activities.Persistence;
    using System.Activities.Tracking;
    using System.Activities.Validation;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using System.Transactions;
    using System.Xml.Linq;
    using System.Workflow.Runtime;
    using System.Workflow.ComponentModel.Compiler;
    using ValidationError = System.Activities.Validation.ValidationError;
    using System.Workflow.Runtime.Hosting;
    using System.Workflow.Activities;
    using System.Runtime.Serialization;

    [SuppressMessage("Microsoft.Naming", "CA1724:TypeNamesShouldNotMatchNamespaces",
        Justification = "The type name 'Interop' conflicts in whole or in part with the namespace name 'System.Web.Services.Interop' - not common usage")]
    [Obsolete("The WF3 Types are deprecated. Instead, please use the new WF4 Types from System.Activities.*")] 
    public sealed class Interop : NativeActivity, ICustomTypeDescriptor
    {
        static Func<TimerExtension> getDefaultTimerExtension = new Func<TimerExtension>(GetDefaultTimerExtension);
        static Func<InteropPersistenceParticipant> getInteropPersistenceParticipant = new Func<InteropPersistenceParticipant>(GetInteropPersistenceParticipant);
        Dictionary<string, Argument> properties;
        Dictionary<string, object> metaProperties;
        System.Workflow.ComponentModel.Activity v1Activity;
        IList<PropertyInfo> outputPropertyDefinitions;
        HashSet<string> extraDynamicArguments;
        bool exposedBodyPropertiesCacheIsValid;
        IList<InteropProperty> exposedBodyProperties;
        Variable<InteropExecutor> interopActivityExecutor;
        Variable<RuntimeTransactionHandle> runtimeTransactionHandle;
        BookmarkCallback onResumeBookmark;
        CompletionCallback onPersistComplete;
        BookmarkCallback onTransactionComplete;
        Type activityType;
        Persist persistActivity;

        internal const string InArgumentSuffix = "In";
        internal const string OutArgumentSuffix = "Out";
        Variable<bool> persistOnClose;
        Variable<InteropEnlistment> interopEnlistment;
        Variable<Exception> outstandingException;

        object thisLock;
        // true if the body type is a valid activity. used so we can have delayed validation support in the designer
        bool hasValidBody;
        // true if the V3 activity property names will conflict with our generated argument names
        bool hasNameCollision;

        public Interop()
            : base()
        {
            this.interopActivityExecutor = new Variable<InteropExecutor>();
            this.runtimeTransactionHandle = new Variable<RuntimeTransactionHandle>();
            this.persistOnClose = new Variable<bool>();
            this.interopEnlistment = new Variable<InteropEnlistment>();
            this.outstandingException = new Variable<Exception>();
            this.onResumeBookmark = new BookmarkCallback(this.OnResumeBookmark);
            this.persistActivity = new Persist();
            this.thisLock = new object();
            base.Constraints.Add(ProcessAdvancedConstraints());
        }

        [DefaultValue(null)]
        public Type ActivityType
        {
            get
            {
                return this.activityType;
            }
            set
            {
                if (value != this.activityType)
                {
                    this.hasValidBody = false;
                    if (value != null)
                    {
                        if (typeof(System.Workflow.ComponentModel.Activity).IsAssignableFrom(value)
                            && value.GetConstructor(Type.EmptyTypes) != null)
                        {
                            this.hasValidBody = true;
                        }
                    }

                    this.activityType = value;

                    if (this.metaProperties != null)
                    {
                        this.metaProperties.Clear();
                    }
                    if (this.outputPropertyDefinitions != null)
                    {
                        this.outputPropertyDefinitions.Clear();
                    }
                    if (this.properties != null)
                    {
                        this.properties.Clear();
                    }
                    if (this.exposedBodyProperties != null)
                    {
                        for (int i = 0; i < this.exposedBodyProperties.Count; i++)
                        {
                            this.exposedBodyProperties[i].Invalidate();
                        }
                        this.exposedBodyProperties.Clear();
                    }
                    this.exposedBodyPropertiesCacheIsValid = false;

                    this.v1Activity = null;
                }
            }
        }

        [Browsable(false)]
        public IDictionary<string, Argument> ActivityProperties
        {
            get
            {
                if (this.properties == null)
                {
                    this.properties = new Dictionary<string, Argument>();
                }

                return this.properties;
            }
        }

        [Browsable(false)]
        public IDictionary<string, object> ActivityMetaProperties
        {
            get
            {
                if (this.metaProperties == null)
                {
                    this.metaProperties = new Dictionary<string, object>();
                }
                return this.metaProperties;
            }
        }

        protected override bool CanInduceIdle
        {
            get
            {
                return true;
            }
        }

        internal System.Workflow.ComponentModel.Activity ComponentModelActivity
        {
            get
            {
                if (this.v1Activity == null && this.ActivityType != null)
                {
                    Debug.Assert(this.hasValidBody, "should only be called when we have a valid body");
                    this.v1Activity = CreateActivity();
                }
                return this.v1Activity;
            }
        }

        internal IList<PropertyInfo> OutputPropertyDefinitions
        {
            get
            {
                return this.outputPropertyDefinitions;
            }
        }

        internal bool HasNameCollision
        {
            get
            {
                return this.hasNameCollision;
            }
        }

        protected override void CacheMetadata(NativeActivityMetadata metadata)
        {
            if (this.extraDynamicArguments != null)
            {
                this.extraDynamicArguments.Clear();
            }

            this.v1Activity = null;

            if (this.hasValidBody)
            {
                //Cache the output properties prop info for look up.
                this.outputPropertyDefinitions = new List<PropertyInfo>();

                //Cache the extra property definitions for look up in OnOpen
                if (this.properties != null)
                {
                    if (this.extraDynamicArguments == null)
                    {
                        this.extraDynamicArguments = new HashSet<string>();
                    }

                    foreach (string name in properties.Keys)
                    {
                        this.extraDynamicArguments.Add(name);
                    }
                }

                //Create matched pair of RuntimeArguments for every property: Property (InArgument) & PropertyOut (Argument)
                PropertyInfo[] bodyProperties = this.ActivityType.GetProperties();
                // recheck for name collisions
                this.hasNameCollision = InteropEnvironment.ParameterHelper.HasPropertyNameCollision(bodyProperties);
                foreach (PropertyInfo propertyInfo in bodyProperties)
                {
                    if (InteropEnvironment.ParameterHelper.IsBindable(propertyInfo))
                    {
                        string propertyInName;
                        //If there are any Property/PropertyOut name pairs already extant, we fall back to renaming the InArgument half of the pair as well
                        if (this.hasNameCollision)
                        {
                            propertyInName = propertyInfo.Name + Interop.InArgumentSuffix;
                        }
                        else
                        {
                            propertyInName = propertyInfo.Name;
                        }
                        //We always rename the OutArgument half of the pair
                        string propertyOutName = propertyInfo.Name + Interop.OutArgumentSuffix;

                        RuntimeArgument inArgument = new RuntimeArgument(propertyInName, propertyInfo.PropertyType, ArgumentDirection.In);
                        RuntimeArgument outArgument = new RuntimeArgument(propertyOutName, propertyInfo.PropertyType, ArgumentDirection.Out);

                        if (this.properties != null)
                        {
                            Argument inBinding = null;
                            if (this.properties.TryGetValue(propertyInName, out inBinding))
                            {
                                if (inBinding.Direction != ArgumentDirection.In)
                                {
                                    throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropArgumentDirectionMismatch, propertyInName, propertyOutName));
                                }

                                this.extraDynamicArguments.Remove(propertyInName);
                                metadata.Bind(inBinding, inArgument);
                            }

                            Argument outBinding = null;
                            if (this.properties.TryGetValue(propertyOutName, out outBinding))
                            {
                                this.extraDynamicArguments.Remove(propertyOutName);
                                metadata.Bind(outBinding, outArgument);
                            }
                        }
                        metadata.AddArgument(inArgument);
                        metadata.AddArgument(outArgument);

                        this.outputPropertyDefinitions.Add(propertyInfo);
                    }
                }
            }

            metadata.SetImplementationVariablesCollection(
                new Collection<Variable>
                {
                    this.interopActivityExecutor,
                    this.runtimeTransactionHandle,
                    this.persistOnClose,
                    this.interopEnlistment,
                    this.outstandingException
                });

            metadata.AddImplementationChild(this.persistActivity);

            if (!this.hasValidBody)
            {
                if (this.ActivityType == null)
                {
                    metadata.AddValidationError(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropBodyNotSet, this.DisplayName));
                }
                else
                {
                    // Body needs to be a WF 3.0 activity
                    if (!typeof(System.Workflow.ComponentModel.Activity).IsAssignableFrom(this.ActivityType))
                    {
                        metadata.AddValidationError(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropWrongBody, this.DisplayName));
                    }

                    // and have a default ctor
                    if (this.ActivityType.GetConstructor(Type.EmptyTypes) == null)
                    {
                        metadata.AddValidationError(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropBodyMustHavePublicDefaultConstructor, this.DisplayName));
                    }
                }
            }
            else
            {
                if (this.extraDynamicArguments != null && this.extraDynamicArguments.Count > 0)
                {
                    metadata.AddValidationError(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.AttemptToBindUnknownProperties, this.DisplayName, this.extraDynamicArguments.First()));
                }
                else
                {
                    try
                    {
                        InitializeMetaProperties(this.ComponentModelActivity);
                        // We call InitializeDefinitionForRuntime in the first call to execute to 
                        // make sure it only happens once.
                    }
                    catch (InvalidOperationException e)
                    {
                        metadata.AddValidationError(e.Message);
                    }
                }
            }

            metadata.AddDefaultExtensionProvider(getDefaultTimerExtension);
            metadata.AddDefaultExtensionProvider(getInteropPersistenceParticipant);
        }

        static TimerExtension GetDefaultTimerExtension()
        {
            return new DurableTimerExtension();
        }

        static InteropPersistenceParticipant GetInteropPersistenceParticipant()
        {
            return new InteropPersistenceParticipant();
        }

        protected override void Execute(NativeActivityContext context)
        {
            // 


            WorkflowRuntimeService workflowRuntimeService = context.GetExtension<WorkflowRuntimeService>();
            if (workflowRuntimeService != null && !(workflowRuntimeService is ExternalDataExchangeService))
            {
                throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropWorkflowRuntimeServiceNotSupported));
            }

            lock (this.thisLock)
            {
                ((System.Workflow.ComponentModel.IDependencyObjectAccessor)this.ComponentModelActivity).InitializeDefinitionForRuntime(null);
            }
            if (!this.ComponentModelActivity.Enabled)
            {
                return;
            }

            System.Workflow.ComponentModel.Activity activityInstance = CreateActivity();
            InitializeMetaProperties(activityInstance);
            activityInstance.SetValue(WorkflowExecutor.WorkflowInstanceIdProperty, context.WorkflowInstanceId);

            InteropExecutor interopExecutor = new InteropExecutor(context.WorkflowInstanceId, activityInstance, this.OutputPropertyDefinitions, this.ComponentModelActivity);

            if (!interopExecutor.HasCheckedForTrackingParticipant)
            {
                interopExecutor.TrackingEnabled = (context.GetExtension<TrackingParticipant>() != null);
                interopExecutor.HasCheckedForTrackingParticipant = true;
            }

            this.interopActivityExecutor.Set(context, interopExecutor);

            //Register the Handle as an execution property so that we can call GetCurrentTransaction or
            //RequestTransactionContext on it later
            RuntimeTransactionHandle runtimeTransactionHandle = this.runtimeTransactionHandle.Get(context);
            context.Properties.Add(runtimeTransactionHandle.ExecutionPropertyName, runtimeTransactionHandle);

            try
            {
                using (new ServiceEnvironment(activityInstance))
                {
                    using (InteropEnvironment interopEnvironment = new InteropEnvironment(
                        interopExecutor, context,
                        this.onResumeBookmark,
                        this,
                        runtimeTransactionHandle.GetCurrentTransaction(context)))
                    {
                        interopEnvironment.Execute(this.ComponentModelActivity, context);
                    }
                }
            }
            catch (Exception exception)
            {
                if (WorkflowExecutor.IsIrrecoverableException(exception) || !this.persistOnClose.Get(context))
                {
                    throw;
                }

                // We are not ----ing the exception.  The exception is saved in this.outstandingException.  
                // We will throw the exception from OnPersistComplete.
            }
        }

        protected override void Cancel(NativeActivityContext context)
        {
            InteropExecutor interopExecutor = this.interopActivityExecutor.Get(context);

            if (!interopExecutor.HasCheckedForTrackingParticipant)
            {
                interopExecutor.TrackingEnabled = (context.GetExtension<TrackingParticipant>() != null);
                interopExecutor.HasCheckedForTrackingParticipant = true;
            }

            interopExecutor.EnsureReload(this);

            try
            {
                using (InteropEnvironment interopEnvironment = new InteropEnvironment(
                    interopExecutor, context,
                    this.onResumeBookmark,
                    this,
                    this.runtimeTransactionHandle.Get(context).GetCurrentTransaction(context)))
                {
                    interopEnvironment.Cancel();
                }
            }
            catch (Exception exception)
            {
                if (WorkflowExecutor.IsIrrecoverableException(exception) || !this.persistOnClose.Get(context))
                {
                    throw;
                }

                // We are not ----ing the exception.  The exception is saved in this.outstandingException.  
                // We will throw the exception from OnPersistComplete.
            }
        }

        internal void SetOutputArgumentValues(IDictionary<string, object> outputs, NativeActivityContext context)
        {
            if ((this.properties != null) && (outputs != null))
            {
                foreach (KeyValuePair<string, object> output in outputs)
                {
                    Argument argument;
                    if (this.properties.TryGetValue(output.Key, out argument) && argument != null)
                    {
                        if (argument.Direction == ArgumentDirection.Out)
                        {
                            argument.Set(context, output.Value);
                        }
                    }
                }
            }
        }

        internal IDictionary<string, object> GetInputArgumentValues(NativeActivityContext context)
        {
            Dictionary<string, object> arguments = null;

            if (this.properties != null)
            {
                foreach (KeyValuePair<string, Argument> parameter in this.properties)
                {
                    Argument argument = parameter.Value;

                    if (argument.Direction == ArgumentDirection.In)
                    {
                        if (arguments == null)
                        {
                            arguments = new Dictionary<string, object>();
                        }

                        arguments.Add(parameter.Key, argument.Get<object>(context));
                    }
                }
            }

            return arguments;
        }

        System.Workflow.ComponentModel.Activity CreateActivity()
        {
            Debug.Assert(this.ActivityType != null, "ActivityType must be set by the time we get here");

            System.Workflow.ComponentModel.Activity activity = Activator.CreateInstance(this.ActivityType) as System.Workflow.ComponentModel.Activity;
            Debug.Assert(activity != null, "We should have validated that the type has a default ctor() and derives from System.Workflow.ComponentModel.Activity.");

            return activity;
        }

        void InitializeMetaProperties(System.Workflow.ComponentModel.Activity activity)
        {
            Debug.Assert((activity.GetType() == this.ActivityType), "activity must be the same type as this.ActivityType");
            if (this.metaProperties != null && this.metaProperties.Count > 0)
            {
                foreach (string name in this.metaProperties.Keys)
                {
                    PropertyInfo property = this.ActivityType.GetProperty(name);
                    if (property == null)
                    {
                        throw new InvalidOperationException(string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.MetaPropertyDoesNotExist, name, this.ActivityType.FullName));
                    }
                    property.SetValue(activity, this.metaProperties[name], null);
                }
            }
        }

        void OnResumeBookmark(NativeActivityContext context, Bookmark bookmark, object state)
        {
            InteropExecutor interopExecutor = this.interopActivityExecutor.Get(context);

            if (!interopExecutor.HasCheckedForTrackingParticipant)
            {
                interopExecutor.TrackingEnabled = (context.GetExtension<TrackingParticipant>() != null);
                interopExecutor.HasCheckedForTrackingParticipant = true;
            }

            interopExecutor.EnsureReload(this);

            try
            {
                using (InteropEnvironment interopEnvironment = new InteropEnvironment(
                    interopExecutor, context,
                    this.onResumeBookmark,
                    this,
                    this.runtimeTransactionHandle.Get(context).GetCurrentTransaction(context)))
                {
                    IComparable queueName = interopExecutor.BookmarkQueueMap[bookmark];
                    interopEnvironment.EnqueueEvent(queueName, state);
                }
            }
            catch (Exception exception)
            {
                if (WorkflowExecutor.IsIrrecoverableException(exception) || !this.persistOnClose.Get(context))
                {
                    throw;
                }

                // We are not ----ing the exception.  The exception is saved in this.outstandingException.  
                // We will throw the exception from OnPersistComplete.
            }
        }

        AttributeCollection ICustomTypeDescriptor.GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName()
        {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter()
        {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents()
        {
            return TypeDescriptor.GetEvents(this, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes)
        {
            List<PropertyDescriptor> properties = new List<PropertyDescriptor>();

            PropertyDescriptorCollection interopProperties;
            if (attributes != null)
            {
                interopProperties = TypeDescriptor.GetProperties(this, attributes, true);
            }
            else
            {
                interopProperties = TypeDescriptor.GetProperties(this, true);
            }
            for (int i = 0; i < interopProperties.Count; i++)
            {
                properties.Add(interopProperties[i]);
            }

            if (this.hasValidBody)
            {
                // First, cache the full set of body properties
                if (!this.exposedBodyPropertiesCacheIsValid)
                {
                    //Create matched pair of RuntimeArguments for every property: Property (InArgument) & PropertyOut (Argument)
                    PropertyInfo[] bodyProperties = this.ActivityType.GetProperties();
                    // recheck for name collisions
                    this.hasNameCollision = InteropEnvironment.ParameterHelper.HasPropertyNameCollision(bodyProperties);
                    for (int i = 0; i < bodyProperties.Length; i++)
                    {
                        PropertyInfo property = bodyProperties[i];
                        bool isMetaProperty;
                        if (InteropEnvironment.ParameterHelper.IsBindableOrMetaProperty(property, out isMetaProperty))
                        {
                            // Propagate the attributes to the PropertyDescriptor, appending a DesignerSerializationVisibility attribute
                            Attribute[] customAttributes = Attribute.GetCustomAttributes(property, true);
                            Attribute[] newAttributes = new Attribute[customAttributes.Length + 1];
                            customAttributes.CopyTo(newAttributes, 0);
                            newAttributes[customAttributes.Length] = new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden);

                            if (this.exposedBodyProperties == null)
                            {
                                this.exposedBodyProperties = new List<InteropProperty>(bodyProperties.Length);
                            }
                            if (isMetaProperty)
                            {
                                InteropProperty descriptor = new LiteralProperty(this, property.Name, property.PropertyType, newAttributes);
                                this.exposedBodyProperties.Add(descriptor);
                            }
                            else
                            {
                                InteropProperty inDescriptor;
                                //If there are any Property/PropertyOut name pairs already extant, we fall back to renaming the InArgument half of the pair as well
                                if (this.hasNameCollision)
                                {
                                    inDescriptor = new ArgumentProperty(this, property.Name + InArgumentSuffix, Argument.Create(property.PropertyType, ArgumentDirection.In), newAttributes);
                                }
                                else
                                {
                                    inDescriptor = new ArgumentProperty(this, property.Name, Argument.Create(property.PropertyType, ArgumentDirection.In), newAttributes);
                                }
                                this.exposedBodyProperties.Add(inDescriptor);
                                //We always rename the OutArgument half of the pair
                                InteropProperty outDescriptor = new ArgumentProperty(this, property.Name + OutArgumentSuffix, Argument.Create(property.PropertyType, ArgumentDirection.Out), newAttributes);
                                this.exposedBodyProperties.Add(outDescriptor);
                            }
                        }
                    }
                    this.exposedBodyPropertiesCacheIsValid = true;
                }
                // Now adds body properties, complying with the filter:
                if (this.exposedBodyProperties != null)
                {
                    for (int i = 0; i < this.exposedBodyProperties.Count; i++)
                    {
                        PropertyDescriptor descriptor = this.exposedBodyProperties[i];
                        if (attributes == null || !ShouldFilterProperty(descriptor, attributes))
                        {
                            properties.Add(descriptor);
                        }
                    }
                }
            }

            return new PropertyDescriptorCollection(properties.ToArray());
        }

        static bool ShouldFilterProperty(PropertyDescriptor property, Attribute[] attributes)
        {
            if (attributes == null || attributes.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < attributes.Length; i++)
            {
                Attribute filterAttribute = attributes[i];
                Attribute propertyAttribute = property.Attributes[filterAttribute.GetType()];
                if (propertyAttribute == null)
                {
                    if (!filterAttribute.IsDefaultAttribute())
                    {
                        return true;
                    }
                }
                else
                {
                    if (!filterAttribute.Match(propertyAttribute))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties()
        {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd)
        {
            InteropProperty intProp = pd as InteropProperty;
            if (intProp != null)
            {
                return intProp.Owner;
            }
            else
            {
                return this;
            }
        }

        internal void OnClose(NativeActivityContext context, Exception exception)
        {
            if (this.persistOnClose.Get(context))
            {
                if (exception == null)
                {
                    context.ScheduleActivity(this.persistActivity);
                }
                else
                {
                    // The V1 workflow faulted and there is an uncaught exception. We cannot throw
                    // the exception right away because we must Persist in order to process the WorkBatch.
                    // So we are saving the uncaught exception and scheduling the Persist activity with a completion callback.
                    // We will throw the exception from OnPersistComplete.
                    this.outstandingException.Set(context, exception);

                    if (this.onPersistComplete == null)
                    {
                        this.onPersistComplete = new CompletionCallback(this.OnPersistComplete);
                    }

                    context.ScheduleActivity(this.persistActivity, this.onPersistComplete);
                }
            }

            this.interopEnlistment.Set(context, null);
        }

        internal void Persist(NativeActivityContext context)
        {
            if (this.onPersistComplete == null)
            {
                this.onPersistComplete = new CompletionCallback(this.OnPersistComplete);
            }

            // If Persist fails for any reason, the workflow aborts
            context.ScheduleActivity(this.persistActivity, this.onPersistComplete);
        }

        internal void OnPersistComplete(NativeActivityContext context, ActivityInstance completedInstance)
        {
            this.persistOnClose.Set(context, false);
            Exception exception = this.outstandingException.Get(context);
            if (exception != null)
            {
                this.outstandingException.Set(context, null);
                throw exception;
            }

            this.Resume(context, null);
        }

        internal void CreateTransaction(NativeActivityContext context, TransactionOptions txOptions)
        {
            RuntimeTransactionHandle transactionHandle = this.runtimeTransactionHandle.Get(context);
            Debug.Assert(transactionHandle != null, "RuntimeTransactionHandle is null");

            transactionHandle.RequestTransactionContext(context, OnTransactionContextAcquired, txOptions);
        }

        void OnTransactionContextAcquired(NativeActivityTransactionContext context, object state)
        {
            Debug.Assert(context != null, "ActivityTransactionContext was null");
            TransactionOptions txOptions = (TransactionOptions)state;
            CommittableTransaction transaction = new CommittableTransaction(txOptions);
            context.SetRuntimeTransaction(transaction);
            this.Resume(context, transaction);
        }

        internal void CommitTransaction(NativeActivityContext context)
        {
            if (this.onTransactionComplete == null)
            {
                this.onTransactionComplete = new BookmarkCallback(this.OnTransactionComplete);
            }

            RuntimeTransactionHandle transactionHandle = this.runtimeTransactionHandle.Get(context);
            transactionHandle.CompleteTransaction(context, this.onTransactionComplete);
        }

        void OnTransactionComplete(NativeActivityContext context, Bookmark bookmark, object state)
        {
            this.Resume(context, null);
        }

        void Resume(NativeActivityContext context, Transaction transaction)
        {
            InteropExecutor interopExecutor = this.interopActivityExecutor.Get(context);

            if (!interopExecutor.HasCheckedForTrackingParticipant)
            {
                interopExecutor.TrackingEnabled = (context.GetExtension<TrackingParticipant>() != null);
                interopExecutor.HasCheckedForTrackingParticipant = true;
            }

            interopExecutor.EnsureReload(this);

            try
            {
                using (InteropEnvironment interopEnvironment = new InteropEnvironment(
                    interopExecutor, context,
                    this.onResumeBookmark,
                    this,
                    transaction))
                {
                    interopEnvironment.Resume();
                }
            }
            catch (Exception exception)
            {
                if (WorkflowExecutor.IsIrrecoverableException(exception) || !this.persistOnClose.Get(context))
                {
                    throw;
                }

                // We are not ----ing the exception.  The exception is saved in this.outstandingException.  
                // We will throw the exception from OnPersistComplete.
            }
        }

        internal void AddResourceManager(NativeActivityContext context, VolatileResourceManager resourceManager)
        {
            if (Transaction.Current != null &&
                Transaction.Current.TransactionInformation.Status == TransactionStatus.Active)
            {
                InteropEnlistment enlistment = this.interopEnlistment.Get(context);
                if (enlistment == null || !enlistment.IsValid)
                {
                    enlistment = new InteropEnlistment(Transaction.Current, resourceManager);
                    Transaction.Current.EnlistVolatile(enlistment, EnlistmentOptions.EnlistDuringPrepareRequired);
                    this.interopEnlistment.Set(context, enlistment);
                }
            }
            else
            {
                InteropPersistenceParticipant persistenceParticipant = context.GetExtension<InteropPersistenceParticipant>();
                persistenceParticipant.Add(this.Id, resourceManager);
                this.persistOnClose.Set(context, true);
            }
        }

        Constraint ProcessAdvancedConstraints()
        {
            DelegateInArgument<Interop> element = new DelegateInArgument<Interop>() { Name = "element" };
            DelegateInArgument<ValidationContext> validationContext = new DelegateInArgument<ValidationContext>() { Name = "validationContext" };
            DelegateInArgument<Activity> parent = new DelegateInArgument<Activity>() { Name = "parent" };

            //This will accumulate all potential violations at the root level. See the use case DIRECT of the Interop spec
            Variable<HashSet<InteropValidationEnum>> rootValidationDataVar = new Variable<HashSet<InteropValidationEnum>>(context => new HashSet<InteropValidationEnum>());

            //This will accumulate all violations at the nested level. See the use case NESTED of the Interop spec
            Variable<HashSet<InteropValidationEnum>> nestedChildrenValidationDataVar = new Variable<HashSet<InteropValidationEnum>>(context => new HashSet<InteropValidationEnum>());

            return new Constraint<Interop>
            {
                Body = new ActivityAction<Interop, ValidationContext>
                {
                    Argument1 = element,
                    Argument2 = validationContext,
                    Handler = new If
                    {
                        Condition = new InArgument<bool>(env => element.Get(env).hasValidBody),
                        Then = new Sequence
                        {
                            Variables = { rootValidationDataVar, nestedChildrenValidationDataVar },
                            Activities = 
                             {
                                //First traverse the interop body and collect all available data for validation. This is done at all levels, DIRECT and NESTED
                                new WalkInteropBodyAndGatherData()
                                {
                                     RootLevelValidationData = new InArgument<HashSet<InteropValidationEnum>>(rootValidationDataVar),
                                     NestedChildrenValidationData = new InArgument<HashSet<InteropValidationEnum>>(nestedChildrenValidationDataVar),
                                     InteropActivity = element
                                },
                                //This is based off the table in the Interop spec.
                                new ValidateAtRootAndNestedLevels()
                                {
                                     RootLevelValidationData = rootValidationDataVar,
                                     NestedChildrenValidationData = nestedChildrenValidationDataVar,
                                     Interop = element,
                                },
                                //Traverse the parent chain of the Interop activity to look for specifc violations regarding composition of 3.0 activities within 4.0 activities.
                                //Specifically, 
                                //  - 3.0 TransactionScope within a 4.0 TransactionScope
                                //  - 3.0 PersistOnClose within a 4.0 TransactionScope
                                //
                                new ForEach<Activity>
                                {
                                    Values = new GetParentChain
                                    {
                                        ValidationContext = validationContext,
                                    },
                                    Body = new ActivityAction<Activity>
                                    {
                                        Argument = parent,
                                        Handler = new Sequence                                   
                                        {
                                            Activities = 
                                            {
                                                new If()
                                                {
                                                    Condition = new Or<bool, bool, bool>
                                                    { 
                                                        Left = new Equal<Type, Type, bool>
                                                        {
                                                            Left = new ObtainType
                                                            {
                                                                Input = parent,
                                                            },
                                                            Right = new InArgument<Type>(context => typeof(System.Activities.Statements.TransactionScope))
                                                        },
                                                        Right = new Equal<string, string, bool>
                                                        {
                                                            Left = new InArgument<string>(env => parent.Get(env).GetType().FullName),
                                                            Right = "System.ServiceModel.Activities.TransactedReceiveScope"
                                                        }
                                                    },
                                                    Then = new Sequence
                                                    {
                                                        Activities = 
                                                        {
                                                            new AssertValidation
                                                            {
                                                                //Here we only pass the NestedChildrenValidationData since root level use 
                                                                //of TransactionScope would have already been flagged as an error
                                                                Assertion = new CheckForTransactionScope()
                                                                {
                                                                     ValidationResults = nestedChildrenValidationDataVar
                                                                },
                                                                Message = new InArgument<string>(ExecutionStringManager.InteropBodyNestedTransactionScope)
                                                            },
                                                            new AssertValidation
                                                            {
                                                                Assertion = new CheckForPersistOnClose()
                                                                {
                                                                     NestedChildrenValidationData = nestedChildrenValidationDataVar,
                                                                     RootLevelValidationData = rootValidationDataVar
                                                                },
                                                                Message = new InArgument<string>(ExecutionStringManager.InteropBodyNestedPersistOnCloseWithinTransactionScope)
                                                            },

                                                        }
                                                    },
                                                },
                                            }
                                        }                                   
                                    }
                                },
                                new ActivityTreeValidation()
                                {
                                    Interop = element
                                }
                            }
                        }
                    }
                }
            };
        }

        class ActivityTreeValidation : NativeActivity
        {
            public ActivityTreeValidation()
            {
            }

            public InArgument<Interop> Interop
            {
                get;
                set;
            }

            protected override void Execute(NativeActivityContext context)
            {
                Interop interop = this.Interop.Get(context);

                if (interop == null)
                {
                    return;
                }

                if (!typeof(System.Workflow.ComponentModel.Activity).IsAssignableFrom(interop.ActivityType))
                {
                    return;
                }

                System.ComponentModel.Design.ServiceContainer container = new System.ComponentModel.Design.ServiceContainer();
                container.AddService(typeof(ITypeProvider), CreateTypeProvider(interop.ActivityType));
                ValidationManager manager = new ValidationManager(container);

                System.Workflow.ComponentModel.Activity interopBody = interop.ComponentModelActivity;
                using (WorkflowCompilationContext.CreateScope(manager))
                {
                    foreach (Validator validator in manager.GetValidators(interop.ActivityType))
                    {
                        ValidationErrorCollection errors = validator.Validate(manager, interopBody);
                        foreach (System.Workflow.ComponentModel.Compiler.ValidationError error in errors)
                        {
                            Constraint.AddValidationError(context, new ValidationError(error.ErrorText, error.IsWarning, error.PropertyName));
                        }
                    }
                }
            }

            static TypeProvider CreateTypeProvider(Type rootType)
            {
                TypeProvider typeProvider = new TypeProvider(null);

                typeProvider.SetLocalAssembly(rootType.Assembly);
                typeProvider.AddAssembly(rootType.Assembly);

                foreach (AssemblyName assemblyName in rootType.Assembly.GetReferencedAssemblies())
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
        }

        class CheckForTransactionScope : CodeActivity<bool>
        {
            public InArgument<HashSet<InteropValidationEnum>> ValidationResults
            {
                get;
                set;
            }

            protected override bool Execute(CodeActivityContext context)
            {
                HashSet<InteropValidationEnum> validationResults = this.ValidationResults.Get(context);
                if (validationResults.Contains(InteropValidationEnum.TransactionScope))
                {
                    return false;
                }

                return true;
            }
        }

        class CheckForPersistOnClose : CodeActivity<bool>
        {
            public InArgument<HashSet<InteropValidationEnum>> NestedChildrenValidationData
            {
                get;
                set;
            }

            public InArgument<HashSet<InteropValidationEnum>> RootLevelValidationData
            {
                get;
                set;
            }

            protected override bool Execute(CodeActivityContext context)
            {
                HashSet<InteropValidationEnum> nestedValidationData = this.NestedChildrenValidationData.Get(context);
                HashSet<InteropValidationEnum> rootValidationData = this.RootLevelValidationData.Get(context);

                if (nestedValidationData.Contains(InteropValidationEnum.PersistOnClose) || rootValidationData.Contains(InteropValidationEnum.PersistOnClose))
                {
                    return false;
                }

                return true;
            }
        }

        class ValidateAtRootAndNestedLevels : NativeActivity
        {
            public ValidateAtRootAndNestedLevels()
            {
            }

            public InArgument<Interop> Interop
            {
                get;
                set;
            }

            public InArgument<HashSet<InteropValidationEnum>> RootLevelValidationData
            {
                get;
                set;
            }

            public InArgument<HashSet<InteropValidationEnum>> NestedChildrenValidationData
            {
                get;
                set;
            }

            protected override void Execute(NativeActivityContext context)
            {
                Interop activity = this.Interop.Get(context);

                foreach (InteropValidationEnum validationEnum in this.RootLevelValidationData.Get(context))
                {
                    //We care to mark PersistOnClose during the walking algorithm because we need to check if it happens under a 4.0 TransactionScopActivity and flag that
                    //That is done later, so skip here. 
                    if (validationEnum != InteropValidationEnum.PersistOnClose)
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropBodyRootLevelViolation, activity.DisplayName, validationEnum.ToString() + "Activity");
                        Constraint.AddValidationError(context, new ValidationError(message));
                    }
                }

                foreach (InteropValidationEnum validationEnum in this.NestedChildrenValidationData.Get(context))
                {
                    //We care to mark PersistOnClose or TransactionScope during the walking algorithm because we need to check if it happens under a 4.0 TransactionScopActivity and flag that
                    //That is done later, so skip here. 
                    if ((validationEnum != InteropValidationEnum.PersistOnClose) && (validationEnum != InteropValidationEnum.TransactionScope))
                    {
                        string message = string.Format(CultureInfo.CurrentCulture, ExecutionStringManager.InteropBodyNestedViolation, activity.DisplayName, validationEnum.ToString() + "Activity");
                        Constraint.AddValidationError(context, new ValidationError(message));
                    }
                }
            }
        }

        class WalkInteropBodyAndGatherData : System.Activities.CodeActivity
        {
            public InArgument<Interop> InteropActivity
            {
                get;
                set;
            }

            public InArgument<HashSet<InteropValidationEnum>> RootLevelValidationData
            {
                get;
                set;
            }

            public InArgument<HashSet<InteropValidationEnum>> NestedChildrenValidationData
            {
                get;
                set;
            }

            protected override void Execute(CodeActivityContext context)
            {
                Interop interop = this.InteropActivity.Get(context);
                Debug.Assert(interop != null, "Interop activity is null");

                Debug.Assert(interop.hasValidBody, "Interop activity has an invalid body");

                System.Workflow.ComponentModel.Activity interopBody = interop.ComponentModelActivity;
                Debug.Assert(interopBody != null, "Interop Body was null");

                HashSet<InteropValidationEnum> validationResults;
                validationResults = this.RootLevelValidationData.Get(context);
                Debug.Assert(validationResults != null, "The RootLevelValidationData hash set was null");

                //Gather data at root level first
                ProcessAtRootLevel(interopBody, validationResults);

                validationResults = null;
                validationResults = this.NestedChildrenValidationData.Get(context);
                Debug.Assert(validationResults != null, "The NestedChildrenValidationData hash set was null");

                //Next, process nested children of the Body
                if (interopBody is System.Workflow.ComponentModel.CompositeActivity)
                {
                    ProcessNestedChildren(interopBody, validationResults);
                }

                return;
            }

            void ProcessAtRootLevel(System.Workflow.ComponentModel.Activity interopBody, HashSet<InteropValidationEnum> validationResults)
            {
                Debug.Assert(interopBody != null, "Interop Body is null");
                Debug.Assert(validationResults != null, "The HashSet of validation results is null");

                if (interopBody.PersistOnClose)
                {
                    validationResults.Add(InteropValidationEnum.PersistOnClose);
                }

                Type interopBodyType = interopBody.GetType();
                if (interopBodyType == typeof(System.Workflow.ComponentModel.TransactionScopeActivity))
                {
                    validationResults.Add(InteropValidationEnum.TransactionScope);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.CodeActivity))
                {
                    validationResults.Add(InteropValidationEnum.Code);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.DelayActivity))
                {
                    validationResults.Add(InteropValidationEnum.Delay);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.InvokeWebServiceActivity))
                {
                    validationResults.Add(InteropValidationEnum.InvokeWebService);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.InvokeWorkflowActivity))
                {
                    validationResults.Add(InteropValidationEnum.InvokeWorkflow);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.PolicyActivity))
                {
                    validationResults.Add(InteropValidationEnum.Policy);
                }
                else if (interopBodyType.FullName == "System.Workflow.Activities.SendActivity")
                {
                    validationResults.Add(InteropValidationEnum.Send);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.SetStateActivity))
                {
                    validationResults.Add(InteropValidationEnum.SetState);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.WebServiceFaultActivity))
                {
                    validationResults.Add(InteropValidationEnum.WebServiceFault);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.WebServiceInputActivity))
                {
                    validationResults.Add(InteropValidationEnum.WebServiceInput);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.WebServiceOutputActivity))
                {
                    validationResults.Add(InteropValidationEnum.WebServiceOutput);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.CompensateActivity))
                {
                    validationResults.Add(InteropValidationEnum.Compensate);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.SuspendActivity))
                {
                    validationResults.Add(InteropValidationEnum.Suspend);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.TerminateActivity))
                {
                    validationResults.Add(InteropValidationEnum.Terminate);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.ThrowActivity))
                {
                    validationResults.Add(InteropValidationEnum.Throw);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.ConditionedActivityGroup))
                {
                    validationResults.Add(InteropValidationEnum.ConditionedActivityGroup);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.EventHandlersActivity))
                {
                    validationResults.Add(InteropValidationEnum.EventHandlers);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.EventHandlingScopeActivity))
                {
                    validationResults.Add(InteropValidationEnum.EventHandlingScope);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.IfElseActivity))
                {
                    validationResults.Add(InteropValidationEnum.IfElse);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.ListenActivity))
                {
                    validationResults.Add(InteropValidationEnum.Listen);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.ParallelActivity))
                {
                    validationResults.Add(InteropValidationEnum.Parallel);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.ReplicatorActivity))
                {
                    validationResults.Add(InteropValidationEnum.Replicator);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.SequenceActivity))
                {
                    validationResults.Add(InteropValidationEnum.Sequence);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.CompensatableSequenceActivity))
                {
                    validationResults.Add(InteropValidationEnum.CompensatableSequence);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.EventDrivenActivity))
                {
                    validationResults.Add(InteropValidationEnum.EventDriven);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.IfElseBranchActivity))
                {
                    validationResults.Add(InteropValidationEnum.IfElseBranch);
                }
                else if (interopBodyType.FullName == "System.Workflow.Activities.ReceiveActivity")
                {
                    validationResults.Add(InteropValidationEnum.Receive);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.SequentialWorkflowActivity))
                {
                    validationResults.Add(InteropValidationEnum.SequentialWorkflow);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.StateFinalizationActivity))
                {
                    validationResults.Add(InteropValidationEnum.StateFinalization);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.StateInitializationActivity))
                {
                    validationResults.Add(InteropValidationEnum.StateInitialization);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.StateActivity))
                {
                    validationResults.Add(InteropValidationEnum.State);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.StateMachineWorkflowActivity))
                {
                    validationResults.Add(InteropValidationEnum.StateMachineWorkflow);
                }
                else if (interopBodyType == typeof(System.Workflow.Activities.WhileActivity))
                {
                    validationResults.Add(InteropValidationEnum.While);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.CancellationHandlerActivity))
                {
                    validationResults.Add(InteropValidationEnum.CancellationHandler);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.CompensatableTransactionScopeActivity))
                {
                    validationResults.Add(InteropValidationEnum.CompensatableTransactionScope);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.CompensationHandlerActivity))
                {
                    validationResults.Add(InteropValidationEnum.CompensationHandler);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.FaultHandlerActivity))
                {
                    validationResults.Add(InteropValidationEnum.FaultHandler);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.FaultHandlersActivity))
                {
                    validationResults.Add(InteropValidationEnum.FaultHandlers);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.SynchronizationScopeActivity))
                {
                    validationResults.Add(InteropValidationEnum.SynchronizationScope);
                }
                else if (interopBodyType == typeof(System.Workflow.ComponentModel.ICompensatableActivity))
                {
                    validationResults.Add(InteropValidationEnum.ICompensatable);
                }
            }

            void ProcessNestedChildren(System.Workflow.ComponentModel.Activity interopBody, HashSet<InteropValidationEnum> validationResults)
            {
                Debug.Assert(interopBody != null, "Interop Body is null");
                Debug.Assert(validationResults != null, "The HashSet of validation results is null");
                bool persistOnClose = false;

                foreach (System.Workflow.ComponentModel.Activity activity in interopBody.CollectNestedActivities())
                {
                    if (activity.PersistOnClose)
                    {
                        persistOnClose = true;
                    }

                    if (activity is System.Workflow.ComponentModel.TransactionScopeActivity)
                    {
                        validationResults.Add(InteropValidationEnum.TransactionScope);
                    }
                    else if (activity is System.Workflow.Activities.InvokeWorkflowActivity)
                    {
                        validationResults.Add(InteropValidationEnum.InvokeWorkflow);
                    }
                    // SendActivity is sealed
                    else if (activity.GetType().FullName == "System.Workflow.Activities.SendActivity")
                    {
                        validationResults.Add(InteropValidationEnum.Send);
                    }
                    else if (activity is System.Workflow.Activities.WebServiceFaultActivity)
                    {
                        validationResults.Add(InteropValidationEnum.WebServiceFault);
                    }
                    else if (activity is System.Workflow.Activities.WebServiceInputActivity)
                    {
                        validationResults.Add(InteropValidationEnum.WebServiceInput);
                    }
                    else if (activity is System.Workflow.Activities.WebServiceOutputActivity)
                    {
                        validationResults.Add(InteropValidationEnum.WebServiceOutput);
                    }
                    else if (activity is System.Workflow.ComponentModel.CompensateActivity)
                    {
                        validationResults.Add(InteropValidationEnum.Compensate);
                    }
                    else if (activity is System.Workflow.ComponentModel.SuspendActivity)
                    {
                        validationResults.Add(InteropValidationEnum.Suspend);
                    }
                    else if (activity is System.Workflow.Activities.CompensatableSequenceActivity)
                    {
                        validationResults.Add(InteropValidationEnum.CompensatableSequence);
                    }
                    // ReceiveActivity is sealed
                    else if (activity.GetType().FullName == "System.Workflow.Activities.ReceiveActivity")
                    {
                        validationResults.Add(InteropValidationEnum.Receive);
                    }
                    else if (activity is System.Workflow.ComponentModel.CompensatableTransactionScopeActivity)
                    {
                        validationResults.Add(InteropValidationEnum.CompensatableTransactionScope);
                    }
                    else if (activity is System.Workflow.ComponentModel.CompensationHandlerActivity)
                    {
                        validationResults.Add(InteropValidationEnum.CompensationHandler);
                    }
                    else if (activity is System.Workflow.ComponentModel.ICompensatableActivity)
                    {
                        validationResults.Add(InteropValidationEnum.ICompensatable);
                    }
                }

                if (persistOnClose)
                {
                    validationResults.Add(InteropValidationEnum.PersistOnClose);
                }
            }
        }

        //This needs to be in sync with the table in the spec
        //We use this internally to keep a hashset of validation data
        enum InteropValidationEnum
        {
            Code,
            Delay,
            InvokeWebService,
            InvokeWorkflow,
            Policy,
            Send,
            SetState,
            WebServiceFault,
            WebServiceInput,
            WebServiceOutput,
            Compensate,
            Suspend,
            ConditionedActivityGroup,
            EventHandlers,
            EventHandlingScope,
            IfElse,
            Listen,
            Parallel,
            Replicator,
            Sequence,
            CompensatableSequence,
            EventDriven,
            IfElseBranch,
            Receive,
            SequentialWorkflow,
            StateFinalization,
            StateInitialization,
            State,
            StateMachineWorkflow,
            While,
            CancellationHandler,
            CompensatableTransactionScope,
            CompensationHandler,
            FaultHandler,
            FaultHandlers,
            SynchronizationScope,
            TransactionScope,
            ICompensatable,
            PersistOnClose,
            Terminate,
            Throw
        }

        class ObtainType : CodeActivity<Type>
        {
            public ObtainType()
            {
            }

            public InArgument<Activity> Input
            {
                get;
                set;
            }

            protected override Type Execute(CodeActivityContext context)
            {
                return this.Input.Get(context).GetType();
            }
        }

        abstract class InteropProperty : PropertyDescriptor
        {
            Interop owner;
            bool isValid;

            public InteropProperty(Interop owner, string name, Attribute[] propertyInfoAttributes)
                : base(name, propertyInfoAttributes)
            {
                this.owner = owner;
                this.isValid = true;
            }

            public override Type ComponentType
            {
                get
                {
                    ThrowIfInvalid();
                    return this.owner.GetType();
                }
            }

            protected internal Interop Owner
            {
                get
                {
                    return this.owner;
                }
            }

            public override bool CanResetValue(object component)
            {
                ThrowIfInvalid();
                return false;
            }

            public override void ResetValue(object component)
            {
                ThrowIfInvalid();
            }

            public override bool ShouldSerializeValue(object component)
            {
                ThrowIfInvalid();
                return false;
            }

            protected void ThrowIfInvalid()
            {
                if (!this.isValid)
                {
                    throw new InvalidOperationException(ExecutionStringManager.InteropInvalidPropertyDescriptor);
                }
            }

            internal void Invalidate()
            {
                this.isValid = false;
            }
        }

        class ArgumentProperty : InteropProperty
        {
            string argumentName;
            Argument argument;

            public ArgumentProperty(Interop owner, string argumentName, Argument argument, Attribute[] attributes)
                : base(owner, argumentName, attributes)
            {
                this.argumentName = argumentName;
                this.argument = argument;
            }

            public override bool IsReadOnly
            {
                get
                {
                    ThrowIfInvalid();
                    return false;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    ThrowIfInvalid();
                    return GetArgument().GetType();
                }
            }

            public override object GetValue(object component)
            {
                ThrowIfInvalid();
                return GetArgument();
            }

            public override void SetValue(object component, object value)
            {
                ThrowIfInvalid();
                if (value != null)
                {
                    this.Owner.ActivityProperties[this.argumentName] = (Argument)value;
                }
                else
                {
                    this.Owner.ActivityProperties.Remove(this.argumentName);
                }
            }

            Argument GetArgument()
            {
                Argument argument;
                if (!this.Owner.ActivityProperties.TryGetValue(this.argumentName, out argument))
                {
                    argument = this.argument;
                }
                return argument;
            }
        }

        class LiteralProperty : InteropProperty
        {
            string literalName;
            Type literalType;

            public LiteralProperty(Interop owner, string literalName, Type literalType, Attribute[] attributes)
                : base(owner, literalName, attributes)
            {
                this.literalName = literalName;
                this.literalType = literalType;
            }

            public override bool IsReadOnly
            {
                get
                {
                    ThrowIfInvalid();
                    return false;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    ThrowIfInvalid();
                    return this.literalType;
                }
            }

            public override object GetValue(object component)
            {
                ThrowIfInvalid();
                return GetLiteral();
            }

            public override void SetValue(object component, object value)
            {
                ThrowIfInvalid();
                this.Owner.ActivityMetaProperties[this.literalName] = value;
            }

            object GetLiteral()
            {
                object literal;
                if (this.Owner.ActivityMetaProperties.TryGetValue(this.literalName, out literal))
                {
                    return literal;
                }
                else
                {
                    return null;
                }
            }
        }

        class InteropPersistenceParticipant : PersistenceIOParticipant
        {
            public InteropPersistenceParticipant()
                : base(true, false)
            {
                this.ResourceManagers = new Dictionary<string, VolatileResourceManager>();
                this.CommittedResourceManagers = new Dictionary<Transaction, Dictionary<string, VolatileResourceManager>>();
            }

            Dictionary<string, VolatileResourceManager> ResourceManagers
            {
                get;
                set;
            }

            Dictionary<Transaction, Dictionary<string, VolatileResourceManager>> CommittedResourceManagers
            {
                get;
                set;
            }

            protected override IAsyncResult BeginOnSave(IDictionary<XName, object> readWriteValues, IDictionary<System.Xml.Linq.XName, object> writeOnlyValues, TimeSpan timeout, AsyncCallback callback, object state)
            {
                try
                {
                    foreach (VolatileResourceManager rm in this.ResourceManagers.Values)
                    {
                        rm.Commit();
                    }
                }
                finally
                {
                    this.CommittedResourceManagers.Add(Transaction.Current, this.ResourceManagers);
                    this.ResourceManagers = new Dictionary<string, VolatileResourceManager>();
                    Transaction.Current.TransactionCompleted += new TransactionCompletedEventHandler(Current_TransactionCompleted);
                }

                return new CompletedAsyncResult(callback, state);
            }

            protected override void EndOnSave(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            void Current_TransactionCompleted(object sender, TransactionEventArgs e)
            {
                if (e.Transaction.TransactionInformation.Status == TransactionStatus.Committed)
                {
                    foreach (VolatileResourceManager rm in this.CommittedResourceManagers[e.Transaction].Values)
                    {
                        rm.Complete();
                    }
                }
                else
                {
                    foreach (VolatileResourceManager rm in this.CommittedResourceManagers[e.Transaction].Values)
                    {
                        rm.ClearAllBatchedWork();
                    }
                }
                this.CommittedResourceManagers.Remove(e.Transaction);
            }

            protected override void Abort()
            {
                foreach (VolatileResourceManager rm in this.ResourceManagers.Values)
                {
                    rm.ClearAllBatchedWork();
                }

                this.ResourceManagers = new Dictionary<string, VolatileResourceManager>();
            }

            internal void Add(string activityId, VolatileResourceManager rm)
            {
                // Add and OnSave shouldn't be called at the same time.  A lock isn't needed here.
                this.ResourceManagers.Add(activityId, rm);
            }
        }

        [DataContract]
        class InteropEnlistment : IEnlistmentNotification
        {
            VolatileResourceManager resourceManager;
            Transaction transaction;

            public InteropEnlistment()
            {
            }

            public InteropEnlistment(Transaction transaction, VolatileResourceManager resourceManager)
            {
                this.resourceManager = resourceManager;
                this.transaction = transaction;
                this.IsValid = true;
            }

            public bool IsValid { get; set; }

            public void Commit(Enlistment enlistment)
            {
                this.resourceManager.Complete();
                enlistment.Done();
            }

            public void InDoubt(Enlistment enlistment)
            {
                // Following the WF3 runtime behavior - Aborting during InDoubt
                this.Rollback(enlistment);
            }

            public void Prepare(PreparingEnlistment preparingEnlistment)
            {
                using (System.Transactions.TransactionScope ts = new System.Transactions.TransactionScope(this.transaction))
                {
                    this.resourceManager.Commit();
                    ts.Complete();
                }
                preparingEnlistment.Prepared();
            }

            public void Rollback(Enlistment enlistment)
            {
                this.resourceManager.ClearAllBatchedWork();
                enlistment.Done();
            }
        }

        class CompletedAsyncResult : IAsyncResult
        {
            AsyncCallback callback;
            bool endCalled;
            ManualResetEvent manualResetEvent;
            object state;
            object thisLock;

            public CompletedAsyncResult(AsyncCallback callback, object state)
            {
                this.callback = callback;
                this.state = state;
                this.thisLock = new object();

                if (callback != null)
                {
                    try
                    {
                        callback(this);
                    }
                    catch (Exception e) // transfer to another thread, this is a fatal situation
                    {
                        throw new InvalidProgramException(ExecutionStringManager.AsyncCallbackThrewException, e);
                    }
                }
            }

            public static void End(IAsyncResult result)
            {
                if (result == null)
                {
                    throw new ArgumentNullException("result");
                }

                CompletedAsyncResult asyncResult = result as CompletedAsyncResult;

                if (asyncResult == null)
                {
                    throw new ArgumentException(ExecutionStringManager.InvalidAsyncResult, "result");
                }

                if (asyncResult.endCalled)
                {
                    throw new InvalidOperationException(ExecutionStringManager.EndCalledTwice);
                }

                asyncResult.endCalled = true;

                if (asyncResult.manualResetEvent != null)
                {
                    asyncResult.manualResetEvent.Close();
                }
            }

            public object AsyncState
            {
                get
                {
                    return state;
                }
            }

            public WaitHandle AsyncWaitHandle
            {
                get
                {
                    if (this.manualResetEvent != null)
                    {
                        return this.manualResetEvent;
                    }

                    lock (ThisLock)
                    {
                        if (this.manualResetEvent == null)
                        {
                            this.manualResetEvent = new ManualResetEvent(true);
                        }
                    }

                    return this.manualResetEvent;
                }
            }

            public bool CompletedSynchronously
            {
                get
                {
                    return true;
                }
            }

            public bool IsCompleted
            {
                get
                {
                    return true;
                }
            }

            object ThisLock
            {
                get
                {
                    return this.thisLock;
                }
            }
        }
    }
}
