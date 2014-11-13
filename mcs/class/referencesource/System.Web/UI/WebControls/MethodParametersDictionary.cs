namespace System.Web.UI.WebControls {
    
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class MethodParametersDictionary : IDictionary<string, MethodParameterValue>, IStateManager {

        private bool _tracking;
        private EventHandler _parametersChangedHandler;
        private Dictionary<string, MethodParameterValue> _innerDictionary;

        /// <devdoc>
        /// Used by Parameters to raise the ParametersChanged event.
        /// </devdoc>
        internal void CallOnParametersChanged() {
            OnParametersChanged(EventArgs.Empty);
        }

        /// <devdoc>
        /// Raises the ParametersChanged event.
        /// </devdoc>
        private void OnParametersChanged(EventArgs e) {
            if (_parametersChangedHandler != null) {
                _parametersChangedHandler(this, e);
            }
        }

        /// <devdoc>
        /// Occurs when any of the Parameter Values in the dictionary change.
        /// </devdoc>
        public event EventHandler ParametersChanged {
            add {
                _parametersChangedHandler = (EventHandler)Delegate.Combine(_parametersChangedHandler, value);
            }
            remove {
                _parametersChangedHandler = (EventHandler)Delegate.Remove(_parametersChangedHandler, value);
            }
        }

        public int Count {
            get {
                return InnerDictionary.Count;
            }
        }

        private void LoadViewState(object savedState) {
            if (savedState != null) {
                Debug.Assert(savedState is Pair);
                Pair pair = (Pair)savedState;

                string[] names = (string[])pair.First;
                object[] states = (object[])pair.Second;

                for (int i = 0; i < names.Length; i++) {
                    string key = names[i];
                    Debug.Assert(!InnerDictionary.ContainsKey(key), "The collection was not empty when loading the viewstate");
                    MethodParameterValue parameter = new MethodParameterValue();
                    Add(key, parameter);
                    ((IStateManager)parameter).LoadViewState(states[i]);
                }
            }
        }

        private object SaveViewState() {
            bool hasState = false;
            int count = Count;
            string[] names = new string[count];
            object[] states = new object[count];

            int i = 0;
            foreach (KeyValuePair<string, MethodParameterValue> kvp in InnerDictionary) {
                names[i] = kvp.Key;
                states[i] = ((IStateManager)kvp.Value).SaveViewState();
                if (states[i] != null)
                    hasState = true;
                i++;
            }

            return hasState ? new Pair(names, states) : null;
        }

        private void TrackViewState() {
            _tracking = true;
            foreach (MethodParameterValue parameter in InnerDictionary.Values) {
                ((IStateManager)parameter).TrackViewState();
            }
        }

        private Dictionary<string, MethodParameterValue> InnerDictionary {
            get {
                if (_innerDictionary == null) {
                    _innerDictionary = new Dictionary<string, MethodParameterValue>();
                }
                return _innerDictionary;
            }
        }

        #region IDictionary<KeyValuePair<string,MethodParameter>> Members
        public ICollection<string> Keys {
            get {
                return InnerDictionary.Keys;
            }
        }

        public ICollection<MethodParameterValue> Values {
            get {
                return InnerDictionary.Values;
            }
        }

        public MethodParameterValue this[string key] {
            get {
                return InnerDictionary[key];
            }
            set {
                InnerDictionary[key] = value;
                if (value != null) {
                    value.SetOwner(this);
                }
            }
        }

        public void Add(string key, MethodParameterValue value) {
            InnerDictionary.Add(key, value);
            if (value != null) {
                value.SetOwner(this);
                if (_tracking) {
                    ((IStateManager)value).TrackViewState();
                }
            }
        }

        public bool ContainsKey(string key) {
            return InnerDictionary.ContainsKey(key);
        }

        public bool Remove(string key) {
            if (InnerDictionary.ContainsKey(key)) {
                MethodParameterValue value = InnerDictionary[key];
                if (value != null) {
                    value.SetOwner(null);
                }
            }
            return InnerDictionary.Remove(key);
        }

        public bool TryGetValue(string key, out MethodParameterValue value) {
            return InnerDictionary.TryGetValue(key, out value);
        }
        #endregion

        #region ICollection<KeyValuePair<string,MethodParameter>> Members
        void ICollection<KeyValuePair<string, MethodParameterValue>>.Clear() {
            InnerDictionary.Clear();
        }

        bool ICollection<KeyValuePair<string, MethodParameterValue>>.IsReadOnly {
            get {
                return ((ICollection<KeyValuePair<string, MethodParameterValue>>)InnerDictionary).IsReadOnly;
            }
        }

        void ICollection<KeyValuePair<string, MethodParameterValue>>.Add(KeyValuePair<string, MethodParameterValue> item) {
            Add(item.Key, item.Value);
        }

        bool ICollection<KeyValuePair<string, MethodParameterValue>>.Contains(KeyValuePair<string, MethodParameterValue> item) {
            return ((ICollection<KeyValuePair<string, MethodParameterValue>>)InnerDictionary).Contains(item);
        }

        void ICollection<KeyValuePair<string, MethodParameterValue>>.CopyTo(KeyValuePair<string, MethodParameterValue>[] array, int arrayIndex) {
            ((ICollection<KeyValuePair<string, MethodParameterValue>>)InnerDictionary).CopyTo(array, arrayIndex);
        }

        bool ICollection<KeyValuePair<string, MethodParameterValue>>.Remove(KeyValuePair<string, MethodParameterValue> item) {
            return ((ICollection<KeyValuePair<string, MethodParameterValue>>)InnerDictionary).Remove(item);
        }
        #endregion

        #region IEnumerable<KeyValuePair<string,MethodParameter>> Members
        public IEnumerator<KeyValuePair<string, MethodParameterValue>> GetEnumerator() {
            return InnerDictionary.GetEnumerator();
        }
        #endregion

        #region IEnumerable Members
        IEnumerator IEnumerable.GetEnumerator() {
            return GetEnumerator();
        }
        #endregion

        #region Implementation of IStateManager

        bool IStateManager.IsTrackingViewState {
            get {
                return _tracking;
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
