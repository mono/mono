//
// System.Data.SqlClient.SqlInfoMessageEventArgs
//
// Authors:
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//	
// (C) 2005 Mainsoft Corporation (http://www.mainsoft.com)
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
using System.Data;

namespace System.Data.SqlClient
{
	/// <summary>
	/// Summary description for SqlInfoMessageEventArgs.
	/// </summary>
	public sealed class SqlInfoMessageEventArgs : EventArgs
	{
		#region Fields

		SqlErrorCollection errors ;

		#endregion // Fields

		#region Constructors
	
		internal SqlInfoMessageEventArgs (SqlErrorCollection errors)
		{
			this.errors = errors;
		}

		#endregion // Constructors

		#region Properties

		public SqlErrorCollection Errors 
		{
			get { return errors; }
		}	

		public string Message 
		{
			get { return errors[0].Message; }
		}	

		public string Source 
		{
			get { return errors[0].Source; }
		}

		#endregion // Properties

		#region Methods

		public override string ToString() 
		{
			return Message;
		}

		#endregion // Methods
	}
}
