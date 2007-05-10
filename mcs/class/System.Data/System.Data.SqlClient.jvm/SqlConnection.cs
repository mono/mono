//
// System.Data.SqlClient.SqlConnection
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

using System.Data;
using System.Data.Common;
using System.Collections;
using System.Data.ProviderBase;

using java.sql;

using System.Configuration;
using Mainsoft.Data.Configuration;
using Mainsoft.Data.Jdbc.Providers;

namespace System.Data.SqlClient
{
	public class SqlConnection : AbstractDBConnection
	{
		#region Fields

		private const int DEFAULT_PACKET_SIZE = 8192;

		#endregion // Fields

		#region Constructors

		public SqlConnection() : this(null)
		{
		}

		public SqlConnection(String connectionString) : base(connectionString)
		{
		}

		#endregion // Constructors

		#region Events

		[DataCategory ("InfoMessage")]
		[DataSysDescription ("Event triggered when messages arrive from the DataSource.")]
		public event SqlInfoMessageEventHandler InfoMessage;

		#endregion // Events

		#region Properties

		public string WorkstationId
		{
			get { return (string)ConnectionStringBuilder["workstation id"]; }
		}

		public int PacketSize
		{
			get { 
				string packetSize = (string)ConnectionStringBuilder["Packet Size"];
				if (packetSize == null || packetSize.Length == 0) {
					return DEFAULT_PACKET_SIZE;
				}				
				try {
					return Convert.ToInt32(packetSize);
				}
				catch(FormatException e) {
					throw ExceptionHelper.InvalidValueForKey("packet size");
				}
				catch (OverflowException e) {
					throw ExceptionHelper.InvalidValueForKey("packet size");
				}
			}
		}

		protected override IConnectionProvider GetConnectionProvider() {
			IDictionary conProviderDict = ConnectionStringDictionary.Parse(ConnectionString);
			string provider = (string)conProviderDict["Provider"];
			if (provider == null)
				provider = "SQLCLIENT";

			return GetConnectionProvider("Mainsoft.Data.Configuration/SqlClientProviders", provider);
		}

		#endregion // Properties

		#region Methods

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
			return BeginTransaction(isolationLevel);
		}

		public SqlTransaction BeginTransaction(String transactionName)
		{
			return BeginTransaction(IsolationLevel.ReadCommitted,transactionName);
		}

		public new SqlTransaction BeginTransaction(IsolationLevel isolationLevel)
		{
			return BeginTransaction(isolationLevel,"Transaction");
		}

		public new SqlTransaction BeginTransaction()
		{
			return BeginTransaction(IsolationLevel.ReadCommitted);
		}
        
		public SqlTransaction BeginTransaction(IsolationLevel isolationLevel, string transactionName)
		{
			return new SqlTransaction(isolationLevel, this, transactionName);
		}

		public new SqlCommand CreateCommand()
		{
			return new SqlCommand(this);
		}

		protected override DbCommand CreateDbCommand() {
			return CreateCommand();
		}

		protected internal sealed override void OnSqlWarning(SQLWarning warning)
		{
			SqlErrorCollection col = new SqlErrorCollection(warning, this);
			OnSqlInfoMessage(new SqlInfoMessageEventArgs(col));
		}

		protected sealed override SystemException CreateException(SQLException e)
		{
			return new SqlException(e, this);		
		}

		protected sealed override SystemException CreateException(string message)
		{
			return new SqlException(message, null, this);		
		}

		private void OnSqlInfoMessage (SqlInfoMessageEventArgs value)
		{
			if (InfoMessage != null) {
				InfoMessage (this, value);
			}
		}

#if NET_2_0

		[MonoNotSupported("")]
		public static void ChangePassword (string connectionString, string newPassword) 
		{
			throw new NotImplementedException ();

			// FIXME: refactored from Mono implementation.  Not finished!!!
			if (connectionString == null || newPassword == null || newPassword == String.Empty)
				throw new ArgumentNullException ();
			if (newPassword.Length > 128)
				throw new ArgumentException ("The value of newPassword exceeds its permittable length which is 128");

			SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder (connectionString);
			if (builder.IntegratedSecurity) {
				throw new ArgumentException ("Can't use integrated security when changing password");
			}
			
			using (SqlConnection conn = new SqlConnection (connectionString)) {
				conn.Open ();
				SqlCommand cmd = conn.CreateCommand ();
				cmd.CommandText = "sp_password";
				cmd.CommandType = CommandType.StoredProcedure;
				// FIXME: Need to extract old password and user from our structures
				// of the connectionString.
				cmd.Parameters.Add (builder.Password); // Is this good???
				cmd.Parameters.Add (newPassword);
				cmd.Parameters.Add (builder.UserID); // Is this good???
				cmd.ExecuteNonQuery();
			}
		}

		#region Pooling

		[MonoNotSupported("Pooling not supported")]
		public static void ClearPool (SqlConnection connection) 
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Pooling not supported")]
		public static void ClearAllPools () 
		{
			throw new NotImplementedException ();
		}

		#endregion
		#region Statistics

		[MonoNotSupported ("Statistics not supported")]
		public IDictionary RetrieveStatistics ()
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("Statistics not supported")]
		public void ResetStatistics ()
		{
			throw new NotImplementedException ();
		}

		#endregion
#endif
		#endregion // Methods

	}
}
