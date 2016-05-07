// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    class HttpMessageHandlerFactoryValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type)
        {
            return type == typeof(HttpMessageHandlerFactoryElement);
        }

        public override void Validate(object value)
        {
            HttpMessageHandlerFactoryElement configElement = (HttpMessageHandlerFactoryElement)value;
            if (!string.IsNullOrWhiteSpace(configElement.Type) && configElement.Handlers != null && configElement.Handlers.Count > 0)
            {
                throw FxTrace.Exception.AsError(new ConfigurationErrorsException(SR.GetString(SR.HttpMessageHandlerFactoryConfigInvalid_WithBothTypeAndHandlerList, ConfigurationStrings.MessageHandlerFactory, ConfigurationStrings.Type, ConfigurationStrings.Handlers)));
            }
        }
    }
}
