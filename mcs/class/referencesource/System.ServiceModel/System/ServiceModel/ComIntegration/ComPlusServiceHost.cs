//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
#pragma warning disable 1634, 1691

namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.ServiceModel.Dispatcher;
    using System.ServiceModel.Description;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime.Diagnostics;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.Diagnostics;
    using System.ServiceModel.Diagnostics;
    

    abstract class ComPlusServiceHost : ServiceHostBase
    {
        ServiceInfo info;

        protected void Initialize (Guid clsid,
                                   ServiceElement service,
                                   ComCatalogObject applicationObject,
                                   ComCatalogObject classObject,
                                   HostingMode hostingMode)
        {
            VerifyFunctionality();         
 
            this.info = new ServiceInfo(clsid,
                                        service,
                                        applicationObject,
                                        classObject,
                                        hostingMode);
            base.InitializeDescription(new UriSchemeKeyedCollection());
        }

        protected override void ApplyConfiguration()
        {
            
        }

        protected override ServiceDescription CreateDescription(out IDictionary<string, ContractDescription> implementedContracts)
        {
            try
            {                
                ComPlusServiceLoader loader = new ComPlusServiceLoader(this.info);
                ServiceDescription description = loader.Load(this);
                implementedContracts = null;
                return description;
            }
            catch (Exception e)
            {
                DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                            (ushort)System.Runtime.Diagnostics.EventLogCategory.ComPlus,
                            (uint)System.Runtime.Diagnostics.EventLogEventId.ComPlusServiceHostStartingServiceError,
                            this.info.AppID.ToString(),
                            this.info.Clsid.ToString(),
                            e.ToString());
                        throw;
            }
        }

        protected override void InitializeRuntime()
        {
            ComPlusServiceHostTrace.Trace(TraceEventType.Information, TraceCode.ComIntegrationServiceHostStartingService,
                                SR.TraceCodeComIntegrationServiceHostStartingService, this.info);
            try
            {
                DispatcherBuilder dispatcherBuilder = new DispatcherBuilder();
                dispatcherBuilder.InitializeServiceHost(this.Description, this); 
            }
            catch (Exception e)
            {
                if (System.ServiceModel.DiagnosticUtility.ShouldTraceError)
                {
                    DiagnosticUtility.EventLog.LogEvent(TraceEventType.Error,
                        (ushort)System.Runtime.Diagnostics.EventLogCategory.ComPlus,
                        (uint)System.Runtime.Diagnostics.EventLogEventId.ComPlusServiceHostStartingServiceError,
                        this.info.AppID.ToString(),
                        this.info.Clsid.ToString(),
                        e.ToString());
                }

                throw;
            }
            ComPlusServiceHostTrace.Trace(TraceEventType.Verbose, TraceCode.ComIntegrationServiceHostStartedServiceDetails,
                                SR.TraceCodeComIntegrationServiceHostStartedServiceDetails, this.info, this.Description);
            ComPlusServiceHostTrace.Trace(TraceEventType.Information, TraceCode.ComIntegrationServiceHostStartedService,
                                SR.TraceCodeComIntegrationServiceHostStartedService, this.info);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            ComPlusServiceHostTrace.Trace(TraceEventType.Information, TraceCode.ComIntegrationServiceHostStoppingService,
                                SR.TraceCodeComIntegrationServiceHostStoppingService, this.info);
            base.OnClose(timeout);
            ComPlusServiceHostTrace.Trace(TraceEventType.Information, TraceCode.ComIntegrationServiceHostStoppedService,
                                SR.TraceCodeComIntegrationServiceHostStoppedService, this.info);
        }
        protected void VerifyFunctionality()
        {
            object serviceConfig = new CServiceConfig();
            IServiceSysTxnConfig sysTxnconfing = serviceConfig as IServiceSysTxnConfig;
            if (sysTxnconfing == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.QFENotPresent());
            }
        }
    }
}
