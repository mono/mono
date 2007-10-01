//
// System.ComponentModel.Design.Data.DataSourceProviderService
//
// Author:
//	Atsushi Enomoto (atsushi@ximian.com)
//
// Copyright (C) 2007 Novell, Inc.
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

using System.ComponentModel;
using System.Windows.Forms;

namespace System.ComponentModel.Design.Data
{
	[System.Runtime.InteropServices.Guid ("ABE5C1F0-C96E-40c4-A22D-4A5CEC899BDC")]
	public abstract class DataSourceProviderService
	{
		protected DataSourceProviderService ()
		{
		}

		public abstract bool SupportsAddNewDataSource { get; }

		public abstract bool SupportsConfigureDataSource { get; }

		public abstract object AddDataSourceInstance (IDesignerHost host, DataSourceDescriptor dataSourceDescriptor);

		public abstract DataSourceGroupCollection GetDataSources ();

		public abstract DataSourceGroup InvokeAddNewDataSource (IWin32Window parentWindow, FormStartPosition startPosition);

		public abstract bool InvokeConfigureDataSource (IWin32Window parentWindow, FormStartPosition startPosition, DataSourceDescriptor dataSourceDescriptor);

		public abstract void NotifyDataSourceComponentAdded (object dsc);

	}
}

#endif
