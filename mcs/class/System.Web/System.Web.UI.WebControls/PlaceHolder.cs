/**
 * Namespace: System.Web.UI.WebControls
 * Class:     PlaceHolder
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
	[ControlBuilder(typeof(PlaceHolderControlBuilder))]
	[DefaultProperty("ID")]
	public class PlaceHolder : Control
	{
		public PlaceHolder(): base()
		{
		}
	}
}
