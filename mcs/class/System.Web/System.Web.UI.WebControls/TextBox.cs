
/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TextBox
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  10%
 * 
 * (C) Gaurav Vaish (2002)
 */

using System;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class TextBox : WebControl, IPostBackDataHandler
	{
		public TextBox(): base(HtmlTextWriterTag.Input)
		{
		}
		
		public virtual bool AutoPostBack
		{
			get
			{
				object o = ViewState["AutoPostBack"];
				if(o!=null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["AutoPostBack"] = value;
			}
		}
		
		public virtual int Columns
		{
			get
			{
				object o = ViewState["Columns"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				ViewState["Columns"] = value;
			}
		}
		
		public virtual int MaxLength
		{
			get
			{
				object o = ViewState["MaxLrngth"];
				if(o != null)
					return (int)o;
				return 0;
			}
			set
			{
				ViewState["MaxLrngth"] = value;
			}
		}
		
		public virtual bool ReadOnly
		{
			get
			{
				object o = ViewState["ReadOnly"];
				if(o != null)
					return (bool)o;
				return false;
			}
			set
			{
				ViewState["ReadOnly"] = value;
			}
		}
	}
}
