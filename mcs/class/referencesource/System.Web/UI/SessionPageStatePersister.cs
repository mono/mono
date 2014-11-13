//------------------------------------------------------------------------------
// <copyright file="SessionPageStatePersister.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI {
    using System.Collections;
    using System.Collections.Specialized;
    using System.IO;
    using System.Text;
    using System.Web.SessionState;
    using System.Web.Configuration;
    using System.Web.Security.Cryptography;

    public class SessionPageStatePersister : PageStatePersister {
        private const string _viewStateSessionKey = "__SESSIONVIEWSTATE";
        private const string _viewStateQueueKey = "__VIEWSTATEQUEUE";

        public SessionPageStatePersister(Page page) : base (page) {
            HttpSessionState session = null;
            try {
                session = page.Session;
            }
            catch {
                // ignore, throw if session is null.
            }
            if (session == null) {
                throw new ArgumentException(SR.GetString(SR.SessionPageStatePersister_SessionMustBeEnabled));
            }
        }

        public override void Load() {
            NameValueCollection requestValueCollection = Page.RequestValueCollection;
            if (requestValueCollection == null) {
                return;
            }

            try {
                string combinedSerializedStateString = Page.RequestViewStateString;
                string persistedStateID = null;
                bool controlStateInSession = false;

                // SessionState will persist a Pair of <bool requiresControlStateInSession, string/pair>,
                // where if requiresControlStateInSession is true, second will just be the sessionID, as
                // we will store both control state and view state in session.  Otherwise, we store just the
                // view state in session and the pair will be <id, ControlState>
                if (!String.IsNullOrEmpty(combinedSerializedStateString)) {
                    Pair combinedState = (Pair)Util.DeserializeWithAssert(StateFormatter2, combinedSerializedStateString, Purpose.WebForms_SessionPageStatePersister_ClientState);
                    // Check if we are storing control state in session as well
                    if ((bool)combinedState.First) {
                        // So the second is the persistedID
                        persistedStateID = (string)combinedState.Second;
                        controlStateInSession = true;
                    }
                    else {
                        // Second is <sessionID, ControlState>
                        Pair pair = (Pair)combinedState.Second;
                        persistedStateID = (string)pair.First;
                        ControlState = pair.Second;
                    }
                }

                if (persistedStateID != null) {
                    object sessionData = Page.Session[_viewStateSessionKey + persistedStateID];
                    if (controlStateInSession) {
                        Pair combinedState = sessionData as Pair;
                        if (combinedState != null) {
                            ViewState = combinedState.First;
                            ControlState = combinedState.Second;
                        }
                    }
                    else {
                        ViewState = sessionData;
                    }
                }
            }
            catch (Exception e) {
                // Setup the formatter for this exception, to make sure this message shows up
                // in an error page as opposed to the inner-most exception's message.
                HttpException newException = new HttpException(SR.GetString(SR.Invalid_ControlState), e);
                newException.SetFormatter(new UseLastUnhandledErrorFormatter(newException));

                throw newException;
            }
        }


        /// <devdoc>
        ///     To be supplied.
        /// </devdoc>
        public override void Save() {
            bool requiresControlStateInSession = false;
            object clientData = null;

            Triplet vsTrip = ViewState as Triplet;
            // no session view state to store.
            if ((ControlState != null) ||
                ((vsTrip == null || vsTrip.Second != null || vsTrip.Third != null) && ViewState != null)) {
                HttpSessionState session = Page.Session;

                string sessionViewStateID = Convert.ToString(DateTime.Now.Ticks, 16);

                object state = null;
                requiresControlStateInSession = Page.Request.Browser.RequiresControlStateInSession;
                if (requiresControlStateInSession) {
                    // ClientState will just be sessionID
                    state = new Pair(ViewState, ControlState);
                    clientData = sessionViewStateID;
                }
                else {
                    // ClientState will be a <sessionID, ControlState>
                    state = ViewState;
                    clientData = new Pair(sessionViewStateID, ControlState);
                }

                string sessionKey = _viewStateSessionKey + sessionViewStateID;
                session[sessionKey] = state;

                Queue queue = session[_viewStateQueueKey] as Queue;

                if (queue == null) {
                    queue = new Queue();
                    session[_viewStateQueueKey] = queue;
                }
                queue.Enqueue(sessionKey);

                SessionPageStateSection cfg = RuntimeConfig.GetConfig(Page.Request.Context).SessionPageState;
                int queueCount = queue.Count;

                if (cfg != null && queueCount > cfg.HistorySize ||
                     cfg == null && queueCount > SessionPageStateSection.DefaultHistorySize) {
                    string oldSessionKey = (string)queue.Dequeue();
                    session.Remove(oldSessionKey);
                }
            }

            if (clientData != null) {
                Page.ClientState = Util.SerializeWithAssert(StateFormatter2, new Pair(requiresControlStateInSession, clientData), Purpose.WebForms_SessionPageStatePersister_ClientState);
            }
        }
    }
}
