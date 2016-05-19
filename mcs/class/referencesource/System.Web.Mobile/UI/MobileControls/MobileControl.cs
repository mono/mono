//------------------------------------------------------------------------------
// <copyright file="MobileControl.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System.Text;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mobile;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.Util;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile control base class.
     * All core controls and extension controls extend from this class.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl"]/*' />
    [
        ControlBuilderAttribute(typeof(MobileControlBuilder)),
        Designer(typeof(System.Web.UI.Design.MobileControls.MobileControlDesigner)),
        ParseChildren(false),
        PersistChildren(false),
        ToolboxItem(false),
        ToolboxItemFilter("System.Web.UI"),
        ToolboxItemFilter("System.Web.UI.MobileControls", ToolboxItemFilterType.Require),
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public abstract class MobileControl : Control, IAttributeAccessor
    {
        private IControlAdapter _adapter;
        private DeviceSpecific _deviceSpecific;
        internal const String InnerTextViewStateKey = "_!InnerText";
        private static String[] _ignoredCustomAttributes;

        static MobileControl()
        {
            // Note: These should be in alphabetical order!
            _ignoredCustomAttributes = new String[2];
            _ignoredCustomAttributes[0] = "designtimedragdrop";
            _ignoredCustomAttributes[1] = "name";
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.Adapter"]/*' />
        [
            Browsable(false),
            Bindable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public new IControlAdapter Adapter
        {
            get
            {
                if (_adapter == null && MobilePage != null)
                {
                    _adapter = MobilePage.GetControlAdapter(this);
                }
                return _adapter;
            }
        }

        [
            DefaultValue(false)
        ]
        public override sealed bool EnableTheming {
            get {
                return false;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.Theme_Not_Supported_On_MobileControls));
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.MobilePage"]/*' />
        [
            Browsable(false),
            Bindable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public MobilePage MobilePage
        {
            get
            {
                Page page = Page;
                if (page != null)
                {
                    MobilePage mobilePage = page as MobilePage;
                    if (mobilePage == null)
                    {
                        if (Site == null || !Site.DesignMode)
                        {
                            throw new Exception(
                                SR.GetString(SR.MobileControl_MustBeInMobilePage,
                                             Page));
                        }
                    }
                    return mobilePage;
                }
                else
                {
                    return null;
                }
            }
        }

        private Form _form = null;

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.Form"]/*' />
        [
            Browsable(false),
            Bindable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public Form Form
        {
            get
            {
                if (_form == null)
                {
                    for (Control control = this; control != null; control = control.Parent)
                    {
                        Form form = control as Form;
                        if (form != null)
                        {
                            _form = form;
                            return _form;
                        }
                    }

                    if (_form == null && RequiresForm)
                    {
                        throw new Exception(SR.GetString(SR.MobileControl_MustBeInForm, 
                                                         UniqueID,
                                                         GetType().Name));
                    }
                }
                return _form;
            }
        }

        [
            Browsable(false)
        ]
        public override sealed string SkinID {
            get {
                return String.Empty;
            }
            set {
                throw new NotSupportedException(SR.GetString(SR.Theme_Not_Supported_On_MobileControls));
            }
        }

        [
            EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override sealed void ApplyStyleSheetSkin(Page page) {
            throw new NotSupportedException(SR.GetString(SR.Theme_Not_Supported_On_MobileControls));
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.ResolveFormReference"]/*' />
        public Form ResolveFormReference(String formID)
        {
            Form form = ResolveFormReferenceNoThrow(formID);
            if (form == null)
            {
                throw new ArgumentException(
                    SR.GetString(SR.MobilePage_FormNotFound, formID));
            }

            return form;
        }

        internal Form ResolveFormReferenceNoThrow(String formID)
        {
            for (Control ctl = this; ctl != null; ctl = ctl.Parent)
            {
                if (ctl is TemplateControl)
                {
                    Form childForm = ctl.FindControl(formID) as Form;
                    if (childForm != null)
                    {
                        return childForm;
                    }
                }
            }

            return null;
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.AddedControl"]/*' />
        protected override void AddedControl(Control control, int index) 
        {
            _cachedInnerText = null;
            base.AddedControl(control, index);
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.RemovedControl"]/*' />
        protected override void RemovedControl(Control control) {
            MobileControl ctl = control as MobileControl;
            if (ctl != null) {
                ctl.InvalidateParentStyles();
            }

            // Remove the cached _deviceSpecific.
            DeviceSpecific deviceSpecific = control as DeviceSpecific;
            if (deviceSpecific != null) {
                _deviceSpecific.SetOwner(null);
                _deviceSpecific = null;
            }

            _cachedInnerText = null;
            base.RemovedControl(control);
        }

        internal virtual void InvalidateParentStyles()
        {
            Style.InvalidateParentStyle();
        }

        /////////////////////////////////////////////////////////////////////////
        //  TEMPLATES SUPPORT
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.IsTemplated"]/*' />
        [
            Browsable(false),
            Bindable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual bool IsTemplated
        {
            get
            {
                if (_deviceSpecific != null && _deviceSpecific.HasTemplates)
                {
                    return true;
                }
                else
                {
                    Style referredStyle = Style.ReferredStyle;
                    return referredStyle != null && referredStyle.IsTemplated;
                }
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.GetTemplate"]/*' />
        public virtual ITemplate GetTemplate(String templateName)
        {
            Debug.Assert(this is ITemplateable);
            ITemplate t = null;

            if (_deviceSpecific != null)
            {
                t = _deviceSpecific.GetTemplate(templateName);
            }

            if (t == null)
            {
                Style referredStyle = Style.ReferredStyle;
                if (referredStyle != null)
                {
                    t = referredStyle.GetTemplate(templateName);
                }
            }

            return t;
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.EnsureTemplatedUI"]/*' />
        public virtual void EnsureTemplatedUI()
        {
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.CreateTemplatedUI"]/*' />
        protected virtual void CreateTemplatedUI(bool doDataBind)
        {
            // It is possible for a rogue control to call this even though
            // the control is not templated. Catch and throw an exception for
            // this case.
            if (!IsTemplated)
            {
                throw new Exception(
                    SR.GetString(SR.MobileControl_NoTemplatesDefined));
            }
            Adapter.CreateTemplatedUI(doDataBind);
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.CreateDefaultTemplatedUI"]/*' />
        public virtual void CreateDefaultTemplatedUI(bool doDataBind)
        {
            // Create nothing by default.
        }

        // Return the nearest containing TemplateControl (UserControl or Page),
        // or null if none.
        internal TemplateControl FindContainingTemplateControl()
        {
            Control control = this;
            while (!(control is TemplateControl) &&
                   control != null)
            {
                control = control.Parent;
            }

            // We assume that the only template controls are Page and
            // UserControl. 
            Debug.Assert(control == null ||
                         control is Page ||
                         control is UserControl);

            return (TemplateControl)control;
        }
        
        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.ResolveUrl"]/*' />
        public new String ResolveUrl(String relativeUrl)
        {
            int length;

            if (relativeUrl == null || 
                    (length = relativeUrl.Length) == 0 || 
                    !UrlPath.IsRelativeUrl(relativeUrl))
            {
                return relativeUrl;
            }

            // VSWhidbey 450801. Make the relativeUrl app-absolute first.
            relativeUrl = UrlPath.MakeVirtualPathAppAbsolute(relativeUrl);

            // Deal with app relative syntax (e.g. ~/foo)
            String baseUrl = UrlPath.MakeVirtualPathAppAbsolute(TemplateSourceDirectory);

            // Determine if there are any . or .. sequences.

            bool containsDots = false;
            for (int examine = 0; examine < length; examine++) 
            {
                examine = relativeUrl.IndexOf('.', examine);
                if (examine < 0)
                {
                    break;
                }

                // Expression borrowed from UrlPath.cs
                if ((examine == 0 || relativeUrl[examine - 1] == '/')
                    && (examine + 1 == length || relativeUrl[examine + 1] == '/' ||
                        (relativeUrl[examine + 1] == '.' && 
                            (examine + 2 == length || relativeUrl[examine + 2] == '/'))))
                {
                    containsDots = true;
                    break;
                }
            }

            if (!containsDots)
            {
                if (baseUrl.Length == 0)
                {
                    return relativeUrl;
                }

                TemplateControl parentTemplateControl = FindContainingTemplateControl();
                if (parentTemplateControl == null || parentTemplateControl is MobilePage)
                {
                    return relativeUrl;
                }
            }

            if (baseUrl.IndexOf(' ') != -1)
            {
                baseUrl = baseUrl.Replace(" ", "%20");
            }

            String url = UrlPath.Combine(baseUrl, relativeUrl);
            return Context.Response.ApplyAppPathModifier(url);
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.OnDataBinding"]/*' />
        protected override void OnDataBinding(EventArgs e) 
        {
            if (_containsDataboundLiteral)
            {
                _cachedInnerText = null;
            }
            base.OnDataBinding(e);
        }

        /////////////////////////////////////////////////////////////////////////
        //  BEGIN DESIGN TIME TEMPLATES SUPPORT
        /////////////////////////////////////////////////////////////////////////

        // We need to expose the DeviceSpecific in runtime code for two purposes. A. We need to
        // access all components inside DeviceSpecific. B. We have to persist the modified HTML.
        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.DeviceSpecific"]/*' />
        [
            Browsable(false),
            Bindable(false),
            //PersistenceMode(PersistenceMode.InnerDefaultProperty)
            PersistenceMode(PersistenceMode.InnerProperty)
        ]
        public DeviceSpecific DeviceSpecific
        {
            get
            {
                return _deviceSpecific;
            }
            set
            {
                _deviceSpecific = value;
                if (value != null)
                {
                    value.SetOwner(this);
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  CONTROL OVERRIDES
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.AddParsedSubObject"]/*' />
        protected override void AddParsedSubObject(Object obj)
        {
            if (obj is DeviceSpecific)
            {
                if (DeviceSpecific == null)
                {
                    DeviceSpecific = (DeviceSpecific)obj;
                }
                else
                {
                    throw new Exception(
                        SR.GetString(SR.MobileControl_NoMultipleDeviceSpecifics));
                }
            }
            else
            {
                base.AddParsedSubObject(obj);
            }
        }

        internal virtual void ApplyDeviceSpecifics()
        {
            if (_deviceSpecific != null)
            {
                _deviceSpecific.ApplyProperties();
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  BEGIN ADAPTER PLUMBING
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.OnInit"]/*' />
        protected override void OnInit(EventArgs e)
        {
            MobilePage page = MobilePage;
            bool isRuntime = page != null && !page.DesignMode;

            // We don't want to override the properties at design time.
            if (isRuntime)
            {
                ApplyDeviceSpecifics();
            }
            if (Adapter != null)
            {
                Adapter.OnInit(e);
            }
            base.OnInit(e);

            // If we are being created after the first pass 
            // then 

            if (isRuntime && page.PrivateViewStateLoaded)
            {
                Object privateViewState = ((MobilePage)Page).GetPrivateViewState(this);
                if(privateViewState != null)
                {
                    LoadPrivateViewStateInternal(privateViewState);
                }
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.IsFormSubmitControl"]/*' />
        protected virtual bool IsFormSubmitControl()
        {
            return false;
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.OnLoad"]/*' />
        protected override void OnLoad(EventArgs e)
        {
            IPostBackEventHandler eventHandler = this as IPostBackEventHandler;
            if (Form != null && eventHandler != null && IsFormSubmitControl())
            {
                Form.RegisterEventHandler(eventHandler);
            }

            // Handle custom attributes.

            if (_customAttributes != null && !MobilePage.AllowCustomAttributes)
            {
                // AUI 2346

                ICollection keys = CustomAttributes.Keys;
                String illegalCustomAttributes = null;
                if (keys != null)
                {
                    foreach (String key in keys)
                    {
                        String keyLower = key.ToLower(CultureInfo.InvariantCulture);
                        if (Array.BinarySearch(_ignoredCustomAttributes, keyLower) < 0)
                        {
                            if (illegalCustomAttributes != null)
                            {
                                illegalCustomAttributes += "; ";
                            }
                            illegalCustomAttributes += key + "=" + CustomAttributes[key];
                        }
                    }
                }

                if (illegalCustomAttributes != null)
                {
                    throw new Exception(
                        SR.GetString(SR.MobileControl_NoCustomAttributes,
                                     illegalCustomAttributes));
                }
                
            }
            Adapter.OnLoad(e);
            base.OnLoad(e);
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.OnPreRender"]/*' />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            Adapter.OnPreRender(e);
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.Render"]/*' />
        protected override void Render(HtmlTextWriter writer)
        {
            if (RequiresForm && Page != null)
            {
                Page.VerifyRenderingInServerForm(this);
            }

            if (IsVisibleOnPage(Form.CurrentPage))
            {
                OnRender(writer);
            }
        }

        internal virtual bool RequiresForm
        {
            get
            {
                return true;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.OnRender"]/*' />
        protected virtual void OnRender(HtmlTextWriter writer)
        {
            Adapter.Render(writer);
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.OnUnload"]/*' />
        protected override void OnUnload(EventArgs e)
        {
            base.OnUnload(e);
            if (Adapter != null) {
                Adapter.OnUnload(e);
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.RenderChildren"]/*' />
        public new void RenderChildren(HtmlTextWriter writer)
        {
            base.RenderChildren(writer);
        }

        /////////////////////////////////////////////////////////////////////////
        //  VIEW STATE SUPPORT
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.TrackViewState"]/*' />
        protected override void TrackViewState()
        {
            base.TrackViewState();
            ((IStateManager)Style).TrackViewState();
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.LoadViewState"]/*' />
        protected override void LoadViewState(Object savedState) 
        {
            if (savedState != null)
            {
                Object[] state = (Object[])savedState;
                if (state[0] != null)
                {
                    base.LoadViewState(state[0]);
                }
                if (state[1] != null)
                {
                    ((IStateManager)Style).LoadViewState(state[1]);
                }

                // Reset the property if persisted before, done similarly in
                // ASP.NET
                String s = (String)ViewState[InnerTextViewStateKey];
                if (s != null)
                {
                    InnerText = s;
                }
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.SaveViewState"]/*' />
        protected override Object SaveViewState()
        {
            Object baseState = base.SaveViewState();
            Object styleState = ((IStateManager)Style).SaveViewState();

            if (baseState == null && styleState == null)
            {
                return null;
            }

            return new Object[2] { baseState, styleState };
        }

        internal void SavePrivateViewStateInternal()        
        {
            Object privateState = SavePrivateViewState();
            Object adapterState = Adapter.SaveAdapterState();
            if (privateState != null || adapterState != null)
            {
                privateState = new Object[] { privateState, adapterState };
                MobilePage.AddClientViewState(this, privateState);
            }
        }

        internal void LoadPrivateViewStateInternal(Object state)
        {
            Debug.Assert(state != null);
            Object[] privateState = (Object[])state;
            if (privateState[0] != null)
            {
                LoadPrivateViewState(privateState[0]);
            }
            if (privateState[1] != null)
            {
                Adapter.LoadAdapterState(privateState[1]);
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.LoadPrivateViewState"]/*' />
        protected virtual void LoadPrivateViewState(Object state)
        {
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.SavePrivateViewState"]/*' />
        protected virtual Object SavePrivateViewState()
        {
            return null;
        }

        /////////////////////////////////////////////////////////////////////////
        //  CUSTOM PROPERTIES
        /////////////////////////////////////////////////////////////////////////

        private StateBag _customAttributes;

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.CustomAttributes"]/*' />
        [
            Browsable(false),
            Bindable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public StateBag CustomAttributes
        {
            get
            {
                if (_customAttributes == null)
                {
                    _customAttributes = new StateBag(true); // Ignore case
                    if (IsTrackingViewState)
                    {
                        ((IStateManager)_customAttributes).TrackViewState();
                    }
                }
                return _customAttributes;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.IAttributeAccessor.GetAttribute"]/*' />
        /// <internalonly/>
        protected String GetAttribute(String name) 
        {
            return (_customAttributes != null) ? (String)_customAttributes[name] : null;
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.IAttributeAccessor.SetAttribute"]/*' />
        /// <internalonly/>
        protected void SetAttribute(String name, String value) 
        {
            CustomAttributes[name] = value;
        }

        /////////////////////////////////////////////////////////////////////////
        //  BEGIN STYLE PROPERTIES
        /////////////////////////////////////////////////////////////////////////

        Style _style;

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.Style"]/*' />
        [
            Browsable(false),
            Bindable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        internal protected virtual Style Style
        {
            get
            {
                if (_style == null)
                {
                    _style = CreateStyle();
                    _style.SetControl(this);
                }
                return _style;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.CreateStyle"]/*' />
        protected virtual Style CreateStyle()
        {
            return new Style();
        }

        internal void RefreshStyle()
        {
            this.Style.Refresh();
        }


        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.StyleReference"]/*' />
        [
            Bindable(false),
            DefaultValue(null),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.MobileControl_StyleReference),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.StyleReferenceConverter))
        ]
        public virtual String StyleReference
        {
            get
            {
                return this.Style.StyleReference;
            }
            set
            {
                this.Style.StyleReference = value;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.Font"]/*' />
        [
            DefaultValue(null),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.MobileControl_Font),
            NotifyParentProperty(true)
        ]
        public virtual FontInfo Font
        {
            get
            {
                return this.Style.Font;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.Alignment"]/*' />
        [
            Bindable(true),
            DefaultValue(Alignment.NotSet),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.MobileControl_Alignment),
        ]
        public virtual Alignment Alignment
        {
            get
            {
                return this.Style.Alignment;
            }
            set
            {
                this.Style.Alignment = value;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.Wrapping"]/*' />
        [
            Bindable(true),
            DefaultValue(Wrapping.NotSet),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.MobileControl_Wrapping),
        ]
        public virtual Wrapping Wrapping
        {
            get
            {
                return this.Style.Wrapping;
            }
            set
            {
                this.Style.Wrapping = value;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.ForeColor"]/*' />
        [
            Bindable(true),
            DefaultValue(typeof(Color), ""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Style_ForeColor),
            TypeConverterAttribute(typeof(WebColorConverter))
        ]
        public virtual Color ForeColor
        {
            get
            {
                return this.Style.ForeColor;
            }
            set
            {
                this.Style.ForeColor = value;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.BackColor"]/*' />
        [
            Bindable(true),
            DefaultValue(typeof(Color), ""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.MobileControl_BackColor),
            TypeConverterAttribute(typeof(WebColorConverter))
        ]
        public virtual Color BackColor
        {
            get
            {
                return this.Style.BackColor;
            }
            set
            {
                this.Style.BackColor = value;
            }
        }


        /////////////////////////////////////////////////////////////////////////
        //  END STYLE PROPERTIES
        /////////////////////////////////////////////////////////////////////////

        private String _cachedInnerText = null;
        private bool _containsDataboundLiteral = false;
        private static char[] _newlineChars = { '\r', '\n' };

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.InnerText"]/*' />
        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never),
        ]
        protected String InnerText
        {
            get
            {
                String text = null;
                String innerTextAttribute = (String)ViewState[InnerTextViewStateKey];

                if (_cachedInnerText != null)
                {
                    text = _cachedInnerText;
                }
                else if (HasControls())
                {
                    _containsDataboundLiteral = false;

                    bool containTag;

                    bool trimInnerText = TrimInnerText;
                    bool trimNewlines = TrimNewlines;

                    text = GetControlText(out containTag, out _containsDataboundLiteral, trimInnerText);

                    // Cannot throw exceptions from properties at designtime,
                    // this will break property browser.
                    if (containTag && 
                        !AllowInnerMarkup && 
                        (MobilePage == null || !MobilePage.DesignMode))
                    {
                        throw new Exception(
                            SR.GetString(SR.MobileControl_InnerTextCannotContainTags, 
                            GetType().ToString(), 
                            UniqueID));

                    }

                    // Reset text to empty string at design time if the control
                    // contains DataBoundLiteral child control.
                    if (MobilePage != null && 
                        MobilePage.DesignMode && 
                        _containsDataboundLiteral)
                    {
                        text = String.Empty;
                    }


                    if (trimNewlines)
                    {
                        // Need to trim leading and trailing whitespace, but only up to last newline.

                        int start = 0;
                        int finish = text.Length;
                        int length = text.Length;
                        int i;
                        for (i = 0; i < length; i++)
                        {
                            char c = text[i];
                            if (c == '\n')
                            {
                                start = i + 1;
                            }
                            else if (!Char.IsWhiteSpace(c))
                            {
                                break;
                            }
                        }

                        for (int i2 = length - 1; i2 > i; i2--)
                        {
                            char c = text[i2];
                            if (c == '\r')
                            {
                                finish = i2;
                            }
                            else if (!Char.IsWhiteSpace(c))
                            {
                                break;
                            }
                        }

                        text = text.Substring(start, finish - start);
                    }

                    if ((trimInnerText || trimNewlines) && text.IndexOf('\r') != -1)
                    {
                        // Replace newlines with spaces.
                        text = text.Replace("\r\n", " ");
                    }

                    if (trimNewlines && text.Trim().Length == 0)
                    {
                        text = String.Empty;
                    }

                    _cachedInnerText = text;
                }

                if (text == null || text.Length == 0)
                {
                    text = innerTextAttribute;
                    if (text == null)
                    {
                        text = String.Empty;
                    }
                }

                return text;
            }

            set
            {
                if (!AllowMultiLines && value != null && value.IndexOf('\r') >= 0)
                {
                    throw new ArgumentException(
                        SR.GetString(SR.MobileControl_TextCannotContainNewlines, 
                                     GetType().ToString(), 
                                     UniqueID));
                }

                ViewState[InnerTextViewStateKey] = value;

                // There may be other types of child controls and they should
                // be preserved.  Removing the specific controls in backward
                // direction so they can be removed properly.
                for (int i = Controls.Count - 1; i >= 0; i--)
                {
                    Control child = Controls[i];
                    if (child is LiteralControl ||
                        child is DataBoundLiteralControl ||
                        child is DesignerDataBoundLiteralControl)
                    {
                        Controls.RemoveAt(i);
                    }
                }
                _cachedInnerText = null;
            }
        }

        // make this an internal method so designer could reuse the same logic.
        internal String GetControlText(out bool containTag, out bool containDataboundLiteral)
        {
            return GetControlText(out containTag, out containDataboundLiteral, false);
        }


        private String GetControlText(out bool containTag, out bool containDataboundLiteral, bool trim)
        {
            containTag = false;
            containDataboundLiteral = false;
            bool allowInnerMarkup = AllowInnerMarkup;
            String returnedText = null;

            // PERF: Optimized to avoid constructing builder and writer, unless
            // needed.

            StringBuilder builder = null;
            StringWriter writer = null;
            foreach (Control child in Controls)
            {
                String text;
                bool translate;

                if (child is LiteralControl)
                {
                    text = ((LiteralControl)child).Text;
                    containTag = containTag || text.IndexOf('<') != -1;
                    if (allowInnerMarkup)
                    {
                        translate = false;
                    }
                    else
                    {
                        translate = text.IndexOf('&') != -1;
                    }
                }
                else if (child is DataBoundLiteralControl)
                {
                    text = ((DataBoundLiteralControl)child).Text;
                    containDataboundLiteral = true;
                    // Databound text is not in the persistence format, and thus should not
                    // be translated.
                    translate = false;
                }
                else if (child is DesignerDataBoundLiteralControl)
                {
                    text = ((DesignerDataBoundLiteralControl)child).Text;

                    // If the DesignerDataBoundLiteralControl is not databound, we simply
                    // return empty string for the Text property.
                    if (text == null || text.Length == 0)
                    {
                        containDataboundLiteral = true;
                    }

                    // Databound text is not in the persistence format, and thus should not
                    // be translated.
                    translate = false;
                }
                else if (child is HtmlContainerControl)
                {
                    containTag = true;
                    break;
                }
                else
                {
                    continue;
                }

                if (trim)
                {
                    text = text.Trim ();
                }

                if (translate || returnedText != null)
                {
                    builder = new StringBuilder();
                    writer = new StringWriter(builder, CultureInfo.InvariantCulture);
                    if (returnedText != null)
                    {
                        writer.Write(returnedText);
                        returnedText = null;
                    }
                }

                if (writer != null)
                {
                    if (translate)
                    {
                        TranslateAndAppendText(text, writer);
                    }
                    else
                    {
                        writer.Write(text);
                    }
                }
                else
                {
                    returnedText = text;
                }
            }

            if (returnedText != null)
            {
                return returnedText;
            }
            else if (builder == null)
            {
                return String.Empty;
            }
            else
            {
                return builder.ToString();
            }
        }

        static internal void TranslateAndAppendText(String text, StringWriter writer)
        {
            // Can't quite use HtmlDecode, because HtmlDecode doesn't
            // parse &nbsp; the way we'd like it to.

            if (text.IndexOf('&') != -1)
            {
                if (text.IndexOf("&nbsp;", StringComparison.Ordinal) != -1)
                {
                    text = text.Replace("&nbsp;", "\u00A0");
                }

                HttpUtility.HtmlDecode(text, writer);
            }
            else
            {
                writer.Write(text);
            }
        }

        internal virtual bool AllowMultiLines
        {
            get
            {
                return false;
            }
        }

        internal virtual bool AllowInnerMarkup
        {
            get
            {
                return false;
            }
        }

        internal virtual bool TrimInnerText
        {
            get
            {
                return true;
            }
        }

        internal virtual bool TrimNewlines
        {
            get
            {
                return false;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.AddLinkedForms"]/*' />
        public virtual void AddLinkedForms(IList linkedForms)
        {
        }

        //  convenience method for returning a non-null string value
        internal String ToString(Object o)
        {
            if (o == null)
                return String.Empty;
            if (o is String)
                return (String)o;
            return o.ToString();
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.IsVisibleOnPage"]/*' />
        public bool IsVisibleOnPage(int pageNumber)
        {
            if (!EnablePagination)
            {
                return true;
            }
            if (FirstPage < 0 || LastPage < 0)
            {
                return true;
            }
            return pageNumber >= FirstPage && pageNumber <= LastPage;
        }

        private int _firstPage = -1;
        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.FirstPage"]/*' />
        [
            Browsable(false),
            Bindable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public int FirstPage
        {
            get
            {
                return _firstPage;
            }
            set
            {
                _firstPage = value;
            }
        }

        private int _lastPage = -1;
        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.LastPage"]/*' />
        [
            Browsable(false),
            Bindable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public int LastPage
        {
            get
            {
                return _lastPage;
            }
            set
            {
                _lastPage = value;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.VisibleWeight"]/*' />
        [
            Browsable(false),
            Bindable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public virtual int VisibleWeight
        {
            get
            {
                int weight = 0;
                if (HasControls())
                {
                    foreach (Control child in Controls)
                    {
                        if (child is MobileControl && child.Visible)
                        {
                            MobileControl control = (MobileControl)child;
                            weight += control.GetVisibleWeight();
                        }
                    }
                }
                if (weight == 0)
                {
                    weight = ControlPager.DefaultWeight;
                }
                return weight;
            }
        }

        internal int GetVisibleWeight()
        {
            int weight = Adapter.VisibleWeight;
            if (weight == ControlPager.UseDefaultWeight)
            {
                weight = VisibleWeight;
            }
            return weight;
        }

        private bool _enablePagination = true;

        internal bool EnablePagination
        {
            get
            {
                return _enablePagination;
            }
            set
            {
                _enablePagination = value;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.PaginateChildren"]/*' />
        protected virtual bool PaginateChildren
        {
            get
            {
                return true;
            }
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.PaginateRecursive"]/*' />
        public virtual void PaginateRecursive(ControlPager pager)
        {
            if (!EnablePagination)
            {
                return;
            }

            if (PaginateChildren || this.Form.ControlToPaginate == this)
            {
                int firstAssignedPage = -1;
                DoPaginateChildren(pager, this, ref firstAssignedPage);
                if (firstAssignedPage != -1)
                {
                    this.FirstPage = firstAssignedPage;
                }
                else
                {
                    this.FirstPage = pager.GetPage(GetVisibleWeight());
                }
                this.LastPage = pager.PageCount;
            }
            else
            {
                int pageNumber = pager.GetPage(GetVisibleWeight());
                SetControlPageRecursive(this, pageNumber);
                this.FirstPage = pageNumber;
                this.LastPage = pageNumber;
            }
        }

        internal static void DoPaginateChildren(ControlPager pager, Control ctl, ref int firstAssignedPage)
        {
            if (ctl.HasControls())
            {
                foreach (Control child in ctl.Controls)
                {
                    if (child.Visible)
                    {
                        MobileControl mobileCtl = child as MobileControl;
                        if (mobileCtl != null)
                        {
                            mobileCtl.PaginateRecursive(pager);
                            if (firstAssignedPage == -1)
                            {
                                firstAssignedPage = mobileCtl.FirstPage;
                            }
                        }
                        else if (child is UserControl)
                        {
                            DoPaginateChildren(pager, child, ref firstAssignedPage);
                        }
                    }
                }
            }
        }

        internal static void SetControlPageRecursive(Control control, int page)
        {
            if (control.HasControls())
            {
                foreach (Control child in control.Controls)
                {
                    MobileControl mobileChild = child as MobileControl;
                    if (mobileChild != null)
                    {
                            mobileChild.SetControlPage(page);
 
                    }
                    else 
                    {
                        SetControlPageRecursive(child, page);
                    }
                }
            }
        }

        internal static void SetEnablePaginationRecursive(Control control, bool pagination) 
        {
            if(control.HasControls()) {
                foreach(Control child in control.Controls) {
                    SetEnablePaginationRecursive(child,pagination);
                }
            }
            MobileControl mobileControl = control as MobileControl;
            if(mobileControl != null) {
                mobileControl.EnablePagination = pagination;
            }
        }

        internal virtual void SetControlPage(int page)
        {
            FirstPage = page;
            LastPage = page;
            SetControlPageRecursive(this, page);
        }

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.OnPageChange"]/*' />
        protected virtual void OnPageChange(int oldPageIndex, int newPageIndex)
        {
            MobileControl.OnPageChangeRecursive(this, oldPageIndex, newPageIndex);
        }

        private static void OnPageChangeRecursive(Control ctl, int oldPageIndex, int newPageIndex)
        {
            if (ctl.HasControls())
            {
                foreach (Control child in ctl.Controls)
                {
                    MobileControl mobileCtl = child as MobileControl;
                    if (mobileCtl != null)
                    {
                        mobileCtl.OnPageChange(oldPageIndex, newPageIndex);
                    }
                    else
                    {
                        OnPageChangeRecursive(child, oldPageIndex, newPageIndex);
                    }
                }
            }
        }

        internal bool _breakAfter = true;

        /// <include file='doc\MobileControl.uex' path='docs/doc[@for="MobileControl.BreakAfter"]/*' />
        [
            Browsable(true),
            Bindable(true),
            DefaultValue(true),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.MobileControl_BreakAfter)
        ]
        public virtual bool BreakAfter
        {
            get
            {
                return _breakAfter;
            }

            set
            {
                _breakAfter = value;
            }
        }

        // BEGIN: logic to check for infinite cycles when control trees are instantiated
        // through templates.
        
        // InstantiatingTemplate refers to template that instantiated this control
        // (or null if not instantiated by template).
        private ITemplate _instantiatingTemplate = null;
        private ITemplate InstantiatingTemplate
        {
            get
            {
                return _instantiatingTemplate;
            }
        }

        // The prospective parent of this control is passed as a parameter.  Typically this
        // control has not been added to parent.Controls, so cannot use Parent property.
        private void SetInstantiatingTemplateAndVerify (ITemplate instantiatingTemplate, MobileControl parent)
        {
            for (MobileControl c = parent; c != null; c = c.Parent as MobileControl)
            {
                if (c.InstantiatingTemplate == instantiatingTemplate)
                {
                    throw new Exception (SR.GetString(SR.MobileControl_InfiniteTemplateRecursion));
                }
            }
            _instantiatingTemplate = instantiatingTemplate;
        }
        
        // Typically target has not been added to targetParent.Controls collection yet.
        internal void CheckedInstantiateTemplate(ITemplate template, MobileControl target, MobileControl targetParent)
        {
            template.InstantiateIn (target);
            target.SetInstantiatingTemplateAndVerify (template, targetParent);
        }

        #region IAttributeAccessor implementation
        String IAttributeAccessor.GetAttribute(String name) {
            return GetAttribute(name);
        }

        void IAttributeAccessor.SetAttribute(String name, String value) {
            SetAttribute(name, value);
        }
        #endregion
    }
}
