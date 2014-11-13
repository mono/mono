namespace System.Web.DynamicData {
    using System;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Web.Resources;
    using System.Diagnostics;
    
    /// <summary>
    /// Field type that can display DynamicData UI
    /// </summary>
    [Designer("System.Web.DynamicData.Design.DynamicFieldDesigner, " + AssemblyRef.SystemWebDynamicDataDesign)]
    public class DynamicField : DataControlField, IAttributeAccessor, IFieldFormattingOptions {

        private bool _customConvertEmptyStringToNullSet;
        private bool _customApplyFormatInEditModeSet;
        private MetaColumn _column;
        private IDictionary<string, string> _attributes;

        /// <summary>
        /// same as base. uses column's display name if possible
        /// </summary>
        public override string HeaderText {
            get {
                object o = ViewState["HeaderText"];
                if (o != null)
                    return (string)o;

                // Default to the Column's DisplayName
                if (Column != null)
                    return Column.DisplayName;

                // If we couldn't get it, use the name if the data field
                return DataField;
            }
            set {
                base.HeaderText = value;
            }
        }

        /// <summary>
        /// same as base. uses column's SortExpression property, if possible.
        /// </summary>
        public override string SortExpression {
            get {
                object o = ViewState["SortExpression"];
                if (o != null)
                    return (string)o;

                // Default to the Column's SortExpression
                if (Column != null)
                    return Column.SortExpression;

                return String.Empty;
            }
            set {
                base.SortExpression = value;
            }
        }

        /// <summary>
        /// Determines whether the control validates client input or not, defaults to inherit from parent.
        /// </summary>
        [
        Category("Behavior"),
        ResourceDescription("DynamicField_ValidateRequestMode"),
        DefaultValue(ValidateRequestMode.Inherit)
        ]
        public new ValidateRequestMode ValidateRequestMode {
            get {
                return base.ValidateRequestMode;
            }
            set {
                base.ValidateRequestMode = value;
            }
        }

        [
        SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "ReadOnly", Justification="Matches DataBoundControlMode value"),
        DefaultValue(false),
        Category("Behavior"),
        ResourceDescription("DynamicField_ReadOnly"),
        ]
        /// <summary>
        /// Forces this DynamicField to always load a ReadOnly template
        /// </summary>
        public virtual bool ReadOnly {
            get {
                object o = ViewState["ReadOnly"];
                return (o == null ? false : (bool)o);
            }
            set {
                ViewState["ReadOnly"] = value;
            }
        }

        /// <summary>
        /// The name of the column that this field handles
        /// </summary>
        [
        Category("Data"),
        DefaultValue(""),
        ResourceDescription("DynamicControlFieldCommon_DataField")
        ]
        public virtual string DataField {
            get {
                object o = ViewState["DataField"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                if (!String.Equals(value, ViewState["DataField"])) {
                    ViewState["DataField"] = value;
                    OnFieldChanged();
                }
            }
        }

        /// <summary>
        /// The MetaColumn that this fiedl is working with
        /// </summary>
        protected MetaColumn Column {
            get {
                // Don't do anything in Design mode. In some cases in the Designer (in the Edit field dialog),
                // DesignMode actually returns true, so checking for a null Control provides an additional check.
                if (DesignMode || Control == null)
                    return null;

                if (_column == null) {
                    MetaTable table = Control.FindMetaTable();
                    if (table == null) {
                        throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, DynamicDataResources.DynamicControl_ControlNeedsToExistInADataControlUsingDynamicDataSource));
                    }
                    _column = table.GetColumn(DataField);
                }
                return _column;
            }
        }

        /// <summary>
        /// An optional UIHint specified on the field
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
                if (!String.Equals(value, ViewState["UIHint"])) {
                    ViewState["UIHint"] = value;
                    OnFieldChanged();
                }
            }
        }

        /// <summary>
        /// The validation group that the field template needs to be in
        /// </summary>
        [
        Category("Behavior"),
        DefaultValue(""),
        ResourceDescription("DynamicControlFieldCommon_ValidationGroup")
        ]
        public virtual string ValidationGroup {
            get {
                object o = ViewState["ValidationGroup"];
                return ((o == null) ? String.Empty : (string)o);
            }
            set {
                if (!String.Equals(value, ViewState["ValidationGroup"])) {
                    ViewState["ValidationGroup"] = value;
                    OnFieldChanged();
                }
            }
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        protected override DataControlField CreateField() {
            return new DynamicField();
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        public override void InitializeCell(DataControlFieldCell cell, DataControlCellType cellType,
            DataControlRowState rowState, int rowIndex) {

            base.InitializeCell(cell, cellType, rowState, rowIndex);

            if (cellType == DataControlCellType.DataCell) {
                DynamicControl control = CreateDynamicControl();
                control.DataField = DataField;
                control.Mode = DetermineControlMode(rowState);
                
                // Copy various properties into the control
                if (_customApplyFormatInEditModeSet) {
                    control.ApplyFormatInEditMode = ApplyFormatInEditMode;
                }
                if (_customConvertEmptyStringToNullSet) {
                    control.ConvertEmptyStringToNull = ConvertEmptyStringToNull;
                }
                control.DataFormatString = DataFormatString;
                if (ViewState["HtmlEncode"] == null) {
                    // There is no Column in Design Mode
                    if (!DesignMode) {
                        control.HtmlEncode = Column.HtmlEncode;
                    }
                }
                else {
                    control.HtmlEncode = HtmlEncode;
                }
                control.NullDisplayText = NullDisplayText;
                control.UIHint = UIHint;
                control.ValidationGroup = ValidationGroup;

                // Pass it all the extra declarative attributes that we got
                control.SetAttributes(_attributes);

                ConfigureDynamicControl(control);

                cell.Controls.Add(control);
            }
        }

        /// <summary>
        /// Provides a way for classes deriving from DynamicField to override how DynamicControl gets created.
        /// </summary>
        /// <returns></returns>
        protected virtual DynamicControl CreateDynamicControl() {
            return new DynamicControl();
        }

        /// <summary>
        /// Provides a hook to further modify a DynamicControl that was created by the InitializeCell method
        /// </summary>
        /// <param name="control"></param>
        protected virtual void ConfigureDynamicControl(DynamicControl control) {
            Debug.Assert(control != null);
        }

        private DataBoundControlMode DetermineControlMode(DataControlRowState rowState) {
            if (ReadOnly) {
                return DataBoundControlMode.ReadOnly;
            }

            bool edit = (rowState & DataControlRowState.Edit) != 0;
            bool insert = (rowState & DataControlRowState.Insert) != 0;

            if (edit) {
                return DataBoundControlMode.Edit;
            } else if (insert) {
                return DataBoundControlMode.Insert;
            } else {
                return DataBoundControlMode.ReadOnly;
            }
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        public override void ExtractValuesFromCell(IOrderedDictionary dictionary, DataControlFieldCell cell,
            DataControlRowState rowState, bool includeReadOnly) {
            Misc.ExtractValuesFromBindableControls(dictionary, cell);
        }

        /// <summary>
        /// See base class documentation
        /// </summary>
        protected override void CopyProperties(DataControlField newField) {
            base.CopyProperties(newField);
            DynamicField field = ((DynamicField)newField);
            field.DataField = DataField;
            field.ApplyFormatInEditMode = ApplyFormatInEditMode;
            field.ConvertEmptyStringToNull = ConvertEmptyStringToNull;
            field.HtmlEncode = HtmlEncode;
            field.ReadOnly = ReadOnly;
            field.NullDisplayText = NullDisplayText;
            field.UIHint = UIHint;
            field.ValidationGroup = ValidationGroup;
            field.DataFormatString = DataFormatString;
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
