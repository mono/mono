//------------------------------------------------------------------------------------------------------
// <copyright file="ImpersonateOnSerializingReplyMessageProperty.cs" company="Microsoft Corporation"> 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//----------------------------------------------------------------------------------------------------

namespace System.ServiceModel.Security
{
    using System.Runtime;
    using System.Security;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Diagnostics;

    /// <summary>
    /// The helper class to enable impersonation while serializing the body of the reply message.
    /// </summary>
    public class ImpersonateOnSerializingReplyMessageProperty : IMessageProperty
    {
        const string PropertyName = "ImpersonateOnSerializingReplyMessageProperty";
        MessageRpc rpc;

        internal ImpersonateOnSerializingReplyMessageProperty(ref MessageRpc rpc)
        {
           this.rpc = rpc;
        }

        /// <summary>
        /// Gets the name of the message property.
        /// </summary>
        public static string Name
        {
            get { return PropertyName; }
        }

        /// <summary>
        /// Gets the ImpersonateOnSerializingReplyMessageProperty property from a message.
        /// </summary>
        /// <param name="message">The message to extract the property from.</param>
        /// <param name="property">An output paramter to hold the ImpersonateOnSerializingReplyMessageProperty property.</param>
        /// <returns>True if the ImpersonateOnSerializingReplyMessageProperty property was found.</returns>
        public static bool TryGet(Message message, out ImpersonateOnSerializingReplyMessageProperty property)
        {
            if (message == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("message");
            }

            return TryGet(message.Properties, out property);
        }

        /// <summary>
        /// Gets the ImpersonateOnSerializingReplyMessageProperty property from MessageProperties.
        /// </summary>
        /// <param name="properties">The MessagePropeties object.</param>
        /// <param name="property">An output paramter to hold the ImpersonateOnSerializingReplyMessageProperty property.</param>
        /// <returns>True if the ImpersonateOnSerializingReplyMessageProperty property was found.</returns>
        public static bool TryGet(MessageProperties properties, out ImpersonateOnSerializingReplyMessageProperty property)
        {
            if (properties == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("properties");
            }

            object value = null;
            if (properties.TryGetValue(PropertyName, out value))
            {
                property = value as ImpersonateOnSerializingReplyMessageProperty;
            }
            else
            {
                property = null;
            }

            return property != null;
        }
        
        /// <summary>
        /// Creates a copy of the message property.
        /// </summary>
        /// <returns>Returns a copy of the message property.</returns>
        public IMessageProperty CreateCopy()
        {
            ImpersonateOnSerializingReplyMessageProperty result = new ImpersonateOnSerializingReplyMessageProperty(ref this.rpc);
            return result;
        }
              
        /// <summary>
        /// Starts Impersonating with the caller's context if impersonation is enabled on the service and sets the appropriate principal on the thread as per the service configuration.
        /// </summary>
        /// <param name="impersonationContext">The impersonated context.</param>
        /// <param name="originalPrincipal">The original principal on the thread before invoking this method.</param>
        /// <param name="isThreadPrincipalSet">The value determines if the principal was set on the thread by the method.</param>
        /// <returns>Returns false if operation context was not available to impersonate.</returns>
        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method StartImpersonation.", Safe = "Manages the result of impersonation and properly Disposes it.")]
        [SecuritySafeCritical]
        public void StartImpersonation(out IDisposable impersonationContext, out IPrincipal originalPrincipal, out bool isThreadPrincipalSet)
        {
            impersonationContext = null;
            originalPrincipal = null;
            isThreadPrincipalSet = false;

            if (OperationContext.Current != null)
            {
                EndpointDispatcher endpointDispatcher = OperationContext.Current.EndpointDispatcher;
                if (endpointDispatcher != null)
                {
                    DispatchRuntime dispatchRuntime = endpointDispatcher.DispatchRuntime;
                    ImmutableDispatchRuntime runtime = dispatchRuntime.GetRuntime();
                    if (runtime != null && runtime.SecurityImpersonation != null)
                    {
                        runtime.SecurityImpersonation.StartImpersonation(ref this.rpc, out impersonationContext, out originalPrincipal, out isThreadPrincipalSet);
                    }
                }
            }
        }

        /// <summary>
        /// Reverts impersonation and sets the original principal on the thread.
        /// </summary>
        /// <param name="impersonationContext">The impersonation context to revert.</param>
        /// <param name="originalPrincipal">The original principal to set on the thread.</param>
        /// <param name="isThreadPrincipalSet">The value determines if the thread principal was set during impersonation.</param>
        /// <returns>Returns false if operation context was not available to revert the impersonation.</returns>
        [Fx.Tag.SecurityNote(Critical = "Calls SecurityCritical method StartImpersonation.", Safe = "Manages the result of impersonation and properly Disposes it.")]
        [SecuritySafeCritical]
        public void StopImpersonation(IDisposable impersonationContext, IPrincipal originalPrincipal, bool isThreadPrincipalSet)
        {
            if (OperationContext.Current != null)
            {
                EndpointDispatcher endpointDispatcher = OperationContext.Current.EndpointDispatcher;
                if (endpointDispatcher != null)
                {
                    DispatchRuntime dispatchRuntime = endpointDispatcher.DispatchRuntime;
                    ImmutableDispatchRuntime runtime = dispatchRuntime.GetRuntime();
                    if (runtime != null && runtime.SecurityImpersonation != null)
                    {
                       runtime.SecurityImpersonation.StopImpersonation(ref this.rpc, impersonationContext, originalPrincipal, isThreadPrincipalSet);
                    }
                }
            }
        }
    }
}
