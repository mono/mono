//------------------------------------------------------------------------------
// <copyright file="CustomLoaderAttribute.cs" company="Microsoft">
//    Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Hosting {
    using System;

    // Used to locate a custom loader implementation within a bin-deployed assembly.

    [AttributeUsage(AttributeTargets.Assembly, AllowMultiple = false, Inherited = false)]
    public sealed class CustomLoaderAttribute : Attribute {
        public CustomLoaderAttribute(Type customLoaderType) {
            if (customLoaderType == null) {
                throw new ArgumentNullException("customLoaderType");
            }

            // CustomLoaderHelper will verify that the type implements the correct interface.
            CustomLoaderType = customLoaderType;
        }

        public Type CustomLoaderType { get; private set; }
    }
}
