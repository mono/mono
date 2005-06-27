
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

namespace IBM.Data.DB2
{

	public class DB2ClientUtils
	{

		#region DB2CheckReturn

		public static void DB2CheckReturn(short sqlRet, short handleType, IntPtr handle, string message, DB2Connection connection)
		{
			switch (sqlRet) 
			{
				case DB2Constants.SQL_SUCCESS_WITH_INFO:
					if(connection != null)
					{
						connection.OnInfoMessage(handleType, handle);
					}
					goto case DB2Constants.SQL_SUCCESS;
				case DB2Constants.SQL_SUCCESS:
				case DB2Constants.SQL_NO_DATA:
					return;

				case DB2Constants.SQL_INVALID_HANDLE:
					throw new ArgumentException("Invalid handle");

				default:
				case DB2Constants.SQL_ERROR:
					throw new DB2Exception(handleType, handle, message);
			}
		}
		public static void DB2CheckReturn(short sqlRet, short handleType, IntPtr handle, string message)
		{
			DB2CheckReturn(sqlRet, handleType, handle, message, null);
		}
		#endregion
	}
}
