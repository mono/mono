//------------------------------------------------------------------------------
// <copyright file="Form.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Web.Mobile;
using System.Web.UI;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{
    /// <include file='doc\Form.uex' path='docs/doc[@for="Form"]/*' />
    [
        ControlBuilderAttribute(typeof(FormControlBuilder)),
        DefaultEvent("Activate"),
        Designer(typeof(System.Web.UI.Design.MobileControls.FormDesigner)),
        DesignerAdapter(typeof(System.Web.UI.MobileControls.Adapters.HtmlFormAdapter)),
        PersistChildren(true),
        ToolboxData("<{0}:Form runat=\"server\"></{0}:Form>"),
        ToolboxItem("System.Web.UI.Design.WebControlToolboxItem, " + AssemblyRef.SystemDesign)
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class Form : Panel, ITemplateable, IPostBackEventHandler
    {

        private static readonly Object EventActivate         = new Object();
        private static readonly Object EventDeactivate       = new Object();
        private static readonly Object EventPaginated        = new Object();

        private PagerStyle _pagerStyle;
        private int _cachedCurrentPage = -1;
        private Panel _headerContainer = null;
        private Panel _footerContainer = null;
        private Panel _scriptContainer = null;

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Form"]/*' />
        public Form()
        {
            _pagerStyle = new PagerStyle();
            _pagerStyle.SetControl(this);
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Title"]/*' />
        /// <summary>
        /// <para>
        /// Gets or sets the title used to identify the form.
        /// </para>
        /// </summary>
        /// <value>
        /// <para>
        /// The title may be rendered as part of the form, on devices
        /// that support a title separate from page content (e.g., on
        /// the title bar of a browser using the title tag in HTML).
        /// </para>
        /// </value>
        [
            Bindable(true),
            DefaultValue(""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Form_Title)
        ]
        public String Title
        {
            get
            {
                String s = (String) ViewState["Title"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["Title"] = value;
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Method"]/*' />
        [
            Bindable(true),
            DefaultValue(FormMethod.Post),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.Form_Method)
        ]
        public FormMethod Method
        {
            get
            {
                Object o = ViewState["Method"];
                return ((o != null) ? (FormMethod)o : FormMethod.Post);
            }
            set
            {
                ViewState["Method"] = value;
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Action"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            Editor(typeof(System.Web.UI.Design.UrlEditor),
                typeof(System.Drawing.Design.UITypeEditor)),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.Form_Action)
        ]
        public String Action
        {
            get
            {
                String s = (String) ViewState["Action"];
                return((s != null) ? s : String.Empty);
            }
            set
            {
                ViewState["Action"] = value;
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.PagerStyle"]/*' />
        [
            DefaultValue(null),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
            MobileCategory(SR.Category_Style),
            MobileSysDescription(SR.Form_PagerStyle),
            NotifyParentProperty(true)
        ]
        public PagerStyle PagerStyle
        {
            get
            {
                return _pagerStyle;
            }
        }

        /// <internalonly/>
        protected void RaisePostBackEvent(String eventArgument)
        {
            // If the event argument is all numeric, then it's a pager event.

            bool notANumber = false;
            int length = eventArgument.Length;
            for (int i = 0; i < length; i++)
            {
                char c = eventArgument[i];
                if (c < '0' || c > '9')
                {
                    notANumber = true;
                    break;
                }
            }

            if (!notANumber && length > 0)
            {
                try
                {
                    int page = Int32.Parse(eventArgument, CultureInfo.InvariantCulture);
                    if (page > 0)
                    {
                        int oldPage = _currentPage;
                        if(_cachedCurrentPage == -1)
                        {
                            _currentPage = page;
                            _cachedCurrentPage = page;
                        }

                        // currentpage may != page if page is invalid
                        OnPageChange(oldPage, _currentPage);
                    }
                    return;
                }
                catch
                {
                    // Argument may be invalid number, so let adapter handle it.
                }
            }

            Adapter.HandlePostBackEvent(eventArgument);
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Activate"]/*' />
        [
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.Form_OnActivate)
        ]
        public event EventHandler Activate
        {
            add
            {
                Events.AddHandler(EventActivate, value);
            }
            remove
            {
                Events.RemoveHandler(EventActivate, value);
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Deactivate"]/*' />
        [
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.Form_OnDeactivate)
        ]
        public event EventHandler Deactivate
        {
            add
            {
                Events.AddHandler(EventDeactivate, value);
            }
            remove
            {
                Events.RemoveHandler(EventDeactivate, value);
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Paginated"]/*' />
        [
            MobileCategory(SR.Category_Action),
            MobileSysDescription(SR.Form_OnPaginated)
        ]
        public event EventHandler Paginated
        {
            add
            {
                Events.AddHandler(EventPaginated, value);
            }
            remove
            {
                Events.RemoveHandler(EventPaginated, value);
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Header"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Panel Header
        {
            get
            {
                return _headerContainer;
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Footer"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Panel Footer
        {
            get
            {
                return _footerContainer;
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Script"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Panel Script
        {
            get
            {
                return _scriptContainer;
            }
        }

        internal bool Activated = false;

        internal void FireActivate(EventArgs e)
        {
            if (!Activated)
            {
                Activated = true;
                OnActivate(e);
            }
        }

        internal void FireDeactivate(EventArgs e)
        {
            if (Activated)
            {
                Activated = false;
                OnDeactivate(e);
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.OnActivate"]/*' />
        protected virtual void OnActivate(EventArgs e)
        {
            EventHandler onActivate = (EventHandler)Events[EventActivate];
            if (onActivate != null)
            {
                onActivate(this, e);
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.OnDeactivate"]/*' />
        protected virtual void OnDeactivate(EventArgs e)
        {
            EventHandler onDeactivate = (EventHandler)Events[EventDeactivate];
            if (onDeactivate != null)
            {
                onDeactivate(this, e);
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.HasActivateHandler"]/*' />
        public virtual bool HasActivateHandler()
        {
            return Events[EventActivate] != null;
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.HasDeactivateHandler"]/*' />
        public virtual bool HasDeactivateHandler()
        {
            return Events[EventDeactivate] != null;
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.OnPaginated"]/*' />
        protected virtual void OnPaginated(EventArgs e)
        {
            EventHandler onPaginated = (EventHandler)Events[EventPaginated];
            if (onPaginated != null)
            {
                onPaginated(this, e);
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.OnInit"]/*' />
        protected override void OnInit(EventArgs e)
        {
            if (MobilePage != null && !MobilePage.DesignMode)
            {
                // Be sure we're not included in any other Forms.
                for (Control control = this.Parent; control != null; control = control.Parent)
                {
                    Form parentForm = control as Form;
                    if (parentForm != null)
                    {
                        throw new Exception(SR.GetString(SR.Form_NestedForms,
                            this.ID,
                            parentForm.ID));
                    }
                }
            }
            
            base.OnInit(e);
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.GetLinkedForms"]/*' />
        public IList GetLinkedForms(int optimumPageWeight)
        {
            // Always add itself to the list as the first one
            UniqueSet set = new UniqueSet();
            set.Add(this);

            // If the form has a deactivate handler, then we
            // can't send the form with linked forms, because server interaction
            // is necessary to move between forms.

            if (HasDeactivateHandler())
            {
                return set;
            }

            // Will stop once no new forms are added in a pass, or has
            // reached the optimum page weight.

            int totalWeight = 0;
            int i;

            // negative means the caller doesn't care about the weight
            bool checkWeight = optimumPageWeight >= 0;

            for (i = 0; i < set.Count; i++)
            {
                Form form = (Form)set[i];

                if (checkWeight)
                {
                    totalWeight += form.GetVisibleWeight();
                    if (totalWeight > optimumPageWeight)
                    {
                        break;
                    }
                }
                form.AddLinkedForms(set);
            }

            // More forms may have been linked than the total weight allows.
            // Remove these.

            if (i != 0 &&  // i == 0 means only one form is in the list
                i < set.Count)
            {
                for (int j = set.Count - 1; j >= i; j--)
                {
                    set.RemoveAt(j);
                }
            }

            return set;
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.CreateDefaultTemplatedUI"]/*' />
        public override void CreateDefaultTemplatedUI(bool doDataBind) 
        {
            ITemplate headerTemplate = GetTemplate(Constants.HeaderTemplateTag);
            ITemplate footerTemplate = GetTemplate(Constants.FooterTemplateTag);
            ITemplate scriptTemplate = GetTemplate(Constants.ScriptTemplateTag);

            if (scriptTemplate != null)
            {
                _scriptContainer = new TemplateContainer();
                // The scriptTemplate is not added to the controls tree, so no need to do
                // CheckedInstantiateTemplate to check for infinite recursion.
                scriptTemplate.InstantiateIn(_scriptContainer);
                _scriptContainer.EnablePagination = false;
            }

            if (headerTemplate != null)
            {
                _headerContainer = new TemplateContainer();
                CheckedInstantiateTemplate (headerTemplate, _headerContainer, this);
                _headerContainer.EnablePagination = false;
                Controls.AddAt(0, _headerContainer);
            }
            
            if (footerTemplate != null)
            {
                _footerContainer = new TemplateContainer();
                CheckedInstantiateTemplate (footerTemplate, _footerContainer, this);
                _footerContainer.EnablePagination = false;
                Controls.Add(_footerContainer);
            }

            // Do not call base.CreateDefaultTemplatedUI(), since we don't want
            // Forms to have ContentTemplates as Panels do.
        }

        private int _pageCount = -1;
        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.PageCount"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int PageCount
        {
            get
            {
                // If not yet paginated, it's okay to return -1.

                return _pageCount;
            }
        }

        private int _currentPage = 1;
        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.CurrentPage"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public int CurrentPage
        {
            get
            {
                return _currentPage;
            }
            set
            {
                if (_currentPage != value) 
                {
                    OnPageChange (_currentPage, value);
                }
                _currentPage = value;
                _cachedCurrentPage = _currentPage;
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.BreakAfter"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override bool BreakAfter
        {
            get
            {
                if (MobilePage.DesignMode)
                {
                    return true;
                }
                throw new Exception(
                    SR.GetString(SR.Form_PropertyNotAccessible, "BreakAfter"));
            }

            set
            {
                throw new Exception(
                    SR.GetString(SR.Form_PropertyNotSettable, "BreakAfter"));
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.OnPreRender"]/*' />
        protected override void OnPreRender(EventArgs e)
        {
            // AUI 3630
            base.OnPreRender(e);

            _pageCount = PaginateForm();

            // Clamp to 1 < _currentPage <= PageCount

            int page = Math.Max(Math.Min(_currentPage, _pageCount), 1);
            if(_currentPage != page)
            {
                _currentPage = page;
            }

            if ((Paginate) || (ControlToPaginate != null))
            {
                OnPaginated(new EventArgs());
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.Render"]/*' />
        protected override void Render(HtmlTextWriter writer)
        {
            MobilePage.EnterFormRender(this);
            OnRender(writer);
            MobilePage.ExitFormRender();
        }

        private int PaginateForm()
        {
            int pageWeight = MobilePage.Adapter.OptimumPageWeight;
            if (Header != null)
            {
                pageWeight -= Header.VisibleWeight;
            }
            if (Footer != null)
            {
                pageWeight -= Footer.VisibleWeight;
            }
            if (pageWeight <= 0)
            {
                //


                pageWeight = MobilePage.Adapter.OptimumPageWeight / 2;
            }

            if (IsFormPaginationAllowed())
            {
                if ((Paginate) || (ControlToPaginate == this))
                {
                    ControlPager pager = new ControlPager(this, pageWeight);
                    base.PaginateRecursive(pager);
                    return pager.PageCount;
                }
                else if(ControlToPaginate != null)
                {
                    ControlPager pager = new ControlPager(this, pageWeight);
                    SetControlPage(1);
                    Control control = ControlToPaginate;
                    MobileControl ctp = control as MobileControl;
                    if(ctp != null) 
                    {
                        ctp.PaginateRecursive(pager);
                    }
                    else 
                    {
                        int firstAssignedPage = -1;
                        DoPaginateChildren(pager, control, ref firstAssignedPage);
                    }
                    while(control != this)
                    {
                        MobileControl mc = control as MobileControl;
                        if(mc != null)
                        {
                            if(mc is Form)
                            {
                                throw(new Exception(SR.GetString(SR.Form_InvalidControlToPaginateForm)));
                            }
                            if (mc.FirstPage > ctp.FirstPage)
                            {
                                mc.FirstPage = ctp.FirstPage;
                            }
                            if(mc.LastPage < ctp.LastPage)
                            {
                                mc.LastPage = ctp.LastPage;
                            }
                        }
                        control = control.Parent;
                    }
                    this.LastPage = Math.Max(pager.PageCount, 1);
                    if(Header != null)
                    {
                        SetEnablePaginationRecursive(Header, false);
                    }
                    if(Footer != null)
                    {
                        SetEnablePaginationRecursive(Footer, false);
                    }
                    return this.LastPage;
                }
            }
            
            return 1;
        }

        private bool IsFormPaginationAllowed()
        {
            // AUI 4721

            if (Action.Length == 0)
            {
                return true;
            }

            MobileCapabilities device = (MobileCapabilities)Page.Request.Browser;
            String type = device.PreferredRenderingType;
            bool javascriptSupported = device.JavaScript;

            return javascriptSupported || 
                    (type != MobileCapabilities.PreferredRenderingTypeHtml32 &&
                     type != MobileCapabilities.PreferredRenderingTypeChtml10);
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.PaginateRecursive"]/*' />
        public override void PaginateRecursive(ControlPager pager)
        {
            _pageCount = PaginateForm();
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.PaginateChildren"]/*' />
        protected override bool PaginateChildren
        {
            get
            {
                return true;
            }
        }

        private Control _controlToPaginate = null;

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.ControlToPaginate"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public Control ControlToPaginate
        {
            get
            {
                if(_controlToPaginate != null)
                {
                    return _controlToPaginate;
                }
                if(ViewState["ControlToPaginate"] != null)
                {
                    _controlToPaginate = Page.FindControl((ViewState["ControlToPaginate"]).ToString());
                }
                return _controlToPaginate;
            }
            set
            {
                if(value != null)
                {
                    ViewState["ControlToPaginate"] = value.UniqueID;
                }
                else
                {
                    ViewState.Remove("ControlToPaginate");
                }
                _controlToPaginate = value;
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.LoadPrivateViewState"]/*' />
        protected override void LoadPrivateViewState(Object state)
        {
            _currentPage = state != null ? (int)state : 1;
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.SavePrivateViewState"]/*' />
        protected override Object SavePrivateViewState()
        {
            int currentPage = _currentPage;
            if (currentPage > 1 && this == MobilePage.ActiveForm)
            {
                return currentPage;
            }
            else
            {
                return null;
            }
        }

        /// <include file='doc\Form.uex' path='docs/doc[@for="Form.OnDataBinding"]/*' />
        protected override void OnDataBinding(EventArgs e)
        {
            if(Script != null)
            {
                Script.DataBind();
            }
            base.OnDataBinding(e);
        }

        private IPostBackEventHandler _defaultEventHandler = null;
        internal void RegisterEventHandler(IPostBackEventHandler control)
        {
            if (_defaultEventHandler == null)
            {
                _defaultEventHandler = control;
            }
        }

        internal IPostBackEventHandler DefaultEventHandler
        {
            get
            {
                return _defaultEventHandler;
            }
        }

        internal override void InvalidateParentStyles()
        {
            PagerStyle.InvalidateParentStyle();
            base.InvalidateParentStyles();
        }

        #region IPostBackEventHandler implementation
        void IPostBackEventHandler.RaisePostBackEvent(String eventArgument) {
            RaisePostBackEvent(eventArgument);
        }
        #endregion 
    }

    /// <include file='doc\Form.uex' path='docs/doc[@for="FormControlBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class FormControlBuilder : LiteralTextContainerControlBuilder
    {
        /// <include file='doc\Form.uex' path='docs/doc[@for="FormControlBuilder.AppendSubBuilder"]/*' />
        public override void AppendSubBuilder(ControlBuilder subBuilder)
        {
            Type controlType = subBuilder.ControlType;
            if(!(
                 (subBuilder.GetType().FullName == "System.Web.UI.CodeBlockBuilder") ||
                 (typeof(MobileControl).IsAssignableFrom(controlType)) ||
                 (typeof(UserControl).IsAssignableFrom(controlType)) ||
                 (typeof(DeviceSpecific).IsAssignableFrom(controlType))
                 ))
            {
                throw(new Exception(SR.GetString(SR.Form_InvalidSubControlType, subBuilder.TagName)));
            }
            base.AppendSubBuilder(subBuilder);
        }
    }
}
