namespace System.Web.DynamicData {
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Web.UI.WebControls;
    using System.Drawing;
    using System.Diagnostics;

    /// <summary>
    /// Control used to render Dynamic Data driven UI
    /// </summary>
    [ToolboxBitmap(typeof(DynamicControl), "DynamicControl.bmp")]
    public class DynamicControl : Control, IAttributeAccessor, IFieldTemplateHost, IFieldFormattingOptions {

        private bool _customConvertEmptyStringToNullSet;
        private bool _customApplyFormatInEditModeSet;
        private MetaTable _table;
        private IMetaTable _iMetaTable;
        private IDictionary<string, string> _attributes;
        private IMetaColumn _iMetaColumn;
        private MetaColumn _column;

        /// <summary>
        /// </summary>
        public DynamicControl() {
        }

        internal DynamicControl(IMetaTable table) {
            _iMetaTable = table;
        }

        internal DynamicControl(IMetaColumn column) {
            _iMetaColumn = column;
        }

        /// <param name="mode">The mode that the associated field template should be in (readonly, edit, insert)</param>
        public DynamicControl(DataBoundControlMode mode) {
            Mode = mode;
        }

        /// <summary>
        /// The name of the column that this control handles
        /// </summary>
        [
        Category("Data"),
        DefaultValue(""),
        ResourceDescription("DynamicControlFieldCommon_DataField")
        ]
        public string DataField {
            get {
                object o = ViewState["DataField"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                if (!String.Equals(value, ViewState["DataField"])) {
                    ViewState["DataField"] = value;
                }
            }
        }

        /// <summary>
        /// The MetaColumn that this control is working with
        /// </summary>
        [Browsable(false)]
        public MetaColumn Column {
            get {
                return _column;
            }
            set {
                _column = value;
                _iMetaColumn = value;
            }
        }

        /// <summary>
        /// The UIHint used by this control to locate the proper field template
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        ResourceDescription("DynamicControlFieldCommon_UIHint")
        ]
        public virtual string UIHint {
            get {
                object o = ViewState["UIHint"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["UIHint"] = value;
            }
        }

        /// <summary>
        /// Gets or sets the CSS class property of the DynamicControl
        /// </summary>
        [
        Category("Appearance"),
        DefaultValue(""),
        CssClassProperty()
        ]
        public virtual string CssClass {
            get {
                object o = ViewState["CssClass"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["CssClass"] = value;
            }
        }

        /// <summary>
        /// The MetaTable that this control is associated with
        /// </summary>
        [Browsable(false)]
        public virtual MetaTable Table {
            get {
                if (_table == null) {
                    _table = this.FindMetaTable();

                    if (_table == null) {
                        throw new Exception(String.Format(CultureInfo.CurrentCulture,
                             DynamicDataResources.DynamicControl_ControlNeedsToExistInADataControlUsingDynamicDataSource));
                    }
                }
                return _table;
            }
        }

        private IMetaTable IMetaTable {
            get {
                return _iMetaTable ?? Table;
            }
        }

        private IMetaColumn IMetaColumn {
            get {
                return _iMetaColumn;
            }
            set {
                _iMetaColumn = value;
                _column = value as MetaColumn;
            }
        }

        /// <summary>
        /// The rendering mode: readonly, edit or insert
        /// </summary>
        [
        DefaultValue(DataBoundControlMode.ReadOnly),
        Category("Behavior"),
        ResourceDescription("DynamicField_Mode")
        ]
        public DataBoundControlMode Mode {
            get {
                object o = ViewState["Mode"];
                return ((o == null) ? DataBoundControlMode.ReadOnly : (DataBoundControlMode)o);
            }
            set {
                ViewState["Mode"] = value;
            }
        }

        /// <summary>
        /// The validation group that the field template need to be in
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        Themeable(false),
        ResourceDescription("DynamicControlFieldCommon_ValidationGroup")
        ]
        public virtual string ValidationGroup {
            get {
                object o = ViewState["ValidationGroup"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["ValidationGroup"] = value;
            }
        }

        internal Control CreateControl() {
            Debug.Assert(FieldTemplate == null);

            FieldTemplate = (Control)IMetaColumn.Model.FieldTemplateFactory.CreateFieldTemplate(Column, Mode, UIHint);
            if (FieldTemplate != null) {
                ((IFieldTemplate)FieldTemplate).SetHost(this);

                // If we got some extra attributes declared on the tag, assign them to the user control if it has matching properties
                if (_attributes != null) {
                    var ucType = FieldTemplate.GetType();
                    foreach (var entry in _attributes) {
                        // Look for a public property by that name on th user control
                        var propInfo = ucType.GetProperty(entry.Key, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
                        if (propInfo != null) {
                            // Convert the value to the type of the property and set it
                            var value = PropertyConverter.ObjectFromString(propInfo.PropertyType, propInfo, entry.Value);
                            propInfo.SetValue(FieldTemplate, value, null);
                        }
                    }
                }

                // Give it the column name as its ID, unless there is already a control by that name
                string id = GetControlIDFromColumnName(IMetaColumn.Name);
                if (FindControl(id) == null)
                    FieldTemplate.ID = id;
            }
            return FieldTemplate;
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        [SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            // Don't do anything in Design mode
            if (DesignMode)
                return;

            ResolveColumn();
            FieldTemplate = CreateControl();
            // Add it to the tree
            if (FieldTemplate != null) {
                Controls.Add(FieldTemplate);
            }
        }

        /// <summary>
        /// Renders the underlying field template control. If this DynamicControl's CssClass tag is defined,
        /// the underlying control's output will be wrapped in a span tag with that css class applied.
        /// </summary>
        /// <param name="writer"></param>
        protected override void Render(HtmlTextWriter writer) {

            // In Design mode, simply render the string Databound.  Ideally, we'd do something fancier
            // that takes into account the type of the column.  But at lesat, rendering *something* makes
            // the design layout look reasonable
            if (DesignMode) {
                writer.Write(DynamicDataResources.DynamicControlDesignRender);
                return;
            }

            // If there is a CSS class, output it in a span tag.  Otherwise, don't use any extra tag.
            if (!String.IsNullOrEmpty(CssClass)) {
                writer.AddAttribute(HtmlTextWriterAttribute.Class, CssClass);
                writer.RenderBeginTag(HtmlTextWriterTag.Span);
                base.Render(writer);
                writer.RenderEndTag();
            }
            else {
                base.Render(writer);
            }
        }

        internal void ResolveColumn() {
            if (IMetaColumn == null) {
                if (String.IsNullOrEmpty(DataField)) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.DynamicControl_ControlMustHaveDateFieldAttribute, GetType().Name, ID));
                }

                IMetaColumn = IMetaTable.GetColumn(DataField);
                

                // Try to get them various settings from the model, unless they were explicitely
                // specified on the control
                if (String.IsNullOrEmpty(UIHint)) {
                    UIHint = IMetaColumn.UIHint;
                }
                if (String.IsNullOrEmpty(DataFormatString)) {
                    DataFormatString = IMetaColumn.DataFormatString;
                }
                if (String.IsNullOrEmpty(NullDisplayText)) {
                    NullDisplayText = IMetaColumn.NullDisplayText;
                }
                if (!_customConvertEmptyStringToNullSet) {
                    ConvertEmptyStringToNull = IMetaColumn.ConvertEmptyStringToNull;
                }
                if (!_customApplyFormatInEditModeSet) {
                    ApplyFormatInEditMode = IMetaColumn.ApplyFormatInEditMode;
                }
                if (ViewState["HtmlEncode"] == null) {
                    HtmlEncode = IMetaColumn.HtmlEncode;
                }
            }
        }

        /// <summary>
        /// The Field Template that was created for this control
        /// </summary>
        [Browsable(false)]
        public Control FieldTemplate { get; private set; }

        internal static string GetControlIDFromColumnName(string columnName) {
            return "__" + columnName;
        }

        internal void SetAttributes(IDictionary<string, string> attributes) {
            _attributes = attributes;
        }

        #region IAttributeAccessor Members

        /// <summary>
        /// See IAttributeAccessor
        /// </summary>
        public string GetAttribute(string key) {
            if (_attributes == null)
                return String.Empty;
            return _attributes[key];
        }

        /// <summary>
        /// See IAttributeAccessor
        /// </summary>
        public void SetAttribute(string key, string value) {
            if (_attributes == null) {
                _attributes = new Dictionary<string, string>();
            }
            _attributes[key] = value;
        }

        #endregion

        #region IFieldTemplateHost Members

        IFieldFormattingOptions IFieldTemplateHost.FormattingOptions {
            get { return this; }
        }

        #endregion

        #region IFieldFormattingOptions Members

        /// <summary>
        /// See IFieldFormattingOptions
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        ResourceDescription("DynamicControlFieldCommon_ConvertEmptyStringToNull")
        ]
        public bool ConvertEmptyStringToNull {
            get {
                object o = ViewState["ConvertEmptyStringToNull"];
                return (o == null ? false : (bool)o);
            }
            set {
                _customConvertEmptyStringToNullSet = true;
                ViewState["ConvertEmptyStringToNull"] = value;
            }
        }

