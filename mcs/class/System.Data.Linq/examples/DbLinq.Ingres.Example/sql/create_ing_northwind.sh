echo "This batch will throw some errors, if the user already exists or "
echo "the database does not exist. You can safely ignore those"
echo "You should run this as the installation owner (the user, that installed Ingres)"
echo "Tested on Ingres 9.2.0 Build 118"
echo "Trying to drop database northwind..."
destroydb northwind -ulinquser
echo "Creating user LinqUser..."
sql iidbdb < create_User.sql
echo "Creating database northwind..."
createdb northwind -ulinquser
echo "Filling data into northwind..."
sql northwind -ulinquser < create_Northwind.sql
echo "Done."
