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

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
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

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
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
			string min = MaximumValue;
			if(!CanConvert(min, Type))
			{
				string[] fmt = new string[4];
				fmt[0] = min;
				fmt[1] = "MinimumValue";
				fmt[2] = ID;
				fmt[3] = PropertyConverter.EnumToString(typeof(ValidationDataType), Type);
				throw new HttpException(HttpRuntime.FormatResourceString("Validator_value_bad_type", fmt));
			}

			if(Compare(max, min, ValidationCompareOperator.GreaterThan, ValidationDataType.Double))
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
