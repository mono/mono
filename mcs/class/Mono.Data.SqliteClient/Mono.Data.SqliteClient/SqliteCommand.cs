// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteCommand.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//

using System;
using System.Runtime.InteropServices;
using System.Data;

namespace Mono.Data.SqliteClient {
        public class SqliteCommand : IDbCommand
        {
                SqliteConnection parent_conn;
//                SqliteTransaction transaction;
                IDbTransaction transaction;
                string sql;
                int timeout;
                CommandType type;
                UpdateRowSource upd_row_source;
                SqliteParameterCollection sql_params;

                public SqliteCommand ()
                {
                        sql = "";
                        sql_params = new SqliteParameterCollection ();
                }

                public SqliteCommand (string sqlText, SqliteConnection dbConn)
                {
                        sql = sqlText;
                        parent_conn = dbConn;
                        sql_params = new SqliteParameterCollection ();
                }

                public SqliteCommand (string sqlText, SqliteConnection dbConn, IDbTransaction trans)
                {
                        sql = sqlText;
                        parent_conn = dbConn;
                        transaction = trans;
                        sql_params = new SqliteParameterCollection ();
                }

                public void Dispose ()
                {
                }

                public string CommandText {
                        get {
                                return sql;
                        }
                        set {
                                sql = value;
                        }
                }

                // note that we could actually implement
                // a timeout with sqlite, but setting up a signal to interrupt us after
                // a certain amount of time, but it's probably not worth the effort
                public int CommandTimeout {
                        get {
                                return timeout;
                        }
                        set {
                                timeout = value;
                        }
                }

                IDbConnection IDbCommand.Connection {
                        get {
                                return parent_conn;
                        }
                        set {
                                if (!(value is SqliteConnection)) {
                                        throw new InvalidOperationException ("Can't set Connection to something other than a SqliteConnection");
                                }
                                parent_conn = (SqliteConnection) value;
                        }
                }

                public SqliteConnection Connection {
                        get {
                                return parent_conn;
                        }
                        set {
                                parent_conn = value;
                        }
                }

                public CommandType CommandType {
                        get {
                                return type;
                        }
                        set {
                                type = value;
                        }
                }

                IDataParameterCollection IDbCommand.Parameters {
                        get {
                                return Parameters;
                        }
                }

                public SqliteParameterCollection Parameters {
                        get {
                                return sql_params;
                        }
                }

                public IDbTransaction Transaction {
                        get {
                                return transaction;
                        }
                        set {
                                transaction = value;
                        }
                }

                public UpdateRowSource UpdatedRowSource {
                        get {
                                return upd_row_source;
                        }
                        set {
                                upd_row_source = value;
                        }
                }

                public void Prepare ()
                {
                }

                public void Cancel ()
                {
                }

                IDbDataParameter IDbCommand.CreateParameter ()
                {
                        return CreateParameter ();
                }

                public SqliteParameter CreateParameter ()
                {
                        return new SqliteParameter ();
                }

                public int ExecuteNonQuery ()
                {
                        int rows_affected;
                        SqliteDataReader r = ExecuteReader (CommandBehavior.Default, false, out rows_affected);
                        return rows_affected;
                }

                public object ExecuteScalar ()
                {
                        SqliteDataReader r = ExecuteReader ();
                        if (r == null || !r.Read ()) {
                                return null;
                        }
                        object o = r[0];
                        r.Close ();
                        return o;
                }

                IDataReader IDbCommand.ExecuteReader ()
                {
                        return ExecuteReader ();
                }

                IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
                {
                        return ExecuteReader (behavior);
                }

                public SqliteDataReader ExecuteReader ()
                {
                        return ExecuteReader (CommandBehavior.Default);
                }

                public SqliteDataReader ExecuteReader (CommandBehavior behavior)
                {
                        int r;
                        return ExecuteReader (behavior, true, out r);
                }

                public SqliteDataReader ExecuteReader (CommandBehavior behavior, bool want_results, out int rows_affected)
                {
                        SqliteDataReader reader = null;
                        SqliteError err;

                        parent_conn.StartExec ();

                        try {
                                if (want_results) {
                                        reader = new SqliteDataReader (this);
                                        err = sqlite_exec (parent_conn.Handle,
                                                           sql,
                                                           new SqliteCallbackFunction (reader.SqliteCallback),
                                                           IntPtr.Zero,
                                                           IntPtr.Zero);
                                        reader.ReadingDone ();
                                } else {
                                        err = sqlite_exec (parent_conn.Handle,
                                                           sql,
                                                           null,
                                                           IntPtr.Zero,
                                                           IntPtr.Zero);
                                }
                        } finally {
                                parent_conn.EndExec ();
                        }

                        if (err != SqliteError.OK)
                                throw new ApplicationException ("Sqlite error " + err);

                        rows_affected = NumChanges ();
                        return reader;
                }


                internal int NumChanges () {
                        return sqlite_changes (parent_conn.Handle);
                }

                internal unsafe delegate int SqliteCallbackFunction (ref object o, int argc, sbyte **argv, sbyte **colnames);

                [DllImport("sqlite")]
                static extern SqliteError sqlite_exec (IntPtr handle, string sql, SqliteCallbackFunction callback,
                                                       IntPtr user_data, IntPtr errstr_ptr);

                [DllImport("sqlite")]
                static extern int sqlite_changes (IntPtr handle);

                internal enum SqliteError : int {
                        OK,
                        Error,
                        Internal,
                        Perm,
                        Abort,
                        Busy,
                        Locked,
                        NoMem,
                        ReadOnly,
                        Interrupt,
                        IOErr,
                        Corrupt,
                        NotFound,
                        Full,
                        CantOpen,
                        Protocol,
                        Empty,
                        Schema,
                        TooBig,
                        Constraint,
                        Mismatch,
                        Misuse
                }
        }
}
