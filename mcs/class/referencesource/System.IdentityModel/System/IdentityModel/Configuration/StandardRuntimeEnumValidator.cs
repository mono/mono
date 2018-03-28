//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.IdentityModel.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    internal class StandardRuntimeEnumValidator : ConfigurationValidatorBase
    {
        Type enumType;

        public StandardRuntimeEnumValidator(Type enumType)
        {
            this.enumType = enumType;
        }

        public override bool CanValidate(Type type)
        {
            return (type.IsEnum);
        }

        public override void Validate(object value)
        {
            if (!Enum.IsDefined(enumType, value))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("value", (int)value, enumType));
            }
        }
    }
}
