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
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	public class DataList: BaseDataList, INamingContainer, IRepeatInfoUser
	{
		//
		public const string CancelCommandName = "Cancel";
		public const string DeleteCommandName = "Delete";
		public const string EditCommandName = "Edit";
		public const string SelectCommandName = "Select";
		public const string UpdateCommandName = "Update";

		//TODO: From where will I update the values of the following ItemStyles?
		private TableItemStyle alternatingItemStyle;
		private TableItemStyle editItemStyle;
		private TableItemStyle footerStyle;

		private ITemplate alternatingItemTemplate;
		private ITemplate editItemTemplate;
		private ITemplate footerTemplate;

		private int editItemIndex;
		private bool extractTemplateRows;
		
		public DataList()
		{
			alternatingItemStyle = new TableItemStyle();
			editItemStyle        = new TableItemStyle();
			footerStyle          = new TableItemStyle();

			alternatingItemTemplate = null;
			editItemTemplate        = null;
			footerTemplate          = null;
			
			extractTemplateRows = false;
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
		
		//TODO: To implement the following functions found in the BaseDataList abstract class
		/*
		 * PrepareControlHierarchy()
		 * CreateControlHeirarchy(bool)
		 */
		 
		public void CreateControlHierarchy(bool create)
		{
			throw new NotImplementedException();
			//TODO: THE LOST WORLD
			// Put here to get compilation going
		}
		
		//Impemented methods/properties of IRepeatInfoUser
		//TODO: Check all these implementations are valid or a total absurd
		bool IRepeatInfoUser.HasFooter
		{
			get
			{
				if(footerTemplate!=null)
					return true;
				return false;
			}
		}
	}
}
