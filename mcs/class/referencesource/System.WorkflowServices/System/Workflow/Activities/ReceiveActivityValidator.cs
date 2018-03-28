//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System.ServiceModel;
    using System.Reflection;
    using System.Collections.Generic;
    using System.Workflow.ComponentModel;
    using System.Workflow.ComponentModel.Compiler;

    class ReceiveActivityValidator : CompositeActivityValidator
    {
        public override ValidationErrorCollection Validate(
            ValidationManager manager,
            object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            ReceiveActivity receiveActivity = obj as ReceiveActivity;
            if (receiveActivity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("obj",
                    SR2.GetString(SR2.Error_ArgumentTypeInvalid, "obj", typeof(ReceiveActivity)));
            }

            ITypeProvider typeProvider = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.General_MissingService, typeof(ITypeProvider).Name)));
            }

            if (receiveActivity.ServiceOperationInfo == null)
            {
                validationErrors.Add(
                    new ValidationError(
                    SR2.GetString(SR2.Error_Validation_OperationInfoNotSpecified, receiveActivity.Name),
                    WorkflowServicesErrorNumbers.Error_OperationInfoNotSpecified,
                    false,
                    "ServiceOperationInfo"));
            }
            else
            {
                // validate operation info
                //
                ValidationErrorCollection operationInfoValidationErrors =
                    ValidationHelper.ValidateOperationInfo(
                    receiveActivity,
                    receiveActivity.ServiceOperationInfo,
                    manager);

                validationErrors.AddRange(operationInfoValidationErrors);

                // do not validate parameter binding if the operation info is not valid
                // we might generate noise and false positives.
                //
                if (operationInfoValidationErrors.Count == 0)
                {
                    validationErrors.AddRange(
                        ValidationHelper.ValidateParameterBindings(receiveActivity, receiveActivity.ServiceOperationInfo,
                        receiveActivity.ParameterBindings, manager));
                }

                // validate the context token
                //
                validationErrors.AddRange(
                    ValidationHelper.ValidateContextToken(receiveActivity, receiveActivity.ContextToken, manager));
            }

            // Check if the validation for all service operations being implemented
            // has been done previously. 
            // If it has been done once then ServiceOperationsImplementedValidationMarker 
            // will be on the context stack.
            //
            if (validationErrors.Count == 0 &&
                manager.Context[typeof(ServiceOperationsImplementedValidationMarker)] == null)
            {
                Activity rootActivity = receiveActivity;
                while (rootActivity.Parent != null)
                {
                    rootActivity = rootActivity.Parent;
                }

                validationErrors.AddRange(
                    ValidationHelper.ValidateAllServiceOperationsImplemented(
                    manager,
                    rootActivity));
            }

            return validationErrors;
        }
    }
}
