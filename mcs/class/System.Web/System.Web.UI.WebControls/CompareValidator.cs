/**
* Namespace: System.Web.UI.WebControls
* Class:     CompareValidator
*
* Author:  Gaurav Vaish
* Maintainer: gvaish@iitk.ac.in
* Implementation: yes
* Status:  80%
*
* (C) Gaurav Vaish (2001)
*/

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[ToolboxData("<{0}:CompareValidator runat=\"server\""
	             + "ErrorMessage=\"CompareValidator\"></{0}:CompareValidator>")]
	public class CompareValidator: BaseCompareValidator
	{
		public CompareValidator()
		{
			// Intitalize();
		}

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

		protected override bool EvaluateIsValid()
		{
			string ctrl = GetControlValidationValue(ControlToValidate);
			if(ctrl != null && ctrl.Length > 0)
			{
				string cmp = (ControlToCompare.Length > 0 ?
				              ControlToCompare : ValueToCompare);
				return BaseCompareValidator.Compare(ctrl, cmp,
				                                    Operator, Type);
			}
			return true;
		}
	}
}
