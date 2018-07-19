//------------------------------------------------------------------------------
// <copyright file="WebPartMenuStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <devdoc>
    /// </devdoc>
    public sealed class WebPartMenuStyle : TableStyle, ICustomTypeDescriptor {

        private const int PROP_SHADOWCOLOR = 0x00200000;

        public WebPartMenuStyle() : this(null) {
        }

        public WebPartMenuStyle(StateBag bag) : base(bag) {
            CellPadding = 1;
            CellSpacing = 0;
        }

        /// <devdoc>
        /// </devdoc>
        [
        WebCategory("Appearance"),
        DefaultValue(typeof(Color), ""),
        TypeConverterAttribute(typeof(WebColorConverter)),
        WebSysDescription(SR.WebPartMenuStyle_ShadowColor)
        ]
        public Color ShadowColor {
            get {
                if (IsSet(PROP_SHADOWCOLOR)) {
                    return (Color)(ViewState["ShadowColor"]);
                }
                return Color.Empty;
            }
            set {
                ViewState["ShadowColor"] = value;
                SetBit(PROP_SHADOWCOLOR);
            }
        }

        /// <devdoc>
        /// </devdoc>
        [
        Browsable(false),
        DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden),
        EditorBrowsable(EditorBrowsableState.Never)
        ]
        public override HorizontalAlign HorizontalAlign {
            get {
                return base.HorizontalAlign;
            }
            set {
            }
        }

        /// <internalonly/>
        protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver) {
            base.FillStyleAttributes(attributes, urlResolver);

            Color shadowColor = ShadowColor;
            if (shadowColor.IsEmpty == false) {
                string colorValue = ColorTranslator.ToHtml(shadowColor);
                string filterValue = "progid:DXImageTransform.Microsoft.Shadow(color='" + colorValue + "', Direction=135, Strength=3)";

                attributes.Add(HtmlTextWriterStyle.Filter, filterValue);
            }
        }

        /// <internalonly/>
        public override void CopyFrom(Style s) {
            if (s != null && !s.IsEmpty) {
                base.CopyFrom(s);

                if (s is WebPartMenuStyle) {
                    WebPartMenuStyle ms = (WebPartMenuStyle)s;

                    // Only copy the BackImageUrl if it isn't in the source Style's registered CSS class
                    if (s.RegisteredCssClass.Length != 0) {
                        if (ms.IsSet(PROP_SHADOWCOLOR)) {
                            ViewState.Remove("ShadowColor");
                            ClearBit(PROP_SHADOWCOLOR);
                        }
                    }
                    else {
                        if (ms.IsSet(PROP_SHADOWCOLOR)) {
                            this.ShadowColor = ms.ShadowColor;
                        }
                    }
                }
            }
        }

        /// <internalonly/>
        public override void MergeWith(Style s) {
            if (s != null && !s.IsEmpty) {
                if (IsEmpty) {
                    // merge into an empty style is equivalent to a copy,
                    // which is more efficient
                    CopyFrom(s);
                    return;
                }

                base.MergeWith(s);

                if (s is WebPartMenuStyle) {
                    WebPartMenuStyle ms = (WebPartMenuStyle)s;

                    // Since we're already copying the registered CSS class in base.MergeWith, we don't
                    // need to any attributes that would be included in that class.
                    if (s.RegisteredCssClass.Length == 0) {
                        if (ms.IsSet(PROP_SHADOWCOLOR) && !this.IsSet(PROP_SHADOWCOLOR))
                            this.ShadowColor = ms.ShadowColor;
                    }
                }
            }
        }

        /// <internalonly/>
        public override void Reset() {
            if (IsSet(PROP_SHADOWCOLOR)) {
                ViewState.Remove("ShadowColor");
            }

            base.Reset();
        }

        #region ICustomTypeDesciptor implementation
        System.ComponentModel.AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return TypeDescriptor.GetAttributes(this, true);
        }

        string ICustomTypeDescriptor.GetClassName() {
            return TypeDescriptor.GetClassName(this, true);
        }

        string ICustomTypeDescriptor.GetComponentName() {
            return TypeDescriptor.GetComponentName(this, true);
        }

        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return TypeDescriptor.GetConverter(this, true);
        }

        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }

        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }

        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return TypeDescriptor.GetEvents(this, true);
        }

        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            return ((ICustomTypeDescriptor)this).GetProperties(null);
        }

        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            PropertyDescriptorCollection oldProperties = TypeDescriptor.GetProperties(GetType(), attributes);
            PropertyDescriptor[] newProperties = new PropertyDescriptor[oldProperties.Count];

            PropertyDescriptor oldPaddingProperty = oldProperties["CellPadding"];
            PropertyDescriptor newPaddingProperty =
                TypeDescriptor.CreateProperty(GetType(), oldPaddingProperty, new DefaultValueAttribute(1));

            PropertyDescriptor oldSpacingProperty = oldProperties["CellSpacing"];
            PropertyDescriptor newSpacingProperty =
                TypeDescriptor.CreateProperty(GetType(), oldSpacingProperty, new DefaultValueAttribute(0));

            PropertyDescriptor oldFontProperty = oldProperties["Font"];
            PropertyDescriptor newFontProperty =
                TypeDescriptor.CreateProperty(GetType(), oldFontProperty,
                    new BrowsableAttribute(false),
                    new ThemeableAttribute(false),
                    new EditorBrowsableAttribute(EditorBrowsableState.Never));

            PropertyDescriptor oldForeColorProperty = oldProperties["ForeColor"];
            PropertyDescriptor newForeColorProperty =
                TypeDescriptor.CreateProperty(GetType(), oldForeColorProperty,
                    new BrowsableAttribute(false),
                    new ThemeableAttribute(false),
                    new EditorBrowsableAttribute(EditorBrowsableState.Never));

            for (int i = 0; i < oldProperties.Count; i++) {
                PropertyDescriptor property = oldProperties[i];
                if (property == oldPaddingProperty) {
                    newProperties[i] = newPaddingProperty;
                }
                else if (property == oldSpacingProperty) {
                    newProperties[i] = newSpacingProperty;
                }
                else if (property == oldFontProperty) {
                    newProperties[i] = newFontProperty;
                }
                else if (property == oldForeColorProperty) {
                    newProperties[i] = newForeColorProperty;
                }
                else {
                    newProperties[i] = property;
                }
            }

            return new PropertyDescriptorCollection(newProperties, true);
        }

        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return this;
        }
        #endregion //ICustomTypeDescriptor implementation
    }
}
