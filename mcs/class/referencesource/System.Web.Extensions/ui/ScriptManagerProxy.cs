//------------------------------------------------------------------------------
// <copyright file="ScriptManagerProxy.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.Util;

    [
    DefaultProperty("Scripts"),
    Designer("System.Web.UI.Design.ScriptManagerProxyDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
    NonVisualControl(),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxBitmap(typeof(EmbeddedResourceFinder), "System.Web.Resources.ScriptManagerProxy.bmp")
    ]
    public class ScriptManagerProxy : Control, IControl {

        private IScriptManagerInternal _scriptManager;
        private CompositeScriptReference _compositeScript;
        private ScriptReferenceCollection _scripts;
        private ServiceReferenceCollection _services;

        private ProfileServiceManager _profileServiceManager;
        private AuthenticationServiceManager _authenticationServiceManager;
        private RoleServiceManager _roleServiceManager;

        private static readonly object _navigateEvent = new object();

        public ScriptManagerProxy() {
        }

        internal ScriptManagerProxy(IScriptManagerInternal scriptManager) {
            _scriptManager = scriptManager;
        }

        [
        ResourceDescription("ScriptManager_AuthenticationService"),
        Category("Behavior"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public AuthenticationServiceManager AuthenticationService {
            get {
                if(_authenticationServiceManager == null) {
                    _authenticationServiceManager = new AuthenticationServiceManager();
                }
                return _authenticationServiceManager;
            }
        }

        [
        ResourceDescription("ScriptManager_CompositeScript"),
        Category("Behavior"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public CompositeScriptReference CompositeScript {
            get {
                if (_compositeScript == null) {
                    _compositeScript = new CompositeScriptReference();
                }
                return _compositeScript;
            }
        }

        internal bool HasProfileServiceManager {
            get {
                return this._profileServiceManager != null;
            }
        }
        internal bool HasAuthenticationServiceManager {
            get {
                return this._authenticationServiceManager != null;
            }
        }

        internal bool HasRoleServiceManager {
            get {
                return this._roleServiceManager != null;
            }
        }

        internal EventHandler<HistoryEventArgs> NavigateEvent {
            get {
                return (EventHandler<HistoryEventArgs>)Events[_navigateEvent];
            }
        }

        [
        ResourceDescription("ScriptManager_ProfileService"),
        Category("Behavior"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public ProfileServiceManager ProfileService {
            get {
                if(_profileServiceManager == null) {
                    _profileServiceManager = new ProfileServiceManager();
                }
                return _profileServiceManager;
            }
        }

        [
        ResourceDescription("ScriptManager_RoleService"),
        Category("Behavior"),
        DefaultValue(null),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public RoleServiceManager RoleService {
            get {
                if(_roleServiceManager == null) {
                    _roleServiceManager = new RoleServiceManager();
                }
                return _roleServiceManager;
            }
        }

        private IScriptManagerInternal ScriptManager {
            get {
                if (_scriptManager == null) {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    _scriptManager = UI.ScriptManager.GetCurrent(Page);
                    if (_scriptManager == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.Common_ScriptManagerRequired, ID));
                    }
                }
                return _scriptManager;
            }
        }

        [
        ResourceDescription("ScriptManager_Scripts"),
        Category("Behavior"),
        DefaultValue(null),
        Editor("System.Web.UI.Design.CollectionEditorBase, " +
            AssemblyRef.SystemWebExtensionsDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public ScriptReferenceCollection Scripts {
            get {
                if (_scripts == null) {
                    _scripts = new ScriptReferenceCollection();
                }
                return _scripts;
            }
        }

        [
        ResourceDescription("ScriptManager_Services"),
        Category("Behavior"),
        DefaultValue(null),
        Editor("System.Web.UI.Design.ServiceReferenceCollectionEditor, " +
            AssemblyRef.SystemWebExtensionsDesign, typeof(UITypeEditor)),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public ServiceReferenceCollection Services {
            get {
                if (_services == null) {
                    _services = new ServiceReferenceCollection();
                }
                return _services;
            }
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool Visible {
            get {
                return base.Visible;
            }
            set {
                throw new NotImplementedException();
            }
        }

        [
        Category("Action"),
        ResourceDescription("ScriptManager_Navigate"),
        ]
        public event EventHandler<HistoryEventArgs> Navigate {
            add {
                Events.AddHandler(_navigateEvent, value);
            }
            remove {
                Events.RemoveHandler(_navigateEvent, value);
            }
        }

        internal void CollectScripts(List<ScriptReferenceBase> scripts) {
            if ((_compositeScript != null) && (_compositeScript.Scripts.Count != 0)) {
                _compositeScript.ClientUrlResolver = this;
                _compositeScript.ContainingControl = this;
                _compositeScript.IsStaticReference = true;
                scripts.Add(_compositeScript);
            }
            // PERF: Use field directly to avoid creating List if not already created
            if (_scripts != null) {
                foreach (ScriptReference scriptReference in _scripts) {
                    scriptReference.ClientUrlResolver = this;
                    scriptReference.ContainingControl = this;
                    scriptReference.IsStaticReference = true;
                    scripts.Add(scriptReference);
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            if (!DesignMode) {
                ScriptManager.RegisterProxy(this);
            }
        }

        internal void RegisterServices(ScriptManager scriptManager) {
            // PERF: Use field directly to avoid creating List if not already created
            if (_services != null) {
                foreach (ServiceReference serviceReference in _services) {
                    serviceReference.Register(this, scriptManager);
                }
            }
        }

        #region IControl Members
        HttpContextBase IControl.Context {
            get {
                return new System.Web.HttpContextWrapper(Context);
            }
        }

        bool IControl.DesignMode {
            get {
                return DesignMode;
            }
        }
        #endregion
    }
}
