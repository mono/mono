// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Description
{
    using System.Reflection;

    internal static class TaskOperationDescriptionValidator
    {
        internal static void Validate(OperationDescription operationDescription, bool isForService)
        {
            MethodInfo taskMethod = operationDescription.TaskMethod;
            if (taskMethod != null)
            {
                if (isForService)
                {
                    // no other method ([....], async) is allowed to co-exist with a task-based method on the server-side.
                    EnsureNoSyncMethod(operationDescription);
                    EnsureNoBeginEndMethod(operationDescription);
                }
                else
                {
                    // no out/ref parameter is allowed on the client-side.
                    EnsureNoOutputParameters(taskMethod);
                }

                EnsureParametersAreSupported(taskMethod);
            }
        }

        private static void EnsureNoSyncMethod(OperationDescription operation)
        {
            if (operation.SyncMethod != null)
            {
                string method1Name = operation.TaskMethod.Name;
                string method2Name = operation.SyncMethod.Name;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotHaveTwoOperationsWithTheSameName3, method1Name, method2Name, operation.DeclaringContract.ContractType)));
            }
        }

        private static void EnsureNoBeginEndMethod(OperationDescription operation)
        {
            if (operation.BeginMethod != null)
            {
                string method1Name = operation.TaskMethod.Name;
                string method2Name = operation.BeginMethod.Name;
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.CannotHaveTwoOperationsWithTheSameName3, method1Name, method2Name, operation.DeclaringContract.ContractType)));
            }
        }

        private static void EnsureParametersAreSupported(MethodInfo method)
        {
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                Type parameterType = parameter.ParameterType;
                if ((parameterType == ServiceReflector.CancellationTokenType) ||
                    (parameterType.IsGenericType && parameterType.GetGenericTypeDefinition() == ServiceReflector.IProgressType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new NotSupportedException(SR.GetString(SR.TaskMethodParameterNotSupported, parameterType)));
                }
            }
        }

        private static void EnsureNoOutputParameters(MethodInfo method)
        {
            if (ServiceReflector.HasOutputParameters(method, false))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.TaskMethodMustNotHaveOutParameter)));
            }
        }
    }
}
