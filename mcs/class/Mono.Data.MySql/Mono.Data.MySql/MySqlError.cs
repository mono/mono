//
// Mono.Data.MySql.MySqlError
//
// Author:
//    Daniel Morgan <danmorg@sc.rr.com>
//
// (c)copyright 2002 Daniel Morgan
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
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.MySql
{
	public class MySqlError
	{
		private string message;
		private string source;
		private string sqlstate;
		private int nativeerror;

		#region Constructors

		internal MySqlError(string Source)
		{
			nativeerror = 1;
			source = Source;
			message = "Error in " + source;
			sqlstate = "";
		}

		#endregion // Constructors
		
		#region Properties

		public string Message
		{
			get
			{
				return message;
			}
		}

		public int NativeError
		{
			get
			{
				return nativeerror;
			}
		}

		public string Source
		{
			get
			{
				return source;
			}
		}

		public string SQLState
		{
			get
			{
				return sqlstate;
			}
		}

		#endregion // Properties

	}
}