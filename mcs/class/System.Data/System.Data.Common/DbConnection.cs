//
// System.Data.Common.DbConnection
//
// Author:
//   Tim Coleman (tim@timcoleman.com)
//
// Copyright (C) Tim Coleman, 2003
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
#if NET_2_0 && !TARGET_JVM
using System.Transactions;
#endif

#if NET_4_5
using System.Threading;
using System.Threading.Tasks;
#endif

namespace System.Data.Common {
	public abstract class DbConnection : Component, IDbConnection, IDisposable
	{
		#region Constructors

		protected DbConnection ()
		{
		}

		#endregion // Constructors

		#region Properties
		[RecommendedAsConfigurable (true)]
		[RefreshProperties (RefreshProperties.All)]
		[DefaultValue ("")]
		public abstract string ConnectionString { get; set; }

		public abstract string Database { get; }
		public abstract string DataSource { get; }
		
		[Browsable (false)]
		public abstract string ServerVersion { get; }
		
		[Browsable (false)]
		public abstract ConnectionState State { get; }

		public virtual int ConnectionTimeout { 
			get { return 15; }
		}

		#endregion // Properties

		#region Methods

		protected abstract DbTransaction BeginDbTransaction (IsolationLevel isolationLevel);

		public DbTransaction BeginTransaction ()
		{
			return BeginDbTransaction (IsolationLevel.Unspecified);
		}

		public DbTransaction BeginTransaction (IsolationLevel isolationLevel)
		{
			return BeginDbTransaction (isolationLevel);
		}

		public abstract void ChangeDatabase (string databaseName);
		public abstract void Close ();

		public DbCommand CreateCommand ()
		{
			return CreateDbCommand ();
		}

		protected abstract DbCommand CreateDbCommand ();

#if NET_2_0 && !TARGET_JVM
		public virtual void EnlistTransaction (Transaction transaction)
		{
			throw new NotSupportedException ();                        
		}
#endif

#if NET_2_0
		static class DataTypes
		{
			static readonly ColumnInfo [] columns = {
				new ColumnInfo ("TypeName", typeof(string)),
				new ColumnInfo ("ProviderDbType", typeof(int)),
				new ColumnInfo ("ColumnSize", typeof(long)),
				new ColumnInfo ("CreateFormat", typeof(string)),
				new ColumnInfo ("CreateParameters", typeof(string)),
				new ColumnInfo ("DataType", typeof(string)),
				new ColumnInfo ("IsAutoIncrementable", typeof(bool)),
				new ColumnInfo ("IsBestMatch", typeof(bool)),
				new ColumnInfo ("IsCaseSensitive", typeof(bool)),
				new ColumnInfo ("IsFixedLength", typeof(bool)),
				new ColumnInfo ("IsFixedPrecisionScale", typeof(bool)),
				new ColumnInfo ("IsLong", typeof(bool)),
				new ColumnInfo ("IsNullable", typeof(bool)),
				new ColumnInfo ("IsSearchable", typeof(bool)),
				new ColumnInfo ("IsSearchableWithLike", typeof(bool)),
				new ColumnInfo ("IsUnsigned", typeof(bool)),
				new ColumnInfo ("MaximumScale", typeof(short)),
				new ColumnInfo ("MinimumScale", typeof(short)),
				new ColumnInfo ("IsConcurrencyType", typeof(bool)),
				new ColumnInfo ("IsLiteralSupported", typeof(bool)),
				new ColumnInfo ("LiteralPrefix", typeof(string)),
				new ColumnInfo ("LiteralSuffix", typeof(string))
			};

