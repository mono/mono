//
// System.Web.UI.WebControls.RangeValidator.cs
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
using System.Xml;

namespace System.Web.UI.WebControls
{
	[ToolboxData("<{0}:RangeValidator runat=\"server\" "
	              + "ErrorMessage=\"RangeValidator\"></{0}:RangeValidator>")]
	public class RangeValidator : BaseCompareValidator
	{
		public RangeValidator(): base()
		{
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("The maximum value that the validated control can be assigned.")]
		public string MaximumValue
		{
			get
			{
				object o = ViewState["MaximumValue"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["MaximumValue"] = value;
			}
		}

#if NET_2_0
	    [ThemeableAttribute (false)]
#else
		[Bindable (true)]
#endif
		[DefaultValue (""), WebCategory ("Behavior")]
		[WebSysDescription ("The minimum value that the validated control can be assigned.")]
		public string MinimumValue
		{
			get
			{
				object o = ViewState["MinimumValue"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["MinimumValue"] = value;
			}
		}

		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			base.AddAttributesToRender(writer);
			if(base.RenderUplevel)
			{
				writer.AddAttribute("evaluationfunction", "RangeValidatorEvaluateIsValid");
				writer.AddAttribute("maximumvalue", MaximumValue);
				writer.AddAttribute("minimumvalue", MinimumValue);
			}
		}

		protected override bool ControlPropertiesValid()
		{
			string max = MaximumValue;
			if(!CanConvert(max, Type))
			{
				string[] fmt = new string[4];
				fmt[0] = max;
				fmt[1] = "MaximumValue";
				fmt[2] = ID;
				fmt[3] = PropertyConverter.EnumToString(typeof(ValidationDataType), Type);
				throw new HttpException(HttpRuntime.FormatResourceString("Validator_value_bad_type", fmt));
			}
			string min = MinimumValue;
			if(!CanConvert(min, Type))
			{
				string[] fmt = new string[4];
				fmt[0] = min;
				fmt[1] = "MinimumValue";
				fmt[2] = ID;
				fmt[3] = PropertyConverter.EnumToString(typeof(ValidationDataType), Type);
				throw new HttpException(HttpRuntime.FormatResourceString("Validator_value_bad_type", fmt));
			}

			if(Compare(min,max,  ValidationCompareOperator.GreaterThan, Type))
			{
				string[] fmt = new string[3];
				fmt[0] = min;
				fmt[1] = max;
				fmt[2] = PropertyConverter.EnumToString(typeof(ValidationDataType), Type);
				throw new HttpException(HttpRuntime.FormatResourceString("Validator_range_overalap", fmt));
			}
			return base.ControlPropertiesValid();
		}

		protected override bool EvaluateIsValid()
		{
			string ctrl = GetControlValidationValue(ControlToValidate);
			if(ctrl == null || ctrl.Trim().Length == 0)
			{
				return true;
			}
			bool retVal = Compare(ctrl, MinimumValue, ValidationCompareOperator.GreaterThanEqual,
			                     this.Type);
			if(retVal)
			{
				retVal = Compare(ctrl, MaximumValue, ValidationCompareOperator.LessThanEqual,
				                 this.Type);
			}
			return retVal;
		}
	}
}
