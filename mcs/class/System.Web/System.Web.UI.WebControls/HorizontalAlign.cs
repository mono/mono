/**
 * Namespace: System.Web.UI.WebControls
 * Enumeration:     HorizontalAlign
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	[TypeConverter(typeof(HorizontalAlignConverter))]
	public enum HorizontalAlign
	{
		NotSet,
		Left,
		Center,
		Right,
		Justify
	}
}
