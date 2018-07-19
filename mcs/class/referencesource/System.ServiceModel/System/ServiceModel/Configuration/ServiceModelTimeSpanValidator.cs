//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime;

    // deals with 
    [AttributeUsage(AttributeTargets.Property)]
    sealed class ServiceModelTimeSpanValidatorAttribute : ConfigurationValidatorAttribute
    {
        TimeSpanValidatorAttribute innerValidatorAttribute;

        public ServiceModelTimeSpanValidatorAttribute()
        {
            this.innerValidatorAttribute = new TimeSpanValidatorAttribute();
            this.innerValidatorAttribute.MaxValueString = TimeoutHelper.MaxWait.ToString();
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get
            {
                return new TimeSpanOrInfiniteValidator(MinValue, MaxValue);
            }
        }

        public TimeSpan MinValue
        {
            get
            {
                return this.innerValidatorAttribute.MinValue;
            }
        }

        public string MinValueString
        {
            get
            {
                return this.innerValidatorAttribute.MinValueString;
            }
            set
            {
                this.innerValidatorAttribute.MinValueString = value;
            }
        }

        public TimeSpan MaxValue
        {
            get
            {
                return this.innerValidatorAttribute.MaxValue;
            }
        }

        public string MaxValueString
        {
            get
            {
                return this.innerValidatorAttribute.MaxValueString;
            }
            set
            {
                this.innerValidatorAttribute.MaxValueString = value;
            }
        }
    }
}
