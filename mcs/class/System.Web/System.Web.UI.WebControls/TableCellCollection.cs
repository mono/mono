/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TableCellCollection
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  ??%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public sealed class TableCellCollection: IList, ICollection, IEnumerable
	{
		private TableRow owner;
		
		public TableCellCollection(TableRow owner)
		{
			this.owner = owner;
		}
	}
}
