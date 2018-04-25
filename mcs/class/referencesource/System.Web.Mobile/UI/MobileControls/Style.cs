//------------------------------------------------------------------------------
// <copyright file="Style.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Design;
using System.Diagnostics;
using System.Globalization;
using System.Web;
using System.Web.UI;
using System.Web.UI.Design.WebControls;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

using WebCtrlStyle = System.Web.UI.WebControls.Style;
using System.Security.Permissions;

namespace System.Web.UI.MobileControls
{

    /*
     * Mobile Style class.
     * This class can be used to define external styles that can be referenced by other controls.
     */
    /// <include file='doc\Style.uex' path='docs/doc[@for="Style"]/*' />
    [
        ControlBuilderAttribute(typeof(MobileControlBuilder)),
        TypeConverterAttribute(typeof(ExpandableObjectConverter))
    ]
    [AspNetHostingPermission(SecurityAction.LinkDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [AspNetHostingPermission(SecurityAction.InheritanceDemand, Level=AspNetHostingPermissionLevel.Minimal)]
    [Obsolete("The System.Web.Mobile.dll assembly has been deprecated and should no longer be used. For information about how to develop ASP.NET mobile applications, see http://go.microsoft.com/fwlink/?LinkId=157231.")]
    public class Style : IParserAccessor, ITemplateable, IStateManager, ICloneable
    {
        //  registers styles and retrieves keys used for storing properties in internal hashtable
        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.AlignmentKey"]/*' />
        public static readonly Object
            AlignmentKey = RegisterStyle("Alignment", typeof(Alignment)    , System.Web.UI.MobileControls.Alignment.NotSet, true),
            WrappingKey  = RegisterStyle("Wrapping" , typeof(Wrapping)     , System.Web.UI.MobileControls.Wrapping.NotSet , true),
            BoldKey      = RegisterStyle("Bold",      typeof(BooleanOption), BooleanOption.NotSet, true),
            ItalicKey    = RegisterStyle("Italic",    typeof(BooleanOption), BooleanOption.NotSet, true),
            FontSizeKey  = RegisterStyle("FontSize" , typeof(FontSize)     , System.Web.UI.MobileControls.FontSize.NotSet , true),
            FontNameKey  = RegisterStyle("FontName" , typeof(String)       , String.Empty    , true),
            ForeColorKey = RegisterStyle("ForeColor", typeof(Color)        , Color.Empty     , true),
            BackColorKey = RegisterStyle("BackColor", typeof(Color)        , Color.Empty     , false);

        private bool _marked = false;       //  used by IStateManager
        private MobileControl _control;     //  containing control
        private Style _referredStyle;       //  referred style
        private bool _checkedStyleReference;//  referred style is valid.
        private StateBag _state;            //  name => object pairs
        private DeviceSpecific _deviceSpecific;
        private FontInfo _font;
        private Style _cachedParentStyle;

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.Style"]/*' />
        public Style()
        {
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.Clone"]/*' />
        public object Clone()
        {
            Style clone = new Style();
            foreach(String key in State.Keys)
            {
                clone.State[key] = State[key];
            }
            clone._control = _control;
            return clone;
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.ApplyTo"]/*' />
        public void ApplyTo(WebControl control)
        {
            control.Font.Name = (String)this[FontNameKey, true];
            switch ((FontSize)this[FontSizeKey, true])
            {
                case FontSize.Small:
                    control.Font.Size = FontUnit.Small;
                    break;
                case FontSize.Large:
                    control.Font.Size = FontUnit.Large;
                    break;
                default:
                    control.Font.Size = FontUnit.Medium;
                    break;
            }
            control.Font.Bold   = ((BooleanOption)this[BoldKey, true]) == BooleanOption.True;
            control.Font.Italic = ((BooleanOption)this[ItalicKey, true]) == BooleanOption.True;
            control.ForeColor   = (Color)this[ForeColorKey, true];
            control.BackColor   = (Color)this[BackColorKey, true];
        }

        internal void ApplyTo(WebCtrlStyle style)
        {
            style.Font.Bold     = ((BooleanOption)this[BoldKey, true]) == BooleanOption.True;
            style.Font.Italic   = ((BooleanOption)this[ItalicKey, true]) == BooleanOption.True;
            style.Font.Name     = (String)this[FontNameKey, true];
            style.ForeColor     = (Color)this[ForeColorKey, true];
            style.BackColor     = (Color)this[BackColorKey, true];

            switch ((FontSize)this[FontSizeKey, true])
            {
                case FontSize.Large :
                    style.Font.Size = FontUnit.Larger;
                    break;

                case FontSize.Small :
                    style.Font.Size = FontUnit.Smaller;
                    break;

                default :
                    break;
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.State"]/*' />
        protected internal StateBag State
        {
            get
            {
                if (_state == null)
                {
                    _state = new StateBag();
                    if (((IStateManager)this).IsTrackingViewState)
                    {
                        ((IStateManager)_state).TrackViewState();
                    }
                }

                return _state;
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.IStateManager.IsTrackingViewState"]/*' />
        /// <internalonly/>
        protected bool IsTrackingViewState
        {
            get
            {
                return _marked;
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.IStateManager.TrackViewState"]/*' />
        /// <internalonly/>
        protected void TrackViewState()
        {
            _marked = true;
            if (_state != null)
            {
                ((IStateManager)_state).TrackViewState();
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.IStateManager.SaveViewState"]/*' />
        /// <internalonly/>
        protected Object SaveViewState()
        {
            return _state != null ? ((IStateManager)_state).SaveViewState() : null;
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.IStateManager.LoadViewState"]/*' />
        /// <internalonly/>
        protected void LoadViewState(Object state)
        {
            if (state != null)
            {
                // mark _referredStyle as dirty to force reload from viewstate.
                Refresh();
                
                ((IStateManager)State).LoadViewState(state);
            }
        }

        internal void SetDirty()
        {
            // VSWHIDBEY 236464. The bag needs to be set dirty not individual items.
            State.SetDirty(true);
/*            
            foreach (StateItem item in State.Values)
            {
                item.IsDirty = true;
            }
*/
        }

        internal void Refresh()
        {
            _referredStyle = null;
            _checkedStyleReference = false;
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.Control"]/*' />
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public MobileControl Control
        {
            get
            {
                return _control;
            }
        }

        internal void SetControl(MobileControl control)
        {
            _control = control;
            _cachedParentStyle = null;
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.Name"]/*' />
        [
            Browsable(false),
            DefaultValue(""),
            MobileSysDescription(SR.Style_Name),
            NotifyParentProperty(true),
        ]
        public String Name
        {
            get
            {
                String name = (String)State["Name"];
                return name != null ? name : String.Empty;
            }
            set
            {
                State["Name"] = value;
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.StyleReference"]/*' />
        [
            Bindable(false),
            DefaultValue(null),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Style_Reference),
            NotifyParentProperty(true),
            TypeConverter(typeof(System.Web.UI.Design.MobileControls.Converters.StyleReferenceConverter)),
        ]
        public virtual String StyleReference
        {
            get
            {
                return (String)State["StyleReference"];
            }
            set
            {
                State["StyleReference"] = value;
                Refresh();      // mark referred style as dirty
            }
        }

        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        internal Style ReferredStyle
        {
            get
            {
                if (_checkedStyleReference)
                {
                    return _referredStyle;
                }

                _checkedStyleReference = true;
                String reference = StyleReference;

                if (String.IsNullOrEmpty(reference))
                {
                    _referredStyle = null;
                    return null;
                }

                Debug.Assert(_referredStyle == null ||
                             String.Equals(reference, _referredStyle.Name, StringComparison.OrdinalIgnoreCase), 
                             "Inconsistency in style names - _referredStyle must be dirty.");
                
                if (_referredStyle == null)
                {
                    // Look in the stylesheet in the nearest templated control
                    TemplateControl nearestTemplatedControl =
                        _control.FindContainingTemplateControl();

                    StyleSheet stylesheet = null;
                    MobilePage mobilePage = nearestTemplatedControl as MobilePage;
                    if (mobilePage != null)
                    {
                        stylesheet = mobilePage.StyleSheet;
                    }
                    else
                    {
                        MobileUserControl mobileUserControl =
                            nearestTemplatedControl as MobileUserControl;
                        if (mobileUserControl != null)
                        {
                            stylesheet = mobileUserControl.StyleSheet;

                            // If stylesheet is in mobileUserControl at designtime, 
                            // simply use the one in MobilePage
                            if (_control.MobilePage.DesignMode)
                            {
                                Debug.Assert(stylesheet == StyleSheet.Default);
                                stylesheet = _control.MobilePage.StyleSheet;
                            }
                        }

                        // Stylesheets won't be recognized in regular user
                        // controls.
                    }

                    if (stylesheet != null)
                    {
                        // when page does not contain StyleSheet
                        // controls, Default stylesheet will search twice. 
                        _referredStyle = stylesheet[reference];
                    }

                    if (_referredStyle == null)
                    {
                        // built in styles
                        _referredStyle = StyleSheet.Default[reference];

                        // No exceptions in Designer, will handle differently.
                        if (_referredStyle == null && !_control.MobilePage.DesignMode)
                        {
                            String exceptionResource;
                            if (nearestTemplatedControl is UserControl &&
                                !(nearestTemplatedControl is MobileUserControl))
                            {
                                // Throw a specific error message in this case
                                exceptionResource =
                                    SR.Style_StyleNotFoundOnGenericUserControl;
                            }
                            else
                            {
                                exceptionResource =
                                    SR.Style_StyleNotFound;
                            }
                                
                            throw new Exception(SR.GetString(exceptionResource,
                                                             reference)); 
                        }
                    }
                }

                return _referredStyle;
            }
        }

        //  late bound method for accessing style properties
        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.this"]/*' />
        public Object this[Object key]
        {
            get
            {
                return GetValue((Property)key, false, true, null);
            }
            set
            {
                Property property = (Property)key;
                Object defaultValue = property.DefaultValue;
                State[property.Name] = defaultValue.Equals(value) ? null : value;
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.this1"]/*' />
        public Object this[Object key, bool inherit]
        {
            get
            {
                return GetValue((Property)key, inherit, true, null);
            }
        }

        private Object GetValue(Property property, bool inherit, bool returnDefault, Hashtable stylesEncountered)
        {
            //  try to retrieve from internal value
            Object value = State[property.Name];

            if (value == null)
            {
                //  else retrieve from style reference
                if (inherit)
                {
                    Style style = this.ReferredStyle;
                    if (style != null)
                    {
                        if (stylesEncountered == null)
                        {
                            stylesEncountered = new Hashtable(); 
                        }

                        if (stylesEncountered.ContainsKey(style))
                        {
                            if (_control.MobilePage != null && _control.MobilePage.DesignMode)
                            {
                                return property.DefaultValue;
                            }
                            else
                            {
                                throw new Exception(SR.GetString(SR.Style_CircularReference, this.Name));
                            }
                        }
                        stylesEncountered[style] = true;
                        value = style.GetValue(property, inherit, false, stylesEncountered);
                    }
                }

                if (value == null)
                {
                    //  else retrieve from control ancestor
                    if (inherit && property.Inherit && _control != null)
                    {
                        Style parentStyle = null;
                        if (_cachedParentStyle == null)
                        {
                            if (_control.Parent is MobileControl)
                            {
                                parentStyle = ((MobileControl)_control.Parent).Style;
                                _cachedParentStyle = parentStyle;
                            }
                            // DeviceSpecific is treated as control at design time, however, we need to get
                            // the styles from devicespecific's parent.
                            else if (_control.MobilePage != null &&
                                _control.MobilePage.DesignMode && 
                                _control.Parent is DeviceSpecific &&
                                _control.Parent.Parent is MobileControl)
                            {
                                parentStyle = ((MobileControl)_control.Parent.Parent).Style;
                            }
                            else if(!(_control is Form))
                            {
                                Control _tempControl = _control.Parent;
                                while(!(_tempControl is MobileControl) && (_tempControl != null))
                                {
                                    _tempControl = _tempControl.Parent;
                                }
                                if(_tempControl != null)
                                {
                                    parentStyle = ((MobileControl)_tempControl).Style;
                                }
                            }
                        }
                        else
                        {
                            parentStyle = _cachedParentStyle;
                        }

                        if (parentStyle != null)
                        {
                            value = parentStyle.GetValue(property, inherit, false, null);
                        }
                    }

                    //  else retrieve default value
                    if (value == null && returnDefault)
                    {
                        value = property.DefaultValue;
                    }
                }
            }
            return value;
        }

        internal void InvalidateParentStyle()
        {
            _cachedParentStyle = null;
        }

        ////////////////////////////////////////////////////////////////////////////
        //  BEGIN STYLES
        ////////////////////////////////////////////////////////////////////////////

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.Font"]/*' />
        [
            DefaultValue(null),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Style_Font),
            NotifyParentProperty(true)
        ]
        public FontInfo Font
        {
            get
            {
                if (_font == null)
                {
                    _font = new FontInfo(this);
                }
                return _font;
            }
        }

        // FontSize and FontName internal
        // we still need these methods on style due to their inheritance and
        // persistence behavior, they're referenced from FontInfo.cs.
        // 


        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        internal String FontName
        {
            get
            {
                return (String)this[FontNameKey];
            }
            set
            {
                this[FontNameKey] = value;
            }
        }

        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        internal BooleanOption Bold
        {
            get
            {
                return (BooleanOption)this[BoldKey];
            }
            set
            {
                this[BoldKey] = value;
            }
        }

        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        internal BooleanOption Italic
        {
            get
            {
                return (BooleanOption)this[ItalicKey];
            }
            set
            {
                this[ItalicKey] = value;
            }
        }

        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        internal FontSize FontSize
        {
            get
            {
                return (FontSize)this[FontSizeKey];
            }
            set
            {
                this[FontSizeKey] = value;
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.Alignment"]/*' />
        [
            Bindable(true),
            DefaultValue(Alignment.NotSet),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Style_Alignment),
            NotifyParentProperty(true),
        ]
        public Alignment Alignment
        {
            get
            {
                return (Alignment)this[AlignmentKey];
            }
            set
            {
                this[AlignmentKey] = value;
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.Wrapping"]/*' />
        [
            Bindable(true),
            DefaultValue(Wrapping.NotSet),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Style_Wrapping),
            NotifyParentProperty(true),
        ]
        public Wrapping Wrapping
        {
            get
            {
                return (Wrapping)this[WrappingKey];
            }
            set
            {
                this[WrappingKey] = value;
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.ForeColor"]/*' />
        [
            Bindable(true),
            DefaultValue(typeof(Color), ""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Style_ForeColor),
            NotifyParentProperty(true),
            TypeConverterAttribute(typeof(WebColorConverter)),
        ]
        public Color ForeColor
        {
            get
            {
                return (Color)this[ForeColorKey];
            }
            set
            {
                this[ForeColorKey] = value;
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.BackColor"]/*' />
        [
            Bindable(true),
            DefaultValue(typeof(Color), ""),
            MobileCategory(SR.Category_Appearance),
            MobileSysDescription(SR.Style_BackColor),
            NotifyParentProperty(true),
            TypeConverterAttribute(typeof(WebColorConverter))
        ]
        public Color BackColor
        {
            get
            {
                return (Color)this[BackColorKey];
            }
            set
            {
                this[BackColorKey] = value;
            }
        }

        /////////////////////////////////////////////////////////////////////////
        //  TEMPLATES SUPPORT
        /////////////////////////////////////////////////////////////////////////


        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.IParserAccessor.AddParsedSubObject"]/*' />
        /// <internalonly/>
        protected void AddParsedSubObject(Object o)
        {
            if (o is DeviceSpecific)
            {
                if (DeviceSpecific != null)
                {
                    throw new Exception(
                        SR.GetString(SR.MobileControl_NoMultipleDeviceSpecifics));
                }
                DeviceSpecific = (DeviceSpecific)o;

                // This code works by assuming that the time this
                // method is called, that Control has not yet been
                // set.  Thus, we set the DeviceSpecific's parent
                // control when the Control itself is set.  If Control
                // != null here, then we won't get that opportunity. 
                Debug.Assert(Control == null);
            }
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.IsTemplated"]/*' />
        [
            Browsable(false),
            DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        ]
        public bool IsTemplated
        {
            get
            {
                return IsTemplatedInternal(null);
            }
        }

        internal bool IsTemplatedInternal(Hashtable stylesEncountered)
        {
            if (_deviceSpecific != null && _deviceSpecific.HasTemplates)
            {
                return true;
            }

            Style referredStyle = ReferredStyle;
            if (referredStyle == null)
            {
                return false;
            }

            if (stylesEncountered == null)
            {
                stylesEncountered = new Hashtable(); 
            }
            if (stylesEncountered.ContainsKey(referredStyle))
            {
                if (_control.MobilePage != null && _control.MobilePage.DesignMode)
                {
                    return false;
                }
                else
                {
                    throw new Exception(SR.GetString(SR.Style_CircularReference, this.Name));
                }
            }
            // referredStyle != null
            stylesEncountered[referredStyle] = true;
            return referredStyle.IsTemplatedInternal(stylesEncountered);
        }

        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.GetTemplate"]/*' />
        public ITemplate GetTemplate(String templateName)
        {
            return GetTemplateInternal(templateName, null);
        }

        internal ITemplate GetTemplateInternal(String templateName, Hashtable stylesEncountered)
        {
            ITemplate t = null;
            if (_deviceSpecific != null)
            {
                t = (ITemplate)_deviceSpecific.GetTemplate (templateName);
            }

            Style referredStyle = ReferredStyle;
            if (t == null && referredStyle != null)
            {
                // Check for cyclical style references.
                if (stylesEncountered == null)
                {
                    stylesEncountered = new Hashtable ();
                }
                if (stylesEncountered.ContainsKey(referredStyle))
                {
                    if (_control.MobilePage != null && _control.MobilePage.DesignMode)
                    {
                        return null;
                    }
                    else
                    {
                        throw new Exception(SR.GetString(SR.Style_CircularReference, this.Name));
                    }
                }

                // No cycle detected.
                stylesEncountered[referredStyle] = true;                
                t = referredStyle.GetTemplateInternal(templateName, stylesEncountered);
            }

            return t;
        }

        // Design-time only property
        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.DeviceSpecific"]/*' />
        [
            Browsable(false),
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
                if (null != value)
                {
                    value.SetOwner(this);
                }
            }
        }

        //  registers a new type of style and returns the KEY for accessing it
        /// <include file='doc\Style.uex' path='docs/doc[@for="Style.RegisterStyle"]/*' />
        public static Object RegisterStyle(String name, Type type, Object defaultValue, bool inherit)
        {
            return new Property(name, type, defaultValue, inherit);
        }

        class Property
        {
            public String Name;
            public Type   Type;
            public Object DefaultValue;
            public bool   Inherit;        // can be inherited from parent?

            public Property(String name, Type type, Object defaultValue, bool inherit)
            {
                this.Name = name;
                this.Type = type;
                this.DefaultValue = defaultValue;
                this.Inherit = inherit;
            }
        }

        #region Implementation of IStateManager
        /// <internalonly/>
        bool IStateManager.IsTrackingViewState {
            get {
                return IsTrackingViewState;
            }
        }

        /// <internalonly/>
        void IStateManager.LoadViewState(object state) {
            LoadViewState(state);
        }

        /// <internalonly/>
        void IStateManager.TrackViewState() {
            TrackViewState();
        }

        /// <internalonly/>
        object IStateManager.SaveViewState() {
            return SaveViewState();
        }
        #endregion

        #region IParserAccessor implementation
        void IParserAccessor.AddParsedSubObject(Object o) {
            AddParsedSubObject(o);
        }
        #endregion
    }
}
