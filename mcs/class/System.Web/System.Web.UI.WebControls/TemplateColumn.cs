/**
 * Namespace: System.Web.UI.WebControls
 * Class:     TemplateColumn
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
	public class TemplateColumn : DataGridColumn
	{
		private ITemplate editItemTemplate;
		private ITemplate footerTemplate;
		private ITemplate headerTemplate;
		private ITemplate itemTemplate;
		
		public TemplateColumn(): base()
		{
		}

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
			}
			if(toRender != null)
			{
				cell.Text = String.Empty;
				toRender.InstantiateIn(cell);
			}
		}
	}
}
