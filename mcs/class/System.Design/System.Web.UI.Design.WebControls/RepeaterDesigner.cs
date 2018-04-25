//
// System.Web.UI.Design.WebControls.RepeaterDesigner.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms.Design;
using System.Web.UI.Design;

namespace System.Web.UI.Design.WebControls {

	public class RepeaterDesigner : ControlDesigner, IDataSourceProvider
	{
		string data_member;
		string data_source;

		public RepeaterDesigner ()
			: base ()
		{
		}

		public string DataMember {
			get { return data_member; }
			set { data_member = value; }
		}

		public string DataSource {
			get { return data_source; }
			set { data_source = value; }
		}

		protected bool TemplatesExist {
			get { throw new NotImplementedException (); }
		}

		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		protected IEnumerable GetDesignTimeDataSource (int minimumRows)
		{
			throw new NotImplementedException ();
		}

		protected IEnumerable GetDesignTimeDataSource (
			IEnumerable selectedDataSource,
			int minimumRows)
		{
			throw new NotImplementedException ();
		}

		public override string GetDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}

		protected override string GetEmptyDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}
		
		protected override string GetErrorDesignTimeHtml (Exception e)
		{
			throw new NotImplementedException ();
		}

		public virtual IEnumerable GetResolvedSelectedDataSource ()
		{
			throw new NotImplementedException ();
		}
		
		public virtual object GetSelectedDataSource ()
		{
			throw new NotImplementedException ();
		}
		
		public override void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
		}

		public override void OnComponentChanged (object source, ComponentChangedEventArgs ce)
		{
			throw new NotImplementedException ();
		}

		protected internal virtual void OnDataSourceChanged ()
		{
			throw new NotImplementedException ();
		}

		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}
	}
}