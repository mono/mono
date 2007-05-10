//
// System.Data.SqlClient.SqlCommand
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
using System.Xml;

using java.sql;

namespace System.Data.SqlClient
{
	public sealed class SqlCommand : AbstractDbCommand
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

		protected override string InternalCommandText {
			get {
				string commandText = CommandText;
				if (CommandType != CommandType.StoredProcedure ||
					string.IsNullOrEmpty (commandText) ||
					commandText [0] == '[' ||
					commandText.IndexOf ('.') >= 0)
					return commandText;

				string trimmedCommandText = commandText.TrimEnd ();
				if (trimmedCommandText.Length > 0 && trimmedCommandText [trimmedCommandText.Length - 1] != ')')
					commandText = String.Concat ("[", commandText, "]");

				return commandText;
			}
		}

		public new SqlConnection Connection
		{
			get { return (SqlConnection)base.Connection; }
			set { base.Connection = value; }
		}
        
		public new SqlParameterCollection Parameters
		{
			get { 
				return (SqlParameterCollection)base.Parameters; 
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

		public XmlReader ExecuteXmlReader() {
			return SqlXmlTextReader.Create(ExecuteReader(CommandBehavior.SequentialAccess));
		}

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

		protected sealed override void CheckParameters()
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
				if (String.Compare(parameterName, userParameter.Placeholder.Trim(), true, System.Globalization.CultureInfo.InvariantCulture) == 0) {
					return userParameter;
				}
			}

			return null;
		}

		protected override AbstractDbParameter GetReturnParameter (IList userParametersList)
		{
			for(int i=0; i < userParametersList.Count; i++) {
				AbstractDbParameter userParameter = (AbstractDbParameter)userParametersList[i];
				if (userParameter.Direction == ParameterDirection.ReturnValue) {
					return userParameter;
				}
			}

			return null; 
		}

		protected sealed override DbParameter CreateParameterInternal()
		{
			return new SqlParameter();
		}

		protected sealed override DbDataReader CreateReader()
		{
			return new SqlDataReader(this);
		}

		protected sealed override DbParameterCollection CreateParameterCollection(AbstractDbCommand parent)
		{
			return new SqlParameterCollection((SqlCommand)parent);
		}

		protected internal sealed override SystemException CreateException(SQLException e)
		{
			return new SqlException(e, Connection);
		}

		#region Asynchronous behavior
#if NET_2_0
		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public IAsyncResult BeginExecuteReader () 
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public IAsyncResult BeginExecuteReader (CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public IAsyncResult BeginExecuteReader (AsyncCallback callback, Object stateObject) 
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public IAsyncResult BeginExecuteReader (AsyncCallback callback, Object stateObject, CommandBehavior behavior) 
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public SqlDataReader EndExecuteReader (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public IAsyncResult BeginExecuteXmlReader () 
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public IAsyncResult BeginExecuteXmlReader (AsyncCallback callback, Object stateObject)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public XmlReader EndExecuteXmlReader (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public IAsyncResult BeginExecuteNonQuery ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public IAsyncResult BeginExecuteNonQuery (AsyncCallback callback, Object stateObject)
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Asynchronous behavior not implemented")]
		public int EndExecuteNonQuery (IAsyncResult asyncResult)
		{
			throw new NotImplementedException ();
		}
#endif
		#endregion
		#endregion // Methods
	}
}
