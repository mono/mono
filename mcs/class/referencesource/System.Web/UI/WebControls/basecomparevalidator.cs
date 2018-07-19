//------------------------------------------------------------------------------
// <copyright file="basecomparevalidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.Drawing;
    using System.Globalization;
    using System.Web;
    using System.Web.UI.HtmlControls;
    using System.Text.RegularExpressions;
    using System.Text;
    using System.Web.Util;


    /// <devdoc>
    ///    <para> Serves as the abstract base
    ///       class for validators that do typed comparisons.</para>
    /// </devdoc>
    public abstract class BaseCompareValidator : BaseValidator {


        /// <devdoc>
        ///    <para>Gets or sets the data type that specifies how the values
        ///       being compared should be interpreted.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(ValidationDataType.String),
        WebSysDescription(SR.RangeValidator_Type)
        ]
        public ValidationDataType Type {
            get {
                object o = ViewState["Type"];
                return((o == null) ? ValidationDataType.String : (ValidationDataType)o);
            }
            set {
                if (value < ValidationDataType.String || value > ValidationDataType.Currency) {
                    throw new ArgumentOutOfRangeException("value");
                }
                ViewState["Type"] = value;
            }
        }


        /// <devdoc>
        ///     Whether we should do culture invariant conversion against the
        ///     string value properties on the control
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(false),
        WebSysDescription(SR.BaseCompareValidator_CultureInvariantValues)
        ]
        public bool CultureInvariantValues {
            get {
                object o = ViewState["CultureInvariantValues"];
                return((o == null) ? false : (bool)o);
            }
            set {
                ViewState["CultureInvariantValues"] = value;
            }
        }


        /// <internalonly/>
        /// <devdoc>
        ///    AddAttributesToRender method
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);
            if (RenderUplevel) {
                ValidationDataType type = Type;
                if (type != ValidationDataType.String) {
                    string id = ClientID;
                    HtmlTextWriter expandoAttributeWriter = (EnableLegacyRendering || IsUnobtrusive) ? writer : null;

                    AddExpandoAttribute(expandoAttributeWriter, id, "type", PropertyConverter.EnumToString(typeof(ValidationDataType), type), false);

                    NumberFormatInfo info = NumberFormatInfo.CurrentInfo;
                    if (type == ValidationDataType.Double) {
                        string decimalChar = info.NumberDecimalSeparator;
                        AddExpandoAttribute(expandoAttributeWriter, id, "decimalchar", decimalChar);
                    }
                    else if (type == ValidationDataType.Currency) {
                        string decimalChar = info.CurrencyDecimalSeparator;
                        AddExpandoAttribute(expandoAttributeWriter, id, "decimalchar", decimalChar);

                        string groupChar = info.CurrencyGroupSeparator;
                        // Map non-break space onto regular space for parsing
                        if (groupChar[0] == 160)
                            groupChar = " ";
                        AddExpandoAttribute(expandoAttributeWriter, id, "groupchar", groupChar);

                        int digits = info.CurrencyDecimalDigits;
                        AddExpandoAttribute(expandoAttributeWriter, id, "digits", digits.ToString(NumberFormatInfo.InvariantInfo), false);

                        // VSWhidbey 83165
                        int groupSize = GetCurrencyGroupSize(info);
                        if (groupSize > 0) {
                            AddExpandoAttribute(expandoAttributeWriter, id, "groupsize", groupSize.ToString(NumberFormatInfo.InvariantInfo), false);
                        }
                    }
                    else if (type == ValidationDataType.Date) {
                        AddExpandoAttribute(expandoAttributeWriter, id, "dateorder", GetDateElementOrder(), false);
                        AddExpandoAttribute(expandoAttributeWriter, id, "cutoffyear", CutoffYear.ToString(NumberFormatInfo.InvariantInfo), false);

                        // VSWhidbey 504553: The changes of this bug make client-side script not
                        // using the century attribute anymore, but still generating it for
                        // backward compatibility with Everett pages.
                        int currentYear = DateTime.Today.Year;
                        int century = currentYear - (currentYear % 100);
                        AddExpandoAttribute(expandoAttributeWriter, id, "century", century.ToString(NumberFormatInfo.InvariantInfo), false);
                    }
                }
            }
        }



        /// <devdoc>
        ///    Check if the text can be converted to the type
        /// </devdoc>
        public static bool CanConvert(string text, ValidationDataType type) {
            return CanConvert(text, type, false);
        }


        public static bool CanConvert(string text, ValidationDataType type, bool cultureInvariant) {
            object value = null;
            return Convert(text, type, cultureInvariant, out value);
        }


        /// <internalonly/>
        /// <devdoc>
        ///    Return the order of date elements for the current culture
        /// </devdoc>
        protected static string GetDateElementOrder() {
            DateTimeFormatInfo info = DateTimeFormatInfo.CurrentInfo;
            string shortPattern = info.ShortDatePattern;
            if (shortPattern.IndexOf('y') < shortPattern.IndexOf('M')) {
                return "ymd";
            }
            else if (shortPattern.IndexOf('M') < shortPattern.IndexOf('d')) {
                return "mdy";
            }
            else {
                return "dmy";
            }
        }

        // VSWhidbey 83165
        private static int GetCurrencyGroupSize(NumberFormatInfo info) {
            int [] groupSizes = info.CurrencyGroupSizes;
            if (groupSizes != null && groupSizes.Length == 1) {
                return groupSizes[0];
            }
            else {
                return -1;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected static int CutoffYear {
            get {
                return DateTimeFormatInfo.CurrentInfo.Calendar.TwoDigitYearMax;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected static int GetFullYear(int shortYear) {
            Debug.Assert(shortYear >= 0 && shortYear < 100);
            return DateTimeFormatInfo.CurrentInfo.Calendar.ToFourDigitYear(shortYear);
        }


        /// <devdoc>
        ///    Try to convert the test into the validation data type
        /// </devdoc>
        protected static bool Convert(string text, ValidationDataType type, out object value) {
            return Convert(text, type, false, out value);
        }


        protected static bool Convert(string text, ValidationDataType type, bool cultureInvariant, out object value) {

            value = null;
            try {
                switch (type) {
                    case ValidationDataType.String:
                        value = text;
                        break;

                    case ValidationDataType.Integer:
                        value = Int32.Parse(text, CultureInfo.InvariantCulture);
                        break;

                    case ValidationDataType.Double: {
                        string cleanInput;
                        if (cultureInvariant) {
                            cleanInput = ConvertDouble(text, CultureInfo.InvariantCulture.NumberFormat);
                        }
                        else {
                            cleanInput = ConvertDouble(text, NumberFormatInfo.CurrentInfo);
                        }

                        if (cleanInput != null) {
                            value = Double.Parse(cleanInput, CultureInfo.InvariantCulture);
                        }
                        break;
                    }

                    case ValidationDataType.Date: {
                        if (cultureInvariant) {
                            value = ConvertDate(text, "ymd");
                        }
                        else {
                            // if the calendar is not gregorian, we should not enable client-side, so just parse it directly:
                            if (!(DateTimeFormatInfo.CurrentInfo.Calendar.GetType() == typeof(GregorianCalendar))) {
                                value = DateTime.Parse(text, CultureInfo.CurrentCulture);
                                break;
                            }

                            string dateElementOrder = GetDateElementOrder();
                            value = ConvertDate(text, dateElementOrder);
                        }
                        break;
                    }

                    case ValidationDataType.Currency: {
                        string cleanInput;
                        if (cultureInvariant) {
                            cleanInput = ConvertCurrency(text, CultureInfo.InvariantCulture.NumberFormat);
                        }
                        else {
                            cleanInput = ConvertCurrency(text, NumberFormatInfo.CurrentInfo);
                        }

                        if (cleanInput != null) {
                            value = Decimal.Parse(cleanInput, CultureInfo.InvariantCulture);
                        }
                        break;
                    }
                }
            }
            catch {
                value = null;
            }
            return (value != null);
        }

        private static string ConvertCurrency(string text, NumberFormatInfo info) {
            string decimalChar = info.CurrencyDecimalSeparator;
            string groupChar = info.CurrencyGroupSeparator;

            // VSWhidbey 83165
            string beginGroupSize, subsequentGroupSize;
            int groupSize = GetCurrencyGroupSize(info);
            if (groupSize > 0) {
                string groupSizeText = groupSize.ToString(NumberFormatInfo.InvariantInfo);
                beginGroupSize = "{1," + groupSizeText + "}";
                subsequentGroupSize = "{" + groupSizeText + "}";
            }
            else {
                beginGroupSize = subsequentGroupSize = "+";
            }

            // Map non-break space onto regular space for parsing
            if (groupChar[0] == 160)
                groupChar = " ";
            int digits = info.CurrencyDecimalDigits;
            bool hasDigits = (digits > 0);
            string currencyExpression =
                "^\\s*([-\\+])?((\\d" + beginGroupSize + "(\\" + groupChar + "\\d" + subsequentGroupSize + ")+)|\\d*)"
                + (hasDigits ? "\\" + decimalChar + "?(\\d{0," + digits.ToString(NumberFormatInfo.InvariantInfo) + "})" : string.Empty)
                + "\\s*$";

            Match m = Regex.Match(text, currencyExpression);
            if (!m.Success) {
                return null;
            }

            // Make sure there are some valid digits
            if (m.Groups[2].Length == 0 && hasDigits && m.Groups[5].Length == 0) {
                return null;
            }

            return m.Groups[1].Value
                   + m.Groups[2].Value.Replace(groupChar, string.Empty)
                   + ((hasDigits && m.Groups[5].Length > 0) ? "." + m.Groups[5].Value : string.Empty);
        }

        private static string ConvertDouble(string text, NumberFormatInfo info) {
            // VSWhidbey 83156: If text is empty, it would be default to 0 for
            // backward compatibility reason.
            if (text.Length == 0) {
                return "0";
            }

            string decimalChar = info.NumberDecimalSeparator;
            string doubleExpression = "^\\s*([-\\+])?(\\d*)\\" + decimalChar + "?(\\d*)\\s*$";

            Match m = Regex.Match(text, doubleExpression);
            if (!m.Success) {
                return null;
            }

            // Make sure there are some valid digits
            if (m.Groups[2].Length == 0 && m.Groups[3].Length == 0) {
                return null;
            }

            return m.Groups[1].Value
                   + (m.Groups[2].Length > 0 ? m.Groups[2].Value : "0")
                   + ((m.Groups[3].Length > 0) ? "." + m.Groups[3].Value: string.Empty);
        }

        // ****************************************************************************************************************
        // **                                                                                                            **
        // ** NOTE: When updating the regular expressions in this method, you must also update the regular expressions   **
        // **       in WebUIValidation.js::ValidatorConvert().  The server and client regular expressions must match.    **
        // **                                                                                                            **
        // ****************************************************************************************************************
        private static object ConvertDate(string text, string dateElementOrder) {
            // always allow the YMD format, if they specify 4 digits
            string dateYearFirstExpression = "^\\s*((\\d{4})|(\\d{2}))([-/]|\\. ?)(\\d{1,2})\\4(\\d{1,2})\\.?\\s*$";
            Match m = Regex.Match(text, dateYearFirstExpression);
            int day, month, year;
            if (m.Success && (m.Groups[2].Success || dateElementOrder == "ymd")) {
                day = Int32.Parse(m.Groups[6].Value, CultureInfo.InvariantCulture);
                month = Int32.Parse(m.Groups[5].Value, CultureInfo.InvariantCulture);
                if (m.Groups[2].Success) {
                    year = Int32.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
                }
                else {
                    year = GetFullYear(Int32.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture));
                }
            }
            else {
                if (dateElementOrder == "ymd") {
                    return null;
                }

                // also check for the year last format
                string dateYearLastExpression = "^\\s*(\\d{1,2})([-/]|\\. ?)(\\d{1,2})(?:\\s|\\2)((\\d{4})|(\\d{2}))(?:\\s\u0433\\.|\\.)?\\s*$";
                m = Regex.Match(text, dateYearLastExpression);
                if (!m.Success) {
                    return null;
                }
                if (dateElementOrder == "mdy") {
                    day = Int32.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
                    month = Int32.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                }
                else {
                    day = Int32.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture);
                    month = Int32.Parse(m.Groups[3].Value, CultureInfo.InvariantCulture);
                }
                if (m.Groups[5].Success) {
                    year = Int32.Parse(m.Groups[5].Value, CultureInfo.InvariantCulture);
                } else {
                    year = GetFullYear(Int32.Parse(m.Groups[6].Value, CultureInfo.InvariantCulture));
                }
            }
            return new DateTime(year, month, day);
        }


        /// <devdoc>
        ///    Compare two strings using the type and operator
        /// </devdoc>
        protected static bool Compare(string leftText, string rightText, ValidationCompareOperator op, ValidationDataType type) {
            return Compare(leftText, false, rightText, false, op, type);
        }


        protected static bool Compare(string leftText, bool cultureInvariantLeftText,
                                      string rightText, bool cultureInvariantRightText,
                                      ValidationCompareOperator op, ValidationDataType type) {
            object leftObject;
            if (!Convert(leftText, type, cultureInvariantLeftText, out leftObject))
                return false;

            if (op == ValidationCompareOperator.DataTypeCheck)
                return true;

            object rightObject;
            if (!Convert(rightText, type, cultureInvariantRightText, out rightObject))
                return true;

            int compareResult;
            switch (type) {
                case ValidationDataType.String:
                    compareResult = String.Compare((string)leftObject, (string) rightObject, false, CultureInfo.CurrentCulture);
                    break;

                case ValidationDataType.Integer:
                    compareResult = ((int)leftObject).CompareTo(rightObject);
                    break;

                case ValidationDataType.Double:
                    compareResult = ((double)leftObject).CompareTo(rightObject);
                    break;

                case ValidationDataType.Date:
                    compareResult = ((DateTime)leftObject).CompareTo(rightObject);
                    break;

                case ValidationDataType.Currency:
                    compareResult = ((Decimal)leftObject).CompareTo(rightObject);
                    break;

                default:
                    Debug.Fail("Unknown Type");
                    return true;
            }

            switch (op) {
                case ValidationCompareOperator.Equal:
                    return compareResult == 0;
                case ValidationCompareOperator.NotEqual:
                    return compareResult != 0;
                case ValidationCompareOperator.GreaterThan:
                    return compareResult > 0 ;
                case ValidationCompareOperator.GreaterThanEqual:
                    return compareResult >= 0 ;
                case ValidationCompareOperator.LessThan:
                    return compareResult < 0 ;
                case ValidationCompareOperator.LessThanEqual:
                    return compareResult <= 0 ;
                default:
                    Debug.Fail("Unknown Operator");
                    return true;
            }
        }


        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected override bool DetermineRenderUplevel() {
            // We don't do client-side validation for dates with non gregorian calendars
            if (Type == ValidationDataType.Date && DateTimeFormatInfo.CurrentInfo.Calendar.GetType() != typeof(GregorianCalendar)) {
                return false;
            }
            return base.DetermineRenderUplevel();
        }

        internal string ConvertToShortDateString(string text) {
            // VSWhidbey 83099, 85305, we should ignore error if it happens and
            // leave text as intact when parsing the date.  We assume the caller
            // (validator) is able to handle invalid text itself.
            DateTime date;
            if (DateTime.TryParse(text, CultureInfo.CurrentCulture, DateTimeStyles.None, out date)) {
                text = date.ToShortDateString();
            }
            return text;
        }

        internal bool IsInStandardDateFormat(string date) {
            // VSWhidbey 115454: We identify that date string with only numbers
            // and specific punctuation separators is in standard date format.
            const string standardDateExpression = "^\\s*(\\d+)([-/]|\\. ?)(\\d+)\\2(\\d+)\\s*$";
            return Regex.Match(date, standardDateExpression).Success;
        }

        internal string ConvertCultureInvariantToCurrentCultureFormat(string valueInString,
                                                                      ValidationDataType type) {
            object value;
            Convert(valueInString, type, true, out value);
            if (value is DateTime) {
                // For Date type we explicitly want the date portion only
                return ((DateTime) value).ToShortDateString();
            }
            else {
                return System.Convert.ToString(value, CultureInfo.CurrentCulture);
            }
        }
    }
}
