#region Imports

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.IO;
using System.ComponentModel.Design;
using System.ComponentModel.Design.Serialization;
using System.Workflow.ComponentModel;
using System.Workflow.ComponentModel.Compiler;
using System.Workflow.ComponentModel.Serialization;
using System.Workflow.ComponentModel.Design;
using System.Workflow.Runtime;

#endregion


namespace System.Workflow.Runtime.Hosting
{

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public class DefaultWorkflowLoaderService : WorkflowLoaderService
    {
        protected internal override Activity CreateInstance(Type workflowType)
        {
            if (workflowType == null)
                throw new ArgumentNullException("workflowType");
            
            if (!typeof(Activity).IsAssignableFrom(workflowType))
                throw new ArgumentException(ExecutionStringManager.TypeMustImplementRootActivity);

            if (workflowType.GetConstructor(System.Type.EmptyTypes) == null)
                throw new ArgumentException(ExecutionStringManager.TypeMustHavePublicDefaultConstructor);

            return Activator.CreateInstance(workflowType) as Activity;
        }
        
        // This function will create a new root activity definition tree by deserializing the xoml and the rules file.
        protected internal override Activity CreateInstance(XmlReader workflowDefinitionReader, XmlReader rulesReader)
        {
            if (workflowDefinitionReader == null)
                throw new ArgumentNullException("workflowDefinitionReader");

            Activity root = null;
            ValidationErrorCollection errors = new ValidationErrorCollection();
            ServiceContainer serviceContainer = new ServiceContainer();
            ITypeProvider typeProvider = this.Runtime.GetService<ITypeProvider>();
            if (typeProvider != null)
                serviceContainer.AddService(typeof(ITypeProvider), typeProvider);

            DesignerSerializationManager manager = new DesignerSerializationManager(serviceContainer);
            try
            {
                using (manager.CreateSession())
                {
                    WorkflowMarkupSerializationManager xomlSerializationManager = new WorkflowMarkupSerializationManager(manager);
                    root = new WorkflowMarkupSerializer().Deserialize(xomlSerializationManager, workflowDefinitionReader) as Activity;
                    if (root != null && rulesReader != null)
                    {
                        object rules = new WorkflowMarkupSerializer().Deserialize(xomlSerializationManager, rulesReader);
                        root.SetValue(ConditionTypeConverter.DeclarativeConditionDynamicProp, rules);
                    }

                    foreach (object error in manager.Errors)
                    {
                        if (error is WorkflowMarkupSerializationException)
                            errors.Add(new ValidationError(((WorkflowMarkupSerializationException)error).Message, ErrorNumbers.Error_SerializationError));
                        else
                            errors.Add(new ValidationError(error.ToString(), ErrorNumbers.Error_SerializationError));
                    }
                }
            }
            catch (Exception e)
            {
                errors.Add(new ValidationError(e.Message, ErrorNumbers.Error_SerializationError));
            }

            if (errors.HasErrors)
                throw new WorkflowValidationFailedException(ExecutionStringManager.WorkflowValidationFailure, errors);

            return root;
        }
    }
}
