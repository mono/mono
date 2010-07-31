//
// System.Data.OleDb.OleDbCommand
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

namespace System.Data.OleDb
{
	public sealed class OleDbCommand : AbstractDbCommand
	{

		#region Fields

		internal static readonly int oracleTypeRefCursor = java.sql.Types.OTHER;
		private static readonly int _oracleRefCursor = -10; // oracle.jdbc.OracleTypes.CURSOR
		private int _currentParameterIndex = 0;
		private ResultSet _currentRefCursor;

		#endregion // Fields

		#region Constructors

		static OleDbCommand()
		{
			try {
				java.lang.Class OracleTypesClass = java.lang.Class.forName("oracle.jdbc.OracleTypes");
				_oracleRefCursor = OracleTypesClass.getField("CURSOR").getInt(null);
			}
			catch(java.lang.ClassNotFoundException e) {
				// oracle driver is not in classpath - just continue
			}
		}

		/**
		 * Initializes a new instance of the OleDbCommand class.
		 * The base constructor initializes all fields to their default values.
		 * The following table shows initial property values for an instance of SqlCommand.
		 */
		public OleDbCommand() : this(null, null, null)
		{
		}

		public OleDbCommand(OleDbConnection connection) : this(null, connection, null)
		{
		}

		/**
		 * Initializes a new instance of the OleDbCommand class with the text of the query.
		 * @param cmdText The text of the query.
		 */
		public OleDbCommand(String cmdText) : this(cmdText, null, null)
		{
		}

		/**
		 * Initializes a new instance of the OleDbCommand class with the text of the query and a SqlConnection.
		 * @param cmdText The text of the query.
		 * @param connection A SqlConnection that represents the connection to an instance of SQL Server.
		 */
		public OleDbCommand(String cmdText, OleDbConnection connection) : this(cmdText, connection, null)
		{
		}

		/**
		 * Initializes a new instance of the OleDbCommand class with the text of the query, a SqlConnection, and the Transaction.
		 * @param cmdText The text of the query.
		 * @param connection A SqlConnection that represents the connection to an instance of SQL Server.
		 * @param transaction The SqlTransaction in which the OleDbCommand executes.
		 */
		public OleDbCommand(
			String cmdText,
			OleDbConnection connection,
			OleDbTransaction transaction)
			: base(cmdText, connection, transaction)
		{
		}

		#endregion // Constructors

		#region Properties

		public new OleDbConnection Connection
		{
			get { return (OleDbConnection)base.Connection; }
			set { base.Connection = (AbstractDBConnection)value; }
		}

		public new OleDbParameterCollection Parameters
		{
			get { 
				return (OleDbParameterCollection)base.Parameters; 
			}
		}

		public new OleDbTransaction Transaction
		{
			get { return (OleDbTransaction)base.Transaction; }
			set { base.Transaction = (DbTransaction)value; }
		}

		protected internal sealed override ResultSet CurrentResultSet
		{
			get { 
				try {
					ResultSet resultSet = base.CurrentResultSet;
 
					if (resultSet != null) {
						return resultSet;						
					}
					return CurrentRefCursor;
				}
				catch(SQLException e) {
					throw CreateException(e);
				}
			}
		}

		private ResultSet CurrentRefCursor
		{
			get {
				if (_currentParameterIndex < 0) {
					NextRefCursor();
				}
				if (_currentRefCursor == null && _currentParameterIndex < InternalParameters.Count) {
					_currentRefCursor = (ResultSet)((CallableStatement)Statement).getObject(_currentParameterIndex + 1);
				}
				return _currentRefCursor;
			}
		}

		#endregion // Properties

		#region Methods

		public new OleDbDataReader ExecuteReader()
		{
			return (OleDbDataReader)ExecuteReader(CommandBehavior.Default);
		}

		public new OleDbDataReader ExecuteReader(CommandBehavior behavior)
		{
			return (OleDbDataReader)base.ExecuteReader(behavior);
		}

		public new OleDbParameter CreateParameter()
		{
			return (OleDbParameter)CreateParameterInternal();
		} 

		protected sealed override void CheckParameters()
		{
			for(int i = 0; i < Parameters.Count; i++) {
				OleDbParameter parameter = (OleDbParameter)Parameters[i];
				if ((parameter.OleDbType == OleDbType.Empty) || (parameter.OleDbType == OleDbType.Error)) {
					throw ExceptionHelper.ParametersNotInitialized(i,parameter.ParameterName,parameter.OleDbType.ToString());
				}

				if (((parameter.OleDbType == OleDbType.Char) || (parameter.OleDbType == OleDbType.Binary) ||
					(parameter.OleDbType == OleDbType.VarWChar) || (parameter.OleDbType == OleDbType.VarBinary) ||
					(parameter.OleDbType == OleDbType.VarNumeric)) && (parameter.Size == 0)) {
					throw ExceptionHelper.WrongParameterSize("OleDb");
				}
			}
		}

		protected sealed override DbParameter CreateParameterInternal()
		{
			return new OleDbParameter();
		}

		protected sealed override DbParameterCollection CreateParameterCollection(AbstractDbCommand parent)
		{
			return new OleDbParameterCollection((OleDbCommand)parent);
		}

		public override object Clone() {
			OleDbCommand clone = (OleDbCommand)base.Clone();
			clone._currentParameterIndex = 0;
			clone._currentRefCursor = null;
			return clone;
		}

		protected override void PrepareInternalParameters()
		{
			InternalParameters.Clear();
			_currentParameterIndex = -1;
		}

		protected override void BindOutputParameter(AbstractDbParameter parameter, int parameterIndex)
		{
			CallableStatement callableStatement = ((CallableStatement)Statement);
			if (((OleDbParameter)parameter).IsOracleRefCursor) {
				callableStatement.registerOutParameter(++parameterIndex, _oracleRefCursor);
			}
			else {
				base.BindOutputParameter(parameter, parameterIndex);
			}
		}

		protected override bool SkipParameter(DbParameter parameter)
		{
			return ((OleDbParameter)parameter).IsOracleRefCursor;
		}

		protected internal override bool NextResultSet()
		{
			try { 
				bool hasMoreResults = base.NextResultSet();

				if (hasMoreResults) {
					return true;
				}
				else {
					return NextRefCursor();
				}
			}
			catch (SQLException e) {
				throw CreateException(e);
			}
		}

		private bool NextRefCursor()
		{
			_currentRefCursor = null;
			// FIXME : should we count all parameters or only out ones?
			for (_currentParameterIndex++;InternalParameters.Count > _currentParameterIndex;_currentParameterIndex++) {
				if (((OleDbParameter)InternalParameters[_currentParameterIndex]).IsOracleRefCursor) {
					return true;						
				}
			}
			return false;
		}

		protected sealed override DbDataReader CreateReader()
		{
			return new OleDbDataReader(this);
		}

		protected internal sealed override SystemException CreateException(SQLException e)
		{
			return new OleDbException(e,Connection);		
		}

		#endregion // Methods
      
	}
}