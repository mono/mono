/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TargetConverter
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class TargetConverter : StringConverter
	{
		private StandardValuesCollection standardValues;
		private string[] values = {
			"_parent",
			"_self",
			"_blank",
			"_search",
			"_top"
		};
		
		public TargetConverter(): base()
		{
		}
		
		public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			if(standardValues == null)
			{
				standardValues = new StandardValuesCollection(values);
			}
			return standardValues;
		}
		
		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}
		
		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}
	}
}
