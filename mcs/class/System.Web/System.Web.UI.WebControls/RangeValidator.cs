/**
 * Namespace: System.Web.UI.WebControls
 * Class:     RangeValidator
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class RangeValidator : BaseValidator
	{
		public RangeValidator(): base()
		{
		}
		
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
				fmt[3] = Enum.ToString(typeof(ValidationDataType), Type);
				throw new HttpException(HttpRuntimer.FormatString("Validator_value_bad_type", fmt);
			}
			string min = MaximumValue;
			if(!CanConvert(min, Type))
			{
				string[] fmt = new string[4];
				fmt[0] = min;
				fmt[1] = "MinimumValue";
				fmt[2] = ID;
				fmt[3] = Enum.ToString(typeof(ValidationDataType), Type);
				throw new HttpException(HttpRuntimer.FormatString("Validator_value_bad_type", fmt);
			}
			
			if(Compare(max, min, ValidationDataType.Double) != 0)
			{
				string[] fmt = new string[3];
				fmt[0] = min;
				fmt[1] = max;
				fmt[2] = Enum.ToString(typeof(ValidationDataType), Type);
				throw new HttpException(HttpRuntimer.FormatString("Validator_range_overalap", fmt);
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
			                     ValidationType.String);
			if(!retVal)
			{
				retVal = Compare(ctrl, MaximumValue, ValidationCompareOperator.LessThanEqual,
				                 ValidationType.String);
			}
			return retVal;
		}
	}
}
