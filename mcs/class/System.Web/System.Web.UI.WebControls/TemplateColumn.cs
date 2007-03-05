//
// System.Web.UI.WebControls.TemplateColumn
//
// Authors:
//	Ben Maurer (bmaurer@novell.com)
//
// (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.ComponentModel;
using System.Web.UI;

namespace System.Web.UI.WebControls {
	public class TemplateColumn : DataGridColumn {
		public override void InitializeCell (TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell (cell, columnIndex, itemType);
			ITemplate t = null;
			switch (itemType) {
			case ListItemType.Header:
				t = HeaderTemplate;
				break;
			case ListItemType.Footer:
				t = FooterTemplate;
				break;
			case ListItemType.Item:
			case ListItemType.AlternatingItem:
			case ListItemType.SelectedItem:
				t = ItemTemplate;
				if (t == null)
					cell.Text = "&nbsp;";
				break;
			case ListItemType.EditItem:
				t = EditItemTemplate;
				if (t == null)
					t = ItemTemplate;
				if (t == null)
					cell.Text = "&nbsp;";
				break;
			}
			
			if (t != null)
				t.InstantiateIn (cell);
		}

		ITemplate editItemTemplate, footerTemplate, headerTemplate, itemTemplate;
		
		[Browsable(false)]
		[DefaultValue (null)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof(DataGridItem))]
		[WebSysDescription ("")]
		public virtual ITemplate EditItemTemplate {
			get { return editItemTemplate; }
			set { editItemTemplate = value; }
		}
		
		[Browsable(false)]
		[DefaultValue (null)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof(DataGridItem))]
		[WebSysDescription ("")]
		public virtual ITemplate FooterTemplate {
			get { return footerTemplate; }
			set { footerTemplate = value; }	
		}
		
		[Browsable(false)]
		[DefaultValue (null)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof(DataGridItem))]
		[WebSysDescription ("")]
		public virtual ITemplate HeaderTemplate {
			get { return headerTemplate; }
			set { headerTemplate = value; }
		}
		
		[Browsable(false)]
		[DefaultValue (null)]
		[PersistenceMode(PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof(DataGridItem))]
		[WebSysDescription ("")]
		public virtual ITemplate ItemTemplate {
			get { return itemTemplate; }
			set { itemTemplate = value; }
		}
	}
}
