//------------------------------------------------------------------------------
// <copyright file="ConfigurationValidatorBase.cs" company="Microsoft">
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

    public abstract class ConfigurationValidatorBase {
        public virtual bool CanValidate(Type type) {
            return false;
        }
        public abstract void Validate(object value);
    }
}
