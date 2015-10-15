//------------------------------------------------------------------------------
// <copyright file="LoginView.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.Collections;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Web.Security;
    using System.Web.UI;


    /// <devdoc>
    /// Renders exactly one of its templates, chosen by whether a user is logged in
    /// and the roles that contain the user.
    /// </devdoc>
    [
    Bindable(false),
    ParseChildren(true),
    PersistChildren(false),
    Designer("System.Web.UI.Design.WebControls.LoginViewDesigner," + AssemblyRef.SystemDesign),
    DefaultProperty("CurrentView"),
    DefaultEvent("ViewChanged"),
    Themeable(true),
    ]
    public class LoginView : Control, INamingContainer {

        private RoleGroupCollection _roleGroups;
        private ITemplate _loggedInTemplate;
        private ITemplate _anonymousTemplate;

        private int _templateIndex;

        private const int anonymousTemplateIndex = 0;
        private const int loggedInTemplateIndex = 1;
        private const int roleGroupStartingIndex = 2;

        private static readonly object EventViewChanging = new object();
        private static readonly object EventViewChanged = new object();


        /// <devdoc>
        /// Template shown when no user is logged in.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(LoginView)),
        ]
        public virtual ITemplate AnonymousTemplate {
            get {
                return _anonymousTemplate;
            }
            set {
                _anonymousTemplate = value;
            }
        }

        [
        Browsable(true),
        ]
        public override bool EnableTheming {
            get {
                return base.EnableTheming;
            }
            set {
                base.EnableTheming = value;
            }
        }

        [
        Browsable(true),
        ]
        public override string SkinID {
            get {
                return base.SkinID;
            }
            set {
                base.SkinID = value;
            }
        }



        /// <devdoc>
        /// Copied from CompositeControl.  This control does not extend CompositeControl because it should not be a WebControl.
        /// </devdoc>
        public override ControlCollection Controls {
            get {
                EnsureChildControls();
                return base.Controls;
            }
        }


        /// <devdoc>
        /// Copied from CompositeControl.  This control does not extend CompositeControl because it should not be a WebControl.
        /// Does not call Base.DataBind(), since we need to call EnsureChildControls() between
        /// OnDataBinding() and DataBindChildren().
        /// </devdoc>
        public override void DataBind() {
            // Do our own databinding
            OnDataBinding(EventArgs.Empty);

            EnsureChildControls();

            // Do all of our children's databinding
            DataBindChildren();
        }


        /// <devdoc>
        /// Template shown when a user is logged in, but the user is not in any role associated with a template.
        /// </devdoc>
        [
        Browsable(false),
        DefaultValue(null),
        PersistenceMode(PersistenceMode.InnerProperty),
        TemplateContainer(typeof(LoginView)),
        ]
        public virtual ITemplate LoggedInTemplate {
            get {
                return _loggedInTemplate;
            }
            set {
                _loggedInTemplate = value;
            }
        }


        /// <devdoc>
        /// Maps groups of roles to templates.
        /// </devdoc>
        [
        WebCategory("Behavior"),
        MergableProperty(false),
        Themeable(false),
        Filterable(false),
        PersistenceMode(PersistenceMode.InnerProperty),
        WebSysDescription(SR.LoginView_RoleGroups)
        ]
        public virtual RoleGroupCollection RoleGroups {
            get {
                if (_roleGroups == null) {
                    _roleGroups = new RoleGroupCollection();
                }
                return _roleGroups;
            }
        }

        /// <devdoc>
        /// Index of the template rendered on the previous page load.  Saved in ControlState.
        /// 0:   AnonymousTemplate
        /// 1:   LoggedInTemplate
        /// >=2: RoleGroup template with index n-2
        /// </devdoc>
        private int TemplateIndex {
            get {
                return _templateIndex;
            }
            set {
                if (value != TemplateIndex) {
                    OnViewChanging(EventArgs.Empty);
                    _templateIndex = value;
                    ChildControlsCreated = false;
                    OnViewChanged(EventArgs.Empty);
                }
            }
        }


        /// <devdoc>
        /// Raised after the view is changed.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.LoginView_ViewChanged)
        ]
        public event EventHandler ViewChanged {
            add {
                Events.AddHandler(EventViewChanged, value);
            }
            remove {
                Events.RemoveHandler(EventViewChanged, value);
            }
        }


        /// <devdoc>
        /// Raised before the view is changed.  Not cancellable, because the view is changed
        /// when the logged-in user changes, and it wouldn't make sense to cancel this.
        /// </devdoc>
        [
        WebCategory("Action"),
        WebSysDescription(SR.LoginView_ViewChanging)
        ]
        public event EventHandler ViewChanging {
            add {
                Events.AddHandler(EventViewChanging, value);
            }
            remove {
                Events.RemoveHandler(EventViewChanging, value);
            }
        }


        /// <devdoc>
        /// Instantiate the appropriate template.
        /// </devdoc>
        protected internal override void CreateChildControls() {
            Controls.Clear();

            // For the first request, set _templateIndex now, so the correct template is
            // instantiated and we do not raise the ViewChanging/ViewChanged events.
            Page page = Page;
            if (page != null && !page.IsPostBack && !DesignMode) {
                _templateIndex = GetTemplateIndex();
            }

            int templateIndex = TemplateIndex;
            ITemplate template = null;
            switch (templateIndex) {
                case anonymousTemplateIndex:
                    template = AnonymousTemplate;
                    break;
                case loggedInTemplateIndex:
                    template = LoggedInTemplate;
                    break;
                default:
                    int roleGroupIndex = templateIndex - roleGroupStartingIndex;
                    RoleGroupCollection roleGroups = RoleGroups;
                    if (0 <= roleGroupIndex && roleGroupIndex < roleGroups.Count) {
                        template = roleGroups[roleGroupIndex].ContentTemplate;
                    }
                    break;
            }

            if (template != null) {
                Control templateContainer = new Control();
                template.InstantiateIn(templateContainer);
                Controls.Add(templateContainer);
            }
        }

        [
        EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override void Focus() {
            throw new NotSupportedException(SR.GetString(SR.NoFocusSupport, this.GetType().Name));
        }

        /// <devdoc>
        /// Loads the control state for those properties that should persist across postbacks
        /// even when EnableViewState=false.
        /// </devdoc>
        protected internal override void LoadControlState(object savedState) {
            if (savedState != null) {
                Pair state = (Pair)savedState;
                if (state.First != null) {
                    base.LoadControlState(state.First);
                }
                if (state.Second != null) {
                    _templateIndex = (int)state.Second;
                }
            }
        }


        protected internal override void OnInit(EventArgs e) {
            base.OnInit(e);
            if (Page != null) {
                Page.RegisterRequiresControlState(this);
            }
        }


        /// <devdoc>
        /// Sets the TemplateIndex based on the current user.
        /// </devdoc>
        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            TemplateIndex = GetTemplateIndex();

            // This is called in Control.PreRenderRecursiveInteral, but we need to call it again
            // since we may have changed the TemplateIndex
            EnsureChildControls();
        }


        /// <devdoc>
        /// Raises the ViewChanged event.
        /// </devdoc>
        protected virtual void OnViewChanged(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventViewChanged];
            if (handler != null) {
                handler(this, e);
            }
        }


        /// <devdoc>
        /// Raises the ViewChanging event.
        /// </devdoc>
        protected virtual void OnViewChanging(EventArgs e) {
            EventHandler handler = (EventHandler)Events[EventViewChanging];
            if (handler != null) {
                handler(this, e);
            }
        }

        protected internal override void Render(HtmlTextWriter writer) {
            EnsureChildControls();
            base.Render(writer);
        }

        /// <devdoc>
        /// Saves the control state for those properties that should persist across postbacks
        /// even when EnableViewState=false.
        /// </devdoc>
        protected internal override object SaveControlState() {
            object baseState = base.SaveControlState();
            if (baseState != null || _templateIndex != 0) {
                object templateIndexState = null;

                if (_templateIndex != 0) {
                    templateIndexState = _templateIndex;
                }
                return new Pair(baseState, templateIndexState);
            }
            return null;
        }


        /// <devdoc>
        /// Allows the designer to set the TemplateIndex, so the different templates can be shown in the designer.
        /// </devdoc>
        [SecurityPermission(SecurityAction.Demand, Unrestricted = true)]
        protected override void SetDesignModeState(IDictionary data) {
            if (data != null) {
                object o = data["TemplateIndex"];
                if (o != null) {
                    TemplateIndex = (int)o;

                    // Note: we always recreate the child controls in the designer to correctly handle the case of
                    // the currently selected role group being deleted.  This is necessary because the
                    // setter for TemplateIndex won't recreate the controls if the TemplateIndex is unchanged,
                    // which is the case when deleting all but the last role group. [Fix for 
                    ChildControlsCreated = false;
                }
            }
        }

        private int GetTemplateIndex() {
            if (!DesignMode && Page != null && Page.Request.IsAuthenticated) {
                IPrincipal user = LoginUtil.GetUser(this);
                int roleGroupIndex = -1;

                // Unlikely but possible for Page.Request.IsAuthenticated to be true and
                // user to be null.
                if (user != null) {
                    roleGroupIndex = RoleGroups.GetMatchingRoleGroupInternal(user);
                }

                if (roleGroupIndex >= 0) {
                    return roleGroupIndex + roleGroupStartingIndex;
                }
                else {
                    return loggedInTemplateIndex;
                }
            }
            else {
                return anonymousTemplateIndex;
            }
        }
    }
}
