using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Web.DynamicData.Util;
using System.Web.Resources;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace System.Web.DynamicData {

    /// <summary>
    /// The base class for all field template user controls
    /// </summary>
    public class FieldTemplateUserControl : UserControl, IBindableControl, IFieldTemplate {

        private static RequiredAttribute s_defaultRequiredAttribute = new RequiredAttribute();
        private Dictionary<Type, bool> _ignoredModelValidationAttributes;
        private object _fieldValue;
        private DefaultValueMapping _defaultValueMapping;
        private bool _pageDataItemSet;
        private object _pageDataItem;

        public FieldTemplateUserControl() {
        }

        internal FieldTemplateUserControl(DefaultValueMapping defaultValueMapping) {
            _defaultValueMapping = defaultValueMapping;
        }

        /// <summary>
        /// The host that provides context to this field template
        /// </summary>
        [Browsable(false)]
        public IFieldTemplateHost Host { get; private set; }

        /// <summary>
        /// The formatting options that need to be applied to this field template
        /// </summary>
        [Browsable(false)]
        public IFieldFormattingOptions FormattingOptions { get; private set; }

        /// <summary>
        /// The MetaColumn that this field template is working with
        /// </summary>
        [Browsable(false)]
        public MetaColumn Column {
            get {
                return Host.Column;
            }
        }

        /// <summary>
        /// The ContainerType in which this 
        /// </summary>
        [Browsable(false)]
        public virtual ContainerType ContainerType {
            get {
                return Misc.FindContainerType(this);
            }
        }

        /// <summary>
        /// The MetaTable that this field's column belongs to
        /// </summary>
        [Browsable(false)]
        public MetaTable Table {
            get {
                return Column.Table;
            }
        }

        /// <summary>
        /// Casts the MetaColumn to a MetaForeignKeyColumn. Throws if it is not an FK column.
        /// </summary>
        [Browsable(false)]
        public MetaForeignKeyColumn ForeignKeyColumn {
            get {
                var foreignKeyColumn = Column as MetaForeignKeyColumn;
                if (foreignKeyColumn == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FieldTemplateUserControl_ColumnIsNotFK, Column.Name));
                }
                return foreignKeyColumn;
            }
        }

        /// <summary>
        /// Casts the MetaColumn to a MetaChildrenColumn. Throws if it is not an Children column.
        /// </summary>
        [Browsable(false)]
        public MetaChildrenColumn ChildrenColumn {
            get {
                var childrenColumn = Column as MetaChildrenColumn;
                if (childrenColumn == null) {
                    throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                        DynamicDataResources.FieldTemplateUserControl_ColumnIsNotChildren, Column.Name));
                }
                return childrenColumn;
            }
        }

        /// <summary>
        /// The mode (readonly, edit, insert) that the field template should use
        /// </summary>
        [Browsable(false)]
        public DataBoundControlMode Mode {
            get {
                return Host.Mode;
            }
        }

        /// <summary>
        /// The collection of metadata attributes that apply to this column
        /// </summary>
        [Browsable(false)]
        public System.ComponentModel.AttributeCollection MetadataAttributes {
            get {
                return Column.Attributes;
            }
        }

        /// <summary>
        /// Returns the data control that handles the field inside the field template
        /// </summary>
        [Browsable(false)]
        public virtual Control DataControl {
            get {
                return null;
            }
        }

        /// <summary>
        /// The current data object. Equivalent to Page.GetDataItem()
        /// </summary>
        [Browsable(false)]
        public virtual object Row {
            get {
                // The DataItem is normally null in insert mode, we're going to surface the DictionaryCustomTypeDescriptor if there is a
                //  a default value was specified for this column.
                if (Mode == DataBoundControlMode.Insert && DefaultValueMapping != null && DefaultValueMapping.Contains(Column)) {
                    return DefaultValueMapping.Instance;
                }

                // Used for unit testing. We can't use null since thats a valid value.
                if (_pageDataItemSet) {
                    return _pageDataItem;
                }
                return Page.GetDataItem();
            }
            internal set {
                // Only set in unit tests.
                _pageDataItem = value;
                _pageDataItemSet = true;
            }
        }

        /// <summary>
        /// The value of the Column in the current Row
        /// </summary>
        [Browsable(false)]
        public virtual object FieldValue {
            get {
                // If a field value was explicitly set, use it instead of the usual logic.
                if (_fieldValue != null)
                    return _fieldValue;

                return GetColumnValue(Column);
            }

            set {
                _fieldValue = value;
            }
        }

        /// <summary>
        /// Get the value of a specific column in the current row
        /// </summary>
        /// <param name="column"></param>
        /// <returns></returns>
        protected virtual object GetColumnValue(MetaColumn column) {           
            object row = Row;
            if (row != null) {
                return DataBinder.GetPropertyValue(row, column.Name);
            }

            // Fallback on old behavior
            if (Mode == DataBoundControlMode.Insert) {
                return column.DefaultValue;
            }

            return null;
        }

        /// <summary>
        /// Return the field value as a formatted string
        /// </summary>
        [Browsable(false)]
        public virtual string FieldValueString {
            get {
                // Get the string and preprocess it
                return FormatFieldValue(FieldValue);
            }
        }

        /// <summary>
        /// Similar to FieldValueString, but the string is to be used when the field is in edit mode
        /// </summary>
        [Browsable(false)]
        public virtual string FieldValueEditString {
            get {
                return FormattingOptions.FormatEditValue(FieldValue);
            }
        }

        /// <summary>
        /// Only applies to FK columns. Returns a URL that links to the page that displays the details
        /// of the foreign key entity. e.g. In the Product table's Category column, this produces a link
        /// that goes to the details of the category that the product is in
        /// </summary>
        protected string ForeignKeyPath {
            get {
                return ForeignKeyColumn.GetForeignKeyPath(PageAction.Details, Row);
            }
        }

        internal DefaultValueMapping DefaultValueMapping {
            get {
                if (_defaultValueMapping == null) {
                    // Ensure this only gets accessed in insert mode
                    Debug.Assert(Mode == DataBoundControlMode.Insert);
                    _defaultValueMapping = MetaTableHelper.GetDefaultValueMapping(this, Context.ToWrapper());
                }
                return _defaultValueMapping;
            }
        }

        /// <summary>
        /// Same as ForeignKeyPath, except that it allows the path part of the URL to be overriden. This is
        /// used when using pages that don't live under DynamicData/CustomPages.
        /// </summary>
        /// <param name="path">The path override</param>
        /// <returns></returns>
        protected string BuildForeignKeyPath(string path) {
            // If a path was passed in, resolved it relative to the containing page
            if (!String.IsNullOrEmpty(path)) {
                path = ResolveParentRelativePath(path);
            }

            return ForeignKeyColumn.GetForeignKeyPath(PageAction.Details, Row, path);
        }

        /// <summary>
        /// Only applies to Children columns. Returns a URL that links to the page that displays the list
        /// of children entities. e.g. In the Category table's Products column, this produces a link
        /// that goes to the list of Products that are in this Category.
        /// </summary>
        protected string ChildrenPath {
            get {
                return ChildrenColumn.GetChildrenPath(PageAction.List, Row);
            }
        }

        /// <summary>
        /// Same as ChildrenPath, except that it allows the path part of the URL to be overriden. This is
        /// used when using pages that don't live under DynamicData/CustomPages.
        /// </summary>
        /// <param name="path">The path override</param>
        /// <returns></returns>
        protected string BuildChildrenPath(string path) {
            // If a path was passed in, resolved it relative to the containing page
            if (!String.IsNullOrEmpty(path)) {
                path = ResolveParentRelativePath(path);
            }

            return ChildrenColumn.GetChildrenPath(PageAction.List, Row, path);
        }

        // Resolve a relative path based on the containing page
        private string ResolveParentRelativePath(string path) {
            if (path == null || TemplateControl == null)
                return path;

            Control parentControl = TemplateControl.Parent;
            if (parentControl == null)
                return path;

            return parentControl.ResolveUrl(path);
        }

        /// <summary>
        /// Return the field template for another column
        /// </summary>
        protected FieldTemplateUserControl FindOtherFieldTemplate(string columnName) {
            return Parent.FindFieldTemplate(columnName) as FieldTemplateUserControl;
        }

        /// <summary>
        /// Only applies to FK columns. Populate the list control with all the values from the parent table 
        /// </summary>
        /// <param name="listControl">The control to be populated</param>
        protected void PopulateListControl(ListControl listControl) {
            Type enumType;
            if (Column is MetaForeignKeyColumn) {
                Misc.FillListItemCollection(ForeignKeyColumn.ParentTable, listControl.Items);
            } else if (Column.IsEnumType(out enumType)) {
                Debug.Assert(enumType != null);
                FillEnumListControl(listControl, enumType);
            }
        }

        private void FillEnumListControl(ListControl list, Type enumType) {
            foreach (DictionaryEntry entry in Misc.GetEnumNamesAndValues(enumType)) {
                list.Items.Add(new ListItem((string)entry.Key, (string)entry.Value));
            }
        }

        /// <summary>
        /// Gets a string representation of the column's value so that it can be matched with
        /// values populated in a dropdown. This currently works for FK and Enum columns only.
        /// The method returns null for other column types.
        /// </summary>
        /// <returns></returns>
        protected string GetSelectedValueString() {
            Type enumType;
            if (Column is MetaForeignKeyColumn) {
                return ForeignKeyColumn.GetForeignKeyString(Row);
            } else if(Column.IsEnumType(out enumType)) {
                return Misc.GetUnderlyingTypeValueString(enumType, FieldValue);
            }
            return null;
        }

        /// <summary>
        /// Only applies to FK columns. This is used when saving the value of a foreign key, typically selected
        /// from a drop down.
        /// </summary>
        /// <param name="dictionary">The dictionary that contains all the new values</param>
        /// <param name="selectedValue">The value to be saved. Typically, this comes from DropDownList.SelectedValue</param>
        protected virtual void ExtractForeignKey(IDictionary dictionary, string selectedValue) {
            ForeignKeyColumn.ExtractForeignKey(dictionary, selectedValue);
        }

        /// <summary>
        /// Apply potential HTML encoding and formatting to a string that needs to be displayed
        /// </summary>
        /// <param name="fieldValue">The value that should be formatted</param>
        /// <returns>the formatted value</returns>
        public virtual string FormatFieldValue(object fieldValue) {
            return FormattingOptions.FormatValue(fieldValue);
        }

        /// <summary>
        /// Return either the input value or null based on ConvertEmptyStringToNull and NullDisplayText
        /// </summary>
        /// <param name="value">The input value</param>
        /// <returns>The converted value</returns>
        protected virtual object ConvertEditedValue(string value) {
            return FormattingOptions.ConvertEditedValue(value);
        }

        /// <summary>
        /// Set up a validator for dynamic data use. It sets the ValidationGroup on all validators,
        /// and also performs additional logic for some specific validator types. e.g. for a RangeValidator
        /// it sets the range values if they exist on the model.
        /// </summary>
        /// <param name="validator">The validator to be set up</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "We really want Set Up as two words")]
        protected virtual void SetUpValidator(BaseValidator validator) {
            SetUpValidator(validator, Column);
        }

        /// <summary>
        /// Set up a validator for dynamic data use. It sets the ValidationGroup on all validators,
        /// and also performs additional logic for some specific validator types. e.g. for a RangeValidator
        /// it sets the range values if they exist on the model.
        /// </summary>
        /// <param name="validator">The validator to be set up</param>
        /// <param name="column">The column for which the validator is getting set</param>
        [SuppressMessage("Microsoft.Naming", "CA1702:CompoundWordsShouldBeCasedCorrectly",
            Justification = "We really want Set Up as two words")]
        protected virtual void SetUpValidator(BaseValidator validator, MetaColumn column) {

            // Set the validation group to match the dynamic control
            validator.ValidationGroup = Host.ValidationGroup;

            if (validator is DynamicValidator) {
                SetUpDynamicValidator((DynamicValidator)validator, column);
            }
            else if (validator is RequiredFieldValidator) {
                SetUpRequiredFieldValidator((RequiredFieldValidator)validator, column);
            }
            else if (validator is CompareValidator) {
                SetUpCompareValidator((CompareValidator)validator, column);
            }
            else if (validator is RangeValidator) {
                SetUpRangeValidator((RangeValidator)validator, column);
            }
            else if (validator is RegularExpressionValidator) {
                SetUpRegexValidator((RegularExpressionValidator)validator, column);
            }

            validator.ToolTip = validator.ErrorMessage;
            validator.Text = "*";
        }

        private void SetUpDynamicValidator(DynamicValidator validator, MetaColumn column) {
            validator.Column = column;

            // Tell the DynamicValidator which validation attributes it should ignore (because
            // they're already handled by server side ASP.NET validator controls)
            validator.SetIgnoredModelValidationAttributes(_ignoredModelValidationAttributes);
        }

        private void SetUpRequiredFieldValidator(RequiredFieldValidator validator, MetaColumn column) {
            var requiredAttribute = column.Metadata.RequiredAttribute;
            if (requiredAttribute!= null && requiredAttribute.AllowEmptyStrings) {
                // Dev10 Bug 749744
                // If somone explicitly set AllowEmptyStrings = true then we assume that they want to
                // allow empty strings to go into a database even if the column is marked as required.
                // Since ASP.NET validators always get an empty string, this essential turns of
                // required field validation.
                IgnoreModelValidationAttribute(typeof(RequiredAttribute));
            } else if (column.IsRequired) {
                validator.Enabled = true;

                // Make sure the attribute doesn't get validated a second time by the DynamicValidator
                IgnoreModelValidationAttribute(typeof(RequiredAttribute));

                if (String.IsNullOrEmpty(validator.ErrorMessage)) {
                    string columnErrorMessage = column.RequiredErrorMessage;
                    if (String.IsNullOrEmpty(columnErrorMessage)) {
                        // generate default error message
                        validator.ErrorMessage = HttpUtility.HtmlEncode(s_defaultRequiredAttribute.FormatErrorMessage(column.DisplayName));
                    } else {
                        validator.ErrorMessage = HttpUtility.HtmlEncode(columnErrorMessage);
                    }
                }
            }
        }

        private void SetUpCompareValidator(CompareValidator validator, MetaColumn column) {
            validator.Operator = ValidationCompareOperator.DataTypeCheck;

            ValidationDataType? dataType = null;
            string errorMessage = null;
            if (column.ColumnType == typeof(DateTime)) {
                dataType = ValidationDataType.Date;
                errorMessage = String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.FieldTemplateUserControl_CompareValidationError_Date,
                    column.DisplayName);
            } else if (column.IsInteger && column.ColumnType != typeof(long)) {
                // long is unsupported because it's larger than int
                dataType = ValidationDataType.Integer;
                errorMessage = String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.FieldTemplateUserControl_CompareValidationError_Integer,
                    column.DisplayName);
            } else if (column.ColumnType == typeof(decimal)) {
                // 

                dataType = ValidationDataType.Double;
                errorMessage = String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.FieldTemplateUserControl_CompareValidationError_Decimal,
                    column.DisplayName);
            } else if (column.IsFloatingPoint) {
                dataType = ValidationDataType.Double;
                errorMessage = String.Format(CultureInfo.CurrentCulture,
                    DynamicDataResources.FieldTemplateUserControl_CompareValidationError_Decimal,
                    column.DisplayName);
            }

            if (dataType != null) {
                Debug.Assert(errorMessage != null);
                validator.Enabled = true;
                validator.Type = dataType.Value;
                if (String.IsNullOrEmpty(validator.ErrorMessage)) {
                    validator.ErrorMessage = HttpUtility.HtmlEncode(errorMessage);
                }
            } else {
                // If we don't recognize the type, turn off the validator
                validator.Enabled = false;
            }
        }

        private void SetUpRangeValidator(RangeValidator validator, MetaColumn column) {
            // Nothing to do if no range was specified
            var rangeAttribute = column.Attributes.OfType<RangeAttribute>().FirstOrDefault();
            if (rangeAttribute == null)
                return;

            // Make sure the attribute doesn't get validated a second time by the DynamicValidator
            IgnoreModelValidationAttribute(rangeAttribute.GetType());

            validator.Enabled = true;

            Func<object, string> converter;
            switch (validator.Type) {
                case ValidationDataType.Integer:
                    converter = val => Convert.ToInt32(val, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                    break;
                case ValidationDataType.Double:
                    converter = val => Convert.ToDouble(val, CultureInfo.InvariantCulture).ToString(CultureInfo.InvariantCulture);
                    break;
                case ValidationDataType.String:
                default:
                    converter = val => val.ToString();
                    break;
            }
            validator.MinimumValue = converter(rangeAttribute.Minimum);
            validator.MaximumValue = converter(rangeAttribute.Maximum);

            if (String.IsNullOrEmpty(validator.ErrorMessage)) {
                validator.ErrorMessage = HttpUtility.HtmlEncode(rangeAttribute.FormatErrorMessage(column.DisplayName));
            }
        }

        private void SetUpRegexValidator(RegularExpressionValidator validator, MetaColumn column) {
            // Nothing to do if no regex was specified
            var regexAttribute = column.Attributes.OfType<RegularExpressionAttribute>().FirstOrDefault();
            if (regexAttribute == null)
                return;

            // Make sure the attribute doesn't get validated a second time by the DynamicValidator
            IgnoreModelValidationAttribute(regexAttribute.GetType());

            validator.Enabled = true;
            validator.ValidationExpression = regexAttribute.Pattern;

            if (String.IsNullOrEmpty(validator.ErrorMessage)) {
                validator.ErrorMessage = HttpUtility.HtmlEncode(regexAttribute.FormatErrorMessage(column.DisplayName));
            }
        }

        /// <summary>
        /// This method instructs the DynamicValidator to ignore a specific type of model
        /// validation attributes. This is called when that attribute type is already being
        /// fully handled by an ASP.NET validator controls. Without this call, the validation
        /// could happen twice, resulting in a duplicated error message
        /// </summary>
        [SuppressMessage("Microsoft.Usage", "CA2301:EmbeddableTypesInContainersRule", MessageId = "_ignoredModelValidationAttributes", Justification = "The types that go into this dictionary are specifically ValidationAttribute derived types.")]
        protected void IgnoreModelValidationAttribute(Type attributeType)
        {
            // Create the dictionary on demand
            if (_ignoredModelValidationAttributes == null) {
                _ignoredModelValidationAttributes = new Dictionary<Type, bool>();
            }

            // Add the attribute type to the list
            _ignoredModelValidationAttributes[attributeType] = true;
        }

        /// <summary>
        /// Implementation of IBindableControl.ExtractValues
        /// </summary>
        /// <param name="dictionary">The dictionary that contains all the new values</param>
        protected virtual void ExtractValues(IOrderedDictionary dictionary) {
            // To nothing in the base class.  Derived field templates decide what they want to save
        }

        #region IBindableControl Members

        void IBindableControl.ExtractValues(IOrderedDictionary dictionary) {
            ExtractValues(dictionary);
        }

        #endregion

        #region IFieldTemplate Members

        void IFieldTemplate.SetHost(IFieldTemplateHost host) {
            Host = host;
            FormattingOptions = Host.FormattingOptions;
        }

        #endregion
    }
}
