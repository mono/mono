//
// System.Data.OleDb.OleDbError
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


namespace System.Data.OleDb
{
	using java.sql;
	using System.Data.Common;
	using System.Data.ProviderBase;

	[Serializable]
    public sealed class OleDbError : AbstractDbError
    {

		internal OleDbError(SQLException e, AbstractDBConnection connection) : base(e, connection) {
		}
    
        public String Message
        {
            get
            {
                return DbMessage;
            }
        }
    
        public int NativeError
        {
            get
            {
                return DbErrorCode;
            }
        }
    
        public String Source
        {
            get
            {
                return DbSource;
            }
        }
    
        public String SQLState
        {
            get
            {
                return DbSQLState;
            }
        }

		public override string ToString () 
		{
			string toStr;

			toStr = String.Format("OleDbError: {0}. {1}", Message, _e.StackTrace);
			return toStr;

		}
    }
}