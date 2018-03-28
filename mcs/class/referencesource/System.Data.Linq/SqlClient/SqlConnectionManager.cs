using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq.Expressions;
using System.IO;
using System.Reflection;
using System.Text;
using System.Transactions;

namespace System.Data.Linq.SqlClient {
    using System.Data.Linq;
    using System.Data.Linq.Provider;

    internal class SqlConnectionManager : IConnectionManager {
        private IProvider provider;
        private DbConnection connection;
        private bool autoClose;
        private bool disposeConnection;  // should we dispose this connection when the context is disposed?
        private DbTransaction transaction;
        private Transaction systemTransaction;
        private SqlInfoMessageEventHandler infoMessagehandler;
        private List<IConnectionUser> users;
        private int maxUsers;

        internal SqlConnectionManager(IProvider provider, DbConnection con, int maxUsers, bool disposeConnection) {
            this.provider = provider;
            this.connection = con;
            this.maxUsers = maxUsers;
            this.infoMessagehandler = new SqlInfoMessageEventHandler(this.OnInfoMessage);
            this.users = new List<IConnectionUser>(maxUsers);
            this.disposeConnection = disposeConnection;
        }


        public DbConnection UseConnection(IConnectionUser user) {
            if (user == null) {
                throw Error.ArgumentNull("user");
            }
            if (this.connection.State == ConnectionState.Closed) {
                this.connection.Open();
                this.autoClose = true;
                this.AddInfoMessageHandler();
                if (System.Transactions.Transaction.Current != null) {
                    System.Transactions.Transaction.Current.TransactionCompleted += this.OnTransactionCompleted;
                }
            }
            if (this.transaction == null && System.Transactions.Transaction.Current != null &&
                        System.Transactions.Transaction.Current != systemTransaction) {
                this.ClearConnection();
                systemTransaction = System.Transactions.Transaction.Current;
                this.connection.EnlistTransaction(System.Transactions.Transaction.Current);
            }

            if (this.users.Count == this.maxUsers) {
                this.BootUser(this.users[0]);
            }
            this.users.Add(user);
            return this.connection;
        }

        private void BootUser(IConnectionUser user) {
            bool saveAutoClose = this.autoClose;
            this.autoClose = false;
            int index = this.users.IndexOf(user);
            if (index >= 0) {
                this.users.RemoveAt(index);
            }
            user.CompleteUse();
            this.autoClose = saveAutoClose;
        }

        internal DbConnection Connection {
            get { return this.connection; }
        }

        internal int MaxUsers {
            get { return this.maxUsers; }
        }

        internal void DisposeConnection() {
            // only close this guy if we opened it in the first place
            if (this.autoClose) {
                this.CloseConnection();
            }

            // If we created the connection, we need to dispose it even if the user explicitly
            // opened it using the Connection property on the context.
            if (this.connection != null && this.disposeConnection) {
                this.connection.Dispose();
                this.connection = null;
            }
        }

        internal void ClearConnection() {
            while (this.users.Count > 0) {
                this.BootUser(this.users[0]);
            }
        }

        internal bool AutoClose {
            get { return this.autoClose; }
            set { this.autoClose = value; }
        }

        internal DbTransaction Transaction {
            get { return this.transaction; }
            set {
                if (value != this.transaction) {
                    if (value != null) {
                        if (this.connection != value.Connection) {
                            throw Error.TransactionDoesNotMatchConnection();
                        }
                    }
                    this.transaction = value;
                }
            }
        }

        public void ReleaseConnection(IConnectionUser user) {
            if (user == null) {
                throw Error.ArgumentNull("user");
            }
            int index = this.users.IndexOf(user);
            if (index >= 0) {
                this.users.RemoveAt(index);
            }
            if (this.users.Count == 0 && this.autoClose && this.transaction == null && System.Transactions.Transaction.Current == null) {
                this.CloseConnection();
            }
        }

        private void CloseConnection() {
            if (this.connection != null && this.connection.State != ConnectionState.Closed) {
                this.connection.Close();
            }
            this.RemoveInfoMessageHandler();
            this.autoClose = false;
        }

        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs args) {
            if (this.provider.Log != null) {
                this.provider.Log.WriteLine(Strings.LogGeneralInfoMessage(args.Source, args.Message));
            }
        }

        private void OnTransactionCompleted(object sender, System.Transactions.TransactionEventArgs args) {
            if (this.users.Count == 0 && this.autoClose) {
                this.CloseConnection();
            }
        }

        private void AddInfoMessageHandler() {
            SqlConnection scon = this.connection as SqlConnection;
            if (scon != null) {
                scon.InfoMessage += this.infoMessagehandler;
            }
        }

        private void RemoveInfoMessageHandler() {
            SqlConnection scon = this.connection as SqlConnection;
            if (scon != null) {
                scon.InfoMessage -= this.infoMessagehandler;
            }
        }
    }
}
