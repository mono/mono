/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TableHeaderCell
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
	public class TableHeaderCell: TableCell
	{
		public TableHeaderCell(): base(HtmlTextWriterTag.Th)
		{
		}
	}
}
