CREATE USER 'LinqUser'@'%'; SET PASSWORD FOR 'LinqUser'@'%' = PASSWORD('[PLACEHOLDER]');

##
GRANT Select, Insert, Update, Delete, EXECUTE ON `Northwind`.* TO 'LinqUser'@'%';
  FLUSH PRIVILEGES;
