Create user "LinqUser" with password: linq2

In order to install the sql scripts you need to use "psql" located in you \bin directory
 of your PostgreSql installation:

psql -U username -d postgres -f "createDB_Northwind_pg.sql"

Remarks:
More information on how to use psql you can find at:
http://www.postgresql.org/docs/8.1/static/app-psql.html

If you are using PgAdmin3 you first need to execute the create database part of the script
and then change to the newly created database in order to execute the rest of the script.

Currently there are names in the scripts with mixed case names, but because quotes are not used around the names
Postgresql accepts them as lower case names! You can then access them only by naming them lower case. 