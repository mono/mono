//------------------------------------------------------------------------------
// <copyright file="MobileTemplatedControlDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Reflection;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Converters;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.MobileControls;
    using System.Web.UI.MobileControls.Adapters;

    using WebCtrlStyle = System.Web.UI.WebControls.Style;
    using DialogResult = System.Windows.Forms.DialogResult;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal abstract class MobileTemplatedControlDesigner : TemplatedControlDesigner, IMobileDesigner, IDeviceSpecificDesigner
    {
        #if TRACE
            internal static BooleanSwitch TemplateableControlDesignerSwitch =
                new BooleanSwitch("MobileTemplatedControlDesigner", "Enable TemplateableControl designer general purpose traces.");
        #endif

        private System.Windows.Forms.Control    _header;
        private MobileControl                   _mobileControl;
        private System.Web.UI.Control           _control;
        private DesignerVerbCollection          _designerVerbs = null;
        private DeviceSpecificChoice            _currentChoice = null;
        private bool                            _containmentStatusDirty = true;
        private ContainmentStatus               _containmentStatus;
        private IDesignerHost                   _host;
        private IWebFormsDocumentService        _iWebFormsDocumentService;
        private IMobileWebFormServices          _iMobileWebFormServices;
        private const String                    _htmlString = "html";
        private TemplateEditingVerb[]           _templateVerbs;
        private bool                            _templateVerbsDirty = true;
        private const int                       _templateWidth = 275;

        private static readonly String _noChoiceText =
            SR.GetString(SR.DeviceFilter_NoChoice);

        private static readonly String _defaultChoiceText =
            SR.GetString(SR.DeviceFilter_DefaultChoice);

        private static readonly String _nonHtmlSchemaErrorMessage =
            SR.GetString(SR.MobileControl_NonHtmlSchemaErrorMessage);

        private static readonly String _illFormedWarning =
            SR.GetString(SR.TemplateFrame_IllFormedWarning);

        private const String _illFormedHtml =
            "<DIV style=\"font-family:tahoma;font-size:8pt; COLOR: infotext; BACKGROUND-COLOR: infobackground\">{0}</DIV>";

        internal const String DefaultTemplateDeviceFilter = "__TemplateDeviceFilter__";
        private const String _templateDeviceFilterPropertyName = "TemplateDeviceFilter";
        private const String _appliedDeviceFiltersPropertyName = "AppliedDeviceFilters";
        private const String _propertyOverridesPropertyName = "PropertyOverrides";
        private const String _expressionsPropertyName = "Expressions";
        private const String _defaultDeviceSpecificIdentifier = "unique";

        // used by DesignerAdapterUtil.GetMaxWidthToFit
        // and needs to be exposed in object model because
        // custom controls may need to access the value just like
        // DesignerAdapterUtil.GetMaxWidthToFit does.
        public virtual int TemplateWidth
        {
            get
            {
                return _templateWidth;
            }
        }

        public override bool AllowResize
        {
            get
            {
                // Non mobilecontrols (ie. DeviceSpecific) does not render templates, no need to resize.
                // When templates are not defined, we render a read-only fixed
                // size block. Once templates are defined or are being edited
                // the control should allow resizing.
                return InTemplateMode || (_mobileControl != null && _mobileControl.IsTemplated);
            }
        }

        private bool AllowTemplateEditing
        {
            get
            {
                return (CurrentChoice != null && IsHTMLSchema(CurrentChoice) && !ErrorMode);
            }
        }

        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            Editor(typeof(AppliedDeviceFiltersTypeEditor), typeof(UITypeEditor)),
            MergableProperty(false),
            MobileCategory("Category_DeviceSpecific"),
            MobileSysDescription(SR.MobileControl_AppliedDeviceFiltersDescription),
            ParenthesizePropertyName(true),
        ]
        protected String AppliedDeviceFilters
        {
            get
            {
                return String.Empty;
            }
        }

        protected ContainmentStatus ContainmentStatus
        {
            get
            {
                if (!_containmentStatusDirty)
                {
                    return _containmentStatus;
                }

                _containmentStatus =
                    DesignerAdapterUtil.GetContainmentStatus(_control);

                _containmentStatusDirty = false;
                return _containmentStatus;
            }
        }

        public DeviceSpecificChoice CurrentChoice
        {
            get
            {
                return _currentChoice;
            }

            set
            {
                if (_currentChoice != value)
                {
                    SetTemplateVerbsDirty();

                    _currentChoice = value;
                    OnCurrentChoiceChange();
                }
            }
        }

        public virtual DeviceSpecific CurrentDeviceSpecific
        {
            get
            {
                if (null == _mobileControl)
                {
                    return null;
                }

                return _mobileControl.DeviceSpecific;
            }
        }

        internal Object DesignTimeElementInternal
        {
            get
            {
                return typeof(HtmlControlDesigner).InvokeMember("DesignTimeElement", 
                    BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.NonPublic, 
                    null, this, null, CultureInfo.InvariantCulture);
            }
        }

        public override bool DesignTimeHtmlRequiresLoadComplete
        {
            get
            {
                return true;
            }
        }

        // 


        // Return true only when GetErrorMessage returns non-null string and
        // it is not info mode (warning only).
        protected virtual bool ErrorMode
        {
            get
            {
                bool infoMode;
                return (GetErrorMessage(out infoMode) != null && !infoMode);
            }
        }

        protected IDesignerHost Host
        {
            get
            {
                if (_host != null)
                {
                    return _host;
                }
                _host = (IDesignerHost)GetService(typeof(IDesignerHost));
                Debug.Assert(_host != null);
                return _host;
            }
        }

        protected IMobileWebFormServices IMobileWebFormServices
        {
            get
            {
                if (_iMobileWebFormServices == null)
                {
                    _iMobileWebFormServices =
                        (IMobileWebFormServices)GetService(typeof(IMobileWebFormServices));
                }

                return _iMobileWebFormServices;
            }
        }

        private IWebFormsDocumentService IWebFormsDocumentService
        {
            get
            {
                if (_iWebFormsDocumentService == null)
                {
                    _iWebFormsDocumentService =
                        (IWebFormsDocumentService)GetService(typeof(IWebFormsDocumentService));

                    Debug.Assert(_iWebFormsDocumentService != null);
                }

                return _iWebFormsDocumentService;
            }
        }

        /// <summary>
        ///     Indicates whether the initial page load is completed
        /// </summary>
        protected bool LoadComplete
        {
            get
            {
                return !IWebFormsDocumentService.IsLoading;
            }
        }

        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            Editor(typeof(PropertyOverridesTypeEditor), typeof(UITypeEditor)),
            MergableProperty(false),
            MobileCategory("Category_DeviceSpecific"),
            MobileSysDescription(SR.MobileControl_DeviceSpecificPropsDescription),
            ParenthesizePropertyName(true),
        ]
        protected String PropertyOverrides
        {
            get
            {
                return String.Empty;
            }
        }

        [
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            MobileSysDescription(SR.TemplateableDesigner_TemplateChoiceDescription),
            TypeConverter(typeof(ChoiceConverter)),
        ]
        public String TemplateDeviceFilter
        {
            get
            {
                if (null == CurrentChoice)
                {
                    return _noChoiceText;
                }
                if (CurrentChoice.Filter.Length == 0)
                {
                    return _defaultChoiceText;
                }
                else
                {
                    return DesignerUtility.ChoiceToUniqueIdentifier(CurrentChoice);
                }
            }
            set
            {
                if (String.IsNullOrEmpty(value) ||
                    value.Equals(SR.GetString(SR.DeviceFilter_NoChoice)))
                {
                    CurrentChoice = null;
                    return;
                }

                if (null == CurrentDeviceSpecific)
                {
                    return;
                }

                Debug.Assert(CurrentDeviceSpecific.Choices != null);

                foreach (DeviceSpecificChoice choice in CurrentDeviceSpecific.Choices)
                {
                    if (DesignerUtility.ChoiceToUniqueIdentifier(choice).Equals(value) ||
                        (choice.Filter.Length == 0 &&
                         value.Equals(SR.GetString(SR.DeviceFilter_DefaultChoice))))
                    {
                        CurrentChoice = choice;
                        return;
                    }
                }

                CurrentChoice = null;
            }
        }

        private bool ValidContainment
        {
            get
            {
                return (ContainmentStatus == ContainmentStatus.InForm ||
                        ContainmentStatus == ContainmentStatus.InPanel ||
                        ContainmentStatus == ContainmentStatus.InTemplateFrame);
            }
        }

        public override DesignerVerbCollection Verbs
        {
            get
            {
                if (_designerVerbs == null)
                {
                    _designerVerbs = new DesignerVerbCollection();

                    _designerVerbs.Add(new DesignerVerb(
                        SR.GetString(SR.TemplateableDesigner_SetTemplatesFilterVerb),
                        new EventHandler(this.OnSetTemplatesFilterVerb)));
                }

                _designerVerbs[0].Enabled = !this.InTemplateMode;
                return _designerVerbs;
            }
        }

        protected WebCtrlStyle WebCtrlStyle
        {
            get
            {
                WebCtrlStyle style = new WebCtrlStyle();

                if (_mobileControl != null)
                {
                    _mobileControl.Style.ApplyTo(style);
                }
                else
                {
                    Debug.Assert(_control is DeviceSpecific);
                    if (_control.Parent is Panel)
                    {
                        ((Panel)_control.Parent).Style.ApplyTo(style);
                    }
                }

                return style;
            }
        }

        [
            Conditional("DEBUG")
        ]
        private void CheckTemplateName(String templateName)
        {
            Debug.Assert (
                templateName == Constants.HeaderTemplateTag ||
                templateName == Constants.FooterTemplateTag ||
                templateName == Constants.ItemTemplateTag ||
                templateName == Constants.AlternatingItemTemplateTag ||
                templateName == Constants.SeparatorTemplateTag ||
                templateName == Constants.ItemDetailsTemplateTag ||
                templateName == Constants.ContentTemplateTag);
        }

        protected override ITemplateEditingFrame CreateTemplateEditingFrame(TemplateEditingVerb verb)
        {
            ITemplateEditingService teService =
                (ITemplateEditingService)GetService(typeof(ITemplateEditingService));
            Debug.Assert(teService != null,
                "How did we get this far without an ITemplateEditingService");

            String[] templateNames = GetTemplateFrameNames(verb.Index);
            ITemplateEditingFrame editingFrame = teService.CreateFrame(
                this,
                TemplateDeviceFilter,
                templateNames,
                WebCtrlStyle,
                null /* we don't have template styles */);

            editingFrame.InitialWidth = _templateWidth;
            return editingFrame;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeTemplateVerbs();

                if (IMobileWebFormServices != null)
                {
                    // If the page is in loading mode, it means the remove is trigged by webformdesigner.
                    if (IWebFormsDocumentService.IsLoading)
                    {
                        IMobileWebFormServices.SetCache(_control.ID, (Object) DefaultTemplateDeviceFilter, (Object) this.TemplateDeviceFilter);
                    }
                    else
                    {
                        // setting to null will remove the entry.
                        IMobileWebFormServices.SetCache(_control.ID, (Object) DefaultTemplateDeviceFilter, null);
                    }
                }
            }

            base.Dispose(disposing);
        }

        public override void OnComponentChanged(Object sender, ComponentChangedEventArgs ce)
        {
            base.OnComponentChanged(sender, ce);

            MemberDescriptor member = ce.Member;
            if (member != null &&
                member.GetType().FullName.Equals(Constants.ReflectPropertyDescriptorTypeFullName))
            {
                PropertyDescriptor propDesc = (PropertyDescriptor)member;

                switch (propDesc.Name)
                {
                    case "ID":
                    {
                        // Update the dictionary of device filters stored in the page designer
                        // setting to null will remove the entry.
                        IMobileWebFormServices.SetCache(ce.OldValue.ToString(), (Object) DefaultTemplateDeviceFilter, null);
                        break;
                    }

                    case "BackColor":
                    case "ForeColor":
                    case "Font":
                    case "StyleReference":
                    {
                        SetTemplateVerbsDirty();
                        break;
                    }
                }
            }
        }

        private void DisposeTemplateVerbs()
        {
            if (_templateVerbs != null)
            {
                for (int i = 0; i < _templateVerbs.Length; i++)
                {
                    _templateVerbs[i].Dispose();
                }

                _templateVerbs = null;
                _templateVerbsDirty = true;
            }
        }

        protected override TemplateEditingVerb[] GetCachedTemplateEditingVerbs()
        {
            if (ErrorMode)
            {
                return null;
            }

            // dispose template verbs during template editing would cause exiting from editing mode
            // without saving.
            if (_templateVerbsDirty == true && !InTemplateMode)
            {
                DisposeTemplateVerbs();

                _templateVerbs = GetTemplateVerbs();
                _templateVerbsDirty = false;
            }

            foreach(TemplateEditingVerb verb in _templateVerbs)
            {
                verb.Enabled = AllowTemplateEditing;
            }

            return _templateVerbs;
        }

        // Gets the HTML to be used for the design time representation of the control runtime.
        public sealed override String GetDesignTimeHtml()
        {
            if (!LoadComplete)
            {
                return null;
            }

            bool infoMode;
            String errorMessage = GetErrorMessage(out infoMode);
            SetStyleAttributes();

            if (null != errorMessage)
            {
                return GetDesignTimeErrorHtml(errorMessage, infoMode);
            }

            String designTimeHTML = null;

            // This is to avoiding cascading error rendering.
            try
            {
                designTimeHTML = GetDesignTimeNormalHtml();
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.ToString());
                designTimeHTML = GetDesignTimeErrorHtml(ex.Message, false);
            }

            return designTimeHTML;
        }

        protected virtual String GetDesignTimeErrorHtml(String errorMessage, bool infoMode)
        {
            return DesignerAdapterUtil.GetDesignTimeErrorHtml(
                errorMessage, infoMode, _control, Behavior, ContainmentStatus);
        }

        protected virtual String GetDesignTimeNormalHtml()
        {
            return GetEmptyDesignTimeHtml();
        }

        // We sealed this method because it will never be called
        // by our designers under current structure.
        protected override sealed String GetErrorDesignTimeHtml(Exception e)
        {
            return base.GetErrorDesignTimeHtml(e);
        }

        protected virtual String GetErrorMessage(out bool infoMode)
        {
            infoMode = false;

            if (!DesignerAdapterUtil.InMobileUserControl(_control))
            {
                if (DesignerAdapterUtil.InUserControl(_control))
                {
                    infoMode = true;
                    return MobileControlDesigner._userControlWarningMessage;
                }

                if (!DesignerAdapterUtil.InMobilePage(_control))
                {
                    return MobileControlDesigner._mobilePageErrorMessage;
                }

                if (!ValidContainment)
                {
                    return MobileControlDesigner._formPanelContainmentErrorMessage;
                }
            }

            if (CurrentChoice != null && !IsHTMLSchema(CurrentChoice))
            {
                infoMode = true;
                return _nonHtmlSchemaErrorMessage;
            }

            // Containment is valid, return null;
            return null;
        }

        /// <summary>
        ///    <para>
        ///       Gets the HTML to be persisted for the content present within the associated server control runtime.
        ///    </para>
        /// </summary>
        /// <returns>
        ///    <para>
        ///       Persistable Inner HTML.
        ///    </para>
        /// </returns>
        public override String GetPersistInnerHtml()
        {
            String persist = null;

            if (InTemplateMode)
            {
                SaveActiveTemplateEditingFrame();
            }

            if (IsDirty)
            {
                persist = MobileControlPersister.PersistInnerProperties(Component, Host);
            }

            if (InTemplateMode)
            {
                IsDirty = true;
            }

            return persist;
        }

        public override String GetTemplateContent(
            ITemplateEditingFrame editingFrame,
            String templateName,
            out bool allowEditing)
        {
            Debug.Assert(AllowTemplateEditing);
#if DEBUG
            CheckTemplateName(templateName);
#endif
            allowEditing = true;

            ITemplate template = null;
            String templateContent = String.Empty;

            // Here we trust the TemplateVerbs to give valid template names
            template = (ITemplate)CurrentChoice.Templates[templateName];

            if (template != null)
            {
                templateContent = GetTextFromTemplate(template);
                if (!IsCompleteHtml(templateContent))
                {
                    allowEditing = false;
                    templateContent = String.Format(CultureInfo.CurrentCulture, _illFormedHtml, _illFormedWarning);
                }
            }

            return templateContent;
        }

        protected abstract String[] GetTemplateFrameNames(int index);

        protected abstract TemplateEditingVerb[] GetTemplateVerbs();

        /// <summary>
        ///    <para>
        ///       Initializes the designer.
        ///    </para>
        /// </summary>
        /// <param name='component'>
        ///    The control element being designed.
        /// </param>
        /// <remarks>
        ///    <para>
        ///       This is called by the designer host to establish the component being
        ///       designed.
        ///    </para>
        /// </remarks>
        /// <seealso cref='System.ComponentModel.Design.IDesigner'/>
        public override void Initialize(IComponent component)
        {
            Debug.Assert(component is System.Web.UI.MobileControls.MobileControl ||
                         component is System.Web.UI.MobileControls.DeviceSpecific,
                         "MobileTemplatedControlDesigner.Initialize - Invalid (Mobile) Control");

            base.Initialize(component);

            if (component is System.Web.UI.MobileControls.MobileControl)
            {
                _mobileControl = (System.Web.UI.MobileControls.MobileControl) component;
            }
            // else the component is a DeviceSpecific control
            _control = (System.Web.UI.Control) component;

            if (IMobileWebFormServices != null)
            {
                this.TemplateDeviceFilter = (String) IMobileWebFormServices.GetCache(_control.ID, (Object)DefaultTemplateDeviceFilter);
            }
        }

        private bool IsCompleteHtml(String templateContent)
        {
            if (!String.IsNullOrEmpty(templateContent))
            {
                return SimpleParser.IsWellFormed(templateContent);
            }

            // if template is empty, it's always editable.
            return true;
        }

        protected bool IsHTMLSchema(DeviceSpecificChoice choice)
        {
            Debug.Assert(choice != null);

            return choice.Xmlns != null &&
                choice.Xmlns.ToLower(CultureInfo.InvariantCulture).IndexOf(_htmlString, StringComparison.Ordinal) != -1;
        }

        // Notification that is called when current choice is changed, it is currently
        // used to notify StyleSheet that template device filter is changed.
        protected virtual void OnCurrentChoiceChange()
        {
        }

        /// <summary>
        ///    <para>
        ///       Notification that is called when internal changes have been made.
        ///    </para>
        /// </summary>
        protected virtual void OnInternalChange()
        {
            ISite site = _control.Site;
            if (site != null)
            {
                IComponentChangeService changeService =
                    (IComponentChangeService)site.GetService(typeof(IComponentChangeService));
                if (changeService != null)
                {
                    try
                    {
                        changeService.OnComponentChanging(_control, null);
                    }
                    catch (CheckoutException ex)
                    {
                        if (ex == CheckoutException.Canceled)
                        {
                            return;
                        }
                        throw;
                    }
                    changeService.OnComponentChanged(_control, null, null, null);
                }
            }
        }

        /// <summary>
        ///    <para>
        ///       Notification that is called when the associated control is parented.
        ///    </para>
        /// </summary>
        public override void OnSetParent()
        {
            base.OnSetParent();

            // Template verbs may need to be refreshed
            SetTemplateVerbsDirty();

            // this needs to be set before OnLoadComplete;
            _containmentStatusDirty = true;

            if (LoadComplete)
            {
                UpdateRendering();
            }
        }

        private void OnSetTemplatesFilterVerb(Object sender, EventArgs e)
        {
            ShowTemplatingOptionsDialog();
        }

        protected override void OnTemplateModeChanged()
        {
            base.OnTemplateModeChanged();

            if (InTemplateMode)
            {
                // Set xmlns in view linked document to show HTML intrinsic
                // controls in property grid with same schema used by
                // Intellisense for current choice tag in HTML view.

                // This code won't work in Venus now since there are no viewlinks and
                // they don't support this kind of schema.
                /*
                NativeMethods.IHTMLElement htmlElement = (NativeMethods.IHTMLElement) ((IHtmlControlDesignerBehavior) Behavior).DesignTimeElementView;
                Debug.Assert(htmlElement != null,
                    "Invalid HTML element in MobileTemplateControlDesigner.OnTemplateModeChanged");
                NativeMethods.IHTMLDocument2 htmlDocument2 = (NativeMethods.IHTMLDocument2) htmlElement.GetDocument();
                Debug.Assert(htmlDocument2 != null,
                    "Invalid HTML Document2 in MobileTemplateControlDesigner.OnTemplateModeChanged");
                NativeMethods.IHTMLElement htmlBody = (NativeMethods.IHTMLElement) htmlDocument2.GetBody();
                Debug.Assert(htmlBody != null,
                    "Invalid HTML Body in MobileTemplateControlDesigner.OnTemplateModeChanged");
                htmlBody.SetAttribute("xmlns", (Object) CurrentChoice.Xmlns, 0);
                */
            }
        }

        protected override void PreFilterProperties(IDictionary properties)
        {
            base.PreFilterProperties(properties);

            // DesignTime Property only, we will use this to select choices.
            properties[_templateDeviceFilterPropertyName] =
                TypeDescriptor.CreateProperty(this.GetType(),
                _templateDeviceFilterPropertyName,
                typeof(String),
                new DefaultValueAttribute(SR.GetString(SR.DeviceFilter_NoChoice)),
                MobileCategoryAttribute.Design,
                InTemplateMode ? BrowsableAttribute.No : BrowsableAttribute.Yes
                );

            // design time only entry used to display dialog box used to create choices.
            properties[_appliedDeviceFiltersPropertyName] =
                TypeDescriptor.CreateProperty(this.GetType(),
                _appliedDeviceFiltersPropertyName,
                typeof(String),
                InTemplateMode? BrowsableAttribute.No : BrowsableAttribute.Yes
                );

            // design time only entry used to display dialog box to create choices.
            properties[_propertyOverridesPropertyName] =
                TypeDescriptor.CreateProperty(this.GetType(),
                _propertyOverridesPropertyName,
                typeof(String),
                InTemplateMode? BrowsableAttribute.No : BrowsableAttribute.Yes
                );

            PropertyDescriptor property = (PropertyDescriptor) properties[_expressionsPropertyName];
            if (property != null) {
                properties[_expressionsPropertyName] = TypeDescriptor.CreateProperty(this.GetType(), property, BrowsableAttribute.No);
            }
        }

        protected virtual void SetStyleAttributes()
        {
            Debug.Assert(Behavior != null);
            DesignerAdapterUtil.SetStandardStyleAttributes(Behavior, ContainmentStatus);
        }

        public override void SetTemplateContent(
            ITemplateEditingFrame editingFrame,
            String templateName,
            String templateContent)
        {
            Debug.Assert(AllowTemplateEditing);

            // Debug build only checking
            CheckTemplateName(templateName);

            ITemplate template = null;

            if ((templateContent != null) && (templateContent.Length != 0))
            {
                template = GetTemplateFromText(templateContent);
            }
            else
            {
                CurrentChoice.Templates.Remove(templateName);
                return;
            }

            // Here we trust the TemplateVerbs to give valid template names
            CurrentChoice.Templates[templateName] = template;
        }

        protected internal void SetTemplateVerbsDirty()
        {
            _templateVerbsDirty = true;
        }

        protected virtual void ShowTemplatingOptionsDialog()
        {
            IComponentChangeService changeService =
                (IComponentChangeService)GetService(typeof(IComponentChangeService));
            if (changeService != null)
            {
                try
                {
                    changeService.OnComponentChanging(_control, null);
                }
                catch (CheckoutException ex)
                {
                    if (ex == CheckoutException.Canceled)
                    {
                        return;
                    }
                    throw;
                }
            }

            try
            {
                TemplatingOptionsDialog dialog = new TemplatingOptionsDialog(
                    this,
                    _control.Site,
                    MobileControlDesigner.MergingContextTemplates);
                dialog.ShowDialog();
            }
            finally
            {
                if (changeService != null)
                {
                    changeService.OnComponentChanged(_control, null, null, null);

                    if (IMobileWebFormServices != null)
                    {
                        IMobileWebFormServices.ClearUndoStack();
                    }
                }
            }
        }

        public void UpdateRendering()
        {
            if (!(null == _mobileControl || _mobileControl is StyleSheet))
            {
                _mobileControl.RefreshStyle();
            }

            // template editing frame need to be recreated because the style
            // (WebCtrlStyle) to use may have to change
            SetTemplateVerbsDirty();

            if (!InTemplateMode)
            {
                UpdateDesignTimeHtml();
            }
        }

        ////////////////////////////////////////////////////////////////////////
        //  Begin IDeviceSpecificDesigner Implementation
        ////////////////////////////////////////////////////////////////////////

        void IDeviceSpecificDesigner.SetDeviceSpecificEditor
            (IRefreshableDeviceSpecificEditor editor)
        {
        }

        String IDeviceSpecificDesigner.CurrentDeviceSpecificID
        {
            get
            {
                return _defaultDeviceSpecificIdentifier;
            }
        }

        System.Windows.Forms.Control IDeviceSpecificDesigner.Header
        {
            get
            {
                return _header;
            }
        }

        System.Web.UI.Control IDeviceSpecificDesigner.UnderlyingControl
        {
            get
            {
                return _control;
            }
        }

        Object IDeviceSpecificDesigner.UnderlyingObject
        {
            get
            {
                return _control;
            }
        }

        void IDeviceSpecificDesigner.InitHeader(int mergingContext)
        {
            HeaderPanel panel = new HeaderPanel();
            HeaderLabel lblDescription = new HeaderLabel();

            lblDescription.TabIndex = 0;
            lblDescription.Height = 24;
            lblDescription.Width = 204;
            panel.Height = 28;
            panel.Width = 204;
            panel.Controls.Add(lblDescription);

            switch (mergingContext)
            {
                case MobileControlDesigner.MergingContextTemplates:
                {
                    lblDescription.Text = SR.GetString(SR.TemplateableDesigner_SettingTemplatingChoiceDescription);
                    break;
                }

                default:
                {
                    lblDescription.Text = SR.GetString(SR.TemplateableDesigner_SettingGenericChoiceDescription);
                    break;
                }
            }

            _header = panel;
        }

        void IDeviceSpecificDesigner.RefreshHeader(int mergingContext)
        {
        }

        bool IDeviceSpecificDesigner.GetDeviceSpecific(String deviceSpecificParentID, out DeviceSpecific ds)
        {
            Debug.Assert(_defaultDeviceSpecificIdentifier == deviceSpecificParentID);
            ds = CurrentDeviceSpecific;
            return true;
        }

        void IDeviceSpecificDesigner.SetDeviceSpecific(String deviceSpecificParentID, DeviceSpecific ds)
        {
            Debug.Assert(_defaultDeviceSpecificIdentifier == deviceSpecificParentID);

            if (_mobileControl != null)
            {
                if (null != ds)
                {
                    ds.SetOwner(_mobileControl);
                }
                _mobileControl.DeviceSpecific = ds;
            }
            else if (_control != null && ds == null)
            {
                Debug.Assert(_control is DeviceSpecific);

                // Clear the choices if it is a DeviceSpecific control.
                ((DeviceSpecific)_control).Choices.Clear();
            }

            if (null != CurrentChoice)
            {
                if (null == ds)
                {
                    CurrentChoice = null;
                }
                else
                {
                    // This makes sure that the CurrentChoice value is set to null if
                    // it was deleted during the deviceSpecific object editing
                    if (CurrentChoice.Filter.Length == 0)
                    {
                        TemplateDeviceFilter = SR.GetString(SR.DeviceFilter_DefaultChoice);
                    }
                    else
                    {
                        TemplateDeviceFilter = DesignerUtility.ChoiceToUniqueIdentifier(CurrentChoice);
                    }
                }
            }
        }

        void IDeviceSpecificDesigner.UseCurrentDeviceSpecificID()
        {
        }

        ////////////////////////////////////////////////////////////////////////
        //  End IDeviceSpecificDesigner Implementation
        ////////////////////////////////////////////////////////////////////////

        // Hack : Internal class used to provide TemplateContainerAttribute for Templates.
        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class TemplateContainer
        {
            [
                TemplateContainer(typeof(MobileListItem))
            ]
            internal ITemplate HeaderTemplate
            {
                get {return null;}
            }

            [
                TemplateContainer(typeof(MobileListItem))
            ]
            internal ITemplate FooterTemplate
            {
                get {return null;}
            }

            [
                TemplateContainer(typeof(MobileListItem))
            ]
            internal ITemplate ItemTemplate
            {
                get {return null;}
            }

            [
                TemplateContainer(typeof(MobileListItem))
            ]
            internal ITemplate AlternatingItemTemplate
            {
                get {return null;}
            }

            [
                TemplateContainer(typeof(MobileListItem))
            ]
            internal ITemplate SeparatorTemplate
            {
                get {return null;}
            }

            [
                TemplateContainer(typeof(MobileListItem))
            ]
            internal ITemplate ContentTemplate
            {
                get {return null;}
            }

            [
                TemplateContainer(typeof(MobileListItem))
            ]
            internal ITemplate LabelTemplate
            {
                get {return null;}
            }

            [
                TemplateContainer(typeof(MobileListItem))
            ]
            internal ITemplate ItemDetailsTemplate
            {
                get {return null;}
            }
        }
    }
}
