//------------------------------------------------------------------------------
// <copyright file="HiddenFieldPersister.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Security.Permissions;
    using System.Web.Security.Cryptography;

    public class HiddenFieldPageStatePersister : PageStatePersister {
        public HiddenFieldPageStatePersister(Page page) : base (page) {
        }

        public override void Load() {
            NameValueCollection requestValueCollection = Page.RequestValueCollection;
            if (requestValueCollection == null) {
                return;
            }

            string viewStateString = null;
            try {
                viewStateString = Page.RequestViewStateString;

                // VSWhidbey 160556
                if (!String.IsNullOrEmpty(viewStateString) || !String.IsNullOrEmpty(Page.ViewStateUserKey)) {
                    Pair combinedState = (Pair)Util.DeserializeWithAssert(StateFormatter2, viewStateString, Purpose.WebForms_HiddenFieldPageStatePersister_ClientState);
                    ViewState = combinedState.First;
                    ControlState = combinedState.Second;
                }
            }
            catch (Exception e) {
                // throw if this is a wrapped ViewStateException -- mac validation failed
                if (e.InnerException is ViewStateException) {
                    throw;
                }

                ViewStateException.ThrowViewStateError(e, viewStateString);
            }
        }


        /// <devdoc>
        ///     To be supplied.
        /// </devdoc>
        public override void Save() {
            if (ViewState != null || ControlState != null) {
                Page.ClientState = Util.SerializeWithAssert(StateFormatter2, new Pair(ViewState, ControlState), Purpose.WebForms_HiddenFieldPageStatePersister_ClientState);
            }
        }
    }
}
