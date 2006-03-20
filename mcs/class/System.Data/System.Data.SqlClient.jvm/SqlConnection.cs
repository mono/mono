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

		static readonly IConnectionProvider _connectionProvider;

		#endregion // Fields

		#region Constructors

		static SqlConnection() {
			IDictionary providerInfo = (IDictionary)((IList) ConfigurationSettings.GetConfig("Mainsoft.Data.Configuration/SqlClientProvider"))[0];
			string providerType = (string) providerInfo [ConfigurationConsts.ProviderType];
			if (providerType == null || providerType.Length == 0)
				_connectionProvider = new GenericProvider (providerInfo); 
			else {
				Type t = Type.GetType (providerType);
				_connectionProvider = (IConnectionProvider) Activator.CreateInstance (t , new object[] {providerInfo});
			}
		}

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

		[DataCategory ("StateChange")]
		[DataSysDescription ("Event triggered when the connection changes state.")]
		public event StateChangeEventHandler StateChange;

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
			return _connectionProvider;
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

		protected internal sealed override void OnStateChanged(ConnectionState orig, ConnectionState current)
		{
			if(StateChange != null) {
				StateChange(this, new StateChangeEventArgs(orig, current));
			}
		}

		public override void Close()
		{
			ConnectionState orig = State;
			base.Close();
			ConnectionState current = State;
			if(current != orig) {
				OnStateChanged(orig, current);
			}
		}

		#endregion // Methods
        
	}
}
