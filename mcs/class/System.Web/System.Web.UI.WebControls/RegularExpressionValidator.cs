/**
 * Namespace: System.Web.UI.WebControls
 * Class:     RegularExpressionValidator
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;
using System.Text.RegularExpressions;

namespace System.Web.UI.WebControls
{
	[ToolboxData("<{0}:RegularExpressionValidator runat=\"server\" "
	              + "ErrorMessage=\"RegularExpressionValidator\">"
	              + "</{0}:RegularExpressionValidator>")]
	public class RegularExpressionValidator : BaseValidator
	{
		public RegularExpressionValidator(): base()
		{
		}

		public string ValidationExpression
		{
			get
			{
				object o = ViewState["ValidationExpression"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				try
				{
					Regex.IsMatch("", value);
				} catch(Exception)
				{
					throw new HttpException(HttpRuntime.FormatResourceString("Validator_bad_regex", value));
				}
				ViewState["ValidationExpression"] = value;
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(base.RenderUplevel)
			{
				writer.AddAttribute("evaluationfunction", "RegularExpressionValidatorEvaluateIsValid");
				string exp = ValidationExpression;
				if(exp.Length > 0)
				{
					writer.AddAttribute("validationexpression", exp);
				}
			}
		}

		protected override bool EvaluateIsValid ()
		{
			string ctrl = GetControlValidationValue (ControlToValidate);
			if (ctrl == null || ctrl.Trim ().Length == 0)
				return true;

			bool retVal;
			try {
				Match match = Regex.Match (ctrl, ValidationExpression);
				if (match.Success && match.Index == 0) {
					retVal = true;
				} else {
					retVal = false;
				}
			} catch (Exception) {
				retVal = true;
			}
			return retVal;
		}
	}
}
