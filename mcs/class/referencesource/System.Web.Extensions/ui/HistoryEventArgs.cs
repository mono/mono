namespace System.Web.UI {
    using System;
    using System.Collections.Specialized;

    public class HistoryEventArgs : EventArgs {
        private NameValueCollection _state;

        public HistoryEventArgs(NameValueCollection state) {
            _state = state;
        }

        public NameValueCollection State {
            get { return _state; }
        }
    }
}
