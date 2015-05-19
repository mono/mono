//
// System.Web.UI.WebControls.DataPager
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2008 Novell, Inc
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
using System.Security.Permissions;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class DataPagerCommandEventArgs : CommandEventArgs
	{
		public DataPagerFieldItem Item {
			get;
			private set;
		}

		public int NewMaximumRows {
			get;
			set;
		}

		public int NewStartRowIndex {
			get;
			set;
		}

		public DataPagerField PagerField {
			get;
			private set;
		}

		public int TotalRowCount {
			get;
			private set;
		}
		
		public DataPagerCommandEventArgs (DataPagerField pagerField, int totalRowCount, CommandEventArgs originalArgs, DataPagerFieldItem item)
			: base (originalArgs)
		{
			Item = item;
			NewMaximumRows = -1;
			NewStartRowIndex = -1;
			PagerField = pagerField;
			TotalRowCount = totalRowCount;
		}
	}
}
