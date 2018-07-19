//
// System.Web.UI.WebControls.CompareValidator
//
// Authors:
//	Chris Toshok (toshok@novell.com)
//
// (C) 2005-2010 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Globalization;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[ToolboxData("<{0}:CompareValidator runat=\"server\" ErrorMessage=\"CompareValidator\"></{0}:CompareValidator>")]
	public class CompareValidator : BaseCompareValidator
	{
		public CompareValidator ()
		{
		}

		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			if (RenderUplevel) {
				RegisterExpandoAttribute (ClientID, "evaluationfunction", "CompareValidatorEvaluateIsValid");
				if (ControlToCompare.Length > 0)
					RegisterExpandoAttribute (ClientID, "controltocompare", GetControlRenderID (ControlToCompare));
				if (ValueToCompare.Length > 0)
					RegisterExpandoAttribute (ClientID, "valuetocompare", ValueToCompare, true);
				RegisterExpandoAttribute (ClientID, "operator", Operator.ToString ());
			}

			base.AddAttributesToRender (writer);
		}

		protected override bool ControlPropertiesValid ()
		{
			if ((this.Operator != ValidationCompareOperator.DataTypeCheck) && ControlToCompare.Length == 0 &&
			    !BaseCompareValidator.CanConvert (this.ValueToCompare, this.Type, this.CultureInvariantValues)) {
				throw new HttpException(
					String.Format("Unable to convert the value: {0} as a {1}", ValueToCompare,
						      Enum.GetName(typeof(ValidationDataType), this.Type)));
			}

			if (ControlToCompare.Length > 0) {
				if (string.CompareOrdinal (ControlToCompare, ControlToValidate) == 0)
					throw new HttpException (String.Format ("Control '{0}' cannot have the same value '{1}' for both ControlToValidate and ControlToCompare.", ID, ControlToCompare));
				CheckControlValidationProperty (ControlToCompare, String.Empty);
			}
			
			return base.ControlPropertiesValid ();
		}

		protected override bool EvaluateIsValid ()
		{
			string control_value;

			control_value = GetControlValidationValue (this.ControlToValidate);
			if (control_value == null)
				return true;
			control_value = control_value.Trim ();
			if (control_value.Length == 0)
				return true;

			string compare;
			/* ControlToCompare takes precendence, if it's set. */
			string controlToCompare = ControlToCompare;
			compare = (!String.IsNullOrEmpty (controlToCompare) ? GetControlValidationValue (controlToCompare) : ValueToCompare);

			return BaseCompareValidator.Compare (GetControlValidationValue (ControlToValidate), false, 
							     compare, this.CultureInvariantValues,
							     Operator, this.Type);
		}

		[DefaultValue("")]
		[TypeConverter(typeof(System.Web.UI.WebControls.ValidatedControlConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[Themeable (false)]
		public string ControlToCompare {
			get { return ViewState.GetString ("ControlToCompare", String.Empty); }
			set { ViewState["ControlToCompare"] = value; }
		}

		[DefaultValue(ValidationCompareOperator.Equal)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[Themeable (false)]
		public ValidationCompareOperator Operator {
			get { return (ValidationCompareOperator)ViewState.GetInt ("Operator", (int)ValidationCompareOperator.Equal); }
			set { ViewState ["Operator"] = (int)value; }
		}

		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		[Themeable (false)]
		public string ValueToCompare {
			get { return ViewState.GetString ("ValueToCompare", String.Empty); }
			set { ViewState ["ValueToCompare"] = value; }
		}
	}
}
