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
				case ListItemType.Item:   toRender = itemTemplate;
				                          break;
				case ListItemType.AlternatingItem: toRender = itemTemplate;
				                          break;
				case ListItemType.SelectedItem: toRender = editItemTemplate;
				                          break;
				default:                  toRender = editItemTemplate;
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
