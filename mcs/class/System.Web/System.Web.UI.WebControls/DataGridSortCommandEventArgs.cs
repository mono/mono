/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGridSortCommandEventArgs
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
	public sealed class DataGridSortCommandEventArgs : EventArgs
	{
		private object source;
		private string sortExpr;

		public DataGridSortCommandEventArgs(object commandSource, DataGridCommandEventArgs dce)
		{
			source   = commandSource;
			sortExpr = (string)dce.CommandArgument;
		}

		public object CommandSource
		{
			get
			{
				return source;
			}
		}

		public string SortExpression
		{
			get
			{
				return sortExpr;
			}
		}
	}
}
