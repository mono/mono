//
// System.Data.SqlClient.SqlCommandBuilder.cs
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

namespace System.Data.SqlClient {

	/// <summary>
	/// Builder of one command
	/// that will be used in manipulating a table for
	/// a DataSet that is assoicated with a database.
	/// </summary>
	public sealed class SqlCommandBuilder : Component {
		
		[MonoTODO]
		public SqlCommandBuilder() {

		}

		[MonoTODO]
		public SqlCommandBuilder(SqlDataAdapter adapter) {
		
		}

		[MonoTODO]
		public SqlDataAdapter DataAdapter {
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
		public static void DeriveParameters(SqlCommand command) {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlCommand GetDeleteCommand() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlCommand GetInsertCommand() {
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SqlCommand GetUpdateCommand() {
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
		~SqlCommandBuilder() {
			// FIXME: create destructor - release resources
		}
	}
}

