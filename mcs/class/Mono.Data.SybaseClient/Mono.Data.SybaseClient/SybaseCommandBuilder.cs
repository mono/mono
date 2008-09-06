//
// Mono.Data.SybaseClient.SybaseCommandBuilder.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2002
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

namespace Mono.Data.SybaseClient {
	[MonoTODO("Implement getting KeyInfo in SybaseDataReader before implementing SybaseCommandBuilder.")]
	public sealed class SybaseCommandBuilder : Component 
	{
		#region Constructors
		SybaseDataAdapter adapter;

		public SybaseCommandBuilder () 
		{
			adapter = null;
		}

		public SybaseCommandBuilder (SybaseDataAdapter adapter) : this ()
		{
			DataAdapter = adapter;
		}
		
		#endregion // Constructors

		#region Properties

		public SybaseDataAdapter DataAdapter {
			get { return adapter; }
			set { 
				if (adapter != null)
					adapter.RowUpdating -= new SybaseRowUpdatingEventHandler (RowUpdatingHandler);

				adapter = value; 

				if (adapter != null)
					adapter.RowUpdating += new SybaseRowUpdatingEventHandler (RowUpdatingHandler);
			}
		}

		[MonoTODO]
		public string QuotePrefix {
			get { throw new NotImplementedException (); } 
			set { throw new NotImplementedException (); }
		}

		[MonoTODO]
		public string QuoteSuffix {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public static void DeriveParameters (SybaseCommand command) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void Dispose(bool disposing) 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseCommand GetDeleteCommand() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseCommand GetInsertCommand() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public SybaseCommand GetUpdateCommand() 
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public void RefreshSchema() 
		{
			throw new NotImplementedException ();
		}

		#endregion // Methods

		#region Event Handlers

		private void RowUpdatingHandler (object sender, SybaseRowUpdatingEventArgs args)
		{
			if (args.Command != null)
				return;
			try {
				switch (args.StatementType) {
				case StatementType.Insert:
					args.Command = GetInsertCommand ();
					break;
				case StatementType.Update:
					args.Command = GetUpdateCommand ();
					break;
				case StatementType.Delete:
					args.Command = GetDeleteCommand ();
					break;
				}
			} catch (Exception e) {
				args.Errors = e;
				args.Status = UpdateStatus.ErrorsOccurred;
			}

		#endregion // Event Handlers
		}
	}
}


