/**
 * Namespace: System.Web.UI.WebControls
 * Enumeration:     VerticalAlign
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
	[TypeConverter(typeof(VerticalAlignConverter))]
	public enum VerticalAlign
	{
		NotSet,
		Top,
		Middle,
		Bottom
	}
}
