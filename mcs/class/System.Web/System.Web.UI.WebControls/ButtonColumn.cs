/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ButtonColumn
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Status:  20%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class ButtonColumn : DataGridColumn
	{
		private PropertyDescriptor textFieldDescriptor;
		
		public ButtonColumn()
		{
			Initialize();
		}

		public override void Initialize()
		{
			base.Initialize();
			textFieldDescriptor = null;
		}
		
		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell(cell, columnIndex, itemType);
			//TODO: I also have to do some column specific work
			throw new NotImplementedException();
		}
		
		public virtual ButtonColumnType ButtonType
		{
			get
			{
				object o = ViewState["ButtonType"];
				if(o!=null)
					return (ButtonColumnType)o;
				return ButtonColumnType.LinkButton;
			}
			set
			{
				if(!System.Enum.IsDefined(typeof(ButtonColumnType), value))
					throw new ArgumentException();
				ViewState["ButtonType"] = value;
			}
		}
		
		public virtual string CommandName
		{
			get
			{
				string cn = (string)ViewState["CommandName"];
				if(cn!=null)
					return cn;
				return String.Empty;
			}
			set
			{
				ViewState["CommandName"] = value;
			}
		}

		public virtual string DataTextField
		{
			get
			{
				string dtf = (string)ViewState["DataTextField"];
				if(dtf!=null)
					return dtf;
				return String.Empty;
			}
			set
			{
				ViewState["DataTextField"] = value;
			}
		}

		public virtual string DataTextFormatString
		{
			get
			{
				string dtfs = (string)ViewState["DataTextFormatString"];
				if(dtfs!=null)
					return dtfs;
				return String.Empty;
			}
			set
			{
				ViewState["DataTextFormatString"] = value;
			}
		}

		public virtual string Text
		{
			get
			{
				string text = (string)ViewState["Text"];
				if(text!=null)
					return text;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}
		
		protected virtual string FormatDataTextValue(object dataTextValue)
		{
			// TODO: The LOST WORLD! :))
			throw new NotImplementedException();
			return String.Empty;
		}
		
	}
}
