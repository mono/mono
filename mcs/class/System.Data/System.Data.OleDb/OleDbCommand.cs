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

#if NET_2_0
using System.Data.ProviderBase;
using System.Data;
#endif

namespace System.Data.OleDb
{
	/// <summary>
	/// Represents an SQL statement or stored procedure to execute against a data source.
	/// </summary>
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.OleDbCommandDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
	[ToolboxItemAttribute ("System.Drawing.Design.ToolboxItem, "+ Consts.AssemblySystem_Drawing)]
	public sealed class OleDbCommand : 
#if NET_2_0
	DbCommandBase
#else
	Component
#endif
	, ICloneable, IDbCommand
	{
		#region Fields

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

		#endregion // Fields

		#region Constructors

		public OleDbCommand ()
	        {
			commandText = String.Empty;
			timeout = 30; // default timeout per .NET
			commandType = CommandType.Text;
			connection = null;
			parameters = new OleDbParameterCollection ();
			transaction = null;
			designTimeVisible = false;
			dataReader = null;
			behavior = CommandBehavior.Default;
			gdaCommand = IntPtr.Zero;
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

		public OleDbCommand (string cmdText,
				     OleDbConnection connection,
				     OleDbTransaction transaction) : this (cmdText, connection)
		{
			this.transaction = transaction;
		}

		#endregion // Constructors

		#region Properties
	
		[DataCategory ("Data")]
		[DefaultValue ("")]
                [DataSysDescriptionAttribute ("Command text to execute")]
                [EditorAttribute ("Microsoft.VSDesigner.Data.ADO.Design.OleDbCommandTextEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public string CommandText 
		{
			get {
				return commandText;
			}
			set { 
				commandText = value;
			}
		}

		[DataSysDescriptionAttribute ("Time to wait for command to execute")]
		[DefaultValue (30)]
		public int CommandTimeout {
			get {
				return timeout;
			}
			set {
				timeout = value;
			}
		}

		[DataCategory ("Data")]
                [DefaultValue ("Text")]
		[DataSysDescriptionAttribute ("How to interpret the CommandText")]
		[RefreshPropertiesAttribute (RefreshProperties.All)]
		public CommandType CommandType { 
			get {
				return commandType;
			}
			set {
				commandType = value;
			}
		}

		[DataCategory ("Behavior")]
		[DataSysDescriptionAttribute ("Connection used by the command")]
		[DefaultValue (null)]
		[EditorAttribute ("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public OleDbConnection Connection { 
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
		public bool DesignTimeVisible { 
			get {
				return designTimeVisible;
			}
			set {
				designTimeVisible = value;
			}
		}

		[DataCategory ("Data")]
		[DataSysDescriptionAttribute ("The parameters collection")]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
		public OleDbParameterCollection Parameters {
			get {
				return parameters;
			}
		}
		
		[BrowsableAttribute (false)]
		[DataSysDescriptionAttribute ("The transaction used by the command")]
		[DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public OleDbTransaction Transaction {
			get {
				return transaction;
			}
			set {
				transaction = value;
			}
		}

		[DataCategory ("Behavior")]
		[DefaultValue (UpdateRowSource.Both)]
		[DataSysDescriptionAttribute ("When used by a DataAdapter.Update, how command results are applied to the current DataRow")]
		public UpdateRowSource UpdatedRowSource { 
			[MonoTODO]
			get {
				throw new NotImplementedException ();
			}
			[MonoTODO]
			set {
				throw new NotImplementedException ();
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

		IDataParameterCollection IDbCommand.Parameters  {
			get {
				return Parameters;
			}
		}

		IDbTransaction IDbCommand.Transaction  {
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
		public void Cancel () 
		{
			throw new NotImplementedException ();
		}

		public OleDbParameter CreateParameter ()
		{
			return new OleDbParameter ();
		}

		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}
		
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
			throw new NotImplementedException ();
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
				libgda.gda_command_set_text (gdaCommand, commandText);
				libgda.gda_command_set_command_type (gdaCommand, type);
			} else {
				gdaCommand = libgda.gda_command_new (commandText, type, 0);
			}

			//libgda.gda_command_set_transaction 
		}
		
		public int ExecuteNonQuery ()
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

		public OleDbDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		public OleDbDataReader ExecuteReader (CommandBehavior behavior)
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
		
		public object ExecuteScalar ()
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

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();	
		}

		[MonoTODO]
		public void Prepare ()
		{
			throw new NotImplementedException ();	
		}

		public void ResetCommandTimeout ()
		{
			timeout = 30;
		}
		
#if NET_2_0
		[MonoTODO]
		protected override DbParameter CreateDbParameter ()
		{
			throw new NotImplementedException ();	
		}
		
		[MonoTODO]
		protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior)
		{
			throw new NotImplementedException ();	
		}
		
		[MonoTODO]
		protected override DbConnection DbConnection {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		protected override DbParameterCollection DbParameterCollection {
			get { throw new NotImplementedException (); }
		}
		
		[MonoTODO]
		protected override DbTransaction DbTransaction {
			get { throw new NotImplementedException (); }
			set { throw new NotImplementedException (); }
		}
#endif

		#endregion
	}
}
