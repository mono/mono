//------------------------------------------------------------------------------
// <copyright file="ScriptReferenceEventArgs.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Web;

    public class CompositeScriptReferenceEventArgs : EventArgs {
        private readonly CompositeScriptReference _compositeScript;

        public CompositeScriptReferenceEventArgs(CompositeScriptReference compositeScript) {
            if (compositeScript == null) {
                throw new ArgumentNullException("compositeScript");
            }
            _compositeScript = compositeScript;
        }

        public CompositeScriptReference CompositeScript {
            get {
                return _compositeScript;
            }
        }
    }
}
