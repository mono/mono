//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.EnterpriseServices;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.Security.Principal;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics;
    using System.ServiceModel.Dispatcher;
    using SafeCloseHandle = System.IdentityModel.SafeCloseHandle;
    
    class ComPlusInstanceProvider : IInstanceProvider
    {
        ServiceInfo info;
        static readonly Guid IID_IUnknown = new Guid("00000000-0000-0000-C000-000000000046");

        public ComPlusInstanceProvider(ServiceInfo info)
        {
            this.info = info;
        }

        public object GetInstance(InstanceContext instanceContext)
        {
            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.ComPlusInstanceProviderRequiresMessage0)));
        }

        // We call ContextUtil.IsInTransaction and ContextUtil.TransactionId, from a non-APTCA assembly. There is no identified security vulnerability with these properties, 
        // so we can't justify adding a demand for full trust here. Both properties call code marked as usafe, but no user input is passed to it and results are not
        // cached (so there is no leak as a side-effect).
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods)]
        public object GetInstance(InstanceContext instanceContext, Message message)
        {

            object result = null;
            Guid incomingTransactionID = Guid.Empty;
            if (ContextUtil.IsInTransaction)
                incomingTransactionID = ContextUtil.TransactionId;
            ComPlusInstanceCreationTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationInstanceCreationRequest,
                            SR.TraceCodeComIntegrationInstanceCreationRequest, this.info, message, incomingTransactionID);

            WindowsIdentity callerIdentity = null;
            callerIdentity = MessageUtil.GetMessageIdentity(message);
            WindowsImpersonationContext impersonateContext = null;
            try
            {
                try
                {

                    if (this.info.HostingMode ==
                        HostingMode.WebHostOutOfProcess)
                    {

                        if (SecurityUtils.IsAtleastImpersonationToken(new SafeCloseHandle(callerIdentity.Token, false)))
                            impersonateContext = callerIdentity.Impersonate();
                        else
                            throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new COMException (SR.GetString(SR.BadImpersonationLevelForOutOfProcWas), HR.ERROR_BAD_IMPERSONATION_LEVEL));
                        
                    }

                    CLSCTX clsctx = CLSCTX.SERVER;
                    if (PlatformSupportsBitness && (this.info.HostingMode ==
                        HostingMode.WebHostOutOfProcess))
                    {
                        if (this.info.Bitness == Bitness.Bitness32)
                        {
                            clsctx |= CLSCTX.ACTIVATE_32_BIT_SERVER;
                        }
                        else
                        {
                            clsctx |= CLSCTX.ACTIVATE_64_BIT_SERVER;
                        }
                    }

                    result = SafeNativeMethods.CoCreateInstance(
                            info.Clsid,
                            null,
                            clsctx,
                            IID_IUnknown);
                }
                finally
                {
                    if (impersonateContext != null)
                        impersonateContext.Undo();
                }
            }
            catch (Exception e)
            {
                if (Fx.IsFatal(e))
                    throw;    

                Uri from = null;
                if (message.Headers.From != null)
                    from = message.Headers.From.Uri;

                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.ComPlus,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.ComPlusInstanceCreationError,
                    from == null ? string.Empty : from.ToString(),
                    this.info.AppID.ToString(),
                    this.info.Clsid.ToString(),
                    incomingTransactionID.ToString(),
                    callerIdentity.Name,
                    e.ToString());

                throw TraceUtility.ThrowHelperError(e, message);
            }
            
            
            TransactionProxy proxy = instanceContext.Extensions.Find<TransactionProxy>();
            if (proxy != null)
            {
                proxy.InstanceID = result.GetHashCode();
            }

            ComPlusInstanceCreationTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationInstanceCreationSuccess,
                        SR.TraceCodeComIntegrationInstanceCreationSuccess, this.info, message, result.GetHashCode(), incomingTransactionID);
            return result;
        }

        public void ReleaseInstance(InstanceContext instanceContext, object instance)
        {
            int instanceID = instance.GetHashCode();
            IDisposable disposable = instance as IDisposable;
            if (disposable != null)
            {
                disposable.Dispose();
            }
            else
            {
                // All ServicedComponents are disposable, so we don't
                // have to worry about getting a ServicedComponent
                // here.
                //
                Marshal.ReleaseComObject(instance);
            }

            ComPlusInstanceCreationTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationInstanceReleased,
                        SR.TraceCodeComIntegrationInstanceReleased, this.info, instanceContext, instanceID);
        }

        static bool platformSupportsBitness;
        static bool platformSupportsBitnessSet;

        static bool PlatformSupportsBitness
        {
            get
            {
                if (!platformSupportsBitnessSet)
                {
                    // Bitness is supported on Windows 2003 Server SP1 or
                    // greater.
                    //
                    if (Environment.OSVersion.Version.Major > 5)
                        platformSupportsBitness = true;
                    else if (Environment.OSVersion.Version.Major == 5)
                    {
                        if (Environment.OSVersion.Version.Minor > 2)
                            platformSupportsBitness = true;
                        else if (Environment.OSVersion.Version.Minor == 2)
                        {
                            if (!string.IsNullOrEmpty(Environment.OSVersion.ServicePack))
                                platformSupportsBitness = true;
                        }
                    }
                    platformSupportsBitnessSet = true;
                }

                return platformSupportsBitness;
            }
        }
    }
}
