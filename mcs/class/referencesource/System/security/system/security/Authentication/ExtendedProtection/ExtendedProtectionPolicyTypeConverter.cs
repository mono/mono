//------------------------------------------------------------------------------
//
// Copyright (c) Microsoft Corporation.  All rights reserved.
//
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design.Serialization;
using System.Globalization;
using System.Reflection;

namespace System.Security.Authentication.ExtendedProtection
{
    public class ExtendedProtectionPolicyTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                ExtendedProtectionPolicy policy = value as ExtendedProtectionPolicy;

                if (policy != null)
                {
                    Type[] parameterTypes;
                    object[] parameterValues;

                    if (policy.PolicyEnforcement == PolicyEnforcement.Never)
                    {
                        parameterTypes = new Type[] { typeof(PolicyEnforcement) };
                        parameterValues = new object[] { PolicyEnforcement.Never };
                    }
                    else
                    {
                        parameterTypes = new Type[] { typeof(PolicyEnforcement), typeof(ProtectionScenario), typeof(ICollection) };

                        object[] customServiceNames = null;
                        if (policy.CustomServiceNames != null && policy.CustomServiceNames.Count > 0)
                        {
                            customServiceNames = new object[policy.CustomServiceNames.Count];
                            ((ICollection)policy.CustomServiceNames).CopyTo(customServiceNames, 0);
                        }

                        parameterValues = new object[] { policy.PolicyEnforcement, policy.ProtectionScenario, customServiceNames };
                    }

                    ConstructorInfo constructor = typeof(ExtendedProtectionPolicy).GetConstructor(parameterTypes);
                    return new InstanceDescriptor(constructor, parameterValues);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
