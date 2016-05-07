//------------------------------------------------------------------------------
// <copyright file="Attributes.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Profile {

    using System;
    using System.ComponentModel;
    using System.Security.Permissions;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ProfileProviderAttribute : Attribute {
        private string _ProviderName;
        public ProfileProviderAttribute(string providerName)
        {
            _ProviderName = providerName;
        }
        public string ProviderName
        {
            get {
                return _ProviderName;
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class SettingsAllowAnonymousAttribute : Attribute
    {
        private bool _Allow;
        public SettingsAllowAnonymousAttribute(bool allow) {
            _Allow = allow;
        }
        public bool Allow {
            get {
                return _Allow;
            }
        }
        public override bool IsDefaultAttribute() {
            return !_Allow;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class CustomProviderDataAttribute : Attribute
    {
        private string _CustomProviderData;
        public CustomProviderDataAttribute(string customProviderData) {
            _CustomProviderData = customProviderData;
        }
        public string CustomProviderData {
            get {
                return _CustomProviderData;
            }
        }
        public override bool IsDefaultAttribute() {
            return string.IsNullOrEmpty(_CustomProviderData);
        }
    }
}
