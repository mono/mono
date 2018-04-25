//------------------------------------------------------------------------------
// <copyright file="CompareValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Web;
    using System.Globalization;
    using System.Web.Util;


    /// <devdoc>
    ///    <para> Compares the value of an input control to another input control or
    ///       a constant value using a variety of operators and types.</para>
    /// </devdoc>
    [
    ToolboxData("<{0}:CompareValidator runat=\"server\" ErrorMessage=\"CompareValidator\"></{0}:CompareValidator>")
    ]
    public class CompareValidator : BaseCompareValidator {


        /// <devdoc>
        ///    <para>Gets or sets the ID of the input control to compare with.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        WebSysDescription(SR.CompareValidator_ControlToCompare),
        TypeConverter(typeof(ValidatedControlConverter))
        ]                                         
        public string ControlToCompare {
            get { 
                object o = ViewState["ControlToCompare"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["ControlToCompare"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the comparison operation to perform.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(ValidationCompareOperator.Equal),
        WebSysDescription(SR.CompareValidator_Operator)
        ]                                         
        public ValidationCompareOperator Operator {
            get { 
                object o = ViewState["Operator"];
                return((o == null) ? ValidationCompareOperator.Equal : (ValidationCompareOperator)o);
            }
            set {
                if (value < ValidationCompareOperator.Equal || value > ValidationCompareOperator.DataTypeCheck) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Operator"] = value;
            }
        }


        /// <devdoc>
        ///    <para>Gets or sets the specific value to compare with.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        WebSysDescription(SR.CompareValidator_ValueToCompare)
        ]                                         
        public string ValueToCompare {
            get { 
                object o = ViewState["ValueToCompare"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["ValueToCompare"] = value;
            }        
        }

        // <summary>
        //  AddAttributesToRender method
        // </summary>

        /// <internalonly/>
        /// <devdoc>
        ///    <para>Adds the attributes of this control to the output stream for rendering on the 
        ///       client.</para>
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);
            if (RenderUplevel) {
                string id = ClientID;
                HtmlTextWriter expandoAttributeWriter = (EnableLegacyRendering || IsUnobtrusive) ? writer : null;
                AddExpandoAttribute(expandoAttributeWriter, id, "evaluationfunction", "CompareValidatorEvaluateIsValid", false);
                if (ControlToCompare.Length > 0) {
                    string controlToCompareID = GetControlRenderID(ControlToCompare);
                    AddExpandoAttribute(expandoAttributeWriter, id, "controltocompare", controlToCompareID);
                    AddExpandoAttribute(expandoAttributeWriter, id, "controlhookup", controlToCompareID);
                }
                if (ValueToCompare.Length > 0) {

                    string valueToCompareString = ValueToCompare;
                    if (CultureInvariantValues) {
                        valueToCompareString = ConvertCultureInvariantToCurrentCultureFormat(valueToCompareString, Type);
                    }
                    AddExpandoAttribute(expandoAttributeWriter, id, "valuetocompare", valueToCompareString);
                }
                if (Operator != ValidationCompareOperator.Equal) {
                    AddExpandoAttribute(expandoAttributeWriter, id, "operator", PropertyConverter.EnumToString(typeof(ValidationCompareOperator), Operator), false);
                }
            }
        }        


        /// <internalonly/>
        /// <devdoc>
        ///    <para> Checks the properties of a the control for valid values.</para>
        /// </devdoc>
        protected override bool ControlPropertiesValid() {

            // Check the control id references 
            if (ControlToCompare.Length > 0) {
                CheckControlValidationProperty(ControlToCompare, "ControlToCompare");

                if (StringUtil.EqualsIgnoreCase(ControlToValidate, ControlToCompare)) {
                    throw new HttpException(SR.GetString(SR.Validator_bad_compare_control, 
                                                                             ID, 
                                                                             ControlToCompare));
                }
            }   
            else {
                // Check Values
                if (Operator != ValidationCompareOperator.DataTypeCheck &&
                    !CanConvert(ValueToCompare, Type, CultureInvariantValues)) {
                        throw new HttpException(
                                           SR.GetString(SR.Validator_value_bad_type, 
                                                                           new string [] {
                                                                               ValueToCompare,
                                                                               "ValueToCompare",
                                                                               ID, 
                                                                               PropertyConverter.EnumToString(typeof(ValidationDataType), Type),
                                                                           }));
                }
            }
            return base.ControlPropertiesValid();
        }


        /// <internalonly/>
        /// <devdoc>
        ///    EvaluateIsValid method
        /// </devdoc>
        protected override bool EvaluateIsValid() {

            Debug.Assert(PropertiesValid, "Properties should have already been checked");

            // Get the peices of text from the control.
            string leftText = GetControlValidationValue(ControlToValidate);
            Debug.Assert(leftText != null, "Should have already caught this!");

            // Special case: if the string is blank, we don't try to validate it. The input should be
            // trimmed for coordination with the RequiredFieldValidator.
            if (leftText.Trim().Length == 0) {
                return true;
            }

            // VSWhidbey 83168
            bool convertDate = (Type == ValidationDataType.Date && !DetermineRenderUplevel());
            if (convertDate && !IsInStandardDateFormat(leftText)) {
                leftText = ConvertToShortDateString(leftText);
            }

            // The control has precedence over the fixed value
            bool isCultureInvariantValue = false;
            string rightText = string.Empty;
            if (ControlToCompare.Length > 0) {
                rightText = GetControlValidationValue(ControlToCompare);
                Debug.Assert(rightText != null, "Should have already caught this!");

                // VSWhidbey 83089
                if (convertDate && !IsInStandardDateFormat(rightText)) {
                    rightText = ConvertToShortDateString(rightText);
                }
            }
            else {
                rightText = ValueToCompare;
                isCultureInvariantValue = CultureInvariantValues;
            }

            return Compare(leftText, false, rightText, isCultureInvariantValue, Operator, Type);

        }
    }
}
