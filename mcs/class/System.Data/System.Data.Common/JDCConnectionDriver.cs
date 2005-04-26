namespace System.Data.Common
{

using java.sql;
using java.util;


class JDCConnectionDriver : Driver
{

    public static readonly String URL_PREFIX = "jdbc:jdc:";
    private static readonly int MAJOR_VERSION = 1;
    private static readonly int MINOR_VERSION = 0;
    private JDCConnectionPool pool;

    public JDCConnectionDriver(String driver, String url,
                                 String user, String password)
                            //throws ClassNotFoundException,
                              // InstantiationException, IllegalAccessException,
                                //SQLException
    {
        DriverManager.registerDriver(this);
        java.lang.Class.forName(driver).newInstance();
        pool = new JDCConnectionPool(url, user, password);
    }

    public Connection connect(String url, Properties props)
                                      // throws SQLException
    {
        if(!url.StartsWith(URL_PREFIX))
        {
             return null;
        }

        return pool.getConnection();
    }

    public bool acceptsURL(String url)
    {
        return url.StartsWith(URL_PREFIX);
    }

    public int getMajorVersion()
    {
        return MAJOR_VERSION;
    }

    public int getMinorVersion()
    {
        return MINOR_VERSION;
    }

    public DriverPropertyInfo[] getPropertyInfo(String str, Properties props)
    {
        return new DriverPropertyInfo[0];
    }

    public bool jdbcCompliant()
    {
        return false;
    }
}
}