namespace System.Web.DynamicData {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web.UI.WebControls;
    using System.Web.UI;
    using System.Collections;
    using System.Collections.Specialized;

    // Dynamically creates a field template for text or boolean fields
    internal class SimpleFieldTemplateUserControl : FieldTemplateUserControl {
        private const string TextBoxID = "TextBox";

        // Used for edit scenariors, since we only have one value for these
        // simple fieldtemplates
        private Func<object> _valueExtrator;
        private List<BaseValidator> _validators;

        protected SimpleFieldTemplateUserControl() {
        }

        protected override void OnInit(EventArgs e) {
            base.OnInit(e);
            // Add the validators to the field template
            InitializeValidators();
        }

        protected override void ExtractValues(IOrderedDictionary dictionary) {
            if (_valueExtrator != null) {
                object value = _valueExtrator();
                string stringValue = value as string;
                dictionary[Column.Name] = (stringValue != null ? ConvertEditedValue(stringValue) : value);
            }
        }

        public static SimpleFieldTemplateUserControl CreateBooleanTemplate(bool readOnly) {
            SimpleFieldTemplateUserControl control = new SimpleFieldTemplateUserControl();

            var checkBox = new CheckBox();
            checkBox.Enabled = !readOnly;
            checkBox.DataBinding += (sender, e) => {
                if (control.FieldValue != null) {
                    checkBox.Checked = (bool)control.FieldValue;
                }
            };

            if (!readOnly) {
                control._valueExtrator = () => checkBox.Checked;
            }

            control.Controls.Add(checkBox);
            return control;
        }

        public static SimpleFieldTemplateUserControl CreateTextTemplate(MetaColumn column, bool readOnly) {
            SimpleFieldTemplateUserControl control = new SimpleFieldTemplateUserControl();
            if (readOnly) {
                var literal = new Literal();
                literal.DataBinding += (sender, e) => {
                    literal.Text = control.FieldValueString;
                };
                control.Controls.Add(literal);
            }
            else {
                var textBox = new TextBox();                
                textBox.DataBinding += (sender, e) => {                    
                    textBox.Text = control.FieldValueEditString;                    
                };
                // Logic copied from BoundField
                if (column.ColumnType.IsPrimitive) {
                    textBox.Columns = 5;
                }
                control._valueExtrator = () => textBox.Text;
                textBox.CssClass = "DDTextBox";
                textBox.ID = TextBoxID;
                control.Controls.Add(textBox);
                control.CreateValidators(column);
            }

            return control;
        }

        internal void InitializeValidators() {
            if (_validators != null) {
                _validators.ForEach(v => Controls.Add(v));
                _validators.ForEach(v => SetUpValidator(v));
            }
        }

        // This method create's validators for a particular column type. This should be as close to the the actual FieldTemplates (user controls) as possible.
        // DateTime -> Required, Regex
        // Integer -> Regex, Required, Range, Compare
        // Decimal -> Regex, Required, Range, Compare
        // Text -> Regex, Required
        // Enum -> Required
        private void CreateValidators(MetaColumn column) {
            if (_validators == null) {
                _validators = new List<BaseValidator>();
            }

            // Exclude regular expression validator for enum columns
            if (column.GetEnumType() == null) {
                RegularExpressionValidator regularExpressionValidator = new RegularExpressionValidator {
                    ControlToValidate = TextBoxID,
                    Enabled = false,
                    Display = ValidatorDisplay.Static,
                    CssClass = "DDControl DDValidator"
                };
                _validators.Add(regularExpressionValidator);
            }

            if (column.IsInteger || column.ColumnType == typeof(decimal) || column.ColumnType == typeof(double) || column.ColumnType == typeof(float)) {
                RangeValidator rangeValidator = new RangeValidator {
                    ControlToValidate = TextBoxID,
                    Enabled = false,
                    Display = ValidatorDisplay.Static,
                    MinimumValue = "0",
                    MaximumValue = "100",
                    CssClass = "DDControl DDValidator",
                    Type = column.IsInteger ? ValidationDataType.Integer : ValidationDataType.Double                    
                };
                _validators.Add(rangeValidator);

                CompareValidator compareValidator = new CompareValidator {
                    ControlToValidate = TextBoxID,
                    Enabled = false,
                    Display = ValidatorDisplay.Static,
                    Operator = ValidationCompareOperator.DataTypeCheck,
                    CssClass = "DDControl DDValidator",
                    Type = column.IsInteger ? ValidationDataType.Integer : ValidationDataType.Double
                };
                _validators.Add(compareValidator);
            }

            RequiredFieldValidator requiredFieldValidator = new RequiredFieldValidator {
                ControlToValidate = TextBoxID,
                Enabled = false,
                CssClass = "DDControl DDValidator",
                Display = ValidatorDisplay.Static
            };
            _validators.Add(requiredFieldValidator);


            DynamicValidator dynamicValidator = new DynamicValidator {
                ControlToValidate = TextBoxID,
                CssClass = "DDControl DDValidator",
                Display = ValidatorDisplay.Static
            };
            _validators.Add(dynamicValidator);
        }
    }
}
