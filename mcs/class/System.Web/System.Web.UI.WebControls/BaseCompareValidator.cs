/**
 * Namespace: System.Web.UI.WebControls
 * Class:     BaseCompareValidator
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Implementation: yes
 * Contact: <gvaish@iitk.ac.in>
 * Status:  30%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public abstract class BaseCompareValidator: BaseValidator
	{
		protected BaseCompareValidator(): base()
		{
			super();
		}
		
		/*[
			WebSysDescriptionAttribute("RangeValidator_Type"),
			WebCategoryAttribute("Behaviour"),
			DefaultValueAttribute(System.Web.UI.WebControls.ValidationDataType)
		]*/

		public static bool CanConvert(string text, ValidationDataType type)
		{
			//TODO: Implement me
			return false;
		}
		
		public ValidationDataType Type
		{
			get
			{
				object o = ViewState["Type"];
				if(o!=null)
					return (ValidationDataType)o;
				return ValidationDataType.String;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(ValidationDataType), value))
					throw new ArgumentException();
				ViewState["Type"] = value;
			}
		}
		
		protected static int CutoffYear
		{
			get
			{
				return DateTimeFormatInfo.CurrentInfo.Calendar.TwoDigitYearMax;
			}
		}

		protected static int GetFullYear(int shortYear)
		{
			//TODO: Implement me
		}
		
		protected override void AddAttributesToRender(HtmlTextWriter writer)
		{
			/*
			ValidationDataType vdt;
			NumberFormatInfo   nfi;
			DateTime           dt;
			*/
			base.AddAttributesToRender(writer);
			if(base.RenderUplevel)
			{
				// TODO: The Lost World
			}
			throw new NotImplementedException();
		}
		
		// Undocumented
		protected bool Compare(string leftText, string rightText, ValidationCompareOperator op, ValidationDataType type)
		{
			throw new NotImplementedException();
		}
		
		// Uncodumented
		protected bool Convert(string text, ValidationDataType type, out object convertedValue)
		{
			throw new NotImplementedException();
			convertedValue = null;
			try
			{
				switch(type)
				{
					case
				}
			}
		}
	}
}
