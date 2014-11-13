// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    [AttributeUsage(AttributeTargets.Property)]
    sealed class HttpMessageHandlerFactoryValidatorAttribute : ConfigurationValidatorAttribute
    {
        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return new HttpMessageHandlerFactoryValidator();
            }
        }
    }
}
