//
// System.Data.Odbc.OdbcCommand
//
// Authors:
//   Brian Ritchie (brianlritchie@hotmail.com)
//
// Copyright (C) Brian Ritchie, 2002
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

using System;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
#if NET_2_0
using System.Data.ProviderBase;
#endif // NET_2_0
using System.Collections;
using System.Runtime.InteropServices;

namespace System.Data.Odbc
{
        /// <summary>
	/// Represents an SQL statement or stored procedure to execute against a data source.
	/// </summary>
	[DesignerAttribute ("Microsoft.VSDesigner.Data.VS.OdbcCommandDesigner, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.ComponentModel.Design.IDesigner")]
        [ToolboxItemAttribute ("System.Drawing.Design.ToolboxItem, "+ Consts.AssemblySystem_Drawing)]
#if NET_2_0
        public sealed class OdbcCommand : DbCommandBase, ICloneable
#else
	public sealed class OdbcCommand : Component, ICloneable, IDbCommand
#endif //NET_2_0
	{
		#region Fields

#if ONLY_1_1
		string commandText;
		int timeout;
		CommandType commandType;
#endif // ONLY_1_1

		OdbcConnection connection;
		OdbcTransaction transaction;
		OdbcParameterCollection _parameters;

		bool designTimeVisible;
		bool prepared=false;
		OdbcDataReader dataReader;
		IntPtr hstmt;
		
		#endregion // Fields

		#region Constructors

		public OdbcCommand ()
		{
			this.CommandText = String.Empty;
			this.CommandTimeout = 30; // default timeout 
			this.CommandType = CommandType.Text;
			Connection = null;
			_parameters = new OdbcParameterCollection ();
			Transaction = null;
			designTimeVisible = false;
			dataReader = null;
		}

		public OdbcCommand (string cmdText) : this ()
		{
			CommandText = cmdText;
		}

		public OdbcCommand (string cmdText, OdbcConnection connection)
			: this (cmdText)
		{
			Connection = connection;
		}

		public OdbcCommand (string cmdText,
				     OdbcConnection connection,
				     OdbcTransaction transaction) : this (cmdText, connection)
		{
			this.Transaction = transaction;
		}

		#endregion // Constructors

		#region Properties

		internal IntPtr hStmt
		{
			get { return hstmt; }
		}
		

#if ONLY_1_1
                [OdbcCategory ("Data")]
                [DefaultValue ("")]
                [OdbcDescriptionAttribute ("Command text to execute")]
                [EditorAttribute ("Microsoft.VSDesigner.Data.Odbc.Design.OdbcCommandTextEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
                [RefreshPropertiesAttribute (RefreshProperties.All)]
		public string CommandText 
		{
			get {
				return commandText;
			}
			set { 
				prepared=false;
				commandText = value;
			}
		}

#else
                [OdbcCategory ("Data")]
                [DefaultValue ("")]
                [OdbcDescriptionAttribute ("Command text to execute")]
                [EditorAttribute ("Microsoft.VSDesigner.Data.Odbc.Design.OdbcCommandTextEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
                [RefreshPropertiesAttribute (RefreshProperties.All)]
		public override string CommandText
		{
			get {
				return base.CommandText;
			}
			set {
				prepared=false;
				base.CommandText = value;
			}
		}
#endif // ONLY_1_1

#if ONLY_1_1
		[OdbcDescriptionAttribute ("Time to wait for command to execute")]
                [DefaultValue (30)]
		public int CommandTimeout {
			get {
				return timeout;
			}
			set {
				timeout = value;
			}
		}

		[OdbcCategory ("Data")]
                [DefaultValue ("Text")]
                [OdbcDescriptionAttribute ("How to interpret the CommandText")]
                [RefreshPropertiesAttribute (RefreshProperties.All)]
		public CommandType CommandType { 
			get {
				return commandType;
			}
			set {
				commandType = value;
			}
		}

		[OdbcCategory ("Behavior")]
                [OdbcDescriptionAttribute ("Connection used by the command")]
                [DefaultValue (null)]
                [EditorAttribute ("Microsoft.VSDesigner.Data.Design.DbConnectionEditor, "+ Consts.AssemblyMicrosoft_VSDesigner, "System.Drawing.Design.UITypeEditor, "+ Consts.AssemblySystem_Drawing )]
		public OdbcConnection Connection { 
			get {
				return connection;
			}
			set {
				connection = value;
			}
		}
#endif // ONLY_1_1

#if NET_2_0
                public new OdbcConnection Connection
                {
                        get { return DbConnection as OdbcConnection; }
                        set { DbConnection = value; }
                }
                
#endif // NET_2_0

#if  ONLY_1_1
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

#endif // ONLY_1_1

		[OdbcCategory ("Data")]
                [OdbcDescriptionAttribute ("The parameters collection")]
                [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Content)]
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcParameterCollection Parameters {
			get {
#if ONLY_1_1
				return _parameters;
#else
                                return base.Parameters as OdbcParameterCollection;
#endif // ONLY_1_1

			}
		}
		
		[BrowsableAttribute (false)]
                [OdbcDescriptionAttribute ("The transaction used by the command")]
                [DesignerSerializationVisibilityAttribute (DesignerSerializationVisibility.Hidden)]
		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcTransaction Transaction {
			get {
				return transaction;
			}
			set {
				transaction = value;
			}
		}

#if ONLY_1_1
		
		[OdbcCategory ("Behavior")]
                [DefaultValue (UpdateRowSource.Both)]
                [OdbcDescriptionAttribute ("When used by a DataAdapter.Update, how command results are applied to the current DataRow")]
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
				Connection = (OdbcConnection) value;
			}
		}
#endif // ONLY_1_1
#if NET_2_0
                protected override DbConnection DbConnection 
                {
                        get { return connection; }
                        set { 
                                connection = (OdbcConnection) value; 
                        }                        
                }

#endif // NET_2_0

#if ONLY_1_1
		IDataParameterCollection IDbCommand.Parameters  {
			get {
				return Parameters;
			}
		}
#else
                protected override DbParameterCollection DbParameterCollection
                {
                        get { return _parameters as DbParameterCollection;}
                }
                
#endif // NET_2_0

#if ONLY_1_1
		IDbTransaction IDbCommand.Transaction  {
			get {
				return (IDbTransaction) Transaction;
			}
			set {
				 if (value is OdbcTransaction)
                                {
                                        Transaction = (OdbcTransaction)value;
                                }
                                else
                                {
                                        throw new ArgumentException ();
                                }
			}
		}
#else
		protected override DbTransaction DbTransaction 
                {
			get { return transaction; }
			set {
                                transaction = (OdbcTransaction)value;
			}
		}
#endif // ONLY_1_1



		#endregion // Properties

		#region Methods

		public
#if NET_2_0
                override
#endif // NET_2_0
                void Cancel () 
		{
			if (hstmt!=IntPtr.Zero)
			{
				OdbcReturn Ret=libodbc.SQLCancel(hstmt);
				if ((Ret!=OdbcReturn.Success) && (Ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLCancel",OdbcHandleType.Stmt,hstmt));
			}
			else
				throw new InvalidOperationException();
		}

#if ONLY_1_1
		IDbDataParameter IDbCommand.CreateParameter ()
		{
			return CreateParameter ();
		}

		public OdbcParameter CreateParameter ()
		{
			return new OdbcParameter ();
		}
#else
                protected override DbParameter CreateDbParameter ()
                {
                        return CreateParameter ();
                }
                
#endif // ONLY_1_1

		
		[MonoTODO]
		protected override void Dispose (bool disposing)
		{
		}
		
		private void ExecSQL(string sql)
		{
			OdbcReturn ret;

			if ((Parameters.Count>0) && !prepared)
				Prepare();
	
			if (prepared)
			{
				ret=libodbc.SQLExecute(hstmt);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLExecute",OdbcHandleType.Stmt,hstmt));
			}
			else
			{
				ret=libodbc.SQLAllocHandle(OdbcHandleType.Stmt, Connection.hDbc, ref hstmt);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLAllocHandle",OdbcHandleType.Dbc,Connection.hDbc));

				ret=libodbc.SQLExecDirect(hstmt, sql, sql.Length);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLExecDirect",OdbcHandleType.Stmt,hstmt));
			}
		}

		public
#if NET_2_0
                override
#endif // NET_2_0
                int ExecuteNonQuery ()
		{
			return ExecuteNonQuery (true);
		}

		private int ExecuteNonQuery (bool freeHandle) 
		{
			int records = 0;
			if (Connection == null)
				throw new InvalidOperationException ();
			if (Connection.State == ConnectionState.Closed)
				throw new InvalidOperationException ();
			// FIXME: a third check is mentioned in .NET docs

			ExecSQL(CommandText);

			// .NET documentation says that except for INSERT, UPDATE and
                        // DELETE  where the return value is the number of rows affected
                        // for the rest of the commands the return value is -1.
                        if ((CommandText.ToUpper().IndexOf("UPDATE")!=-1) ||
                                    (CommandText.ToUpper().IndexOf("INSERT")!=-1) ||
                                    (CommandText.ToUpper().IndexOf("DELETE")!=-1)) {
                                                                                                    
                                        int numrows = 0;
                                        OdbcReturn ret = libodbc.SQLRowCount(hstmt,ref numrows);
                                        records = numrows;
                        }
                        else
                                        records = -1;

			if (freeHandle && !prepared) {
				OdbcReturn ret = libodbc.SQLFreeHandle( (ushort) OdbcHandleType.Stmt, hstmt);
				if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
					throw new OdbcException(new OdbcError("SQLFreeHandle",OdbcHandleType.Stmt,hstmt));
			}
			return records;
		}

		public
#if NET_2_0
                override
#endif // NET_2_0
                void Prepare()
		{
			OdbcReturn ret=libodbc.SQLAllocHandle(OdbcHandleType.Stmt, Connection.hDbc, ref hstmt);
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException(new OdbcError("SQLAllocHandle",OdbcHandleType.Dbc,Connection.hDbc));

			ret=libodbc.SQLPrepare(hstmt, CommandText, CommandText.Length);
			if ((ret!=OdbcReturn.Success) && (ret!=OdbcReturn.SuccessWithInfo)) 
				throw new OdbcException(new OdbcError("SQLPrepare",OdbcHandleType.Stmt,hstmt));

			int i=1;
			foreach (OdbcParameter p in Parameters)
			{
				p.Bind(hstmt, i);
				i++;
			}

			prepared=true;
		}


		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcDataReader ExecuteReader ()
		{
			return ExecuteReader (CommandBehavior.Default);
		}

#if ONLY_1_1
		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}
#else
                protected override DbDataReader ExecuteDbDataReader (CommandBehavior behavior)
                {
                        return ExecuteReader (behavior);
                }
                
#endif // ONLY_1_1

		public
#if NET_2_0
                new
#endif // NET_2_0
                OdbcDataReader ExecuteReader (CommandBehavior behavior)
		{
			ExecuteNonQuery(false);
			dataReader=new OdbcDataReader(this,behavior);
			return dataReader;
		}

#if ONLY_1_1
                IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}
#endif // ONLY_1_1

#if ONLY_1_1
		public object ExecuteScalar ()
		{
			object val = null;
			OdbcDataReader reader=ExecuteReader();
			try
			{
				if (reader.Read ())
					val=reader[0];
			}
			finally
			{
				reader.Close();
			}
			return val;
		}
#endif // ONLY_1_1

		[MonoTODO]
		object ICloneable.Clone ()
		{
			throw new NotImplementedException ();	
		}

		public 
#if NET_2_0
                override
#endif // NET_2_0
                void ResetCommandTimeout ()
		{
			CommandTimeout = 30;
		}

		#endregion
	}
}
