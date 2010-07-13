//
// System.Web.UI.WebControls.BaseDataBoundControl.cs
//
// Authors:
//	Lluis Sanchez Gual (lluis@novell.com)
//
// Copyright (C) 2004-2010 Novell, Inc (http://www.novell.com)
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

using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DefaultProperty ("DataSourceID")]
	[DesignerAttribute ("System.Web.UI.Design.WebControls.BaseDataBoundControlDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	public abstract class BaseDataBoundControl: WebControl
	{
		static readonly object dataBoundEvent = new object ();

		EventHandlerList events = new EventHandlerList ();
		object dataSource;
		bool initialized;
		bool preRendered;
		bool requiresDataBinding;
		
		public event EventHandler DataBound {
			add { events.AddHandler (dataBoundEvent, value); }
			remove { events.RemoveHandler (dataBoundEvent, value); }
		}
		
		protected BaseDataBoundControl ()
		{
		}

		/* Used for controls that used to inherit from
		 * WebControl, so the tag can propagate upwards
		 */
		internal BaseDataBoundControl (HtmlTextWriterTag tag) : base (tag)
		{
		}
		
		[BindableAttribute (true)]
		[ThemeableAttribute (false)]
		[DefaultValueAttribute (null)]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public virtual object DataSource {
			get { return dataSource; }
			set {
				if(value!=null)
					ValidateDataSource (value);
				dataSource = value;
				OnDataPropertyChanged ();
			}
		}
		
		[DefaultValueAttribute ("")]
		[ThemeableAttribute (false)]
		public virtual string DataSourceID {
			get { return ViewState.GetString ("DataSourceID", String.Empty); }
			set {
				ViewState["DataSourceID"] = value;
				OnDataPropertyChanged ();
			}
		}
		
		protected bool Initialized {
			get { return initialized; }
		}
		
		protected bool IsBoundUsingDataSourceID {
			get { return DataSourceID.Length > 0; }
		}
		
		protected bool RequiresDataBinding {
			get { return requiresDataBinding; }
			set {
				// MSDN: If you set the RequiresDataBinding
				// property to true when the data-bound control
				// has already begun to render its output to the
				// page, the current HTTP request is not a
				// callback, and you are using the DataSourceID
				// property to identify the data source control
				// to bind to, the DataBind method is called
				// immediately. In this case, the
				// RequiresDataBinding property is _not_ actually
				// set to true.
				//
				// LAMESPEC, the docs quoted above mention that
				// DataBind is called in the described
				// case. This is wrong since that way we don't
				// break recursion when the property is set from
				// within the OnSelect handler in user's
				// code. EnsureDataBound makes sure that no
				// recursive binding is performed. Also the
				// property DOES get set in this case (according
				// to tests)
				if (value && preRendered && IsBoundUsingDataSourceID && Page != null && !Page.IsCallback) {
					requiresDataBinding = true;
					EnsureDataBound ();
				} else
					requiresDataBinding = value;
			}
		}
#if NET_4_0
		public override bool SupportsDisabledAttribute {
			get { return RenderingCompatibilityLessThan40; }
		}
#endif
		protected void ConfirmInitState ()
		{
			initialized = true;
		}
		
		public override void DataBind ()
		{
			PerformSelect ();
		}

		protected virtual void EnsureDataBound ()
		{
			if (RequiresDataBinding && IsBoundUsingDataSourceID)
				DataBind ();
		}
		
		protected virtual void OnDataBound (EventArgs e)
		{
			EventHandler eh = events [dataBoundEvent] as EventHandler;
			if (eh != null)
				eh (this, e);
		}

		protected virtual void OnDataPropertyChanged ()
		{
			if (Initialized)
				RequiresDataBinding = true;
		}
		
		protected internal override void OnInit (EventArgs e)
		{
			base.OnInit (e);
			Page.PreLoad += new EventHandler (OnPagePreLoad);

			if (!IsViewStateEnabled && Page != null && Page.IsPostBack)
				RequiresDataBinding = true;
		}
		
		protected virtual void OnPagePreLoad (object sender, EventArgs e)
		{
			ConfirmInitState ();
		}
		
		protected internal override void OnPreRender (EventArgs e)
		{
			preRendered = true;
			EnsureDataBound ();
			base.OnPreRender (e);
		}

		internal Control FindDataSource ()
		{
			Control ctrl;
			Control namingContainer = NamingContainer;
			string dataSourceID = DataSourceID;
			
			while (namingContainer != null) {
				ctrl = namingContainer.FindControl (dataSourceID);
				if (ctrl != null)
					return ctrl;
				namingContainer = namingContainer.NamingContainer;
			}

			return null;
		}
		
		protected abstract void PerformSelect ();
		
		protected abstract void ValidateDataSource (object dataSource);
		
	}
}
