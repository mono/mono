//------------------------------------------------------------------------------
// <copyright file="ProfilePropertyNameValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web.Util;

    internal sealed class ProfilePropertyNameValidator : ConfigurationValidatorBase
    {
        public override bool CanValidate(Type type) {
            return (type == typeof(string));
        }
        public override void Validate(object value) {
            if (value == null) {
                throw new ArgumentNullException("value");
            }
            
            string s = value as string;
            if (s != null) {
                s = s.Trim();
            }
            if (string.IsNullOrEmpty(s)) {
                throw new ArgumentException(SR.GetString(SR.Profile_name_can_not_be_empty));
            }

            if (s.Contains(".")) {
                throw new ArgumentException(SR.GetString(SR.Profile_name_can_not_contain_period));
            }
        }
        internal static ProfilePropertyNameValidator SingletonInstance = new ProfilePropertyNameValidator();
    }
}
