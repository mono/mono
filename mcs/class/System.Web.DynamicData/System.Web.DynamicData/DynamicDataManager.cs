//
// DynamicDataManager.cs
//
// Author:
//	Atsushi Enomoto <atsushi@ximian.com>
//      Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2008-2009 Novell Inc. http://novell.com
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Security.Principal;
using System.Web.Caching;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.DynamicData.ModelProviders;

namespace System.Web.DynamicData
{
	[ToolboxBitmap (typeof(DynamicDataManager), "DynamicDataManager.ico")]
	[AspNetHostingPermission (SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[NonVisualControl]
	[ParseChildren (true)]
	[PersistChildren (false)]
	public class DynamicDataManager : Control
	{
		private class AutoFieldGenerator : IAutoFieldGenerator
		{
			MetaTable table;
			
			public AutoFieldGenerator (MetaTable table)
			{
				this.table = table;
			}
			
			public ICollection GenerateFields (Control ctl)
			{
				var ret = new List <DynamicField> ();
				foreach (MetaColumn column in table.Columns) {
					if (!column.Scaffold)
						continue;
					
					var field = new DynamicField ();
					field.DataField = column.Name;
					ret.Add (field);
				}
				
				return ret;
			}
		}

		Dictionary <IDynamicDataSource, bool> knownDataSources;
		
		public DynamicDataManager ()
		{
		}

		public bool AutoLoadForeignKeys {
			get;
			set;
		}

		[Browsable (false)]
		public override bool Visible {
			get { return true; }

			// NOTE: it is supposed to throw the exception
			set { throw new NotImplementedException (); }
		}

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);

			// http://forums.asp.net/p/1257004/2339034.aspx
			// http://forums.asp.net/t/1297860.aspx
			// http://forums.asp.net/p/1396453/3005197.aspx#3005197
			if (knownDataSources != null) {
				foreach (var de in knownDataSources) {
					IDynamicDataSource dds = de.Key;
					if (dds == null)
						continue;

					dds.ExpandDynamicWhereParameters ();
				}
			}
		}

		public void RegisterControl (Control control)
		{
			RegisterControl (control, false);
		}

		public void RegisterControl (Control control, bool setSelectionFromUrl)
		{
			// .NET doesn't check for null here, but since I don't like such code, we
			// will do the check and throw the same exception as .NET
			if (control == null)
				throw new NullReferenceException ();

			if (!ControlIsValid (control))
				throw new Exception ("Controls of type " + control.GetType () + " are not supported.");

			// http://forums.asp.net/p/1257004/2339034.aspx
			// http://forums.asp.net/p/1383908/2936065.aspx
			DataBoundControl dbc = control as DataBoundControl;
			if (dbc != null) {
				IDynamicDataSource dds = dbc.DataSourceObject as IDynamicDataSource;
				if (dds == null)
					return;

				RegisterDataSource (dds);
				MetaTable table = dds.GetTable ();
				if (table == null)
					return;

				if (String.IsNullOrEmpty (dds.Where))
					dds.AutoGenerateWhereClause = true;
				else
					dds.AutoGenerateWhereClause = false;

				Type contextType = dds.ContextType;
				if (contextType == null)
					dds.ContextType = table.DataContextType;

				string entityName = dds.EntitySetName;
				if (String.IsNullOrEmpty (entityName))
					dds.EntitySetName = table.DataContextPropertyName;

				if (AutoLoadForeignKeys) {
					var ldds = dds as LinqDataSource;
					if (ldds != null)
						ldds.LoadWithForeignKeys (table.EntityType);
				}
				
				var gv = control as GridView;
				if (gv != null) {
					gv.ColumnsGenerator = new AutoFieldGenerator (table);
					return;
				}

				var dv = control as DetailsView;
				if (dv != null) {
					dv.RowsGenerator = new AutoFieldGenerator (table);
					return;
				}
			}
		}

		void RegisterDataSource (IDynamicDataSource dds)
		{
			if (knownDataSources == null) {
				knownDataSources = new Dictionary <IDynamicDataSource, bool> ();
				knownDataSources.Add (dds, true);
				return;
			}
			
			if (knownDataSources.ContainsKey (dds))
				return;

			knownDataSources.Add (dds, true);
		}
		
		bool ControlIsValid (Control control)
		{
			if (control is Repeater) {
				if (control.NamingContainer == null)
					throw new HttpException ("The Repeater control '" + control.ID + "' does not have a naming container.");
				return true;
			}
			
			DataBoundControl dbc = control as DataBoundControl;
			if (dbc == null)
				return false;
			
			return true;
		}
	}
}
