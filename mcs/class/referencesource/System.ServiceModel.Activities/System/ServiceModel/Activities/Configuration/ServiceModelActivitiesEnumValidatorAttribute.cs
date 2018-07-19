//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.Configuration;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class ServiceModelActivitiesEnumValidatorAttribute : ConfigurationValidatorAttribute
    {
        Type enumHelperType;

        public ServiceModelActivitiesEnumValidatorAttribute(Type enumHelperType)
        {
            this.EnumHelperType = enumHelperType;
        }

        public Type EnumHelperType
        {
            get { return this.enumHelperType; }
            set { this.enumHelperType = value; }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get { return new ServiceModelActivitiesEnumValidator(enumHelperType); }
        }
    }
}
