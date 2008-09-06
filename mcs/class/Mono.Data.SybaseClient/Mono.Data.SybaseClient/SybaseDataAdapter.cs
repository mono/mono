//
// Mono.Data.SybaseClient.SybaseDataAdapter.cs
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//   Daniel Morgan (monodanmorg@yahoo.com)
//
// Copyright (C) Tim Coleman, 2002
// Copyright (C) Daniel Morgan, 2008
//
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
using System.ComponentModel;
using System.Data;
using System.Data.Common;

namespace Mono.Data.SybaseClient {
	[DefaultEvent ("RowUpdated")]
	public sealed class SybaseDataAdapter : DbDataAdapter, IDbDataAdapter, ICloneable
	{
		#region Fields
	
#if !NET_2_0
		bool disposed;
#endif
		SybaseCommand deleteCommand;
		SybaseCommand insertCommand;
		SybaseCommand selectCommand;
		SybaseCommand updateCommand;
#if NET_2_0
		int updateBatchSize;
#endif

		static readonly object EventRowUpdated = new object(); 
		static readonly object EventRowUpdating = new object(); 

		#endregion

		#region Constructors
		
		public SybaseDataAdapter () : this ((SybaseCommand) null)
		{
		}

		public SybaseDataAdapter (SybaseCommand selectCommand) 
		{
			SelectCommand = selectCommand;
#if NET_2_0
			UpdateBatchSize = 1;
#endif
		}

		public SybaseDataAdapter (string selectCommandText, SybaseConnection selectConnection) 
			: this (new SybaseCommand (selectCommandText, selectConnection))
		{ 
		}

		public SybaseDataAdapter (string selectCommandText, string selectConnectionString)
			: this (selectCommandText, new SybaseConnection (selectConnectionString))
		{
		}

		#endregion

		#region Properties

		public new SybaseCommand DeleteCommand {
			get { return deleteCommand; }
			set { deleteCommand = value; }
		}

		public new SybaseCommand InsertCommand {
			get { return insertCommand; }
			set { insertCommand = value; }
		}

		public new SybaseCommand SelectCommand {
			get { return selectCommand; }
			set { selectCommand = value; }
		}

		public new SybaseCommand UpdateCommand {
			get { return updateCommand; }
			set { updateCommand = value; }
		}

		IDbCommand IDbDataAdapter.DeleteCommand {
			get { return DeleteCommand; }
			set { DeleteCommand = (SybaseCommand) value; }
		}

		IDbCommand IDbDataAdapter.InsertCommand {
			get { return InsertCommand; }
			set { InsertCommand = (SybaseCommand) value; }
		}

		IDbCommand IDbDataAdapter.SelectCommand {
			get { return SelectCommand; }
			set { SelectCommand = (SybaseCommand) value; }
		}

		IDbCommand IDbDataAdapter.UpdateCommand {
			get { return UpdateCommand; }
			set { UpdateCommand = (SybaseCommand) value; }
		}

		ITableMappingCollection IDataAdapter.TableMappings {
			get { return TableMappings; }
		}

#if NET_2_0
		public override int UpdateBatchSize {
			get { return updateBatchSize; }
			set {
				if (value < 0)
					throw new ArgumentOutOfRangeException ("UpdateBatchSize");
				updateBatchSize = value; 
			}
		}
#endif

#if !NET_2_0
		protected override void Dispose (bool disposing)
		{
			if (!disposed) {
				if (disposing) {
					// Release managed resources
				}
				// Release unmanaged resources
				disposed = true;
			}
			base.Dispose (disposing);
		}
#endif

		#endregion // Properties

		#region Methods

		protected override RowUpdatedEventArgs CreateRowUpdatedEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SybaseRowUpdatedEventArgs (dataRow, command, statementType, tableMapping);
		}


		protected override RowUpdatingEventArgs CreateRowUpdatingEvent (DataRow dataRow, IDbCommand command, StatementType statementType, DataTableMapping tableMapping) 
		{
			return new SybaseRowUpdatingEventArgs (dataRow, command, statementType, tableMapping);
		}

		protected override void OnRowUpdated (RowUpdatedEventArgs value) 
		{
         		SybaseRowUpdatedEventHandler handler = (SybaseRowUpdatedEventHandler) Events[EventRowUpdated];
			if ((handler != null) && (value is SybaseRowUpdatedEventArgs))
            			handler(this, (SybaseRowUpdatedEventArgs) value);
		}

		protected override void OnRowUpdating (RowUpdatingEventArgs value) 
		{
         		SybaseRowUpdatingEventHandler handler = (SybaseRowUpdatingEventHandler) Events[EventRowUpdating];
			if ((handler != null) && (value is SybaseRowUpdatingEventArgs))
            			handler(this, (SybaseRowUpdatingEventArgs) value);
		}

#if NET_2_0
		[MonoTODO]
		protected override int AddToBatch (IDbCommand command)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void ClearBatch ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override int ExecuteBatch ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override IDataParameter GetBatchedParameter (int commandIdentifier, int  parameterIndex)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void InitializeBatching ()
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		protected override void TerminateBatching ()
		{
			throw new NotImplementedException ();
		}
#endif

		#endregion // Methods

		#region Events and Delegates

		public event SybaseRowUpdatedEventHandler RowUpdated {
			add { Events.AddHandler (EventRowUpdated, value); }
			remove { Events.RemoveHandler (EventRowUpdated, value); }
		}

		public event SybaseRowUpdatingEventHandler RowUpdating {
			add { Events.AddHandler (EventRowUpdating, value); }
			remove { Events.RemoveHandler (EventRowUpdating, value); }
		}

		#endregion // Events and Delegates

	}
}
