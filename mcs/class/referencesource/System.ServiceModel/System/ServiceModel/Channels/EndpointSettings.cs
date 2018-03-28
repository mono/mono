// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;

    internal static class EndpointSettings
    {
        internal const string ValidateOptionalClientCertificates = "wcf:HttpTransport:ValidateOptionalClientCertificates";

        internal static T GetValue<T>(BindingContext context, string name, T defaultValue)
        {
            if (context == null ||
                context.BindingParameters == null ||
                context.BindingParameters.Count == 0)
            {
                return defaultValue;
            }

            return GetValue<T>(context.BindingParameters, name, defaultValue);
        }

        internal static T GetValue<T>(BindingParameterCollection bindingParameters, string name, T defaultValue)
        {
            if (bindingParameters == null ||
                bindingParameters.Count == 0)
            {
                return defaultValue;
            }

            IDictionary<string, object> endpointSettings = bindingParameters.Find<IDictionary<string, object>>();

            object setting;

            if (endpointSettings == null ||
                !endpointSettings.TryGetValue(name, out setting) ||
                !(setting is T))
            {
                return defaultValue;
            }

            return (T)setting;
        }
    }
}
