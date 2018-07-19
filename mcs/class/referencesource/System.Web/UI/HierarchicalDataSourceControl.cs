//------------------------------------------------------------------------------
// <copyright file="HierarchicalDataSourceControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------
namespace System.Web.UI {

    using System.ComponentModel;
    using System.Security.Permissions;
    

    [
    Bindable(false),
    ControlBuilder(typeof(DataSourceControlBuilder)),
    Designer("System.Web.UI.Design.HierarchicalDataSourceDesigner, " + AssemblyRef.SystemDesign),
    NonVisualControl()
    ]
    public abstract class HierarchicalDataSourceControl : Control, IHierarchicalDataSource {

        private static readonly object EventDataSourceChanged = new object();

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
        EditorBrowsable(EditorBrowsableState.Never)
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

        protected abstract HierarchicalDataSourceView GetHierarchicalView(string viewPath);

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override bool HasControls() {
            return base.HasControls();
        }

        protected virtual void OnDataSourceChanged(EventArgs e) {
            EventHandler onDataSourceChangedHandler = (EventHandler)Events[EventDataSourceChanged];
            if (onDataSourceChangedHandler != null) 
                onDataSourceChangedHandler(this, e);
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override void RenderControl(HtmlTextWriter writer) {
            base.RenderControl(writer);
        }

        #region Implementation of IHierarchicalDataSource
        /// <summary>
        ///   Raised when the underlying data source has changed. The
        ///   change may be due to a change in the control's properties,
        ///   or a change in the data due to an edit action performed by
        ///   the DataSourceControl.
        /// </summary>
        event EventHandler IHierarchicalDataSource.DataSourceChanged {
            add {
                Events.AddHandler(EventDataSourceChanged, value);
            }
            remove {
                Events.RemoveHandler(EventDataSourceChanged, value);
            }
        }



        /// <internalonly/>
        HierarchicalDataSourceView IHierarchicalDataSource.GetHierarchicalView(string viewPath) {
            return GetHierarchicalView(viewPath);
        }
        #endregion
    }
}
