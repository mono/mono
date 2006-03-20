To run unit test the following should be prepared:

1. Test\System.Data.OracleClient.J2EE.config should contain an ConnectionString setting, i.e.:

<?xml version="1.0" encoding="utf-8" ?>
<configuration>
<appSettings>
    <add key="ConnectionString" value="User ID=ghtdb;Password=ghtdb;Data Source=xp050" />
  </appSettings>
</configuration>

2. A target db should be prepared with the relevant structure, following are the instruction for each supported database.

in order to create the testing database, on ORACLE, run:
Run the scripts with a user wich have administrator permissions. (by default user:system, password:mainsoft).

sqlplus "user/password@database_sid" @GHTDB.ORACLE.sql
sqlplus "user/password@database_sid" @GHTDB.Data.ORACLE.sql

for example 
sqlplus "system/mainsoft@rafim" @GHTDB.ORACLE.sql
sqlplus "system/mainsoft@rafim" @GHTDB.Data.ORACLE.sql

