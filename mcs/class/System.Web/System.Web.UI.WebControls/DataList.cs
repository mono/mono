/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataList
 * 
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  20%
 * 
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class DataList: BaseDataList, INamingContainer, IRepeatInfoUser
	{
		//
		public const string CancelCommandName = "Cancel";
		public const string DeleteCommandName = "Delete";
		public const string EditCommandName   = "Edit";
		public const string SelectCommandName = "Select";
		public const string UpdateCommandName = "Update";

		//TODO: From where will I update the values of the following ItemStyles?
		private TableItemStyle alternatingItemStyle;
		private TableItemStyle editItemStyle;
		private TableItemStyle footerStyle;
		private TableItemStyle headerStyle;

		private ITemplate alternatingItemTemplate;
		private ITemplate editItemTemplate;
		private ITemplate footerTemplate;
		private ITemplate headerTemplate;

		private int editItemIndex;
		private bool extractTemplateRows;
		
		private ArrayList itemsArray;
		
		public DataList()
		{
			alternatingItemStyle = new TableItemStyle();
			editItemStyle        = new TableItemStyle();
			footerStyle          = new TableItemStyle();

			alternatingItemTemplate = null;
			editItemTemplate        = null;
			footerTemplate          = null;
			headerTemplate          = null;
			
			extractTemplateRows = false;
			
			itemsArray = null;
		}
		
		public virtual TableItemStyle AlternatingItemStyle
		{
			get
			{
				return alternatingItemStyle;
			}
		}
		
		public virtual ITemplate AlternatingItemTemplate
		{
			get
			{
				return alternatingItemTemplate;
			}
			set
			{
				alternatingItemTemplate = value;
			}
		}
		
		public virtual int EditItemIndex
		{
			get
			{
				return editItemIndex;
			}
			set
			{
				editItemIndex = value;
			}
		}
		
		public virtual TableItemStyle EditItemStyle
		{
			get
			{
				return editItemStyle;
			}
			set
			{
				editItemStyle = value;
			}
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
			}
		}

		public virtual bool ExtractTemplateRows
		{
			get
			{
				return extractTemplateRows;
			}
			set
			{
				extractTemplateRows = value;
			}
		}
		
		public virtual TableItemStyle FooterStyle
		{
			get
			{
				return footerStyle;
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
			}
		}
		
		public virtual bool ShowHeader
		{
			get
			{
				object o = ViewState["ShowHeader"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowHeader"] = value;
			}
		}
		
		public virtual bool ShowFooter
		{
			get
			{
				object o = ViewState["ShowFooter"];
				if(o!=null)
					return (bool)o;
				return true;
			}
			set
			{
				ViewState["ShowFooter"] = value;
			}
		}
		
		private DataListItem GetItem(ListItemType itemType, int repeatIndex)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Undocumented
		/// </summary>
		protected override void CreateControlHierarchy(bool create)
		{
			throw new NotImplementedException();
		}
		
		/// <summary>
		/// Undocumented
		/// </summary>
		protected override void PrepareControlHierarchy()
		{
			throw new NotImplementedException();
		}
		
		bool IRepeatInfoUser.HasFooter
		{
			get
			{
				return !(ShowFooter && footerTemplate!=null);
			}
		}
		
		bool IRepeatInfoUser.HasHeader
		{
			get
			{
				return !(ShowHeader && headerTemplate!=null);
			}
		}
		
		bool IRepeatInfoUser.HasSeparators
		{
			get
			{
				return (separatorTemplate!=null);
			}
		}
		
		int IRepeatInfoUser.RepeatedItemCount
		{
			get
			{
				if(itemsArray!=null)
					return itemsArray.Count;
				return 0;
			}
		}
		
		void IRepeatInfoUser.RenderItem(ListItemType itemType, int repeatIndex, RepeatInfo repeatInfo, HtmlTextWriter writer)
		{
			DataListItem item = GetItem(itemType, repeatIndex);
			if(item!=null)
			{
				item.RenderItem(writer, extractTemplateRows, repeatInfo.RepeatLayout == RepeatLayout.Table);
			}
		}
		
		Style IRepeatInfoUser.GetItemStyle(ListItemType itemType, int repeatIndex)
		{
			if(GetItem(itemType, repeatIndex)!=null && ControlStyleCreated)
				return ControlStyle;
		}
	}
}
