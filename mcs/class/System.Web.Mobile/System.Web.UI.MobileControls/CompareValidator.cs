/**
 * Project   : Mono
 * Namespace : System.Web.UI.MobileControls
 * Class     : CompareValidator
 * Author    : Gaurav Vaish
 *
 * Copyright : 2003 with Gaurav Vaish, and with
 *             Ximian Inc
 */

using System.ComponentModel;
using System.Web.UI;
using System.Web.Mobile;
using System.Web.UI.WebControls;

namespace System.Web.UI.MobileControls
{
	public class CompareValidator : BaseValidator
	{
		private System.Web.UI.WebControls.CompareValidator webCmpVal;
		
		public CompareValidator()
		{
		}
		
		protected override System.Web.UI.WebControls.BaseValidator CreateWebValidator()
		{
			webCmpVal = new System.Web.UI.WebControls.CompareValidator();
			return webCmpVal;
		}
		
		protected override bool EvaluateIsValid()
		{
			return base.EvaluateIsValidInternal();
		}

		protected override bool ControlPropertiesValid()
		{
			if(ControlToCompare.Length > 0)
			{
				base.CheckControlValidationProperty(ControlToCompare, "ControlToCompare");
				if(String.Compare(ControlToCompare, ControlToValidate, true) == 0)
				{
					// FIXME
					throw new ArgumentException("CompareValidator_BadCompareControl");
				}
			} else
			{
				if(Operator != ValidationCompareOperator.DataTypeCheck)
				{
					if(!BaseCompareValidator.CanConvert(ValueToCompare, Type))
					{
						// FIXME
						throw new ArgumentException("Validator_ValueBadType");
					}
				}
			}
			return base.ControlPropertiesValid();
		}
		
		public string ControlToCompare
		{
			get
			{
				return webCmpVal.ControlToCompare;
			}
			set
			{
				webCmpVal.ControlToCompare = value;
			}
		}
		
		public ValidationCompareOperator Operator
		{
			get
			{
				return webCmpVal.Operator;
			}
			set
			{
				webCmpVal.Operator = value;
			}
		}
		
		public ValidationDataType Type
		{
			get
			{
				return webCmpVal.Type;
			}
			set
			{
				webCmpVal.Type = value;
			}
		}
		
		public string ValueToCompare
		{
			get
			{
				return webCmpVal.ValueToCompare;
			}
			set
			{
				webCmpVal.ValueToCompare = value;
			}
		}
	}
}
