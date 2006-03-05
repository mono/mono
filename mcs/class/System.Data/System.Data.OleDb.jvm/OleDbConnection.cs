//
// System.Data.OleDb.OleDbConnection
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
using System.Globalization;

using java.sql;

using System.Configuration;
using Mainsoft.Data.Configuration;
using Mainsoft.Data.Jdbc.Providers;

namespace System.Data.OleDb
{
	public sealed class OleDbConnection : AbstractDBConnection {
		#region Fields 

		static readonly IList _providers = (IList) ConfigurationSettings.GetConfig("Mainsoft.Data.Configuration/OleDbProviders");

		#endregion //Fields

		#region Events

		public event OleDbInfoMessageEventHandler InfoMessage;
		public event StateChangeEventHandler StateChange;

		#endregion // Events
		
		#region Constructors

		public OleDbConnection() : this(null) {
		}

		public OleDbConnection(String connectionString) : base(connectionString) {			
		}

		#endregion // Constructors

		#region Properties

		public String Provider {
			get {
				IDictionary conDict = ConnectionStringBuilder;
				string provider = (string)conDict["Provider"];
				if (provider == null || provider.Length == 0)
					throw ExceptionHelper.OleDbNoProviderSpecified();

				return provider;
			}
		}

		protected override IConnectionProvider GetConnectionProvider() {
			IDictionary conProviderDict = ConnectionStringDictionary.Parse(ConnectionString);
			string providerName = (string)conProviderDict["Provider"];

			for (int i = 0; i < _providers.Count; i++) {
				IDictionary providerInfo = (IDictionary) _providers[i];
					
				string curProvider = (string)providerInfo["Provider"];
				if (String.Compare(providerName, curProvider, true, CultureInfo.InvariantCulture) == 0) {
					string providerType = (string) providerInfo [ConfigurationConsts.ProviderType];
					if (providerType == null || providerType.Length == 0)
						return new GenericProvider (providerInfo); 
					else {
						Type t = Type.GetType (providerType);
						return (IConnectionProvider) Activator.CreateInstance (t , new object[] {providerInfo});
					}
				}
			}

			return new GenericProvider (conProviderDict);
		}

		#endregion // Properties

		#region Methods

		public new OleDbTransaction BeginTransaction(IsolationLevel level)
		{
			return new OleDbTransaction(level, this);
		}

		public new OleDbTransaction BeginTransaction()
		{
			return BeginTransaction(IsolationLevel.ReadCommitted);
		}

		public new OleDbCommand CreateCommand()
		{
			return new OleDbCommand(this);
		}

		protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) {
			return BeginTransaction();
		}

		protected override DbCommand CreateDbCommand() {
			return CreateCommand();
		}

		protected sealed override SystemException CreateException(SQLException e)
		{
			return new OleDbException(e,this);		
		}

		protected sealed override SystemException CreateException(string message)
		{
			return new OleDbException(message, null, this);	
		}

		public DataTable GetOleDbSchemaTable (Guid schema, object[] restrictions)
		{
			DataTable schemaTable = new DataTable("Tables");
			schemaTable.Columns.Add("TABLE_CATALOG");
			schemaTable.Columns.Add("TABLE_SCHEMA");
			schemaTable.Columns.Add("TABLE_NAME");
			schemaTable.Columns.Add("TABLE_TYPE");
			schemaTable.Columns.Add("TABLE_GUID");
			schemaTable.Columns.Add("DESCRIPTION");
			schemaTable.Columns.Add("TABLE_PROPID");
			schemaTable.Columns.Add("DATE_CREATED");
			schemaTable.Columns.Add("DATE_MODIFIED");
            
            
			java.sql.Connection con = JdbcConnection;
			String catalog = con.getCatalog();
            
			DatabaseMetaData meta = con.getMetaData();
			ResultSet schemaRes = meta.getSchemas();
			System.Collections.ArrayList schemas = new System.Collections.ArrayList();
			while(schemaRes.next()) {
				schemas.Add(schemaRes.getString(1));
			}
			schemaRes.close();

			for(int i = 0; i < schemas.Count; i++) {
				ResultSet tableRes = meta.getTables(catalog, schemas[i].ToString(), null, null);
				while(tableRes.next()) {
					DataRow row = schemaTable.NewRow();
					row["TABLE_CATALOG"] = catalog;
					row["TABLE_SCHEMA"] = schemas[i];
					row["TABLE_NAME"] = tableRes.getString("TABLE_NAME");
					row["TABLE_TYPE"] = tableRes.getString("TABLE_TYPE");
					row["DESCRIPTION"] = tableRes.getString("REMARKS");
                    
					schemaTable.Rows.Add(row);
				}
				tableRes.close();
			}
			return schemaTable;
		}

		public static void ReleaseObjectPool()
		{
			// since we're using connection pool from app servet, this is by design
			//throw new NotImplementedException();
		}

		protected internal sealed override void OnSqlWarning(SQLWarning warning)
		{
			OleDbErrorCollection col = new OleDbErrorCollection(warning, this);
			OnOleDbInfoMessage(new OleDbInfoMessageEventArgs(col));
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

		private void OnOleDbInfoMessage (OleDbInfoMessageEventArgs value)
		{
			if (InfoMessage != null) {
				InfoMessage (this, value);
			}
		}

		#endregion // Methods

	}
}
