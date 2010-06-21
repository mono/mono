//
// System.Web.UI.WebControls.AccessDataSource.cs
//
// Authors:
//   Sanjay Gupta (gsanjay@novell.com)
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

using System.IO;
using System.ComponentModel;
using System.Data.Common;
using System.Drawing;
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	// CAS
	[AspNetHostingPermissionAttribute (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[DesignerAttribute ("System.Web.UI.Design.WebControls.AccessDataSourceDesigner, " + Consts.AssemblySystem_Design, "System.ComponentModel.Design.IDesigner")]
	[ToolboxBitmap ("")]
	public class AccessDataSource : SqlDataSource
	{
		const string PROVIDER_NAME = "System.Data.OleDb";
		const string PROVIDER_STRING = "Microsoft.Jet.OLEDB.4.0";

		//string dataFile;
		string connectionString;

		public AccessDataSource () : base ()
		{
			base.ProviderName = PROVIDER_NAME;
		}

		public AccessDataSource (string dataFile, string selectCommand) : 
			base (String.Empty, selectCommand)
		{
			//this.dataFile = dataFile;
			this.ProviderName = PROVIDER_NAME;
		}

		protected override SqlDataSourceView CreateDataSourceView (string viewName)
		{
			AccessDataSourceView view = new AccessDataSourceView (this, viewName, this.Context);
			if (IsTrackingViewState)
				((IStateManager) view).TrackViewState ();				
			return view;
		}

		[DesignerSerializationVisibility (DesignerSerializationVisibility.Hidden)]
		[Browsable (false)]
		[MonoTODO("AccessDataSource does not support SQL Cache Dependencies")]
		public override string SqlCacheDependency {
			get { throw new NotSupportedException ("AccessDataSource does not supports SQL Cache Dependencies."); }
			set { throw new NotSupportedException ("AccessDataSource does not supports SQL Cache Dependencies."); }
		}

		[MonoTODO ("why override?  maybe it doesn't call DbProviderFactories.GetFactory?")]
		protected override DbProviderFactory GetDbProviderFactory ()
		{
			return DbProviderFactories.GetFactory (PROVIDER_NAME);
		}

		string GetPhysicalDataFilePath ()
		{
			if (String.IsNullOrEmpty (DataFile))
				return String.Empty;

			// more here?  how do we handle |DataDirectory|?
			return HttpContext.Current.Request.MapPath (DataFile);
		}

		[BrowsableAttribute (false), 
		DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public override string ConnectionString {
			get {
				if (connectionString == null) {
					connectionString = String.Concat ("Provider=", PROVIDER_STRING, "; Data Source=", GetPhysicalDataFilePath ());
				}

				return connectionString;
			}
			set {
				throw new InvalidOperationException 
					("The ConnectionString is automatically generated for AccessDataSource and hence cannot be set."); 
			}
		}

		[UrlPropertyAttribute]
		[DefaultValueAttribute ("")]
		[WebCategoryAttribute ("Data")]
		[WebSysDescriptionAttribute ("MS Office Access database file name")]
		[EditorAttribute ("System.Web.UI.Design.MdbDataFileEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
		public string DataFile {
			get { return ViewState.GetString ("DataFile", String.Empty); }
			set {
				ViewState ["DataFile"] = value;
				connectionString = null;
			}
		}

		[BrowsableAttribute (false), 
		DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]			
		public override string ProviderName {
			get { return base.ProviderName; }
			set { throw new InvalidOperationException
				("Setting ProviderName on an AccessDataSource is not allowed");
			}
		}
	}
}

