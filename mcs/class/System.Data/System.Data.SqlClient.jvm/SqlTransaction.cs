//
// System.Data.SqlClient.SqlTransaction
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

namespace System.Data.SqlClient
{
    using System.Data;
	using System.Data.ProviderBase;
    /*
    * Current Limitations:
    * 1. Rollback(String savePoint) - not implemented.
    * 2. Save(String) - not implemented.
    */

    public sealed class SqlTransaction : AbstractTransaction
    {
    

        internal SqlTransaction(IsolationLevel isolationLevel, SqlConnection connection, String transactionName) : base(isolationLevel, connection, transactionName)
        {
        }

        
        internal SqlTransaction(SqlConnection connection) : base(IsolationLevel.ReadCommitted, connection, null)
        {
        }

        internal SqlTransaction(SqlConnection connection, String transactionName) : base(IsolationLevel.ReadCommitted, connection, transactionName)
        {
        }

        internal SqlTransaction(IsolationLevel isolationLevel, SqlConnection connection) : base(isolationLevel, connection, null)
        {
        }

        public new SqlConnection Connection
        {
            get
            {
                return (SqlConnection)_connection;
            }
        }

		[MonoNotSupported("")]
		public void Save (string savePointName) 
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public void Rollback (string transactionName) 
		{
			throw new NotImplementedException ();
		}
    }
}