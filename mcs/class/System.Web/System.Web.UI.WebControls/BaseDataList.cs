/**
 * Namespace: System.Web.UI.WebControls
 * Class:     BaseDataList
 *
 * Author:  Gaurav Vaish
 * Maintainer: gvaish@iitk.ac.in
 * Contact: <my_scripts2001@yahoo.com>, <gvaish@iitk.ac.in>
 * Implementation: yes
 * Status:  100%
 *
 * (C) Gaurav Vaish (2001)
 */

using System;
using System.ComponentModel;
using System.Collections;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("SelectedIndexChanged")]
	[DefaultProperty("DataSource")]
	[ParseChildren(true)]
	[PersistChildren(false)]
	//TODO: [Designer("??")]
	public abstract class BaseDataList: WebControl
	{
		private  static readonly object SelectedIndexChangedEvent = new object();
		internal static string          ItemCountViewStateKey     = "_!ItemCount";

		private DataKeyCollection dataKeys;
		private object            dataSource;

		public BaseDataList() : base()
		{
		}

		public static bool IsBindableType(Type type)
		{
			if(type.IsPrimitive || type == typeof(string) || type == typeof(DateTime) || type == typeof(Decimal))
				return true;
			return false;
		}

		public override ControlCollection Controls
		{
			get
			{
				EnsureChildControls();
				return base.Controls;
			}
		}

		public override void DataBind()
		{
			OnDataBinding(EventArgs.Empty);
		}

		[WebCategory("Action")]
		[WebSysDescription("BaseDataList_OnSelectedIndexChanged")]
		public event EventHandler SelectedIndexChanged
		{
			add
			{
				Events.AddHandler(SelectedIndexChangedEvent, value);
			}
			remove
			{
				Events.RemoveHandler(SelectedIndexChangedEvent, value);
			}
		}

		[Bindable(true)]
		[DefaultValue(-1)]
		[WebCategory("Layout")]
		[WebSysDescription("BaseDataList_CellPadding")]
		public virtual int CellPadding
		{
			get
			{
				if(!ControlStyleCreated)
					return -1;
				return ((TableStyle)ControlStyle).CellPadding;
			}
			set
			{
				((TableStyle)ControlStyle).CellPadding = value;
			}
		}

		[Bindable(true)]
		[DefaultValue(-1)]
		[WebCategory("Layout")]
		[WebSysDescription("BaseDataList_CellSpacing")]
		public virtual int CellSpacing
		{
			get
			{
				if(!ControlStyleCreated)
					return -1;
				return ((TableStyle)ControlStyle).CellSpacing;
			}
			set
			{
				((TableStyle)ControlStyle).CellSpacing = value;
			}
		}

		[DefaultValue("")]
		[WebCategory("Data")]
		[WebSysDescription("BaseDataList_DataKeyField")]
		public virtual string DataKeyField
		{
			get
			{
				object o = ViewState["DataKeyField"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataKeyField"] = value;
			}
		}

		[Browsable(true)]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebSysDescription("BaseDataList_DataKeys")]
		public DataKeyCollection DataKeys
		{
			get
			{
				if( dataKeys==null )
					dataKeys = new DataKeyCollection(DataKeysArray);
				return dataKeys;

			}
		}

		[DefaultValue("")]
		[WebCategory("Data")]
		[WebSysDescription("BaseDataList_DataMember")]
		public string DataMember
		{
			get
			{
				object o = ViewState["DataMember"];
				if(o!=null)
					return (string)o;
				return String.Empty;
			}
			set
			{
				ViewState["DataMember"] = value;
			}
		}

		[Bindable(true)]
		//[DefaultValue(null)]
		//[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebCategory("Data")]
		[WebSysDescription("BaseDataList_DataSource")]
		public virtual object DataSource
		{
			get
			{
				return dataSource;
			}
			set
			{
				if( (value!=null) && (value is IListSource || value is IEnumerable) )
				{
					dataSource = value;
				} else
				{
					throw new ArgumentException(HttpRuntime.FormatResourceString("Invalid_DataSource_Type", ID));
				}
			}
		}

		[Bindable(true)]
		[DefaultValue(GridLines.Both)]
		[WebCategory("Appearance")]
		[WebSysDescription("BaseDataList_GridLines")]
		public virtual GridLines GridLines
		{
			get
			{
				if(ControlStyleCreated)
					return ((TableStyle)ControlStyle).GridLines;
				return GridLines.Both;
			}
			set
			{
				((TableStyle)ControlStyle).GridLines = value;
			}
		}

		[Bindable(true)]
		[DefaultValue(HorizontalAlign.NotSet)]
		[WebCategory("Layout")]
		[WebSysDescription("BaseDataList_HorizontalAlign")]
		public virtual HorizontalAlign HorizontalAlign
		{
			get
			{
				if(ControlStyleCreated)
					return ((TableStyle)ControlStyle).HorizontalAlign;
				return HorizontalAlign.NotSet;
			}
			set
			{
				((TableStyle)ControlStyle).HorizontalAlign = value;
			}
		}

		protected ArrayList DataKeysArray
		{
			get
			{
				object o = ViewState["DataKeys"];
				if(o == null)
				{
					o = new ArrayList();
					ViewState["DataKeys"] = o;
				}
				return (ArrayList)o;
			}
		}

		protected override void AddParsedSubObject(object o)
		{
			// Preventing literal controls from being added as children.
		}

		protected override void CreateChildControls()
		{
			Controls.Clear();
			if(ViewState[ItemCountViewStateKey]!=null)
			{
				CreateControlHierarchy(true);
				ClearChildViewState();
			}
		}

		protected override void OnDataBinding(EventArgs e)
		{
			base.OnDataBinding(e);
			Controls.Clear();
			ClearChildViewState();
			CreateControlHierarchy(true);
			ChildControlsCreated = true;
			TrackViewState();
		}

		protected virtual void OnSelectedIndexChanged(EventArgs e)
		{
			if(Events != null)
			{
				EventHandler eh = (EventHandler)(Events[SelectedIndexChangedEvent]);
				if(eh!=null)
					eh(this, e);
			}
		}

		protected override void Render(HtmlTextWriter writer)
		{
			PrepareControlHierarchy();
			RenderContents(writer);
		}

		protected abstract void PrepareControlHierarchy();
		protected abstract void CreateControlHierarchy(bool useDataSource);
	}
}
