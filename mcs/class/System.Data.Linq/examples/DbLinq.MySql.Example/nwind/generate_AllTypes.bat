REM you need to have DbMetal.exe compiled in the standard directory

..\..\..\DbMetal\bin\DbMetal.exe -Provider=MySql -server:localhost -user:LinqUser -password:[PLACEHOLDER] -database:AllTypes -namespace:AllTypesExample -code:AllTypes.cs 
