namespace System.Web.DynamicData {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.Linq;
    using System.Web;
    using System.Web.Resources;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    /// <summary>
    /// Validator that enforces model validation. It can be used either at the field level or the entity level
    /// </summary>
    [ToolboxBitmap(typeof(DynamicValidator), "DynamicValidator.bmp")]
    public class DynamicValidator : BaseValidator {

        private IDynamicDataSource _dataSource;
        private Exception _exception;
        private Dictionary<Type, bool> _ignoredModelValidationAttributes;

        /// <summary>
        /// The name of the column to be validated, or null for entity level validation
        /// </summary>
        [Browsable(false)]
        [Themeable(false)]
        public string ColumnName {
            get {
                return (Column == null) ? String.Empty : Column.Name;
            }
        }

        private IDynamicDataSource DynamicDataSource {
            get {
                if (_dataSource == null) {
                    // get data source for the parent container.
                    _dataSource = this.FindDataSourceControl();
                    // get the data source for the targeted data bound control.
                    if (_dataSource == null) {
                        Control c = NamingContainer.FindControl(ControlToValidate);
                        if (c == null) {
                            throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture,
                                DynamicDataResources.DynamicValidator_ControlNotFound, ControlToValidate, ID));
                        }

                        _dataSource = c.FindDataSourceControl();
                    }
                }
                return _dataSource;
            }
        }

        /// <summary>
        /// The column to be validated, or null for entity level validation
        /// </summary>
        [Browsable(false)]
        [Themeable(false)]
        public MetaColumn Column { get; set; }

        /// <summary>
        /// The validation exception that occurred, if any
        /// </summary>
        protected virtual Exception ValidationException {
            get {
                return _exception;
            }
            set {
                _exception = value;
            }
        }

        /// <summary>
        /// Overridden from base
        /// </summary>
        protected override bool ControlPropertiesValid() {
            bool hasDataSource = DynamicDataSource != null;

            // We can't call the base when there is no Column, because the control will be something like
            // a GridView, which doesn't have a validation property and would cause the base to fail.
            if (String.IsNullOrEmpty(ColumnName)) {
                return hasDataSource;
            }

            return base.ControlPropertiesValid();
        }

        /// <summary>
        /// Overridden from base
        /// </summary>
        protected override bool EvaluateIsValid() {
            // Check if there were some model exceptions (e.g. OnProductNameChanging throwing)
            Exception e = ValidationException;
            if (e != null) {
                ErrorMessage = HttpUtility.HtmlEncode(e.Message);
                return false;
            }

            if (Column == null)
                return true;


            string controlValue = GetControlValidationValue(ControlToValidate);
            if (controlValue == null) {
                // can return null if ControlToValidate is empty, or if the control is not written correctly to return a value
                // this does not mean that the value was null
                return true;
            }

            if (Column is MetaForeignKeyColumn || Column is MetaChildrenColumn) {
                // do not perform conversion or validation on relationship columns as controlValue is the serialized form
                // of a foreign key which would be useless to a validation attribute
                return true;
            }

            // Check if any of our validators want to fail the value
            controlValue = (string)Column.ConvertEditedValue(controlValue);

            object value;
            if (!TryConvertControlValue(controlValue, Column.ColumnType, out value)) {
                ErrorMessage = HttpUtility.HtmlEncode(DynamicDataResources.DynamicValidator_CannotConvertValue);
                return false;
            }

            return ValueIsValid(value);
        }

        internal static bool TryConvertControlValue(string controlValue, Type columnType, out object value) {
            try {
                if (controlValue == null) {
                    value = null;
                } else if (columnType == typeof(string)) {
                    value = controlValue;
                } else if (controlValue.Length != 0) {
                    value = Misc.ChangeType(controlValue, columnType);
                } else {
                    value = null;
                }
                return true;
            } catch (Exception) {
                value = null;
                return false;
            }
        }

        private bool ValueIsValid(object value) {
            // Go through all the model validation attribute to make sure they're valid
            foreach (var attrib in Column.Attributes.Cast<Attribute>().OfType<ValidationAttribute>()) {

                // Ignore it if it's found in the ignore list
                if (_ignoredModelValidationAttributes != null &&
                    _ignoredModelValidationAttributes.ContainsKey(attrib.GetType())) {
                    continue;
                }

                //DynamicValidator can not pass in a ValidationContext as it does
                //not have an easy way to get the data object row. Hence we will
                //not support attributes that require Validation Context (Ex : CompareAttribute).
                if (attrib.RequiresValidationContext) {
                    continue;
                }

                if (!attrib.IsValid(value)) {
                    ErrorMessage = HttpUtility.HtmlEncode(attrib.FormatErrorMessage(Column.DisplayName));
                    return false;
                }
            }

            return true;
        }

        internal void SetIgnoredModelValidationAttributes(Dictionary<Type, bool> ignoredModelValidationAttributes) {
            _ignoredModelValidationAttributes = ignoredModelValidationAttributes;
        }

        private void OnException(object sender, DynamicValidatorEventArgs e) {
            ValidateException(e.Exception);
        }

        /// <summary>
        /// Overridden from base
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Security", "CA2109:ReviewVisibleEventHandlers")]
        protected override void OnInit(EventArgs e) {
            base.OnInit(e);

            // Don't do anything in Design mode
            if (DesignMode)
                return;

            IDynamicDataSource dataSource = DynamicDataSource;
            if (dataSource != null) {
                // Register for datasource exception so that we're called if an error occurs
                // during an update/insert/delete
                dataSource.Exception += new EventHandler<DynamicValidatorEventArgs>(OnException);
            }
        }

        /// <summary>
        /// Called when an exception happens. Typically, this sets the ValidationException
        /// </summary>
        /// <param name="exception">The exception</param>
        protected virtual void ValidateException(Exception exception) {
            if (exception == null) {
                return;
            }

            ValidationException = null;
            
            // IDynamicValidatorExceptions are used by LinqDataSource to wrap exceptions caused by problems
            // with setting model properties (columns), such as exceptions thrown from the OnXYZChanging
            // methods
            IDynamicValidatorException e = exception as IDynamicValidatorException;
            if (e != null) {
                HandleDynamicValidatorException(e);
            } else {
                // It's not a column specific exception.  e.g. it could be coming from
                // OnValidate (DLinq), or could be caused by a database error.
                // We only want to use it if it's a ValidationException, otherwise we
                // could end up displaying sensitive database errors to the end user
                if (Column == null && exception is ValidationException) {
                    if (exception.InnerException == null) {
                        ValidationException = exception;
                    } else {
                        ValidationException = exception.InnerException;
                    }
                }
            }
        }

        private void HandleDynamicValidatorException(IDynamicValidatorException e) {
            if (Column == null) {
                // IDynamicValidatorException only applies to column exceptions
                return;
            }

            List<string> columnNames = GetValidationColumnNames(Column);

            foreach (string name in columnNames) {
                // see if the exception wraps any child exceptions relevant to this column
                Exception inner;
                if (e.InnerExceptions.TryGetValue(name, out inner)) {
                    // Stop as soon as we find the first exception.
                    ValidationException = inner;
                    return;
                }
            }
        }

        /// <summary>
        /// Get the names of all the columns that can throw an exception that will affect the setting of the 
        /// value of the given column.
        /// </summary>
        private static List<string> GetValidationColumnNames(MetaColumn column) {
            List<string> columnNames = new List<string>();
            columnNames.Add(column.Name); // add it first so that it gets checked first
            var fkColumn = column as MetaForeignKeyColumn;
            if (fkColumn != null) {
                columnNames.AddRange(fkColumn.ForeignKeyNames);
            }
            return columnNames;
        }
    }
}
