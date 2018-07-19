//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Activities.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Diagnostics.Application;

    internal class ServiceModelActivitiesEnumValidator : ConfigurationValidatorBase
    {
        Type enumHelperType;
        MethodInfo isDefined;

        public ServiceModelActivitiesEnumValidator(Type enumHelperType)
        {
            this.enumHelperType = enumHelperType;
            this.isDefined = this.enumHelperType.GetMethod("IsDefined", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
        }

        public override bool CanValidate(Type type)
        {
            return (this.isDefined != null);
        }

        public override void Validate(object value)
        {
            bool retVal = (bool)this.isDefined.Invoke(null, new object[] { value });
            
            if (!retVal)
            {
                ParameterInfo[] isDefinedParameters = this.isDefined.GetParameters();
                throw FxTrace.Exception.AsError(new InvalidEnumArgumentException("value", (int)value, isDefinedParameters[0].ParameterType));
            }
        }
    }
}
