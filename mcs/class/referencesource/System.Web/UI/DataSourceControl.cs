//------------------------------------------------------------------------------
// <copyright file="DataSourceControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.Collections;
    using System.ComponentModel;
    using System.Security.Permissions;


    /// <summary>
    /// A DataSourceControl represents a data source that can be used to
    /// data-bind a DataBoundControl.
    /// DataSourceControl is an abstract base class that defines the
    /// interface between a DataBoundControl and its data source.
    /// The design of DataSourceControl enables creation of a variety of
    /// data controls with different underlying data sources such
    /// as SqlDataControl, WebServiceDataControl, XmlDataControl etc.
    /// The data source is implemented as a control even though it
    /// has no visual rendering, to allow it to be persisted
    /// declaratively, and to allow it to participate in state
    /// management should it choose to.
    /// In abstract terms a DataSourceControl has an underlying data source.
    /// This data source may contain one or more lists of data within it.
    /// Each list is associated with a name and at the bare minimum
    /// supports enumeration via the IEnumerable interface. A DataBoundControl
    /// is typically bound to a single list within the DataControl.
    /// </summary>
    [
    Bindable(false),
    ControlBuilder(typeof(DataSourceControlBuilder)),
    Designer("System.Web.UI.Design.DataSourceDesigner, " + AssemblyRef.SystemDesign),
    NonVisualControl()
    ]
    public abstract class DataSourceControl : Control, IDataSource, IListSource {

        private static readonly object EventDataSourceChanged = new object();
        private static readonly object EventDataSourceChangedInternal = new object();

        [
        Browsable(false),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override string ClientID {
            get {
                return base.ClientID;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override ClientIDMode ClientIDMode {
            get {
                return base.ClientIDMode;
            }
            set {
                throw new NotSupportedException();
            }
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override ControlCollection Controls {
            get {
                return base.Controls;
            }
        }

        [
        Browsable(false),
        DefaultValue(false),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool EnableTheming {
            get {
                return false;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.NoThemingSupport, this.GetType().Name));
            }
        }

        [
        Browsable(false),
        DefaultValue(""),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override string SkinID {
            get {
                return String.Empty;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.NoThemingSupport, this.GetType().Name));
            }
        }

        /// <summary>
        /// Gets or sets a value that indicates whether a control should be rendered on
        /// the page.
        /// </summary>
        [
        Browsable(false),
        DefaultValue(false),
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool Visible {
            get {
                return false;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.ControlNonVisual, this.GetType().Name));
            }
        }

        /// <devdoc>
        /// Raised internally by a DataSource when data-related state is
        /// changed. DataSourceViews attach to this event so that they can
        /// be notified when the parent data source changes.
        /// This event is separate from DataSourceChanged because the DataSourceView
        /// attaches to it and fires its DataSourceViewChanged event.  We want that
        /// to fire before the DataSourceChanged event.
        /// </devdoc>
        internal event EventHandler DataSourceChangedInternal {
            add {
                Events.AddHandler(EventDataSourceChangedInternal, value);
            }
            remove {
                Events.RemoveHandler(EventDataSourceChangedInternal, value);
            }
        }


        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override void ApplyStyleSheetSkin(Page page) {
            base.ApplyStyleSheetSkin(page);
        }

        /// <devdoc>
        /// Overidden to prevent child controls from being added to this control.
        /// </devdoc>
        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override Control FindControl(string id) {
            return base.FindControl(id);
        }

        /// <devdoc>
        /// </devdoc>
        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override void Focus() {
            throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
        }

        protected abstract DataSourceView GetView(string viewName);

        protected virtual ICollection GetViewNames() {
            return null;
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool HasControls() {
            return base.HasControls();
        }

        private void OnDataSourceChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventDataSourceChanged];
            if (handler != null) {
                handler(this, e);
            }
        }

        private void OnDataSourceChangedInternal(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventDataSourceChangedInternal];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected virtual void RaiseDataSourceChangedEvent(EventArgs e) {
            OnDataSourceChangedInternal(e);
            OnDataSourceChanged(e);
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override void RenderControl(HtmlTextWriter writer) {
            base.RenderControl(writer);
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
                Events.AddHandler(EventDataSourceChanged, value);
            }
            remove {
                Events.RemoveHandler(EventDataSourceChanged, value);
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


        #region Implementation of IListSource

        /// <internalonly/>
        bool IListSource.ContainsListCollection {
            get {
                if (DesignMode) {
                    return false;
                }
                return ListSourceHelper.ContainsListCollection(this);
            }
        }


        /// <internalonly/>
        IList IListSource.GetList() {
            if (DesignMode) {
                return null;
            }
            return ListSourceHelper.GetList(this);
        }
        #endregion
    }

}
