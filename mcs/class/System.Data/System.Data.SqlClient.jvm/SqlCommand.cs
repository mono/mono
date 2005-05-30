//
// System.Data.SqlClient.SqlCommand
//
// Author:
//   Boris Kirzner (borisk@mainsoft.com)
//   Konstantin Triger (kostat@mainsoft.com)
//

using System;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;

using java.sql;

namespace System.Data.SqlClient
{
	public class SqlCommand : AbstractDbCommand, IDbCommand, IDisposable, ICloneable
	{
		#region Fields

		#endregion // Fields

		#region Constructors

		// Initializes a new instance of the SqlCommand class.
		// The base constructor initializes all fields to their default values.
		// The following table shows initial property values for an instance of SqlCommand.
		public SqlCommand() : this(null, null, null)
		{
		}

		public SqlCommand(SqlConnection connection) : this(null, connection, null)
		{
		}

		// Initializes a new instance of the SqlCommand class with the text of the query.
		public SqlCommand(String cmdText) : this(cmdText, null, null)
		{
		}

		// Initializes a new instance of the SqlCommand class with the text of the query and a SqlConnection.
		public SqlCommand(String cmdText, SqlConnection connection) : this(cmdText, connection, null)
		{
		}

		// Initializes a new instance of the SqlCommand class with the text of the query, a SqlConnection, and the Transaction.
		public SqlCommand(
			String cmdText,
			SqlConnection connection,
			SqlTransaction transaction)
			: base(cmdText, connection, transaction)
		{
		}

		#endregion // Constructors

		#region Properties

		public new SqlConnection Connection
		{
			get { return (SqlConnection)base.Connection; }
			set { base.Connection = value; }
		}
        
		public new SqlParameterCollection Parameters
		{
			get { 
				if (_parameters == null) {
					_parameters = CreateParameterCollection(this);
				}
				return (SqlParameterCollection)_parameters; 
			}
		}

		public new SqlTransaction Transaction
		{
			get { return (SqlTransaction)base.Transaction; }
			set { base.Transaction = value; }
		}

#if USE_DOTNET_REGEX
		protected override Regex StoredProcedureRegExp
#else
		protected override java.util.regex.Pattern StoredProcedureRegExp {
#endif
			get { return SqlStatementsHelper.NamedParameterStoredProcedureRegExp; }
		}

		protected override SimpleRegex ParameterRegExp
		{
			get { return SqlStatementsHelper.NamedParameterRegExp; }
		}

		#endregion // Properties

		#region Methods

		public new SqlDataReader ExecuteReader()
		{
			return (SqlDataReader)ExecuteReader(CommandBehavior.Default);
		}

		public new SqlDataReader ExecuteReader(CommandBehavior behavior)
		{
			return (SqlDataReader)base.ExecuteReader(behavior);
		}

		public new SqlParameter CreateParameter()
		{
			return (SqlParameter)CreateParameterInternal();
		}

		protected override void CheckParameters()
		{
			// do nothing
		}

		protected override AbstractDbParameter GetUserParameter(string parameterName, IList userParametersList, int userParametersListPosition/*,int userParametersListStart,int userParameterListCount*/)
		{
//			Match match = SqlStatementsHelper.NamedParameterRegExp.Match(parameterName);
//			parameterName = match.Result("${USERPARAM}");
//			if (parameterName.Length == 0)
//				return null;

			for(int i=0; i < userParametersList.Count; i++) {
				AbstractDbParameter userParameter = (AbstractDbParameter)userParametersList[i];
				if (String.Compare(parameterName, userParameter.ParameterName.Trim(), true) == 0) {
					return userParameter;
				}
			}

			return null;
		}

		protected override DbParameter CreateParameterInternal()
		{
			return new SqlParameter();
		}

		protected override DbDataReader CreateReader()
		{
			return new SqlDataReader(this);
		}

		protected override DbParameterCollection CreateParameterCollection(AbstractDbCommand parent)
		{
			return new SqlParameterCollection((SqlCommand)parent);
		}

		public object Clone()
		{
			SqlCommand clone = new SqlCommand();
			CopyTo(clone);
			return clone;
		}

		protected override SystemException CreateException(SQLException e)
		{
			return new SqlException(e, Connection);		
		}

		#endregion // Methods
	}
}