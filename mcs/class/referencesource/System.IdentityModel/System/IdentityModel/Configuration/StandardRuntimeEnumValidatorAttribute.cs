//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.IdentityModel.Configuration
{
    using System;
    using System.Configuration;

    [AttributeUsage(AttributeTargets.Property)]
    internal sealed class StandardRuntimeEnumValidatorAttribute : ConfigurationValidatorAttribute
    {
        Type enumType;

        public StandardRuntimeEnumValidatorAttribute(Type enumType)
        {
            this.EnumType = enumType;
        }

        public Type EnumType
        {
            get { return this.enumType; }
            set { this.enumType = value; }
        }

        public override ConfigurationValidatorBase ValidatorInstance
        {
            get { return new StandardRuntimeEnumValidator(enumType); }
        }
    }
}
