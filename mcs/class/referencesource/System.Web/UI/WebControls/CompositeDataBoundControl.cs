//------------------------------------------------------------------------------
// <copyright file="CompositeDataBoundControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;

    public abstract class CompositeDataBoundControl : DataBoundControl, INamingContainer {

        internal const string ItemCountViewStateKey = "_!ItemCount";

        private string _updateMethod;
        private string _insertMethod;
        private string _deleteMethod;

        protected override bool IsUsingModelBinders {
            get {
                return !String.IsNullOrEmpty(SelectMethod) ||
                       !String.IsNullOrEmpty(UpdateMethod) ||
                       !String.IsNullOrEmpty(DeleteMethod) ||
                       !String.IsNullOrEmpty(InsertMethod);
            }
        }

        /// <summary>
        /// The name of the method on the page which is called when this Control does an update operation.
        /// </summary>
        protected internal string UpdateMethod {
            get {
                return _updateMethod ?? String.Empty;
            }
            set {
                if (!String.Equals(_updateMethod, value, StringComparison.OrdinalIgnoreCase)) {
                    _updateMethod = value;
                    OnDataPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The name of the method on the page which is called when this Control does a delete operation.
        /// </summary>
        protected internal string DeleteMethod {
            get {
                return _deleteMethod ?? String.Empty;
            }
            set {
                if (!String.Equals(_deleteMethod, value, StringComparison.OrdinalIgnoreCase)) {
                    _deleteMethod = value;
                    OnDataPropertyChanged();
                }
            }
        }

        /// <summary>
        /// The name of the method on the page which is called when this Control does an insert operation.
        /// </summary>
        protected internal string InsertMethod {
            get {
                return _insertMethod ?? String.Empty;
            }
            set {
                if (!String.Equals(_insertMethod, value, StringComparison.OrdinalIgnoreCase)) {
                    _insertMethod = value;
                    OnDataPropertyChanged();
                }
            }
        }

        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }

        /// <summary>
        /// Overriden by DataBoundControl to determine if the control should
        /// recreate its control hierarchy based on values in view state.
        /// If the control hierarchy should be created, i.e. view state does
        /// exist, it calls CreateChildControls with a dummy (empty) data source
        /// which is usable for enumeration purposes only.
        /// </summary>
        protected internal override void CreateChildControls() {
            Controls.Clear();
            object controlCount = ViewState[ItemCountViewStateKey];

            if (controlCount == null && RequiresDataBinding) {
                EnsureDataBound();
            }

            if (controlCount != null && ((int)controlCount) != -1) {
                DummyDataSource dummyDataSource = new DummyDataSource((int)controlCount);
                CreateChildControls(dummyDataSource, false);
                ClearChildViewState();
            }
        }


        /// <summary>
        /// Performs the work of creating the control hierarchy based on a data source.
        /// When dataBinding is true, the specified data source contains real
        /// data, and the data is supposed to be pushed into the UI.
        /// When dataBinding is false, the specified data source is a dummy data
        /// source, that allows enumerating the right number of items, but the items
        /// themselves are null and do not contain data. In this case, the recreated
        /// control hierarchy reinitializes its state from view state.
        /// It enables a DataBoundControl to encapsulate the logic of creating its
        /// control hierarchy in both modes into a single code path.
        /// </summary>
        /// <param name="dataSource">
        /// The data source to be used to enumerate items.
        /// </param>
        /// <param name="dataBinding">
        /// Whether the method has been called from DataBind or not.
        /// </param>
        /// <returns>
        /// The number of items created based on the data source. Put another way, its
        /// the number of items enumerated from the data source.
        /// </returns>
        protected abstract int CreateChildControls(IEnumerable dataSource,
                                                   bool dataBinding);


        /// <summary>
        /// Overriden by DataBoundControl to use its properties to determine the real
        /// data source that the control should bind to. It then clears the existing
        /// control hierarchy, and calls createChildControls to create a new control
        /// hierarchy based on the resolved data source.
        /// The implementation resolves various data source related properties to
        /// arrive at the appropriate IEnumerable implementation to use as the real
        /// data source.
        /// When resolving data sources, the DataSourceControlID takes highest precedence.
        /// In this mode, DataMember is used to access the appropriate list from the
        /// DataControl.
        /// If DataSourceControlID is not set, the value of the DataSource property is used.
        /// In this second alternative, DataMember is used to extract the appropriate
        /// list if the control has been handed an IListSource as a data source.
        /// </summary>
        protected internal override void PerformDataBinding(IEnumerable data) {
            base.PerformDataBinding(data);

            Controls.Clear();
            ClearChildViewState();

            TrackViewState();

            int controlCount = CreateChildControls(data, true);
            ChildControlsCreated = true;
            ViewState[ItemCountViewStateKey] = controlCount;
        }
    }
}

