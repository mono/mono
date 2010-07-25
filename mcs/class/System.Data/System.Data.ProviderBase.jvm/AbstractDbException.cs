//
// System.Data.ProviderBase.AbstractDbException
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


namespace System.Data.ProviderBase {

	using java.sql;
	using System.Text;
	using System.Data.Common;

	/*
	* CURRENT LIMITATIONS
	* 1. Constructor for serialization SqlException(SerializationInfo info, StreamingContext sc) 
	*    is not supported.
	* 2. Method "void GetObjectData(...,...)" is not supported (serialization)
	*/

	public abstract class AbstractDbException :
#if NET_2_0
		DbException
#else
		System.SystemException
#endif
	{
		protected SQLException _cause;
		protected AbstractDBConnection _connection;

		protected AbstractDbException(Exception cause, AbstractDBConnection connection) : base(cause.Message, cause) {
			_connection = connection;
		}

		protected AbstractDbException(SQLException cause, AbstractDBConnection connection) : this(String.Empty, cause, connection) {}

		protected AbstractDbException(string message, SQLException cause, AbstractDBConnection connection) : base(message, cause) {
			_connection = connection;
			_cause = cause;
		}

		abstract protected AbstractDbErrorCollection DbErrors { get; }

		/**
		 * Gets the error code of the error.
		 * @return The error code of the error.
		 */
		protected int DbErrorCode {
			get {
				return _cause != null ? _cause.getErrorCode() : 0;
			}
		}

 
		/**
		 * Gets the name of the OLE DB provider that generated the error.
		 * @return the name of the OLE DB provider that generated the error. 
		 */
		public override String Source {
			get {
				return _connection != null ? _connection.JdbcProvider : null;
			}
		}

		public override string Message {
			get {
				StringBuilder sb = new StringBuilder();
				string message = base.Message;
				bool addNewLine = false;
				if (message != null && message.Length > 0) {
					sb.Append(message);
					addNewLine = true;
				}

				foreach (AbstractDbError err in DbErrors) {
					if (addNewLine)
						sb.Append(Environment.NewLine);

					addNewLine = true;
					sb.Append(err.DbMessage);
				}
				return sb.ToString();
			}
		}

	}
}