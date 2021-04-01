To run unit test the following should be prepared:

A target db should be prepared with the relevant structure, following are the instruction for each supported database.

in order to create the testing database, on ORACLE, run:
Run the scripts with a user wich have administrator permissions.

sqlplus "user/password@database_sid" @GHTDB.ORACLE.sql
sqlplus "user/password@database_sid" @GHTDB.Data.ORACLE.sql

for example 
sqlplus "system/mainsoft@rafim" @GHTDB.ORACLE.sql
sqlplus "system/mainsoft@rafim" @GHTDB.Data.ORACLE.sql

