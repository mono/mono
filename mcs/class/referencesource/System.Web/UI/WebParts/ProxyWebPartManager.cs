//------------------------------------------------------------------------------
// <copyright file="ProxyWebPartManager.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Web.UI;

    [
    Bindable(false),
    Designer("System.Web.UI.Design.WebControls.WebParts.ProxyWebPartManagerDesigner, " + AssemblyRef.SystemDesign),
    NonVisualControl(),
    ParseChildren(true),
    PersistChildren(false)
    ]
    public class ProxyWebPartManager : Control {

        private ProxyWebPartConnectionCollection _staticConnections;

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override string ClientID {
            get {
                return base.ClientID;
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

        [
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        EditorBrowsable(EditorBrowsableState.Never),
        MergableProperty(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebCategory("Behavior"),
        WebSysDescription(SR.WebPartManager_StaticConnections),
        ]
        public ProxyWebPartConnectionCollection StaticConnections {
            get {
                if (_staticConnections == null) {
                    _staticConnections = new ProxyWebPartConnectionCollection();
                }
                return _staticConnections;
            }
        }

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

        protected override ControlCollection CreateControlCollection() {
            return new EmptyControlCollection(this);
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override void Focus() {
            throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
        }

        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            Page page = Page;
            Debug.Assert(page != null);
            if ((page != null) && !DesignMode) {
                WebPartManager webPartManager = WebPartManager.GetCurrentWebPartManager(page);
                if (webPartManager == null) {
                    throw new InvalidOperationException(SR.GetString(SR.WebPartManagerRequired));
                }
                StaticConnections.SetWebPartManager(webPartManager);
            }
        }

    }
}

