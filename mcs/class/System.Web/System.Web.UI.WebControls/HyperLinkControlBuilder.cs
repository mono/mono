/**
 * Namespace: System.Web.UI.WebControls
 * Class:     HyperLinkControlBuilder
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2002)
 */

namespace System.Web.UI.WebControls
{
	public class HyperLinkControlBuilder : ControlBuilder
	{
		public HyperLinkControlBuilder(): base()
		{
		}
		
		public override bool AllowWhitespaceLiterals()
		{
			return false;
		}
	}
}
