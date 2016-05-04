namespace System.Web.UI.WebControls {
    /// <summary>
    /// This class tracks the values of SelectMethod parameters of ModelDataSourceView that use a custom value provider across multiple page requests.
    /// </summary>
    internal sealed class MethodParameterValue : IStateManager {

        private MethodParametersDictionary _owner;
        private bool _tracking;
        private StateBag _viewState;
        private static readonly string s_valueViewStateKey = "ParameterValue";

        /// <devdoc>
        /// Tells the Parameter the dictionary it belongs to
        /// </devdoc>
        internal void SetOwner(MethodParametersDictionary owner) {
            _owner = owner;
        }

        /// <devdoc>
        /// Raises the ParameterChanged event.
        /// </devdoc>
        private void OnParameterChanged() {
            if (_owner != null) {
                _owner.CallOnParametersChanged();
            }
        }

        internal void UpdateValue(object newValue) {
            object oldValue = ViewState[s_valueViewStateKey];
            ViewState[s_valueViewStateKey] = newValue;

            if ((newValue == null && oldValue != null) || (newValue != null && !newValue.Equals(oldValue))) {
                OnParameterChanged();
            }
        }

        /// <devdoc>
        /// Indicates whether the MethodParameter is tracking view state.
        /// </devdoc>
        private bool IsTrackingViewState {
            get {
                return _tracking;
            }
        }

        /// <devdoc>
        /// Indicates a dictionary of state information that allows you to save and restore
        /// the state of a MethodParameter across multiple requests for the same page.
        /// </devdoc>
        private StateBag ViewState {
            get {
                if (_viewState == null) {
                    _viewState = new StateBag();
                    if (_tracking)
                        _viewState.TrackViewState();
                }

                return _viewState;
            }
        }

        /// <devdoc>
        /// Loads view state.
        /// </devdoc>
        private void LoadViewState(object savedState) {
            if (savedState != null) {
                ViewState.LoadViewState(savedState);
            }
        }

        /// <devdoc>
        /// Saves view state.
        /// </devdoc>
        private object SaveViewState() {
            return (_viewState != null) ? _viewState.SaveViewState() : null;
        }

        /// <devdoc>
        /// Tells the MethodParameter to start tracking property changes.
        /// </devdoc>
        private void TrackViewState() {
            _tracking = true;

            if (_viewState != null) {
                _viewState.TrackViewState();
            }
        }

        #region Implementation of IStateManager

        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        void IStateManager.LoadViewState(object savedState) {
            LoadViewState(savedState);
        }

        object IStateManager.SaveViewState() {
            return SaveViewState();
        }

        void IStateManager.TrackViewState() {
            TrackViewState();
        }
        #endregion
    }
}
