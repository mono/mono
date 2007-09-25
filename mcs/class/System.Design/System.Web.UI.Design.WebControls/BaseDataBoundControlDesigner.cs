//
// BaseDataBoundControlDesigner.cs
//
// Author:
//   Marek Habersack <mhabersack@novell.com>
//
// (C) 2007 Novell, Inc.
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
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Web.UI.WebControls;

namespace System.Web.UI.Design.WebControls
{
	public abstract class BaseDataBoundControlDesigner : ControlDesigner
	{
		[MonoNotSupported ("")]
		protected BaseDataBoundControlDesigner ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public string DataSource {
			[MonoNotSupported ("")]
			get {
				throw new NotImplementedException ();
			}

			[MonoNotSupported ("")]
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		public string DataSourceID {
			[MonoNotSupported ("")]
			get {
				throw new NotImplementedException ();
			}
			
			[MonoNotSupported ("")]
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoNotSupported ("")]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public override string GetDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public override void Initialize (IComponent component)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public static DialogResult ShowCreateDataSourceDialog (ControlDesigner controlDesigner, Type dataSourceType,
								       bool configure, out string dataSourceID)
		{
			throw new NotImplementedException ();
		}
		
		protected abstract bool ConnectToDataSource ();
		protected abstract void CreateDataSource ();
		protected abstract void DataBind (BaseDataBoundControl dataBoundControl);
		protected abstract void DisconnectFromDataSource ();

		[MonoNotSupported ("")]
		protected override string GetEmptyDesignTimeHtml ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		protected override string GetErrorDesignTimeHtml (Exception e)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		protected virtual void OnDataSourceChanged (bool forceUpdateView)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		protected virtual void OnSchemaRefreshed ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		protected override void PreFilterProperties (IDictionary properties)
		{
			throw new NotImplementedException ();
		}
	}
}
#endif