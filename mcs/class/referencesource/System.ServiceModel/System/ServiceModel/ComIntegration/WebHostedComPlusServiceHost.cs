//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------
namespace System.ServiceModel.ComIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;
    using System.ServiceModel;
    using System.ServiceModel.Configuration;
    using System.ServiceModel.Diagnostics;

    class WebHostedComPlusServiceHost : ComPlusServiceHost
    {
        public WebHostedComPlusServiceHost(string webhostParams, Uri[] baseAddresses)
        {
            foreach (Uri address in baseAddresses)
                this.InternalBaseAddresses.Add(address);

            // Split up the parameter string into "clsid,appid".
            //
            string[] parameters = webhostParams.Split(',');
            if (parameters.Length != 2)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.ListenerInitFailed(
                    SR.GetString(SR.ServiceStringFormatError,
                                 webhostParams)));
            }

            Guid clsid;
            Guid appId;

            if (!DiagnosticUtility.Utility.TryCreateGuid(parameters[0], out clsid))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.ListenerInitFailed(
                    SR.GetString(SR.ServiceStringFormatError,
                                 webhostParams)));
            }

            if (!DiagnosticUtility.Utility.TryCreateGuid(parameters[1], out appId))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.ListenerInitFailed(
                    SR.GetString(SR.ServiceStringFormatError,
                                 webhostParams)));
            }

            // "B" == "With dashes and curly braces"
            // (The catalog gives us GUIDs in this format)
            //
            string clsidString = clsid.ToString("B").ToUpperInvariant();

            // Look up the COM+ AdminSDK information for this
            // AppID/CLSID pair.
            //
            ComCatalogObject application;
            application = CatalogUtil.FindApplication(appId);
            if (application == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.ListenerInitFailed(
                    SR.GetString(SR.ApplicationNotFound,
                                 appId.ToString("B").ToUpperInvariant())));
            }

            ComCatalogCollection classes;
            classes = application.GetCollection("Components");

            ComCatalogObject classObject = null;
            foreach (ComCatalogObject tempClassObject in classes)
            {
                string otherClsid = (string)tempClassObject.GetValue("CLSID");
                if (clsidString.Equals(
                        otherClsid,
                        StringComparison.OrdinalIgnoreCase))
                {
                    classObject = tempClassObject;
                    break;
                }
            }

            if (classObject == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.ListenerInitFailed(
                    SR.GetString(SR.ClsidNotInApplication,
                                 clsidString,
                                 appId.ToString("B").ToUpperInvariant())));
            }

            // Load up Indigo configuration, get the configuration for
            // this service.
            //
            ServicesSection services = ServicesSection.GetSection();
            ServiceElement service = null;
            foreach (ServiceElement serviceInConfig in services.Services)
            {
                Guid clsidFromConfig = Guid.Empty;
                Guid appidFromConfig = Guid.Empty;

                string[] serviceParams = serviceInConfig.Name.Split(',');
                if (serviceParams.Length != 2)
                {
                    continue;
                }


                if (!DiagnosticUtility.Utility.TryCreateGuid(serviceParams[0], out appidFromConfig))
                {
                    // We are tolerant of having non COM+ based services 
                    // for webhost.
                    continue;
                }

                if (!DiagnosticUtility.Utility.TryCreateGuid(serviceParams[1], out clsidFromConfig))
                {
                    // We are tolerant of having non COM+ based services 
                    // for webhost.
                    continue;
                }

                if (clsidFromConfig == clsid && appidFromConfig == appId)
                {
                    service = serviceInConfig;
                    break;
                }
            }
            if (service == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(System.ServiceModel.ComIntegration.Error.ListenerInitFailed(
                    SR.GetString(SR.ClsidNotInConfiguration,
                                 clsidString)));
            }


            // Hosting mode evaluation
            //
            HostingMode hostingMode;
            int activation = (int)application.GetValue("Activation");
            if (activation == 0)
            {
                hostingMode = HostingMode.WebHostInProcess;
            }
            else
            {
                hostingMode = HostingMode.WebHostOutOfProcess;
            }

            // Now we have everything we need, do common
            // initialization.
            //
            Initialize(clsid,
                        service,
                        application,
                        classObject,
                        hostingMode);
        }
    }
}
