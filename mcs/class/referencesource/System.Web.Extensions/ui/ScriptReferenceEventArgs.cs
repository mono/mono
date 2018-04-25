//------------------------------------------------------------------------------
// <copyright file="ScriptReferenceEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Web;

    public class ScriptReferenceEventArgs : EventArgs {
        private readonly ScriptReference _script;

        public ScriptReferenceEventArgs(ScriptReference script) {
            if (script == null) {
                throw new ArgumentNullException("script");
            }
            _script = script;
        }

        public ScriptReference Script {
            get {
                return _script;
            }
        }
    }
}
