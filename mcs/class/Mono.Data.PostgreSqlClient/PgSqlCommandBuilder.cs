//
// Mono.Data.PostgreSqlClient.PgSqlCommandBuilder.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
//

using System;
using System.Data;
using System.ComponentModel;

namespace Mono.Data.PostgreSqlClient {

	/// <summary>
	/// Builder of one command
	/// that will be used in manipulating a table for
	/// a DataSet that is assoicated with a database.
	/// </summary>
	public sealed class PgSqlCommandBuilder : Component {
		
		[MonoTODO]
		public PgSqlCommandBuilder() {

		}

		[MonoTODO]
		public PgSqlCommandBuilder(PgSqlDataAdapter adapter) {
		
		}

		[MonoTODO]
		public PgSqlDataAdapter DataAdapter {
			get {
				throw new NotImplementedException ();
			}
			
			set{
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string QuotePrefix {
			get {
				throw new NotImplementedException ();
			} 
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public string QuoteSuffix {
			get {
				throw new NotImplementedException ();
			}
			
			set {
				throw new NotImplementedException ();
			}
		}

		[MonoTODO]
		public static void DeriveParameters(PgSqlCommand command) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PgSqlCommand GetDeleteCommand() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PgSqlCommand GetInsertCommand() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public PgSqlCommand GetUpdateCommand() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RefreshSchema() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose(bool disposing) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		~PgSqlCommandBuilder() {
			// FIXME: create destructor - release resources
		}
	}
}

