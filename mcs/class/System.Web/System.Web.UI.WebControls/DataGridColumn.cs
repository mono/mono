/**
 * Namespace: System.Web.UI.WebControls
 * Class:     DataGridColumn
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
	[TypeConverter(typeof(ExpandableObjectConverter))]
	public abstract class DataGridColumn : IStateManager
	{
		private StateBag viewState;
		private bool     marked;
		private TableItemStyle footerStyle;
		private TableItemStyle headerStyle;
		private TableItemStyle itemStyle;

		private DataGrid owner;
		private bool     designMode;

		public DataGridColumn()
		{
			viewState = new StateBag();
		}

		internal TableItemStyle FooterStyleInternal
		{
			get
			{
				return footerStyle;
			}
		}

		internal TableItemStyle HeaderStyleInternal
		{
			get
			{
				return headerStyle;
			}
		}

		internal TableItemStyle ItemStyleInternal
		{
			get
			{
				return itemStyle;
			}
		}

		public virtual TableItemStyle FooterStyle
		{
			get
			{
				if(footerStyle == null)
				{
					footerStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						footerStyle.TrackViewState();
					}
				}
				return footerStyle;
			}
		}

		public virtual TableItemStyle HeaderStyle
		{
			get
			{
				if(headerStyle == null)
				{
					headerStyle= new TableItemStyle();
					if(IsTrackingViewState)
					{
						headerStyle.TrackViewState();
					}
				}
				return headerStyle;
			}
		}

		public virtual TableItemStyle ItemStyle
		{
			get
			{
				if(itemStyle == null)
				{
					itemStyle = new TableItemStyle();
					if(IsTrackingViewState)
					{
						itemStyle.TrackViewState();
					}
				}
				return itemStyle;
			}
		}

		public virtual string FooterText
		{
			get
			{
				object o = ViewState["FooterText"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["FooterText"] = value;
				OnColumnChanged();
			}
		}

		public virtual string HeaderImageUrl
		{
			get
			{
				object o = ViewState["HeaderImageUrl"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["HeaderImageUrl"] = value;
				OnColumnChanged();
			}
		}

		public virtual string HeaderText
		{
			get
			{
				object o = ViewState["HeaderText"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["HeaderText"] = value;
				OnColumnChanged();
			}
		}

		public virtual string SortExpression
		{
			get
			{
				object o = ViewState["SortExpression"];
				if(o != null)
				{
					return (string)o;
				}
				return String.Empty;
			}
			set
			{
				ViewState["SortExpression"] = value;
				OnColumnChanged();
			}
		}

		public bool Visible
		{
			get
			{
				object o = ViewState["Visible"];
				if(o != null)
				{
					return (bool)o;
				}
				return true;
			}
			set
			{
				ViewState["Visible"] = value;
				OnColumnChanged();
			}
		}

		public virtual void Initialize()
		{
			if(owner != null && owner.Site != null)
			{
				designMode = owner.Site.DesignMode;
			}
		}

		public virtual void InitializeCell(TableCell cell, int columnIndex, ListItemType itemType)
		{
			switch(itemType)
			{
				case ListItemType.Header : InitializeCellHeader(cell, columnIndex);
				                           break;
				case ListItemType.Footer : InitializeCellFooter(cell, columnIndex);
				                           break;
				default                  : return;
			}
		}

		private void InitializeCellHeader(TableCell cell, int columnIndex)
		{
			WebControl ctrl = null;
			bool       sort = true;
			string     sortExpr = "";
			ImageButton headButton;
			Image       headImage;
			LinkButtonInternal link;

			if(owner != null)
			{
				sort = owner.AllowSorting;
			}
			if(sort)
			{
				sortExpr = SortExpression;
				if(sortExpr.Length == 0)
				{
					sort = false;
				}
			}
			if(HeaderImageUrl.Length > 0)
			{
				if(sort)
				{
					headButton = new ImageButton();
					headButton.ImageUrl = HeaderImageUrl;
					headButton.CommandName = "Sort";
					headButton.CommandArgument = sortExpr;
					headButton.CausesValidation = false;
					ctrl = headButton;
				} else
				{
					headImage = new Image();
					headImage.ImageUrl = HeaderImageUrl;
					ctrl = headImage;
				}
			} else
			{
				if(sort)
				{
					link = new LinkButtonInternal();
					link.Text = HeaderText;
					link.CommandName = "Sort";
					link.CommandArgument = sortExpr;
					link.CausesValidation = false;
					ctrl = link;
				} else
				{
					if(HeaderText.Length > 0)
					{
						cell.Text = HeaderText;
					} else
					{
						cell.Text = "&nbsp;";
					}
				}
			}
			if(ctrl != null)
			{
				cell.Controls.Add(ctrl);
			}
		}

		private void InitializeCellFooter(TableCell cell, int columnIndex)
		{
			cell.Text = (FooterText.Length > 0 ? FooterText : "&nbsp;");
		}

		public override string ToString()
		{
			return String.Empty;
		}

		protected bool DesignMode
		{
			get
			{
				return designMode;
			}
		}

		protected DataGrid Owner
		{
			get
			{
				return owner;
			}
		}

		protected StateBag ViewState
		{
			get
			{
				return viewState;
			}
		}

		/// <summary>
		/// Undocumented
		/// </summary>
		protected virtual void OnColumnChanged()
		{
			if(owner != null)
			{
				owner.OnColumnsChanged();
			}
		}

		internal void SetOwner (DataGrid datagrid)
		{
			owner = datagrid;
		}
		
		protected virtual object SaveViewState()
		{
			object[] states = new object[4];
			states[0] = ViewState.SaveViewState();
			states[1] = (footerStyle == null ? null : footerStyle.SaveViewState());
			states[2] = (headerStyle == null ? null : headerStyle.SaveViewState());
			states[3] = (itemStyle == null ? null : itemStyle.SaveViewState());
			return states;
		}

		protected virtual void LoadViewState(object savedState)
		{
			if(savedState!= null)
			{
				object[] states = (object[]) savedState;
				if(states != null)
				{
					ViewState.LoadViewState(states[0]);
					FooterStyle.LoadViewState(states[1]);
					HeaderStyle.LoadViewState(states[2]);
					ItemStyle.LoadViewState(states[3]);
				}
			}
		}

		protected virtual void TrackViewState()
		{
			marked = true;
			ViewState.TrackViewState();
			if(footerStyle != null)
			{
				footerStyle.TrackViewState();
			}
			if(headerStyle != null)
			{
				headerStyle.TrackViewState();
			}
			if(itemStyle != null)
			{
				itemStyle.TrackViewState();
			}
		}

		protected bool IsTrackingViewState
		{
			get
			{
				return marked;
			}
		}

		void IStateManager.LoadViewState(object savedState)
		{
			LoadViewState(savedState);
		}

		object IStateManager.SaveViewState()
		{
			return SaveViewState();
		}

		void IStateManager.TrackViewState()
		{
			TrackViewState();
		}

		bool IStateManager.IsTrackingViewState
		{
			get
			{
				return IsTrackingViewState;
			}
		}
	}
}
