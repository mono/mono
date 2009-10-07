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
// Copyright (c) 2005 Novell, Inc. (http://www.novell.com)
//
// Authors:
//	Peter Bartok	(pbartok@novell.com)
//
//

using System.ComponentModel;
using System.Globalization;
using System.Security.Permissions;
using System.Web.Util;

// Modeled after Nikhil Kothari's sample in "ASP Server Controls and Components", pp368

namespace System.Web.UI.WebControls {

	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
#if NET_2_0
	[ToolboxData("<{0}:RangeValidator runat=\"server\" ErrorMessage=\"RangeValidator\"></{0}:RangeValidator>")]
#else
	[ToolboxData("<{0}:RangeValidator runat=server ErrorMessage=\"RangeValidator\"></{0}:RangeValidator>")]
#endif
	public class RangeValidator : BaseCompareValidator {
		#region Public Constructors
		public RangeValidator() {
		}
		#endregion	// Public Constructors

		#region Public Instance Properties
#if NET_2_0
		[Themeable (false)]
#else
		[Bindable(true)]
#endif
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public string MaximumValue {
			get {
				return ViewState.GetString("MaximumValue", String.Empty);
			}

			set {
				ViewState["MaximumValue"] = value;
			}
		}

#if NET_2_0
		[Themeable (false)]
#else
		[Bindable(true)]
#endif
		[DefaultValue("")]
		[WebSysDescription ("")]
		[WebCategory ("Behavior")]
		public string MinimumValue {
			get {
				return ViewState.GetString("MinimumValue", String.Empty);
			}

			set {
				ViewState["MinimumValue"] = value;
			}
		}
		#endregion	// Public Instance Properties

		#region Public Instance Methods
		protected override void AddAttributesToRender(HtmlTextWriter writer) {
			base.AddAttributesToRender (writer);

			if (RenderUplevel) {
#if NET_2_0
				RegisterExpandoAttribute (ClientID, "evaluationfunction", "RangeValidatorEvaluateIsValid");
				RegisterExpandoAttribute (ClientID, "minimumvalue", MinimumValue, true);
				RegisterExpandoAttribute (ClientID, "maximumvalue", MaximumValue, true);
#else
				writer.AddAttribute("evaluationfunction", "RangeValidatorEvaluateIsValid", false); // FIXME - we need to define this in client code
				writer.AddAttribute("minimumValue", MinimumValue.ToString(Helpers.InvariantCulture));
				writer.AddAttribute("maximumValue", MaximumValue.ToString(Helpers.InvariantCulture));
#endif
			}
		}

		protected override bool ControlPropertiesValid() {
			if (!CanConvert(MinimumValue, this.Type)) {
				throw new HttpException("Minimum value cannot be converterd to type " + this.Type.ToString());
			}
			if (!CanConvert(MaximumValue, this.Type)) {
				throw new HttpException("Maximum value cannot be converterd to type " + this.Type.ToString());
			}
			if (this.Type != ValidationDataType.String) {
				if (Compare(MinimumValue, MaximumValue, ValidationCompareOperator.GreaterThan, this.Type)) {
					throw new HttpException("Maximum value must be equal or bigger than Minimum value");
				}
			}
			return base.ControlPropertiesValid ();
		}

		protected override bool EvaluateIsValid() {
			string	control_value;

			control_value = GetControlValidationValue(this.ControlToValidate);
			if (control_value == null)
				return true;
			control_value = control_value.Trim();
			if (control_value.Length == 0)
				return true;

			if (Compare(control_value, MinimumValue, ValidationCompareOperator.GreaterThanEqual, this.Type)) {
				if (Compare(control_value, MaximumValue, ValidationCompareOperator.LessThanEqual, this.Type)) {
					return true;
				}
			}
			return false;
		}
		#endregion	// Public Instance Methods
	}
}
