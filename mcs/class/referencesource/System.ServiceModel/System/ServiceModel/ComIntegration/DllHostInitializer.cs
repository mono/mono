//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.EnterpriseServices;
    using System.Runtime.InteropServices;
    using System.ServiceModel.Configuration;
    using System.Diagnostics;
    using System.Runtime;
    using System.Runtime.Diagnostics;
    using System.Threading;
    using System.ServiceModel.Diagnostics;

    class DllHostInitializeWorker
    {

        List<ComPlusServiceHost> hosts = new List<ComPlusServiceHost>();
        Guid applicationId;

        // This thread pings rpcss and the host process so that
        // it does not assume that we are stuck and kills itself.
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods)] // no identified security vulnerability that would justify making a breaking change
        public static void PingProc(object o)
        {
            IProcessInitControl control = o as IProcessInitControl;

            try
            {
                // This will loop for a max of 2000 seconds, which is a sanity check  
                // that should never be hit. The assumption behind that is that
                // the main thread will not get stuck. It will either make progress
                // or fail with an exception, which will abort this thread and kill the process.
                // No COM app should take longer than 30 minutes to initialize since an app that
                // takes that long would have to be so big that it hits other limits before it hits this.
                for (int i = 0; i < 200; i++)
                {
                    Thread.Sleep(10000);

                    // Add 30 more seconds to the timeout
                    control.ResetInitializerTimeout(30);
                }
            }
            catch (ThreadAbortException)
            {
            }
        }

        // We call ContextUtil.ApplicationId, from a non-APTCA assembly. There is no identified security vulnerability with that property, so we can't justify
        // adding a demand for full trust here, causing a breaking change to the public DllHostInitializer.Startup(..) method that calls this one.
        // ContextUtil.ApplicationId calls a native function (GetObjectContext from mtxex.dll), but it doesn't pass user input to it, and it doesn't 
        // cache its result (so there is no leak as a side-effect).
        [SuppressMessage(FxCop.Category.Security, FxCop.Rule.AptcaMethodsShouldOnlyCallAptcaMethods)]
        public void Startup(IProcessInitControl control)
        {
            // Find our application object, and associated components
            // (classes) collection from the COM+ catalog.
            //
            applicationId = ContextUtil.ApplicationId;

            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Information, TraceCode.ComIntegrationDllHostInitializerStarting,
                            SR.TraceCodeComIntegrationDllHostInitializerStarting, applicationId);

            Thread pingThread = null;

            try
            {
                pingThread = new Thread(PingProc);
                pingThread.Start(control);

                ComCatalogObject application;
                application = CatalogUtil.FindApplication(applicationId);
                if (application == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(
                        SR.GetString(SR.ApplicationNotFound,
                                     applicationId.ToString("B").ToUpperInvariant())));
                }

                bool processPooled = ((int)application.GetValue("ConcurrentApps")) > 1;
                if (processPooled)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(
                        SR.GetString(SR.PooledApplicationNotSupportedForComplusHostedScenarios,
                                     applicationId.ToString("B").ToUpperInvariant())));
                }

                bool processRecycled = ((int)application.GetValue("RecycleLifetimeLimit")) > 0 ||
                                       ((int)application.GetValue("RecycleCallLimit")) > 0 ||
                                       ((int)application.GetValue("RecycleActivationLimit")) > 0 ||
                                       ((int)application.GetValue("RecycleMemoryLimit")) > 0;

                if (processRecycled)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.ListenerInitFailed(
                        SR.GetString(SR.RecycledApplicationNotSupportedForComplusHostedScenarios,
                                     applicationId.ToString("B").ToUpperInvariant())));
                }


                ComCatalogCollection classes;
                classes = application.GetCollection("Components");

                // Load up Indigo configuration.
                //
                ServicesSection services = ServicesSection.GetSection();
                bool foundService = false;


                foreach (ServiceElement service in services.Services)
                {
                    Guid clsidToCompare = Guid.Empty;
                    Guid appIdToCompare = Guid.Empty;

                    string[] serviceParams = service.Name.Split(',');
                    if (serviceParams.Length != 2)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.OnlyClsidsAllowedForServiceType, service.Name)));
                    }

                    if (!DiagnosticUtility.Utility.TryCreateGuid(serviceParams[0], out appIdToCompare))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.OnlyClsidsAllowedForServiceType, service.Name)));
                    }

                    if (!DiagnosticUtility.Utility.TryCreateGuid(serviceParams[1], out clsidToCompare))
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString(SR.OnlyClsidsAllowedForServiceType, service.Name)));
                    }

                    foundService = false;

                    // CODEWORK: Consider farming this out across multiple threadpool threads.
                    // When it was discovered that startup time could be a problem it was too late
                    // to to do that since it can cause failure conditions that need to be considered
                    // (such as the threadpool running out) so we decided not to touch that part.
                    // But since this can potentially take a very long time on big COM apps
                    // it should be parallelized at some point.
                    foreach (ComCatalogObject classObject in classes)
                    {
                        Guid clsid = Fx.CreateGuid((string)classObject.GetValue("CLSID"));

                        if (clsid == clsidToCompare && applicationId == appIdToCompare)
                        {
                            foundService = true;
                            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationDllHostInitializerAddingHost,
                                SR.TraceCodeComIntegrationDllHostInitializerAddingHost, applicationId, clsid, service);
                            this.hosts.Add(
                                new DllHostedComPlusServiceHost(clsid,
                                                                service,
                                                                application,
                                                                classObject));
                        }
                    }
                    if (!foundService)
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(SR.GetString
                        (SR.CannotFindClsidInApplication, clsidToCompare.ToString("B").ToUpperInvariant(), applicationId.ToString("B").ToUpperInvariant())));
                }
                if (foundService == false)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(Error.DllHostInitializerFoundNoServices());
                }

                foreach (ComPlusServiceHost host in this.hosts)
                {
                    host.Open();
                }

            }
            catch (Exception e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                    (ushort)System.Runtime.Diagnostics.EventLogCategory.ComPlus,
                    (uint)System.Runtime.Diagnostics.EventLogEventId.ComPlusDllHostInitializerStartingError,
                    applicationId.ToString(),
                    e.ToString());
                throw;
            }
            finally
            {
                if (null != pingThread)
                    pingThread.Abort(); // We are done; stop pinging.

            }
            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Information, TraceCode.ComIntegrationDllHostInitializerStarted,
                            SR.TraceCodeComIntegrationDllHostInitializerStarted, applicationId);
        }

        public void Shutdown()
        {
            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Information, TraceCode.ComIntegrationDllHostInitializerStopping,
                            SR.TraceCodeComIntegrationDllHostInitializerStopping, applicationId);
            foreach (ComPlusServiceHost host in this.hosts)
            {
                host.Close();
            }
            ComPlusDllHostInitializerTrace.Trace(TraceEventType.Information, TraceCode.ComIntegrationDllHostInitializerStopped,
                            SR.TraceCodeComIntegrationDllHostInitializerStopped, applicationId);
        }
    }

    [ComVisible(true)]
    [Guid("59856830-3ECB-4D29-9CFE-DDD0F74B96A2")]
    public class DllHostInitializer : IProcessInitializer
    {
        DllHostInitializeWorker worker = new DllHostInitializeWorker();
        public void Startup(object punkProcessControl)
        {
            IProcessInitControl control = punkProcessControl as IProcessInitControl;
            worker.Startup(control);
        }
        public void Shutdown()
        {
            worker.Shutdown();
        }

    }
}
