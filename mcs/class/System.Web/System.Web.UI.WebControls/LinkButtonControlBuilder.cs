/**
 * Namespace: System.Web.UI.WebControls
 * Class:     LinkButtonControlBuilder
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class LinkButtonControlBuilder : ControlBuilder
	{
		public LinkButtonControlBuilder(): base()
		{
		}
		
		public override bool AllowWhitespaceLiterals()
		{
			return false;
		}
	}
}
