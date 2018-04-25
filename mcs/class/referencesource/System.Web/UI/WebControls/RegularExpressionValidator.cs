//------------------------------------------------------------------------------
// <copyright file="RegularExpressionValidator.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 */

namespace System.Web.UI.WebControls {

    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Text.RegularExpressions;
    using System.Drawing.Design;
    using System.Web;
    using System.Web.Util;


    /// <devdoc>
    ///    <para>Checks if the value of the associated input control matches the pattern 
    ///       of a regular expression.</para>
    /// </devdoc>
    [
    ToolboxData("<{0}:RegularExpressionValidator runat=\"server\" ErrorMessage=\"RegularExpressionValidator\"></{0}:RegularExpressionValidator>")
    ]
    public class RegularExpressionValidator : BaseValidator {


        /// <devdoc>
        ///    <para>Indicates the regular expression assigned to be the validation criteria.</para>
        /// </devdoc>
        [
        WebCategory("Behavior"),
        Themeable(false),
        DefaultValue(""),
        Editor("System.Web.UI.Design.WebControls.RegexTypeEditor, " + AssemblyRef.SystemDesign, typeof(UITypeEditor)),
        WebSysDescription(SR.RegularExpressionValidator_ValidationExpression)
        ]                                         
        public string ValidationExpression {
            get { 
                object o = ViewState["ValidationExpression"];
                return((o == null) ? String.Empty : (string)o);
            }
            set {
                try {
                    Regex.IsMatch(String.Empty, value);
                }
                catch (Exception e) {
                    throw new HttpException(
                                           SR.GetString(SR.Validator_bad_regex, value), e);                    
                }
                ViewState["ValidationExpression"] = value;
            }
        }

        // The timeout for regex
        public int? MatchTimeout { get; set; }

        /// <internalonly/>
        /// <devdoc>
        ///    AddAttributesToRender method
        /// </devdoc>
        protected override void AddAttributesToRender(HtmlTextWriter writer) {
            base.AddAttributesToRender(writer);
            if (RenderUplevel) {
                string id = ClientID;
                HtmlTextWriter expandoAttributeWriter = (EnableLegacyRendering || IsUnobtrusive) ? writer : null;
                AddExpandoAttribute(expandoAttributeWriter, id, "evaluationfunction", "RegularExpressionValidatorEvaluateIsValid", false);
                if (ValidationExpression.Length > 0) {
                    AddExpandoAttribute(expandoAttributeWriter, id, "validationexpression", ValidationExpression);
                }
            }
        }            


        /// <internalonly/>
        /// <devdoc>
        ///    EvaluateIsValid method
        /// </devdoc>
        protected override bool EvaluateIsValid() {

            // Always succeeds if input is empty or value was not found
            string controlValue = GetControlValidationValue(ControlToValidate);
            Debug.Assert(controlValue != null, "Should have already been checked");
            if (controlValue == null || controlValue.Trim().Length == 0) {
                return true;
            }

            try {
                // we are looking for an exact match, not just a search hit
                // Adding timeout for Regex in case of malicious string causing DoS
                Match m = RegexUtil.Match(controlValue, ValidationExpression, RegexOptions.None, MatchTimeout);

                return(m.Success && m.Index == 0 && m.Length == controlValue.Length);
            } 
            catch (ArgumentOutOfRangeException) {
                throw;
            } 
            catch {
                Debug.Fail("Regex error should have been caught in property setter.");
                return true;
            }
        }

    }
}

