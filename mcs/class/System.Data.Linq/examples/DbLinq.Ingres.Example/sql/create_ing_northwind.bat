@echo off
echo This batch will throw some errors, if the user already exists or 
echo the database does not exist. You can safely ignore those
echo.
echo You should run this as the installation owner (the user, that installed Ingres)
echo.
echo Tested on Ingres 9.2.0 Build 118

echo Trying to drop database northwind...
destroydb northwind -uLinqUser > nwind.log

echo Creating user LinqUser...
sql iidbdb < create_User.sql >> nwind.log

echo Creating database northwind...
createdb northwind -uLinqUser >> nwind.log

echo Filling data into northwind...
sql northwind -uLinqUser < create_Northwind.sql >> nwind.log

echo Done.
pause
