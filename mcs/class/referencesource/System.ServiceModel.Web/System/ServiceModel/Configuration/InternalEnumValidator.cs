//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Reflection;
    using System.ServiceModel.Channels;

    internal class InternalEnumValidator : ConfigurationValidatorBase
    {
        Type enumHelperType;
        MethodInfo isDefined;

        public InternalEnumValidator(Type enumHelperType)
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
            bool retVal = (bool) this.isDefined.Invoke(null, new object[] { value });

            if (!retVal)
            {
                ParameterInfo[] isDefinedParameters = this.isDefined.GetParameters();
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int) value, isDefinedParameters[0].ParameterType));
            }
        }
    }
}
