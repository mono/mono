//------------------------------------------------------------------------------
// <copyright file="UpdatePanelTrigger.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web;

    public abstract class UpdatePanelTrigger {
#if DEBUG
        private bool _initialized;
#endif
        private UpdatePanel _owner;

        protected UpdatePanelTrigger() {
        }

        [Browsable(false)]
        public UpdatePanel Owner {
            get {
                return _owner;
            }
        }

        protected internal abstract bool HasTriggered();

        protected internal virtual void Initialize() {
#if DEBUG
            Debug.Assert(!_initialized, "The trigger has already been initialized");
            _initialized = true;
#endif
        }

        internal void SetOwner(UpdatePanel owner) {
            _owner = owner;
        }
    }
}
