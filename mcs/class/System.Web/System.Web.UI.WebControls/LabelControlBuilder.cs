/**
 * Namespace: System.Web.UI.WebControls
 * Class:     LabelControlBuilder
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

namespace System.Web.UI.WebControls
{
	public class LabelControlBuilder : ControlBuilder
	{
		public LabelControlBuilder(): base()
		{
		}
		
		public override bool AllowWhitespaceLiterals()
		{
			return false;
		}
	}
}
