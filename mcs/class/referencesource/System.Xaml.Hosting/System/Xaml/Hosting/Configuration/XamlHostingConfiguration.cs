//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Xaml.Hosting.Configuration
{
    using System;
    using System.Configuration;
    using System.Web.Configuration;
    using System.Runtime;
    using System.Security;

    static class XamlHostingConfiguration
    {
        internal const string CollectionName = "";
        internal const string HttpHandlerType = "httpHandlerType";
        internal const string XamlHostingConfigGroup = @"system.xaml.hosting";
        internal const string XamlHostingSection = XamlHostingConfigGroup + "/httpHandlers";
        internal const string XamlRootElementType = "xamlRootElementType";

        internal static bool TryGetHttpHandlerType(string virtualPath, Type hostedXamlType, out Type httpHandlerType)
        {
            XamlHostingSection section = LoadXamlHostingSection(virtualPath);
            if (null == section) 
            { 
                ConfigurationErrorsException configException = new ConfigurationErrorsException(SR.ConfigSectionNotFound);                 
                throw FxTrace.Exception.AsError(configException); 
            }
            return section.Handlers.TryGetHttpHandlerType(hostedXamlType, out httpHandlerType);
        }

        static XamlHostingSection LoadXamlHostingSection(string virtualPath)
        {
            //WebConfigurationManager returns the same section object for a given virtual directory (not virtual path).
            return (XamlHostingSection)WebConfigurationManager.GetSection(XamlHostingConfiguration.XamlHostingSection, virtualPath);
        }
    }
}

