/**
 * Namespace: System.Web.UI.WebControls
 * Class:     EditCommandColumn
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
	public class EditCommandColumn : DataGridColumn
	{
		public EditCommandColumn(): base()
		{
		}
		
		public virtual ButtonColumnType ButtonType
		{
			get
			{
				object o = ViewState["ButtonType"];
				if(o != null)
				{
					return (ButtonColumnType)o;
				}
				return ButtonColumnType.LinkButton;
			}
			set
			{
				if(!Enum.IsDefined(typeof(ButtonColumnType), value))
				{
					throw new ArgumentException();
				}
				ViewState["ButtonType"] = value;
				OnColumnChanged();
			}
		}

		public virtual string CancelText
		{
			get
			{
				object o = ViewState["CancelText"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["CancelText"] = value;
				OnColumnChanged();
			}
		}

		public virtual string EditText
		{
			get
			{
				object o = ViewState["EditText"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["EditText"] = value;
				OnColumnChanged();
			}
		}

		public virtual string UpdateText
		{
			get
			{
				object o = ViewState["UpdateText"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["UpdateText"] = value;
				OnColumnChanged();
			}
		}
		
		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell(cell, columnIndex, itemType);
			
			if (itemType == ListItemType.Header || itemType == ListItemType.Footer)
				return;
			
			if (itemType == ListItemType.EditItem) {
				cell.Controls.Add (MakeButton ("Update", UpdateText));
				cell.Controls.Add (new LiteralControl ("&nbsp;"));
				cell.Controls.Add (MakeButton ("Cancel", CancelText));
			} else {
				cell.Controls.Add (MakeButton ("Edit", EditText));
			}
		}
		
		Control MakeButton (string commandName, string text)
		{
			if (ButtonType == ButtonColumnType.LinkButton) {
				DataGridLinkButton ret = new DataGridLinkButton ();
				ret.CommandName = commandName;
				ret.Text = text;
				return ret;
			} else {
				Button ret = new Button ();
				ret.CommandName = commandName;
				ret.Text = text;
				return ret;
			}
		}
	}
}
