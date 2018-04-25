//------------------------------------------------------------------------------
// <copyright file="DataPagerField.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls {
    public abstract class DataPagerField : IStateManager {
        private StateBag _stateBag;
        private bool _trackViewState;
        private DataPager _dataPager;

        internal event EventHandler FieldChanged;

        protected DataPagerField() {
            _stateBag = new StateBag();
        }

        protected StateBag ViewState {
            get {
                return _stateBag;
            }
        }

        protected bool IsTrackingViewState {
            get {
                return _trackViewState;
            }
        }

        protected DataPager DataPager {
            get {
                return _dataPager;
            }
        }

        protected bool QueryStringHandled {
            get {
                return DataPager.QueryStringHandled;
            }
            set {
                DataPager.QueryStringHandled = value;
            }
        }

        protected string QueryStringValue {
            get {
                return DataPager.QueryStringValue;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(true),
        ResourceDescription("DataPagerField_Visible")
        ]
        public bool Visible {
            get {
                object o = ViewState["Visible"];
                if (o != null) {
                    return (bool)o;
                }
                return true;
            }
            set {
                if (value != Visible) {
                    ViewState["Visible"] = value;
                    OnFieldChanged();
                }
            }
        }

        protected internal DataPagerField CloneField() {
            DataPagerField newField = CreateField();
            CopyProperties(newField);
            return newField;
        }

        protected virtual void CopyProperties(DataPagerField newField) {
            newField.Visible = Visible;
        }

        public abstract void CreateDataPagers(DataPagerFieldItem container, int startRowIndex, int maximumRows, int totalRowCount, int fieldIndex);
        
        protected abstract DataPagerField CreateField();

        [SuppressMessage("Microsoft.Design", "CA1055:UriReturnValuesShouldNotBeStrings",
            Justification="Return value matches HyperLink.NavigateUrl property type.")]
        protected string GetQueryStringNavigateUrl(int pageNumber) {
            return DataPager.GetQueryStringNavigateUrl(pageNumber);
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers", MessageId = "0#")]
        public abstract void HandleEvent(CommandEventArgs e);

        protected virtual void LoadViewState(object savedState) {
            if (savedState != null) {
                object[] myState = (object[])savedState;

                if (myState[0] != null)
                    ((IStateManager)ViewState).LoadViewState(myState[0]);
            }
        }

        protected virtual void OnFieldChanged() {
            if (FieldChanged != null) {
                FieldChanged(this, EventArgs.Empty);
            }
        }


        protected virtual object SaveViewState() {
            object state = ((IStateManager)ViewState).SaveViewState();

            if ((state != null)) {
                return new object[1] {
                    state
                };
            }

            return null;
        }

        internal void SetDirty() {
            _stateBag.SetDirty(true);
        }

        internal void SetDataPager(DataPager dataPager) {
            _dataPager = dataPager;
        }

        protected virtual void TrackViewState() {
            _trackViewState = true;
            ((IStateManager)ViewState).TrackViewState();
        }

        #region IStateManager
        /// <internalonly/>
        /// <devdoc>
        /// Return true if tracking state changes.
        /// </devdoc>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        /// <internalonly/>
        /// <devdoc>
        /// Load previously saved state.
        /// </devdoc>
        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        /// <internalonly/>
        /// <devdoc>
        /// Start tracking state changes.
        /// </devdoc>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        /// <internalonly/>
        /// <devdoc>
        /// Return object containing state changes.
        /// </devdoc>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }
        #endregion IStateManager
    }
}
