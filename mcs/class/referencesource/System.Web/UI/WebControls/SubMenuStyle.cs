//------------------------------------------------------------------------------
// <copyright file="SubMenuStyle.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {
    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Text;
    using System.Web.UI;

    /// <devdoc>
    ///    Specifies the style of a SubMenu.
    /// </devdoc>
    public class SubMenuStyle : Style, ICustomTypeDescriptor {
        private const int PROP_VPADDING = 0x00010000;
        private const int PROP_HPADDING = 0x00020000;

        public SubMenuStyle() : base() {
        }

        public SubMenuStyle(StateBag bag) : base(bag) {
        }

        /// <devdoc>
        /// Gets and sets the horizontal padding around the node text
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), ""),
        WebCategory("Layout"),
        NotifyParentProperty(true),
        WebSysDescription(SR.SubMenuStyle_HorizontalPadding),
        ]
        public Unit HorizontalPadding {
            get {
                if (IsSet(PROP_HPADDING)) {
                    return (Unit)(ViewState["HorizontalPadding"]);
                }
                return Unit.Empty;
            }
            set {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["HorizontalPadding"] = value;
                SetBit(PROP_HPADDING);
            }
        }


        /// <devdoc>
        /// Gets and sets the vertical padding around the node text
        /// </devdoc>
        [
        DefaultValue(typeof(Unit), ""),
        WebCategory("Layout"),
        NotifyParentProperty(true),
        WebSysDescription(SR.SubMenuStyle_VerticalPadding),
        ]
        public Unit VerticalPadding {
            get {
                if (IsSet(PROP_VPADDING)) {
                    return (Unit)(ViewState["VerticalPadding"]);
                }
                return Unit.Empty;
            }
            set {
                if ((value.Type == UnitType.Percentage) || (value.Value < 0)) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["VerticalPadding"] = value;
                SetBit(PROP_VPADDING);
            }
        }


        /// <devdoc>
        ///    Copies non-blank elements from the specified style, overwriting existing
        ///    style elements if necessary.
        /// </devdoc>
        public override void CopyFrom(Style s) {
            if (s != null) {
                base.CopyFrom(s);

                SubMenuStyle sms = s as SubMenuStyle;
                if (sms != null && !sms.IsEmpty) {
                    // Only copy the paddings if they aren't in the source Style's registered CSS class
                    if (s.RegisteredCssClass.Length != 0) {
                        if (sms.IsSet(PROP_VPADDING)) {
                            ViewState.Remove("VerticalPadding");
                            ClearBit(PROP_VPADDING);
                        }

                        if (sms.IsSet(PROP_HPADDING)) {
                            ViewState.Remove("HorizontalPadding");
                            ClearBit(PROP_HPADDING);
                        }
                    }
                    else {
                        if (sms.IsSet(PROP_VPADDING)) {
                            this.VerticalPadding = sms.VerticalPadding;
                        }

                        if (sms.IsSet(PROP_HPADDING)) {
                            this.HorizontalPadding = sms.HorizontalPadding;
                        }
                    }
                }
            }
        }


        protected override void FillStyleAttributes(CssStyleCollection attributes, IUrlResolutionService urlResolver) {
            // The style will be rendered on container elements that does not contain text directly.
            // It does not render font and forecolor.
            // Users should set font and forecolor on MenuItems styles.
            // Copying the code from the base class, except for the part that deals with Font and ForeColor.
            StateBag viewState = ViewState;
            Color c;

            // BackColor
            if (base.IsSet(PROP_BACKCOLOR)) {
                c = (Color)viewState["BackColor"];
                if (!c.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.BackgroundColor, ColorTranslator.ToHtml(c));
                }
            }

            // BorderColor
            if (base.IsSet(PROP_BORDERCOLOR)) {
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

            Unit u;

            // Height
            if (base.IsSet(PROP_HEIGHT)) {
                u = (Unit)viewState["Height"];
                if (!u.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.Height, u.ToString(CultureInfo.InvariantCulture));
                }
            }

            // Width
            if (base.IsSet(PROP_WIDTH)) {
                u = (Unit)viewState["Width"];
                if (!u.IsEmpty) {
                    attributes.Add(HtmlTextWriterStyle.Width, u.ToString(CultureInfo.InvariantCulture));
                }
            }

            if (!HorizontalPadding.IsEmpty || !VerticalPadding.IsEmpty) {
                attributes.Add(HtmlTextWriterStyle.Padding, string.Format(CultureInfo.InvariantCulture,
                    "{0} {1} {0} {1}",
                    VerticalPadding.IsEmpty ? Unit.Pixel(0) : VerticalPadding,
                    HorizontalPadding.IsEmpty ? Unit.Pixel(0) : HorizontalPadding));
            }
        }


        /// <devdoc>
        ///    Copies non-blank elements from the specified style, but will not overwrite
        ///    any existing style elements.
        /// </devdoc>
        public override void MergeWith(Style s) {
            if (s != null) {
                if (IsEmpty) {
                    // Merging with an empty style is equivalent to copying,
                    // which is more efficient.
                    CopyFrom(s);
                    return;
                }

                base.MergeWith(s);

                SubMenuStyle sms = s as SubMenuStyle;
                // Since we're already copying the registered CSS class in base.MergeWith, we don't
                // need to any attributes that would be included in that class.
                if (sms != null && !sms.IsEmpty && s.RegisteredCssClass.Length == 0) {
                    if (sms.IsSet(PROP_VPADDING) && !this.IsSet(PROP_VPADDING)) {
                        this.VerticalPadding = sms.VerticalPadding;
                    }

                    if (sms.IsSet(PROP_HPADDING) && !this.IsSet(PROP_HPADDING)) {
                        this.HorizontalPadding = sms.HorizontalPadding;
                    }
                }
            }
        }


        /// <devdoc>
        ///    Clears out any defined style elements from the state bag.
        /// </devdoc>
        public override void Reset() {
            if (IsSet(PROP_VPADDING))
                ViewState.Remove("VerticalPadding");
            if (IsSet(PROP_HPADDING))
                ViewState.Remove("HorizontalPadding");

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

			PropertyDescriptor fontProperty = oldProperties["Font"];
			PropertyDescriptor forecolorProperty = oldProperties["ForeColor"];

			Attribute[] newAttributes = new Attribute[] {
                new BrowsableAttribute(false),
                new EditorBrowsableAttribute(EditorBrowsableState.Never),
                new ThemeableAttribute(false),
            };

			for (int i = 0; i < oldProperties.Count; i++) {
				PropertyDescriptor property = oldProperties[i];
				if ((property == fontProperty) || (property == forecolorProperty)) {
					newProperties[i] = TypeDescriptor.CreateProperty(GetType(), property, newAttributes);
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
