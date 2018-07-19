//------------------------------------------------------------------------------
// <copyright file="TimeSpanMinutesConverter.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using System.Xml;
using System.Collections.Specialized;
using System.Globalization;
using System.ComponentModel;
using System.Security;
using System.Text;
using System.Configuration;

namespace System.Web.Configuration {

    [System.Security.Permissions.HostProtection(MayLeakOnAbort = true)]
    public sealed class MachineKeyValidationConverter : ConfigurationConverterBase {

        public override object ConvertTo(ITypeDescriptorContext ctx, CultureInfo ci, object value, Type type) {
            if (!(value is MachineKeyValidation)) {
                throw new ArgumentException(SR.GetString(SR.Config_Invalid_enum_value, "SHA1, MD5, 3DES, AES, HMACSHA256, HMACSHA384, HMACSHA512"));
            }
            return ConvertFromEnum((MachineKeyValidation)value);
        }

        public override object ConvertFrom(ITypeDescriptorContext ctx, CultureInfo ci, object data)
        {
            return ConvertToEnum((string)data);
        }

        internal static string ConvertFromEnum(MachineKeyValidation enumValue)
        {
            switch (enumValue) {
                case MachineKeyValidation.SHA1:
                    return "SHA1";
                case MachineKeyValidation.MD5:
                    return "MD5";
                case MachineKeyValidation.TripleDES:
                    return "3DES";
                case MachineKeyValidation.AES:
                    return "AES";
                case MachineKeyValidation.HMACSHA256:
                    return "HMACSHA256";
                case MachineKeyValidation.HMACSHA384:
                    return "HMACSHA384";
                case MachineKeyValidation.HMACSHA512:
                    return "HMACSHA512";
                default:
                    throw new ArgumentException(SR.GetString(SR.Wrong_validation_enum));
            }
        }

        internal static MachineKeyValidation ConvertToEnum(string strValue)
        {
            if (strValue==null)
                return MachineKeySection.DefaultValidation;

            switch (strValue)
            {
                case "SHA1":
                    return MachineKeyValidation.SHA1;
                case "MD5":
                    return MachineKeyValidation.MD5;
                case "3DES":
                    return MachineKeyValidation.TripleDES;
                case "AES":
                    return MachineKeyValidation.AES;
                case "HMACSHA256":
                    return MachineKeyValidation.HMACSHA256;
                case "HMACSHA384":
                    return MachineKeyValidation.HMACSHA384;
                case "HMACSHA512":
                    return MachineKeyValidation.HMACSHA512;
                default:
                    if (strValue.StartsWith("alg:", StringComparison.Ordinal))
                        return MachineKeyValidation.Custom;
                    throw new ArgumentException(SR.GetString(SR.Wrong_validation_enum));
            }
        }
    }
}