			static readonly object [][] rows = {
				new object [] {"smallint", 16, 5, "smallint", null, "System.Int16", true, true,
					       false, true, true, false, true, true, false, false, null,
					       null, false, null, null, null},
				new object [] {"int", 8, 10, "int", null, "System.Int32",
					       true, true, false, true, true, false, true, true, false,
					       false, null, null, false, null, null, null},
				new object [] {"real", 13, 7, "real", null,
					       "System.Single", false, true, false, true, false, false,
					       true, true, false, false, null, null, false, null, null, null},
				new object [] {"float", 6, 53, "float({0})",
					       "number of bits used to store the mantissa", "System.Double",
					       false, true, false, true, false, false, true, true,
					       false, false, null, null, false, null, null, null},
				new object [] {"money", 9, 19, "money", null,
					       "System.Decimal", false, false, false, true, true,
					       false, true, true, false, false, null, null, false,
					       null, null, null},
				new object [] {"smallmoney", 17, 10, "smallmoney", null,
					       "System.Decimal", false, false, false, true, true, false,
					       true, true, false, false, null, null, false, null, null, null},
				new object [] {"bit", 2, 1, "bit", null, "System.Boolean",
					       false, false, false, true, false, false, true, true,
					       false, null, null, null, false, null, null, null},
				new object [] {"tinyint", 20, 3, "tinyint", null,
					       "System.SByte", true, true, false, true, true, false,
					       true, true, false, true, null, null, false, null, null, null},
				new object [] {"bigint", 0, 19, "bigint", null,
					       "System.Int64", true, true, false, true, true, false,
					       true, true, false, false, null, null, false, null, null, null},
				new object [] {"timestamp", 19, 8, "timestamp", null,
					       "System.Byte[]", false, false, false, true, false, false,
					       false, true, false, null, null, null, true, null, "0x", null},
				new object [] {"binary", 1, 8000, "binary({0})", "length",
					       "System.Byte[]", false, true, false, true, false, false,
					       true, true, false, null, null, null, false, null, "0x", null},
				new object [] {"image", 7, 2147483647, "image", null,
					       "System.Byte[]", false, true, false, false, false, true,
					       true, false, false, null, null, null, false, null, "0x", null},
				new object [] {"text", 18, 2147483647, "text", null,
					       "System.String", false, true, false, false, false, true,
					       true, false, true, null, null, null, false, null, "'", "'"},
				new object [] {"ntext", 11, 1073741823, "ntext", null,
					       "System.String", false, true, false, false, false, true,
					       true, false, true, null, null, null, false, null, "N'", "'"},
				new object [] {"decimal", 5, 38, "decimal({0}, {1})",
					       "precision,scale", "System.Decimal", true, true, false,
					       true, false, false, true, true, false, false, 38, 0,
					       false, null, null, null},
				new object [] {"numeric", 5, 38, "numeric({0}, {1})",
					       "precision,scale", "System.Decimal", true, true, false,
					       true, false, false, true, true, false, false, 38, 0,
					       false, null, null, null},
				new object [] {"datetime", 4, 23, "datetime", null,
					       "System.DateTime", false, true, false, true, false, false,
					       true, true, true, null, null, null, false, null, "{ts '", "'}"},
				new object [] {"smalldatetime", 15, 16, "smalldatetime", null,
					       "System.DateTime", false, true, false, true, false, false,
					       true, true, true, null, null, null, false, null, "{ts '", "'}"},
				new object [] {"sql_variant", 23, null, "sql_variant",
					       null, "System.Object", false, true, false, false, false,
					       false, true, true, false, null, null, null, false, false,
					       null, null},
				new object [] {"xml", 25, 2147483647, "xml", null,
					       "System.String", false, false, false, false, false, true,
					       true, false, false, null, null, null, false, false, null, null},
				new object [] {"varchar", 22, 2147483647, "varchar({0})",
					       "max length", "System.String", false, true, false, false,
					       false, false, true, true, true, null, null, null, false,
					       null, "'", "'"},
				new object [] {"char", 3, 2147483647, "char({0})", "length",
					       "System.String", false, true, false, true, false, false,
					       true, true, true, null, null, null, false, null, "'", "'"},
				new object [] {"nchar", 10, 1073741823, "nchar({0})", "length",
					       "System.String", false, true, false, true, false, false,
					       true, true, true, null, null, null, false, null, "N'", "'"},
				new object [] {"nvarchar", 12, 1073741823, "nvarchar({0})", "max length",
					       "System.String", false, true, false, false, false, false, true, true,
					       true, null, null, null, false, null, "N'", "'"},
				new object [] {"varbinary", 21, 1073741823, "varbinary({0})",
					       "max length", "System.Byte[]", false, true, false, false,
					       false, false, true, true, false, null, null, null, false,
					       null, "0x", null},
				new object [] {"uniqueidentifier", 14, 16, "uniqueidentifier", null,
					       "System.Guid", false, true, false, true, false, false, true,
					       true, false, null, null, null, false, null, "'", "'"}
			};

			static DataTable instance;
			static public DataTable Instance {
				get {
					if (instance == null) {
						instance = new DataTable ("DataTypes");
						foreach (ColumnInfo c in columns)
							instance.Columns.Add (c.name, c.type);
						foreach (object [] row in rows)
							instance.LoadDataRow (row, true);
					}
					return instance;
				}
			}
		}

		struct ColumnInfo {
			public string name;
			public Type type;
			public ColumnInfo (string name, Type type)
			{
				this.name = name; this.type = type;
			}
		}

		internal static class MetaDataCollections
		{
			static readonly ColumnInfo [] columns = {
				new ColumnInfo ("CollectionName", typeof (string)),
				new ColumnInfo ("NumberOfRestrictions", typeof (int)),
				new ColumnInfo ("NumberOfIdentifierParts", typeof (int))
			};

