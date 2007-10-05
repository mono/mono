//
// System.Web.UI.Design.HierarchicalDataSourceDesigner
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2007 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Globalization;
using System.Security.Permissions;

namespace System.Web.UI.Design {

	[SecurityPermission (SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public class HierarchicalDataSourceDesigner : ControlDesigner, IHierarchicalDataSourceDesigner
	{
		public HierarchicalDataSourceDesigner ()
		{
		}

		public event EventHandler DataSourceChanged;

		public event EventHandler SchemaRefreshed;

		[MonoTODO]
		public override DesignerActionListCollection ActionLists {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual bool CanConfigure {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public virtual bool CanRefreshSchema {
			get { throw new NotImplementedException (); }
		}

		[MonoTODO]
		protected bool SuppressingDataSourceEvents {
			get { throw new NotImplementedException (); }
		}

		public virtual void Configure ()
		{
			throw new NotSupportedException ();
		}

		[MonoTODO]
		public override string GetDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual DesignerHierarchicalDataSourceView GetView (string viewPath)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnDataSourceChanged (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected virtual void OnSchemaRefreshed (EventArgs e)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void RefreshSchema (bool preferSilent)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void ResumeDataSourceEvents ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public virtual void SuppressDataSourceEvents ()
		{
			throw new NotImplementedException ();
		}
	}

}

#endif
