//------------------------------------------------------------------------------
// <copyright file="AppearanceEditorPart.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls.WebParts {

    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    public sealed class AppearanceEditorPart : EditorPart {

        private TextBox _title;
        private UnitInput _height;
        private UnitInput _width;
        private DropDownList _chromeType;
        private CheckBox _hidden;
        private DropDownList _direction;

        private string _titleErrorMessage;
        private string _heightErrorMessage;
        private string _widthErrorMessage;
        private string _chromeTypeErrorMessage;
        private string _hiddenErrorMessage;
        private string _directionErrorMessage;

        private const int TextBoxColumns = 30;
        private const int MinUnitValue = 0;
        private const int MaxUnitValue = 32767;

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Themeable(false)]
        public override string DefaultButton {
            get { return base.DefaultButton; }
            set { base.DefaultButton = value; }
        }

        private bool HasError {
            get {
                return (_titleErrorMessage != null || _heightErrorMessage != null ||
                        _widthErrorMessage != null || _chromeTypeErrorMessage != null ||
                        _hiddenErrorMessage != null || _directionErrorMessage != null);
            }
        }

        [
        WebSysDefaultValue(SR.AppearanceEditorPart_PartTitle),
        ]
        public override string Title {
            get {
                string s = (string)ViewState["Title"];
                return (s != null) ? s : SR.GetString(SR.AppearanceEditorPart_PartTitle);
            }
            set {
                ViewState["Title"] = value;
            }
        }

        public override bool ApplyChanges() {
            WebPart webPart = WebPartToEdit;

            Debug.Assert(webPart != null);
            if (webPart != null) {
                EnsureChildControls();
                bool allowLayoutChange = webPart.Zone.AllowLayoutChange;

                try {
                    webPart.Title = _title.Text;
                }
                catch (Exception e) {
                    _titleErrorMessage = CreateErrorMessage(e.Message);
                }

                if (allowLayoutChange) {
                    try {
                        TypeConverter chromeTypeConverter = TypeDescriptor.GetConverter(typeof(PartChromeType));
                        webPart.ChromeType = (PartChromeType)chromeTypeConverter.ConvertFromString(_chromeType.SelectedValue);
                    }
                    catch (Exception e) {
                        _chromeTypeErrorMessage = CreateErrorMessage(e.Message);
                    }
                }

                try {
                    TypeConverter directionConverter = TypeDescriptor.GetConverter(typeof(ContentDirection));
                    webPart.Direction = (ContentDirection)directionConverter.ConvertFromString(_direction.SelectedValue);
                }
                catch (Exception e) {
                    _directionErrorMessage = CreateErrorMessage(e.Message);
                }

                if (allowLayoutChange) {
                    Unit height = Unit.Empty;
                    string heightValueString = _height.Value;
                    if (!String.IsNullOrEmpty(heightValueString)) {
                        double heightValue;
                        if (Double.TryParse(_height.Value, NumberStyles.Float | NumberStyles.AllowThousands,
                                            CultureInfo.CurrentCulture, out heightValue)) {
                            if (heightValue < MinUnitValue) {
                                _heightErrorMessage = SR.GetString(SR.EditorPart_PropertyMinValue, MinUnitValue.ToString(CultureInfo.CurrentCulture));
                            }
                            else if (heightValue > MaxUnitValue) {
                                _heightErrorMessage = SR.GetString(SR.EditorPart_PropertyMaxValue, MaxUnitValue.ToString(CultureInfo.CurrentCulture));
                            }
                            else {
                                height = new Unit(heightValue, _height.Type);
                            }
                        }
                        else {
                            _heightErrorMessage = SR.GetString(SR.EditorPart_PropertyMustBeDecimal);
                        }
                    }

                    if (_heightErrorMessage == null) {
                        try {
                            webPart.Height = (Unit)height;
                        }
                        catch (Exception e) {
                            _heightErrorMessage = CreateErrorMessage(e.Message);
                        }
                    }
                }

                if (allowLayoutChange) {
                    Unit width = Unit.Empty;
                    string widthValueString = _width.Value;
                    if (!String.IsNullOrEmpty(widthValueString)) {
                        double widthValue;
                        if (Double.TryParse(_width.Value, NumberStyles.Float| NumberStyles.AllowThousands,
                                            CultureInfo.CurrentCulture, out widthValue)) {
                            if (widthValue < MinUnitValue) {
                                _widthErrorMessage = SR.GetString(SR.EditorPart_PropertyMinValue, MinUnitValue.ToString(CultureInfo.CurrentCulture));
                            }
                            else if (widthValue > MaxUnitValue) {
                                _widthErrorMessage = SR.GetString(SR.EditorPart_PropertyMaxValue, MaxUnitValue.ToString(CultureInfo.CurrentCulture));
                            }
                            else {
                                width = new Unit(widthValue, _width.Type);
                            }
                        }
                        else {
                            _widthErrorMessage = SR.GetString(SR.EditorPart_PropertyMustBeDecimal);
                        }
                    }
                    if (_widthErrorMessage == null) {
                        try {
                            webPart.Width = (Unit)width;
                        }
                        catch (Exception e) {
                            _widthErrorMessage = CreateErrorMessage(e.Message);
                        }
                    }
                }

                if (allowLayoutChange && webPart.AllowHide) {
                    try {
                        webPart.Hidden = _hidden.Checked;
                    }
                    catch (Exception e) {
                        _hiddenErrorMessage = CreateErrorMessage(e.Message);
                    }
                }
            }

            return !HasError;
        }

        protected internal override void CreateChildControls() {
            ControlCollection controls = Controls;
            controls.Clear();

            _title = new TextBox();
            _title.Columns = TextBoxColumns;
            controls.Add(_title);

            TypeConverter chromeTypeConverter = TypeDescriptor.GetConverter(typeof(PartChromeType));
            _chromeType = new DropDownList();
            _chromeType.Items.Add(new ListItem(SR.GetString(SR.PartChromeType_Default),
                                              chromeTypeConverter.ConvertToString(PartChromeType.Default)));
            _chromeType.Items.Add(new ListItem(SR.GetString(SR.PartChromeType_TitleAndBorder),
                                              chromeTypeConverter.ConvertToString(PartChromeType.TitleAndBorder)));
            _chromeType.Items.Add(new ListItem(SR.GetString(SR.PartChromeType_TitleOnly),
                                              chromeTypeConverter.ConvertToString(PartChromeType.TitleOnly)));
            _chromeType.Items.Add(new ListItem(SR.GetString(SR.PartChromeType_BorderOnly),
                                              chromeTypeConverter.ConvertToString(PartChromeType.BorderOnly)));
            _chromeType.Items.Add(new ListItem(SR.GetString(SR.PartChromeType_None),
                                              chromeTypeConverter.ConvertToString(PartChromeType.None)));
            controls.Add(_chromeType);

            TypeConverter directionConverter = TypeDescriptor.GetConverter(typeof(ContentDirection));
            _direction = new DropDownList();
            _direction.Items.Add(new ListItem(SR.GetString(SR.ContentDirection_NotSet),
                                              directionConverter.ConvertToString(ContentDirection.NotSet)));
            _direction.Items.Add(new ListItem(SR.GetString(SR.ContentDirection_LeftToRight),
                                              directionConverter.ConvertToString(ContentDirection.LeftToRight)));
            _direction.Items.Add(new ListItem(SR.GetString(SR.ContentDirection_RightToLeft),
                                              directionConverter.ConvertToString(ContentDirection.RightToLeft)));
            controls.Add(_direction);

            _height = new UnitInput();
            controls.Add(_height);

            _width = new UnitInput();
            controls.Add(_width);

            _hidden = new CheckBox();
            controls.Add(_hidden);

            // We don't need viewstate enabled on our child controls.  Disable for perf.
            foreach (Control c in controls) {
                c.EnableViewState = false;
            }
        }

        protected internal override void OnPreRender(EventArgs e) {
            base.OnPreRender(e);

            // We want to synchronize the EditorPart to the state of the WebPart on every page load,
            // so we stay current if the WebPart changes in the background.
            if (Display && Visible && !HasError) {
                SyncChanges();
            }
        }

        protected internal override void RenderContents(HtmlTextWriter writer) {
            if (Page != null) {
                Page.VerifyRenderingInServerForm(this);
            }

            // HACK: Need this for child controls to be created at design-time when control is inside template
            EnsureChildControls();

            string[] propertyDisplayNames = new string[] {
                SR.GetString(SR.AppearanceEditorPart_Title),
                SR.GetString(SR.AppearanceEditorPart_ChromeType),
                SR.GetString(SR.AppearanceEditorPart_Direction),
                SR.GetString(SR.AppearanceEditorPart_Height),
                SR.GetString(SR.AppearanceEditorPart_Width),
                SR.GetString(SR.AppearanceEditorPart_Hidden),
            };

            WebControl[] propertyEditors = new WebControl[] {
                _title,
                _chromeType,
                _direction,
                _height,
                _width,
                _hidden,
            };

            string[] errorMessages = new string[] {
                _titleErrorMessage,
                _chromeTypeErrorMessage,
                _directionErrorMessage,
                _heightErrorMessage,
                _widthErrorMessage,
                _hiddenErrorMessage,
            };

            RenderPropertyEditors(writer, propertyDisplayNames, null /* propertyDescriptions */,
                                  propertyEditors, errorMessages);
        }

        public override void SyncChanges() {
            WebPart webPart = WebPartToEdit;

            Debug.Assert(webPart != null);
            if (webPart != null) {
                bool allowLayoutChange = webPart.Zone.AllowLayoutChange;

                EnsureChildControls();
                _title.Text = webPart.Title;

                TypeConverter chromeTypeConverter = TypeDescriptor.GetConverter(typeof(PartChromeType));
                _chromeType.SelectedValue = chromeTypeConverter.ConvertToString(webPart.ChromeType);
                _chromeType.Enabled = allowLayoutChange;

                TypeConverter directionConverter = TypeDescriptor.GetConverter(typeof(ContentDirection));
                _direction.SelectedValue = directionConverter.ConvertToString(webPart.Direction);

                _height.Unit = webPart.Height;
                _height.Enabled = allowLayoutChange;

                _width.Unit = webPart.Width;
                _width.Enabled = allowLayoutChange;

                _hidden.Checked = webPart.Hidden;
                _hidden.Enabled = allowLayoutChange && webPart.AllowHide;
            }
        }

        private sealed class UnitInput : CompositeControl {
            private TextBox _value;
            private DropDownList _type;
            private const int TextBoxColumns = 2;

            public string Value {
                get {
                    return (_value != null) ? _value.Text : String.Empty;
                }
            }

            public UnitType Type {
                get {
                    return (_type != null) ?
                        (UnitType)Int32.Parse(_type.SelectedValue, CultureInfo.InvariantCulture) : (UnitType)0;
                }
            }

            public Unit Unit {
                set {
                    EnsureChildControls();
                    if (value == Unit.Empty) {
                        _value.Text = String.Empty;
                        _type.SelectedIndex = 0;
                    }
                    else {
                        _value.Text = value.Value.ToString(CultureInfo.CurrentCulture);
                        _type.SelectedValue = ((int)value.Type).ToString(CultureInfo.InvariantCulture);
                    }
                }
            }

            protected internal override void CreateChildControls() {
                Controls.Clear();

                _value = new TextBox();
                _value.Columns = TextBoxColumns;
                Controls.Add(_value);

                _type = new DropDownList();
                _type.Items.Add(new ListItem(SR.GetString(SR.AppearanceEditorPart_Pixels),
                                             ((int)UnitType.Pixel).ToString(CultureInfo.InvariantCulture)));
                _type.Items.Add(new ListItem(SR.GetString(SR.AppearanceEditorPart_Points),
                                             ((int)UnitType.Point).ToString(CultureInfo.InvariantCulture)));
                _type.Items.Add(new ListItem(SR.GetString(SR.AppearanceEditorPart_Picas),
                                             ((int)UnitType.Pica).ToString(CultureInfo.InvariantCulture)));
                _type.Items.Add(new ListItem(SR.GetString(SR.AppearanceEditorPart_Inches),
                                             ((int)UnitType.Inch).ToString(CultureInfo.InvariantCulture)));
                _type.Items.Add(new ListItem(SR.GetString(SR.AppearanceEditorPart_Millimeters),
                                             ((int)UnitType.Mm).ToString(CultureInfo.InvariantCulture)));
                _type.Items.Add(new ListItem(SR.GetString(SR.AppearanceEditorPart_Centimeters),
                                             ((int)UnitType.Cm).ToString(CultureInfo.InvariantCulture)));
                _type.Items.Add(new ListItem(SR.GetString(SR.AppearanceEditorPart_Percent),
                                             ((int)UnitType.Percentage).ToString(CultureInfo.InvariantCulture)));
                _type.Items.Add(new ListItem(SR.GetString(SR.AppearanceEditorPart_Em),
                                             ((int)UnitType.Em).ToString(CultureInfo.InvariantCulture)));
                _type.Items.Add(new ListItem(SR.GetString(SR.AppearanceEditorPart_Ex),
                                             ((int)UnitType.Ex).ToString(CultureInfo.InvariantCulture)));
                Controls.Add(_type);
            }

            protected internal override void Render(HtmlTextWriter writer) {
                // Needed for designtime
                EnsureChildControls();

                _value.ApplyStyle(ControlStyle);
                _value.RenderControl(writer);

                writer.Write("&nbsp;");

                _type.ApplyStyle(ControlStyle);
                _type.RenderControl(writer);
            }
        }
    }
}
