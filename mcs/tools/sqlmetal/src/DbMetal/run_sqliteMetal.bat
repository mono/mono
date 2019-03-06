@echo off

rem the following script is obsolete

DEL Northwind.db3
type ..\Example\DbLinq.SQLite.Example\sql\create_Northwind.sql | sqlite3 Northwind.db3
type ..\Example\DbLinq.SQLite.Example\sql\create_AllTypes.sql | sqlite3 Northwind.db3

REM: note that the '-sprocs' option is turned on
REM MySqlMetal.exe -database:Northwind -server:localhost -user:LinqUser -password:linq2 -namespace:nwind -dbml:nwind_mysql.dbml -sprocs
bin\SQLiteMetal.exe -database:Northwind.db3 -namespace:nwind -code:Northwind.cs

pause


