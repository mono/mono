//
// System.Data.ProviderBase.AbstractDbCommand
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
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Data;
using System.Data.Common;

using java.sql;
using java.io;

#if !USE_DOTNET_REGEXP
using java.util.regex;
#endif

namespace System.Data.ProviderBase
{
	public abstract class AbstractDbCommand : DbCommand, ICloneable
	{
		#region ProcedureColumnCache

		internal sealed class ProcedureColumnCache : AbstractDbMetaDataCache
		{
			internal ArrayList GetProcedureColumns(AbstractDBConnection connection, String commandText,AbstractDbCommand command) 
			{
				string connectionCatalog = connection.JdbcConnection.getCatalog();
				string key = String.Concat(connection.ConnectionString, connectionCatalog, commandText);
				System.Collections.Hashtable cache = Cache;

				ArrayList col = cache[key] as ArrayList;

				if (null != col) {
					return col;
				}
	
				col = connection.GetProcedureColumns(commandText,command);
				if (col != null)
					cache[key] = col;
				return col;				
			}
		}

		#endregion

		#region SqlStatementsHelper

		internal sealed class SqlStatementsHelper
		{
			#region Fields
#if USE_DOTNET_REGEXP			
			internal static readonly Regex NamedParameterStoredProcedureRegExp = new Regex(@"^\s*{?\s*((?<RETVAL>@\w+)\s*=\s*)?call\s+(?<PROCNAME>(((\[[^\]]*\])|([^\.\(])*)\s*\.\s*){0,2}(\[[^\]]*\]|((\s*[^\.\(\)\{\}\s])+)))\s*(\(\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)?\s*(,\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)\s*)*\))?\s*}?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			internal static readonly Regex SimpleParameterStoredProcedureRegExp = new Regex(@"^\s*{?\s*((?<RETVAL>\?)\s*=\s*)?call\s+(?<PROCNAME>(((\[[^\]]*\])|([^\.\(])*)\s*\.\s*){0,2}(\[[^\]]*\]|((\s*[^\.\(\)\{\}\s])+)))\s*(\(\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)?\s*(,\s*(?<USERPARAM>((""([^""]|(""""))*"")|('([^']|(''))*')|[^,])*)\s*)*\))?\s*}?\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
			internal static readonly Regex ForBrowseStatementReqExp = new Regex(@"\s+FOR\s+BROWSE\s*$", RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture);
#else
			internal static readonly Pattern NamedParameterStoredProcedureRegExp = Pattern.compile(@"^\s*\{?\s*(?:(@\w+)\s*=\s*)?call\s+((?:(?:(?:\[[^\]]*\])|(?:[^\.\(\)\{\}\[\]])*)\s*\.\s*){0,2}(?:\[[^\]]*\]|(?:(?:\s*[^\.\(\)\{\}\[\]])+)))\s*(?:\((.*)\))?\s*\}?\s*$", Pattern.CASE_INSENSITIVE);
			internal static readonly Pattern SimpleParameterStoredProcedureRegExp = Pattern.compile(@"^\s*\{?\s*(?:(\?)\s*=\s*)?call\s+((?:(?:(?:\[[^\]]*\])|(?:[^\.\(\)\{\}\[\]])*)\s*\.\s*){0,2}(?:\[[^\]]*\]|(?:(?:\s*[^\.\(\)\{\}\[\]])+)))\s*(?:\((.*)\))?\s*\}?\s*$", Pattern.CASE_INSENSITIVE);
			internal static readonly Pattern ForBrowseStatementReqExp = Pattern.compile(@"\s+FOR\s+BROWSE\s*$", Pattern.CASE_INSENSITIVE);
#endif

			internal static readonly SimpleRegex NamedParameterRegExp = new SqlParamsRegex();
			internal static readonly SimpleRegex SimpleParameterRegExp = new OleDbParamsRegex();

			internal static readonly SimpleRegex CompoundStatementSplitterReqExp = new CharacterSplitterRegex(';');
			internal static readonly SimpleRegex ProcedureParameterSplitterReqExp = new CharacterSplitterRegex(',');

			#endregion // Fields
		}

		#endregion // SqlStatementsHelper

		#region Fields

		string _commandText;
		int _commandTimeout;
		CommandType _commandType;
		bool _designTimeVisible;
		UpdateRowSource _updatedRowSource;

		private DbParameterCollection _parameters;
		private java.sql.Statement _statement;
		private AbstractDBConnection _connection;
		private AbstractTransaction _transaction;
		private bool _isCommandPrepared;
		private CommandBehavior _behavior;
		private ArrayList _internalParameters;
		string _javaCommandText;
		private int _recordsAffected;
		private ResultSet _currentResultSet;
		private DbDataReader _currentReader;
		private bool _nullParametersInPrepare;
		private bool _hasResultSet;
		private bool _explicitPrepare;

		static ProcedureColumnCache _procedureColumnCache = new ProcedureColumnCache();

		#endregion // Fields

		#region Constructors

		public AbstractDbCommand(
			String cmdText,
			AbstractDBConnection connection,
			AbstractTransaction transaction)
		{
			_connection = connection;
			_commandText = cmdText;
			_transaction = transaction;

			_commandTimeout = 30;
			_commandType = CommandType.Text;
			_designTimeVisible = true;
			_updatedRowSource = UpdateRowSource.Both;

			_isCommandPrepared = false;
			_explicitPrepare = false;
			_recordsAffected = -1;
			if (connection != null) {
				connection.AddReference(this);
			}
		}

		#endregion // Constructors

		#region Properties

		public override int CommandTimeout {
			get { return _commandTimeout; }
			set { _commandTimeout = value; }
		}

		public override CommandType CommandType {
			get { return _commandType; }
			set { _commandType = value; }
		}

		public override bool DesignTimeVisible {
			get { return _designTimeVisible; }
			set { _designTimeVisible = value; }
		}	

		public override UpdateRowSource UpdatedRowSource {
			get { return _updatedRowSource; }
			set { _updatedRowSource = value; }
		}

		protected override DbParameterCollection DbParameterCollection
		{
			get {
				if (_parameters == null) {
					_parameters = CreateParameterCollection(this);
				}
				return _parameters; 
			}
		}

		protected override DbConnection DbConnection
		{
			get { return (DbConnection)_connection; }
			set {
				if (value == _connection) {
					return;
				}

				if (_currentReader != null && !_currentReader.IsClosed) {
					throw ExceptionHelper.ConnectionIsBusy(this.GetType().Name,((AbstractDBConnection)_connection).InternalState);
				}
				if (_connection != null) {
					_connection.RemoveReference(this);
				}
				_connection = (AbstractDBConnection) value;
				if (_connection != null) {
					_connection.AddReference(this);
				}
			}
		}

		protected override DbTransaction DbTransaction
		{
			get { return _transaction != null ? _transaction.ActiveTransaction : null; }
			set { _transaction = (AbstractTransaction)value; }
		}

		public override string CommandText
		{
			get { return _commandText; }
			set { 
				if (CommandText == null || String.Compare(CommandText, value,  true) != 0) {
					_commandText = value;
					_isCommandPrepared = false;
					_explicitPrepare = false;
				}
			}
		}

		protected virtual string InternalCommandText {
			get { return CommandText; }
			//set { CommandText = value; }
		}

		internal CommandBehavior Behavior
		{
			get { return _behavior; }
			set { _behavior = value; }
		}

		bool IsCommandPrepared
		{
			get { return _isCommandPrepared; }
			set { _isCommandPrepared = value; }
		}

		bool NullParametersInPrepare
		{
			get { return _nullParametersInPrepare; }
			set { _nullParametersInPrepare = value; }
		}

		protected ArrayList InternalParameters
		{
			get {
				if (_internalParameters == null) {
					_internalParameters = new ArrayList();
				}
				return _internalParameters;
			}
		}

		// Number of records affected by execution of batch statement
		// -1 for SELECT statements.
		internal int RecordsAffected
		{
			get {
				return _recordsAffected;
			}
		}

		// AbstractDbCommand acts as IEnumerator over JDBC statement
		// AbstractDbCommand.CurrentResultSet corresponds to IEnumerator.Current
		protected internal virtual ResultSet CurrentResultSet
		{
			get { 
				try {
					if (_currentResultSet == null && _hasResultSet) {
						_currentResultSet = _statement.getResultSet(); 
					}
					return _currentResultSet;
				}
				catch(SQLException e) {
					throw new Exception(e.Message, e);
				}
			}
		}

		protected internal java.sql.Statement Statement
		{
			get { return _statement; }
		}
#if USE_DOTNET_REGEX
		protected virtual Regex StoredProcedureRegExp
#else
		protected virtual Pattern StoredProcedureRegExp
#endif
		{
			get { return SqlStatementsHelper.SimpleParameterStoredProcedureRegExp; }
		}

		protected virtual SimpleRegex ParameterRegExp
		{
			get { return SqlStatementsHelper.SimpleParameterRegExp; }
		}

		#endregion // Properties

		#region Methods

		protected abstract DbParameter CreateParameterInternal();

		protected abstract void CheckParameters();

		protected abstract DbDataReader CreateReader();

		protected abstract DbParameterCollection CreateParameterCollection(AbstractDbCommand parent);

		protected internal abstract SystemException CreateException(SQLException e);

		public override int ExecuteNonQuery ()
		{
			IDataReader reader = null;
			try {
				reader = ExecuteReader ();
			}
			finally {
				if (reader != null)
					reader.Close ();				
			}
			return reader.RecordsAffected;
		}

		public override object ExecuteScalar ()
		{
			IDataReader reader = ExecuteReader(CommandBehavior.SequentialAccess);
			
			try {
				do {
					if (reader.FieldCount > 0 && reader.Read ())
						return reader.GetValue (0);			
				}
				while (reader.NextResult ());
				return null;
			} finally {
				reader.Close();
			}
		}

		public virtual void ResetCommandTimeout ()
		{
			_commandTimeout = 30;
		}

		public override void Cancel()
		{
			try {
				if (_statement != null)
					_statement.cancel();
			}
			catch {
				// MSDN says : "If there is nothing to cancel, nothing happens. 
				// However, if there is a command in process, and the attempt to cancel fails, 
				// no exception is generated."
			}
		}
		
		protected virtual bool SkipParameter(DbParameter parameter)
		{
			return false;
		}

		protected sealed override DbParameter CreateDbParameter()
		{
			return CreateParameterInternal();
		}

		internal ArrayList DeriveParameters(string procedureName, bool throwIfNotExist)
		{
			try {
				ArrayList col = _procedureColumnCache.GetProcedureColumns((AbstractDBConnection)Connection, procedureName, this);
				if (col == null) {
					if (throwIfNotExist)
						throw ExceptionHelper.NoStoredProcedureExists(procedureName);
					col = new ArrayList();
				}

				return col;
			}
			catch(SQLException e) {
				throw CreateException(e);
			}
		}

		string CreateTableDirectCommandText(string tableNames) {
			string forBrowse = String.Empty;
			if ((Behavior & CommandBehavior.KeyInfo) != 0) {
				AbstractDBConnection connection = (AbstractDBConnection)Connection;
				if (connection != null) {
					string dbname = connection.JdbcConnection.getMetaData().getDatabaseProductName();
					if (dbname == "Microsoft SQL Server")	//must add "FOR BROWSE" for selects
						forBrowse = " FOR BROWSE";
				}
			}

			string[] names = tableNames.Split(',');
			StringBuilder sb = new StringBuilder();

			for(int i = 0; i < names.Length; i++) {
				sb.Append("SELECT * FROM ");
				sb.Append(names[i]);
				sb.Append(forBrowse);
				sb.Append(';');
			}
				
			if(names.Length <= 1) {
				sb.Remove(sb.Length - 1,1);
			}
			return sb.ToString();
		}

		private string PrepareCommandTextAndParameters()
		{
			NullParametersInPrepare = false;
			switch (CommandType) {
				case CommandType.TableDirect :
					return CreateTableDirectCommandText(CommandText);
				case CommandType.StoredProcedure :
					return CreateStoredProcedureCommandTextSimple (InternalCommandText, Parameters, DeriveParameters (InternalCommandText, false));
				case CommandType.Text :

					int userParametersPosition = 0;
					int charsConsumed = 0;
					StringBuilder sb = new StringBuilder(CommandText.Length);

					for (SimpleMatch match = SqlStatementsHelper.CompoundStatementSplitterReqExp.Match(CommandText);
						match.Success;
						match = match.NextMatch()) {

						int length = match.Length;

						if (length == 0)
							continue;

						int start = match.Index;
						string value = match.Value;

						sb.Append(CommandText, charsConsumed, start-charsConsumed);
						charsConsumed = start + length;

#if USE_DOTNET_REGEX
						Match storedProcMatch = StoredProcedureRegExp.Match(value);
						// count parameters for all kinds of simple statements 
						userParametersPosition +=
							(storedProcMatch.Success) ?
							// statement is stored procedure call
							CreateStoredProcedureCommandText(sb, value, storedProcMatch, Parameters, userParametersPosition) :
							// statement is a simple SQL query				
							PrepareSimpleQuery(sb, value, Parameters, userParametersPosition);	
#else
						Matcher storedProcMatch = StoredProcedureRegExp.matcher((java.lang.CharSequence)(object)value);
						userParametersPosition +=
							(storedProcMatch.find()) ?
							// statement is stored procedure call
							CreateStoredProcedureCommandText(sb, value, storedProcMatch, Parameters, userParametersPosition) :
							// statement is a simple SQL query				
							PrepareSimpleQuery(sb, value, Parameters, userParametersPosition);
#endif
					}

					sb.Append(CommandText, charsConsumed, CommandText.Length-charsConsumed);

					return sb.ToString();
			}
			return null;
		}

		string CreateStoredProcedureCommandTextSimple(string procedureName, IDataParameterCollection userParams, IList derivedParams) {
			StringBuilder sb = new StringBuilder();

			int curUserPos = 0;
			int curDerivedPos = 0;
			bool addParas = true;
			string trimedProcedureName = (procedureName != null) ? procedureName.TrimEnd() : String.Empty;
			if (trimedProcedureName.Length > 0 && trimedProcedureName[trimedProcedureName.Length-1] == ')')
				addParas = false;
			
				AbstractDbParameter derivedParam = (derivedParams.Count > 0) ? (AbstractDbParameter)derivedParams[curDerivedPos] : null;
				if (derivedParam != null) {
					if (derivedParam.Direction == ParameterDirection.ReturnValue)
						curDerivedPos++;
					else
						derivedParam = null; //play as if there is no retval parameter
				}
				AbstractDbParameter returnValueParameter = GetReturnParameter (userParams);
				if (returnValueParameter != null) {
					curUserPos++;
					InternalParameters.Add(returnValueParameter);
					sb.Append("{? = call ");

					if (derivedParam != null && !returnValueParameter.IsDbTypeSet) {
						returnValueParameter.JdbcType = derivedParam.JdbcType;
					}
				}
				else {
					sb.Append("{call ");
				}

			sb.Append(procedureName);
			if (addParas)
				sb.Append('(');

			bool needComma = false;
			for (int i = curDerivedPos; i < derivedParams.Count; i++) {
				AbstractDbParameter derivedParameter = (AbstractDbParameter)derivedParams[curDerivedPos++];
				
				bool addParam = false;

				if (derivedParameter.IsSpecial) {
					// derived parameter is special - never appears in user parameters or user values
					InternalParameters.Add((AbstractDbParameter)derivedParameter.Clone());
					addParam = true;
				}
				else {
					AbstractDbParameter userParameter = GetUserParameter(derivedParameter.Placeholder, userParams, curUserPos);
					if (userParameter != null) {
						curUserPos++;
						InternalParameters.Add(userParameter);
						addParam = true;

						if (derivedParameter != null && !userParameter.IsDbTypeSet) {
							userParameter.JdbcType = derivedParameter.JdbcType;
						}
					}
				}

				if (addParam) {
					if (needComma)
						sb.Append(',');
					else
						needComma = true;

					sb.Append('?');
				}
			}

			for (int i = curUserPos; i < userParams.Count; i++) {
				if (needComma)
					sb.Append(',');
				else
					needComma = true;

				AbstractDbParameter userParameter = (AbstractDbParameter)userParams[curUserPos++];
				InternalParameters.Add(userParameter);

				sb.Append('?');
			}

			if (addParas)
				sb.Append(')');
			sb.Append('}');
			return sb.ToString();
		}

		/// <summary>
		/// We suppose that user parameters are in the same order as devived parameters except the special cases
		/// (return value, oracle ref cursors etc.)
		/// </summary>
		//protected virtual string CreateStoredProcedureCommandText(string procedureName, IList userParametersList, int userParametersListStart/*, int userParametersListCount*/, string[] userValuesList, ArrayList derivedParametersList)
#if USE_DOTNET_REGEX
		int CreateStoredProcedureCommandText(StringBuilder sb, string sql, Match match, IDataParameterCollection userParams, int userParamsStartPosition)
#else
		int CreateStoredProcedureCommandText(StringBuilder sb, string sql, Matcher match, IDataParameterCollection userParams, int userParamsStartPosition)
#endif
		{
			int curUserPos = userParamsStartPosition;
#if USE_DOTNET_REGEX
			Group procNameGroup = null;

			for (Match procNameMatch = match; procNameMatch.Success; procNameMatch = procNameMatch.NextMatch()){
				procNameGroup = match.Groups["PROCNAME"];
				if (!procNameGroup.Success) {
					continue;
				}
			}

			if (procNameGroup == null || !procNameGroup.Success)
				throw new ArgumentException("Not a stored procedure call: '{0}'", sql);

			ArrayList derivedParameters = DeriveParameters(procNameGroup.Value, false);
#else
			ArrayList derivedParameters = DeriveParameters(match.group(2).Trim(), false);
#endif
			int curDerivedPos = 0;

			AbstractDbParameter retValderivedParameter = curDerivedPos < derivedParameters.Count ?
				(AbstractDbParameter)derivedParameters[curDerivedPos] : null;
			if (retValderivedParameter != null && retValderivedParameter.Direction == ParameterDirection.ReturnValue)
				curDerivedPos++;

			int queryCurrentPosition = 0;
			
#if USE_DOTNET_REGEX
			for (Match retValMatch = match; retValMatch.Success; retValMatch = retValMatch.NextMatch()){
				Group retval = retValMatch.Groups["RETVAL"];
				if (!retval.Success) {
					continue;
				}

				int retvalIndex = retval.Index;
				string retvalValue = retval.Value;
				int retvalLength = retval.Length;
#else
			int retvalIndex = match.start(1);
			for (;retvalIndex >= 0;) {
				string retvalValue = match.group(1);
				int retvalLength = retvalValue.Length;
#endif

				sb.Append(sql, queryCurrentPosition, retvalIndex);
				AbstractDbParameter userParameter = GetUserParameter(retvalValue, userParams, curUserPos);
				if (userParameter != null) {
					sb.Append('?');
					InternalParameters.Add(userParameter);

					if (retValderivedParameter != null && !userParameter.IsDbTypeSet) {
						userParameter.JdbcType = retValderivedParameter.JdbcType;
					}

					curUserPos++;
				}
				else {
					sb.Append(retvalValue);
				}

				queryCurrentPosition = (retvalIndex + retvalLength);

				break;
			}

#if USE_DOTNET_REGEX
			sb.Append(sql, queryCurrentPosition, procNameGroup.Index + procNameGroup.Length - queryCurrentPosition);
			queryCurrentPosition = procNameGroup.Index + procNameGroup.Length;
#else
			sb.Append(sql, queryCurrentPosition, match.end(2) - queryCurrentPosition);
			queryCurrentPosition = match.end(2);
#endif

			bool hasUserParams = false;

#if USE_DOTNET_REGEX
			must rewrite the regex to not parse params to have single code with java regex
#else
			int paramsStart = match.start(3);
			if (paramsStart >= 0) {
#endif

				hasUserParams = true;
				sb.Append(sql,queryCurrentPosition,paramsStart - queryCurrentPosition);
				queryCurrentPosition = paramsStart;

				for (SimpleMatch m = SqlStatementsHelper.ProcedureParameterSplitterReqExp.Match(match.group(3));
					m.Success;m = m.NextMatch()) {

					SimpleCapture parameterCapture = m;
					sb.Append(sql,queryCurrentPosition,paramsStart + parameterCapture.Index - queryCurrentPosition);

					// advance in query
					queryCurrentPosition = paramsStart + parameterCapture.Index + parameterCapture.Length;

					AbstractDbParameter derivedParameter = curDerivedPos < derivedParameters.Count ?
						(AbstractDbParameter)derivedParameters[curDerivedPos++] : null;
					
					//check for special params
					while (derivedParameter != null && derivedParameter.IsSpecial) {
						// derived parameter is special - never appears in user parameters or user values
						InternalParameters.Add((AbstractDbParameter)derivedParameter.Clone());
						sb.Append('?');
						sb.Append(',');

						derivedParameter = curDerivedPos < derivedParameters.Count ?
							(AbstractDbParameter)derivedParameters[curDerivedPos++] : null;
					}

					AbstractDbParameter userParameter = GetUserParameter(parameterCapture.Value.Trim(), userParams, curUserPos);

					if (userParameter != null) {
						sb.Append('?');
						InternalParameters.Add(userParameter);
						if (derivedParameter != null && !userParameter.IsDbTypeSet) {
							userParameter.JdbcType = derivedParameter.JdbcType;
						}
						// advance in user parameters
						curUserPos++;				
					}
					else {
						sb.Append(parameterCapture.Value);
					}									
				}					
			}

			bool addedSpecialParams = false;

			for (int i = curDerivedPos; i < derivedParameters.Count;) {
				AbstractDbParameter derivedParameter = (AbstractDbParameter)derivedParameters[i++];
				if (derivedParameter.IsSpecial) {
					// derived parameter is special - never appears in user parameters or user values
					if (!hasUserParams && !addedSpecialParams) {
						addedSpecialParams = true;
						curDerivedPos++;
						sb.Append('(');
					}

					for (;curDerivedPos < i;curDerivedPos++)
						sb.Append(',');

					InternalParameters.Add((AbstractDbParameter)derivedParameter.Clone());
					sb.Append('?');
				}
			}

			if (!hasUserParams && addedSpecialParams)
				sb.Append(')');

			sb.Append(sql,queryCurrentPosition,sql.Length - queryCurrentPosition);
			return curUserPos - userParamsStartPosition;
		}

		protected virtual AbstractDbParameter GetUserParameter(string parameterName, IList userParametersList, int userParametersListPosition)
		{
			if (userParametersListPosition < userParametersList.Count) {
				AbstractDbParameter param = (AbstractDbParameter)userParametersList[userParametersListPosition];
				if (param.Placeholder == parameterName)
					return param;
			}
			return null;
		}

		protected virtual AbstractDbParameter GetReturnParameter (IList userParametersList)
		{
			AbstractDbParameter param = GetUserParameter ("?", userParametersList, 0); 

			if (param != null && param.Direction == ParameterDirection.ReturnValue)
				return param;

			return null;
		}

		int PrepareSimpleQuery(StringBuilder sb, string query, IList userParametersList, int userParametersListStart)
		{
			int queryCurrentPosition = 0;
			int userParametersListPosition = userParametersListStart;

			if (userParametersList.Count > 0) {
				for (SimpleMatch m = ParameterRegExp.Match(query);
					m.Success;m = m.NextMatch()) {

					SimpleCapture parameterCapture = m;
					sb.Append(query,queryCurrentPosition,parameterCapture.Index - queryCurrentPosition);

					// advance in query
					queryCurrentPosition = parameterCapture.Index + parameterCapture.Length;	

					AbstractDbParameter userParameter = GetUserParameter(parameterCapture.Value, userParametersList, userParametersListPosition);

					if (userParameter != null) {
						if (IsNullParameter(userParameter)) {
							sb.Append("null");
							NullParametersInPrepare = true;
						}
						else {
							sb.Append('?');
							InternalParameters.Add(userParameter);	
						}	
						// advance in user parameters
						userParametersListPosition++;				
					}
					else {
						sb.Append(parameterCapture.Value);
					}
				}
			}

			sb.Append(query,queryCurrentPosition,query.Length - queryCurrentPosition);
			int userParamsConsumed = userParametersListPosition - userParametersListStart;

			if ((Behavior & CommandBehavior.KeyInfo) == 0)
				return userParamsConsumed;

			AbstractDBConnection connection = (AbstractDBConnection)Connection;
			if (connection == null)
				return userParamsConsumed;

			string dbname = connection.JdbcConnection.getMetaData().getDatabaseProductName();
			if (dbname == "Microsoft SQL Server") {	//must add "FOR BROWSE" for selects
#if USE_DOTNET_REGEX
					if (!SqlStatementsHelper.ForBrowseStatementReqExp.IsMatch(query))
						sb.Append(" FOR BROWSE");
#else
					if (!SqlStatementsHelper.ForBrowseStatementReqExp.matcher ((java.lang.CharSequence)(object)query).find ())
						sb.Append (" FOR BROWSE");
#endif
			}

			return userParamsConsumed;
		}

		protected virtual bool IsNullParameter(AbstractDbParameter parameter)
		{
			return ((parameter.Value == null || parameter.Value == DBNull.Value) && !parameter.IsDbTypeSet);
		}

		protected virtual void PrepareInternalParameters()
		{
			InternalParameters.Clear();
		}
        
		protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
		{
			AbstractDBConnection connection = (AbstractDBConnection)Connection;
			if (connection == null) {
				throw ExceptionHelper.ConnectionNotInitialized("ExecuteReader");
			}

			connection.IsExecuting = true;

			try {
				IDbTransaction transaction = Transaction;
				if ((transaction != null && transaction.Connection != connection) ||
					(transaction == null && !connection.JdbcConnection.getAutoCommit ())) {
					throw ExceptionHelper.TransactionNotInitialized ();
				}

				Behavior = behavior;

				PrepareInternalParameters();			
				PrepareInternal();

				// For SchemaOnly there is no need for statement execution
				if (Behavior != CommandBehavior.SchemaOnly) {
					_recordsAffected = -1;

					// FIXME: this causes SP in MS Sql Server to create no mor than one row.
					if ((Behavior & CommandBehavior.SingleRow) != 0) {
						_statement.setMaxRows (1);
					}
				
					if(_statement is PreparedStatement) {
						BindParameters(InternalParameters);
						_hasResultSet = ((PreparedStatement)_statement).execute();
					}
					else {
						_hasResultSet =_statement.execute(_javaCommandText);					
					}
		
					if (!_hasResultSet) {
						int updateCount = _statement.getUpdateCount();
						if (updateCount >= 0) {
							AccumulateRecordsAffected(updateCount);
							_hasResultSet = true; //play as if we have resultset
							NextResultSet();
						}
					}					
				}
				connection.IsFetching = true;
				try {
					_currentReader = CreateReader();
				}
				catch(Exception e) {
					connection.IsFetching = false;
					throw e;
				}
				return _currentReader;
			}
			catch(SQLException e) {				
				throw CreateException(e);
			}
			finally {
				connection.IsExecuting = false;
				NullParametersInPrepare = false;
			}
		}

		public override void Prepare()
		{
			((AbstractDBConnection)Connection).IsExecuting = true;
			try {
				CheckParameters();
				_explicitPrepare = true;
			}
			finally {
				((AbstractDBConnection)Connection).IsExecuting = false;
			}
		}

		private void PrepareInternal()
		{
			if ((Connection == null) || (Connection.State != ConnectionState.Open)) {
				throw ExceptionHelper.ConnectionNotOpened("Prepare",(Connection != null) ? Connection.State.ToString() : "");
			}

			if (IsCommandPrepared) {
				// maybe we have to prepare the command again
				bool hasNullParameters = false;
				for(int i = 0; (i < Parameters.Count) && !hasNullParameters; i++) {
					AbstractDbParameter parameter = (AbstractDbParameter)Parameters[i];
					if (IsNullParameter(parameter)) {
						// if we still have null parameters - have to prepare agail
						IsCommandPrepared = false;
						hasNullParameters = true;
					}
				}

				if (!NullParametersInPrepare && hasNullParameters) {
					// if we prepeared using null parameters and now there is no null parameters - need to prepare again
					IsCommandPrepared = false;
				}
			}

			if (!IsCommandPrepared) {

				_javaCommandText = PrepareCommandTextAndParameters();

				java.sql.Connection jdbcCon = _connection.JdbcConnection;

				// For SchemaOnly we just prepare statement (for future use in GetSchemaTable)
				if (Behavior == CommandBehavior.SchemaOnly) {
					if (CommandType == CommandType.StoredProcedure)
						_statement = jdbcCon.prepareCall(_javaCommandText);
					else
						_statement = jdbcCon.prepareStatement(_javaCommandText);	
					return;
				}

				if (CommandType == CommandType.StoredProcedure)
					_statement = jdbcCon.prepareCall(_javaCommandText);
				else {
					int internalParametersCount = InternalParameters.Count;
					if ( internalParametersCount > 0) {
						bool hasOnlyInputParameters = true;
						for(int i=0; i < internalParametersCount; i++) {
							AbstractDbParameter internalParameter = (AbstractDbParameter)InternalParameters[i];
							if (IsNullParameter(internalParameter)) {
								NullParametersInPrepare = true;
							}

							if ((internalParameter.Direction & ParameterDirection.Output) != 0){
								hasOnlyInputParameters = false;
							}
						}

						if (hasOnlyInputParameters) {
							_statement = jdbcCon.prepareStatement(_javaCommandText);	
						}
						else {						
							_statement = jdbcCon.prepareCall(_javaCommandText);
						}
					}
					else {
						if (_explicitPrepare) {
							_statement = jdbcCon.prepareStatement(_javaCommandText);				
						}
						else {
							_statement = jdbcCon.createStatement();					
						}
					}
				}
				IsCommandPrepared = true;
			}
		}

		protected void BindParameters(ArrayList parameters)
		{
			for(int parameterIndex = 0; parameterIndex < parameters.Count; parameterIndex++) {
				AbstractDbParameter parameter = (AbstractDbParameter)parameters[parameterIndex];
				switch (parameter.Direction) {
					case ParameterDirection.Input :
						BindInputParameter(parameter,parameterIndex);
						break;
					case ParameterDirection.InputOutput:
						BindInputParameter(parameter,parameterIndex);
						BindOutputParameter(parameter,parameterIndex);
						break;
					case ParameterDirection.Output :
						BindOutputParameter(parameter,parameterIndex);
						break;
					case ParameterDirection.ReturnValue :
						BindOutputParameter(parameter,parameterIndex);
						break;
				}
			}
		}
		
		protected virtual void BindInputParameter(AbstractDbParameter parameter, int parameterIndex)
		{
			object value = parameter.ConvertedValue;			
			// java parameters are 1 based, while .net are 0 based
			parameterIndex++; 
			PreparedStatement preparedStatement = ((PreparedStatement)_statement);

			switch ((DbConvert.JavaSqlTypes)parameter.JdbcType) {
				case DbConvert.JavaSqlTypes.DATALINK:
				case DbConvert.JavaSqlTypes.DISTINCT:
				case DbConvert.JavaSqlTypes.JAVA_OBJECT:
				case DbConvert.JavaSqlTypes.OTHER:
				case DbConvert.JavaSqlTypes.REF:
				case DbConvert.JavaSqlTypes.STRUCT: {
					preparedStatement.setObject(parameterIndex, value, (int)parameter.JdbcType);
					return;
				}
			}

			if ((value is DBNull) || (value == null)) {
				preparedStatement.setNull(parameterIndex, (int)((AbstractDbParameter)parameter).JdbcType);
			}
			else if (value is long) {
				preparedStatement.setLong(parameterIndex, (long)value);
			}
			else if (value is byte[]) {
				if (((byte[])value).Length <= 4000) {
					preparedStatement.setBytes(parameterIndex, vmw.common.TypeUtils.ToSByteArray((byte[]) value));
				}
				else {
					InputStream iStream=new ByteArrayInputStream(vmw.common.TypeUtils.ToSByteArray((byte[]) value));
					preparedStatement.setBinaryStream(parameterIndex,iStream,((byte[])value).Length);
				}
			}
			else if (value is byte) {
				preparedStatement.setByte(parameterIndex, (sbyte)(byte)value);
			}
			else if (value is char[]) {
				Reader reader = new CharArrayReader((char[])value);
				preparedStatement.setCharacterStream(parameterIndex,reader,((char[])value).Length);
			}
			else if (value is bool) {
				preparedStatement.setBoolean(parameterIndex, (bool) value);
			}
			else if (value is char) {
				preparedStatement.setString(parameterIndex, ((char)value).ToString());
			}
			else if (value is DateTime) {
				switch ((DbConvert.JavaSqlTypes)parameter.JdbcType) {
					default:
					case DbConvert.JavaSqlTypes.TIMESTAMP:
						preparedStatement.setTimestamp(parameterIndex,DbConvert.ClrTicksToJavaTimestamp(((DateTime)value).Ticks));
						break;
					case DbConvert.JavaSqlTypes.TIME:
						preparedStatement.setTime(parameterIndex,DbConvert.ClrTicksToJavaTime(((DateTime)value).Ticks));
						break;
					case DbConvert.JavaSqlTypes.DATE:
						preparedStatement.setDate(parameterIndex,DbConvert.ClrTicksToJavaDate(((DateTime)value).Ticks));
						break;
				}
			}
			else if (value is TimeSpan) {
				if (parameter.JdbcType == (int)DbConvert.JavaSqlTypes.TIMESTAMP)
					preparedStatement.setTimestamp(parameterIndex,DbConvert.ClrTicksToJavaTimestamp(((TimeSpan)value).Ticks));
				else
					preparedStatement.setTime(parameterIndex,DbConvert.ClrTicksToJavaTime(((TimeSpan)value).Ticks));
			}
			else if (value is Decimal) {
				preparedStatement.setBigDecimal(parameterIndex, vmw.common.PrimitiveTypeUtils.DecimalToBigDecimal((Decimal) value));
			}
			else if (value is double) {
				preparedStatement.setDouble(parameterIndex, (double)value);
			}
			else if (value is float) {
				preparedStatement.setFloat(parameterIndex, (float)value);
			}
			else if (value is int) {
				preparedStatement.setInt(parameterIndex, (int)value);
			}
			else if (value is string) {
				//can not be done for inout params, due to Oracle problem with FIXED_CHAR out param fetching
				if (parameter.Direction == ParameterDirection.Input && 
					preparedStatement is Mainsoft.Data.Jdbc.Providers.IPreparedStatement &&
					(DbConvert.JavaSqlTypes)parameter.JdbcType == DbConvert.JavaSqlTypes.CHAR) {
					((Mainsoft.Data.Jdbc.Providers.IPreparedStatement)preparedStatement)
						.setChar(parameterIndex, (string)value);
				}
				else
					preparedStatement.setString(parameterIndex, (string)value);
			}
			else if (value is Guid) {
				preparedStatement.setString(parameterIndex, value.ToString());
			}
			else if (value is short) {
				preparedStatement.setShort(parameterIndex, (short)value);
			}
			else if (value is sbyte) {
				preparedStatement.setByte(parameterIndex, (sbyte)value);
			}
			else {
				preparedStatement.setObject(parameterIndex, value);
			}
		}

		protected virtual void BindOutputParameter(AbstractDbParameter parameter, int parameterIndex)
		{
			parameter.Validate();
			int jdbcType = (int)parameter.JdbcType;		
			// java parameters are 1 based, while .net are 0 based
			parameterIndex++;

			CallableStatement callableStatement = ((CallableStatement)_statement);

			// the scale has a meening only in DECIMAL and NUMERIC parameters
			if (jdbcType == Types.DECIMAL || jdbcType == Types.NUMERIC) {
				if(parameter.DbType == DbType.Currency) {
					callableStatement.registerOutParameter(parameterIndex, jdbcType, 4);
				}
				else {
					callableStatement.registerOutParameter(parameterIndex, jdbcType, parameter.Scale);
				}
			}
			else {
				callableStatement.registerOutParameter(parameterIndex, jdbcType);
			}
		}

		private void FillOutputParameters()
		{	
			if  (!(_statement is CallableStatement)) {
				return;
			}
			for(int i = 0; i < InternalParameters.Count; i++) {
				AbstractDbParameter parameter = (AbstractDbParameter)InternalParameters[i];
				ParameterDirection direction = parameter.Direction;
				if (((direction & ParameterDirection.Output) != 0) && !SkipParameter(parameter)) {					
					FillOutputParameter(parameter, i);
				}
				// drop jdbc type of out parameter, since it possibly was updated in ExecuteReader
				parameter.IsJdbcTypeSet = false;
			}
		}

		protected virtual void FillOutputParameter(DbParameter parameter, int index)
		{			
			CallableStatement callableStatement = (CallableStatement)_statement;
			ParameterMetadataWrapper parameterMetadataWrapper = null; 
			// FIXME wait for other drivers to implement
//			try {
//				parameterMetadataWrapper = new ParameterMetadataWrapper(callableStatement.getParameterMetaData());
//			}
//			catch {
//				// suppress error : ms driver for sql server does not implement getParameterMetaData
//				// suppress exception : ms driver for sql server does not implement getParameterMetaData
//			}
			DbConvert.JavaSqlTypes javaSqlType = (DbConvert.JavaSqlTypes)((AbstractDbParameter)parameter).JdbcType;
			try {
				parameter.Value = DbConvert.JavaResultSetToClrWrapper(callableStatement,index,javaSqlType,parameter.Size,parameterMetadataWrapper);
			}
			catch(java.sql.SQLException e) {
				throw CreateException(e);
			}
		}

		// AbstractDbCommand acts as IEnumerator over JDBC statement
		// AbstractDbCommand.NextResultSet corresponds to IEnumerator.MoveNext
		protected internal virtual bool NextResultSet()
		{
			if (!_hasResultSet)
				return false;

			try {
				for(;;) {
					_hasResultSet = _statement.getMoreResults();
					if (_hasResultSet)
						return true;
					int updateCount = _statement.getUpdateCount();
					if (updateCount < 0)
						return false;

					AccumulateRecordsAffected(updateCount);	
				}
			}
			catch (SQLException e) {
				throw CreateException(e);
			}
			finally {
				_currentResultSet = null;
			}
		}

		private void AccumulateRecordsAffected(int updateCount)
		{ 
			if (_recordsAffected < 0) {
				_recordsAffected = updateCount;
			}
			else {
				_recordsAffected += updateCount;
			}
		}

		internal void OnReaderClosed(object reader)
		{
			CloseInternal();
			if (Connection != null) {
				((AbstractDBConnection)Connection).RemoveReference(reader);
				((AbstractDBConnection)Connection).IsFetching = false;
				if ((Behavior & CommandBehavior.CloseConnection) != 0) {
					Connection.Close();
				}
			}			
		}

		internal void CloseInternal()
		{
			if (Behavior != CommandBehavior.SchemaOnly) {
				if (_statement != null) {
					while (NextResultSet()) {
					}							
					FillOutputParameters();				
				}
			}
			_currentReader = null;
			CleanUp();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing) {
				CleanUp();
			}
			base.Dispose(disposing);
		}

		private void CleanUp()
		{
			if (_currentReader != null) {
				// we must preserve statement object until we have an associated reader object that might access it.
				return;
			}
			if (Connection != null) {
				((AbstractDBConnection)Connection).RemoveReference(this);
			}
			if (_statement != null) {
				_statement.close();
				_statement = null;
			}				
			IsCommandPrepared = false;
			_internalParameters = null;
			_currentResultSet = null;
		}

		internal void OnSchemaChanging()
		{
		}

		#endregion // Methods

		#region ICloneable Members

		public virtual object Clone() {
			AbstractDbCommand target = (AbstractDbCommand)MemberwiseClone();
			target._statement = null;
			target._isCommandPrepared = false;
			target._internalParameters = null;
			target._javaCommandText = null;
			target._recordsAffected = -1;
			target._currentResultSet = null;
			target._currentReader = null;
			target._nullParametersInPrepare = false;
			target._hasResultSet = false;
			target._explicitPrepare = false;
			if (Parameters != null && Parameters.Count > 0) {
				target._parameters = CreateParameterCollection(target);
				for(int i=0 ; i < Parameters.Count; i++) {
					target.Parameters.Add(((AbstractDbParameter)Parameters[i]).Clone());
				}
			}
			return target;
		}

		#endregion
	}
}
