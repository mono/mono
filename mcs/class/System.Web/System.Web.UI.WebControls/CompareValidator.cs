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

using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;

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

		[DefaultValue (""), Bindable (true), WebCategory ("Behavior")]
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
	}
}
