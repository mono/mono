//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;

    [AttributeUsage(AttributeTargets.Property)]
    sealed class WebEncodingValidatorAttribute : ConfigurationValidatorAttribute
    {
        public WebEncodingValidatorAttribute()
        {
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get { return new WebEncodingValidator(); }
        }
    }
}
