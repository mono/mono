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

		public virtual string DataNavigateUrlFormatString
		{
			get
			{
				object o = ViewState["DataNavigateUrlFormatString"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataNavigateUrlFormatString"] = value;
			}
		}

		public virtual string DataTextField
		{
			get
			{
				object o = ViewState["DataTextField"];
				if(o != null)
					return (string)o;
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
				object o = ViewState["DataTextFormatString"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataTextFormatString"] = value;
			}
		}

		public virtual string NavigateUrl
		{
			get
			{
				object o = ViewState["NavigateUrl"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["NavigateUrl"] = value;
			}
		}

		public virtual string Target
		{
			get
			{
				object o = ViewState["Target"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Target"] = value;
			}
		}

		public virtual string Text
		{
			get
			{
				object o = ViewState["Text"];
				if(o != null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["Text"] = value;
			}
		}

		public override void Initialize()
		{
			textFieldDescriptor = null;
			urlFieldDescriptor  = null;
		}

		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell(cell, columnIndex, itemType);
			if(Enum.IsDefined(typeof(ListItemType), itemType) && itemType != ListItemType.Footer)
			{
				HyperLink toDisplay = new HyperLink();
				toDisplay.Text                 = Text;
				toDisplay.NavigateUrl          = NavigateUrl;
				toDisplay.Target               = Target;
				if(DataTextField.Length > 0 || DataNavigateUrlField.Length>0)
				{
					toDisplay.DataBinding += new EventHandler(OnDataBindHyperLinkColumn);
				}
				cell.Controls.Add(toDisplay);
			}
		}

		private void OnDataBindHyperLinkColumn(object sender, EventArgs e)
		{
			HyperLink link = (HyperLink)sender;
			object item    = ((DataGridItem)link.NamingContainer).DataItem;
			if(textFieldDescriptor == null && urlFieldDescriptor == null)
			{
				textFieldDescriptor = TypeDescriptor.GetProperties(item).Find(DataTextField, true);
				if(textFieldDescriptor == null && !DesignMode)
					throw new HttpException(HttpRuntime.FormatResourceString("Field_Not_Found", DataTextField));
				urlFieldDescriptor = TypeDescriptor.GetProperties(item).Find(DataNavigateUrlField, true);
				if(urlFieldDescriptor == null && !DesignMode)
					throw new HttpException(HttpRuntime.FormatResourceString("Field_Not_Found", DataNavigateUrlField));
			}

			if(textFieldDescriptor != null)
			{
				link.Text = FormatDataTextValue(textFieldDescriptor.GetValue(item));
			} else
			{
				link.Text = "Sample_DataBound_Text";
			}

			if(urlFieldDescriptor != null)
			{
				link.NavigateUrl = FormatDataNavigateUrlValue(urlFieldDescriptor.GetValue(item));
			} else
			{
				link.NavigateUrl = "url";
			}
		}

		protected virtual string FormatDataNavigateUrlValue(object dataUrlValue)
		{
			string retVal = String.Empty;
			if(dataUrlValue != null)
			{
				if(DataNavigateUrlFormatString.Length > 0)
				{
					retVal = String.Format(DataNavigateUrlFormatString, dataUrlValue);
				} else
				{
					retVal = dataUrlValue.ToString();
				}
			}
			return retVal;
		}

		protected virtual string FormatDataTextValue(object dataTextValue)
		{
			string retVal = String.Empty;
			if(dataTextValue != null)
			{
				if(DataTextFormatString.Length > 0)
				{
					retVal = String.Format(DataTextFormatString, dataTextValue);
				} else
				{
					retVal = dataTextValue.ToString();
				}
			}
			return retVal;
		}
	}
}
