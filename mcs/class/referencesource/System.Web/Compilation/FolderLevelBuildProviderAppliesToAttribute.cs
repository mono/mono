//------------------------------------------------------------------------------
// <copyright file="FolderLevelBuildProviderAppliesToAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Compilation {

    using System.Security.Permissions;
    using System.Web.Configuration;

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class FolderLevelBuildProviderAppliesToAttribute : Attribute {

        private FolderLevelBuildProviderAppliesTo _appliesTo;

        public FolderLevelBuildProviderAppliesToAttribute(FolderLevelBuildProviderAppliesTo appliesTo) {
            _appliesTo = appliesTo;
        }

        public FolderLevelBuildProviderAppliesTo AppliesTo {
            get {
                return _appliesTo;
            }
        }
    }
}