			static readonly object [][] rows = {
				new object [] {"MetaDataCollections", 0, 0},
				new object [] {"DataSourceInformation", 0, 0},
				new object [] {"DataTypes", 0, 0},
				new object [] {"Restrictions", 0, 0},
				new object [] {"ReservedWords", 0, 0},
				new object [] {"Users", 1, 1},
				new object [] {"Databases", 1, 1},
				new object [] {"Tables", 4, 3},
				new object [] {"Columns", 4, 4},
				new object [] {"Views", 3, 3},
				new object [] {"ViewColumns", 4, 4},
				new object [] {"ProcedureParameters", 4, 1},
				new object [] {"Procedures", 4, 3},
				new object [] {"ForeignKeys", 4, 3},
				new object [] {"IndexColumns", 5, 4},
				new object [] {"Indexes", 4, 3},
				new object [] {"UserDefinedTypes", 2, 1}
			};

			static DataTable instance;
			static public DataTable Instance {
				get {
					if (instance == null) {
						instance = new DataTable ("GetSchema");
						foreach (ColumnInfo c in columns)
							instance.Columns.Add (c.name, c.type);
						foreach (object [] row in rows)
							instance.LoadDataRow (row, true);
					}
					return instance;
				}
			}
		}

		static class Restrictions
		{
			static readonly ColumnInfo [] columns = {
				new ColumnInfo ("CollectionName", typeof (string)),
				new ColumnInfo ("RestrictionName", typeof(string)),
				new ColumnInfo ("ParameterName", typeof(string)),
				new ColumnInfo ("RestrictionDefault", typeof(string)),
				new ColumnInfo ("RestrictionNumber", typeof(int))
			};

			static readonly object [][] rows = {
				new object [] {"Users", "User_Name", "@Name", "name", 1},
				new object [] {"Databases", "Name", "@Name", "Name", 1},

				new object [] {"Tables", "Catalog", "@Catalog", "TABLE_CATALOG", 1},
				new object [] {"Tables", "Owner", "@Owner", "TABLE_SCHEMA", 2},
				new object [] {"Tables", "Table", "@Name", "TABLE_NAME", 3},
				new object [] {"Tables", "TableType", "@TableType", "TABLE_TYPE", 4},

				new object [] {"Columns", "Catalog", "@Catalog", "TABLE_CATALOG", 1},
				new object [] {"Columns", "Owner", "@Owner", "TABLE_SCHEMA", 2},
				new object [] {"Columns", "Table", "@Table", "TABLE_NAME", 3},
				new object [] {"Columns", "Column", "@Column", "COLUMN_NAME", 4},

				new object [] {"Views", "Catalog", "@Catalog", "TABLE_CATALOG", 1},
				new object [] {"Views", "Owner", "@Owner", "TABLE_SCHEMA", 2},
				new object [] {"Views", "Table", "@Table", "TABLE_NAME", 3},

				new object [] {"ViewColumns", "Catalog", "@Catalog", "VIEW_CATALOG", 1},
				new object [] {"ViewColumns", "Owner", "@Owner", "VIEW_SCHEMA", 2},
				new object [] {"ViewColumns", "Table", "@Table", "VIEW_NAME", 3},
				new object [] {"ViewColumns", "Column", "@Column", "COLUMN_NAME", 4},

				new object [] {"ProcedureParameters", "Catalog", "@Catalog", "SPECIFIC_CATALOG", 1},
				new object [] {"ProcedureParameters", "Owner", "@Owner", "SPECIFIC_SCHEMA", 2},
				new object [] {"ProcedureParameters", "Name", "@Name", "SPECIFIC_NAME", 3},
				new object [] {"ProcedureParameters", "Parameter", "@Parameter", "PARAMETER_NAME", 4},

				new object [] {"Procedures", "Catalog", "@Catalog", "SPECIFIC_CATALOG", 1},
				new object [] {"Procedures", "Owner", "@Owner", "SPECIFIC_SCHEMA", 2},
				new object [] {"Procedures", "Name", "@Name", "SPECIFIC_NAME", 3},
				new object [] {"Procedures", "Type", "@Type", "ROUTINE_TYPE", 4},

				new object [] {"IndexColumns", "Catalog", "@Catalog", "db_name(}", 1},
				new object [] {"IndexColumns", "Owner", "@Owner", "user_name(}", 2},
				new object [] {"IndexColumns", "Table", "@Table", "o.name", 3},
				new object [] {"IndexColumns", "ConstraintName", "@ConstraintName", "x.name", 4},
				new object [] {"IndexColumns", "Column", "@Column", "c.name", 5},

				new object [] {"Indexes", "Catalog", "@Catalog", "db_name(}", 1},
				new object [] {"Indexes", "Owner", "@Owner", "user_name(}", 2},
				new object [] {"Indexes", "Table", "@Table", "o.name", 3},
				new object [] {"Indexes", "Name", "@Name", "x.name", 4},

				new object [] {"UserDefinedTypes", "assembly_name", "@AssemblyName", "assemblies.name", 1},
				new object [] {"UserDefinedTypes", "udt_name", "@UDTName", "types.assembly_class", 2},

				new object [] {"ForeignKeys", "Catalog", "@Catalog", "CONSTRAINT_CATALOG", 1},
				new object [] {"ForeignKeys", "Owner", "@Owner", "CONSTRAINT_SCHEMA", 2},
				new object [] {"ForeignKeys", "Table", "@Table", "TABLE_NAME", 3},
				new object [] {"ForeignKeys", "Name", "@Name", "CONSTRAINT_NAME", 4}
			};

