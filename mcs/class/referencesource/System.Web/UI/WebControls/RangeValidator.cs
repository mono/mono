//------------------------------------------------------------------------------
// <copyright file="RangeValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Web;
    using System.Globalization;
    using System.Web.Util;


    /// <devdoc>
    ///    <para> Checks if the value of the associated input control
    ///       is within some minimum and maximum values, which
    ///       can be constant values or values of other controls.</para>
    /// </devdoc>
    [
    ToolboxData("<{0}:RangeValidator runat=\"server\" ErrorMessage=\"RangeValidator\"></{0}:RangeValidator>")
    ]
    public class RangeValidator : BaseCompareValidator {


        /// <devdoc>
        ///    <para> Gets or sets the maximum value of the validation range.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        WebSysDescription(SR.RangeValidator_MaximumValue)
        ]
        public string MaximumValue {
            get {
                object o = ViewState["MaximumValue"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["MaximumValue"] = value;
            }
        }


        /// <devdoc>
        ///    <para> Gets or sets the minimum value of the validation range.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        WebSysDescription(SR.RangeValidator_MinmumValue)
        ]
        public string MinimumValue {
            get {
                object o = ViewState["MinimumValue"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                ViewState["MinimumValue"] = value;
            }
        }



        /// <internalonly/>
        /// <devdoc>
        ///    AddAttributesToRender method
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);
            if (RenderUplevel) {
                string id = ClientID;
                HtmlTextWriter expandoAttributeWriter = (EnableLegacyRendering || IsUnobtrusive) ? writer : null;
                AddExpandoAttribute(expandoAttributeWriter, id, "evaluationfunction", "RangeValidatorEvaluateIsValid", false);

                string maxValueString = MaximumValue;
                string minValueString = MinimumValue;
                if (CultureInvariantValues) {
                    maxValueString = ConvertCultureInvariantToCurrentCultureFormat(maxValueString, Type);
                    minValueString = ConvertCultureInvariantToCurrentCultureFormat(minValueString, Type);
                }
                AddExpandoAttribute(expandoAttributeWriter, id, "maximumvalue", maxValueString);
                AddExpandoAttribute(expandoAttributeWriter, id, "minimumvalue", minValueString);
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    This is a check of properties to determine any errors made by the developer
        /// </devdoc>
        protected override bool ControlPropertiesValid() {
            ValidateValues();
            return base.ControlPropertiesValid();
        }


        /// <internalonly/>
        /// <devdoc>
        ///    EvaluateIsValid method
        /// </devdoc>
        protected override bool EvaluateIsValid() {

            Debug.Assert(PropertiesValid, "Should have already been checked");

            // Get the peices of text from the control(s).
            string text = GetControlValidationValue(ControlToValidate);
            Debug.Assert(text != null, "Should have already caught this!");

            // Special case: if the string is blank, we don't try to validate it. The input should be
            // trimmed for coordination with the RequiredFieldValidator.
            if (text.Trim().Length == 0) {
                return true;
            }

            // VSWhidbey 83168
            if (Type == ValidationDataType.Date &&
                !DetermineRenderUplevel() &&
                !IsInStandardDateFormat(text)) {
                text = ConvertToShortDateString(text);
            }

            return(Compare(text, false, MinimumValue, CultureInvariantValues, ValidationCompareOperator.GreaterThanEqual, Type)
                   && Compare(text, false, MaximumValue, CultureInvariantValues, ValidationCompareOperator.LessThanEqual, Type));
        }

        /// <devdoc>
        ///
        /// </devdoc>
        private void ValidateValues() {
            // Check the control values can be converted to data type
            string maximumValue = MaximumValue;
            if (!CanConvert(maximumValue, Type, CultureInvariantValues)) {
                throw new HttpException(
                                       SR.GetString(SR.Validator_value_bad_type,
                                                                       new string [] {
                                                                           maximumValue,
                                                                           "MaximumValue",
                                                                           ID,
                                                                           PropertyConverter.EnumToString(typeof(ValidationDataType), Type)
                                                                       }));
            }
            string minumumValue = MinimumValue;
            if (!CanConvert(minumumValue, Type, CultureInvariantValues)) {
                throw new HttpException(
                                       SR.GetString(SR.Validator_value_bad_type,
                                                                       new string [] {
                                                                           minumumValue,
                                                                           "MinimumValue",
                                                                           ID,
                                                                           PropertyConverter.EnumToString(typeof(ValidationDataType), Type)
                                                                       }));
            }
            // Check for overlap.
            if (Compare(minumumValue, CultureInvariantValues,
                        maximumValue, CultureInvariantValues,
                        ValidationCompareOperator.GreaterThan, Type))  {
                throw new HttpException(
                                       SR.GetString(SR.Validator_range_overalap,
                                                                       new string [] {
                                                                           maximumValue,
                                                                           minumumValue,
                                                                           ID,
                                                                       }));
            }
        }
    }
}

