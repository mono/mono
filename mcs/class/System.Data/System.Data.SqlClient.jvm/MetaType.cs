namespace System.Data.SqlClient
{

using System.Data;

public class MetaType
{
    /**@todo Implement this CLASS!!!*/
    public MetaType()
    {
    }


    public static MetaType GetDefaultMetaType()
    {
        return null;
    }

    public static MetaType GetMetaType(DbType value)
    {
        return null;
    }

    public static MetaType GetMetaType(SqlDbType value)
    {
        return null;
    }

    public DbType DbType
    {
        get
        {
            return DbType.String;
        }
    }

    public SqlDbType SqlDbType
    {
        get
        {
            return SqlDbType.VarChar;
        }
    }

    public int TDSType
    {
        get
        {
            return 0;
        }
    }
}}