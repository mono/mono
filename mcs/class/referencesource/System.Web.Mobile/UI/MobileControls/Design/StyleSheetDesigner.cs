//------------------------------------------------------------------------------
// <copyright file="StyleSheetDesigner.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.Design.MobileControls
{
    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing.Design;
    using System.Globalization;
    using System.Text;
    using System.Web.UI;
    using System.Web.UI.Design;
    using System.Web.UI.Design.MobileControls.Adapters;
    using System.Web.UI.Design.MobileControls.Converters;
    using System.Web.UI.Design.MobileControls.Util;
    using System.Web.UI.MobileControls;
    using System.Windows.Forms;

    using Control = System.Web.UI.Control;
    using DataBindingCollectionEditor = System.Web.UI.Design.DataBindingCollectionEditor;

    [
        System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
        Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
    ]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    internal class StyleSheetDesigner : MobileTemplatedControlDesigner, IDeviceSpecificDesigner
    {
        internal static BooleanSwitch StyleSheetDesignerSwitch =
            new BooleanSwitch("StyleSheetDesigner", "Enable StyleSheet designer general purpose traces.");

        private IWebFormsDocumentService _iWebFormsDocumentService;
        private IRefreshableDeviceSpecificEditor _deviceSpecificEditor;
        private DesignerVerbCollection _designerVerbs;
        private System.Web.UI.MobileControls.StyleSheet _styleSheet;
        private Style _currentStyle, _tmpCurrentStyle;
        private bool _isDuplicate;
        private MergedUI _mergedUI = null;
        private ArrayList _cycledStyles = null;
        private const int _templateWidth = 300;
        private static bool _requiresDesignTimeChanges = false;
        private bool _shouldRepersistStyles = false;
        private EventHandler _loadComplete = null;

        private const String _templatesStylePropName = "TemplateStyle";
        private const String _persistedStylesPropName = "PersistedStyles";

        private const String _designTimeHTML =
            @"
                <table cellpadding=4 cellspacing=0 width='300px' style='font-family:tahoma;font-size:8pt;color:buttontext;background-color:buttonface;border: solid 1px;border-top-color:buttonhighlight;border-left-color:buttonhighlight;border-bottom-color:buttonshadow;border-right-color:buttonshadow'>
                  <tr><td colspan=2><span style='font-weight:bold'>StyleSheet</span> - {0}</td></tr>
                  <tr><td style='padding-top:0;padding-bottom:0;width:55%;padding-left:10px;font-weight:bold'>Template Style:</td><td style='padding-top:0;padding-bottom:0'>{1}</td></tr>
                  <tr><td style='padding-top:0;padding-bottom:0;width:55%;padding-left:10px;font-weight:bold'>Template Device Filter:</td><td style='padding-top:0;padding-bottom:0'>{2}</td></tr>
                  <tr><td colspan=2 style='padding-top:4px'>{3}</td></tr>
                </table>
             ";

        private const String _specialCaseDesignTimeHTML =
            @"
                <table cellpadding=4 cellspacing=0 width='300px' style='font-family:tahoma;font-size:8pt;color:buttontext;background-color:buttonface;border: solid 1px;border-top-color:buttonhighlight;border-left-color:buttonhighlight;border-bottom-color:buttonshadow;border-right-color:buttonshadow'>
                  <tr><td colspan=2><span style='font-weight:bold'>StyleSheet</span> - {0}</td></tr>
                  <tr><td style='padding-top:0;padding-bottom:0;width:55%;padding-left:10px;font-weight:bold'>Template Style:</td><td style='padding-top:0;padding-bottom:0'>{1}</td></tr>
                  <tr><td style='padding-top:0;padding-bottom:0;width:55%;padding-left:10px;font-weight:bold'>Template Device Filter:</td><td style='padding-top:0;padding-bottom:0'>{2}</td></tr>
                  <tr><td colspan=2 style='padding-top:4px'>{3}</td></tr>
                  <tr><td colspan=2>
                    <table style='font-size:8pt;color:window;background-color:ButtonShadow'>
                      <tr><td valign='top'><img src='{4}'/></td><td>{5}</td></tr>
                    </table>
                  </td></tr>
                </table>
             ";

        private const int _headerFooterTemplates            = 0;
        private const int _itemTemplates                    = 1;
        private const int _separatorTemplate                = 2;
        private const int _contentTemplate                  = 3;
        private const int _numberOfTemplateFrames           = 4;

        private static readonly String[][] _templateFrameNames =
            new String[][] {
                               new String [] { Constants.HeaderTemplateTag, Constants.FooterTemplateTag },
                               new String [] { Constants.ItemTemplateTag, Constants.AlternatingItemTemplateTag, Constants.ItemDetailsTemplateTag },
                               new String [] { Constants.SeparatorTemplateTag },
                               new String [] { Constants.ContentTemplateTag }
                           };

        private const String _templateStyle = "__TemplateStyle__";

        // used by DesignerAdapterUtil.GetMaxWidthToFit
        // and needs to be exposed in object model because
        // custom controls may need to access the value just like
        // DesignerAdapterUtil.GetMaxWidthToFit does.
        public override int TemplateWidth
        {
            get
            {
                return _templateWidth;
            }
        }

        private MobilePage MobilePage
        {
            get
            {
                IComponent component = DesignerAdapterUtil.GetRootComponent(Component);
                if (component is MobileUserControl)
                {
                    return ((Control)component).Page as MobilePage;
                }
                return component as MobilePage;
            }
        }

        private Control RootControl
        {
            get
            {
                IComponent component = DesignerAdapterUtil.GetRootComponent(Component);
                return component as Control;
            }
        }

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
            Debug.Assert(component is System.Web.UI.MobileControls.StyleSheet,
                         "StyleSheetDesigner.Initialize - Invalid StyleSheet Control");
            base.Initialize(component);

            _isDuplicate = false;
            _styleSheet = (System.Web.UI.MobileControls.StyleSheet) component;
            if(_requiresDesignTimeChanges)
            {
                _shouldRepersistStyles = true;
            }
            _loadComplete = new EventHandler(this.OnLoadComplete);
            IWebFormsDocumentService.LoadComplete += _loadComplete;

            if (IMobileWebFormServices != null)
            {
                TemplateStyle = (String) IMobileWebFormServices.GetCache(_styleSheet.ID, _templateStyle);
                TemplateDeviceFilter = 
                    (String) IMobileWebFormServices.GetCache(
                    _styleSheet.ID,
                    MobileTemplatedControlDesigner.DefaultTemplateDeviceFilter);
            }
        }

        private void OnLoadComplete(Object source, EventArgs e) 
        {
            if(_shouldRepersistStyles)
            {
                IsDirty = true;
                OnInternalChange();
            }
            _requiresDesignTimeChanges = false;
            _shouldRepersistStyles = false;

            UpdateDesignTimeHtml();
        }
        
        internal static void SetRequiresDesignTimeChanges()
        {
            _requiresDesignTimeChanges = true;
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
        
        protected override ITemplateEditingFrame CreateTemplateEditingFrame(TemplateEditingVerb verb)
        {
            ITemplateEditingService teService = 
                (ITemplateEditingService)GetService(typeof(ITemplateEditingService));
            Debug.Assert(teService != null,
                "How did we get this far without an ITemplateEditingService");

            String[] templateNames = GetTemplateFrameNames(verb.Index);
            ITemplateEditingFrame editingFrame = teService.CreateFrame(
                this, 
                TemplateDeviceFilter + " (" + TemplateStyle + ")",
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
                UpdateActiveStyleSheet();
                
                if (_loadComplete != null)
                {
                    IWebFormsDocumentService.LoadComplete -= _loadComplete;
                    _loadComplete = null;
                }

                if (IMobileWebFormServices != null)
                {
                    // If the page is in loading mode, it means the remove is trigged by webformdesigner.
                    if (!LoadComplete)
                    {
                        IMobileWebFormServices.SetCache(_styleSheet.ID, (Object) _templateStyle, (Object) this.TemplateStyle);
                    }
                    else
                    {
                        // setting to null will remove the entry.
                        IMobileWebFormServices.SetCache(_styleSheet.ID, (Object) _templateStyle, null);
                    }
                }
            }

            base.Dispose(disposing);
        }

        private void UpdateActiveStyleSheet()
        {
            if (MobilePage != null && MobilePage.StyleSheet == _styleSheet)
            {
                IDesigner designer = null;

                // currently active stylesheet is deleted
                MobilePage.StyleSheet = StyleSheet.Default;
                StyleSheet _newStyleSheet = null;

                Debug.Assert(RootControl != null);
                foreach (Control control in RootControl.Controls)
                {
                    // Find new stylesheet
                    if (control is StyleSheet && _newStyleSheet == null && control != _styleSheet)
                    {
                        designer = Host.GetDesigner((IComponent) control);
                        // AUI 7285
                        if (designer != null)
                        {
                            _newStyleSheet = (StyleSheet) control;
                        }
                    }
                }

                MobilePage.StyleSheet = _newStyleSheet;
                if (null != _newStyleSheet)
                {
                    Debug.Assert(designer != null);
                    StyleSheetDesigner ssd = designer as StyleSheetDesigner;
                    Debug.Assert(ssd != null, "ssd is null in StyleSheetDesigner");
                    ssd.TreatAsDuplicate(false);
                }
                RefreshPageView();
            }
        }
        
        protected override String[] GetTemplateFrameNames(int index)
        {
            Debug.Assert(index >= 0 & index <= _templateFrameNames.Length);
            return _templateFrameNames[index];
        }

        protected override TemplateEditingVerb[] GetTemplateVerbs()
        {
            TemplateEditingVerb[] templateVerbs = new TemplateEditingVerb[_numberOfTemplateFrames];

            templateVerbs[_headerFooterTemplates] = new TemplateEditingVerb(
                SR.GetString(SR.TemplateFrame_HeaderFooterTemplates),
                _headerFooterTemplates,
                this);
            templateVerbs[_itemTemplates] = new TemplateEditingVerb(
                SR.GetString(SR.TemplateFrame_ItemTemplates),
                _itemTemplates,
                this);
            templateVerbs[_separatorTemplate] = new TemplateEditingVerb(
                SR.GetString(SR.TemplateFrame_SeparatorTemplate),
                _separatorTemplate,
                this);
            templateVerbs[_contentTemplate] = new TemplateEditingVerb(
                SR.GetString(SR.TemplateFrame_ContentTemplate),
                _contentTemplate,
                this);

            return templateVerbs;
        }

        /// <summary>
        ///    <para>
        ///       Delegate to handle component changed event.
        ///    </para>
        /// </summary>
        /// <param name='sender'>
        ///    The object sending the event.
        /// </param>
        /// <param name='ce'>
        ///    The event object used when firing a component changed notification.
        /// </param>
        /// <remarks>
        ///    <para>
        ///       This is called after a property has been changed. It allows the implementor
        ///       to do any post-processing that may be needed after a property change.
        ///    </para>
        /// </remarks>
        public override void OnComponentChanged(Object sender, ComponentChangedEventArgs ce) 
        {
            // Delegate to the base class implementation first!
            base.OnComponentChanged(sender, ce);

            MemberDescriptor member = ce.Member;
            if (member != null && 
                member.GetType().FullName.Equals(Constants.ReflectPropertyDescriptorTypeFullName))
            {
                PropertyDescriptor propDesc = (PropertyDescriptor)member;
                
                if (propDesc.Name.Equals("ID"))
                {
                    // Update the dictionary of device filters stored in the page designer
                    // setting to null will remove the entry.
                    IMobileWebFormServices.SetCache(ce.OldValue.ToString(), (Object) _templateStyle, null);
                }
            }
        }

        internal void OnStylesChanged()
        {
            // If this is not a new stylesheet and it is the current stylesheet
            if (MobilePage != null && MobilePage.StyleSheet == _styleSheet)
            {
                // Refresh the whole page assuming styles have been changed.
                RefreshPageView();
                ClearCycledStyles();
            }
        }

        private void RefreshPageView()
        {
            if (IMobileWebFormServices != null)
            {
                IMobileWebFormServices.RefreshPageView();
            }
        }

        public override void OnSetParent() 
        {
            base.OnSetParent();

            // This is not a MobilePage or the styleSheet is already the active styleSheet.
            // The latter happens when the active StyleSheet is drag/drop to another location
            // which forces its parent to be changed.
            if (MobilePage == null)
            {
                return;
            }

            if (MobilePage.StyleSheet == _styleSheet)
            {
                if (!(_styleSheet.Parent is MobilePage
                    || _styleSheet.Parent is MobileUserControl))
                {
                    UpdateActiveStyleSheet();
                }
                return;
            }

            if (MobilePage.StyleSheet != StyleSheet.Default) 
            {
                // can't accept more than 1 stylesheet
                TreatAsDuplicate(true);

                // the current valid StyleSheet is intentionaly refreshed because
                // if this stylesheet instance is recreated via a Undo operation
                // the current valid StyleSheet appears as a duplicate if not refreshed.
                IDesigner designer = Host.GetDesigner((IComponent) MobilePage.StyleSheet);
                Debug.Assert(designer != null, "designer is null in StyleSheetDesigner");
                StyleSheetDesigner ssd = (StyleSheetDesigner) designer;
                ssd.UpdateRendering();
            }
            else if (_styleSheet.Parent is MobilePage ||
                     _styleSheet.Parent is MobileUserControl)
            {
                // the active stylesheet is changed
                MobilePage.StyleSheet = _styleSheet;
                _isDuplicate = false;
            }
            RefreshPageView();
        }

        protected override void OnTemplateModeChanged() 
        {
            base.OnTemplateModeChanged();

            // Refresh all mobilecontrols after exit template editing mode.
            if (!InTemplateMode)
            {
                RefreshPageView();
            }        
        }

        public void TreatAsDuplicate(bool isDuplicate)
        {
            if (isDuplicate != _isDuplicate)
            {
                _isDuplicate = isDuplicate;
                SetTemplateVerbsDirty();
                UpdateDesignTimeHtml();
            }
        }

        protected override bool ErrorMode
        {
            get
            {
                return base.ErrorMode
                    || _isDuplicate
                    || _styleSheet.DuplicateStyles.Count > 0;
            }
        }

        private StringCollection GetDuplicateStyleNames()
        {
            StringCollection duplicateNamesList = new StringCollection();

            // Filter out repeated duplicate names using case insensitive
            // hash table
            HybridDictionary duplicateNamesHash = new HybridDictionary(
                true /* Names not case sensitive */ );
            foreach(Style style in _styleSheet.DuplicateStyles)
            {
                duplicateNamesHash[style.Name] = true;
            }

            // Copy remaining names into a string list
            foreach(DictionaryEntry entry in duplicateNamesHash)
            {
                duplicateNamesList.Add((String)entry.Key);
            }
            return duplicateNamesList;
        }

        protected override String GetDesignTimeNormalHtml()
        {
            String curStyle, message;
            ArrayList lstStylesInCycle = null;

            if (null == CurrentStyle)
            {
                curStyle = SR.GetString(SR.StyleSheet_PropNotSet);
            }
            else
            {
                curStyle = HttpUtility.HtmlEncode(CurrentStyle.Name);
            }

            String curChoice;

            if (null == CurrentChoice)
            {
                curChoice = SR.GetString(SR.StyleSheet_PropNotSet);
            }
            else
            {
                if (CurrentChoice.Filter.Length == 0)
                {
                    curChoice = SR.GetString(SR.DeviceFilter_DefaultChoice);
                }
                else
                {
                    curChoice = HttpUtility.HtmlEncode(DesignerUtility.ChoiceToUniqueIdentifier(CurrentChoice));
                }
            }

            message = SR.GetString(SR.StyleSheet_DefaultMessage);

            bool renderErrorMsg = false;
            String errorMsg = null;
            String errorIconUrl = null;

            if(_isDuplicate)
            {
                renderErrorMsg = true;
                errorMsg = SR.GetString(SR.StyleSheet_DuplicateWarningMessage);
                errorIconUrl = MobileControlDesigner.errorIcon;
            }
            else if(_styleSheet.DuplicateStyles.Count > 0)
            {
                renderErrorMsg = true;
                errorMsg = SR.GetString(
                    SR.StyleSheet_DuplicateStyleNamesMessage,
                    GenericUI.BuildCommaDelimitedList(
                        GetDuplicateStyleNames()
                    )
                );
                errorIconUrl = MobileControlDesigner.errorIcon;
            }
            else if (null != CurrentStyle && null != CurrentChoice)
            {
                if (IsHTMLSchema(CurrentChoice))
                {
                    message = SR.GetString(SR.StyleSheet_TemplateEditingMessage);
                }
                else
                {
                    // User has selected non-html schema
                    renderErrorMsg = true;
                    errorMsg = SR.GetString(SR.MobileControl_NonHtmlSchemaErrorMessage);
                    errorIconUrl = MobileControlDesigner.infoIcon;
                }
            }

            if (renderErrorMsg)
            {
                Debug.Assert(errorMsg != null && errorIconUrl != null);
                return String.Format(CultureInfo.CurrentCulture, _specialCaseDesignTimeHTML, 
                    new Object[]
                                     {
                                         _styleSheet.Site.Name,
                                         curStyle,
                                         curChoice,
                                         message,
                                         errorIconUrl,
                                         errorMsg
                                     });
            }
            else
            {
                lstStylesInCycle = DetectCycles();

                // 

                if (lstStylesInCycle != null && lstStylesInCycle.Count > 0)
                {
                    String cycledStyles = String.Empty;
                    // 

                    foreach (Object obj in lstStylesInCycle)
                    {
                        Style cycledStyle = (Style) obj;
                        if (cycledStyles.Length > 0)
                        {
                            cycledStyles += ", ";  
                        }
                        cycledStyles += cycledStyle.Name;
                    }
                    return String.Format(CultureInfo.CurrentCulture, _specialCaseDesignTimeHTML, 
                        new Object[]
                                         {
                                             _styleSheet.Site.Name,
                                             curStyle,
                                             curChoice,
                                             message,
                                             MobileControlDesigner.errorIcon,
                                             SR.GetString(SR.StyleSheet_RefCycleErrorMessage, cycledStyles)
                                         });
                }
                else
                {
                    return String.Format(CultureInfo.CurrentCulture, _designTimeHTML, 
                        new Object[]
                                         {
                                             _styleSheet.Site.Name,
                                             curStyle,
                                             curChoice,
                                             message
                                         });
                }
            }
        }

        private void ClearCycledStyles()
        {
            _cycledStyles = null;
        }

/* O(n) algorithm for loop detection
        private HybridDictionary DetectCycles()
        {
            if (_cycledStyles == null)
            {
                _cycledStyles = new HybridDictionary();
                ICollection styles = _styleSheet.Styles;

                // Initialize the set
                Hashtable styleSet = new Hashtable(styles.Count);
                foreach (String key in styles)
                {
                    styleSet.Add(key, true);
                }

                while (styleSet.Count > 0)
                {
                    Style style = null;
                    foreach (String key in styleSet.Keys)
                    {
                        style = (Style)_styleSheet[key];
                        Debug.Assert(style != null);
                        break;
                    }

                    int count = 0;
                    Traverse(styleSet, style, count);
                }
            }
            return _cycledStyles;
        }
        
        private bool Traverse(Hashtable styleSet, Style style)
        {
            String reference = style.StyleReference;
            Style nextStyle = null;
            bool result = false;

            styleSet.Remove(style.Name.ToLower(CultureInfo.InvariantCulture));

            if (reference == null || reference.Length == 0 || 
                ((nextStyle = (Style)_styleSheet[reference]) == null) ||
                (!styleSet.Contains(nextStyle)))
            {
                result = false;
            }
            else if (_cycledStyles.Contains(nextStyle) || 
                Traverse(styleSet, nextStyle, ++count))
            {
                Debug.Assert(_cycledStyles != null);
                if (!_cycledStyles.Contains(style))
                {
                    _cycledStyles.Add(style, "");
                }
                result = true;
            }
            
            return result;
        }
*/
        private ArrayList DetectCycles()
        {
            if (_cycledStyles == null)
            {
                _cycledStyles = new ArrayList();
                ICollection styles = _styleSheet.Styles;

                foreach (String key in styles)
                {
                    Style style = (Style) _styleSheet[key];
                    Style styleTmp;
                    Debug.Assert(style != null);

                    bool cycle = false;
                    String reference = style.StyleReference;
                    String name = style.Name;

                    int count = styles.Count + 1;

                    while ((reference != null && reference.Length > 0) && count > 0)
                    {
                        if (0 == String.Compare(name, reference, StringComparison.OrdinalIgnoreCase))
                        {
                            cycle = true;
                            break;
                        }
                        else
                        {
                            styleTmp = _styleSheet[reference];
                            if (null != styleTmp)
                            {
                                reference = styleTmp.StyleReference;
                                count --;
                            }
                            else
                            {
                                reference = null;
                            }
                        }
                    }

                    if (cycle)
                    {
                        _cycledStyles.Add(style);
                    }
                }
            }

            return _cycledStyles;
        }

        ////////////////////////////////////////////////////////////////////////
        //  Begin IDeviceSpecificDesigner Implementation
        ////////////////////////////////////////////////////////////////////////

        void IDeviceSpecificDesigner.SetDeviceSpecificEditor
            (IRefreshableDeviceSpecificEditor editor)
        {
            _deviceSpecificEditor = editor;
        }

        String IDeviceSpecificDesigner.CurrentDeviceSpecificID
        {
            get
            {
                if (_tmpCurrentStyle == null)
                {
                    return null;
                }

                if (_styleSheet[_tmpCurrentStyle.Name] == null)
                {
                    _tmpCurrentStyle = null;
                }
                return (_tmpCurrentStyle != null) ? _tmpCurrentStyle.Name.ToLower(CultureInfo.InvariantCulture) : null;
            }
        }

        System.Windows.Forms.Control IDeviceSpecificDesigner.Header
        {
            get
            {
                return _mergedUI;
            }
        }

        System.Web.UI.Control IDeviceSpecificDesigner.UnderlyingControl
        {
            get
            {
                return _styleSheet;
            }
        }

        Object IDeviceSpecificDesigner.UnderlyingObject
        {
            get
            {
                if (null != _mergedUI.CbStyles.SelectedItem)
                {
                    String styleName = (String) _mergedUI.CbStyles.SelectedItem;
                    return _styleSheet[styleName];
                }
                else
                {
                    return null;
                }
            }
        }

        bool IDeviceSpecificDesigner.GetDeviceSpecific(String deviceSpecificParentID, out DeviceSpecific ds)
        {
            Style style = (Style) _styleSheet[deviceSpecificParentID];
            if (null == style)
            {
                ds = null;
                return false;
            }
            else
            {
                ds = style.DeviceSpecific;
                return true;
            }
        }

        void IDeviceSpecificDesigner.SetDeviceSpecific(String deviceSpecificParentID, DeviceSpecific ds)
        {
            Style style = (Style) _styleSheet[deviceSpecificParentID];
            Debug.Assert(null != style, "style is null in IDeviceSpecificDesigner.SetDeviceSpecific");
            if (null != ds)
            {
                ds.SetOwner((MobileControl) _styleSheet);
            }
            style.DeviceSpecific = ds;

            if (CurrentChoice != null && 0 == String.Compare(CurrentStyle.Name, deviceSpecificParentID, StringComparison.OrdinalIgnoreCase))
            {
                if (ds == null)
                {
                    CurrentChoice = null;
                }
                else
                {
                    // This makes sure that the CurrentChoice value is set to null is
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

        void IDeviceSpecificDesigner.InitHeader(int mergingContext)
        {
            _mergedUI = new MergedUI();
            _mergedUI.LblStyles.Text = SR.GetString(SR.StyleSheet_StylesCaption);
            _mergedUI.LblStyles.TabIndex = 1;

            _mergedUI.CbStyles.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            _mergedUI.CbStyles.SelectedIndexChanged += new EventHandler(this.OnSelectedIndexChangedStylesComboBox);
            _mergedUI.CbStyles.TabIndex = 2;
            _mergedUI.CbStyles.Sorted = true;

            _mergedUI.BtnEdit.Text = SR.GetString(SR.Stylesheet_EditBtnCaption);
            _mergedUI.BtnEdit.Click += new EventHandler(this.OnClickEditStylesButton);
            _mergedUI.BtnEdit.TabIndex = 3;

            switch (mergingContext)
            {
                case MobileControlDesigner.MergingContextTemplates:
                {
                    _mergedUI.LblHeader.Text = SR.GetString(SR.StyleSheet_SettingTemplatingStyleChoiceDescription);

                    // AUI 2730
                    _mergedUI.CbStyles.Width = 195;
                    _mergedUI.BtnEdit.Location = new System.Drawing.Point(201, 39);
                    break;
                }

                default:
                {
                    _mergedUI.LblHeader.Text = SR.GetString(SR.StyleSheet_SettingGenericStyleChoiceDescription);

                    // AUI 2730
                    _mergedUI.CbStyles.Width = 195;
                    _mergedUI.BtnEdit.Location = new System.Drawing.Point(201, 39);
                    break;
                }
            }
        }

        void IDeviceSpecificDesigner.RefreshHeader(int mergingContext)
        {
            _mergedUI.CbStyles.Items.Clear();
            ICollection styles = _styleSheet.Styles;
            foreach (String key in styles)
            {
                Style style = (Style) _styleSheet[key];
                Debug.Assert(style != null);

                _mergedUI.CbStyles.Items.Add(style.Name);
            }

            if (_mergedUI.CbStyles.Items.Count > 0)
            {
                Debug.Assert(null != CurrentStyle);
                _mergedUI.CbStyles.SelectedItem = CurrentStyle.Name;
                _oldSelectedIndex = _mergedUI.CbStyles.SelectedIndex;
            }

            _mergedUI.CbStyles.Enabled = (_mergedUI.CbStyles.Items.Count > 0);
        }

        void IDeviceSpecificDesigner.UseCurrentDeviceSpecificID()
        {
            if (CurrentStyle != _tmpCurrentStyle)
            {
                CurrentChoice = null;
                CurrentStyle = _tmpCurrentStyle;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  End IDeviceSpecificDesigner Implementation
        /////////////////////////////////////////////////////////////////////////

        private int _oldSelectedIndex;
        
        private void OnSelectedIndexChangedStylesComboBox(Object source, EventArgs e) 
        {
            if (_mergedUI.CbStyles.SelectedIndex != _oldSelectedIndex
                && !_deviceSpecificEditor.RequestRefresh())
            {
                // User needs to correct error before editing a new style.
                _mergedUI.CbStyles.SelectedIndex = _oldSelectedIndex;
                return;
            }
            
            if (_mergedUI.CbStyles.SelectedIndex >= 0)
            {
                _tmpCurrentStyle = (Style) _styleSheet[((String) _mergedUI.CbStyles.SelectedItem).ToLower(CultureInfo.InvariantCulture)];
                _deviceSpecificEditor.Refresh((String) _mergedUI.CbStyles.SelectedItem, _tmpCurrentStyle.DeviceSpecific);
            }
            _oldSelectedIndex = _mergedUI.CbStyles.SelectedIndex;
        }

        private void OnStyleRenamedInEditor(Object source, StyleRenamedEventArgs e)
        {
            _deviceSpecificEditor.DeviceSpecificRenamed(e.OldName, e.NewName);
        }

        private void OnStyleDeletedInEditor(Object source, StyleDeletedEventArgs e)
        {
            _deviceSpecificEditor.DeviceSpecificDeleted(e.Name);
        }

        private void OnClickEditStylesButton(Object source, EventArgs e)
        {

            StylesEditorDialog dialog;
            
            try
            {
                dialog = new StylesEditorDialog(
                    _styleSheet,
                    this,
                    (null != _tmpCurrentStyle) ? _tmpCurrentStyle.Name : null
                );
            }
            catch(ArgumentException ex)
            {
                Debug.Fail(ex.ToString());
                // Block user from entering StylesEditorDialog until they fix
                // duplicate style declarations.
                return;
            }
            
            StylesEditorDialog.StyleRenamedEventHandler renameHandler =
                new StylesEditorDialog.StyleRenamedEventHandler(OnStyleRenamedInEditor);
            StylesEditorDialog.StyleDeletedEventHandler deleteHandler =
                new StylesEditorDialog.StyleDeletedEventHandler(OnStyleDeletedInEditor);
            dialog.StyleRenamed += renameHandler;
            dialog.StyleDeleted += deleteHandler;
            try
            {
                _deviceSpecificEditor.BeginExternalDeviceSpecificEdit();
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    _deviceSpecificEditor.EndExternalDeviceSpecificEdit(
                        true /* commit changes */ );
                    OnInternalChange();

                    ((IDeviceSpecificDesigner) this).RefreshHeader(0);
                    // using mergingContext 0 because this implementation does not use the param.
                    if (_mergedUI.CbStyles.Items.Count == 0)
                    {
                        _deviceSpecificEditor.Refresh(null, null); // force the clean up and 
                        // disabling of the filter controls.
                        _tmpCurrentStyle = null;
                    }

                    _deviceSpecificEditor.UnderlyingObjectsChanged();
                }
                else
                {
                    _deviceSpecificEditor.EndExternalDeviceSpecificEdit(
                        false /* do not commit changes */ );
                }
            }
            finally
            {
               dialog.StyleRenamed -= renameHandler;
               dialog.StyleDeleted -= deleteHandler;
            }
        }

        public Style CurrentStyle
        {
           get
           {
               if (null == _currentStyle)
               {
                   // Since this property is registered to property window (from TemplateStyle),
                   // it will be accessed even before Initialize is called. In that case, 
                   // _styleSheet will be null;
                   if (_styleSheet != null && _styleSheet.Styles.Count > 0)
                   {
                       // how else can you get an entry in the Styles hashtable?
                       // this needs to be fixed once we use an ordered list of styles.
                       ICollection styles = _styleSheet.Styles;
                       foreach (String key in styles)
                       {
                           _currentStyle = (Style) _styleSheet[key];
                           Debug.Assert (_currentStyle != null);
                           break;
                       }
                   }
               }
               return _currentStyle;
           }
           set
           {
               _currentStyle = value;
           }
        }

        public override DeviceSpecific CurrentDeviceSpecific
        {
            get
            {
                if (null == CurrentStyle)
                {
                    return null;
                }

                return CurrentStyle.DeviceSpecific;
            }
        }

        public String TemplateStyle
        {
            get
            {
                if (null == CurrentStyle)
                {
                    return SR.GetString(SR.StyleSheet_PropNotSet);
                }
                return CurrentStyle.Name;
            }
            set
            {
                // Clear DeviceSpecificChoice of previously selected Style
                CurrentChoice = null;
                CurrentStyle = null;
                if (!String.IsNullOrEmpty(value) &&
                    !value.Equals(SR.GetString(SR.StyleSheet_PropNotSet)))
                {
                    ICollection styles = _styleSheet.Styles;
                    foreach (String key in styles)
                    {
                        Style style = (Style) _styleSheet[key];
                        if (style.Name.Equals(value))
                        {
                            CurrentStyle = style;
                            break;
                        }
                    }
                }
                // Clear DeviceSpecificChoice of currently selected Style
                CurrentChoice = null;

                // Invalidate the type descriptor so that the TemplateDeviceFilter gets updated
                TypeDescriptor.Refresh(Component);
            }
        }

        protected override void SetStyleAttributes()
        {
            Debug.Assert(Behavior != null, "Behavior is null");

            String marginTop = null, marginBottom = null, marginRight = null;

            if (ContainmentStatus == ContainmentStatus.AtTopLevel)
            {
                marginTop = "5px";
                marginBottom = "5px";
                marginRight = "30%";
            }
            else
            {
                marginTop = "3px";
                marginBottom = "3px";
                marginRight = "5px";
            }

            Behavior.SetStyleAttribute("marginTop", true, marginTop, true);
            Behavior.SetStyleAttribute("marginBottom", true, marginBottom, true);
            Behavior.SetStyleAttribute("marginRight", true, marginRight, true);
            Behavior.SetStyleAttribute("marginLeft", true, "5px", true);
        }

        protected override void PreFilterProperties(IDictionary properties) 
        {
            base.PreFilterProperties(properties);

            // DesignTime Property only, we will use this to select the current style.
            PropertyDescriptor designerTemplateStyleProp;

            designerTemplateStyleProp =
                TypeDescriptor.CreateProperty(this.GetType(), _templatesStylePropName, typeof(String),
                                     DesignerSerializationVisibilityAttribute.Hidden,
                                     MobileCategoryAttribute.Design,
                                     InTemplateMode ? ReadOnlyAttribute.Yes : ReadOnlyAttribute.No,
                                     InTemplateMode ? BrowsableAttribute.No : BrowsableAttribute.Yes,
                                     new DefaultValueAttribute(SR.GetString(SR.StyleSheet_PropNotSet)),
                                     new TypeConverterAttribute(typeof(StyleConverter)),
                                     new DescriptionAttribute(SR.GetString(SR.StyleSheet_TemplateStyleDescription)));
            properties[_templatesStylePropName] = designerTemplateStyleProp;

            PropertyDescriptor designerPersistedStyles;

            designerPersistedStyles =
                TypeDescriptor.CreateProperty(this.GetType(), _persistedStylesPropName, typeof(ICollection),
                                     //PersistenceTypeAttribute.InnerChild,
                                     PersistenceModeAttribute.InnerDefaultProperty,
                                     BrowsableAttribute.No);
            properties[_persistedStylesPropName] = designerPersistedStyles;
        }

        public ICollection PersistedStyles
        {
            get
            {
                Debug.Assert(null != _styleSheet, "_styleSheet is null");
                ICollection styleKeys = _styleSheet.Styles;
                ArrayList persistedStyles = new ArrayList();
                foreach (String key in styleKeys)
                {
                    Style style = _styleSheet[key];
                    persistedStyles.Add(style);
                }
                foreach (Style style in _styleSheet.DuplicateStyles)
                {
                    persistedStyles.Add(style);
                }
                return persistedStyles;
            }
        }

        /// <summary>
        ///    <para>
        ///       The designer's collection of verbs.
        ///    </para>
        /// </summary>
        /// <value>
        ///    <para>
        ///       An array of type <see cref='DesignerVerb'/> containing the verbs available to the
        ///       designer.
        ///    </para>
        /// </value>
        public override DesignerVerbCollection Verbs 
        {
            get 
            {
                if (_designerVerbs == null) 
                {
                    _designerVerbs = base.Verbs;
                    _designerVerbs.Add(new DesignerVerb(SR.GetString(SR.StyleSheet_StylesEditorVerb),
                                                        new EventHandler(this.OnShowStylesEditor)));
                }
                Debug.Assert(_designerVerbs.Count == 2);

                _designerVerbs[0].Enabled = !this.InTemplateMode;
                _designerVerbs[1].Enabled = !this.InTemplateMode;
                return _designerVerbs;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  BEGIN STYLE DESIGNER EVENTHANDLERS
        /////////////////////////////////////////////////////////////////////////

        protected void OnShowStylesEditor(Object sender, EventArgs e)
        {
            IComponentChangeService changeService = null;

            changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
            if (changeService != null) 
            {
                try 
                {
                    changeService.OnComponentChanging(_styleSheet, null);
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

            DialogResult result = DialogResult.Cancel;
            try 
            {
                StylesEditorDialog dialog = new StylesEditorDialog(_styleSheet, this, null);
                result = dialog.ShowDialog();
            }
            catch(ArgumentException ex)
            {
                Debug.Fail(ex.ToString());
                // Block user from entering StylesEditorDialog until they fix
                // duplicate style declarations.
            }
            finally
            {
                if (changeService != null)
                {
                    changeService.OnComponentChanged(_styleSheet, null, null, null);

                    if (IMobileWebFormServices != null)
                    {
                        IMobileWebFormServices.ClearUndoStack();
                    }
                }
            }
        }

        protected override void OnCurrentChoiceChange()
        {
            SetCurrentChoice();
            RefreshPageView();
        }

        private void SetCurrentChoice()
        {
            if (CurrentStyle != null && CurrentStyle.DeviceSpecific != null)
            {
                this.CurrentStyle.DeviceSpecific.SetDesignerChoice(CurrentChoice);
            }
        }

        private bool ValidContainment
        {
            get
            {
                return (ContainmentStatus == ContainmentStatus.AtTopLevel);
            }
        }

        protected override String GetErrorMessage(out bool infoMode)
        {
            infoMode = false;

            if (!DesignerAdapterUtil.InMobileUserControl(_styleSheet))
            {
                if (DesignerAdapterUtil.InUserControl(_styleSheet))
                {
                    infoMode = true;
                    return MobileControlDesigner._userControlWarningMessage;
                }

                if (!DesignerAdapterUtil.InMobilePage(_styleSheet))
                {
                    return MobileControlDesigner._mobilePageErrorMessage;
                }
            }
            
            if (!ValidContainment)
            {
                return MobileControlDesigner._topPageContainmentErrorMessage;
            }

            // No error condition, return null;
            return null;
        }

        /////////////////////////////////////////////////////////////////////////
        //  END STYLE DESIGNER EVENTHANDLERS
        /////////////////////////////////////////////////////////////////////////

        [
            System.Security.Permissions.SecurityPermission(System.Security.Permissions.SecurityAction.Demand,
            Flags=System.Security.Permissions.SecurityPermissionFlag.UnmanagedCode)
        ]
        private class MergedUI : HeaderPanel
        {
            internal System.Windows.Forms.Label LblStyles;
            internal System.Windows.Forms.ComboBox CbStyles;
            internal System.Windows.Forms.Button BtnEdit;
            internal HeaderLabel LblHeader;

            internal MergedUI()
            {
                this.LblStyles = new System.Windows.Forms.Label();
                this.CbStyles = new System.Windows.Forms.ComboBox();
                this.BtnEdit = new System.Windows.Forms.Button();
                this.LblHeader = new HeaderLabel();
//                this.LblStyles.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
//                    | System.Windows.Forms.AnchorStyles.Right);
                this.LblStyles.Location = new System.Drawing.Point(0, 24);
                this.LblStyles.Size = new System.Drawing.Size(160, 16);
//                this.CbStyles.Anchor = ((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
//                    | System.Windows.Forms.AnchorStyles.Right);
                this.CbStyles.DropDownWidth = 124;
                this.CbStyles.Location = new System.Drawing.Point(0, 40);
                this.CbStyles.Size = new System.Drawing.Size(160, 21);
//                this.BtnEdit.Anchor = (System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right);
                this.BtnEdit.Location = new System.Drawing.Point(164, 39);
                this.BtnEdit.Size = new System.Drawing.Size(75, 23);
                this.LblHeader.Location = new System.Drawing.Point(0, 0);
                this.LblHeader.Size = new System.Drawing.Size(240, 16);
                this.Controls.AddRange(new System.Windows.Forms.Control[] {this.CbStyles,
                                                                           this.LblStyles,
                                                                           this.BtnEdit,
                                                                           this.LblHeader});
                this.Size = new System.Drawing.Size(240, 70);
                this.Location = new System.Drawing.Point(5,6);
            }
        }
    }
}
