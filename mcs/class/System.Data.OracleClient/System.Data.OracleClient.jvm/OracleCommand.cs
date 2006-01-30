//
// System.Data.OracleClient.OracleCommand
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
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;

using java.sql;
// Cannot use this because it makes ArrayList ambiguous reference
//using java.util;

namespace System.Data.OracleClient {
	public sealed class OracleCommand : AbstractDbCommand, IDbCommand, ICloneable {

		#region Fields

		internal static readonly int oracleTypeRefCursor = java.sql.Types.OTHER;
		private static readonly int _oracleRefCursor = -10; // oracle.jdbc.OracleTypes.CURSOR
		private int _currentParameterIndex = 0;
		private ResultSet _currentRefCursor;

		#endregion // Fields

		#region Constructors

		static OracleCommand() {
			try {
				java.lang.Class OracleTypesClass = java.lang.Class.forName("oracle.jdbc.OracleTypes");
				_oracleRefCursor = OracleTypesClass.getField("CURSOR").getInt(null);
			}
			catch(java.lang.ClassNotFoundException e) {
				// oracle driver is not in classpath - just continue
			}
		}

		/**
		 * Initializes a new instance of the OracleCommand class.
		 * The base constructor initializes all fields to their default values.
		 * The following table shows initial property values for an instance of SqlCommand.
		 */
		public OracleCommand() : this(null, null, null) {
		}

		public OracleCommand(OracleConnection connection) : this(null, connection, null) {
		}

		/**
		 * Initializes a new instance of the OracleCommand class with the text of the query.
		 * @param cmdText The text of the query.
		 */
		public OracleCommand(String cmdText) : this(cmdText, null, null) {
		}

		/**
		 * Initializes a new instance of the OracleCommand class with the text of the query and a SqlConnection.
		 * @param cmdText The text of the query.
		 * @param connection A SqlConnection that represents the connection to an instance of SQL Server.
		 */
		public OracleCommand(String cmdText, OracleConnection connection) : this(cmdText, connection, null) {
		}

		/**
		 * Initializes a new instance of the OracleCommand class with the text of the query, a SqlConnection, and the Transaction.
		 * @param cmdText The text of the query.
		 * @param connection A SqlConnection that represents the connection to an instance of SQL Server.
		 * @param transaction The SqlTransaction in which the OracleCommand executes.
		 */
		public OracleCommand(
			String cmdText,
			OracleConnection connection,
			OracleTransaction transaction)
			: base(cmdText, connection, transaction) {
		}

		#endregion // Constructors

		#region Properties

		public new OracleConnection Connection {
			get { return (OracleConnection)base.Connection; }
			set { base.Connection = (AbstractDBConnection)value; }
		}

		public new OracleParameterCollection Parameters {
			get { 
				if (_parameters == null) {
					_parameters = CreateParameterCollection(this);
				}
				return (OracleParameterCollection)_parameters; 
			}
		}

		public new OracleTransaction Transaction {
			get { return (OracleTransaction)base.Transaction; }
			set { base.Transaction = (DbTransaction)value; }
		}

		#endregion // Properties

		#region Methods

		public new OracleDataReader ExecuteReader() {
			return (OracleDataReader)ExecuteReader(CommandBehavior.Default);
		}

		public new OracleDataReader ExecuteReader(CommandBehavior behavior) {
			return (OracleDataReader)base.ExecuteReader(behavior);
		}

		public new OracleParameter CreateParameter() {
			return (OracleParameter)CreateParameterInternal();
		} 

		protected sealed override void CheckParameters() {
			//TBD
		}

		protected sealed override DbParameter CreateParameterInternal() {
			return new OracleParameter();
		}

		protected sealed override DbParameterCollection CreateParameterCollection(AbstractDbCommand parent) {
			return new OracleParameterCollection((OracleCommand)parent);
		}

		public object Clone() {
			OracleCommand clone = new OracleCommand();
			CopyTo(clone);
			return clone;
		}

		protected override void PrepareInternalParameters() {
			InternalParameters.Clear();
			_currentParameterIndex = -1;
		}

		
		protected sealed override DbDataReader CreateReader() {
			return new OracleDataReader(this);
		}

		protected sealed override SystemException CreateException(SQLException e) {
			return new OracleException(e,Connection);		
		}

		#endregion // Methods
      
	}
}