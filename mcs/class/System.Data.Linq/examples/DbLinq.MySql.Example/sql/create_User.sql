CREATE USER 'LinqUser'@'%'; SET PASSWORD FOR 'LinqUser'@'%' = PASSWORD('linq2');

##
GRANT Select, Insert, Update, Delete, EXECUTE ON `Northwind`.* TO 'LinqUser'@'%';
  FLUSH PRIVILEGES;