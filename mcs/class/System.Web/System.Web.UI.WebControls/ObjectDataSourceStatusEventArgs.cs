//
// System.Web.UI.WebControls.ObjectDataSourceStatusEventArgs.cs
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


using System.Collections;

namespace System.Web.UI.WebControls
{
	public class ObjectDataSourceStatusEventArgs : EventArgs
	{
		readonly object returnVal;
		readonly IDictionary outPutParam;
		readonly Exception exception;
		bool exceptionHandled;
		int affectedRows;

		public ObjectDataSourceStatusEventArgs (object returnVal, IDictionary outPutParam)
			:
			this (returnVal, outPutParam, null) { }
		
		public ObjectDataSourceStatusEventArgs (object returnVal, IDictionary outPutParam, Exception e)
		{
			this.returnVal = returnVal;
			this.outPutParam = outPutParam;
			this.exception = e;
			this.exceptionHandled = false;
			this.affectedRows = -1;
		}

		public int AffectedRows {
			get { return affectedRows; }
			set { affectedRows = value; }
		}
		
		public Exception Exception {
			get { return exception; }
		}

		public bool ExceptionHandled {
			get { return exceptionHandled; }
			set { exceptionHandled = value; }
		}

		public IDictionary OutputParameters {
			get { return outPutParam; }
		}
		
		public object ReturnValue {
			get { return returnVal; }
		}
	}
}
