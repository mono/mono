//
// System.Data.SqlClient.SqlError
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
	using System.Data.ProviderBase;
	using java.sql;
	using System.Data.Common;

    /**
     * Collects information relevant to a warning or error returned by SQL Server.
     */

	[Serializable]
    public class SqlError : AbstractDbError
    {
		string _serverVersion;
        /**
         * Initialize SqlError object
         * */
        internal SqlError(SQLException e, AbstractDBConnection connection) : base(e, connection)
        {
			if (connection != null)
				_serverVersion = connection.ServerVersion;
        }

        /**
         * Overridden. Gets the complete text of the error message.
         *
         * @return A string representation of the current object.
         */
        public override String ToString()
        {
            return String.Concat("SqlError:", Message, _e.StackTrace);
        }

        /**
         * Gets the name of the provider that generated the error.
         *
         * @return The name of the provider
         */
        public String Source
        {
            get
            {
                return DbSource;
            }
        }

        /**
         * Gets a number that identifies the type of error.
         *
         * @return Number of the error
         */
        public int Number
        {
            get
            {
                return DbErrorCode;
            }
        }

        /**
         * Gets a numeric error code from SQL Server that represents an error,
         * warning or "no data found" message. For more information on how to
         * decode these values, see SQL Server Books Online.
         *
         * @return Error Code
         */
        public byte State
        {
            get
            {
                return 0; // & BitConstants.ALL_BYTE;
            }
        }

        /**
         * Gets the severity level of the error returned from SQL Server.
         *
         * @return Severity level of the error
         */
        public byte Class
        {
            get
            {
                return 0; // & BitConstants.ALL_BYTE;
            }
        }

        /**
         * Gets the name of the instance of SQL Server that generated the error.
         *
         * @return The name of the server
         */
        public String Server
        {
            get
            {
                return _serverVersion;
            }
        }

        /**
         * Gets the text describing the error.
         *
         * @return The text describing the error
         */
        public String Message
        {
            get
            {
                return DbMessage;
            }
        }

        /**
         * Gets the name of the stored procedure or remote procedure call (RPC)
         * that generated the error.
         *
         * @return The name of stored procedure that generated the error.
         */
        public String Procedure
        {
            get
            {
                return null;
            }
        }

        /**
         * Bets the line number within the Transact-SQL command batch or stored
         * procedure that contains the error.
         *
         * @return Line number of error in stored procedure
         */
        public int LineNumber
        {
            get
            {
                return 0;
            }
        }
    }
}