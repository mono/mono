//
// System.Web.UI.WebControls.BaseDataList.cs
//
// Authors:
//   Gaurav Vaish (gvaish@iitk.ac.in)
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) Gaurav Vaish (2001)
// (C) 2003 Andreas Nahr
// (C) 2004 Novell, Inc. (http://www.novell.com)
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
using System.ComponentModel.Design;
using System.Collections;
using System.Web;
using System.Web.UI;
using System.Web.Util;

namespace System.Web.UI.WebControls
{
	[DefaultEvent("SelectedIndexChanged")]
	[DefaultProperty("DataSource")]
	[Designer("System.Web.UI.Design.WebControls.BaseDataListDesigner, " + Consts.AssemblySystem_Design, typeof (IDesigner))]
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
			#if NET_2_0
			RequiresDataBinding = false;
			#endif
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
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
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
		[DefaultValue(null)]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		[WebCategory("Data")]
		[WebSysDescription("BaseDataList_DataSource")]
		public virtual object DataSource {
			get {
				return dataSource;
			}
			set {
				if (value == null || value is IListSource || value is IEnumerable) {
					dataSource = value;
				} else {
					throw new ArgumentException (HttpRuntime.FormatResourceString (
								"Invalid_DataSource_Type", ID));
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

		// LAMESPEC HorizontalAlign has a Category attribute, this should obviously be a WebCategory attribute
		// but is defined incorrectly in the MS framework

		[Bindable(true)]
		[DefaultValue(HorizontalAlign.NotSet)]
		[Category("Layout")]
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
				CreateControlHierarchy(false);
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
		
		#if NET_2_0

			
			// should be `internal protected' (why, oh WHY did they do that !?!)
			protected override void OnInit (EventArgs e)
			{
				base.OnInit(e);
				inited = true;
				if (!Page.IsPostBack)
					RequiresDataBinding = true;
			}
			
			// should be `internal protected' (why, oh WHY did they do that !?!)
			protected override void OnLoad (EventArgs e)
			{
				IDataSource ds = GetDataSourceObject () as IDataSource;
				if (ds != null && DataSourceID != "")
					ds.DataSourceChanged += new EventHandler (OnDataSourceChanged);
				
				base.OnLoad(e);
			}
			
			// should be `internal protected' (why, oh WHY did they do that !?!)
			protected override void OnPreRender (EventArgs e)
			{
				EnsureDataBound ();
				base.OnPreRender (e);
			}
				
			protected void EnsureDataBound ()
			{
				if (RequiresDataBinding && DataSourceID != "")
					DataBind ();
			}
			
			protected virtual object GetDataSourceObject ()
			{
				if (DataSourceID != "")
					return (IDataSource) NamingContainer.FindControl (DataSourceID);
				
				return DataSource;
			}
			
			protected virtual IEnumerable GetResolvedDataSource ()
			{
				if (DataSource != null && DataSourceID != "")
					throw new HttpException ();
				
				IDataSource ds = this.GetDataSourceObject () as IDataSource;				
				if (ds != null && DataSourceID != "") {					
					return ds.GetView (DataMember).ExecuteSelect (selectArguments); 
				}
				else if (DataSource != null)
					return DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);
				else
					return null; 
			}
			
			protected void OnDataSourceChanged (object sender, EventArgs e)
			{
				RequiresDataBinding = true;
			}
			
			public virtual string DataSourceID {
				get {
					object o = ViewState ["DataSourceID"];
					if (o != null)
						return (string)o;
					
					return String.Empty;
				}
				set {
					if (inited)
						RequiresDataBinding = true;
					
					ViewState ["DataSourceID"] = value;
				}
			}
			
			bool requiresDataBinding;
			protected bool RequiresDataBinding {
				get { return requiresDataBinding; }
				set { requiresDataBinding = value; }
			}
			
			protected bool inited;
			
			DataSourceSelectArguments selectArguments = null;
			
			protected DataSourceSelectArguments SelectArguments {
				get { return selectArguments; }
			}				
		#else
			internal IEnumerable GetResolvedDataSource ()
			{
				if (DataSource != null)
					return DataSourceHelper.GetResolvedDataSource (DataSource, DataMember);
				else
					return null; 
			}
		#endif
	}
}