        /// <summary>
        /// See IFieldFormattingOptions
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(false),
        ResourceDescription("DynamicControlFieldCommon_ApplyFormatInEditMode")
        ]
        public bool ApplyFormatInEditMode {
            get {
                object o = ViewState["ApplyFormatInEditMode"];
                return (o == null ? false : (bool)o);
            }
            set {
                _customApplyFormatInEditModeSet = true;
                ViewState["ApplyFormatInEditMode"] = value;
            }
        }

        /// <summary>
        /// See IFieldFormattingOptions
        /// </summary>
        [
        Category("Data"),
        DefaultValue(""),
        ResourceDescription("DynamicControlFieldCommon_DataFormatString")
        ]
        public string DataFormatString {
            get {
                object o = ViewState["DataFormatString"];
                return (o == null ? String.Empty : (string)o);
            }
            set {
                ViewState["DataFormatString"] = value;
            }
        }

        /// <summary>
        /// See IFieldFormattingOptions
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(true),
        ResourceDescription("DynamicControlFieldCommon_HtmlEncode")
        ]
        public bool HtmlEncode {
            get {
                object o = ViewState["HtmlEncode"];
                return (o == null ? true : (bool)o);
            }
            set {
                ViewState["HtmlEncode"] = value;
            }
        }

        /// <summary>
        /// See IFieldFormattingOptions
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        ResourceDescription("DynamicControlFieldCommon_NullDisplayText")
        ]
        public string NullDisplayText {
            get {
                object o = ViewState["NullDisplayText"];
                return (o == null ? String.Empty : (string)o);
            }
            set {
                ViewState["NullDisplayText"] = value;
            }
        }

        #endregion
    }
}
