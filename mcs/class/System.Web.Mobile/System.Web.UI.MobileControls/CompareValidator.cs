
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
