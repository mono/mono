/**
 * Namespace: System.Web
 * Class:     WebCategoryAttribute
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  95%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;

namespace System.Web
{
	[AttributeUsage(AttributeTargets.All)]
	internal sealed class WebCategoryAttribute : CategoryAttribute
	{
		public WebCategoryAttribute(string category) : base(category)
		{
		}

		[MonoTODO]
		protected override string GetLocalizedString(string value)
		{
			string retVal = base.GetLocalizedString(value);
			if(retVal == null)
			{
				throw new NotImplementedException();
				//retVal = "Category_" + something I don't know how to get!
			}
			return retVal;
		}
	}
}
