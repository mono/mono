//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Channels;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Runtime.InteropServices;
    using System.ComponentModel;

    static class Error
    {
        const string FaultNamespace = System.ServiceModel.FaultException.Namespace;

        public static Exception ActivationAccessDenied()
        {
            return CreateFault("ComActivationAccessDenied",
                            SR.GetString(SR.ComActivationAccessDenied));
        }

        public static Exception QFENotPresent()
        {
            return CreateFault("ServiceHostStartingServiceErrorNoQFE",
                            SR.GetString(SR.ComPlusServiceHostStartingServiceErrorNoQFE));
        }

        public static Exception DirectoryNotFound(string directory)
        {
            return CreateFault("DirectoryNotFound",
                            SR.GetString(SR.TempDirectoryNotFound, directory));
        }

        public static Exception CannotAccessDirectory(string directory)
        {
            return CreateFault("CannotAccessDirectory",
                            SR.GetString(SR.CannotAccessDirectory, directory));
        }

        public static Exception ManifestCreationFailed(string file, string error)
        {
            return CreateFault("ManifestCreationFailed",
                            SR.GetString(SR.ComIntegrationManifestCreationFailed, file, error));
        }

        public static Exception ActivationFailure()
        {
            return CreateFault("ComActivationFailure",
                            SR.GetString(SR.ComActivationFailure));
        }

        public static Exception UnexpectedThreadingModel()
        {
            return CreateFault("UnexpectedThreadingModel",
                            SR.GetString(SR.UnexpectedThreadingModel));
        }

        public static Exception DllHostInitializerFoundNoServices()
        {
            return CreateFault("DllHostInitializerFoundNoServices",
                            SR.GetString(SR.ComDllHostInitializerFoundNoServices));
        }

        public static Exception ServiceMonikerSupportLoadFailed(string dllname)
        {
            return CreateFault("UnableToLoadServiceMonikerSupportDll",
                            SR.GetString(SR.UnableToLoadDll, dllname));
        }


        public static Exception CallAccessDenied()
        {
            return CreateFault("ComAccessDenied",
                            SR.GetString(SR.ComMessageAccessDenied));
        }

        public static Exception RequiresWindowsSecurity()
        {
            return CreateFault("ComWindowsIdentityRequired",
                            SR.GetString(SR.ComRequiresWindowsSecurity));
        }

        public static Exception NoAsyncOperationsAllowed()
        {
            return CreateFault("NoAsyncOperationsAllowed",
                            SR.GetString(SR.ComNoAsyncOperationsAllowed));
        }

        public static Exception DuplicateOperation()
        {
            return CreateFault("DuplicateOperation",
                            SR.GetString(SR.ComDuplicateOperation));
        }

        public static Exception InconsistentSessionRequirements()
        {
            return CreateFault("ComInconsistentSessionRequirements",
                            SR.GetString(SR.ComInconsistentSessionRequirements));
        }

        public static Exception TransactionMismatch()
        {
            // NOTE: The fault created here is identical to the one
            //       created by the TransactionBehavior when
            //       concurrent transactions are not supported.
            //
            return CreateFault("Transactions",
                            SR.GetString(SR.SFxTransactionsNotSupported));
        }

        public static Exception ListenerInitFailed(string message)
        {
            return new ComPlusListenerInitializationException(message);
        }

        public static Exception ListenerInitFailed(string message,
                                                   Exception inner)
        {
            return new ComPlusListenerInitializationException(message, inner);
        }


        static Exception CreateFault(string code, string reason)
        {
            FaultCode codeObj = FaultCode.CreateSenderFaultCode(code, FaultNamespace);
            FaultReason reasonObj = new FaultReason(reason, CultureInfo.CurrentCulture);

            return new FaultException(reasonObj, codeObj);
        }
    }

    [Serializable]
    internal class ComPlusListenerInitializationException : Exception
    {
        public ComPlusListenerInitializationException()
            : base()
        {
        }
        public ComPlusListenerInitializationException(string message)
            : base(message)
        {
        }

        public ComPlusListenerInitializationException(string message,
                                                      Exception inner)
            : base(message, inner)
        {
        }
        protected ComPlusListenerInitializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }

    [Serializable]
    internal class ComPlusProxyProviderException : Exception
    {
        public ComPlusProxyProviderException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
