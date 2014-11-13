//------------------------------------------------------------------------------
// <copyright file="Style.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Web;
    using System.Web.UI;


    /// <devdoc>
    /// <para> Defines the properties and methods of the <see cref='System.Web.UI.WebControls.Style'/> class.</para>
    /// </devdoc>
    [
    ToolboxItem(false),
    TypeConverterAttribute(typeof(EmptyStringExpandableObjectConverter))
    ]
    public class Style : Component, IStateManager {

        // !!NOTE!!
        // PanelStyle also defines a set of flag contants and both sets have to
        // be unique.  Please be careful when adding new flags to either list.
        internal const int UNUSED = 0x0001;

        internal const int PROP_CSSCLASS = 0x0002;
        internal const int PROP_FORECOLOR = 0x0004;
        internal const int PROP_BACKCOLOR = 0x0008;
        internal const int PROP_BORDERCOLOR = 0x0010;
        internal const int PROP_BORDERWIDTH = 0x0020;
        internal const int PROP_BORDERSTYLE = 0x0040;
        internal const int PROP_HEIGHT = 0x0080;
        internal const int PROP_WIDTH = 0x0100;
        internal const int PROP_FONT_NAMES = 0x0200;
        internal const int PROP_FONT_SIZE = 0x0400;
        internal const int PROP_FONT_BOLD = 0x0800;
        internal const int PROP_FONT_ITALIC = 0x1000;
        internal const int PROP_FONT_UNDERLINE = 0x2000;
        internal const int PROP_FONT_OVERLINE = 0x4000;
        internal const int PROP_FONT_STRIKEOUT = 0x8000;

        internal const string SetBitsKey = "_!SB";

        private StateBag statebag;
        private FontInfo fontInfo;
        private string registeredCssClass;
        private bool ownStateBag;
        private bool marked;
        private int setBits;
        private int markedBits;

        // For performance, use this array instead of Enum.Format() to convert a BorderStyle to
        // a string.  CLR is investigating improving the perf of Enum.Format(). (VSWhidbey
        internal static readonly string[] borderStyles = new string[] {"NotSet", "None", "Dotted",
            "Dashed", "Solid", "Double", "Groove", "Ridge", "Inset", "Outset"};


        /// <devdoc>
        /// Initializes a new instance of the Style class.
        /// </devdoc>
        public Style() : this(null) {
            ownStateBag = true;
        }


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.Style'/> class with the
        ///       specified state bag information.  Do not use this constructor if you are overriding
        ///       CreateControlStyle() and are changing some properties on the created style.
        ///    </para>
        /// </devdoc>
        public Style(StateBag bag) {
            statebag = bag;
            marked = false;
            setBits = 0;
            // VSWhidbey 541984: Style inherits from Component and requires finalization, resulting in bad performance
            // When inheriting, if finalization is desired, call GC.ReRegisterForFinalize
            GC.SuppressFinalize(this);
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the background color property of the <see cref='System.Web.UI.WebControls.Style'/> class.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Color), ""),
        WebSysDescription(SR.Style_BackColor),
        NotifyParentProperty(true),
        TypeConverterAttribute(typeof(WebColorConverter))
        ]
        public Color BackColor {
            get {
                if (IsSet(PROP_BACKCOLOR)) {
                    return(Color)(ViewState["BackColor"]);
                }
                return Color.Empty;
            }
            set {
                ViewState["BackColor"] = value;
                SetBit(PROP_BACKCOLOR);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the border color property of the <see cref='System.Web.UI.WebControls.Style'/> class.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Color), ""),
        WebSysDescription(SR.Style_BorderColor),
        NotifyParentProperty(true),
        TypeConverterAttribute(typeof(WebColorConverter))
        ]
        public Color BorderColor {
            get {
                if (IsSet(PROP_BORDERCOLOR)) {
                    return(Color)(ViewState["BorderColor"]);
                }
                return Color.Empty;
            }
            set {
                ViewState["BorderColor"] = value;
                SetBit(PROP_BORDERCOLOR);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the border width property of the <see cref='System.Web.UI.WebControls.Style'/> class.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.Style_BorderWidth),
        NotifyParentProperty(true)
        ]
        public Unit BorderWidth {
            get {
                if (IsSet(PROP_BORDERWIDTH)) {
                    return(Unit)(ViewState["BorderWidth"]);
                }
                return Unit.Empty;
            }
            set {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0)) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.Style_InvalidBorderWidth));
                }
                ViewState["BorderWidth"] = value;
                SetBit(PROP_BORDERWIDTH);
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the border style property of the <see cref='System.Web.UI.WebControls.Style'/>
        /// class.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(BorderStyle.NotSet),
        WebSysDescription(SR.Style_BorderStyle),
        NotifyParentProperty(true)
        ]
        public BorderStyle BorderStyle {
            get {
                if (IsSet(PROP_BORDERSTYLE)) {
                    return(BorderStyle)(ViewState["BorderStyle"]);
                }
                return BorderStyle.NotSet;
            }
            set {
                if (value < BorderStyle.NotSet || value > BorderStyle.Outset) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["BorderStyle"] = value;
                SetBit(PROP_BORDERSTYLE);
            }
        }


        /// <devdoc>
        /// <para>Gets or sets the CSS class property of the <see cref='System.Web.UI.WebControls.Style'/> class.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.Style_CSSClass),
        NotifyParentProperty(true),
        CssClassProperty()
        ]
        public string CssClass {
            get {
                if (IsSet(PROP_CSSCLASS)) {
                    string s = (string)ViewState["CssClass"];
                    return (s == null) ? String.Empty : s;
                }
                return String.Empty;
            }
            set {
                ViewState["CssClass"] = value;
                SetBit(PROP_CSSCLASS);
            }
        }


        /// <devdoc>
        /// <para>Gets font information of the <see cref='System.Web.UI.WebControls.Style'/> class.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        WebSysDescription(SR.Style_Font),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
        NotifyParentProperty(true)
        ]
        public FontInfo Font {
            get {
                if (fontInfo == null)
                    fontInfo = new FontInfo(this);
                return fontInfo;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the foreground color (typically the color
        ///       of the text) property of the <see cref='System.Web.UI.WebControls.Style'/>
        ///       class.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Color), ""),
        WebSysDescription(SR.Style_ForeColor),
        NotifyParentProperty(true),
        TypeConverterAttribute(typeof(WebColorConverter))
        ]
        public Color ForeColor {
            get {
                if (IsSet(PROP_FORECOLOR)) {
                    return(Color)(ViewState["ForeColor"]);
                }
                return Color.Empty;
            }
            set {
                ViewState["ForeColor"] = value;
                SetBit(PROP_FORECOLOR);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the height property of the <see cref='System.Web.UI.WebControls.Style'/> class.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.Style_Height),
        NotifyParentProperty(true)
        ]
        public Unit Height {
            get {
                if (IsSet(PROP_HEIGHT)) {
                    return(Unit)(ViewState["Height"]);
                }
                return Unit.Empty;
            }
            set {
                if (value.Value < 0) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.Style_InvalidHeight));
                }
                ViewState["Height"] = value;
                SetBit(PROP_HEIGHT);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// Gets a value indicating whether any style properties have been set.
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        public virtual bool IsEmpty {
            [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
            get {
                return ((setBits == 0) && (RegisteredCssClass.Length == 0));
            }
        }


        /// <devdoc>
        /// Returns a value indicating whether
        /// any style elements have been defined in the state bag.
        /// </devdoc>
        protected bool IsTrackingViewState {
            get {
                return marked;
            }
        }


        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Advanced)
        ]
        public string RegisteredCssClass {
            get {
                if (registeredCssClass == null) {
                    return String.Empty;
                }
                return registeredCssClass;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        /// Gets the state bag that holds the style properties.
        /// Marked as internal, because FontInfo accesses view state of its owner Style
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)
        ]
        protected internal StateBag ViewState {
            get {
                if (statebag == null) {
                    statebag = new StateBag(false);
                    if (IsTrackingViewState)
                        statebag.TrackViewState();
                }
                return statebag;
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Gets or sets the width property of the <see cref='System.Web.UI.WebControls.Style'/> class.
        ///    </para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(typeof(Unit), ""),
        WebSysDescription(SR.Style_Width),
        NotifyParentProperty(true)
        ]
        public Unit Width {
            get {
                if (IsSet(PROP_WIDTH)) {
                    return(Unit)(ViewState["Width"]);
                }
                return Unit.Empty;
            }
            set {
                if (value.Value < 0) {
                    throw new ArgumentOutOfRangeException("value", SR.GetString(SR.Style_InvalidWidth));
                }
                ViewState["Width"] = value;
                SetBit(PROP_WIDTH);
            }
        }


        /// <devdoc>
        /// </devdoc>
        public void AddAttributesToRender(HtmlTextWriter writer) {
            AddAttributesToRender(writer, null);
        }


        /// <devdoc>
        ///    <para>
        ///       Adds all non-blank style attributes to the HTML output stream to be rendered
        ///       to the client.
        ///    </para>
        /// </devdoc>
        public virtual void AddAttributesToRender(HtmlTextWriter writer, WebControl owner) {
            string cssClass = String.Empty;
            bool renderInlineStyle = true;

            if (IsSet(PROP_CSSCLASS)) {
                cssClass = (string)ViewState["CssClass"];
                if (cssClass == null) {
                    cssClass = String.Empty;
                }
            }
            if (!String.IsNullOrEmpty(registeredCssClass)) {
                renderInlineStyle = false;
                if (cssClass.Length != 0) {
                    cssClass += " " + registeredCssClass;
                }
                else {
                    cssClass = registeredCssClass;
                }
            }

            if (cssClass.Length > 0) {
               writer.AddAttribute(HtmlTextWriterAttribute.Class, cssClass);
            }

            if (renderInlineStyle) {
                CssStyleCollection styleAttributes = GetStyleAttributes(owner);
                styleAttributes.Render(writer);
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Clears the setBits int of the given bit.
        ///    </para>
        /// </devdoc>
        internal void ClearBit(int bit) {
            setBits &= ~bit;
        }



        /// <devdoc>
        ///    <para>
        ///       Copies non-blank elements from the specified style,
        ///       overwriting existing style elements if necessary.
        ///    </para>
        /// </devdoc>
        public virtual void CopyFrom(Style s) {
            if (RegisteredCssClass.Length != 0) {
                throw new InvalidOperationException(SR.GetString(SR.Style_RegisteredStylesAreReadOnly));
            }

            if (s != null && !s.IsEmpty) {
                this.Font.CopyFrom(s.Font);

                if (s.IsSet(PROP_CSSCLASS))
                    this.CssClass = s.CssClass;


                // if the source Style is registered and this one isn't,
                // reset all the styles set by the source Style so it's
                // css class can be used to set those values
                if (s.RegisteredCssClass.Length != 0) {
                    if (IsSet(PROP_CSSCLASS)) {
                        CssClass += " " + s.RegisteredCssClass;
                    }
                    else {
                        CssClass = s.RegisteredCssClass;
                    }

                    if (s.IsSet(PROP_BACKCOLOR) && (s.BackColor != Color.Empty)) {
                        ViewState.Remove("BackColor");
                        ClearBit(PROP_BACKCOLOR);
                    }
                    if (s.IsSet(PROP_FORECOLOR) && (s.ForeColor != Color.Empty)) {
                        ViewState.Remove("ForeColor");
                        ClearBit(PROP_FORECOLOR);
                    }
                    if (s.IsSet(PROP_BORDERCOLOR) && (s.BorderColor != Color.Empty)) {
                        ViewState.Remove("BorderColor");
                        ClearBit(PROP_BORDERCOLOR);
                    }
                    if (s.IsSet(PROP_BORDERWIDTH) && (s.BorderWidth != Unit.Empty)) {
                        ViewState.Remove("BorderWidth");
                        ClearBit(PROP_BORDERWIDTH);
                    }
                    if (s.IsSet(PROP_BORDERSTYLE)) {
                        ViewState.Remove("BorderStyle");
                        ClearBit(PROP_BORDERSTYLE);
                    }
                    if (s.IsSet(PROP_HEIGHT) && (s.Height != Unit.Empty)) {
                        ViewState.Remove("Height");
                        ClearBit(PROP_HEIGHT);
                    }
                    if (s.IsSet(PROP_WIDTH) && (s.Width != Unit.Empty)) {
                        ViewState.Remove("Width");
                        ClearBit(PROP_WIDTH);
                    }
                }
                else {
                    if (s.IsSet(PROP_BACKCOLOR) && (s.BackColor != Color.Empty))
                        this.BackColor = s.BackColor;
                    if (s.IsSet(PROP_FORECOLOR) && (s.ForeColor != Color.Empty))
                        this.ForeColor = s.ForeColor;
                    if (s.IsSet(PROP_BORDERCOLOR) && (s.BorderColor != Color.Empty))
                        this.BorderColor = s.BorderColor;
                    if (s.IsSet(PROP_BORDERWIDTH) && (s.BorderWidth != Unit.Empty))
                        this.BorderWidth = s.BorderWidth;
                    if (s.IsSet(PROP_BORDERSTYLE))
                        this.BorderStyle = s.BorderStyle;
                    if (s.IsSet(PROP_HEIGHT) && (s.Height != Unit.Empty))
                        this.Height = s.Height;
                    if (s.IsSet(PROP_WIDTH) && (s.Width != Unit.Empty))
                        this.Width = s.Width;
                }
            }
        }


        protected virtual void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver) {
            StateBag viewState = ViewState;

            Color c;

            // ForeColor
            if (IsSet(PROP_FORECOLOR)) {
                c = (Color)viewState["ForeColor"];
                if (!c.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.Color, ColorTranslator.ToHtml(c));
                }
            }

            // BackColor
            if (IsSet(PROP_BACKCOLOR)) {
                c = (Color)viewState["BackColor"];
                if (!c.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(c));
                }
            }

            // BorderColor
            if (IsSet(PROP_BORDERCOLOR)) {
                c = (Color)viewState["BorderColor"];
                if (!c.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.BorderColor, ColorTranslator.ToHtml(c));
                }
            }

            BorderStyle bs = this.BorderStyle;
            Unit bu = this.BorderWidth;
            if (!bu.IsEmpty) {
                attributes.Add(HtmlTextWriterStyle.BorderWidth, bu.ToString(CultureInfo.InvariantCulture));
                if (bs == BorderStyle.NotSet) {
                    if (bu.Value != 0.0) {
                        attributes.Add(HtmlTextWriterStyle.BorderStyle, "solid");
                    }
                }
                else {
                    attributes.Add(HtmlTextWriterStyle.BorderStyle, borderStyles[(int)bs]);
                }
            }
            else {
                if (bs != BorderStyle.NotSet) {
                    attributes.Add(HtmlTextWriterStyle.BorderStyle, borderStyles[(int)bs]);
                }
            }

            // need to call the property get in case we have font properties from view state and have not
            // created the font object
            FontInfo font = Font;

            // Font.Names
            string[] names = font.Names;
            if (names.Length > 0) {
                attributes.Add(HtmlTextWriterStyle.FontFamily, Style.FormatStringArray(names, ','));
            }

            // Font.Size
            FontUnit fu = font.Size;
            if (fu.IsEmpty == false) {
                attributes.Add(HtmlTextWriterStyle.FontSize, fu.ToString(CultureInfo.InvariantCulture));
            }

            // Font.Bold
            if (IsSet(PROP_FONT_BOLD)) {
                if (font.Bold) {
                    attributes.Add(HtmlTextWriterStyle.FontWeight, "bold");
                }
                else {
                    attributes.Add(HtmlTextWriterStyle.FontWeight, "normal");
                }
            }

            // Font.Italic
            if (IsSet(PROP_FONT_ITALIC)) {
                if (font.Italic == true) {
                    attributes.Add(HtmlTextWriterStyle.FontStyle, "italic");
                }
                else {
                    attributes.Add(HtmlTextWriterStyle.FontStyle, "normal");
                }
            }

            // 
            string textDecoration = String.Empty;
            if (font.Underline) {
                textDecoration = "underline";
            }
            if (font.Overline) {
                textDecoration += " overline";
            }
            if (font.Strikeout) {
                textDecoration += " line-through";
            }
            if (textDecoration.Length > 0) {
                attributes.Add(HtmlTextWriterStyle.TextDecoration, textDecoration);
            }
            else {
                if (IsSet(PROP_FONT_UNDERLINE) || IsSet(PROP_FONT_OVERLINE) || IsSet(PROP_FONT_STRIKEOUT)) {
                    attributes.Add(HtmlTextWriterStyle.TextDecoration, "none");
                }
            }

            Unit u;

            // Height
            if (IsSet(PROP_HEIGHT)) {
                u = (Unit)viewState["Height"];
                if (!u.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.Height, u.ToString(CultureInfo.InvariantCulture));
                }
            }

            // Width
            if (IsSet(PROP_WIDTH)) {
                u = (Unit)viewState["Width"];
                if (!u.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.Width, u.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private static string FormatStringArray(string[] array, char delimiter) {
            int n = array.Length;

            if (n == 1) {
                return array[0];
            }
            if (n == 0) {
                return String.Empty;
            }
            return String.Join(delimiter.ToString(CultureInfo.InvariantCulture), array);
        }


        /// <devdoc>
        /// Retrieves the collection of CSS style attributes represented by this style.
        /// </devdoc>
        public CssStyleCollection GetStyleAttributes(IUrlResolutionService urlResolver) {
            CssStyleCollection attributes = new CssStyleCollection();

            FillStyleAttributes(attributes, urlResolver);
            return attributes;
        }


        /// <devdoc>
        /// Returns a value indicating whether the specified style
        /// property has been defined in the state bag.
        /// </devdoc>
        [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        internal bool IsSet(int propKey) {
            return (setBits & propKey) != 0;
        }


        /// <devdoc>
        /// Load the previously saved state.
        /// </devdoc>
        protected internal void LoadViewState(object state) {
            if (state != null && ownStateBag)
                ViewState.LoadViewState(state);

            if (statebag != null) {
                object o = ViewState[SetBitsKey];
                if (o != null) {
                    markedBits = (int)o;

                    // markedBits indicates properties that got reloaded into
                    // view state, so update setBits, to indicate these
                    // properties are set as well.
                    setBits |= markedBits;
                }
            }
        }


        /// <devdoc>
        ///    A protected method. Marks the beginning for tracking
        ///    state changes on the control. Any changes made after "mark" will be tracked and
        ///    saved as part of the control viewstate.
        /// </devdoc>
        [System.Runtime.TargetedPatchingOptOut("Performance critical to inline across NGen image boundaries")]
        protected internal virtual void TrackViewState() {
            if (ownStateBag) {
                ViewState.TrackViewState();
            }

            marked = true;
        }


        /// <devdoc>
        /// Copies non-blank elements from the specified style,
        /// but will not overwrite any existing style elements.
        /// </devdoc>
        public virtual void MergeWith(Style s) {
            if (RegisteredCssClass.Length != 0) {
                throw new InvalidOperationException(SR.GetString(SR.Style_RegisteredStylesAreReadOnly));
            }

            if (s == null || s.IsEmpty)
                return;

            if (IsEmpty) {
                // merge into an empty style is equivalent to a copy, which
                // is more efficient
                CopyFrom(s);
                return;
            }

            this.Font.MergeWith(s.Font);

            if (s.IsSet(PROP_CSSCLASS) && !this.IsSet(PROP_CSSCLASS))
                this.CssClass = s.CssClass;

            // If the source Style is registered and this one isn't, copy
            // the CSS class and any style props not included in the CSS class
            // if they aren't set on this Style
            if (s.RegisteredCssClass.Length == 0) {
                if (s.IsSet(PROP_BACKCOLOR) && (!this.IsSet(PROP_BACKCOLOR) || (BackColor == Color.Empty)))
                    this.BackColor = s.BackColor;
                if (s.IsSet(PROP_FORECOLOR) && (!this.IsSet(PROP_FORECOLOR) || (ForeColor == Color.Empty)))
                    this.ForeColor = s.ForeColor;
                if (s.IsSet(PROP_BORDERCOLOR) && (!this.IsSet(PROP_BORDERCOLOR) || (BorderColor == Color.Empty)))
                    this.BorderColor = s.BorderColor;
                if (s.IsSet(PROP_BORDERWIDTH) && (!this.IsSet(PROP_BORDERWIDTH) || (BorderWidth == Unit.Empty)))
                    this.BorderWidth = s.BorderWidth;
                if (s.IsSet(PROP_BORDERSTYLE) && !this.IsSet(PROP_BORDERSTYLE))
                    this.BorderStyle = s.BorderStyle;
                if (s.IsSet(PROP_HEIGHT) && (!this.IsSet(PROP_HEIGHT) || (Height == Unit.Empty)))
                    this.Height = s.Height;
                if (s.IsSet(PROP_WIDTH) && (!this.IsSet(PROP_WIDTH) || (Width == Unit.Empty)))
                    this.Width = s.Width;
            }
            else {
                if (IsSet(PROP_CSSCLASS)) {
                    CssClass += " " + s.RegisteredCssClass;
                }
                else {
                    CssClass = s.RegisteredCssClass;
                }
            }
        }


        /// <devdoc>
        /// Clears out any defined style elements from the state bag.
        /// </devdoc>
        public virtual void Reset() {
            if (statebag != null) {
                if (IsSet(PROP_CSSCLASS))
                    ViewState.Remove("CssClass");
                if (IsSet(PROP_BACKCOLOR))
                    ViewState.Remove("BackColor");
                if (IsSet(PROP_FORECOLOR))
                    ViewState.Remove("ForeColor");
                if (IsSet(PROP_BORDERCOLOR))
                    ViewState.Remove("BorderColor");
                if (IsSet(PROP_BORDERWIDTH))
                    ViewState.Remove("BorderWidth");
                if (IsSet(PROP_BORDERSTYLE))
                    ViewState.Remove("BorderStyle");
                if (IsSet(PROP_HEIGHT))
                    ViewState.Remove("Height");
                if (IsSet(PROP_WIDTH))
                    ViewState.Remove("Width");

                Font.Reset();

                ViewState.Remove(SetBitsKey);
                markedBits = 0;
            }

            setBits = 0;
        }


        /// <devdoc>
        /// Saves any state that has been modified
        /// after the TrackViewState method was invoked.
        /// </devdoc>
        protected internal virtual object SaveViewState() {
            if (statebag != null) {
                if (markedBits != 0) {
                    // new bits or properties were changed
                    // updating the state bag at this point will automatically mark
                    // SetBitsKey as dirty, and it will be added to the resulting viewstate
                    ViewState[SetBitsKey] = markedBits;
                }

                if (ownStateBag)
                    return ViewState.SaveViewState();
            }
            return null;
        }


        /// <internalonly/>
        protected internal virtual void SetBit(int bit) {
            setBits |= bit;
            if (IsTrackingViewState) {
                // since we're tracking changes, include this property change or
                // bit into the markedBits flag set.
                markedBits |= bit;
            }
        }

        public void SetDirty() {
            ViewState.SetDirty(true);
            markedBits = setBits;
        }

        /// <devdoc>
        /// Associated this Style with a CSS class as part of registration with
        /// a style sheet.
        /// </devdoc>
        internal void SetRegisteredCssClass(string cssClass) {
            registeredCssClass = cssClass;
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
    }
}

