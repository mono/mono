//
// System.Web.UI.Design.WebControls.ListControlDesigner.cs
//
// Author: Duncan Mak (duncan@novell.com)
//
// Copyright (C) 2005-2009 Novell, Inc (http://www.novell.com)
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
using System.ComponentModel.Design;
using System.Data;
using System.Web.UI.WebControls;
using System.Windows.Forms.Design;

namespace System.Web.UI.Design.WebControls {

	public class ListControlDesigner : DataBoundControlDesigner
	{
		string data_key_field;
		string data_text_field;
		string data_value_field;
		
		public ListControlDesigner ()
			: base ()
		{
		}

		public override DesignerActionListCollection ActionLists {
			get { throw new NotImplementedException (); }
		}

		protected override bool UseDataSourcePickerActionList {
			get { throw new NotImplementedException (); }
		}

		public string DataKeyField {
			get { return data_key_field; }
			set { data_key_field = value; }
		}

		public string DataTextField {
			get { return data_text_field; }
			set { data_text_field = value; }
		}

		public string DataValueField {
			get { return data_value_field; }
			set { data_value_field = value; }
		}

		protected override void DataBind (BaseDataBoundControl dataBoundControl)
		{
			throw new NotImplementedException ();
		}

		public override void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
		}
		
		public override string GetDesignTimeHtml ()
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

		public override void OnComponentChanged (object sender, ComponentChangedEventArgs e)
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