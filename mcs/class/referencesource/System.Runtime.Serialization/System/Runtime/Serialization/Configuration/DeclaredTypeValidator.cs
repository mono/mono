//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Runtime.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.Runtime.Serialization;

    internal class DeclaredTypeValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type)
        {
            return (typeof(string) == type);
        }

        public override void Validate(object value)
        {
            string type = (string)value;

            if (type.StartsWith(Globals.TypeOfObject.FullName, StringComparison.Ordinal))
            {
                Type t = Type.GetType(type, false);
                if (t != null && Globals.TypeOfObject.Equals(t))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument(SR.GetString(SR.KnownTypeConfigObject));
                }
            }
        }
    }
}
