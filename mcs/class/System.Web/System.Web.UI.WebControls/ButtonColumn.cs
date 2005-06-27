//
// System.Web.UI.WebControls.ButtonColumn.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//
// (C) Gaurav Vaish (2002)
// (C) 2003 Andreas Nahr
//

//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

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
					LinkButton lb = new DataGridLinkButton();
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
					retVal = String.Format(DataTextFormatString, dataTextValue);
				}
				else
				{
					retVal = dataTextValue.ToString();
				}
			}
			return retVal;
		}

		// LAMESPEC The framework uses Description values for metadata here. However they should be WebSysDescriptions
		// because all metadata in this namespace has WebSysDescriptions

#if !NET_2_0
		[Description ("The type of button used in this column.")]
#endif
		[DefaultValue (typeof (ButtonColumnType), "LinkButton"), WebCategory ("Misc")]
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

#if !NET_2_0
		[Description ("The command assigned to this column.")]
#endif
		[DefaultValue (""), WebCategory ("Misc")]
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

#if !NET_2_0
		[Description ("The datafield that is bound to the text property.")]
#endif
		[DefaultValue (""), WebCategory ("Misc")]
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

#if !NET_2_0
		[Description ("A format that is applied to the bound text property.")]
#endif
		[DefaultValue (""), WebCategory ("Misc")]
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

#if NET_2_0
		[Localizable (true)]
#else
		[Description ("The text used for this button.")]
#endif
		[DefaultValue (""), WebCategory ("Misc")]
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
