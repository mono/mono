
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
using System.ComponentModel;

namespace System.Web.UI.WebControls
{
	public class EditCommandColumn : DataGridColumn
	{
		public EditCommandColumn(): base()
		{
		}

#if NET_2_0
	    [DefaultValueAttribute (ButtonColumnType.LinkButton)]
#endif
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

#if NET_2_0
	    [LocalizableAttribute (true)]
    	[DefaultValueAttribute ("")]
#endif
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

#if NET_2_0
	    [DefaultValueAttribute ("")]
    	[LocalizableAttribute (true)]
#endif
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

#if NET_2_0
	    [DefaultValueAttribute ("")]
    	[LocalizableAttribute (true)]
#endif
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
