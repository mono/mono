/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ListItemControlBuilder
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
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class ListItemControlBuilder : ControlBuilder
	{
		public ListItemControlBuilder(): base()
		{
		}
		
		public override bool AllowWhitespaceLiterals()
		{
			return false;
		}
		
		public override bool HtmlDecodeLiterals()
		{
			return true;
		}
	}
}
