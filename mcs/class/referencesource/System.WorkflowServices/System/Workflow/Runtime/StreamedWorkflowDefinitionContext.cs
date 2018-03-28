//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Runtime
{
    using System.Collections;
    using System.ComponentModel.Design;
    using System.ComponentModel.Design.Serialization;
    using System.IO;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel.Serialization;

    class StreamedWorkflowDefinitionContext : WorkflowDefinitionContext
    {
        object lockObject = new object();

        // Double-checked locking pattern requires volatile for read/write synchronization
        volatile System.Workflow.ComponentModel.Activity rootActivity = null;
        byte[] ruleDefinition = null;
        ITypeProvider typeProvider = null;
        byte[] workflowDefinition = null;

        internal StreamedWorkflowDefinitionContext(Stream workflowDefinition, Stream ruleDefinition, ITypeProvider typeProvider)
        {
            if (workflowDefinition == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowDefinition");
            }

            this.workflowDefinition = new byte[workflowDefinition.Length];
            workflowDefinition.Read(this.workflowDefinition, 0, (int) workflowDefinition.Length);

            if (ruleDefinition != null)
            {
                this.ruleDefinition = new byte[ruleDefinition.Length];
                ruleDefinition.Read(this.ruleDefinition, 0, (int) ruleDefinition.Length);
            }

            this.typeProvider = typeProvider;
        }

        internal StreamedWorkflowDefinitionContext(string workflowDefinitionPath, string ruleDefinitionPath, ITypeProvider typeProvider)
        {
            FileStream workflowDefStream = null;
            FileStream ruleDefStream = null;

            if (workflowDefinitionPath == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowDefinitionPath");
            }

            try
            {
                workflowDefStream = new FileStream(workflowDefinitionPath, FileMode.Open, FileAccess.Read);

                if (ruleDefinitionPath != null)
                {
                    ruleDefStream = new FileStream(ruleDefinitionPath, FileMode.Open, FileAccess.Read);
                }

                this.workflowDefinition = new byte[workflowDefStream.Length];
                workflowDefStream.Read(workflowDefinition, 0, (int) workflowDefStream.Length);

                if (ruleDefStream != null)
                {
                    this.ruleDefinition = new byte[ruleDefStream.Length];
                    ruleDefStream.Read(this.ruleDefinition, 0, (int) ruleDefStream.Length);
                }

                this.typeProvider = typeProvider;
            }
            finally
            {
                if (workflowDefStream != null)
                {
                    workflowDefStream.Close();
                }

                if (ruleDefStream != null)
                {
                    ruleDefStream.Close();
                }
            }
        }

        // This is a valid catch of all exceptions
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        // We don't have tracing in Silver as of M2
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "Reliability104:CaughtAndHandledExceptionsRule")]
        public override string ConfigurationName
        {
            get
            {
                if (rootActivity == null)
                {
                    try
                    {
                        GetWorkflowDefinition();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
#if !_PRESHARP_
                            // this throw in a getter is valid 
                            throw;
#endif
                        }
                    }
                }
                return rootActivity != null ? rootActivity.QualifiedName : null;
            }
        }


        // This is a valid catch of all exceptions
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        // We don't have tracing in Silver as of M2
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Reliability", "Reliability104:CaughtAndHandledExceptionsRule")]
        public override string WorkflowName
        {
            get
            {
                if (rootActivity == null)
                {
                    try
                    {
                        GetWorkflowDefinition();
                    }
                    catch (Exception exception)
                    {
                        if (Fx.IsFatal(exception))
                        {
#if !_PRESHARP_
                            // this throw in a getter is valid 
                            throw;
#endif
                        }
                    }
                }
                return rootActivity != null ? NamingHelper.XmlName(rootActivity.QualifiedName) : null;
            }
        }


        public override WorkflowInstance CreateWorkflow()
        {
            return this.CreateWorkflow(Guid.NewGuid());
        }

        public override WorkflowInstance CreateWorkflow(Guid instanceId)
        {
            System.IO.Stream definitionStream = null;
            System.IO.Stream ruleStream = null;
            System.Xml.XmlReader definitionReader = null;
            System.Xml.XmlReader ruleReader = null;

            try
            {
                definitionStream = new System.IO.MemoryStream(workflowDefinition);
                definitionStream.Position = 0;

                definitionReader = System.Xml.XmlReader.Create(definitionStream);

                if (ruleDefinition != null)
                {
                    ruleStream = new System.IO.MemoryStream(ruleDefinition);
                    ruleStream.Position = 0;
                    ruleReader = System.Xml.XmlReader.Create(ruleStream);
                }
                return this.WorkflowRuntime.CreateWorkflow(definitionReader, ruleReader, null, instanceId);
            }
            finally
            {
                if (definitionStream != null)
                {
                    definitionStream.Dispose();
                }
                if (ruleStream != null)
                {
                    ruleStream.Dispose();
                }
            }
        }

        public override System.Workflow.ComponentModel.Activity GetWorkflowDefinition()
        {
            if (rootActivity == null)
            {
                lock (lockObject)
                {
                    if (rootActivity == null)
                    {
                        rootActivity = DeSerizalizeDefinition(workflowDefinition, ruleDefinition);
                    }
                }
            }
            return rootActivity;
        }

        protected override void OnRegister()
        {
            if (this.typeProvider != null)
            {
                if (this.WorkflowRuntime.IsStarted)
                {
                    this.WorkflowRuntime.StopRuntime();
                }
                this.WorkflowRuntime.AddService(this.typeProvider);
            }
        }

        protected override void OnValidate(ValidationErrorCollection errors)
        {
            if (!string.IsNullOrEmpty(this.rootActivity.GetValue(WorkflowMarkupSerializer.XClassProperty) as string))
            {
                errors.Add(new ValidationError(SR2.XomlWorkflowHasClassName, ErrorNumbers.Error_XomlWorkflowHasClassName));
            }

            Queue compositeActivities = new Queue();
            compositeActivities.Enqueue(this.rootActivity);

            while (compositeActivities.Count > 0)
            {
                System.Workflow.ComponentModel.Activity activity = compositeActivities.Dequeue() as System.Workflow.ComponentModel.Activity;

                if (activity.GetValue(WorkflowMarkupSerializer.XCodeProperty) != null)
                {
                    errors.Add(new ValidationError(SR2.XomlWorkflowHasCode, ErrorNumbers.Error_XomlWorkflowHasCode));
                }

                CompositeActivity compositeActivity = activity as CompositeActivity;
                if (compositeActivity != null)
                {
                    foreach (System.Workflow.ComponentModel.Activity childActivity in compositeActivity.EnabledActivities)
                    {
                        compositeActivities.Enqueue(childActivity);
                    }
                }
            }
        }


        System.Workflow.ComponentModel.Activity DeSerizalizeDefinition(byte[] workflowDefinition, byte[] ruleDefinition)
        {
            System.IO.Stream definitionStream = null;
            System.IO.Stream ruleStream = null;

            System.Xml.XmlReader definitionReader = null;
            System.Xml.XmlReader ruleReader = null;

            try
            {
                definitionStream = new System.IO.MemoryStream(workflowDefinition);
                definitionStream.Position = 0;
                definitionReader = System.Xml.XmlReader.Create(definitionStream);

                if (ruleDefinition != null)
                {
                    ruleStream = new System.IO.MemoryStream(ruleDefinition);
                    ruleStream.Position = 0;
                    ruleReader = System.Xml.XmlReader.Create(ruleStream);
                }

                System.Workflow.ComponentModel.Activity root = null;
                ValidationErrorCollection errors = new ValidationErrorCollection();
                ServiceContainer serviceContainer = new ServiceContainer();

                if (this.typeProvider != null)
                {
                    serviceContainer.AddService(typeof(ITypeProvider), this.typeProvider);
                }

                DesignerSerializationManager manager = new DesignerSerializationManager(serviceContainer);
                try
                {
                    using (manager.CreateSession())
                    {
                        WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                        root = new WorkflowMarkupSerializer().Deserialize(xomlSerializationManager, definitionReader) as System.Workflow.ComponentModel.Activity;

                        if (root != null && ruleReader != null)
                        {
                            object rules = new WorkflowMarkupSerializer().Deserialize(xomlSerializationManager, ruleReader);
                            root.SetValue(System.Workflow.Activities.Rules.RuleDefinitions.RuleDefinitionsProperty, rules);
                        }

                        foreach (object error in manager.Errors)
                        {
                            if (error is WorkflowMarkupSerializationException)
                            {
                                errors.Add(new ValidationError(((WorkflowMarkupSerializationException) error).Message, 1));
                            }
                            else
                            {
                                errors.Add(new ValidationError(error.ToString(), 1));
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                    errors.Add(new ValidationError(e.Message, 1));
                }

                if (errors.HasErrors)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WorkflowValidationFailedException(SR2.GetString(SR2.WorkflowValidationFailed), errors));
                }

                return root;
            }
            finally
            {
                if (definitionStream != null)
                {
                    definitionStream.Dispose();
                }
                if (ruleStream != null)
                {
                    ruleStream.Dispose();
                }
            }
        }
    }
}