			static DataTable instance;
			static public DataTable Instance {
				get {
					if (instance == null) {
						instance = new DataTable ("Restrictions");
						foreach (ColumnInfo c in columns)
							instance.Columns.Add (c.name, c.type);
						foreach (object [] row in rows)
							instance.LoadDataRow (row, true);
					}
					return instance;
				}
			}
		}

		static class ReservedWords
		{
			static readonly string [] reservedWords =
			{
				"ADD", "EXCEPT", "PERCENT", "ALL", "EXEC", "PLAN", "ALTER",
				  "EXECUTE", "PRECISION", "AND", "EXISTS", "PRIMARY", "ANY",
				  "EXIT", "PRINT", "AS", "FETCH", "PROC", "ASC", "FILE",
				  "PROCEDURE", "AUTHORIZATION", "FILLFACTOR", "PUBLIC",
				  "BACKUP", "FOR", "RAISERROR", "BEGIN", "FOREIGN", "READ",
				  "BETWEEN", "FREETEXT", "READTEXT", "BREAK", "FREETEXTTABLE",
				  "RECONFIGURE", "BROWSE", "FROM", "REFERENCES", "BULK",
				  "FULL", "REPLICATION", "BY", "FUNCTION", "RESTORE",
				  "CASCADE", "GOTO", "RESTRICT", "CASE", "GRANT", "RETURN",
				  "CHECK", "GROUP", "REVOKE", "CHECKPOINT", "HAVING", "RIGHT",
				  "CLOSE", "HOLDLOCK", "ROLLBACK", "CLUSTERED", "IDENTITY",
				  "ROWCOUNT", "COALESCE", "IDENTITY_INSERT", "ROWGUIDCOL",
				  "COLLATE", "IDENTITYCOL", "RULE", "COLUMN", "IF", "SAVE",
				  "COMMIT", "IN", "SCHEMA", "COMPUTE", "INDEX", "SELECT",
				  "CONSTRAINT", "INNER", "SESSION_USER", "CONTAINS", "INSERT",
				  "SET", "CONTAINSTABLE", "INTERSECT", "SETUSER", "CONTINUE",
				  "INTO", "SHUTDOWN", "CONVERT", "IS", "SOME", "CREATE",
				  "JOIN", "STATISTICS", "CROSS", "KEY", "SYSTEM_USER",
				  "CURRENT", "KILL", "TABLE", "CURRENT_DATE", "LEFT",
				  "TEXTSIZE", "CURRENT_TIME", "LIKE", "THEN",
				  "CURRENT_TIMESTAMP", "LINENO", "TO", "CURRENT_USER", "LOAD",
				  "TOP", "CURSOR", "NATIONAL", "TRAN", "DATABASE", "NOCHECK",
				  "TRANSACTION", "DBCC", "NONCLUSTERED", "TRIGGER",
				  "DEALLOCATE", "NOT", "TRUNCATE", "DECLARE", "NULL",
				  "TSEQUAL", "DEFAULT", "NULLIF", "UNION", "DELETE", "OF",
				  "UNIQUE", "DENY", "OFF", "UPDATE", "DESC", "OFFSETS",
				  "UPDATETEXT", "DISK", "ON", "USE", "DISTINCT", "OPEN",
				  "USER", "DISTRIBUTED", "OPENDATASOURCE", "VALUES", "DOUBLE",
				  "OPENQUERY", "VARYING", "DROP", "OPENROWSET", "VIEW",
				  "DUMMY", "OPENXML", "WAITFOR", "DUMP", "OPTION", "WHEN",
				  "ELSE", "OR", "WHERE", "END", "ORDER", "WHILE", "ERRLVL",
				  "OUTER", "WITH", "ESCAPE", "OVER", "WRITETEXT", "ABSOLUTE",
				  "FOUND", "PRESERVE", "ACTION", "FREE", "PRIOR", "ADMIN",
				  "GENERAL", "PRIVILEGES", "AFTER", "GET", "READS",
				  "AGGREGATE", "GLOBAL", "REAL", "ALIAS", "GO", "RECURSIVE",
				  "ALLOCATE", "GROUPING", "REF", "ARE", "HOST", "REFERENCING",
				  "ARRAY", "HOUR", "RELATIVE", "ASSERTION", "IGNORE", "RESULT",
				  "AT", "IMMEDIATE", "RETURNS", "BEFORE", "INDICATOR", "ROLE",
				  "BINARY", "INITIALIZE", "ROLLUP", "BIT", "INITIALLY",
				  "ROUTINE", "BLOB", "INOUT", "ROW", "BOOLEAN", "INPUT",
				  "ROWS", "BOTH", "INT", "SAVEPOINT", "BREADTH", "INTEGER",
				  "SCROLL", "CALL", "INTERVAL", "SCOPE", "CASCADED",
				  "ISOLATION", "SEARCH", "CAST", "ITERATE", "SECOND",
				  "CATALOG", "LANGUAGE", "SECTION", "CHAR", "LARGE",
				  "SEQUENCE", "CHARACTER", "LAST", "SESSION", "CLASS",
				  "LATERAL", "SETS", "CLOB", "LEADING", "SIZE", "COLLATION",
				  "LESS", "SMALLINT", "COMPLETION", "LEVEL", "SPACE",
				  "CONNECT", "LIMIT", "SPECIFIC", "CONNECTION", "LOCAL",
				  "SPECIFICTYPE", "CONSTRAINTS", "LOCALTIME", "SQL",
				  "CONSTRUCTOR", "LOCALTIMESTAMP", "SQLEXCEPTION",
				  "CORRESPONDING", "LOCATOR", "SQLSTATE", "CUBE", "MAP",
				  "SQLWARNING", "CURRENT_PATH", "MATCH", "START",
				  "CURRENT_ROLE", "MINUTE", "STATE", "CYCLE", "MODIFIES",
				  "STATEMENT", "DATA", "MODIFY", "STATIC", "DATE", "MODULE",
				  "STRUCTURE", "DAY", "MONTH", "TEMPORARY", "DEC", "NAMES",
				  "TERMINATE", "DECIMAL", "NATURAL", "THAN", "DEFERRABLE",
				  "NCHAR", "TIME", "DEFERRED", "NCLOB", "TIMESTAMP", "DEPTH",
				  "NEW", "TIMEZONE_HOUR", "DEREF", "NEXT", "TIMEZONE_MINUTE",
				  "DESCRIBE", "NO", "TRAILING", "DESCRIPTOR", "NONE",
				  "TRANSLATION", "DESTROY", "NUMERIC", "TREAT", "DESTRUCTOR",
				  "OBJECT", "TRUE", "DETERMINISTIC", "OLD", "UNDER",
				  "DICTIONARY", "ONLY", "UNKNOWN", "DIAGNOSTICS", "OPERATION",
				  "UNNEST", "DISCONNECT", "ORDINALITY", "USAGE", "DOMAIN",
				  "OUT", "USING", "DYNAMIC", "OUTPUT", "VALUE", "EACH",
				  "PAD", "VARCHAR", "END-EXEC", "PARAMETER", "VARIABLE",
				  "EQUALS", "PARAMETERS", "WHENEVER", "EVERY", "PARTIAL",
				  "WITHOUT", "EXCEPTION", "PATH", "WORK", "EXTERNAL",
				  "POSTFIX", "WRITE", "FALSE", "PREFIX", "YEAR", "FIRST",
				  "PREORDER", "ZONE", "FLOAT", "PREPARE", "ADA", "AVG",
				  "BIT_LENGTH", "CHAR_LENGTH", "CHARACTER_LENGTH", "COUNT",
				  "EXTRACT", "FORTRAN", "INCLUDE", "INSENSITIVE", "LOWER",
				  "MAX", "MIN", "OCTET_LENGTH", "OVERLAPS", "PASCAL",
				  "POSITION", "SQLCA", "SQLCODE", "SQLERROR", "SUBSTRING",
				  "SUM", "TRANSLATE", "TRIM", "UPPER"
			};
			static DataTable instance;
			static public DataTable Instance {
				get {
					if (instance == null) {
						DataRow row = null;
						instance = new DataTable ("ReservedWords");
						instance.Columns.Add ("ReservedWord", typeof(string));
						foreach (string reservedWord in reservedWords)
						{
							row = instance.NewRow();

							row["ReservedWord"] = reservedWord;
							instance.Rows.Add(row);
						}
					}
					return instance;
				}
			}
		}

