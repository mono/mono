namespace System.Data.SqlClient
{

    /**
     * <p>Title: </p>
     * <p>Description: </p>
     * <p>Copyright: Copyright (c) 2002</p>
     * <p>Company: </p>
     * @author unascribed
     * @version 1.0
     */

    using System.Data;


    /*
    * Current Limitations:
    * 1. Rollback(String savePoint) - not implemented.
    * 2. Save(String) - not implemented.
    */

    public sealed class SqlTransaction : System.Data.Common.AbstractTransaction
    {
    

        internal SqlTransaction(IsolationLevel isolationLevel, SqlConnection connection, String transactionName) : base(isolationLevel, connection, transactionName)
        {
        }

        
        internal SqlTransaction(SqlConnection connection) : base(IsolationLevel.ReadCommitted, connection, null)
        {
        }

        internal SqlTransaction(SqlConnection connection, String transactionName) : base(IsolationLevel.ReadCommitted, connection, transactionName)
        {
        }

        internal SqlTransaction(IsolationLevel isolationLevel, SqlConnection connection) : base(isolationLevel, connection, null)
        {
        }

        public new SqlConnection Connection
        {
            get
            {
                return (SqlConnection)_connection;
            }
        }


    
    }}