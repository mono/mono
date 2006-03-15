//
// System.Web.UI.WebControls.CompareValidator
//
// Authors:
//	Chris Toshok (toshok@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

namespace System.Web.UI.WebControls {
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
#if NET_2_0
	[ToolboxData("<{0}:CompareValidator runat=\"server\" ErrorMessage=\"CompareValidator\"></{0}:CompareValidator>")]
#else
	[ToolboxData("<{0}:CompareValidator runat=server ErrorMessage=\"CompareValidator\"></{0}:CompareValidator>")]
#endif
	public class CompareValidator : BaseCompareValidator
	{
		public CompareValidator ()
		{
		}

		protected override void AddAttributesToRender (HtmlTextWriter w)
		{
			if (RenderUplevel) {
				if (ControlToCompare != "")
					w.AddAttribute ("controltocompare", ControlToCompare);
				if (ValueToCompare != "")
					w.AddAttribute ("valuetocompare", ValueToCompare);
				w.AddAttribute ("operator", Operator.ToString());
				w.AddAttribute("evaluationfunction", "CompareValidatorEvaluateIsValid");
			}

			base.AddAttributesToRender (w);
		}

		protected override bool ControlPropertiesValid ()
		{
			/* if the control id is the default "", or if we're
			 * using the one Operator that ignores the control
			 * id.. */
			if (ControlToCompare == "" || Operator == ValidationCompareOperator.DataTypeCheck)
				return base.ControlPropertiesValid();

			/* attempt to locate the ControlToCompare somewhere on the page */
			Control control = NamingContainer.FindControl (ControlToCompare);
			if (control == null)
				throw new HttpException (String.Format ("Unable to locate ControlToCompare with id `{0}'", ControlToCompare));

			return base.ControlPropertiesValid ();
		}

		protected override bool EvaluateIsValid ()
		{
			/* wtf? */
			if (GetControlValidationValue (ControlToValidate) == "")
				return true;

			string compare;
			/* ControlToCompare takes precendence, if it's set. */
			compare = (ControlToCompare != "" ? GetControlValidationValue (ControlToCompare) : ValueToCompare);

			return BaseCompareValidator.Compare (GetControlValidationValue (ControlToValidate), compare,
							     Operator,
							     this.Type);
		}

		[DefaultValue("")]
		[TypeConverter(typeof(System.Web.UI.WebControls.ValidatedControlConverter))]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
#if NET_2_0
		[Themeable (false)]
#endif
		public string ControlToCompare {
			get { return ViewState.GetString ("ControlToCompare", String.Empty); }
			set { ViewState["ControlToCompare"] = value; }
		}

		[DefaultValue(ValidationCompareOperator.Equal)]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
#if NET_2_0
		[Themeable (false)]
#endif
		public ValidationCompareOperator Operator {
			get { return (ValidationCompareOperator)ViewState.GetInt ("Operator", (int)ValidationCompareOperator.Equal); }
			set { ViewState ["Operator"] = (int)value; }
		}

#if !NET_2_0
		[Bindable(true)]
#endif
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
#if NET_2_0
		[Themeable (false)]
#endif
		public string ValueToCompare {
			get { return ViewState.GetString ("ValueToCompare", String.Empty); }
			set { ViewState ["ValueToCompare"] = value; }
		}
	}
}