		public virtual DataTable GetSchema ()
		{
			return MetaDataCollections.Instance;
		}

		public virtual DataTable GetSchema (string collectionName)
		{
			return GetSchema (collectionName, null);
		}

		private void AddParameter (DbCommand command, string parameterName, DbType parameterType, int parameterSize)
		{
			DbParameter parameter = command.CreateParameter ();
			parameter.ParameterName = parameterName;
			parameter.DbType = parameterType;
			parameter.Size = parameterSize;
			command.Parameters.Add (parameter);
		}

		public virtual DataTable GetSchema (string collectionName, string[] restrictionValues)
		{
			if (collectionName == null)
				//LAMESPEC: In MS.NET, if collectionName is null, it throws ArgumentException.
				throw new ArgumentException ();

			String cName          = null;
			DataTable schemaTable = MetaDataCollections.Instance;
			int length = restrictionValues == null ? 0 : restrictionValues.Length;

			foreach (DataRow row in schemaTable.Rows) {
				if (String.Compare ((string) row ["CollectionName"], collectionName, true) == 0) {
					if (length > (int) row ["NumberOfRestrictions"]) {
						throw new ArgumentException ("More restrictions were provided " +
									     "than the requested schema ('" +
									     row ["CollectionName"].ToString () + "') supports");
					}
					cName = row ["CollectionName"].ToString ();
				}
			}
			if (cName == null)
				throw new ArgumentException ("The requested collection ('" + collectionName + "') is not defined.");

			DbCommand command     = null;
			DataTable dataTable   = new DataTable ();

			switch (cName)
			{
			case "Databases":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select name as database_name, dbid, crdate as create_date " +
				  "from master.sys.sysdatabases where (name = @Name or (@Name " +
				  "is null))";
				AddParameter (command, "@Name", DbType.StringFixedLength, 4000);
				break;
			case "ForeignKeys":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select CONSTRAINT_CATALOG, CONSTRAINT_SCHEMA, CONSTRAINT_NAME, " +
				  "TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, CONSTRAINT_TYPE, " +
				  "IS_DEFERRABLE, INITIALLY_DEFERRED from " +
				  "INFORMATION_SCHEMA.TABLE_CONSTRAINTS where (CONSTRAINT_CATALOG" +
				  " = @Catalog or (@Catalog is null)) and (CONSTRAINT_SCHEMA = " +
				  "@Owner or (@Owner is null)) and (TABLE_NAME = @Table or (" +
				  "@Table is null)) and (CONSTRAINT_NAME = @Name or (@Name is null))" +
				  " and CONSTRAINT_TYPE = 'FOREIGN KEY' order by CONSTRAINT_CATALOG," +
				  " CONSTRAINT_SCHEMA, CONSTRAINT_NAME";
				AddParameter (command, "@Catalog", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Owner", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Table", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Name", DbType.StringFixedLength, 4000);
				break;
			case "Indexes":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select distinct db_name() as constraint_catalog, " +
				  "constraint_schema = user_name (o.uid), " +
				  "constraint_name = x.name, table_catalog = db_name (), " +
				  "table_schema = user_name (o.uid), table_name = o.name, " +
				  "index_name  = x.name from sysobjects o, sysindexes x, " +
				  "sysindexkeys xk where o.type in ('U') and x.id = o.id and " +
				  "o.id = xk.id and x.indid = xk.indid and xk.keyno = x.keycnt " +
				  "and (db_name() = @Catalog or (@Catalog is null)) and " +
				  "(user_name() = @Owner or (@Owner is null)) and (o.name = " +
				  "@Table or (@Table is null)) and (x.name = @Name or (@Name is null))" +
				  "order by table_name, index_name";
				AddParameter (command, "@Catalog", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Owner", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Table", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Name", DbType.StringFixedLength, 4000);
				break;
			case "IndexColumns":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select distinct db_name() as constraint_catalog, " +
				  "constraint_schema = user_name (o.uid), constraint_name = x.name, " +
				  "table_catalog = db_name (), table_schema = user_name (o.uid), " +
				  "table_name = o.name, column_name = c.name, " +
				  "ordinal_position = convert (int, xk.keyno), keyType = c.xtype, " +
				  "index_name = x.name from sysobjects o, sysindexes x, syscolumns c, " +
				  "sysindexkeys xk where o.type in ('U') and x.id = o.id and o.id = c.id " +
				  "and o.id = xk.id and x.indid = xk.indid and c.colid = xk.colid " +
				  "and xk.keyno <= x.keycnt and permissions (o.id, c.name) <> 0 " +
				  "and (db_name() = @Catalog or (@Catalog is null)) and (user_name() " +
				  "= @Owner or (@Owner is null)) and (o.name = @Table or (@Table is" +
				  " null)) and (x.name = @ConstraintName or (@ConstraintName is null)) " +
				  "and (c.name = @Column or (@Column is null)) order by table_name, " +
				  "index_name";
				AddParameter (command, "@Catalog", DbType.StringFixedLength, 8);
				AddParameter (command, "@Owner", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Table", DbType.StringFixedLength, 13);
				AddParameter (command, "@ConstraintName", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Column", DbType.StringFixedLength, 4000);
				break;
			case "Procedures":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME, " +
				  "ROUTINE_CATALOG, ROUTINE_SCHEMA, ROUTINE_NAME, ROUTINE_TYPE, " +
				  "CREATED, LAST_ALTERED from INFORMATION_SCHEMA.ROUTINES where " +
				  "(SPECIFIC_CATALOG = @Catalog or (@Catalog is null)) and " +
				  "(SPECIFIC_SCHEMA = @Owner or (@Owner is null)) and (SPECIFIC_NAME" +
				  " = @Name or (@Name is null)) and (ROUTINE_TYPE = @Type or (@Type " +
				  "is null)) order by SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME";
				AddParameter (command, "@Catalog", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Owner", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Name", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Type", DbType.StringFixedLength, 4000);
				break;
			case "ProcedureParameters":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select SPECIFIC_CATALOG, SPECIFIC_SCHEMA, SPECIFIC_NAME, " +
				  "ORDINAL_POSITION, PARAMETER_MODE, IS_RESULT, AS_LOCATOR, " +
				  "PARAMETER_NAME, DATA_TYPE, CHARACTER_MAXIMUM_LENGTH, " +
				  "CHARACTER_OCTET_LENGTH, COLLATION_CATALOG, COLLATION_SCHEMA, " +
				  "COLLATION_NAME, CHARACTER_SET_CATALOG, CHARACTER_SET_SCHEMA, " +
				  "CHARACTER_SET_NAME, NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, " +
				  "NUMERIC_SCALE, DATETIME_PRECISION, INTERVAL_TYPE, " +
				  "INTERVAL_PRECISION from INFORMATION_SCHEMA.PARAMETERS where " +
				  "(SPECIFIC_CATALOG = @Catalog or (@Catalog is null)) and " +
				  "(SPECIFIC_SCHEMA = @Owner or (@Owner is null)) and (SPECIFIC_NAME = " +
				  "@Name or (@Name is null)) and (PARAMETER_NAME = @Parameter or (" +
				  "@Parameter is null)) order by SPECIFIC_CATALOG, SPECIFIC_SCHEMA," +
				  " SPECIFIC_NAME, PARAMETER_NAME";
				AddParameter (command, "@Catalog", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Owner", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Name", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Parameter", DbType.StringFixedLength, 4000);
				break;
			case "Tables":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, TABLE_TYPE " +
				  "from INFORMATION_SCHEMA.TABLES where" +
				  " (TABLE_CATALOG = @catalog or (@catalog is null)) and " +
				  "(TABLE_SCHEMA = @owner or (@owner is null))and " +
				  "(TABLE_NAME = @name or (@name is null)) and " +
				  "(TABLE_TYPE = @table_type or (@table_type is null))";
				AddParameter (command, "@catalog", DbType.StringFixedLength, 8);
				AddParameter (command, "@owner", DbType.StringFixedLength, 3);
				AddParameter (command, "@name", DbType.StringFixedLength, 11);
				AddParameter (command, "@table_type", DbType.StringFixedLength, 10);
				break;
			case "Columns":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME, " +
				  "ORDINAL_POSITION, COLUMN_DEFAULT, IS_NULLABLE, DATA_TYPE, " +
				  "CHARACTER_MAXIMUM_LENGTH, CHARACTER_OCTET_LENGTH, " +
				  "NUMERIC_PRECISION, NUMERIC_PRECISION_RADIX, NUMERIC_SCALE, " +
				  "DATETIME_PRECISION, CHARACTER_SET_CATALOG, CHARACTER_SET_SCHEMA, " +
				  "CHARACTER_SET_NAME, COLLATION_CATALOG from INFORMATION_SCHEMA.COLUMNS" +
				  " where (TABLE_CATALOG = @Catalog or (@Catalog is null)) and (" +
				  "TABLE_SCHEMA = @Owner or (@Owner is null)) and (TABLE_NAME = @table" +
				  " or (@Table is null)) and (COLUMN_NAME = @column or (@Column is null" +
				  ")) order by TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME";
				AddParameter (command, "@Catalog", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Owner", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Table", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Column", DbType.StringFixedLength, 4000);
				break;
			case "Users":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select uid, name as user_name, createdate, updatedate from sysusers" +
				  " where (name = @Name or (@Name is null))";
				AddParameter (command, "@Name", DbType.StringFixedLength, 4000);
				break;
			case "Views":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME, CHECK_OPTION, " +
				  "IS_UPDATABLE from INFORMATION_SCHEMA.VIEWS where (TABLE_CATALOG" +
				  " = @Catalog or (@Catalog is null)) TABLE_SCHEMA = @Owner or " +
				  "(@Owner is null)) and (TABLE_NAME = @table or (@Table is null))" +
				  " order by TABLE_CATALOG, TABLE_SCHEMA, TABLE_NAME";
				AddParameter (command, "@Catalog", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Owner", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Table", DbType.StringFixedLength, 4000);
				break;
			case "ViewColumns":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select VIEW_CATALOG, VIEW_SCHEMA, VIEW_NAME, TABLE_CATALOG, " +
				  "TABLE_SCHEMA, TABLE_NAME, COLUMN_NAME from " +
				  "INFORMATION_SCHEMA.VIEW_COLUMN_USAGE where (VIEW_CATALOG = " +
				  "@Catalog (@Catalog is null)) and (VIEW_SCHEMA = @Owner (@Owner" +
				  " is null)) and (VIEW_NAME = @Table or (@Table is null)) and " +
				  "(COLUMN_NAME = @Column or (@Column is null)) order by " +
				  "VIEW_CATALOG, VIEW_SCHEMA, VIEW_NAME";
				AddParameter (command, "@Catalog", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Owner", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Table", DbType.StringFixedLength, 4000);
				AddParameter (command, "@Column", DbType.StringFixedLength, 4000);
				break;
			case "UserDefinedTypes":
				command = CreateCommand ();
				command.Connection = this;
				command.CommandText = "select assemblies.name as assembly_name, types.assembly_class " +
				  "as udt_name, ASSEMBLYPROPERTY(assemblies.name, 'VersionMajor') " +
				  "as version_major, ASSEMBLYPROPERTY(assemblies.name, 'VersionMinor') " +
				  "as version_minor, ASSEMBLYPROPERTY(assemblies.name, 'VersionBuild') " +
				  "as version_build, ASSEMBLYPROPERTY(assemblies.name, 'VersionRevision') " +
				  "as version_revision, ASSEMBLYPROPERTY(assemblies.name, 'CultureInfo') " +
				  "as culture_info, ASSEMBLYPROPERTY(assemblies.name, 'PublicKey') " +
				  "as public_key, is_fixed_length, max_length, Create_Date, " +
				  "Permission_set_desc from sys.assemblies as assemblies join " +
				  "sys.assembly_types as types on assemblies.assembly_id = types.assembly_id" +
				  " where (assemblies.name = @AssemblyName or (@AssemblyName is null)) and " +
				  "(types.assembly_class = @UDTName or (@UDTName is null))";
				AddParameter (command, "@AssemblyName", DbType.StringFixedLength, 4000);
				AddParameter (command, "@UDTName", DbType.StringFixedLength, 4000);
				break;
			case "MetaDataCollections":
				return MetaDataCollections.Instance;
			case "DataSourceInformation":
				throw new NotImplementedException ();
			case "DataTypes":
				return DataTypes.Instance;
			case "ReservedWords":
				return ReservedWords.Instance;
			case "Restrictions":
				return Restrictions.Instance;
			}
			for (int i = 0; i < length; i++) {
				command.Parameters [i].Value = restrictionValues [i];
			}
			DbDataAdapter dataAdapter = DbProviderFactory.CreateDataAdapter ();
			dataAdapter.SelectCommand = command;
			dataAdapter.Fill (dataTable);
			return dataTable;
		}

		protected internal virtual DbProviderFactory DbProviderFactory {
			get {
				return null;
			}
		}
#endif

		IDbTransaction IDbConnection.BeginTransaction ()
		{
			return BeginTransaction ();
		}

		IDbTransaction IDbConnection.BeginTransaction (IsolationLevel il)
		{
			return BeginTransaction (il);
		}

		IDbCommand IDbConnection.CreateCommand ()
		{
			return CreateCommand ();
		}
		
		public abstract void Open ();

		protected virtual void OnStateChange (StateChangeEventArgs stateChange)
		{
			if (StateChange != null)
				StateChange (this, stateChange);
		}
		
#if NET_4_5
		public Task OpenAsync ()
		{
			return OpenAsync (CancellationToken.None);
		}
		
		public virtual Task OpenAsync (CancellationToken cancellationToken)
		{
			if (cancellationToken.IsCancellationRequested) {
				return TaskHelper.CreateCanceledTask ();
			}
			
			try {
				Open ();
				return TaskHelper.CreateVoidTask ();
			} catch (Exception e) {
				return TaskHelper.CreateExceptionTask (e);
			}
		}
#endif

		#endregion // Methods

		public virtual event StateChangeEventHandler StateChange;

	}
}
