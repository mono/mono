/**
 * Namespace: System.Web.UI.WebControls
 * Class:     RequiredFieldValidator
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[ToolboxData("<{0}:RequiredFieldValidator runat=\"server\" "
	           + "ErrorMessage=\"RequiredFieldValidator\">"
	           + "</{0}:RequiredFieldValidator>")]
	public class RequiredFieldValidator : BaseValidator
	{
		public RequiredFieldValidator(): base()
		{
		}

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
		[WebSysDescription ("The initial value for the required field.")]
		public string InitialValue
		{
			get
			{
				object o = ViewState["InitialValue"];
				if(o != null)
					return (String)o;
				return String.Empty;
			}
			set
			{
				ViewState["InitialValue"] = value;
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(RenderUplevel)
			{
				writer.AddAttribute("evaluationfunction", "RequiredFieldValidatorEvaluateIsValid");
				writer.AddAttribute("initialvalue", InitialValue);
			}
		}

		protected override bool EvaluateIsValid()
		{
			string val = GetControlValidationValue(ControlToValidate);
			if(val != null)
			{
				return (val.Trim() != InitialValue.Trim());
			}
			return true;
		}
	}
}
