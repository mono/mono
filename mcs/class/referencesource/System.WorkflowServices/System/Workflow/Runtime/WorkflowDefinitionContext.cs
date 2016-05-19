//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.Workflow.Runtime
{
    using System.ComponentModel.Design;
    using System.Reflection;
    using System.Runtime;
    using System.ServiceModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    abstract class WorkflowDefinitionContext
    {
        WorkflowRuntime workflowRuntime;

        public abstract string ConfigurationName
        {
            get;
        }

        public abstract string WorkflowName
        {
            get;
        }

        internal protected WorkflowRuntime WorkflowRuntime
        {
            get
            {
                Fx.Assert(this.workflowRuntime != null, "Attempt to call WorkflowRuntime before Register");
                return this.workflowRuntime;
            }
        }

        public abstract WorkflowInstance CreateWorkflow();
        public abstract WorkflowInstance CreateWorkflow(Guid instanceId);
        public abstract Activity GetWorkflowDefinition();

        internal void Register(WorkflowRuntime workflowRuntime, bool validate)
        {
            if (workflowRuntime == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("workflowRuntime");
            }

            this.workflowRuntime = workflowRuntime;

            OnRegister();

            if (!this.workflowRuntime.IsStarted)
            {
                this.workflowRuntime.StartRuntime();
            }

            if (validate)
            {
                ValidateDefinition();
            }
        }

        protected static TypeProvider CreateTypeProvider(Activity rootActivity)
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
                    {
                        typeProvider.AddAssembly(referencedAssembly);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e))
                    {
                        throw;
                    }
                }

                if (referencedAssembly == null && assemblyName.CodeBase != null)
                {
                    typeProvider.AddAssemblyReference(assemblyName.CodeBase);
                }
            }

            return typeProvider;
        }
        protected abstract void OnRegister();
        protected abstract void OnValidate(ValidationErrorCollection errors);

        void ValidateDefinition()
        {
            ValidationErrorCollection errors = new ValidationErrorCollection();
            Activity rootActivity = GetWorkflowDefinition();

            ITypeProvider typeProvider = CreateTypeProvider(rootActivity);

            ServiceContainer serviceContainer = new ServiceContainer();
            serviceContainer.AddService(typeof(ITypeProvider), typeProvider);

            ValidationManager validationManager = new ValidationManager(serviceContainer);
            foreach (Validator validator in validationManager.GetValidators(rootActivity.GetType()))
            {
                foreach (ValidationError error in validator.Validate(validationManager, rootActivity))
                {
                    if (!error.UserData.Contains(typeof(Activity)))
                    {
                        error.UserData[typeof(Activity)] = rootActivity;
                    }

                    errors.Add(error);
                }
            }

            OnValidate(errors);

            if (errors.HasErrors)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new WorkflowValidationFailedException(SR2.WorkflowValidationFailed, errors));
            }
        }

    }
}
