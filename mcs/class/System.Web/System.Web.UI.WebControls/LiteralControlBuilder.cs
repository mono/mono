/**
 * Namespace: System.Web.UI.WebControls
 * Class:     LiteralControlBuilder
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
	public class LiteralControlBuilder : ControlBuilder
	{
		public LiteralControlBuilder(): base()
		{
		}
		
		public override bool AllowWhitespaceLiterals()
		{
			return false;
		}
		
		public override void AppendSubBuilder(ControlBuilder subBuilder)
		{
			throw new HttpException(HttpRuntime.FormatResourceString("Control_does_not_allow_children",(typeof(Literal)).ToString()));
		}
	}
}
