//
// Mono.Data.PostgreSqlClient.PgSqlCommandBuilder.cs
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Daniel Morgan (danmorg@sc.rr.com)
//
// (C) Ximian, Inc 2002
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

