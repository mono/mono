//------------------------------------------------------------------------------
// <copyright file="NullRuntimeConfig.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    //
    // Return null in LKG scenarios where we cannot even get CachedPathData for machine.config.
    //
    internal class NullRuntimeConfig : RuntimeConfig {
        internal NullRuntimeConfig() : base(null, true) {}

        protected override object GetSectionObject(string sectionName) {
            return null;
        }
    }
}
