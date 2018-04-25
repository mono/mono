//------------------------------------------------------------------------------
// <copyright file="ScriptIgnoreAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Serialization {
    using System;
    using System.Web;

    [
    AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple=false, Inherited=true)
    ]
    public sealed class ScriptIgnoreAttribute : Attribute {
        public ScriptIgnoreAttribute() {
        }

        public bool ApplyToOverrides {
            get;
            set;
        }
    }
}
