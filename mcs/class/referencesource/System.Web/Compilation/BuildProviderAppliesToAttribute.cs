//------------------------------------------------------------------------------
// <copyright file="BuildProviderAppliesToAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System.Security.Permissions;
    using System.Web.Configuration;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class BuildProviderAppliesToAttribute : Attribute {

        private BuildProviderAppliesTo _appliesTo;

        public BuildProviderAppliesToAttribute(BuildProviderAppliesTo appliesTo) {
            _appliesTo = appliesTo;
        }

        public BuildProviderAppliesTo AppliesTo {
            get {
                return _appliesTo;
            }
        }
    }
}
