//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Reflection;
    using System.ServiceModel;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Design;
    using System.Workflow.ComponentModel.Compiler;

    [Obsolete("The System.Workflow.* types are deprecated.  Instead, please use the new types from System.Activities.*")]
    public sealed class WorkflowServiceAttributesDynamicPropertyValidator : Validator
    {
        public override ValidationErrorCollection Validate(ValidationManager manager, object obj)
        {
            if (manager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("manager");
            }

            ValidationErrorCollection validationErrors = ValidationHelpers.ValidateObject(manager, obj);
            if (validationErrors.Count == 0)
            {
                if (manager.Context == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_ContextStackMissing)));
                }

                Activity rootActivity = manager.Context[typeof(Activity)] as Activity;
                if (rootActivity == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new InvalidOperationException(SR2.GetString(SR2.Error_ContextStackItemMissing, typeof(Activity).Name)));
                }
                else
                {
                    validationErrors.AddRange(ValidationHelper.ValidateAllServiceOperationsImplemented(manager, rootActivity));
                }
            }

            return validationErrors;
        }

    }
}
