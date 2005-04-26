namespace System.Data.Common
{

    using java.sql;
    using java.util;


    public class JDCConnectionPool
    {

        private ArrayList _connections;
        private String _url, _user, _password;
        readonly private long timeout = 10000;
        readonly private int _initPoolsize = 10;
        private int _maxPoolSize = 100;
        private static bool _shutdown = false; 

        public JDCConnectionPool(String url, String user, String password)
        {
            _url = url;
            _user = user;
            _password = password;
            _connections = new ArrayList(_initPoolsize);
        }

    
        public void closeConnections()
        {
            lock(this)
            {
                if(_connections.size() > 0)
                {
                    Iterator connlist = _connections.iterator();
    
                    while (connlist.hasNext())
                    {
                        JDCConnection conn = (JDCConnection) connlist.next();
                        removeConnection(conn);
                    }
                }
            }
        }

        private  void removeConnection(JDCConnection conn)
        {
            lock(this)
            {
                _connections.remove(conn);
            }
        }

        public Connection getConnection() //throws SQLException
                                          {
                                              lock(this)
    {

        JDCConnection c;
        for (int i = 0; i < _connections.size(); i++)
    {
        c = (JDCConnection) _connections.get(i);
			
        if (c.lease())
        return c;
    }
        
    if(_connections.size() < _maxPoolSize)
{
    Connection conn = DriverManager.getConnection(_url, _user, _password);
    c = new JDCConnection(conn, this);
    c.lease();
    _connections.add(c);
    return c;
}
        
    return null;
}
	}

	public void returnConnection(JDCConnection conn)
	{
        lock(this)
        {
            conn.expireLease();
        }
	}
    
}
}