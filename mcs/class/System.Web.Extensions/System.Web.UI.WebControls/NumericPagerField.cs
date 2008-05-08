//
// System.Web.UI.WebControls.NumericPagerField
//
// Authors:
//   Marek Habersack (mhabersack@novell.com)
//
// (C) 2007 Novell, Inc
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
#if NET_3_5
using System;
using System.Security.Permissions;
using System.Web;
using System.Web.UI;

namespace System.Web.UI.WebControls
{
	[AspNetHostingPermissionAttribute(SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	[AspNetHostingPermissionAttribute(SecurityAction.InheritanceDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public class NumericPagerField : DataPagerField
	{
		public NumericPagerField ()
		{
		}

		protected override void CopyProperties (DataPagerField newField)
		{
		}

		public override void CreateDataPagers (DataPagerFieldItem container, int startRowIndex, int maximumRows,
						       int totalRowCount, int fieldIndex)
		{
		}

		protected override DataPagerField CreateField ()
		{
			throw new NotImplementedException ();
		}

		public override bool Equals (object o)
		{
			throw new NotImplementedException ();
		}

		public override int GetHashCode ()
		{
			throw new NotImplementedException ();
		}

		public override void HandleEvent (CommandEventArgs e)
		{
		}

		public int ButtonCount {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}

		public ButtonType ButtonType {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}

		public string CurrentPageLabelCssClass {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}

		public string NextPageImageUrl {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}

		public string NextPageText {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}

		public string NextPreviousButtonCssClass {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}

		public string NumericButtonCssClass {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}

		public string PreviousPageImageUrl {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}

		public string PreviousPageText {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}

		public bool RenderNonBreakingSpacesBetweenControls {
			get {
				throw new NotImplementedException ();
			}
			
			set {
			}
		}
	}
}
#endif
