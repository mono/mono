// -*- c-basic-offset: 8; inent-tabs-mode: nil -*-
//
//  SqliteConnection.cs
//
//  Author(s): Vladimir Vukicevic  <vladimir@pobox.com>
//
//  Copyright (C) 2002  Vladimir Vukicevic
//

using System;
using System.Runtime.InteropServices;
using System.Data;

namespace Mono.Data.SqliteClient
{
        public class SqliteConnection : IDbConnection
        {
                string conn_str;
                string db_file;
                int db_mode;
                IntPtr sqlite_handle;

                ConnectionState state;

                public SqliteConnection ()
                {
                        db_file = null;
                        db_mode = 0644;
                        state = ConnectionState.Closed;
                        sqlite_handle = IntPtr.Zero;
                }

                public SqliteConnection (string connstring)
                        : this ()
                {
                        ConnectionString = connstring;
                }

                public void Dispose ()
                {
                        Close ();
                }

                public string ConnectionString
                {
                        get {
                                return conn_str;
                        }
                        set {
                                if (value == null) {
                                        Close ();
                                        conn_str = null;
                                        return;
                                }

                                if (value != conn_str) {
                                        Close ();
                                        conn_str = value;

                                        db_file = null;
                                        db_mode = 0644;

                                        string[] conn_pieces = value.Split (',');
                                        foreach (string piece in conn_pieces) {
                                                piece.Trim ();
                                                string[] arg_pieces = piece.Split ('=');
                                                if (arg_pieces.Length != 2) {
                                                        throw new InvalidOperationException ("Invalid connection string");
                                                }
                                                string token = arg_pieces[0].ToLower ();
                                                string tvalue = arg_pieces[1];
                                                string tvalue_lc = arg_pieces[1].ToLower ();
                                                if (token == "uri") {
                                                        if (tvalue_lc.StartsWith ("file://")) {
                                                                db_file = tvalue.Substring (6);
                                                        } else if (tvalue_lc.StartsWith ("file:")) {
                                                                db_file = tvalue.Substring (5);
                                                        } else if (tvalue_lc.StartsWith ("/")) {
                                                                db_file = tvalue;
                                                        } else {
                                                                throw new InvalidOperationException ("Invalid connection string: invalid URI");
                                                        }
                                                } else if (token == "mode") {
                                                        db_mode = Convert.ToInt32 (tvalue);
                                                }
                                        }

                                        if (db_file == null) {
                                                throw new InvalidOperationException ("Invalid connection string: no URI");
                                        }
                                }
                        }
                }

                public int ConnectionTimeout
                {
                        get {
                                return 0;
                        }
                }

                public string Database
                {
                        get {
                                return db_file;
                        }
                }

                public ConnectionState State
                {
                        get {
                                return state;
                        }
                }

                internal IntPtr Handle
                {
                        get {
                                return sqlite_handle;
                        }
                }

                public void Open ()
                {
                        if (conn_str == null) {
                                throw new InvalidOperationException ("No database specified");
                        }

                        if (state != ConnectionState.Closed) {
                                return;
                        }

                        string errmsg;
                        sqlite_handle = sqlite_open (db_file, db_mode, out errmsg);

                        if (errmsg != null) {
                                throw new ApplicationException (errmsg);
                        }

                        state = ConnectionState.Open;
                }

                public void Close ()
                {
                        if (state != ConnectionState.Open) {
                                return;
                        }

                        state = ConnectionState.Closed;

                        sqlite_close (sqlite_handle);
                        sqlite_handle = IntPtr.Zero;
                }

		public void ChangeDatabase (string databaseName)
                {
                        throw new NotImplementedException ();
                }

                IDbCommand IDbConnection.CreateCommand ()
                {
                        return CreateCommand ();
                }

                public SqliteCommand CreateCommand ()
                {
                        return new SqliteCommand (null, this);
                }

                public IDbTransaction BeginTransaction ()
                {
                    return null;
                }

                public IDbTransaction BeginTransaction (IsolationLevel il)
                {
                    return null;
                }

                internal void StartExec ()
                {
                        // use a mutex here
                        state = ConnectionState.Executing;
                }

                internal void EndExec ()
                {
                        state = ConnectionState.Open;
                }

                [DllImport("sqlite")]
                static extern IntPtr sqlite_open (string dbname, int db_mode, out string errstr);

                [DllImport("sqlite")]
                static extern void sqlite_close (IntPtr sqlite_handle);
        }
}
