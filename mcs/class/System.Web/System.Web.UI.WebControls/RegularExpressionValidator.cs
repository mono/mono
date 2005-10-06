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

#if NET_2_0
	    [ThemeableAttribute (false)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Behavior")]
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
				retVal = Regex.IsMatch (ctrl, "^" + ValidationExpression + "$");
			} catch (Exception) {
				retVal = true;
			}
			return retVal;
		}
	}
}
