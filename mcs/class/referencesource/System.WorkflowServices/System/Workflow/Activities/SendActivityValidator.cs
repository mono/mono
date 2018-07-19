//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.Workflow.Activities
{
    using System.Workflow.ComponentModel.Compiler;
    using System.Workflow.ComponentModel;
    using System.ServiceModel;
    using System.Reflection;
    using System.Collections.Generic;

    class SendActivityValidator : ActivityValidator
    {
        public override ValidationErrorCollection Validate(
            ValidationManager manager,
            object obj)
        {
            ValidationErrorCollection validationErrors = base.Validate(manager, obj);

            SendActivity sendActivity = obj as SendActivity;
            if (sendActivity == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("obj",
                    SR2.GetString(SR2.Error_ArgumentTypeInvalid, "obj", typeof(SendActivity)));
            }

            ITypeProvider typeProvider = manager.GetService(typeof(ITypeProvider)) as ITypeProvider;
            if (typeProvider == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR2.GetString(SR2.General_MissingService, typeof(ITypeProvider).Name)));
            }

            if (sendActivity.ServiceOperationInfo == null)
            {
                validationErrors.Add(
                    new ValidationError(
                    SR2.GetString(SR2.Error_ServiceOperationInfoNotSpecified,
                    sendActivity.Name),
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
                    sendActivity,
                    sendActivity.ServiceOperationInfo,
                    manager);

                validationErrors.AddRange(operationInfoValidationErrors);

                // do not validate parameter binding if the operation info is not valid
                // we might generate noise and false positives.
                //
                if (operationInfoValidationErrors.Count == 0)
                {
                    validationErrors.AddRange(
                        ValidationHelper.ValidateParameterBindings(sendActivity, sendActivity.ServiceOperationInfo,
                        sendActivity.ParameterBindings, manager));
                }

                // validate the endpoint
                //
                validationErrors.AddRange(
                    ValidationHelper.ValidateChannelToken(sendActivity, manager));
            }

            return validationErrors;
        }
    }
}
