//
// Authors:
//    Marek Habersack <mhabersack@novell.com>
//
// Copyright (C) 2010 Novell, Inc (http://novell.com)
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
using System.ComponentModel.Design;
using System.Web.UI;
using System.Web.UI.Design;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls
{
	public class HierarchicalDataBoundControlDesigner : BaseDataBoundControlDesigner
	{
		public override DesignerActionListCollection ActionLists {
			get { throw new NotImplementedException (); }
		}

		public IHierarchicalDataSourceDesigner DataSourceDesigner {
			get { throw new NotImplementedException (); }
		}

		public DesignerHierarchicalDataSourceView DesignerView {
			get { throw new NotImplementedException (); }
		}

		protected virtual bool UseDataSourcePickerActionList {
			get { throw new NotImplementedException (); }
		}
		
		public HierarchicalDataBoundControlDesigner ()
		{
		}

		protected override bool ConnectToDataSource ()
		{
			throw new NotImplementedException ();
		}

		protected override void CreateDataSource ()
		{
			throw new NotImplementedException ();
		}

		protected override void DataBind (BaseDataBoundControl dataBoundControl)
		{
			throw new NotImplementedException ();
		}

		protected override void DisconnectFromDataSource ()
		{
			throw new NotImplementedException ();
		}

		protected virtual IHierarchicalEnumerable GetDesignTimeDataSource ()
		{
			throw new NotImplementedException ();
		}

		protected virtual IHierarchicalEnumerable GetSampleDataSource ()
		{
			throw new NotImplementedException ();
		}

		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}

		
	}
}
