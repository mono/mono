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

using java.sql;

namespace System.Data.SqlClient
{
	public class SqlConnection : AbstractDBConnection, IDbConnection, System.ICloneable
	{
		#region Fields

		private const int DEFAULT_PACKET_SIZE = 8192;

		protected static Hashtable _skippedUserParameters = new Hashtable(new CaseInsensitiveHashCodeProvider(),new CaseInsensitiveComparer());

		private static readonly object _lockObjectStringManager = new object();
		//private static DbStringManager _stringManager = new DbStringManager("System.Data.System.Data.ProviderBase.jvm.SqlClientStrings");

		private static readonly string[] _resourceIgnoredKeys = new string[] {"CON_DATA_SOURCE","CON_DATABASE",
																			  "CON_PASSWORD","CON_USER_ID","CON_TIMEOUT",
																			  "CON_JNDI_NAME","CON_JNDI_PROVIDER","CON_JNDI_FACTORY",
																			  "CON_WORKSTATION_ID","CON_PACKET_SIZE"};

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

		[DataCategory ("StateChange")]
		[DataSysDescription ("Event triggered when the connection changes state.")]
		public event StateChangeEventHandler StateChange;

		#endregion // Events

		#region Properties

		public string WorkstationId
		{
			get { return ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_WORKSTATION_ID")); }
		}

		protected override string JdbcDriverName
		{
			get { return StringManager.GetString("JDBC_DRIVER"); }
		}

		protected internal override JDBC_MODE JdbcMode
		{
			get {
				string[] conJndiNameStr = StringManager.GetStringArray("CON_JNDI_NAME");
				if (!String.Empty.Equals(ConnectionStringHelper.FindValue(UserParameters,conJndiNameStr))) {
					return JDBC_MODE.DATA_SOURCE_MODE;
				}
				return JDBC_MODE.PROVIDER_MODE; 
			}
		}

		public int PacketSize
		{
			get { 
				string packetSize = ConnectionStringHelper.FindValue(UserParameters,StringManager.GetStringArray("CON_PACKET_SIZE"));				
				if (String.Empty.Equals(packetSize)) {
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

		protected override string[] ResourceIgnoredKeys
		{
			get { return _resourceIgnoredKeys; }
		}

		protected override Hashtable SkippedUserParameters
		{
			get { return _skippedUserParameters; }
		}

		protected override PROVIDER_TYPE ProviderType
		{
			get {
				if (JdbcMode != JDBC_MODE.PROVIDER_MODE) {
					return PROVIDER_TYPE.NONE;
				}
				return PROVIDER_TYPE.SQLOLEDB;
			}
		}

		protected override DbStringManager StringManager
		{
			get {
				const string stringManagerName = "System.Data.OleDbConnection.stringManager";
				object stringManager = AppDomain.CurrentDomain.GetData(stringManagerName);
				if (stringManager == null) {
					lock(_lockObjectStringManager) {
						stringManager = AppDomain.CurrentDomain.GetData(stringManagerName);
						if (stringManager != null)
							return (DbStringManager)stringManager;
						stringManager = new DbStringManager("System.Data.System.Data.ProviderBase.jvm.SqlClientStrings");
						AppDomain.CurrentDomain.SetData(stringManagerName, stringManager);
					}
				}
				return (DbStringManager)stringManager;
			}
		}

		#endregion // Properties

		#region Methods

		protected internal override void ValidateConnectionString(string connectionString)
		{
			base.ValidateConnectionString(connectionString);

			// FIXME : validate packet size
		}

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

		internal override void OnSqlWarning(SQLWarning warning)
		{
			SqlErrorCollection col = new SqlErrorCollection(warning, this);
			OnSqlInfoMessage(new SqlInfoMessageEventArgs(col));
		}

		protected internal override void CopyTo(AbstractDBConnection target)
		{
			base.CopyTo(target);
		}

		public object Clone()
		{
			SqlConnection clone = new SqlConnection();
			CopyTo(clone);
			return clone;
		}

		protected override SystemException CreateException(SQLException e)
		{
			return new SqlException(e, this);		
		}

		protected override SystemException CreateException(string message)
		{
			return new SqlException(message, null, this);		
		}

		private void OnSqlInfoMessage (SqlInfoMessageEventArgs value)
		{
			if (InfoMessage != null) {
				InfoMessage (this, value);
			}
		}

		internal override void OnStateChanged(ConnectionState orig, ConnectionState current)
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

		protected override string BuildJdbcUrl()
		{
			switch (JdbcMode) {
				case JDBC_MODE.PROVIDER_MODE :
					return BuildMsSqlUrl();
			}
			return base.BuildJdbcUrl();
		}

		#endregion // Methods
        
	}
}