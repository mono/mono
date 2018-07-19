//------------------------------------------------------------------------------
// <copyright file="DefaultValidator.cs" company="Microsoft">
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
using System.Text.RegularExpressions;

namespace System.Configuration {

    // Default configuration validator
    // Can validate everything and never complains
    public sealed class DefaultValidator : ConfigurationValidatorBase {

        public override bool CanValidate(Type type) {
            return true;
        }
        
        public override void Validate(object value) {
            // Everything is OK with this validator
        }
    }
}
