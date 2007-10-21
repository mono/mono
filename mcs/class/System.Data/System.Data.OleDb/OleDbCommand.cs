//
// System.Data.OleDb.OleDbCommand
//
// Authors:
//   Rodrigo Moya (rodrigo@ximian.com)
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Rodrigo Moya, 2002
// Copyright (C) Tim Coleman, 2002
//

//
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
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
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Data.OleDb
{
	/// <summary>
	/// Represents an SQL statement or stored procedure to execute against a data source.
	/// </summary>
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.OleDbCommandDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemAttribute ("System.Drawing.Design.ToolboxItem, "+ Consts.AssemblySystem_Drawing)]
#if NET_2_0
	[DefaultEvent( "RecordsAffected")]
#endif
	public sealed class OleDbCommand : 
#if NET_2_0
	DbCommand
#else
	Component
#endif
	, ICloneable, IDbCommand
	{
		#region Fields

		const int DEFAULT_COMMAND_TIMEOUT = 30;

		string commandText;
		int timeout;
		CommandType commandType;
		OleDbConnection connection;
		OleDbParameterCollection parameters;
		OleDbTransaction transaction;
		bool designTimeVisible;
		OleDbDataReader dataReader;
		CommandBehavior behavior;
		IntPtr gdaCommand;
		UpdateRowSource updatedRowSource;

		bool disposed;
		
		#endregion // Fields

		#region Constructors

		public OleDbCommand ()
		{
			timeout = DEFAULT_COMMAND_TIMEOUT;
			commandType = CommandType.Text;
			parameters = new OleDbParameterCollection ();
			behavior = CommandBehavior.Default;
			gdaCommand = IntPtr.Zero;
			designTimeVisible = true;
			this.updatedRowSource = UpdateRowSource.Both;
		}

		public OleDbCommand (string cmdText) : this ()
		{
			CommandText = cmdText;
		}

		public OleDbCommand (string cmdText, OleDbConnection connection)
			: this (cmdText)
		{
			Connection = connection;
		}

		public OleDbCommand (string cmdText, OleDbConnection connection,
			OleDbTransaction transaction) : this (cmdText, connection)
		{
			this.transaction = transaction;
		}

		#endregion // Constructors

		#region Properties
	
		[DataCategory ("Data")]
		[DefaultValue ("")]
#if !NET_2_0
		[DataSysDescriptionAttribute ("Command text to execute.")]
#endif
		[EditorAttribute ("Microsoft.VSDesigner.Data.ADO.Design.OleDbCommandTextEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing)]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public 
#if NET_2_0
		override
#endif
		string CommandText {
			get {
				if (commandText == null)
					return string.Empty;
				return commandText;
			}
			set {
				commandText = value;
			}
		}

#if !NET_2_0
		[DataSysDescriptionAttribute ("Time to wait for command to execute.")]
		[DefaultValue (DEFAULT_COMMAND_TIMEOUT)]
#endif
		public
#if NET_2_0
		override
#endif
		int CommandTimeout {
			get {
				return timeout;
			}
			set {
				timeout = value;
			}
		}

		[DataCategory ("Data")]
		[DefaultValue ("Text")]
#if !NET_2_0
		[DataSysDescriptionAttribute ("How to interpret the CommandText.")]
#endif
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public
#if NET_2_0
		override
#endif
		CommandType CommandType {
			get {
				return commandType;
			}
			set {
				commandType = value;
			}
		}

		[DataCategory ("Behavior")]
#if !NET_2_0
		[DataSysDescriptionAttribute ("Connection used by the command.")]
#endif
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public new OleDbConnection Connection {
			get {
				return connection;
			}
			set {
				connection = value;
			}
		}
		
		[BrowsableAttribute (false)]
		[DesignOnlyAttribute (true)]
		[DefaultValue (true)]
#if NET_2_0
		[EditorBrowsable(EditorBrowsableState.Never)]
#endif
		public
#if NET_2_0
		override
#endif
		bool DesignTimeVisible {
			get {
				return designTimeVisible;
			}
			set {
				designTimeVisible = value;
			}
		}

		[DataCategory ("Data")]
#if ONLY_1_1
		[DataSysDescriptionAttribute ("The parameters collection.")]
#endif
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
		public new OleDbParameterCollection Parameters {
			get { return parameters; }
			internal set { parameters = value; }
		}

		[BrowsableAttribute (false)]
#if ONLY_1_1
		[DataSysDescriptionAttribute ("The transaction used by the command.")]
#endif
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public new OleDbTransaction Transaction {
			get {
				return transaction;
			}
			set {
				transaction = value;
			}
		}

		[DataCategory ("Behavior")]
		[DefaultValue (UpdateRowSource.Both)]
#if !NET_2_0
		[DataSysDescriptionAttribute ("When used by a DataAdapter.Update, how command results are applied to the current DataRow.")]
#endif
		[MonoTODO]
		public
#if NET_2_0
		override
#endif
		UpdateRowSource UpdatedRowSource {
			get { return updatedRowSource; }
			set {
				ExceptionHelper.CheckEnumValue (typeof (UpdateRowSource), value);
				updatedRowSource = value;
			}
		}

		IDbConnection IDbCommand.Connection {
			get {
				return Connection;
			}
			set {
				Connection = (OleDbConnection) value;
			}
		}

		IDataParameterCollection IDbCommand.Parameters {
			get {
				return Parameters;
			}
		}

		IDbTransaction IDbCommand.Transaction {
			get {
				return Transaction;
			}
			set {
				Transaction = (OleDbTransaction) value;
			}
		}

		#endregion // Properties

		#region Methods

		[MonoTODO]
		public 
#if NET_2_0
		override 
#endif
		void Cancel () 
		{
			throw new NotImplementedException ();
		}

		public new OleDbParameter CreateParameter ()
		{
			return new OleDbParameter ();
		}

#if !NET_2_0
		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}
#endif
		
		protected override void Dispose (bool disposing)
		{
			if (disposed)
				return;
			
			Connection = null;
			Transaction = null;
			disposed = true;
		}

		private void SetupGdaCommand ()
		{
			GdaCommandType type;
			
			switch (commandType) {
			case CommandType.TableDirect :
				type = GdaCommandType.Table;
				break;
			case CommandType.StoredProcedure :
				type = GdaCommandType.Procedure;
				break;
			case CommandType.Text :
			default :
				type = GdaCommandType.Sql;
				break;
			}
			
			if (gdaCommand != IntPtr.Zero) {
				libgda.gda_command_set_text (gdaCommand, CommandText);
				libgda.gda_command_set_command_type (gdaCommand, type);
			} else {
				gdaCommand = libgda.gda_command_new (CommandText, type, 0);
			}

			//libgda.gda_command_set_transaction 
		}

		public 
#if NET_2_0
		override
#endif
		int ExecuteNonQuery ()
		{
			if (connection == null)
				throw new InvalidOperationException ("connection == null");
			if (connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ("State == Closed");
			// FIXME: a third check is mentioned in .NET docs

			IntPtr gdaConnection = connection.GdaConnection;
			IntPtr gdaParameterList = parameters.GdaParameterList;

			SetupGdaCommand ();
			return libgda.gda_connection_execute_non_query (gdaConnection,
									(IntPtr) gdaCommand,
									gdaParameterList);
		}

		public new OleDbDataReader ExecuteReader ()
		{
			return ExecuteReader (behavior);
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		public new OleDbDataReader ExecuteReader (CommandBehavior behavior)
		{
			ArrayList results = new ArrayList ();
			IntPtr rs_list;
			GdaList glist_node;

			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException ("State != Open");

			this.behavior = behavior;

			IntPtr gdaConnection = connection.GdaConnection;
			IntPtr gdaParameterList = parameters.GdaParameterList;

			/* execute the command */
			SetupGdaCommand ();
			rs_list = libgda.gda_connection_execute_command (
				gdaConnection,
				gdaCommand,
				gdaParameterList);
			if (rs_list != IntPtr.Zero) {
				glist_node = (GdaList) Marshal.PtrToStructure (rs_list, typeof (GdaList));

				while (glist_node != null) {
					results.Add (glist_node.data);
					if (glist_node.next == IntPtr.Zero)
						break;

					glist_node = (GdaList) Marshal.PtrToStructure (glist_node.next,
						typeof (GdaList));
				}
				dataReader = new OleDbDataReader (this, results);
				dataReader.NextResult ();
			}

			return dataReader;
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}
		
		public
#if NET_2_0
		override
#endif
		object ExecuteScalar ()
		{
			SetupGdaCommand ();
			OleDbDataReader reader = ExecuteReader ();
			if (reader == null) {
				return null;
			}
			if (!reader.Read ()) {
				reader.Close ();
				return null;
			}
			object o = reader.GetValue (0);
			reader.Close ();
			return o;
		}

#if NET_2_0
		public
#else
		internal
#endif
		OleDbCommand Clone ()
		{
			OleDbCommand command = new OleDbCommand ();
			command.CommandText = this.CommandText;
			command.CommandTimeout = this.CommandTimeout;
			command.CommandType = this.CommandType;
			command.Connection = this.Connection;
			command.DesignTimeVisible = this.DesignTimeVisible;
			command.Parameters = this.Parameters;
			command.Transaction = this.Transaction;
			return command;
		}

		object ICloneable.Clone ()
		{
			return Clone ();
		}

		[MonoTODO]
		public 
#if NET_2_0
		override
#endif
		void Prepare ()
		{
			throw new NotImplementedException ();
		}

		public void ResetCommandTimeout ()
		{
			timeout = DEFAULT_COMMAND_TIMEOUT;
		}
		
#if NET_2_0
		protected override DbParameter CreateDbParameter ()
		{
			return (DbParameter) CreateParameter ();
		}
		
		protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior)
		{
			return (DbDataReader) ExecuteReader (behavior);
		}
		
		protected override DbConnection DbConnection {
			get { return Connection; }
			set { Connection = (OleDbConnection) value; }
		}
		
		protected override DbParameterCollection DbParameterCollection {
			get { return Parameters; }
		}
		
		protected override DbTransaction DbTransaction {
			get { return Transaction; }
			set { Transaction = (OleDbTransaction) value; }
		}
#endif

		#endregion
	}
}
