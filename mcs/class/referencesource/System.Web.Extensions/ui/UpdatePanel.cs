//------------------------------------------------------------------------------
// <copyright file="UpdatePanel.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
 
namespace System.Web.UI {
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.UI;
    using System.Web.Resources;
    using System.Web.Util;

    using Debug = System.Diagnostics.Debug;

    [
    DefaultProperty("Triggers"),
    Designer("System.Web.UI.Design.UpdatePanelDesigner, " + AssemblyRef.SystemWebExtensionsDesign),
    ParseChildren(true),
    PersistChildren(false),
    ToolboxBitmap(typeof(EmbeddedResourceFinder), "System.Web.Resources.UpdatePanel.bmp")
    ]
    public class UpdatePanel : Control, IAttributeAccessor, IUpdatePanel {

        private const string UpdatePanelToken = "updatePanel";

        private new IPage _page;
        private IScriptManagerInternal _scriptManager;

        private AttributeCollection _attributes;
        private bool _childrenAsTriggers = true;
        private ITemplate _contentTemplate;
        private Control _contentTemplateContainer;
        private bool _asyncPostBackMode;
        private bool _asyncPostBackModeInitialized;
        private UpdatePanelUpdateMode _updateMode = UpdatePanelUpdateMode.Always;
        private bool _rendered;
        private bool _explicitUpdate;
        private UpdatePanelRenderMode _renderMode = UpdatePanelRenderMode.Block;
        private UpdatePanelTriggerCollection _triggers;

        // Keep an explicit check whether the panel registered with ScriptManager. Sometimes
        // OnInit is not called on the panel, so then OnUnload gets called and you get an
        // exception. This can happen if an unhandled exception happened on the page before Init
        // and the page unloads.
        private bool _panelRegistered;

        public UpdatePanel() {
        }

        internal UpdatePanel(IScriptManagerInternal scriptManager, IPage page) {
            _scriptManager = scriptManager;
            _page = page;
        }

        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        WebSysDescription(SR.WebControl_Attributes)
        ]
        public AttributeCollection Attributes {
            get {
                if (_attributes == null) {
                    StateBag bag = new StateBag(true /* ignoreCase */);
                    _attributes = new AttributeCollection(bag);
                }
                return _attributes;
            }
        }

        [
        ResourceDescription("UpdatePanel_ChildrenAsTriggers"),
        Category("Behavior"),
        DefaultValue(true),
        ]
        public bool ChildrenAsTriggers {
            get {
                return _childrenAsTriggers;
            }
            set {
                _childrenAsTriggers = value;
            }
        }

