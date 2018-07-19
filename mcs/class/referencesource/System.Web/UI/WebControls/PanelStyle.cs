//------------------------------------------------------------------------------
// <copyright file="PanelStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Web;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>Specifies the style of the panel.</para>
    /// </devdoc>
    public class PanelStyle : Style {

        // !!NOTE!!
        // Style.cs also defines a set of flag contants and both sets have to
        // be unique.  Please be careful when adding new flags to either list.
        private const int PROP_BACKIMAGEURL = 0x00010000;
        private const int PROP_DIRECTION = 0x00020000;
        private const int PROP_HORIZONTALALIGN = 0x00040000;
        private const int PROP_SCROLLBARS = 0x00080000;
        private const int PROP_WRAP = 0x00100000;

        private const string STR_BACKIMAGEURL = "BackImageUrl";
        private const string STR_DIRECTION = "Direction";
        private const string STR_HORIZONTALALIGN = "HorizontalAlign";
        private const string STR_SCROLLBARS = "ScrollBars";
        private const string STR_WRAP = "Wrap";


        /// <devdoc>
        ///    <para>
        ///       Initializes a new instance of the <see cref='System.Web.UI.WebControls.PanelStyle'/> 
        ///       class with state bag information.
        ///    </para>
        /// </devdoc>
        public PanelStyle(StateBag bag) : base (bag) {
        }


        /// <devdoc>
        ///    <para>Gets or sets the URL of the background image for the panel.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        UrlProperty(),
        WebSysDescription(SR.Panel_BackImageUrl)
        ]
        public virtual string BackImageUrl {
            get {
                if (IsSet(PROP_BACKIMAGEURL)) {
                    return(string)(ViewState[STR_BACKIMAGEURL]);
                }
                return String.Empty;
            }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                ViewState[STR_BACKIMAGEURL] = value;
                SetBit(PROP_BACKIMAGEURL);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets content direction for the panel.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.Panel_Direction)
        ]
        public virtual ContentDirection Direction {
            get {
                if (IsSet(PROP_DIRECTION)) {
                    return (ContentDirection)(ViewState[STR_DIRECTION]);
                }
                return ContentDirection.NotSet;
            }
            set {
                if (value < ContentDirection.NotSet || value > ContentDirection.RightToLeft) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState[STR_DIRECTION] = value;
                SetBit(PROP_DIRECTION);
            }
        }
        

        /// <devdoc>
        ///    <para>Gets or sets the horizontal alignment of the contents within the panel.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.Panel_HorizontalAlign)
        ]
        public virtual HorizontalAlign HorizontalAlign {
            get {
                if (IsSet(PROP_HORIZONTALALIGN)) {
                    return (HorizontalAlign)(ViewState[STR_HORIZONTALALIGN]);
                }
                return HorizontalAlign.NotSet;
            }
            set {
                if (value < HorizontalAlign.NotSet || value > HorizontalAlign.Justify) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState[STR_HORIZONTALALIGN] = value;
                SetBit(PROP_HORIZONTALALIGN);
            }
        }


        /// <devdoc>
        ///     <para>Gets or sets the scrollbar behavior of the panel.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.Panel_ScrollBars)
        ]
        public virtual ScrollBars ScrollBars {
            get {
                if (IsSet(PROP_SCROLLBARS)) {
                    return (ScrollBars)(ViewState[STR_SCROLLBARS]);
                }
                return ScrollBars.None;
            }
            set {
                if (value < ScrollBars.None || value > ScrollBars.Auto) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState[STR_SCROLLBARS] = value;
                SetBit(PROP_SCROLLBARS);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value
        ///       indicating whether the content wraps within the panel.</para>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(""),
        WebSysDescription(SR.Panel_Wrap)
        ]
        public virtual bool Wrap {
            get {
                if (IsSet(PROP_WRAP)) {
                    return (bool)(ViewState[STR_WRAP]);
                }
                return true;
            }
            set {
                ViewState[STR_WRAP] = value;
                SetBit(PROP_WRAP);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Copies non-blank elements from the specified style, overwriting existing
        ///       style elements if necessary.</para>
        /// </devdoc>
        public override void CopyFrom(Style s) {
            if (s != null && !s.IsEmpty) {
                base.CopyFrom(s);

                if (s is PanelStyle) {
                    PanelStyle ts = (PanelStyle)s;

                    if (s.RegisteredCssClass.Length != 0) {
                        if (ts.IsSet(PROP_BACKIMAGEURL)) {
                            ViewState.Remove(STR_BACKIMAGEURL);
                            ClearBit(PROP_BACKIMAGEURL);
                        }
                        if (ts.IsSet(PROP_SCROLLBARS)) {
                            ViewState.Remove(STR_SCROLLBARS);
                            ClearBit(PROP_SCROLLBARS);
                        }
                        if (ts.IsSet(PROP_WRAP)) {
                            ViewState.Remove(STR_WRAP);
                            ClearBit(PROP_WRAP);
                        }
                    }
                    else {
                        if (ts.IsSet(PROP_BACKIMAGEURL))
                            this.BackImageUrl = ts.BackImageUrl;
                        if (ts.IsSet(PROP_SCROLLBARS))
                            this.ScrollBars = ts.ScrollBars;
                        if (ts.IsSet(PROP_WRAP))
                            this.Wrap = ts.Wrap;
                    }

                    if (ts.IsSet(PROP_DIRECTION))
                        this.Direction = ts.Direction;
                    if (ts.IsSet(PROP_HORIZONTALALIGN))
                        this.HorizontalAlign = ts.HorizontalAlign;
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Copies non-blank elements from the specified style, but will not overwrite
        ///       any existing style elements.</para>
        /// </devdoc>
        public override void MergeWith(Style s) {
            if (s != null && !s.IsEmpty) {
                if (IsEmpty) {
                    // merge into an empty style is equivalent to a copy,
                    // which is more efficient
                    CopyFrom(s);
                    return;
                }

                base.MergeWith(s);

                if (s is PanelStyle) {
                    PanelStyle ts = (PanelStyle)s;

                    // Since we're already copying the registered CSS class in base.MergeWith, we don't
                    // need to any attributes that would be included in that class.
                    if (s.RegisteredCssClass.Length == 0) {
                        if (ts.IsSet(PROP_BACKIMAGEURL) && !this.IsSet(PROP_BACKIMAGEURL))
                            this.BackImageUrl = ts.BackImageUrl;
                        if (ts.IsSet(PROP_SCROLLBARS) && !this.IsSet(PROP_SCROLLBARS))
                            this.ScrollBars = ts.ScrollBars;
                        if (ts.IsSet(PROP_WRAP) && !this.IsSet(PROP_WRAP))
                            this.Wrap = ts.Wrap;
                    }

                    if (ts.IsSet(PROP_DIRECTION) && !this.IsSet(PROP_DIRECTION))
                        this.Direction = ts.Direction;
                    if (ts.IsSet(PROP_HORIZONTALALIGN) && !this.IsSet(PROP_HORIZONTALALIGN))
                        this.HorizontalAlign = ts.HorizontalAlign;
                }
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    <para>Clears out any defined style elements from the state bag.</para>
        /// </devdoc>
        public override void Reset() {
            if (IsSet(PROP_BACKIMAGEURL))
                ViewState.Remove(STR_BACKIMAGEURL);
            if (IsSet(PROP_DIRECTION))
                ViewState.Remove(STR_DIRECTION);
            if (IsSet(PROP_HORIZONTALALIGN))
                ViewState.Remove(STR_HORIZONTALALIGN);
            if (IsSet(PROP_SCROLLBARS))
                ViewState.Remove(STR_SCROLLBARS);
            if (IsSet(PROP_WRAP))
                ViewState.Remove(STR_WRAP);

            base.Reset();
        }
    }
}
