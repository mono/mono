/**
 * Namespace: System.Web
 * Class:     WebSysDescriptionAttribute
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
using System.Web;
using System.Web.UI;

namespace System.Web
{
	[AttributeUsage(AttributeTargets.All)]
	internal class WebSysDescriptionAttribute : DescriptionAttribute
	{
		private bool isReplaced;

		public WebSysDescriptionAttribute(string description) : base(description)
		{
		}

		[MonoTODO]
		public override string Description
		{
			get
			{
				if(!isReplaced)
				{
					throw new NotImplementedException();
					//DescriptionValue = Description + do something I donno;
				}
				return Description;
			}
		}
	}
}
