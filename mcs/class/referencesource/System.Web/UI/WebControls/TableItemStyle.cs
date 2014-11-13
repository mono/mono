//------------------------------------------------------------------------------
// <copyright file="TableItemStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Globalization;
    using System.Web;
    using System.Web.Configuration;
    using System.Web.UI;

    /// <devdoc>
    ///    <para>Specifies the style of the table item.</para>
    /// </devdoc>
    public class TableItemStyle : Style {


        /// <devdoc>
        ///    <para>Specifies the horizontal alignment property. This field is
        ///       constant.</para>
        /// </devdoc>
        internal const int PROP_HORZALIGN = 0x00010000;

        /// <devdoc>
        ///    <para>Specifies the vertical alignment property. This field is
        ///       constant.</para>
        /// </devdoc>
        internal const int PROP_VERTALIGN = 0x00020000;

        /// <devdoc>
        ///    <para>Specifies the
        ///       wrap property. This field is constant.</para>
        /// </devdoc>
        internal const int PROP_WRAP = 0x00040000;


        /// <devdoc>
        /// <para>Creates a new instance of the <see cref='System.Web.UI.WebControls.TableItemStyle'/> class.</para>
        /// </devdoc>
        public TableItemStyle() : base() {
        }


        /// <devdoc>
        ///    <para>
        ///       Creates a new instance of the <see cref='System.Web.UI.WebControls.TableItemStyle'/> class with the
        ///       specified state bag.
        ///    </para>
        /// </devdoc>
        public TableItemStyle(StateBag bag) : base(bag) {
        }

        private bool EnableLegacyRendering {
            get {
                return (RuntimeConfig.GetAppConfig().XhtmlConformance.Mode == XhtmlConformanceMode.Legacy);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the horizontal alignment of the table item.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(HorizontalAlign.NotSet),
        WebSysDescription(SR.TableItem_HorizontalAlign),
        NotifyParentProperty(true)
        ]
        public virtual HorizontalAlign HorizontalAlign {
            get {
                if (IsSet(PROP_HORZALIGN)) {
                    return(HorizontalAlign)(ViewState["HorizontalAlign"]);
                }
                return HorizontalAlign.NotSet;
            }
            set {
                if (value < HorizontalAlign.NotSet || value > HorizontalAlign.Justify) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["HorizontalAlign"] = value;
                SetBit(PROP_HORZALIGN);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the vertical alignment of the table item.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(VerticalAlign.NotSet),
        WebSysDescription(SR.TableItem_VerticalAlign),
        NotifyParentProperty(true)
        ]
        public virtual VerticalAlign VerticalAlign {
            get {
                if (IsSet(PROP_VERTALIGN)) {
                    return(VerticalAlign)(ViewState["VerticalAlign"]);
                }
                return VerticalAlign.NotSet;
            }
            set {
                if (value < VerticalAlign.NotSet || value > VerticalAlign.Bottom) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["VerticalAlign"] = value;
                SetBit(PROP_VERTALIGN);
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets a value indicating whether the cell content wraps within the cell.</para>
        /// </devdoc>
        [
        WebCategory("Layout"),
        DefaultValue(true),
        WebSysDescription(SR.TableItemStyle_Wrap),
        NotifyParentProperty(true)
        ]
        public virtual bool Wrap {
            get {
                if (IsSet(PROP_WRAP)) {
                    return(bool)(ViewState["Wrap"]);
                }
                return true;
            }
            set {
                ViewState["Wrap"] = value;
                SetBit(PROP_WRAP);
            }
        }


        /// <devdoc>
        ///    <para>Adds information about horizontal alignment, vertical alignment, and wrap to the list of attributes to render.</para>
        /// </devdoc>
        public override void AddAttributesToRender(HtmlTextWriter writer, WebControl owner) {
            base.AddAttributesToRender(writer, owner);

            if (!Wrap) {
                if (IsControlEnableLegacyRendering(owner)) {
                    writer.AddAttribute(HtmlTextWriterAttribute.Nowrap, "nowrap");
                }
                else {
                    writer.AddStyleAttribute(HtmlTextWriterStyle.WhiteSpace, "nowrap");
                }
            }

            HorizontalAlign hAlign = HorizontalAlign;
            if (hAlign != HorizontalAlign.NotSet) {
                TypeConverter hac = TypeDescriptor.GetConverter(typeof(HorizontalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Align, hac.ConvertToString(hAlign).ToLower(CultureInfo.InvariantCulture));
            }
            
            VerticalAlign vAlign = VerticalAlign;
            if (vAlign != VerticalAlign.NotSet) {
                TypeConverter hac = TypeDescriptor.GetConverter(typeof(VerticalAlign));
                writer.AddAttribute(HtmlTextWriterAttribute.Valign, hac.ConvertToString(vAlign).ToLower(CultureInfo.InvariantCulture));
            }
        }


        /// <devdoc>
        ///    <para>Copies non-blank elements from the specified style, overwriting existing
        ///       style elements if necessary.</para>
        /// </devdoc>
        public override void CopyFrom(Style s) {
            if (s != null && !s.IsEmpty) {

                base.CopyFrom(s);

                if (s is TableItemStyle) {
                    TableItemStyle ts = (TableItemStyle)s;

                    if (s.RegisteredCssClass.Length != 0) {
                        if (ts.IsSet(PROP_WRAP)) {
                            ViewState.Remove("Wrap");
                            ClearBit(PROP_WRAP);
                        }
                    }
                    else {
                        if (ts.IsSet(PROP_WRAP))
                            this.Wrap = ts.Wrap;
                    }

                    if (ts.IsSet(PROP_HORZALIGN))
                        this.HorizontalAlign = ts.HorizontalAlign;
                    if (ts.IsSet(PROP_VERTALIGN))
                        this.VerticalAlign = ts.VerticalAlign;

                }
            }
        }

        private bool IsControlEnableLegacyRendering(Control control) {
            if (control != null) {
                return control.EnableLegacyRendering;
            }
            else {
                return EnableLegacyRendering;
            }
        }


        /// <devdoc>
        ///    <para>Copies non-blank elements from the specified style, but will not overwrite
        ///       any existing style elements.</para>
        /// </devdoc>
        public override void MergeWith(Style s) {
            if (s != null && !s.IsEmpty) {

                if (IsEmpty) {
                    // merge into an empty style is equivalent to a copy, which
                    // is more efficient
                    CopyFrom(s);
                    return;
                }

                base.MergeWith(s);

                if (s is TableItemStyle) {
                    TableItemStyle ts = (TableItemStyle)s;

                    // Since we're already copying the registered CSS class in base.MergeWith, we don't
                    // need to any attributes that would be included in that class.
                    if (s.RegisteredCssClass.Length == 0) {
                        if (ts.IsSet(PROP_WRAP) && !this.IsSet(PROP_WRAP))
                            this.Wrap = ts.Wrap;
                    }

                    if (ts.IsSet(PROP_HORZALIGN) && !this.IsSet(PROP_HORZALIGN))
                        this.HorizontalAlign = ts.HorizontalAlign;
                    if (ts.IsSet(PROP_VERTALIGN) && !this.IsSet(PROP_VERTALIGN))
                        this.VerticalAlign = ts.VerticalAlign;

                }
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Clears out any defined style elements from the state bag.
        ///    </para>
        /// </devdoc>
        public override void Reset() {
            if (IsSet(PROP_HORZALIGN))
                ViewState.Remove("HorizontalAlign");
            if (IsSet(PROP_VERTALIGN))
                ViewState.Remove("VerticalAlign");
            if (IsSet(PROP_WRAP))
                ViewState.Remove("Wrap");

            base.Reset();
        }

        /// <devdoc>
        /// Only serialize if the Wrap property has changed.  This means that we serialize "false" 
        /// if they were set to false in the designer.
        /// </devdoc>
        private void ResetWrap() {
            ViewState.Remove("Wrap");
            ClearBit(PROP_WRAP);
        }

        /// <devdoc>
        /// Only serialize if the Wrap property has changed.  This means that we serialize "false" 
        /// if they were set to false in the designer.
        /// </devdoc>
        private bool ShouldSerializeWrap() {
            return IsSet(PROP_WRAP);
        }


    }
}

