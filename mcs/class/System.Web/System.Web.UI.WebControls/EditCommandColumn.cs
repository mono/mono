/**
 * Namespace: System.Web.UI.WebControls
 * Class:     EditCommandColumn
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  95%
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
		
		[MonoTODO]
		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell(cell, columnIndex, itemType);
			//TODO: I have to read some documents.
		}
	}
}
