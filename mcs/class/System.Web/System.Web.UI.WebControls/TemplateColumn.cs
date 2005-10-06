//
// System.Web.UI.WebControls.TemplateColumn.cs
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
	public class TemplateColumn : DataGridColumn
	{
		private ITemplate editItemTemplate;
		private ITemplate footerTemplate;
		private ITemplate headerTemplate;
		private ITemplate itemTemplate;

		public TemplateColumn(): base()
		{
		}

		[DefaultValue (null), Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty), TemplateContainer (typeof (DataGridItem))]
		[WebSysDescription ("The template that is used to build that are being edited rows for this column.")]
		public virtual ITemplate EditItemTemplate
		{
			get
			{
				return editItemTemplate;
			}
			set
			{
				editItemTemplate = value;
				OnColumnChanged();
			}
		}

		[DefaultValue (null), Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty), TemplateContainer (typeof (DataGridItem))]
		[WebSysDescription ("The template that is used to build the footer for this column.")]
		public virtual ITemplate FooterTemplate
		{
			get
			{
				return footerTemplate;
			}
			set
			{
				footerTemplate = value;
				OnColumnChanged();
			}
		}

		[DefaultValue (null), Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty), TemplateContainer (typeof (DataGridItem))]
		[WebSysDescription ("The template that is used to build the header for this column.")]
		public virtual ITemplate HeaderTemplate
		{
			get
			{
				return headerTemplate;
			}
			set
			{
				headerTemplate = value;
				OnColumnChanged();
			}
		}

		[DefaultValue (null), Browsable (false)]
		[PersistenceMode (PersistenceMode.InnerProperty), TemplateContainer (typeof (DataGridItem))]
		[WebSysDescription ("The template that is used to build rows for this column.")]
		public virtual ITemplate ItemTemplate
		{
			get
			{
				return itemTemplate;
			}
			set
			{
				itemTemplate = value;
				OnColumnChanged();
			}
		}

		public override void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			base.InitializeCell(cell, columnIndex, itemType);
			ITemplate toRender = null;
			switch(itemType)
			{
				case ListItemType.Header: toRender = headerTemplate;
							  break;
				case ListItemType.Footer: toRender = footerTemplate;
							  break;
				case ListItemType.SelectedItem:
				case ListItemType.AlternatingItem:
				case ListItemType.Item:	  toRender = itemTemplate;
							  break;
				case ListItemType.EditItem:
					toRender = (editItemTemplate != null ? editItemTemplate : itemTemplate);
							  break;
			}
			if(toRender != null)
			{
				cell.Text = String.Empty;
				toRender.InstantiateIn(cell);
			}
		}
	}
}