        [
        Browsable(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateInstance(TemplateInstance.Single),
        ]
        public ITemplate ContentTemplate {
            get {
                return _contentTemplate;
            }
            set {
                if (!DesignMode && _contentTemplate != null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanel_CannotSetContentTemplate, ID));
                }
                _contentTemplate = value;
                if (_contentTemplate != null) {
                    // DevDiv 79989: Instantiate the template immediately so that the controls are available as soon as possible
                    CreateContents();
                }
            }
        }

        public sealed override ControlCollection Controls {
            get {
                // We override and seal this property because we have very special semantics
                // on the behavior of this property and the type of ControlCollection we create.
                return base.Controls;
            }
        }

        [
        Browsable(false),
        ]
        public Control ContentTemplateContainer {
            get {
                if (_contentTemplateContainer == null) {
                    _contentTemplateContainer = CreateContentTemplateContainer();
                    AddContentTemplateContainer();
                }
                return _contentTemplateContainer;
            }
        }

        [
        Browsable(false),
        ]
        public bool IsInPartialRendering {
            get {
                return _asyncPostBackMode;
            }
        }

        private IPage IPage {
            get {
                if (_page != null) {
                    return _page;
                }
                else {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    return new PageWrapper(page);
                }
            }
        }

        protected internal virtual bool RequiresUpdate {
            get {
                if (_explicitUpdate || (UpdateMode == UpdatePanelUpdateMode.Always)) {
                    return true;
                }

                if ((_triggers == null) || (_triggers.Count == 0)) {
                    return false;
                }
                return _triggers.HasTriggered();
            }
        }

        [
        ResourceDescription("UpdatePanel_RenderMode"),
        Category("Layout"),
        DefaultValue(UpdatePanelRenderMode.Block),
        ]
        public UpdatePanelRenderMode RenderMode {
            get {
                return _renderMode;
            }
            set {
                if (value < UpdatePanelRenderMode.Block || value > UpdatePanelRenderMode.Inline) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _renderMode = value;
            }
        }

        internal IScriptManagerInternal ScriptManager {
            get {
                if (_scriptManager == null) {
                    Page page = Page;
                    if (page == null) {
                        throw new InvalidOperationException(AtlasWeb.Common_PageCannotBeNull);
                    }
                    _scriptManager = UI.ScriptManager.GetCurrent(page);
                    if (_scriptManager == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.Common_ScriptManagerRequired, ID));
                    }
                }
                return _scriptManager;
            }
        }

        [
        Category("Behavior"),
        DefaultValue(null),
        Editor("System.Web.UI.Design.UpdatePanelTriggerCollectionEditor, " +
            AssemblyRef.SystemWebExtensionsDesign, typeof(UITypeEditor)),
        ResourceDescription("UpdatePanel_Triggers"),
        PersistenceMode(PersistenceMode.InnerProperty),
        MergableProperty(false),
        ]
        public UpdatePanelTriggerCollection Triggers {
            get {
                if (_triggers == null) {
                    // NOTE: This is not view state managed, because the update panel trigger
                    //       collection needs to be ready by InitComplete (so that
                    //       Initialize of all triggers gets called at init time), which
                    //       implies that the trigger collection cannot be modified
                    //       beyond what was set up declaratively.
                    _triggers = new UpdatePanelTriggerCollection(this);
                }
                return _triggers;
            }
        }

        [
        ResourceDescription("UpdatePanel_UpdateMode"),
        Category("Behavior"),
        DefaultValue(UpdatePanelUpdateMode.Always),
        ]
        public UpdatePanelUpdateMode UpdateMode {
            get {
                return _updateMode;
            }
            set {
                if (value < UpdatePanelUpdateMode.Always || value > UpdatePanelUpdateMode.Conditional) {
                    throw new ArgumentOutOfRangeException("value");
                }
                _updateMode = value;
            }
        }

        private SingleChildControlCollection ChildControls {
            get {
                SingleChildControlCollection singleChildCollection = Controls as SingleChildControlCollection;
                Debug.Assert(singleChildCollection != null, "The Controls property did not return the expected control collection instance.");
                return singleChildCollection;
            }
        }

        private void AddContentTemplateContainer() {
            // This will call an internal method to specially add the
            // ContentTemplateContainer to the control tree safely.
            ChildControls.AddSingleChild(_contentTemplateContainer);
        }

        internal void ClearContent() {
            Debug.Assert(DesignMode, "ClearContent should only be used in DesignMode.");
            // DevDiv Bugs 135848:
            // Called from UpdatePanelDesigner to clear control tree when
            // GetDesignTimeHtml(DesignerRegionCollection regions) is called, necessary to avoid
            // duplicate controls being created at design time. See comment in UpdatePanelDesigner.
            ContentTemplateContainer.Controls.Clear();
            _contentTemplateContainer = null;
            ChildControls.ClearInternal();
        }

        private void CreateContents() {
            if (DesignMode) {
                // Clear out old stuff
                ClearContent();
            }

            // The ContentTemplateContainer may have already been created by someone due to
            // some dynamic access. If the container already exists and there is a ContentTemplate,
            // we will instantiate into it.
            if (_contentTemplateContainer == null) {
                _contentTemplateContainer = CreateContentTemplateContainer();

                // The controls inside the template are instantiated into
                // a dummy container to ensure that they all do lifecycle catchup
                // at the same time (i.e. Init1, Init2, Load1, Load2) as opposed to
                // one after another (i.e. Init1, Load1, Init2, Load2).
                if (_contentTemplate != null) {
                    _contentTemplate.InstantiateIn(_contentTemplateContainer);
                }

                AddContentTemplateContainer();
            }
            else if (_contentTemplate != null) {
                // Someone already created a ContentTemplateContainer, instantiate into it
                _contentTemplate.InstantiateIn(_contentTemplateContainer);
            }
        }

        protected virtual Control CreateContentTemplateContainer() {
            return new Control();
        }

        protected sealed override ControlCollection CreateControlCollection() {
            // We override and seal this method because we have very special semantics
            // on the behavior of this method and the type of ControlCollection we create.
            return new SingleChildControlCollection(this);
        }

        protected internal virtual void Initialize() {
            if (_triggers != null) {
                if (ScriptManager.SupportsPartialRendering) {
                    // Triggers need to be initialized in initial requests as well as all postbacks,
                    // however only if partial rendering is enabled.
                    _triggers.Initialize();
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);

            RegisterPanel();

            // DevDiv 79989: Whether the template has been set or not we need to ensure
            // the template container is created by Init to remain consistent with 1.0.
            if (_contentTemplateContainer == null) {
                _contentTemplateContainer = CreateContentTemplateContainer();
                AddContentTemplateContainer();
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnLoad(EventArgs e) {
            base.OnLoad(e);

            if (!DesignMode) {
                if (!ScriptManager.IsInAsyncPostBack) {
                    // In partial rendering mode, ScriptManager calls Initialize.
                    // In all other cases we have to initialize here.

                    // This will cause things like AsyncPostBackTrigger to
                    // register event handlers for control events, which in turn
                    // will lead controls to track property values in view state
                    // and appropriately detect changes on the subsequent postbacks.
                    Initialize();
                }
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            if (!ChildrenAsTriggers && UpdateMode == UpdatePanelUpdateMode.Always) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanel_ChildrenTriggersAndUpdateAlways, ID));
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected internal override void OnUnload(EventArgs e) {
            if (!DesignMode && _panelRegistered) {
                ScriptManager.UnregisterUpdatePanel(this);
            }

            base.OnUnload(e);
        }

        private void RegisterPanel() {
            // Safeguard against registering in design mode, and against registering twice
            if (!DesignMode && !_panelRegistered) {
                // Before we can register we need to make sure all our parent panel (if any) has
                // registered already. This is critical since the ScriptManager assumes that
                // the panels are registered in a specific order.
                Control parent = Parent;
                while (parent != null) {
                    UpdatePanel parentUpdatePanel = parent as UpdatePanel;
                    if (parentUpdatePanel != null) {
                        parentUpdatePanel.RegisterPanel();
                        break;
                    }

                    parent = parent.Parent;
                }

                // Now we can register ourselves
                ScriptManager.RegisterUpdatePanel(this);
                _panelRegistered = true;
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            IPage.VerifyRenderingInServerForm(this);

            base.Render(writer);
        }

        protected internal override void RenderChildren(HtmlTextWriter writer) {
            if (_asyncPostBackMode) {
                Debug.Assert(!DesignMode, "Shouldn't be in DesignMode");
                // Render might sometimes be called twice instead of just once if we are forcing
                // all controls to render to ensure EventValidation is valid.
                if (_rendered) {
                    return;
                }

                HtmlTextWriter childWriter = new HtmlTextWriter(new StringWriter(CultureInfo.CurrentCulture));
                base.RenderChildren(childWriter);

                PageRequestManager.EncodeString(writer, UpdatePanelToken, ClientID, childWriter.InnerWriter.ToString());
            }
            else {
                Debug.Assert(!_rendered);

                writer.AddAttribute(HtmlTextWriterAttribute.Id, ClientID);
                if (_attributes != null) {
                    _attributes.AddAttributes(writer);
                }

                if (RenderMode == UpdatePanelRenderMode.Block) {
                    writer.RenderBeginTag(HtmlTextWriterTag.Div);
                }
                else {
                    writer.RenderBeginTag(HtmlTextWriterTag.Span);
                }
                base.RenderChildren(writer);
                writer.RenderEndTag();
            }

            _rendered = true;
        }

        internal void SetAsyncPostBackMode(bool asyncPostBackMode) {
            if (_asyncPostBackModeInitialized) {
                throw new InvalidOperationException(AtlasWeb.UpdatePanel_SetPartialRenderingModeCalledOnce);
            }

            _asyncPostBackMode = asyncPostBackMode;
            _asyncPostBackModeInitialized = true;
        }

        public void Update() {
            if (UpdateMode == UpdatePanelUpdateMode.Always) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanel_UpdateConditional, ID));
            }

            if (_asyncPostBackModeInitialized) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanel_UpdateTooLate, ID));
            }

            _explicitUpdate = true;
        }

        string IAttributeAccessor.GetAttribute(string key) {
            return (_attributes != null) ? _attributes[key] : null;
        }

        void IAttributeAccessor.SetAttribute(string key, string value) {
            Attributes[key] = value;
        }

        private sealed class SingleChildControlCollection : ControlCollection {
            private bool _allowClear;

            public SingleChildControlCollection(Control owner)
                : base(owner) {
            }

            internal void AddSingleChild(Control child) {
                Debug.Assert(Count == 0, "The collection must be empty if this is called");
                base.Add(child);
            }

            public override void Add(Control child) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
            }

            public override void AddAt(int index, Control child) {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
            }

            public override void Clear() {
                throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
            }

            internal void ClearInternal() {
                try {
                    _allowClear = true;
                    base.Clear();
                }
                finally {
                    _allowClear = false;
                }
            }

            public override void Remove(Control value) {
                if (!_allowClear) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
                }
                base.Remove(value);
            }

            public override void RemoveAt(int index) {
                if (!_allowClear) {
                    throw new InvalidOperationException(String.Format(CultureInfo.InvariantCulture, AtlasWeb.UpdatePanel_CannotModifyControlCollection, Owner.ID));
                }
                base.RemoveAt(index);
            }
        }
        
    }
}
