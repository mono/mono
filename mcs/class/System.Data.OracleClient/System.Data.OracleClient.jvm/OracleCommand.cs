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
using System.Globalization;

using java.sql;
// Cannot use this because it makes ArrayList ambiguous reference
//using java.util;
#if !USE_DOTNET_REGEXP
using java.util.regex;
#endif

namespace System.Data.OracleClient {
	public sealed class OracleCommand : AbstractDbCommand {

		#region Fields
#if USE_DOTNET_REGEXP			
		internal static readonly Regex NamedParameterStoredProcedureRegExp = new Regex(@"^\s*{?\s*((?<RETVAL>\:\w+)\s*=\s*)?call\s+(?<PROCNAME>(((\[[^\]]*\])|([^\.\(])*)\s*\.\s*){0,2}(\[[^\]]*\]|((\s*[^\.\(\)\{\}\s])+)))\s*(\(\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)?\s*(,\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)\s*)*\))?\s*}?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
#else
		internal static readonly Pattern NamedParameterStoredProcedureRegExp = Pattern.compile(@"^\s*\{?\s*(?:(\:\w+)\s*=\s*)?call\s+((?:(?:(?:\[[^\]]*\])|(?:[^\.\(\)\{\}\[\]])*)\s*\.\s*){0,2}(?:\[[^\]]*\]|(?:(?:\s*[^\.\(\)\{\}\[\]])+)))\s*(?:\((.*)\))?\s*\}?\s*$", Pattern.CASE_INSENSITIVE);
#endif
		internal static readonly SimpleRegex NamedParameterRegExp = new OracleParamsRegex();

//		internal static readonly int oracleTypeRefCursor = java.sql.Types.OTHER;
		
		private int _currentParameterIndex = 0;
		private ResultSet _currentRefCursor;

		#endregion // Fields

		#region Constructors

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
				return (OracleParameterCollection)base.Parameters; 
			}
		}

		public new OracleTransaction Transaction {
			get { return (OracleTransaction)base.Transaction; }
			set { base.Transaction = (DbTransaction)value; }
		}

		protected override bool SkipParameter(DbParameter parameter) {
			return ((OracleParameter)parameter).OracleType == OracleType.Cursor;
		}

		protected sealed override ResultSet CurrentResultSet {
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

		private ResultSet CurrentRefCursor {
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

#if USE_DOTNET_REGEX
		protected override Regex StoredProcedureRegExp
#else
		protected override java.util.regex.Pattern StoredProcedureRegExp {
#endif
			get { return NamedParameterStoredProcedureRegExp; }
		}

		protected override SimpleRegex ParameterRegExp {
			get { return NamedParameterRegExp; }
		}

		#endregion // Properties

		#region Methods

		protected override bool NextResultSet() {
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

		private bool NextRefCursor() {
			_currentRefCursor = null;
			for (_currentParameterIndex++;InternalParameters.Count > _currentParameterIndex;_currentParameterIndex++) {
				OracleParameter param = (OracleParameter)InternalParameters[_currentParameterIndex];
				if (param.OracleType == OracleType.Cursor && ((param.Direction & ParameterDirection.Output) == ParameterDirection.Output))
					return true;						
			}
			return false;
		}

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

		protected override AbstractDbParameter GetUserParameter(string parameterName, IList userParametersList, int userParametersListPosition) {
			for(int i=0; i < userParametersList.Count; i++) {
				OracleParameter userParameter = (OracleParameter)userParametersList[i];
				if (String.Compare(parameterName, userParameter.InternalPlaceholder.Trim(), true, CultureInfo.InvariantCulture) == 0) {
					return userParameter;
				}
			}

			return null;
		}

		protected override AbstractDbParameter GetReturnParameter (IList userParametersList) {
			for(int i=0; i < userParametersList.Count; i++) {
				AbstractDbParameter userParameter = (AbstractDbParameter)userParametersList[i];
				if (userParameter.Direction == ParameterDirection.ReturnValue) {
					return userParameter;
				}
			}

			return null; 
		}

		protected sealed override DbParameter CreateParameterInternal() {
			return new OracleParameter();
		}

		protected sealed override DbParameterCollection CreateParameterCollection(AbstractDbCommand parent) {
			return new OracleParameterCollection((OracleCommand)parent);
		}

		public override object Clone() {
			OracleCommand clone = (OracleCommand)base.Clone();
			clone._currentParameterIndex = 0;
			clone._currentRefCursor = null;
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

		public object ExecuteOracleScalar() {
			throw new NotImplementedException();
		}

#if SUPPORT_ORACLE_TYPES
		public int ExecuteOracleNonQuery(
			out OracleString rowid
			) {
			throw new NotImplementedException();
		}
#endif

		#endregion // Methods
      
	}
}