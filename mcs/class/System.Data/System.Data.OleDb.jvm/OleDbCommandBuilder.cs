//
// System.Data.OleDb.OleDbCommandBuilder
//
// Author:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//	Konstantin Triger <kostat@mainsoft.com>
//	Boris Kirzner <borisk@mainsoft.com>
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002	
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


using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Data.ProviderBase;

namespace System.Data.OleDb
{
	/// <summary>
	/// Provides a means of automatically generating single-table commands used to reconcile changes made to a DataSet with the associated database. This class cannot be inherited.
	/// </summary>
	public sealed class OleDbCommandBuilder : AbstractDbCommandBuilder
	{
		#region Fields

		OleDbDataAdapter adapter;
		string quotePrefix;
		string quoteSuffix;

		#endregion // Fields

		#region Constructors
		
		public OleDbCommandBuilder ()
		{
			adapter = null;
			quotePrefix = String.Empty;
			quoteSuffix = String.Empty;
		}

		public OleDbCommandBuilder (OleDbDataAdapter adapter) 
			: this ()
		{
			this.adapter = adapter;
		}

		#endregion // Constructors

		#region Properties

		public OleDbDataAdapter DataAdapter {
			get {
				return adapter;
			}
			set {
				adapter = value;
			}
		}

		public string QuotePrefix {
			get {
				return quotePrefix;
			}
			set {
				quotePrefix = value;
			}
		}

		public string QuoteSuffix {
			get {
				return quoteSuffix;
			}
			set {
				quoteSuffix = value;
			}
		}

		#endregion // Properties

		#region Methods

		public static void DeriveParameters (OleDbCommand command) 
		{
			DeriveParameters((AbstractDbCommand)command);
		}

		protected override void Dispose (bool disposing) 
		{
			base.Dispose ();	
		}

		public new OleDbCommand GetDeleteCommand ()
		{
			return (OleDbCommand) base.GetDeleteCommand ();
		}

		public new OleDbCommand GetInsertCommand ()
		{
			return (OleDbCommand) base.GetInsertCommand ();
		}

		public new OleDbCommand GetUpdateCommand ()
		{
			return (OleDbCommand) base.GetUpdateCommand ();
		}

		[MonoTODO]
		public void RefreshSchema ()
		{
			throw new NotImplementedException ();
		}

#if NET_2_0

		[MonoNotSupported("")]
		public string QuoteIdentifier (string unquotedIdentifier, OleDbConnection connection) 
		{
			throw new NotImplementedException ();
		}

		[MonoNotSupported ("")]
		public string UnquoteIdentifier (string quotedIdentifier, OleDbConnection connection)
		{
			throw new NotImplementedException ();
		}

#endif
		#endregion // Methods
	}
}
