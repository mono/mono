//------------------------------------------------------------------------------
// <copyright file="ModelDataSource.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Web.UI;


    /// <summary>
    /// The data source used by DataBoundControls like GridView/FormView to support data methods
    /// to perform the Delete, Insert, Select, and Update operations. 
    /// </summary>
    public class ModelDataSource : IDataSource, IStateManager {

        internal const string DefaultViewName = "DefaultView";
        private ModelDataSourceView _view;
        private ICollection _viewNames;
        private EventHandler DataSourceChanged;

        public ModelDataSource(Control dataControl) {
            if (dataControl == null) {
                throw new ArgumentNullException("dataControl");
            }
            DataControl = dataControl;
        }

        /// <summary>
        /// Updates the required properties for the one way data binding to work.
        /// </summary>
        public void UpdateProperties(string modelTypeName, string selectMethod) {
            UpdateProperties(modelTypeName, selectMethod, String.Empty, String.Empty, String.Empty, String.Empty);
        }

        /// <summary>
        /// Updates the required properties for the two way data binding to work.
        /// </summary>
        public void UpdateProperties(string modelTypeName, string selectMethod, string updateMethod, string insertMethod, string deleteMethod, string dataKeyName) {
            View.UpdateProperties(modelTypeName, selectMethod, updateMethod, insertMethod, deleteMethod, dataKeyName);
        }

        /// <summary>
        /// Control for which this is an internal data source to support the data methods for data operations.
        /// </summary>
        public Control DataControl {
            get;
            private set;
        }

        public event CallingDataMethodsEventHandler CallingDataMethods {
            add {
                View.CallingDataMethods += value;
            }
            remove {
                View.CallingDataMethods -= value;
            }
        }

        /// <summary>
        /// The default (and only) ModelDataSourceView for this ModelDataSource.
        /// Subclasses can override this method to return a different ModelDataSourceView.
        /// </summary>
        public virtual ModelDataSourceView View {
            get {
                if (_view == null) {
                    _view = new ModelDataSourceView(this);
                }

                return _view;
            }
        }

        protected virtual bool IsTrackingViewState() {
            return ((IStateManager)View).IsTrackingViewState;
        }

        protected virtual void LoadViewState(object savedState) {
            ((IStateManager)View).LoadViewState(savedState);
        }

        protected virtual object SaveViewState() {
            return ((IStateManager)View).SaveViewState();
        }

        protected virtual void TrackViewState() {
            ((IStateManager)View).TrackViewState();
        }

        /// <devdoc>
        /// Gets the view associated with this data source.
        /// </devdoc>
        private DataSourceView GetView(string viewName) {
            //Ignore the input viewName as there is only a single view.
            return View;
        }


        /// <devdoc>
        /// </devdoc>
        private ICollection GetViewNames() {
            if (_viewNames == null) {
                _viewNames = new string[1] { DefaultViewName };
            }
            return _viewNames;
        }
        
        #region Implementation of IDataSource
        /// <summary>
        ///   Raised when the underlying data source has changed. The
        ///   change may be due to a change in the control's properties,
        ///   or a change in the data due to an edit action performed by
        ///   the DataSourceControl.
        /// </summary>
        event EventHandler IDataSource.DataSourceChanged {
            add {
                DataSourceChanged += value;
            }
            remove {
                DataSourceChanged -= value;
            }
        }


        /// <internalonly/>
        DataSourceView IDataSource.GetView(string viewName) {
            return GetView(viewName);
        }


        /// <internalonly/>
        ICollection IDataSource.GetViewNames() {
            return GetViewNames();
        }
        #endregion

        #region Implementation of IStateManager

        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState();
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

