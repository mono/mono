//------------------------------------------------------------------------------
// <copyright file="ScriptServiceAttribute.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.Script.Services {
    using System;
    using System.Web;

    [
    AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)
    ]
    public sealed class ScriptServiceAttribute : Attribute {
        public ScriptServiceAttribute() {
        }
    }
}
