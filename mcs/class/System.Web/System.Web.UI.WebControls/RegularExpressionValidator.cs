//
// System.Web.UI.WebControls.RegularExpressionValidator.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

using System;
using System.ComponentModel;
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

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
		[Editor ("System.Web.UI.Design.WebControls.RegexTypeEditor, " + Consts.AssemblySystem_Design, typeof (System.Drawing.Design.UITypeEditor))]
		[WebSysDescription ("A regular expression that is used to validate.")]
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
