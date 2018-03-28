//------------------------------------------------------------------------------
// <copyright file="StyleSheet.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Globalization;
using System.Web;
using System.Web.Mobile;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.Util;
using System.Reflection;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile StyleSheet class.
     */
    /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet"]/*' />
    [
        ControlBuilderAttribute(typeof(StyleSheetControlBuilder)),
        Designer(typeof(System.Web.UI.Design.MobileControls.StyleSheetDesigner)),
        Editor(typeof(System.Web.UI.Design.MobileControls.StyleSheetComponentEditor),
            typeof(ComponentEditor)),
        ToolboxData("<{0}:StyleSheet runat=\"server\"></{0}:StyleSheet>"),
        ToolboxItem(typeof(System.Web.UI.Design.WebControlToolboxItem))
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class StyleSheet : MobileControl
    {
        private readonly static StyleSheet _default = new StyleSheet();
        private StyleCollection _styles = new StyleCollection();
        private StyleSheet _externalStyleSheet;
        private ArrayList _duplicateStyles = new ArrayList();
        private StyleSheet _referrer;
        private String _resolvedPath;
        private bool _saveAll = false;

        static StyleSheet()
        {
            //  initialize default stylesheet
            {
                Style title = new Style();
                title.Bold = BooleanOption.True;
                title.Font.Size = FontSize.Large;
                title.Name = "title";
                _default.AddParsedSubObject(title);

                Style error = new Style();
                error.ForeColor = Color.Red;
                error.Name = Constants.ErrorStyle;
                _default.AddParsedSubObject(error);

                Style subCommand = new Style();
                subCommand.Font.Size = FontSize.Small;
                subCommand.Name = "subcommand";
                subCommand.Alignment = Alignment.Center;
                _default.AddParsedSubObject(subCommand);
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.Default"]/*' />
        public static StyleSheet Default
        {
            get
            {
                return _default;
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.AddParsedSubObject"]/*' />
        protected override void AddParsedSubObject(Object o)
        {
            if (o is Style)
            {
                Style style = (Style)o;
                style.SetControl(this);
                String name = style.Name;
                if (String.IsNullOrEmpty(name)) {
                    throw new Exception(
                        SR.GetString(SR.StyleSheet_MustContainID));
                }
                // Remember any duplicate styles we encounter.  Validate()
                // will throw if this list is not empty.
                String lowerName = name.ToLower(CultureInfo.InvariantCulture);
                if (_styles[lowerName] != null)
                {
                    _duplicateStyles.Add(style);
                }
                else
                {
                    // Do not overwrite hash table with duplicate style or we
                    // will loose the overwritten style in the designer.
                    this[name] = style;
                }
            }
            else
            {
                base.AddParsedSubObject(o);
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.this"]/*' />
        public Style this[String name]
        {
            get
            {
                Style style = (Style)_styles[name.ToLower(CultureInfo.InvariantCulture)];
                if (style == null && ExternalStyleSheet != null)
                {
                    style = _externalStyleSheet[name];
                }
                return style;
            }
            set
            {
                if (String.IsNullOrEmpty(name)) {
                    throw new ArgumentException(SR.GetString(SR.Style_EmptyName));
                }

                if (!String.Equals(name, value.Name, StringComparison.OrdinalIgnoreCase)) {
                    // If the Style doesn't yet have a name, assign it the one that
                    // it's being set as in the Stylesheet.
                    Debug.Assert(value.Name != null);

                    if (value.Name.Length == 0) {
                        value.Name = name;
                    }
                    else {
                        throw new ArgumentException(
                            SR.GetString(SR.StyleSheet_InvalidStyleName,
                                         value.Name, name));
                    }
                }

                if (value.Control == null)
                {
                    // Necessary for programmatically generated styles...  need
                    // to have their stylesheet set when inserted.
                    value.SetControl(this);
                }
                else if (value.Control != this && MobilePage != null && !MobilePage.DesignMode)
                {
                    // In the rare event that someone tries to share a
                    // style between stylesheets.  (Don't do this check in
                    // design mode, as they do share styles, although in a very
                    // careful way that doesn't cause problems.)
                    throw new Exception(
                        SR.GetString(SR.StyleSheet_StyleAlreadyOwned,
                                     value.Name,
                                     value.Control.ID));
                }

                String lowerName = name.ToLower(CultureInfo.InvariantCulture);

                if (_styles[lowerName] != null)
                {
                    _saveAll = true;
                }
                else if (IsTrackingViewState)
                {
                    value.SetDirty();
                    ((IStateManager)value).TrackViewState();
                }
                
                _styles[lowerName] = value;
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.Styles"]/*' />
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public ICollection Styles
        {
            get
            {
                return (ICollection)_styles;
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.Remove"]/*' />
        public void Remove(String name)
        {
            _saveAll = true;
            if (!_styles.Remove (name))
            {
                throw new ArgumentException(SR.GetString(SR.Style_StyleNotFound, name));
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.ReferencePath"]/*' />
        [
            Bindable(true),
            DefaultValue(""),
            Editor(typeof(System.Web.UI.Design.MobileControls.StyleSheetRefUrlEditor), typeof(UITypeEditor)),
            MobileCategory(SR.Category_Behavior),
            MobileSysDescription(SR.StyleSheet_ReferencePath)
        ]
        public String ReferencePath
        {
            get
            {
                String s = (String)ViewState["ReferencePath"];
                return s != null ? s : String.Empty;
            }
            set
            {
                ViewState["ReferencePath"] = value;
                _externalStyleSheet = null;
            }
        }

        private StyleSheet ExternalStyleSheet
        {
            get
            {
                // If page is null then this is the default stylesheet
                if (Page == null || MobilePage.DesignMode)
                {
                    return null;
                }

                if (_externalStyleSheet == null && ReferencePath.Length > 0)
                {
                    // Should load relative to parent template control (which 
                    // may be a page or a user control).

                    TemplateControl parent = (TemplateControl)Parent;

                    // First check if there are any circular references.

                    String resolvedPath = UrlPath.Combine(parent.TemplateSourceDirectory,
                                                          ReferencePath);
                    for (StyleSheet ss = Referrer; ss != null; ss = ss.Referrer)
                    {
                        if (ss.ResolvedPath != null &&
                                String.Compare(ss.ResolvedPath, resolvedPath, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            throw new Exception(SR.GetString(SR.StyleSheet_LoopReference, 
                                                             ResolvedPath));
                        }
                    }

                    Control control = parent.LoadControl(ReferencePath);

                    // By adding it as a child of this control, the user
                    // control returned above is instantiated and its
                    // children are accessible below.
                    Controls.Add(control);

                    foreach (Control child in control.Controls)
                    {
                        _externalStyleSheet = child as StyleSheet;
                        if (_externalStyleSheet != null)
                        {
                            break;
                        }
                    }
                    if (_externalStyleSheet == null)
                    {
                        throw new Exception(
                            SR.GetString(SR.StyleSheet_NoStyleSheetInExternalFile));
                    }

                    _externalStyleSheet.ResolvedPath = resolvedPath;
                    _externalStyleSheet.Referrer = this;
                }
                return _externalStyleSheet;
            }
        }

        internal override void ApplyDeviceSpecifics()
        {
            // Apply our own device specifics first

            base.ApplyDeviceSpecifics();

            // Iterate over the Styles, invoking ApplyProperties on each Style's
            // DeviceSpecific.

            foreach (String key in _styles.Keys)
            {
                Style style = _styles[key];
                if (style.DeviceSpecific != null)
                {
                    style.DeviceSpecific.ApplyProperties();
                }
            }
        }

        private StyleSheet Referrer
        {
            get
            {
                return _referrer;
            }

            set
            {
                _referrer = value;
            }
        }

        private String ResolvedPath
        {
            get
            {
                return _resolvedPath;
            }

            set
            {
                _resolvedPath = value;
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.TrackViewState"]/*' />
        protected override void TrackViewState()
        {
            for (int i = 0; i < _styles.Count; i++)
            {
                ((IStateManager)_styles.GetAt(i)).TrackViewState();
            }
            base.TrackViewState();
        }

        internal override bool RequiresForm
        {
            get
            {
                return false;
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.SaveViewState"]/*' />
        protected override Object SaveViewState()
        {
            Object baseState = base.SaveViewState();

            int styleCount = _styles.Count;
            Object[] stylesState = new Object[styleCount];

            if (_saveAll)
            {
                for (int i = 0; i < styleCount; i++)
                {
                    Style style = (Style)_styles.GetAt(i);
                    style.SetDirty();
                    stylesState[i] = ((IStateManager)style).SaveViewState();
                }
            }
            else
            {
                bool anySaved = false;
                for (int i = 0; i < styleCount; i++)
                {
                    Style style = (Style)_styles.GetAt(i);
                    stylesState[i] = ((IStateManager)style).SaveViewState();
                    if (stylesState[i] != null)
                    {
                        anySaved = true;
                    }
                }

                if (!anySaved)
                {
                    stylesState = null;
                }
            }

            if (stylesState != null || baseState != null)
            {
                return new Object[] { baseState, _saveAll, stylesState };
            }
            else
            {
                return null;
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.LoadViewState"]/*' />
        protected override void LoadViewState(Object savedState)
        {
            if (savedState != null)
            {
                Object[] o = (Object[])savedState;
                base.LoadViewState(o[0]);

                if (o[2] != null)
                {
                    bool savedAll = (bool)o[1];
                    Object[] stylesState = (Object[])o[2];

                    if (savedAll)
                    {
                        _saveAll = true;
                        _styles.Clear();
                    }

                    int stylesCount = _styles.Count;
                    for (int i = 0; i < stylesState.Length; i++)
                    {
                        if (i >= stylesCount)
                        {
                            Style style = new Style();
                            IStateManager styleStateMgr = (IStateManager)style;

                            styleStateMgr.LoadViewState(stylesState[i]);
                            styleStateMgr.TrackViewState();
                            style.SetControl(this);
                            _styles[style.Name.ToLower(CultureInfo.InvariantCulture)] = style;
                        }
                        else if (stylesState != null)
                        {
                            ((IStateManager)_styles.GetAt(i)).LoadViewState(stylesState[i]);
                        }
                    }
                }
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  BEGIN DESIGNER SUPPORT
        /////////////////////////////////////////////////////////////////////////

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.Clear"]/*' />
        public void Clear()
        {
            _styles.Clear();
        }

        // Do not expose the Visible property in the Designer
        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.Visible"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public override bool Visible 
        {
            get
            {
                // 

                return base.Visible;
            }
            set
            {
                base.Visible = value;
            }
        }

        // Do not expose the EnableViewState property in the Designer
        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.EnableViewState"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public override bool EnableViewState
        {
            get
            {
                return base.EnableViewState;
            }
            set
            {
                base.EnableViewState = value;
            }
        }


        /////////////////////////////////////////////////////////////////////////
        //  END DESIGNER SUPPORT
        /////////////////////////////////////////////////////////////////////////

        /////////////////////////////////////////////////////////////////////////
        //  BEGIN STYLE PROPERTIES
        /////////////////////////////////////////////////////////////////////////

        // Style properties are not applicable in the stylesheet control

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.StyleReference"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override String StyleReference
        {
            get
            {
                if (MobilePage.DesignMode)
                {
                    return String.Empty;
                }
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotAccessible, "StyleReference"));
            }
            set
            {
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotSettable, "StyleReference"));
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.Font"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override FontInfo Font
        {
            get
            {
                // The checking if MobilePage null is necessary because Font is a
                // expandable property, the property browser still check the inner
                // Font properties although it is not browsable. When designer is
                // first loaded, MobilePage is still unassigned.
                if (MobilePage == null || MobilePage.DesignMode)
                {
                    return null;
                }
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotAccessible, "Font"));
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.Alignment"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override Alignment Alignment
        {
            get
            {
                if (MobilePage.DesignMode)
                {
                    return Alignment.NotSet;
                }
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotAccessible, "Alignment"));
            }
            set
            {
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotSettable, "Alignment"));
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.Wrapping"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override Wrapping Wrapping
        {
            get
            {
                if (MobilePage.DesignMode)
                {
                    return Wrapping.NotSet;
                }
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotAccessible, "Wrapping"));
            }
            set
            {
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotSettable, "Wrapping"));
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.ForeColor"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override Color ForeColor
        {
            get
            {
                if (MobilePage.DesignMode)
                {
                    return Color.Empty;
                }
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotAccessible, "ForeColor"));
            }
            set
            {
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotSettable, "ForeColor"));
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.BackColor"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never),
        ]
        public override Color BackColor
        {
            get
            {
                if (MobilePage.DesignMode)
                {
                    return Color.Empty;
                }
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotAccessible, "BackColor"));
            }
            set
            {
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotSettable, "BackColor"));
            }
        }

        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheet.BreakAfter"]/*' />
        [
            Bindable(false),
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
            EditorBrowsable(EditorBrowsableState.Never),
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
                    SR.GetString(SR.StyleSheet_PropertyNotAccessible, "BreakAfter"));
            }
            set
            {
                throw new Exception(
                    SR.GetString(SR.StyleSheet_PropertyNotSettable, "BreakAfter"));
            }
        }

        internal ICollection DuplicateStyles
        {
            get
            {
                return _duplicateStyles;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  END STYLE PROPERTIES
        /////////////////////////////////////////////////////////////////////////

        /*
         * Private style collection class, that allows by-name or by-index access to
         * a set of styles.
         */

        private class StyleCollection : NameObjectCollectionBase
        {
            public Style this[String name]
            {
                get
                {
                    return (Style)BaseGet(name);
                }

                set
                {
                    BaseSet(name, value);
                }
            }

            public Style GetAt(int i)
            {
                return (Style)BaseGet(i);
            }

            public bool Remove(String name)
            {
                if (this[name] == null)
                {
                    return false;
                }
                else
                {
                    this[name].SetControl(null);
                    BaseRemove(name);
                    return true;
                }
            }

            public void Clear()
            {
                BaseClear();
            }
            


        }
    }

    /*
     * StyleSheet Control builder.
     *
     * Copyright (c) 2000 Microsoft Corporation
     */
    /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheetControlBuilder"]/*' />
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class StyleSheetControlBuilder : MobileControlBuilder
    {
        /// <include file='doc\StyleSheet.uex' path='docs/doc[@for="StyleSheetControlBuilder.GetChildControlType"]/*' />
        public override Type GetChildControlType(String name, IDictionary attributes) 
        {
            String lowerCaseName = name.ToLower(CultureInfo.InvariantCulture);

            if (lowerCaseName.EndsWith(":style", StringComparison.Ordinal))
            {
                return typeof(Style);
            }

            // Any extender to Style *MUST* be added in here to be recognized in
            // a StyleSheet.
            // NOTE: Currently no way for third party extenders to add their
            // own styles.  They'll need to specify complete name and
            // runat=server. 

            Type type = null;
            switch (lowerCaseName)
            {
              case "style":
                if(InDesigner)
                {
                    // Indicate to the designer that it needs to add a prefix.
                    System.Web.UI.Design.MobileControls.StyleSheetDesigner.SetRequiresDesignTimeChanges();
                }
                type = typeof(Style);
                break;

              case "pagerstyle":
                type = typeof(PagerStyle);
                break;

              default:
                type = base.GetChildControlType(name, attributes);
                break;
            }

            return type;
        }
    }

}
