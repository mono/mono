//
// System.Web.UI.WebControls.CompareValidator.cs
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
using System.Web;
using System.Web.UI;
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[ToolboxData("<{0}:CompareValidator runat=\"server\""
	             + "ErrorMessage=\"CompareValidator\"></{0}:CompareValidator>")]
	public class CompareValidator: BaseCompareValidator
#if NET_2_0
		, IStaticTextControl
#endif
	{
		public CompareValidator()
		{
			// Intitalize();
		}

#if NET_2_0
    	[ThemeableAttribute (false)]
#endif
		[DefaultValue (""), WebCategory ("Behavior")]
		[TypeConverter (typeof (ValidatedControlConverter))]
		[WebSysDescription ("The ID of a control that is compared.")]
		public string ControlToCompare
		{
			get
			{
				object o = ViewState["ControlToCompare"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}

			set
			{
				ViewState["ControlToCompare"] = value;
			}
		}

#if NET_2_0
    	[ThemeableAttribute (false)]
#endif
		[DefaultValue (typeof (ValidationCompareOperator), "Equal"), WebCategory ("Behavior")]
		[WebSysDescription ("The operator that is used for comparison.")]
		public ValidationCompareOperator Operator
		{
			get
			{
				object o = ViewState["Operator"];
				if(o!=null)
					return (ValidationCompareOperator)o;
				return ValidationCompareOperator.Equal;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(ValidationCompareOperator), value))
					throw new ArgumentException();
				ViewState["Operator"] = value;
			}
		}

#if NET_2_0
    	[ThemeableAttribute (false)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("The value that is compared to.")]
		public string ValueToCompare
		{
			get
			{
				object o = ViewState["ValueToCompare"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["ValueToCompare"] = value;
			}
		}

		protected override bool EvaluateIsValid ()
		{
			string ctrl = GetControlValidationValue (ControlToValidate);
			if (ctrl == null || ctrl.Length == 0)
				return true;

			string cmp;
			if (ControlToCompare.Length > 0) {
				cmp = GetControlValidationValue (ControlToCompare);
			} else {
				cmp = ValueToCompare;
			}

			return Compare (ctrl, cmp, Operator, Type);
		}

		[MonoTODO]
		protected override void AddAttributesToRender (HtmlTextWriter writer)
		{
			base.AddAttributesToRender (writer);
		}

		[MonoTODO]
		protected override bool ControlPropertiesValid ()
		{
			return base.ControlPropertiesValid ();
		}
	}
}
