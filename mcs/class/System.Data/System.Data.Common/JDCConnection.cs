namespace System.Data.Common
{

    using java.sql;
    using java.util;

    using System;

    public class JDCConnection : Connection
    {

        private JDCConnectionPool pool;
        private Connection conn;
        private bool inuse;
        private long timestamp;


        public JDCConnection(Connection conn, JDCConnectionPool pool)
        {
            this.conn=conn;
            this.pool=pool;
            this.inuse=false;
            this.timestamp=0;
        }

        public bool lease()
            
        {
            lock(this)
            {
                if(inuse)
                {
                    return false;
                }
                else
                {
                    inuse=true;
                    timestamp = java.lang.System.currentTimeMillis();
                    return true;
                }
            }
        }
        public bool validate()
        {
            try
            {
                conn.getMetaData();
            }
            catch (Exception)
            {
                return false;
            }

            return true;
        }

        public bool inUse()
        {
            return inuse;
        }

        public long getLastUse()
        {
            return timestamp;
        }

        public void close() //throws SQLException
        {
            pool.returnConnection(this);
        }

        public void expireLease()
        {
            inuse=false;
        }

        protected Connection getConnection()
        {
            return conn;
        }

        public void setTypeMap(Map map) //throws SQLException
        {
            conn.setTypeMap(map);
        }


        public Map getTypeMap() //throws SQLException
        {
            return conn.getTypeMap();
        }

        public PreparedStatement prepareStatement(String sql) //throws SQLException
        {
            return conn.prepareStatement(sql);
        }

        public PreparedStatement prepareStatement(String sql, int resultSetType, int resultSetConcurrency) //throws SQLException
        {
            return conn.prepareStatement(sql, resultSetType, resultSetConcurrency);
        }

        public CallableStatement prepareCall(String sql) //throws SQLException
        {
            return conn.prepareCall(sql);
        }


        public CallableStatement prepareCall(String sql, int resultSetType, int resultSetConcurrency) //throws SQLException
        {
            return conn.prepareCall(sql, resultSetType, resultSetConcurrency);
        }


        public Statement createStatement(int resultSetType, int resultSetConcurrency) //throws SQLException
        {
            return conn.createStatement(resultSetType, resultSetConcurrency);
        }

        public Statement createStatement() //throws SQLException
        {
            return conn.createStatement();
        }

        public String nativeSQL(String sql) //throws SQLException
        {
            return conn.nativeSQL(sql);
        }

        public void setAutoCommit(bool autoCommit) //throws SQLException
        {
            conn.setAutoCommit(autoCommit);
        }

        public bool getAutoCommit() //throws SQLException
        {
            return conn.getAutoCommit();
        }

        public void commit() //throws SQLException
        {
            conn.commit();
        }

        public void rollback() //throws SQLException
        {
            conn.rollback();
        }

        public bool isClosed() //throws SQLException
        {
            if(conn.isClosed())
                return true;

            return !inUse();
        }

        public DatabaseMetaData getMetaData() //throws SQLException
        {
            return conn.getMetaData();
        }

        public void setReadOnly(bool readOnly) //throws SQLException
        {
            conn.setReadOnly(readOnly);
        }

        public bool isReadOnly()// throws SQLException
        {
            return conn.isReadOnly();
        }

        public void setCatalog(String catalog) //throws SQLException
        {
            conn.setCatalog(catalog);
        }

        public String getCatalog() //throws SQLException
        {
            return conn.getCatalog();
        }

        public void setTransactionIsolation(int level) //throws SQLException
        {
            conn.setTransactionIsolation(level);
        }

        public int getTransactionIsolation() //throws SQLException
        {
            return conn.getTransactionIsolation();
        }

        public SQLWarning getWarnings() //throws SQLException
        {
            return conn.getWarnings();
        }

        public void clearWarnings() //throws SQLException
        {
            conn.clearWarnings();
        }

        // --------- JDBC 3.0 -------------
        /*
        public void setHoldability(int holdability)
        {
            conn.setHoldability(holdability);
        }
        
        public int getHoldability()
        {
            return conn.getHoldability();
        }

        public Savepoint setSavepoint()
        {
            return conn.setSavepoint();
        }

        public Savepoint setSavepoint(string name)
        {
            return conn.setSavepoint(name);
        }

        public void rollback(Savepoint savepoint)
        {
            conn.rollback(savepoint);
        }

        public void releaseSavepoint(Savepoint savepoint)
        {
            conn.releaseSavepoint(savepoint);
        }

        public Statement createStatement(int resultSetType, int resultSetConcurrency, 
            int resultSetHoldability)
        {
            return conn.createStatement(resultSetType, resultSetConcurrency, resultSetHoldability);
        }

        public PreparedStatement prepareStatement(String sql, int resultSetType, 
            int resultSetConcurrency, int resultSetHoldability)
        {
            throw new NotImplementedException();
        }

        public CallableStatement prepareCall(String sql, int resultSetType, 
            int resultSetConcurrency, 
            int resultSetHoldability)
        {
            return conn.prepareCall(sql, resultSetType, resultSetConcurrency, resultSetHoldability);
        }

        public PreparedStatement prepareStatement(String sql, int autoGeneratedKeys)
        {
            return conn.prepareStatement(sql, autoGeneratedKeys);
        }

        public PreparedStatement prepareStatement(String sql, int[] columnIndexes)
        {
            return conn.prepareStatement(sql, columnIndexes);
        }

        public PreparedStatement prepareStatement(String sql, String[] columnNames)
        {
            return conn.prepareStatement(sql, columnNames);
        }

        */
    }
}