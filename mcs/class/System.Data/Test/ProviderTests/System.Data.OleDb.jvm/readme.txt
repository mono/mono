To run unit test the following should be prepared:

1. System.Data\Test\ProviderTests\System.Data.OleDb.J2EE.config should contain an ConnectionString setting, i.e.:

<?xml version="1.0" encoding="utf-8" ?>
<configuration>
<appSettings>
    <add key="ConnectionString" value="Provider=SQLOLEDB.1;Data Source=XP050;Initial Catalog=GHTDB;User Id=sa;Password=;" />
  </appSettings>
</configuration>

2. A target db should be prepared with the relevant structure, following are the instruction for each supported database.

in order to create the testing database, on DB2, run:

NOTICE !!! script does not support remote.
you must run this script on the DB2 machine

"C:\Program Files\IBM\SQLLIB\BIN\DB2CMD.exe" DB2SETCP.BAT DB2.EXE -tvf ghtdb.db2.sql
"C:\Program Files\IBM\SQLLIB\BIN\DB2CMD.exe" DB2SETCP.BAT DB2.EXE -td@ -vf ghtdb.sp.db2.sql
"C:\Program Files\IBM\SQLLIB\BIN\DB2CMD.exe" DB2SETCP.BAT DB2.EXE -tvf ghtdb.data.db2.sql

in order to create the testing database, on SQLServer, run:
Run the scripts with a user wich have administrator permissions. (by default user:sa, password:sa).

osql -S <database> -U <username> -P <password> -i GHTDB.MSSQL.sql
osql -S <database> -U <username> -P <password> -i GHTDB.DATA.MSSQL.sql

for example
osql -S powergh -U sa -P sa -i GHTDB.MSSQL.sql
osql -S powergh -U sa -P sa -i GHTDB.DATA.MSSQL.sql

in order to create the testing database, on ORACLE, run:
Run the scripts with a user wich have administrator permissions. (by default user:system, password:mainsoft).

sqlplus "user/password@database_sid" @GHTDB.ORACLE.sql
sqlplus "user/password@database_sid" @GHTDB.Data.ORACLE.sql

for example 
sqlplus "system/mainsoft@rafim" @GHTDB.ORACLE.sql
sqlplus "system/mainsoft@rafim" @GHTDB.Data.ORACLE.sql

---------------------------------------
to execute postgres sql script from command line
---------------------------------------

First, create the target database (GHTDB as UNICODE) and then, run the script
"C:\Program Files\PostgreSQL\8.0\bin\psql.exe" -U<username> <db name> -f <sql file>


NOTE: if you fail to connect to psql, with the error:
psql: FATAL:  password authentication failed for user "postgres"
change your pg_hba file
from:
host    all         all         127.0.0.1/32          md5
change to:
host    all         all         127.0.0.1/32          trust


example:
"C:\Program Files\PostgreSQL\8.0\bin\psql.exe" -q -Upostgres Store -f Store.PostgreSQL.sql

example:
"C:\Program Files\PostgreSQL\8.0\bin\psql.exe" -q -Upostgres template1 -f GHTDB.CreateDB.sql
"C:\Program Files\PostgreSQL\8.0\bin\psql.exe" -q -Upostgres GHTDB -f GHTDB.PostgreSQL.sql
"C:\Program Files\PostgreSQL\8.0\bin\psql.exe" -q -Upostgres GHTDBEX -f GHTDBEX.PostgreSQL.sql
"C:\Program Files\PostgreSQL\8.0\bin\psql.exe" -q -Upostgres GHTDB -f GHTDB.DATA.PostgreSQL.sql

 =================
Sybase isql readme
=================

start the server
start %SYBASE%\ASE-15_0\bin\sqlsrvr -dmaster.dbs -e C:\ASE150\ASE-15_0\bin\errorlog.log

%SYBASE%\OCS-15_0\bin\isql -Usa -P -iC:\Dev\rafi_view\studio\GH\DevQA\tests\db\sybase\GHTDB.SYBASE.sql -oGHTDB.log
%SYBASE%\OCS-15_0\bin\isql -Usa -P -iC:\Dev\rafi_view\studio\GH\DevQA\tests\db\sybase\GHTDB.DATA.SYBASE.sql -oGHTDB.DATA.log

=================
other usefull tips 
use them when having problems
=================
dump transaction GHTDB with no_log

insensitive
charset -Usa -P noaccents.srt cp850
sp_configure "default sortorder id", 44


