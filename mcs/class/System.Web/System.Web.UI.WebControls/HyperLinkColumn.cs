/**
 * Namespace: System.Web.UI.WebControls
 * Class:     HyperLinkColumn
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  5%
 *
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class HyperLinkColumn: DataGridColumn
	{
		PropertyDescriptor textFieldDescriptor;
		PropertyDescriptor urlFieldDescriptor;
		
		public HyperLinkColumn(): base()
		{
		}
		
		public virtual string DataNavigateUrlField
		{
			get
			{
				object o = ViewState["DataNavigateUrlField"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataNavigateUrlField"] = value;
			}
		}
	}
}
