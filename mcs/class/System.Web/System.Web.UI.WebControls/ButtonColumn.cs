/**
 * Namespace: System.Web.UI.WebControls
 * Class:     ButtonColumn
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
using System.ComponentModel;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class ButtonColumn : DataGridColumn
	{
		private PropertyDescriptor textFieldDescriptor;
		
		public ButtonColumn(): base()
		{
		}

		public override void Initialize()
		{
			base.Initialize();
			textFieldDescriptor = null;
		}
		
		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell(cell, columnIndex, itemType);
			if (Enum.IsDefined(typeof(ListItemType), itemType) &&
			    itemType != ListItemType.Footer &&
			    itemType != ListItemType.Header)
			{
				WebControl toDisplay = null;
				if(ButtonType == ButtonColumnType.PushButton)
				{
					Button b = new Button();
					b.Text = Text;
					b.CommandName = CommandName;
					b.CausesValidation = false;
					toDisplay = b;
				} else
				{
					LinkButton lb = new LinkButton();
					lb.Text = Text;
					lb.CommandName = CommandName;
					lb.CausesValidation = false;
					toDisplay = lb;
				}
				if(DataTextField.Length > 0)
				{
					toDisplay.DataBinding += new EventHandler(OnDataBindButtonColumn);
				}
				cell.Controls.Add(toDisplay);
			}
		}
		
		private void OnDataBindButtonColumn(object sender, EventArgs e)
		{
			Control ctrl = (Control)sender;
			object item = ((DataGridItem)ctrl.NamingContainer).DataItem;
			if(textFieldDescriptor == null)
			{
				textFieldDescriptor = TypeDescriptor.GetProperties(item).Find(DataTextField, true);
				if(textFieldDescriptor == null && !DesignMode)
					throw new HttpException(HttpRuntime.FormatResourceString("Field_Not_Found", DataTextField));
			}
			string text;
			if(textFieldDescriptor != null)
			{
				text = FormatDataTextValue(textFieldDescriptor.GetValue(item));
			} else
			{
				text = "Sample_DataBound_Text";
			}
			if(ctrl is LinkButton)
			{
				((LinkButton)ctrl).Text = text;
			}
			else
			{
				((Button)ctrl).Text = text;
			}
		}
		
		protected virtual string FormatDataTextValue(object dataTextValue)
		{
			string retVal = null;
			if(dataTextValue != null)
			{
				if(DataTextFormatString.Length > 0)
				{
					retVal = String.Format((string)dataTextValue, DataTextFormatString);
				}
				else
				{
					retVal = dataTextValue.ToString();
				}
			}
			return retVal;
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
	}
}
