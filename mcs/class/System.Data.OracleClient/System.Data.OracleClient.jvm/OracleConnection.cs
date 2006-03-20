//
// System.Data.OracleClient.OracleConnection
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
using System.Data.ProviderBase;
using System.Collections;

using java.sql;

using System.Configuration;
using Mainsoft.Data.Configuration;
using Mainsoft.Data.Jdbc.Providers;

namespace System.Data.OracleClient {
	public sealed class OracleConnection : AbstractDBConnection, System.ICloneable {
		#region Fields 

		static readonly IConnectionProvider _connectionProvider;

		#endregion //Fields

		#region Events

		public event OracleInfoMessageEventHandler InfoMessage;
		public event StateChangeEventHandler StateChange;

		#endregion // Events
		
		#region Constructors

		static OracleConnection() {
			IDictionary providerInfo = (IDictionary)((IList) ConfigurationSettings.GetConfig("Mainsoft.Data.Configuration/OracleClientProvider"))[0];
			string providerType = (string) providerInfo ["type"];
			if (providerType == null || providerType.Length == 0)
				_connectionProvider = new GenericProvider (providerInfo); 
			else {
				Type t = Type.GetType (providerType);
				_connectionProvider = (IConnectionProvider) Activator.CreateInstance (t , new object[] {providerInfo});
			}
		}

		public OracleConnection() : this(null) {
		}

		public OracleConnection(String connectionString) : base(connectionString) {			
		}

		#endregion // Constructors

		#region Methods

		protected override IConnectionProvider GetConnectionProvider() {
			return _connectionProvider;
		}

		public new OracleTransaction BeginTransaction(IsolationLevel level) {
			return new OracleTransaction(level, this);
		}

		public new OracleTransaction BeginTransaction() {
			return BeginTransaction(IsolationLevel.ReadCommitted);
		}

		public new OracleCommand CreateCommand() {
			return new OracleCommand(this);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
			return BeginTransaction();
		}

		protected override DbCommand CreateDbCommand() {
			return CreateCommand();
		}

		protected sealed override SystemException CreateException(SQLException e) {
			return new OracleException(e,this);		
		}

		protected sealed override SystemException CreateException(string message) {
			return new OracleException(message, null, this);	
		}

		protected sealed override void OnSqlWarning(SQLWarning warning) {
			OracleErrorCollection col = new OracleErrorCollection(warning, this);
			OnOracleInfoMessage(new OracleInfoMessageEventArgs(col));
		}

        
		protected sealed override void OnStateChanged(ConnectionState orig, ConnectionState current) {
			if(StateChange != null) {
				StateChange(this, new StateChangeEventArgs(orig, current));
			}
		}

		public override void Close() {
			ConnectionState orig = State;
			base.Close();
			ConnectionState current = State;
			if(current != orig) {
				OnStateChanged(orig, current);
			}
		}

		private void OnOracleInfoMessage (OracleInfoMessageEventArgs value) {
			if (InfoMessage != null) {
				InfoMessage (this, value);
			}
		}

		#endregion // Methods

	}
}