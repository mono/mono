//
// System.Web.UI.WebControls.FormViewUpdatedEventArgs.cs
//
// Authors:
//   Sanjay Gupta (gsanjay@novell.com)
//
// (C) 2004 Novell, Inc (http://www.novell.com)
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

using System.Collections.Specialized;

namespace System.Web.UI.WebControls
{
	public class FormViewUpdatedEventArgs : EventArgs
	{
		private int rowsAffected;
		private Exception e;
		private bool exceptionHandled;
		private bool keepEditMode;
		
		public DetailsViewUpdatedEventArgs (int affectedRows, Exception e)
		{
			this.rowsAffected = affectedRows;
			this.e = e;
			this.exceptionHandled = false;
			this.keepEditMode = false;
		}
		
		public int AffectedRows {
			get { return rowsAffected; }
		}

		public Exception Exception {
			get { return e };
		}

		public bool ExceptionHandled {
			get { return exceptionHandled; }
			set { exceptionHandled = value; }
		}
	
		public bool KeepInEdittMode {
			get { return keepEditMode; }
			set { keepEditMode = value; }
		}

		public IOrderedDictionary Keys {
			get { return new NotImplementedException(); }
		}

		public IOrderedDictionary NewValues {
			get { return new NotImplementedException(); }
		}

		public IOrderedDictionary OldValues {
			get { return new NotImplementedException(); }
		}
	}
}

#endif
