-- run it as sqlplus /nolog @instnwnd.Oracle.sql

ACCEPT dbname CHAR PROMPT 'Enter database name:  '
ACCEPT sys_pswd CHAR PROMPT 'Enter SYS password:  ' HIDE

connect sys/&sys_pswd@&dbname as SYSDBA

declare
  has_northwind number;
begin 
  select count(*) into has_northwind from all_users where username = 'NORTHWIND';
  if has_northwind = 1 then
    execute immediate 'drop user northwind cascade';
  end if;  
end;
/

create user northwind identified by northwind default tablespace users temporary tablespace temp
/

grant create session to northwind;
grant unlimited tablespace to northwind;
grant create sequence to northwind;
grant create table to northwind;
grant create procedure to northwind;
grant create view to northwind;
grant create trigger to northwind;

connect northwind/northwind@&dbname

set define off
--------------------------------------------------------
--  File created - Sunday-August-09-2009   
--------------------------------------------------------
--------------------------------------------------------
--  DDL for Sequence CATEGORIES_CATEGORYID_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "CATEGORIES_CATEGORYID_SEQ"  MINVALUE 1 MAXVALUE 999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence EMPLOYEES_EMPLOYEEID_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "EMPLOYEES_EMPLOYEEID_SEQ"  MINVALUE 1 MAXVALUE 999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence ORDERS_ORDERID_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "ORDERS_ORDERID_SEQ"  MINVALUE 1 MAXVALUE 999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence PRODUCTS_PRODUCTID_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "PRODUCTS_PRODUCTID_SEQ"  MINVALUE 1 MAXVALUE 999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence SHIPPERS_SHIPPERID_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "SHIPPERS_SHIPPERID_SEQ"  MINVALUE 1 MAXVALUE 999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Sequence SUPPLIERS_SUPPLIERID_SEQ
--------------------------------------------------------

   CREATE SEQUENCE  "SUPPLIERS_SUPPLIERID_SEQ"  MINVALUE 1 MAXVALUE 999999999999999999999999 INCREMENT BY 1 START WITH 1 CACHE 20 NOORDER  NOCYCLE ;
--------------------------------------------------------
--  DDL for Table CATEGORIES
--------------------------------------------------------

  CREATE TABLE "CATEGORIES" 
   (	"CATEGORYID" NUMBER(10,0), 
	"CATEGORYNAME" NVARCHAR2(15), 
	"DESCRIPTION" NCLOB, 
	"PICTURE" BLOB
   ) ;
--------------------------------------------------------
--  DDL for Table CUSTOMERCUSTOMERDEMO
--------------------------------------------------------

  CREATE TABLE "CUSTOMERCUSTOMERDEMO" 
   (	"CUSTOMERID" NCHAR(5), 
	"CUSTOMERTYPEID" NCHAR(10)
   ) ;
--------------------------------------------------------
--  DDL for Table CUSTOMERDEMOGRAPHICS
--------------------------------------------------------

  CREATE TABLE "CUSTOMERDEMOGRAPHICS" 
   (	"CUSTOMERTYPEID" NCHAR(10), 
	"CUSTOMERDESC" NCLOB
   ) ;
--------------------------------------------------------
--  DDL for Table CUSTOMERS
--------------------------------------------------------

  CREATE TABLE "CUSTOMERS" 
   (	"CUSTOMERID" NCHAR(5), 
	"COMPANYNAME" NVARCHAR2(40), 
	"CONTACTNAME" NVARCHAR2(30), 
	"CONTACTTITLE" NVARCHAR2(30), 
	"ADDRESS" NVARCHAR2(60), 
	"CITY" NVARCHAR2(15), 
	"REGION" NVARCHAR2(15), 
	"POSTALCODE" NVARCHAR2(10), 
	"COUNTRY" NVARCHAR2(15), 
	"PHONE" NVARCHAR2(24), 
	"FAX" NVARCHAR2(24)
   ) ;
--------------------------------------------------------
--  DDL for Table EMPLOYEES
--------------------------------------------------------

  CREATE TABLE "EMPLOYEES" 
   (	"EMPLOYEEID" NUMBER(10,0), 
	"LASTNAME" NVARCHAR2(20), 
	"FIRSTNAME" NVARCHAR2(10), 
	"TITLE" NVARCHAR2(30), 
	"TITLEOFCOURTESY" NVARCHAR2(25), 
	"BIRTHDATE" DATE, 
	"HIREDATE" DATE, 
	"ADDRESS" NVARCHAR2(60), 
	"CITY" NVARCHAR2(15), 
	"REGION" NVARCHAR2(15), 
	"POSTALCODE" NVARCHAR2(10), 
	"COUNTRY" NVARCHAR2(15), 
	"HOMEPHONE" NVARCHAR2(24), 
	"EXTENSION" NVARCHAR2(4), 
	"PHOTO" BLOB, 
	"NOTES" NCLOB, 
	"REPORTSTO" NUMBER(10,0), 
	"PHOTOPATH" NVARCHAR2(255)
   ) ;
--------------------------------------------------------
--  DDL for Table EMPLOYEETERRITORIES
--------------------------------------------------------

  CREATE TABLE "EMPLOYEETERRITORIES" 
   (	"EMPLOYEEID" NUMBER(10,0), 
	"TERRITORYID" NVARCHAR2(20)
   ) ;
--------------------------------------------------------
--  DDL for Table ORDERS
--------------------------------------------------------

  CREATE TABLE "ORDERS" 
   (	"ORDERID" NUMBER(10,0), 
	"CUSTOMERID" NCHAR(5), 
	"EMPLOYEEID" NUMBER(10,0), 
	"ORDERDATE" DATE, 
	"REQUIREDDATE" DATE, 
	"SHIPPEDDATE" DATE, 
	"SHIPVIA" NUMBER(10,0), 
	"FREIGHT" NUMBER(19,4) DEFAULT (0), 
	"SHIPNAME" NVARCHAR2(40), 
	"SHIPADDRESS" NVARCHAR2(60), 
	"SHIPCITY" NVARCHAR2(15), 
	"SHIPREGION" NVARCHAR2(15), 
	"SHIPPOSTALCODE" NVARCHAR2(10), 
	"SHIPCOUNTRY" NVARCHAR2(15)
   ) ;
--------------------------------------------------------
--  DDL for Table ORDER_DETAILS
--------------------------------------------------------

  CREATE TABLE "ORDER_DETAILS" 
   (	"ORDERID" NUMBER(10,0), 
	"PRODUCTID" NUMBER(10,0), 
	"UNITPRICE" NUMBER(19,4) DEFAULT (0), 
	"QUANTITY" NUMBER(5,0) DEFAULT (1), 
	"DISCOUNT" FLOAT(126) DEFAULT (0)
   ) ;
 

   COMMENT ON TABLE "ORDER_DETAILS"  IS 'ORIGINAL NAME:Order Details';
--------------------------------------------------------
--  DDL for Table PRODUCTS
--------------------------------------------------------

  CREATE TABLE "PRODUCTS" 
   (	"PRODUCTID" NUMBER(10,0), 
	"PRODUCTNAME" NVARCHAR2(40), 
	"SUPPLIERID" NUMBER(10,0), 
	"CATEGORYID" NUMBER(10,0), 
	"QUANTITYPERUNIT" NVARCHAR2(20), 
	"UNITPRICE" NUMBER(19,4) DEFAULT (0), 
	"UNITSINSTOCK" NUMBER(5,0) DEFAULT (0), 
	"UNITSONORDER" NUMBER(5,0) DEFAULT (0), 
	"REORDERLEVEL" NUMBER(5,0) DEFAULT (0), 
	"DISCONTINUED" NUMBER(1,0) DEFAULT (0)
   ) ;
--------------------------------------------------------
--  DDL for Table REGION
--------------------------------------------------------

  CREATE TABLE "REGION" 
   (	"REGIONID" NUMBER(10,0), 
	"REGIONDESCRIPTION" NCHAR(50)
   ) ;
--------------------------------------------------------
--  DDL for Table SHIPPERS
--------------------------------------------------------

  CREATE TABLE "SHIPPERS" 
   (	"SHIPPERID" NUMBER(10,0), 
	"COMPANYNAME" NVARCHAR2(40), 
	"PHONE" NVARCHAR2(24)
   ) ;
--------------------------------------------------------
--  DDL for Table SUPPLIERS
--------------------------------------------------------

  CREATE TABLE "SUPPLIERS" 
   (	"SUPPLIERID" NUMBER(10,0), 
	"COMPANYNAME" NVARCHAR2(40), 
	"CONTACTNAME" NVARCHAR2(30), 
	"CONTACTTITLE" NVARCHAR2(30), 
	"ADDRESS" NVARCHAR2(60), 
	"CITY" NVARCHAR2(15), 
	"REGION" NVARCHAR2(15), 
	"POSTALCODE" NVARCHAR2(10), 
	"COUNTRY" NVARCHAR2(15), 
	"PHONE" NVARCHAR2(24), 
	"FAX" NVARCHAR2(24), 
	"HOMEPAGE" NCLOB
   ) ;
--------------------------------------------------------
--  DDL for Table TERRITORIES
--------------------------------------------------------

  CREATE TABLE "TERRITORIES" 
   (	"TERRITORYID" NVARCHAR2(20), 
	"TERRITORYDESCRIPTION" NCHAR(50), 
	"REGIONID" NUMBER(10,0)
   ) ;

---------------------------------------------------
--   DATA FOR TABLE CATEGORIES
--   FILTER = none used
---------------------------------------------------
REM INSERTING into CATEGORIES
Insert into CATEGORIES (CATEGORYID,CATEGORYNAME,DESCRIPTION,PICTURE) values (1,'Beverages','Soft drinks, coffees, teas, beers, and ales',hextoraw('0'));
Insert into CATEGORIES (CATEGORYID,CATEGORYNAME,DESCRIPTION,PICTURE) values (2,'Condiments','Sweet and savory sauces, relishes, spreads, and seasonings',hextoraw('0'));
Insert into CATEGORIES (CATEGORYID,CATEGORYNAME,DESCRIPTION,PICTURE) values (3,'Confections','Desserts, candies, and sweet breads',hextoraw('0'));
Insert into CATEGORIES (CATEGORYID,CATEGORYNAME,DESCRIPTION,PICTURE) values (4,'Dairy Products','Cheeses',hextoraw('0'));
Insert into CATEGORIES (CATEGORYID,CATEGORYNAME,DESCRIPTION,PICTURE) values (5,'Grains/Cereals','Breads, crackers, pasta, and cereal',hextoraw('0'));
Insert into CATEGORIES (CATEGORYID,CATEGORYNAME,DESCRIPTION,PICTURE) values (6,'Meat/Poultry','Prepared meats',hextoraw('0'));
Insert into CATEGORIES (CATEGORYID,CATEGORYNAME,DESCRIPTION,PICTURE) values (7,'Produce','Dried fruit and bean curd',hextoraw('0'));
Insert into CATEGORIES (CATEGORYID,CATEGORYNAME,DESCRIPTION,PICTURE) values (8,'Seafood','Seaweed and fish',hextoraw('0'));

---------------------------------------------------
--   END DATA FOR TABLE CATEGORIES
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE CUSTOMERCUSTOMERDEMO
--   FILTER = none used
---------------------------------------------------
REM INSERTING into CUSTOMERCUSTOMERDEMO

---------------------------------------------------
--   END DATA FOR TABLE CUSTOMERCUSTOMERDEMO
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE CUSTOMERDEMOGRAPHICS
--   FILTER = none used
---------------------------------------------------
REM INSERTING into CUSTOMERDEMOGRAPHICS

---------------------------------------------------
--   END DATA FOR TABLE CUSTOMERDEMOGRAPHICS
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE CUSTOMERS
--   FILTER = none used
---------------------------------------------------
REM INSERTING into CUSTOMERS
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('ALFKI','Alfreds Futterkiste','Maria Anders','Sales Representative','Obere Str. 57','Berlin',null,'12209','Germany','030-0074321','030-0076545');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('ANATR','Ana Trujillo Emparedados y helados','Ana Trujillo','Owner','Avda. de la Constitucion 2222','Mexico D.F.',null,'05021','Mexico','(5) 555-4729','(5) 555-3745');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('ANTON','Antonio Moreno Taqueria','Antonio Moreno','Owner','Mataderos  2312','Mexico D.F.',null,'05023','Mexico','(5) 555-3932',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('AROUT','Around the Horn','Thomas Hardy','Sales Representative','120 Hanover Sq.','London',null,'WA1 1DP','UK','(171) 555-7788','(171) 555-6750');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('BERGS','Berglunds snabbkop','Christina Berglund','Order Administrator','Berguvsvagen  8','Lulea',null,'S-958 22','Sweden','0921-12 34 65','0921-12 34 67');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('BLAUS','Blauer See Delikatessen','Hanna Moos','Sales Representative','Forsterstr. 57','Mannheim',null,'68306','Germany','0621-08460','0621-08924');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('BLONP','Blondesddsl pere et fils','Frederique Citeaux','Marketing Manager','24, place Kleber','Strasbourg',null,'67000','France','88.60.15.31','88.60.15.32');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('BOLID','Bolido Comidas preparadas','Martin Sommer','Owner','C/ Araquil, 67','Madrid',null,'28023','Spain','(91) 555 22 82','(91) 555 91 99');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('BONAP','Bon app''','Laurence Lebihan','Owner','12, rue des Bouchers','Marseille',null,'13008','France','91.24.45.40','91.24.45.41');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('BOTTM','Bottom-Dollar Markets','Elizabeth Lincoln','Accounting Manager','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada','(604) 555-4729','(604) 555-3745');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('BSBEV','B''s Beverages','Victoria Ashworth','Sales Representative','Fauntleroy Circus','London',null,'EC2 5NT','UK','(171) 555-1212',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('CACTU','Cactus Comidas para llevar','Patricio Simpson','Sales Agent','Cerrito 333','Buenos Aires',null,'1010','Argentina','(1) 135-5555','(1) 135-4892');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('CENTC','Centro comercial Moctezuma','Francisco Chang','Marketing Manager','Sierras de Granada 9993','Mexico D.F.',null,'05022','Mexico','(5) 555-3392','(5) 555-7293');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('CHOPS','Chop-suey Chinese','Yang Wang','Owner','Hauptstr. 29','Bern',null,'3012','Switzerland','0452-076545',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('COMMI','Comercio Mineiro','Pedro Afonso','Sales Associate','Av. dos Lusiadas, 23','Sao Paulo','SP','05432-043','Brazil','(11) 555-7647',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('CONSH','Consolidated Holdings','Elizabeth Brown','Sales Representative','Berkeley Gardens 12  Brewery','London',null,'WX1 6LT','UK','(171) 555-2282','(171) 555-9199');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('DRACD','Drachenblut Delikatessen','Sven Ottlieb','Order Administrator','Walserweg 21','Aachen',null,'52066','Germany','0241-039123','0241-059428');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('DUMON','Du monde entier','Janine Labrune','Owner','67, rue des Cinquante Otages','Nantes',null,'44000','France','40.67.88.88','40.67.89.89');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('EASTC','Eastern Connection','Ann Devon','Sales Agent','35 King George','London',null,'WX3 6FW','UK','(171) 555-0297','(171) 555-3373');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('ERNSH','Ernst Handel','Roland Mendel','Sales Manager','Kirchgasse 6','Graz',null,'8010','Austria','7675-3425','7675-3426');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('FAMIA','Familia Arquibaldo','Aria Cruz','Marketing Assistant','Rua Oros, 92','Sao Paulo','SP','05442-030','Brazil','(11) 555-9857',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('FISSA','FISSA Fabrica Inter. Salchichas S.A.','Diego Roel','Accounting Manager','C/ Moralzarzal, 86','Madrid',null,'28034','Spain','(91) 555 94 44','(91) 555 55 93');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('FOLIG','Folies gourmandes','Martine Rance','Assistant Sales Agent','184, chaussee de Tournai','Lille',null,'59000','France','20.16.10.16','20.16.10.17');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('FOLKO','Folk och fa HB','Maria Larsson','Owner','Akergatan 24','Bracke',null,'S-844 67','Sweden','0695-34 67 21',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('FRANK','Frankenversand','Peter Franken','Marketing Manager','Berliner Platz 43','Munchen',null,'80805','Germany','089-0877310','089-0877451');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('FRANR','France restauration','Carine Schmitt','Marketing Manager','54, rue Royale','Nantes',null,'44000','France','40.32.21.21','40.32.21.20');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('FRANS','Franchi S.p.A.','Paolo Accorti','Sales Representative','Via Monte Bianco 34','Torino',null,'10100','Italy','011-4988260','011-4988261');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('FURIB','Furia Bacalhau e Frutos do Mar','Lino Rodriguez','Sales Manager','Jardim das rosas n. 32','Lisboa',null,'1675','Portugal','(1) 354-2534','(1) 354-2535');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('GALED','Galeria del gastronomo','Eduardo Saavedra','Marketing Manager','Rambla de Cataluna, 23','Barcelona',null,'08022','Spain','(93) 203 4560','(93) 203 4561');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('GODOS','Godos Cocina Tipica','Jose Pedro Freyre','Sales Manager','C/ Romero, 33','Sevilla',null,'41101','Spain','(95) 555 82 82',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('GOURL','Gourmet Lanchonetes','Andre Fonseca','Sales Associate','Av. Brasil, 442','Campinas','SP','04876-786','Brazil','(11) 555-9482',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('GREAL','Great Lakes Food Market','Howard Snyder','Marketing Manager','2732 Baker Blvd.','Eugene','OR','97403','USA','(503) 555-7555',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('GROSR','GROSELLA-Restaurante','Manuel Pereira','Owner','5? Ave. Los Palos Grandes','Caracas','DF','1081','Venezuela','(2) 283-2951','(2) 283-3397');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('HANAR','Hanari Carnes','Mario Pontes','Accounting Manager','Rua do Paco, 67','Rio de Janeiro','RJ','05454-876','Brazil','(21) 555-0091','(21) 555-8765');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('HILAA','HILARION-Abastos','Carlos Hernandez','Sales Representative','Carrera 22 con Ave. Carlos Soublette #8-35','San Cristobal','Tachira','5022','Venezuela','(5) 555-1340','(5) 555-1948');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('HUNGC','Hungry Coyote Import Store','Yoshi Latimer','Sales Representative','City Center Plaza 516 Main St.','Elgin','OR','97827','USA','(503) 555-6874','(503) 555-2376');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('HUNGO','Hungry Owl All-Night Grocers','Patricia McKenna','Sales Associate','8 Johnstown Road','Cork','Co. Cork',null,'Ireland','2967 542','2967 3333');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('ISLAT','Island Trading','Helen Bennett','Marketing Manager','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK','(198) 555-8888',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('KOENE','Koniglich Essen','Philip Cramer','Sales Associate','Maubelstr. 90','Brandenburg',null,'14776','Germany','0555-09876',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('LACOR','La corne d''abondance','Daniel Tonini','Sales Representative','67, avenue de l''Europe','Versailles',null,'78000','France','30.59.84.10','30.59.85.11');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('LAMAI','La maison d''Asie','Annette Roulet','Sales Manager','1 rue Alsace-Lorraine','Toulouse',null,'31000','France','61.77.61.10','61.77.61.11');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('LAUGB','Laughing Bacchus Wine Cellars','Yoshi Tannamuri','Marketing Assistant','1900 Oak St.','Vancouver','BC','V3F 2K1','Canada','(604) 555-3392','(604) 555-7293');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('LAZYK','Lazy K Kountry Store','John Steel','Marketing Manager','12 Orchestra Terrace','Walla Walla','WA','99362','USA','(509) 555-7969','(509) 555-6221');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('LEHMS','Lehmanns Marktstand','Renate Messner','Sales Representative','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany','069-0245984','069-0245874');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('LETSS','Let''s Stop N Shop','Jaime Yorres','Owner','87 Polk St. Suite 5','San Francisco','CA','94117','USA','(415) 555-5938',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('LILAS','LILA-Supermercado','Carlos Gonzalez','Accounting Manager','Carrera 52 con Ave. Bolivar #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela','(9) 331-6954','(9) 331-7256');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('LINOD','LINO-Delicateses','Felipe Izquierdo','Owner','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela','(8) 34-56-12','(8) 34-93-93');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('LONEP','Lonesome Pine Restaurant','Fran Wilson','Sales Manager','89 Chiaroscuro Rd.','Portland','OR','97219','USA','(503) 555-9573','(503) 555-9646');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('MAGAA','Magazzini Alimentari Riuniti','Giovanni Rovelli','Marketing Manager','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy','035-640230','035-640231');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('MAISD','Maison Dewey','Catherine Dewey','Sales Agent','Rue Joseph-Bens 532','Bruxelles',null,'B-1180','Belgium','(02) 201 24 67','(02) 201 24 68');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('MEREP','Mere Paillarde','Jean Fresniere','Marketing Assistant','43 rue St. Laurent','Montreal','Quebec','H1J 1C3','Canada','(514) 555-8054','(514) 555-8055');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('MORGK','Morgenstern Gesundkost','Alexander Feuer','Marketing Assistant','Heerstr. 22','Leipzig',null,'04179','Germany','0342-023176',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('NORTS','North/South','Simon Crowther','Sales Associate','South House 300 Queensbridge','London',null,'SW7 1RZ','UK','(171) 555-7733','(171) 555-2530');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('OCEAN','Oceano Atlantico Ltda.','Yvonne Moncada','Sales Agent','Ing. Gustavo Moncada 8585 Piso 20-A','Buenos Aires',null,'1010','Argentina','(1) 135-5333','(1) 135-5535');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('OLDWO','Old World Delicatessen','Rene Phillips','Sales Representative','2743 Bering St.','Anchorage','AK','99508','USA','(907) 555-7584','(907) 555-2880');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('OTTIK','Ottilies Kaseladen','Henriette Pfalzheim','Owner','Mehrheimerstr. 369','Koln',null,'50739','Germany','0221-0644327','0221-0765721');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('PARIS','Paris specialites','Marie Bertrand','Owner','265, boulevard Charonne','Paris',null,'75012','France','(1) 42.34.22.66','(1) 42.34.22.77');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('PERIC','Pericles Comidas clasicas','Guillermo Fernandez','Sales Representative','Calle Dr. Jorge Cash 321','Mexico D.F.',null,'05033','Mexico','(5) 552-3745','(5) 545-3745');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('PICCO','Piccolo und mehr','Georg Pipps','Sales Manager','Geislweg 14','Salzburg',null,'5020','Austria','6562-9722','6562-9723');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('PRINI','Princesa Isabel Vinhos','Isabel de Castro','Sales Representative','Estrada da saude n. 58','Lisboa',null,'1756','Portugal','(1) 356-5634',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('QUEDE','Que Delicia','Bernardo Batista','Accounting Manager','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil','(21) 555-4252','(21) 555-4545');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('QUEEN','Queen Cozinha','Lucia Carvalho','Marketing Assistant','Alameda dos Canarios, 891','Sao Paulo','SP','05487-020','Brazil','(11) 555-1189',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('QUICK','QUICK-Stop','Horst Kloss','Accounting Manager','Taucherstra?e 10','Cunewalde',null,'01307','Germany','0372-035188',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('RANCH','Rancho grande','Sergio Gutierrez','Sales Representative','Av. del Libertador 900','Buenos Aires',null,'1010','Argentina','(1) 123-5555','(1) 123-5556');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('RATTC','Rattlesnake Canyon Grocery','Paula Wilson','Assistant Sales Representative','2817 Milton Dr.','Albuquerque','NM','87110','USA','(505) 555-5939','(505) 555-3620');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('REGGC','Reggiani Caseifici','Maurizio Moroni','Sales Associate','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy','0522-556721','0522-556722');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('RICAR','Ricardo Adocicados','Janete Limeira','Assistant Sales Agent','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil','(21) 555-3412',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('RICSU','Richter Supermarkt','Michael Holz','Sales Manager','Grenzacherweg 237','Geneve',null,'1203','Switzerland','0897-034214',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('ROMEY','Romero y tomillo','Alejandra Camino','Accounting Manager','Gran Via, 1','Madrid',null,'28001','Spain','(91) 745 6200','(91) 745 6210');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('SANTG','Sante Gourmet','Jonas Bergulfsen','Owner','Erling Skakkes gate 78','Stavern',null,'4110','Norway','07-98 92 35','07-98 92 47');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('SAVEA','Save-a-lot Markets','Jose Pavarotti','Sales Representative','187 Suffolk Ln.','Boise','ID','83720','USA','(208) 555-8097',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('SEVES','Seven Seas Imports','Hari Kumar','Sales Manager','90 Wadhurst Rd.','London',null,'OX15 4NB','UK','(171) 555-1717','(171) 555-5646');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('SIMOB','Simons bistro','Jytte Petersen','Owner','Vinb?ltet 34','Kobenhavn',null,'1734','Denmark','31 12 34 56','31 13 35 57');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('SPECD','Specialites du monde','Dominique Perrier','Marketing Manager','25, rue Lauriston','Paris',null,'75016','France','(1) 47.55.60.10','(1) 47.55.60.20');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('SPLIR','Split Rail Beer & Ale','Art Braunschweiger','Sales Manager','P.O. Box 555','Lander','WY','82520','USA','(307) 555-4680','(307) 555-6525');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('SUPRD','Supremes delices','Pascale Cartrain','Accounting Manager','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium','(071) 23 67 22 20','(071) 23 67 22 21');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('THEBI','The Big Cheese','Liz Nixon','Marketing Manager','89 Jefferson Way Suite 2','Portland','OR','97201','USA','(503) 555-3612',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('THECR','The Cracker Box','Liu Wong','Marketing Assistant','55 Grizzly Peak Rd.','Butte','MT','59801','USA','(406) 555-5834','(406) 555-8083');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('TOMSP','Toms Spezialitaten','Karin Josephs','Marketing Manager','Luisenstr. 48','Munster',null,'44087','Germany','0251-031259','0251-035695');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('TORTU','Tortuga Restaurante','Miguel Angel Paolino','Owner','Avda. Azteca 123','Mexico D.F.',null,'05033','Mexico','(5) 555-2933',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('TRADH','Tradicao Hipermercados','Anabela Domingues','Sales Representative','Av. Ines de Castro, 414','Sao Paulo','SP','05634-030','Brazil','(11) 555-2167','(11) 555-2168');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('TRAIH','Trail''s Head Gourmet Provisioners','Helvetius Nagy','Sales Associate','722 DaVinci Blvd.','Kirkland','WA','98034','USA','(206) 555-8257','(206) 555-2174');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('VAFFE','Vaffeljernet','Palle Ibsen','Sales Manager','Smagsloget 45','Arhus',null,'8200','Denmark','86 21 32 43','86 22 33 44');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('VICTE','Victuailles en stock','Mary Saveley','Sales Agent','2, rue du Commerce','Lyon',null,'69004','France','78.32.54.86','78.32.54.87');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('VINET','Vins et alcools Chevalier','Paul Henriot','Accounting Manager','59 rue de l''Abbaye','Reims',null,'51100','France','26.47.15.10','26.47.15.11');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('WANDK','Die Wandernde Kuh','Rita Muller','Sales Representative','Adenauerallee 900','Stuttgart',null,'70563','Germany','0711-020361','0711-035428');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('WARTH','Wartian Herkku','Pirkko Koskitalo','Accounting Manager','Torikatu 38','Oulu',null,'90110','Finland','981-443655','981-443655');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('WELLI','Wellington Importadora','Paula Parente','Sales Manager','Rua do Mercado, 12','Resende','SP','08737-363','Brazil','(14) 555-8122',null);
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('WHITC','White Clover Markets','Karl Jablonski','Owner','305 - 14th Ave. S. Suite 3B','Seattle','WA','98128','USA','(206) 555-4112','(206) 555-4115');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('WILMK','Wilman Kala','Matti Karttunen','Owner/Marketing Assistant','Keskuskatu 45','Helsinki',null,'21240','Finland','90-224 8858','90-224 8858');
Insert into CUSTOMERS (CUSTOMERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX) values ('WOLZA','Wolski  Zajazd','Zbyszek Piestrzeniewicz','Owner','ul. Filtrowa 68','Warszawa',null,'01-012','Poland','(26) 642-7012','(26) 642-7012');

---------------------------------------------------
--   END DATA FOR TABLE CUSTOMERS
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE EMPLOYEES
--   FILTER = none used
---------------------------------------------------
REM INSERTING into EMPLOYEES
Insert into EMPLOYEES (EMPLOYEEID,LASTNAME,FIRSTNAME,TITLE,TITLEOFCOURTESY,BIRTHDATE,HIREDATE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,HOMEPHONE,EXTENSION,PHOTO,NOTES,REPORTSTO,PHOTOPATH) values (1,'Davolio','Nancy','Sales Representative','Ms.',to_timestamp('08-12-48 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-92 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),'507 - 20th Ave. E.Apt. 2A','Seattle','WA','98122','USA','(206) 555-9857','5467',hextoraw('0'),'Education includes a BA in psychology from Colorado State University in 1970.  She also completed "The Art of the Cold Call."  Nancy is a member of Toastmasters International.',2,'http://accweb/emmployees/davolio.bmp');
Insert into EMPLOYEES (EMPLOYEEID,LASTNAME,FIRSTNAME,TITLE,TITLEOFCOURTESY,BIRTHDATE,HIREDATE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,HOMEPHONE,EXTENSION,PHOTO,NOTES,REPORTSTO,PHOTOPATH) values (2,'Fuller','Andrew','Vice President, Sales','Dr.',to_timestamp('19-02-52 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-08-92 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),'908 W. Capital Way','Tacoma','WA','98401','USA','(206) 555-9482','3457',hextoraw('0'),'Andrew received his BTS commercial in 1974 and a Ph.D. in international marketing from the University of Dallas in 1981.  He is fluent in French and Italian and reads German.  He joined the company as a sales representative, was promoted to sales manager in January 1992 and to vice president of sales in March 1993.  Andrew is a member of the Sales Management Roundtable, the Seattle Chamber of Commerce, and the Pacific Rim Importers Association.',null,'http://accweb/emmployees/fuller.bmp');
Insert into EMPLOYEES (EMPLOYEEID,LASTNAME,FIRSTNAME,TITLE,TITLEOFCOURTESY,BIRTHDATE,HIREDATE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,HOMEPHONE,EXTENSION,PHOTO,NOTES,REPORTSTO,PHOTOPATH) values (3,'Leverling','Janet','Sales Representative','Ms.',to_timestamp('30-08-63 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-04-92 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),'722 Moss Bay Blvd.','Kirkland','WA','98033','USA','(206) 555-3412','3355',hextoraw('0'),'Janet has a BS degree in chemistry from Boston College (1984).  She has also completed a certificate program in food retailing management.  Janet was hired as a sales associate in 1991 and promoted to sales representative in February 1992.',2,'http://accweb/emmployees/leverling.bmp');
Insert into EMPLOYEES (EMPLOYEEID,LASTNAME,FIRSTNAME,TITLE,TITLEOFCOURTESY,BIRTHDATE,HIREDATE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,HOMEPHONE,EXTENSION,PHOTO,NOTES,REPORTSTO,PHOTOPATH) values (4,'Peacock','Margaret','Sales Representative','Mrs.',to_timestamp('19-09-37 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-05-93 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),'4110 Old Redmond Rd.','Redmond','WA','98052','USA','(206) 555-8122','5176',hextoraw('0'),'Margaret holds a BA in English literature from Concordia College (1958) and an MA from the American Institute of Culinary Arts (1966).  She was assigned to the London office temporarily from July through November 1992.',2,'http://accweb/emmployees/peacock.bmp');
Insert into EMPLOYEES (EMPLOYEEID,LASTNAME,FIRSTNAME,TITLE,TITLEOFCOURTESY,BIRTHDATE,HIREDATE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,HOMEPHONE,EXTENSION,PHOTO,NOTES,REPORTSTO,PHOTOPATH) values (5,'Buchanan','Steven','Sales Manager','Mr.',to_timestamp('04-03-55 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-10-93 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),'14 Garrett Hill','London',null,'SW1 8JR','UK','(71) 555-4848','3453',hextoraw('0'),'Steven Buchanan graduated from St. Andrews University, Scotland, with a BSC degree in 1976.  Upon joining the company as a sales representative in 1992, he spent 6 months in an orientation program at the Seattle office and then returned to his permanent post in London.  He was promoted to sales manager in March 1993.  Mr. Buchanan has completed the courses "Successful Telemarketing" and "International Sales Management."  He is fluent in French.',2,'http://accweb/emmployees/buchanan.bmp');
Insert into EMPLOYEES (EMPLOYEEID,LASTNAME,FIRSTNAME,TITLE,TITLEOFCOURTESY,BIRTHDATE,HIREDATE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,HOMEPHONE,EXTENSION,PHOTO,NOTES,REPORTSTO,PHOTOPATH) values (6,'Suyama','Michael','Sales Representative','Mr.',to_timestamp('02-07-63 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-10-93 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),'Coventry House Miner Rd.','London',null,'EC2 7JR','UK','(71) 555-7773','428',hextoraw('0'),'Michael is a graduate of Sussex University (MA, economics, 1983) and the University of California at Los Angeles (MBA, marketing, 1986).  He has also taken the courses "Multi-Cultural Selling" and "Time Management for the Sales Professional."  He is fluent in Japanese and can read and write French, Portuguese, and Spanish.',5,'http://accweb/emmployees/davolio.bmp');
Insert into EMPLOYEES (EMPLOYEEID,LASTNAME,FIRSTNAME,TITLE,TITLEOFCOURTESY,BIRTHDATE,HIREDATE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,HOMEPHONE,EXTENSION,PHOTO,NOTES,REPORTSTO,PHOTOPATH) values (7,'King','Robert','Sales Representative','Mr.',to_timestamp('29-05-60 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-94 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),'Edgeham Hollow
Winchester Way','London',null,'RG1 9SP','UK','(71) 555-5598','465',hextoraw('0'),'Robert King served in the Peace Corps and traveled extensively before completing his degree in English at the University of Michigan in 1992, the year he joined the company.  After completing a course entitled "Selling in Europe," he was transferred to the London office in March 1993.',5,'http://accweb/emmployees/davolio.bmp');
Insert into EMPLOYEES (EMPLOYEEID,LASTNAME,FIRSTNAME,TITLE,TITLEOFCOURTESY,BIRTHDATE,HIREDATE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,HOMEPHONE,EXTENSION,PHOTO,NOTES,REPORTSTO,PHOTOPATH) values (8,'Callahan','Laura','Inside Sales Coordinator','Ms.',to_timestamp('09-01-58 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-94 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),'4726 - 11th Ave. N.E.','Seattle','WA','98105','USA','(206) 555-1189','2344',hextoraw('0'),'Laura received a BA in psychology from the University of Washington.  She has also completed a course in business French.  She reads and writes French.',2,'http://accweb/emmployees/davolio.bmp');
Insert into EMPLOYEES (EMPLOYEEID,LASTNAME,FIRSTNAME,TITLE,TITLEOFCOURTESY,BIRTHDATE,HIREDATE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,HOMEPHONE,EXTENSION,PHOTO,NOTES,REPORTSTO,PHOTOPATH) values (9,'Dodsworth','Anne','Sales Representative','Ms.',to_timestamp('27-01-66 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-11-94 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),'7 Houndstooth Rd.','London',null,'WG2 7LT','UK','(71) 555-4444','452',hextoraw('0'),'Anne has a BA degree in English from St. Lawrence College.  She is fluent in French and German.',5,'http://accweb/emmployees/davolio.bmp');

---------------------------------------------------
--   END DATA FOR TABLE EMPLOYEES
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE EMPLOYEETERRITORIES
--   FILTER = none used
---------------------------------------------------
REM INSERTING into EMPLOYEETERRITORIES
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (1,'06897');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (1,'19713');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (2,'01581');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (2,'01730');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (2,'01833');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (2,'02116');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (2,'02139');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (2,'02184');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (2,'40222');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (3,'30346');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (3,'31406');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (3,'32859');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (3,'33607');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (4,'20852');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (4,'27403');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (4,'27511');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (5,'02903');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (5,'07960');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (5,'08837');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (5,'10019');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (5,'10038');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (5,'11747');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (5,'14450');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (6,'85014');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (6,'85251');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (6,'98004');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (6,'98052');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (6,'98104');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'60179');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'60601');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'80202');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'80909');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'90405');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'94025');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'94105');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'95008');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'95054');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (7,'95060');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (8,'19428');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (8,'44122');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (8,'45839');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (8,'53404');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (9,'03049');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (9,'03801');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (9,'48075');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (9,'48084');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (9,'48304');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (9,'55113');
Insert into EMPLOYEETERRITORIES (EMPLOYEEID,TERRITORYID) values (9,'55439');

---------------------------------------------------
--   END DATA FOR TABLE EMPLOYEETERRITORIES
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE ORDERS
--   FILTER = none used
---------------------------------------------------
REM INSERTING into ORDERS
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10372,'QUEEN',5,to_timestamp('04-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,890.78,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10373,'HUNGO',4,to_timestamp('05-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,124.12,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10374,'WOLZA',1,to_timestamp('05-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,3.94,'Wolski Zajazd','ul. Filtrowa 68','Warszawa',null,'01-012','Poland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10375,'HUNGC',3,to_timestamp('06-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,20.12,'Hungry Coyote Import Store','City Center Plaza 516 Main St.','Elgin','OR','97827','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10376,'MEREP',1,to_timestamp('09-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,20.39,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10377,'SEVES',1,to_timestamp('09-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,22.21,'Seven Seas Imports','90 Wadhurst Rd.','London',null,'OX15 4NB','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10378,'FOLKO',5,to_timestamp('10-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,5.44,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10379,'QUEDE',2,to_timestamp('11-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,45.03,'Que Del?cia','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10380,'HUNGO',8,to_timestamp('12-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,35.03,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10381,'LILAS',3,to_timestamp('12-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,7.99,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10382,'ERNSH',4,to_timestamp('13-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,94.77,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10383,'AROUT',8,to_timestamp('16-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,34.24,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10384,'BERGS',3,to_timestamp('16-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,168.64,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10385,'SPLIR',1,to_timestamp('17-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,30.96,'Split Rail Beer & Ale','P.O. Box 555','Lander','WY','82520','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10386,'FAMIA',9,to_timestamp('18-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,13.99,'Familia Arquibaldo','Rua Or?s, 92','Sao Paulo','SP','05442-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10387,'SANTG',1,to_timestamp('18-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,93.63,'Sant? Gourmet','Erling Skakkes gate 78','Stavern',null,'4110','Norway');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10388,'SEVES',2,to_timestamp('19-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,34.86,'Seven Seas Imports','90 Wadhurst Rd.','London',null,'OX15 4NB','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10389,'BOTTM',4,to_timestamp('20-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,47.42,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10390,'ERNSH',6,to_timestamp('23-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,126.38,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10391,'DRACD',3,to_timestamp('23-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,5.45,'Drachenblut Delikatessen','Walserweg 21','Aachen',null,'52066','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10392,'PICCO',2,to_timestamp('24-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,122.46,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10393,'SAVEA',1,to_timestamp('25-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,126.56,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10394,'HUNGC',1,to_timestamp('25-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,30.34,'Hungry Coyote Import Store','City Center Plaza 516 Main St.','Elgin','OR','97827','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10395,'HILAA',6,to_timestamp('26-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,184.41,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10396,'FRANK',1,to_timestamp('27-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,135.35,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10397,'PRINI',5,to_timestamp('27-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,60.26,'Princesa Isabel Vinhos','Estrada da sa?de n. 58','Lisboa',null,'1756','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10398,'SAVEA',2,to_timestamp('30-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,89.16,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10399,'VAFFE',8,to_timestamp('31-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,27.36,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10400,'EASTC',1,to_timestamp('01-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,83.93,'Eastern Connection','35 King George','London',null,'WX3 6FW','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10401,'RATTC',1,to_timestamp('01-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,12.51,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10402,'ERNSH',8,to_timestamp('02-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,67.88,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10403,'ERNSH',4,to_timestamp('03-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,73.79,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10404,'MAGAA',2,to_timestamp('03-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,155.97,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10405,'LINOD',1,to_timestamp('06-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,34.82,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10406,'QUEEN',7,to_timestamp('07-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,108.04,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10407,'OTTIK',2,to_timestamp('07-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,91.48,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10408,'FOLIG',8,to_timestamp('08-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,11.26,'Folies gourmandes','184, chauss?e de Tournai','Lille',null,'59000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10409,'OCEAN',3,to_timestamp('09-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,29.83,'Oc?ano Atl?ntico Ltda.','Ing. Gustavo Moncada 8585 Piso 20-A','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10410,'BOTTM',3,to_timestamp('10-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,2.4,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10411,'BOTTM',9,to_timestamp('10-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,23.65,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10412,'WARTH',8,to_timestamp('13-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.77,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10413,'LAMAI',3,to_timestamp('14-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,95.66,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10414,'FAMIA',2,to_timestamp('14-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,21.48,'Familia Arquibaldo','Rua Or?s, 92','Sao Paulo','SP','05442-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10415,'HUNGC',3,to_timestamp('15-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,0.2,'Hungry Coyote Import Store','City Center Plaza 516 Main St.','Elgin','OR','97827','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10416,'WARTH',8,to_timestamp('16-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,22.72,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10417,'SIMOB',4,to_timestamp('16-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,70.29,'Simons bistro','Vinb?ltet 34','Kobenhavn',null,'1734','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10418,'QUICK',4,to_timestamp('17-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,17.55,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10419,'RICSU',4,to_timestamp('20-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,137.35,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10420,'WELLI',3,to_timestamp('21-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,44.12,'Wellington Importadora','Rua do Mercado, 12','Resende','SP','08737-363','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10421,'QUEDE',8,to_timestamp('21-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,99.23,'Que Del?cia','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10422,'FRANS',2,to_timestamp('22-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,3.02,'Franchi S.p.A.','Via Monte Bianco 34','Torino',null,'10100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10423,'GOURL',6,to_timestamp('23-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,24.5,'Gourmet Lanchonetes','Av. Brasil, 442','Campinas','SP','04876-786','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10424,'MEREP',7,to_timestamp('23-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,370.61,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10425,'LAMAI',6,to_timestamp('24-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,7.93,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10426,'GALED',4,to_timestamp('27-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,18.69,'Galer?a del gastron?mo','Rambla de Catalu?a, 23','Barcelona',null,'8022','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10427,'PICCO',4,to_timestamp('27-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,31.29,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10428,'REGGC',7,to_timestamp('28-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,11.09,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10429,'HUNGO',3,to_timestamp('29-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,56.63,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10430,'ERNSH',4,to_timestamp('30-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,458.78,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10431,'BOTTM',4,to_timestamp('30-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,44.17,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10432,'SPLIR',3,to_timestamp('31-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.34,'Split Rail Beer & Ale','P.O. Box 555','Lander','WY','82520','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10433,'PRINI',3,to_timestamp('03-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,73.83,'Princesa Isabel Vinhos','Estrada da sa?de n. 58','Lisboa',null,'1756','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10434,'FOLKO',3,to_timestamp('03-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,17.92,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10435,'CONSH',8,to_timestamp('04-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,9.21,'Consolidated Holdings','Berkeley Gardens 12  Brewery','London',null,'WX1 6LT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10436,'BLONP',3,to_timestamp('05-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,156.66,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10437,'WARTH',8,to_timestamp('05-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,19.97,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10438,'TOMSP',3,to_timestamp('06-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,8.24,'Toms Spezialit?ten','Luisenstr. 48','M?nster',null,'44087','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10439,'MEREP',6,to_timestamp('07-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.07,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10440,'SAVEA',4,to_timestamp('10-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,86.53,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10441,'OLDWO',3,to_timestamp('10-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,73.02,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10442,'ERNSH',3,to_timestamp('11-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,47.94,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10443,'REGGC',8,to_timestamp('12-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,13.95,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10444,'BERGS',3,to_timestamp('12-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,3.5,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10445,'BERGS',3,to_timestamp('13-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,9.3,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10446,'TOMSP',6,to_timestamp('14-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,14.68,'Toms Spezialit?ten','Luisenstr. 48','M?nster',null,'44087','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10447,'RICAR',4,to_timestamp('14-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,68.66,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10448,'RANCH',4,to_timestamp('17-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,38.82,'Rancho grande','Av. del Libertador 900','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10449,'BLONP',3,to_timestamp('18-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,53.3,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10450,'VICTE',8,to_timestamp('19-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,7.23,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10451,'QUICK',4,to_timestamp('19-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,189.09,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10452,'SAVEA',8,to_timestamp('20-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,140.26,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10453,'AROUT',1,to_timestamp('21-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,25.36,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10454,'LAMAI',4,to_timestamp('21-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,2.74,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10248,'VINET',5,to_timestamp('04-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,32.38,'Vins et alcools Chevalier','59 rue de l''Abbaye','Reims',null,'51100','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10249,'TOMSP',6,to_timestamp('05-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,11.61,'Toms Spezialit?ten','Luisenstr. 48','M?nster',null,'44087','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10250,'HANAR',4,to_timestamp('08-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,65.83,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10251,'VICTE',3,to_timestamp('08-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,41.34,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10252,'SUPRD',4,to_timestamp('09-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,51.3,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10253,'HANAR',3,to_timestamp('10-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,58.17,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10254,'CHOPS',5,to_timestamp('11-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,22.98,'Chop-suey Chinese','Hauptstr. 31','Bern',null,'3012','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10255,'RICSU',9,to_timestamp('12-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,148.33,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10256,'WELLI',3,to_timestamp('15-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,13.97,'Wellington Importadora','Rua do Mercado, 12','Resende','SP','08737-363','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10257,'HILAA',4,to_timestamp('16-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,81.91,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10258,'ERNSH',1,to_timestamp('17-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,140.51,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10259,'CENTC',4,to_timestamp('18-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,3.25,'Centro comercial Moctezuma','Sierras de Granada 9993','M?xico D.F.',null,'05022','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10260,'OTTIK',4,to_timestamp('19-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,55.09,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10261,'QUEDE',4,to_timestamp('19-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.05,'Que Del?cia','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10262,'RATTC',8,to_timestamp('22-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,48.29,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10263,'ERNSH',9,to_timestamp('23-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,146.06,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10264,'FOLKO',6,to_timestamp('24-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,3.67,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10265,'BLONP',2,to_timestamp('25-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,55.28,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10266,'WARTH',3,to_timestamp('26-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,25.73,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10267,'FRANK',4,to_timestamp('29-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,208.58,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10268,'GROSR',8,to_timestamp('30-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,66.29,'GROSELLA-Restaurante','5? Ave. Los Palos Grandes','Caracas','DF','1081','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10269,'WHITC',5,to_timestamp('31-07-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,4.56,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10270,'WARTH',1,to_timestamp('01-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,136.54,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10271,'SPLIR',6,to_timestamp('01-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.54,'Split Rail Beer & Ale','P.O. Box 555','Lander','WY','82520','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10272,'RATTC',6,to_timestamp('02-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,98.03,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10273,'QUICK',3,to_timestamp('05-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,76.07,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10274,'VINET',6,to_timestamp('06-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,6.01,'Vins et alcools Chevalier','59 rue de l''Abbaye','Reims',null,'51100','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10275,'MAGAA',1,to_timestamp('07-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,26.93,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10276,'TORTU',8,to_timestamp('08-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,13.84,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10277,'MORGK',2,to_timestamp('09-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,125.77,'Morgenstern Gesundkost','Heerstr. 22','Leipzig',null,'04179','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10278,'BERGS',8,to_timestamp('12-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,92.69,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10279,'LEHMS',8,to_timestamp('13-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,25.83,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10280,'BERGS',2,to_timestamp('14-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,8.98,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10281,'ROMEY',4,to_timestamp('14-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,2.94,'Romero y tomillo','Gran V?a, 1','Madrid',null,'28001','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10282,'ROMEY',4,to_timestamp('15-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,12.69,'Romero y tomillo','Gran V?a, 1','Madrid',null,'28001','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10283,'LILAS',3,to_timestamp('16-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,84.81,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10284,'LEHMS',4,to_timestamp('19-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,76.56,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10285,'QUICK',1,to_timestamp('20-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,76.83,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10286,'QUICK',8,to_timestamp('21-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,229.24,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10287,'RICAR',8,to_timestamp('22-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,12.76,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10288,'REGGC',4,to_timestamp('23-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,7.45,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10289,'BSBEV',7,to_timestamp('26-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,22.77,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10290,'COMMI',8,to_timestamp('27-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,79.7,'Com?rcio Mineiro','Av. dos Lus?adas, 23','Sao Paulo','SP','05432-043','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10291,'QUEDE',6,to_timestamp('27-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,6.4,'Que Del?cia','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10292,'TRADH',1,to_timestamp('28-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.35,'Tradi?ao Hipermercados','Av. In?s de Castro, 414','Sao Paulo','SP','05634-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10293,'TORTU',1,to_timestamp('29-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,21.18,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10294,'RATTC',4,to_timestamp('30-08-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,147.26,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10295,'VINET',2,to_timestamp('02-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.15,'Vins et alcools Chevalier','59 rue de l''Abbaye','Reims',null,'51100','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10296,'LILAS',6,to_timestamp('03-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,0.12,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10297,'BLONP',5,to_timestamp('04-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,5.74,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10298,'HUNGO',6,to_timestamp('05-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,168.22,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10299,'RICAR',4,to_timestamp('06-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,29.76,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10300,'MAGAA',2,to_timestamp('09-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,17.68,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10301,'WANDK',8,to_timestamp('09-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,45.08,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10302,'SUPRD',4,to_timestamp('10-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,6.27,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10303,'GODOS',7,to_timestamp('11-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,107.83,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10304,'TORTU',1,to_timestamp('12-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,63.79,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10305,'OLDWO',8,to_timestamp('13-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,257.62,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10306,'ROMEY',1,to_timestamp('16-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,7.56,'Romero y tomillo','Gran V?a, 1','Madrid',null,'28001','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10307,'LONEP',2,to_timestamp('17-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,0.56,'Lonesome Pine Restaurant','89 Chiaroscuro Rd.','Portland','OR','97219','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10308,'ANATR',7,to_timestamp('18-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.61,'Ana Trujillo Emparedados y helados','Avda. de la Constituci?n 2222','M?xico D.F.',null,'05021','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10309,'HUNGO',3,to_timestamp('19-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,47.3,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10310,'THEBI',8,to_timestamp('20-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,17.52,'The Big Cheese','89 Jefferson Way Suite 2','Portland','OR','97201','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10311,'DUMON',1,to_timestamp('20-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,24.69,'Du monde entier','67, rue des Cinquante Otages','Nantes',null,'44000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10312,'WANDK',2,to_timestamp('23-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,40.26,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10313,'QUICK',2,to_timestamp('24-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.96,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10314,'RATTC',1,to_timestamp('25-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,74.16,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10315,'ISLAT',4,to_timestamp('26-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,41.76,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10316,'RATTC',1,to_timestamp('27-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,150.15,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10317,'LONEP',6,to_timestamp('30-09-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,12.69,'Lonesome Pine Restaurant','89 Chiaroscuro Rd.','Portland','OR','97219','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10318,'ISLAT',8,to_timestamp('01-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.73,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10319,'TORTU',7,to_timestamp('02-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,64.5,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10320,'WARTH',5,to_timestamp('03-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,34.57,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10321,'ISLAT',3,to_timestamp('03-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.43,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10322,'PERIC',7,to_timestamp('04-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,0.4,'Pericles Comidas cl?sicas','Calle Dr. Jorge Cash 321','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10323,'KOENE',4,to_timestamp('07-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,4.88,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10324,'SAVEA',9,to_timestamp('08-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,214.27,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10325,'KOENE',1,to_timestamp('09-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,64.86,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10326,'BOLID',4,to_timestamp('10-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,77.92,'B?lido Comidas preparadas','C/ Araquil, 67','Madrid',null,'28023','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10327,'FOLKO',2,to_timestamp('11-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,63.36,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10328,'FURIB',4,to_timestamp('14-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,87.03,'Furia Bacalhau e Frutos do Mar','Jardim das rosas n. 32','Lisboa',null,'1675','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10329,'SPLIR',4,to_timestamp('15-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,191.67,'Split Rail Beer & Ale','P.O. Box 555','Lander','WY','82520','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10330,'LILAS',3,to_timestamp('16-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,12.75,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10331,'BONAP',9,to_timestamp('16-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,10.19,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10332,'MEREP',3,to_timestamp('17-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,52.84,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10333,'WARTH',5,to_timestamp('18-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,0.59,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10334,'VICTE',8,to_timestamp('21-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,8.56,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10335,'HUNGO',7,to_timestamp('22-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,42.11,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10336,'PRINI',7,to_timestamp('23-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,15.51,'Princesa Isabel Vinhos','Estrada da sa?de n. 58','Lisboa',null,'1756','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10337,'FRANK',4,to_timestamp('24-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,108.26,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10338,'OLDWO',4,to_timestamp('25-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,84.21,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10339,'MEREP',2,to_timestamp('28-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,15.66,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10340,'BONAP',1,to_timestamp('29-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,166.31,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10341,'SIMOB',7,to_timestamp('29-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,26.78,'Simons bistro','Vinb?ltet 34','Kobenhavn',null,'1734','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10342,'FRANK',4,to_timestamp('30-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,54.83,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10343,'LEHMS',4,to_timestamp('31-10-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,110.37,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10344,'WHITC',4,to_timestamp('01-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,23.29,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10345,'QUICK',2,to_timestamp('04-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,249.06,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10346,'RATTC',3,to_timestamp('05-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,142.08,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10347,'FAMIA',4,to_timestamp('06-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,3.1,'Familia Arquibaldo','Rua Or?s, 92','Sao Paulo','SP','05442-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10348,'WANDK',4,to_timestamp('07-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,0.78,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10349,'SPLIR',7,to_timestamp('08-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,8.63,'Split Rail Beer & Ale','P.O. Box 555','Lander','WY','82520','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10350,'LAMAI',6,to_timestamp('11-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,64.19,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10351,'ERNSH',1,to_timestamp('11-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,162.33,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10352,'FURIB',3,to_timestamp('12-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.3,'Furia Bacalhau e Frutos do Mar','Jardim das rosas n. 32','Lisboa',null,'1675','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10353,'PICCO',7,to_timestamp('13-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,360.63,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10354,'PERIC',8,to_timestamp('14-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,53.8,'Pericles Comidas cl?sicas','Calle Dr. Jorge Cash 321','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10355,'AROUT',6,to_timestamp('15-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,41.95,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10356,'WANDK',6,to_timestamp('18-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,36.71,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10357,'LILAS',1,to_timestamp('19-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,34.88,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10358,'LAMAI',5,to_timestamp('20-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,19.64,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10359,'SEVES',5,to_timestamp('21-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,288.43,'Seven Seas Imports','90 Wadhurst Rd.','London',null,'OX15 4NB','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10360,'BLONP',4,to_timestamp('22-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,131.7,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10361,'QUICK',1,to_timestamp('22-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,183.17,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10362,'BONAP',3,to_timestamp('25-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,96.04,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10363,'DRACD',4,to_timestamp('26-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,30.54,'Drachenblut Delikatessen','Walserweg 21','Aachen',null,'52066','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10364,'EASTC',1,to_timestamp('26-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,71.97,'Eastern Connection','35 King George','London',null,'WX3 6FW','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10365,'ANTON',3,to_timestamp('27-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,22,'Antonio Moreno Taquer?a','Mataderos  2312','M?xico D.F.',null,'05023','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10366,'GALED',8,to_timestamp('28-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-01-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,10.14,'Galer?a del gastron?mo','Rambla de Catalu?a, 23','Barcelona',null,'8022','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10367,'VAFFE',7,to_timestamp('28-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,13.55,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10368,'ERNSH',2,to_timestamp('29-11-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,101.95,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10369,'SPLIR',8,to_timestamp('02-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,195.68,'Split Rail Beer & Ale','P.O. Box 555','Lander','WY','82520','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10370,'CHOPS',6,to_timestamp('03-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.17,'Chop-suey Chinese','Hauptstr. 31','Bern',null,'3012','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10371,'LAMAI',1,to_timestamp('03-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-12-96 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,0.45,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10455,'WARTH',8,to_timestamp('24-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,180.45,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10456,'KOENE',8,to_timestamp('25-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,8.12,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10457,'KOENE',2,to_timestamp('25-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,11.57,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10458,'SUPRD',7,to_timestamp('26-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,147.06,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10459,'VICTE',4,to_timestamp('27-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,25.09,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10460,'FOLKO',8,to_timestamp('28-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,16.27,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10461,'LILAS',1,to_timestamp('28-02-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,148.61,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10462,'CONSH',2,to_timestamp('03-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,6.17,'Consolidated Holdings','Berkeley Gardens 12  Brewery','London',null,'WX1 6LT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10463,'SUPRD',5,to_timestamp('04-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,14.78,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10464,'FURIB',4,to_timestamp('04-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,89,'Furia Bacalhau e Frutos do Mar','Jardim das rosas n. 32','Lisboa',null,'1675','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10465,'VAFFE',1,to_timestamp('05-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,145.04,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10466,'COMMI',4,to_timestamp('06-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,11.93,'Com?rcio Mineiro','Av. dos Lus?adas, 23','Sao Paulo','SP','05432-043','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10467,'MAGAA',8,to_timestamp('06-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.93,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10468,'KOENE',3,to_timestamp('07-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,44.12,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10469,'WHITC',1,to_timestamp('10-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,60.18,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10470,'BONAP',4,to_timestamp('11-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,64.56,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10471,'BSBEV',2,to_timestamp('11-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,45.59,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10472,'SEVES',8,to_timestamp('12-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,4.2,'Seven Seas Imports','90 Wadhurst Rd.','London',null,'OX15 4NB','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10473,'ISLAT',1,to_timestamp('13-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,16.37,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10474,'PERIC',5,to_timestamp('13-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,83.49,'Pericles Comidas cl?sicas','Calle Dr. Jorge Cash 321','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10475,'SUPRD',9,to_timestamp('14-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,68.52,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10476,'HILAA',8,to_timestamp('17-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.41,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10477,'PRINI',5,to_timestamp('17-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,13.02,'Princesa Isabel Vinhos','Estrada da sa?de n. 58','Lisboa',null,'1756','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10478,'VICTE',2,to_timestamp('18-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.81,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10479,'RATTC',3,to_timestamp('19-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,708.95,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10480,'FOLIG',6,to_timestamp('20-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.35,'Folies gourmandes','184, chauss?e de Tournai','Lille',null,'59000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10481,'RICAR',8,to_timestamp('20-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,64.33,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10482,'LAZYK',1,to_timestamp('21-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,7.48,'Lazy K Kountry Store','12 Orchestra Terrace','Walla Walla','WA','99362','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10483,'WHITC',7,to_timestamp('24-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,15.28,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10484,'BSBEV',3,to_timestamp('24-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,6.88,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10485,'LINOD',4,to_timestamp('25-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,64.45,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10486,'HILAA',1,to_timestamp('26-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,30.53,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10487,'QUEEN',2,to_timestamp('26-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,71.07,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10488,'FRANK',8,to_timestamp('27-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.93,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10489,'PICCO',6,to_timestamp('28-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,5.29,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10490,'HILAA',7,to_timestamp('31-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,210.19,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10491,'FURIB',8,to_timestamp('31-03-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,16.96,'Furia Bacalhau e Frutos do Mar','Jardim das rosas n. 32','Lisboa',null,'1675','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10492,'BOTTM',3,to_timestamp('01-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,62.89,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10493,'LAMAI',4,to_timestamp('02-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,10.64,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10617,'GREAL',4,to_timestamp('31-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,18.53,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10618,'MEREP',1,to_timestamp('01-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,154.68,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10619,'MEREP',3,to_timestamp('04-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,91.05,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10620,'LAUGB',2,to_timestamp('05-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,0.94,'Laughing Bacchus Wine Cellars','2319 Elm St.','Vancouver','BC','V3F 2K1','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10621,'ISLAT',4,to_timestamp('05-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,23.73,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10622,'RICAR',4,to_timestamp('06-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,50.97,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10623,'FRANK',8,to_timestamp('07-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,97.18,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10624,'THECR',4,to_timestamp('07-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,94.8,'The Cracker Box','55 Grizzly Peak Rd.','Butte','MT','59801','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10625,'ANATR',3,to_timestamp('08-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,43.9,'Ana Trujillo Emparedados y helados','Avda. de la Constituci?n 2222','M?xico D.F.',null,'05021','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10626,'BERGS',1,to_timestamp('11-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,138.69,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10627,'SAVEA',8,to_timestamp('11-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,107.46,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10628,'BLONP',4,to_timestamp('12-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,30.36,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10629,'GODOS',4,to_timestamp('12-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,85.46,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10630,'KOENE',1,to_timestamp('13-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,32.35,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10631,'LAMAI',8,to_timestamp('14-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,0.87,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10632,'WANDK',8,to_timestamp('14-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,41.38,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10633,'ERNSH',7,to_timestamp('15-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,477.9,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10634,'FOLIG',4,to_timestamp('15-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,487.38,'Folies gourmandes','184, chauss?e de Tournai','Lille',null,'59000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10635,'MAGAA',8,to_timestamp('18-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,47.46,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10636,'WARTH',4,to_timestamp('19-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,1.15,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10637,'QUEEN',6,to_timestamp('19-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,201.29,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10638,'LINOD',3,to_timestamp('20-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,158.44,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10639,'SANTG',7,to_timestamp('20-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,38.64,'Sant? Gourmet','Erling Skakkes gate 78','Stavern',null,'4110','Norway');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10640,'WANDK',4,to_timestamp('21-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,23.55,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10641,'HILAA',4,to_timestamp('22-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,179.61,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10642,'SIMOB',7,to_timestamp('22-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,41.89,'Simons bistro','Vinb?ltet 34','Kobenhavn',null,'1734','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10643,'ALFKI',6,to_timestamp('25-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,29.46,'Alfreds Futterkiste','Obere Str. 57','Berlin',null,'12209','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10644,'WELLI',3,to_timestamp('25-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,0.14,'Wellington Importadora','Rua do Mercado, 12','Resende','SP','08737-363','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10645,'HANAR',4,to_timestamp('26-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,12.41,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10646,'HUNGO',9,to_timestamp('27-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,142.33,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10647,'QUEDE',4,to_timestamp('27-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,45.54,'Que Del?cia','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10648,'RICAR',5,to_timestamp('28-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,14.25,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10649,'MAISD',5,to_timestamp('28-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,6.2,'Maison Dewey','Rue Joseph-Bens 532','Bruxelles',null,'B-1180','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10650,'FAMIA',5,to_timestamp('29-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,176.81,'Familia Arquibaldo','Rua Or?s, 92','Sao Paulo','SP','05442-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10651,'WANDK',8,to_timestamp('01-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,20.6,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10652,'GOURL',4,to_timestamp('01-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,7.14,'Gourmet Lanchonetes','Av. Brasil, 442','Campinas','SP','04876-786','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10653,'FRANK',1,to_timestamp('02-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,93.25,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10654,'BERGS',5,to_timestamp('02-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,55.26,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10655,'REGGC',1,to_timestamp('03-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.41,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10656,'GREAL',6,to_timestamp('04-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,57.15,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10657,'SAVEA',2,to_timestamp('04-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,352.69,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10700,'SAVEA',3,to_timestamp('10-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,65.1,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10701,'HUNGO',6,to_timestamp('13-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,220.31,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10702,'ALFKI',4,to_timestamp('13-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,23.94,'Alfred''s Futterkiste','Obere Str. 57','Berlin',null,'12209','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10703,'FOLKO',6,to_timestamp('14-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,152.3,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10704,'QUEEN',6,to_timestamp('14-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,4.78,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10705,'HILAA',9,to_timestamp('15-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.52,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10706,'OLDWO',8,to_timestamp('16-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,135.63,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10707,'AROUT',4,to_timestamp('16-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,21.74,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10708,'THEBI',6,to_timestamp('17-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,2.96,'The Big Cheese','89 Jefferson Way Suite 2','Portland','OR','97201','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10709,'GOURL',1,to_timestamp('17-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,210.8,'Gourmet Lanchonetes','Av. Brasil, 442','Campinas','SP','04876-786','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10710,'FRANS',1,to_timestamp('20-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,4.98,'Franchi S.p.A.','Via Monte Bianco 34','Torino',null,'10100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10711,'SAVEA',5,to_timestamp('21-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,52.41,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10712,'HUNGO',3,to_timestamp('21-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,89.93,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10713,'SAVEA',1,to_timestamp('22-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,167.05,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10714,'SAVEA',5,to_timestamp('22-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,24.49,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10715,'BONAP',3,to_timestamp('23-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,63.2,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10716,'RANCH',4,to_timestamp('24-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,22.57,'Rancho grande','Av. del Libertador 900','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10717,'FRANK',1,to_timestamp('24-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,59.25,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10718,'KOENE',1,to_timestamp('27-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,170.88,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10719,'LETSS',8,to_timestamp('27-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,51.44,'Let''s Stop N Shop','87 Polk St. Suite 5','San Francisco','CA','94117','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10720,'QUEDE',8,to_timestamp('28-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,9.53,'Que Del?cia','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10721,'QUICK',5,to_timestamp('29-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,48.92,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10722,'SAVEA',8,to_timestamp('29-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,74.58,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10723,'WHITC',3,to_timestamp('30-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,21.72,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10724,'MEREP',8,to_timestamp('30-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,57.75,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10725,'FAMIA',4,to_timestamp('31-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,10.83,'Familia Arquibaldo','Rua Or?s, 92','Sao Paulo','SP','05442-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10726,'EASTC',4,to_timestamp('03-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,16.56,'Eastern Connection','35 King George','London',null,'WX3 6FW','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10727,'REGGC',2,to_timestamp('03-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,89.9,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10728,'QUEEN',4,to_timestamp('04-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,58.33,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10729,'LINOD',8,to_timestamp('04-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,141.06,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10730,'BONAP',5,to_timestamp('05-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,20.12,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10731,'CHOPS',7,to_timestamp('06-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,96.65,'Chop-suey Chinese','Hauptstr. 31','Bern',null,'3012','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10732,'BONAP',3,to_timestamp('06-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,16.97,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10733,'BERGS',1,to_timestamp('07-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,110.11,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10734,'GOURL',2,to_timestamp('07-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.63,'Gourmet Lanchonetes','Av. Brasil, 442','Campinas','SP','04876-786','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10735,'LETSS',6,to_timestamp('10-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,45.97,'Let''s Stop N Shop','87 Polk St. Suite 5','San Francisco','CA','94117','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10736,'HUNGO',9,to_timestamp('11-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,44.1,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10737,'VINET',2,to_timestamp('11-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,7.79,'Vins et alcools Chevalier','59 rue de l''Abbaye','Reims',null,'51100','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10738,'SPECD',2,to_timestamp('12-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,2.91,'Sp?cialit?s du monde','25, rue Lauriston','Paris',null,'75016','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10739,'VINET',3,to_timestamp('12-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,11.08,'Vins et alcools Chevalier','59 rue de l''Abbaye','Reims',null,'51100','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10740,'WHITC',4,to_timestamp('13-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,81.88,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10741,'AROUT',4,to_timestamp('14-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,10.96,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10742,'BOTTM',3,to_timestamp('14-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,243.73,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10743,'AROUT',1,to_timestamp('17-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,23.72,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10744,'VAFFE',6,to_timestamp('17-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,69.19,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10745,'QUICK',9,to_timestamp('18-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,3.52,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10746,'CHOPS',1,to_timestamp('19-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,31.43,'Chop-suey Chinese','Hauptstr. 31','Bern',null,'3012','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10747,'PICCO',6,to_timestamp('19-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,117.33,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10748,'SAVEA',3,to_timestamp('20-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,232.55,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10749,'ISLAT',4,to_timestamp('20-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,61.53,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10750,'WARTH',9,to_timestamp('21-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,79.3,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10751,'RICSU',3,to_timestamp('24-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,130.79,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10752,'NORTS',2,to_timestamp('24-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.39,'North/South','South House 300 Queensbridge','London',null,'SW7 1RZ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10753,'FRANS',3,to_timestamp('25-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,7.7,'Franchi S.p.A.','Via Monte Bianco 34','Torino',null,'10100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10754,'MAGAA',6,to_timestamp('25-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,2.38,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10755,'BONAP',4,to_timestamp('26-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,16.71,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10756,'SPLIR',8,to_timestamp('27-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,73.21,'Split Rail Beer & Ale','P.O. Box 555','Lander','WY','82520','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10757,'SAVEA',6,to_timestamp('27-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,8.19,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10758,'RICSU',3,to_timestamp('28-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,138.17,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10759,'ANATR',3,to_timestamp('28-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,11.99,'Ana Trujillo Emparedados y helados','Avda. de la Constituci?n 2222','M?xico D.F.',null,'05021','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10760,'MAISD',4,to_timestamp('01-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,155.64,'Maison Dewey','Rue Joseph-Bens 532','Bruxelles',null,'B-1180','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10761,'RATTC',5,to_timestamp('02-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,18.66,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10762,'FOLKO',3,to_timestamp('02-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,328.74,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10763,'FOLIG',3,to_timestamp('03-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,37.35,'Folies gourmandes','184, chauss?e de Tournai','Lille',null,'59000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10764,'ERNSH',6,to_timestamp('03-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,145.45,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10765,'QUICK',3,to_timestamp('04-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,42.74,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10766,'OTTIK',4,to_timestamp('05-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,157.55,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10767,'SUPRD',4,to_timestamp('05-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.59,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10768,'AROUT',3,to_timestamp('08-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,146.32,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10769,'VAFFE',3,to_timestamp('08-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,65.06,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10770,'HANAR',8,to_timestamp('09-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,5.32,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10771,'ERNSH',9,to_timestamp('10-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,11.19,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10772,'LEHMS',3,to_timestamp('10-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,91.28,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10773,'ERNSH',1,to_timestamp('11-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,96.43,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10774,'FOLKO',4,to_timestamp('11-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,48.2,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10775,'THECR',7,to_timestamp('12-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,20.25,'The Cracker Box','55 Grizzly Peak Rd.','Butte','MT','59801','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10776,'ERNSH',1,to_timestamp('15-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,351.53,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10777,'GOURL',7,to_timestamp('15-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.01,'Gourmet Lanchonetes','Av. Brasil, 442','Campinas','SP','04876-786','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10778,'BERGS',3,to_timestamp('16-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,6.79,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10779,'MORGK',3,to_timestamp('16-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,58.13,'Morgenstern Gesundkost','Heerstr. 22','Leipzig',null,'04179','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10780,'LILAS',2,to_timestamp('16-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,42.13,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10781,'WARTH',2,to_timestamp('17-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,73.16,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10782,'CACTU',9,to_timestamp('17-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.1,'Cactus Comidas para llevar','Cerrito 333','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10783,'HANAR',4,to_timestamp('18-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,124.98,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10658,'QUICK',4,to_timestamp('05-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,364.15,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10659,'QUEEN',7,to_timestamp('05-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,105.81,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10660,'HUNGC',8,to_timestamp('08-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,111.29,'Hungry Coyote Import Store','City Center Plaza 516 Main St.','Elgin','OR','97827','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10661,'HUNGO',7,to_timestamp('09-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,17.55,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10662,'LONEP',3,to_timestamp('09-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.28,'Lonesome Pine Restaurant','89 Chiaroscuro Rd.','Portland','OR','97219','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10663,'BONAP',2,to_timestamp('10-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,113.15,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10664,'FURIB',1,to_timestamp('10-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.27,'Furia Bacalhau e Frutos do Mar','Jardim das rosas n. 32','Lisboa',null,'1675','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10665,'LONEP',1,to_timestamp('11-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,26.31,'Lonesome Pine Restaurant','89 Chiaroscuro Rd.','Portland','OR','97219','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10666,'RICSU',7,to_timestamp('12-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,232.42,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10667,'ERNSH',7,to_timestamp('12-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,78.09,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10668,'WANDK',1,to_timestamp('15-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,47.22,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10669,'SIMOB',2,to_timestamp('15-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,24.39,'Simons bistro','Vinb?ltet 34','Kobenhavn',null,'1734','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10670,'FRANK',4,to_timestamp('16-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,203.48,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10671,'FRANR',1,to_timestamp('17-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,30.34,'France restauration','54, rue Royale','Nantes',null,'44000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10672,'BERGS',9,to_timestamp('17-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,95.75,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10673,'WILMK',2,to_timestamp('18-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,22.76,'Wilman Kala','Keskuskatu 45','Helsinki',null,'21240','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10674,'ISLAT',4,to_timestamp('18-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,0.9,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10675,'FRANK',5,to_timestamp('19-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,31.85,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10676,'TORTU',2,to_timestamp('22-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,2.01,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10677,'ANTON',1,to_timestamp('22-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.03,'Antonio Moreno Taquer?a','Mataderos  2312','M?xico D.F.',null,'05023','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10678,'SAVEA',7,to_timestamp('23-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,388.98,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10679,'BLONP',8,to_timestamp('23-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,27.94,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10680,'OLDWO',1,to_timestamp('24-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,26.61,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10681,'GREAL',3,to_timestamp('25-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,76.13,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10682,'ANTON',3,to_timestamp('25-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,36.13,'Antonio Moreno Taquer?a','Mataderos  2312','M?xico D.F.',null,'05023','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10683,'DUMON',2,to_timestamp('26-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,4.4,'Du monde entier','67, rue des Cinquante Otages','Nantes',null,'44000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10684,'OTTIK',3,to_timestamp('26-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,145.63,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10685,'GOURL',4,to_timestamp('29-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,33.75,'Gourmet Lanchonetes','Av. Brasil, 442','Campinas','SP','04876-786','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10686,'PICCO',2,to_timestamp('30-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,96.5,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10687,'HUNGO',9,to_timestamp('30-09-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,296.43,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10688,'VAFFE',4,to_timestamp('01-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,299.09,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10689,'BERGS',1,to_timestamp('01-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,13.42,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10690,'HANAR',1,to_timestamp('02-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,15.8,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10691,'QUICK',2,to_timestamp('03-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,810.05,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10692,'ALFKI',4,to_timestamp('03-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,61.02,'Alfred''s Futterkiste','Obere Str. 57','Berlin',null,'12209','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10693,'WHITC',3,to_timestamp('06-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,139.34,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10694,'QUICK',8,to_timestamp('06-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,398.36,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10695,'WILMK',7,to_timestamp('07-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,16.72,'Wilman Kala','Keskuskatu 45','Helsinki',null,'21240','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10696,'WHITC',8,to_timestamp('08-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,102.55,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10697,'LINOD',3,to_timestamp('08-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,45.52,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10698,'ERNSH',4,to_timestamp('09-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,272.47,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10699,'MORGK',3,to_timestamp('09-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-11-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-10-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,0.58,'Morgenstern Gesundkost','Heerstr. 22','Leipzig',null,'04179','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10494,'COMMI',4,to_timestamp('02-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,65.99,'Com?rcio Mineiro','Av. dos Lus?adas, 23','Sao Paulo','SP','05432-043','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10495,'LAUGB',3,to_timestamp('03-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.65,'Laughing Bacchus Wine Cellars','2319 Elm St.','Vancouver','BC','V3F 2K1','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10496,'TRADH',7,to_timestamp('04-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,46.77,'Tradi?ao Hipermercados','Av. In?s de Castro, 414','Sao Paulo','SP','05634-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10497,'LEHMS',7,to_timestamp('04-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,36.21,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10498,'HILAA',8,to_timestamp('07-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,29.75,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10499,'LILAS',4,to_timestamp('08-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,102.02,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10500,'LAMAI',6,to_timestamp('09-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,42.68,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10501,'BLAUS',9,to_timestamp('09-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,8.85,'Blauer See Delikatessen','Forsterstr. 57','Mannheim',null,'68306','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10502,'PERIC',2,to_timestamp('10-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,69.32,'Pericles Comidas cl?sicas','Calle Dr. Jorge Cash 321','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10503,'HUNGO',6,to_timestamp('11-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,16.74,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10504,'WHITC',4,to_timestamp('11-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,59.13,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10505,'MEREP',3,to_timestamp('14-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,7.13,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10506,'KOENE',9,to_timestamp('15-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,21.19,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10507,'ANTON',7,to_timestamp('15-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,47.45,'Antonio Moreno Taquer?a','Mataderos  2312','M?xico D.F.',null,'05023','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10508,'OTTIK',1,to_timestamp('16-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.99,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10509,'BLAUS',4,to_timestamp('17-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,0.15,'Blauer See Delikatessen','Forsterstr. 57','Mannheim',null,'68306','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10510,'SAVEA',6,to_timestamp('18-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,367.63,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10511,'BONAP',4,to_timestamp('18-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,350.64,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10512,'FAMIA',7,to_timestamp('21-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.53,'Familia Arquibaldo','Rua Or?s, 92','Sao Paulo','SP','05442-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10513,'WANDK',7,to_timestamp('22-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,105.65,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10514,'ERNSH',3,to_timestamp('22-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,789.95,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10515,'QUICK',2,to_timestamp('23-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,204.47,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10516,'HUNGO',2,to_timestamp('24-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,62.78,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10517,'NORTS',3,to_timestamp('24-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,32.07,'North/South','South House 300 Queensbridge','London',null,'SW7 1RZ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10518,'TORTU',4,to_timestamp('25-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,218.15,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10519,'CHOPS',6,to_timestamp('28-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,91.76,'Chop-suey Chinese','Hauptstr. 31','Bern',null,'3012','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10520,'SANTG',7,to_timestamp('29-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,13.37,'Sant? Gourmet','Erling Skakkes gate 78','Stavern',null,'4110','Norway');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10521,'CACTU',8,to_timestamp('29-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,17.22,'Cactus Comidas para llevar','Cerrito 333','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10522,'LEHMS',4,to_timestamp('30-04-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,45.33,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10523,'SEVES',7,to_timestamp('01-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,77.63,'Seven Seas Imports','90 Wadhurst Rd.','London',null,'OX15 4NB','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10524,'BERGS',1,to_timestamp('01-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,244.79,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10525,'BONAP',1,to_timestamp('02-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,11.06,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10526,'WARTH',4,to_timestamp('05-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,58.59,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10527,'QUICK',7,to_timestamp('05-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,41.9,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10528,'GREAL',6,to_timestamp('06-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.35,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10529,'MAISD',5,to_timestamp('07-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,66.69,'Maison Dewey','Rue Joseph-Bens 532','Bruxelles',null,'B-1180','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10530,'PICCO',3,to_timestamp('08-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,339.22,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10531,'OCEAN',7,to_timestamp('08-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,8.12,'Oc?ano Atl?ntico Ltda.','Ing. Gustavo Moncada 8585 Piso 20-A','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10532,'EASTC',7,to_timestamp('09-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,74.46,'Eastern Connection','35 King George','London',null,'WX3 6FW','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10533,'FOLKO',8,to_timestamp('12-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,188.04,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10534,'LEHMS',8,to_timestamp('12-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,27.94,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10535,'ANTON',4,to_timestamp('13-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,15.64,'Antonio Moreno Taquer?a','Mataderos  2312','M?xico D.F.',null,'05023','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10536,'LEHMS',3,to_timestamp('14-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,58.88,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10537,'RICSU',1,to_timestamp('14-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,78.85,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10538,'BSBEV',9,to_timestamp('15-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.87,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10539,'BSBEV',6,to_timestamp('16-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,12.36,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10540,'QUICK',3,to_timestamp('19-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1007.64,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10541,'HANAR',2,to_timestamp('19-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,68.65,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10542,'KOENE',1,to_timestamp('20-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,10.95,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10543,'LILAS',8,to_timestamp('21-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,48.17,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10544,'LONEP',4,to_timestamp('21-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,24.91,'Lonesome Pine Restaurant','89 Chiaroscuro Rd.','Portland','OR','97219','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10545,'LAZYK',8,to_timestamp('22-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,11.92,'Lazy K Kountry Store','12 Orchestra Terrace','Walla Walla','WA','99362','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10546,'VICTE',1,to_timestamp('23-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,194.72,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10547,'SEVES',3,to_timestamp('23-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,178.43,'Seven Seas Imports','90 Wadhurst Rd.','London',null,'OX15 4NB','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10548,'TOMSP',3,to_timestamp('26-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.43,'Toms Spezialit?ten','Luisenstr. 48','M?nster',null,'44087','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10549,'QUICK',5,to_timestamp('27-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,171.24,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10550,'GODOS',7,to_timestamp('28-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.32,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10551,'FURIB',4,to_timestamp('28-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,72.95,'Furia Bacalhau e Frutos do Mar','Jardim das rosas n. 32','Lisboa',null,'1675','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10552,'HILAA',2,to_timestamp('29-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,83.22,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10553,'WARTH',2,to_timestamp('30-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,149.49,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10554,'OTTIK',4,to_timestamp('30-05-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,120.97,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10555,'SAVEA',6,to_timestamp('02-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,252.49,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10556,'SIMOB',2,to_timestamp('03-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,9.8,'Simons bistro','Vinb?ltet 34','Kobenhavn',null,'1734','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10557,'LEHMS',9,to_timestamp('03-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,96.72,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10558,'AROUT',1,to_timestamp('04-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,72.97,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10559,'BLONP',6,to_timestamp('05-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,8.05,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10560,'FRANK',8,to_timestamp('06-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,36.65,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10561,'FOLKO',2,to_timestamp('06-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,242.21,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10562,'REGGC',1,to_timestamp('09-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,22.95,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10563,'RICAR',2,to_timestamp('10-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,60.43,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10564,'RATTC',4,to_timestamp('10-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,13.75,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10565,'MEREP',8,to_timestamp('11-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,7.15,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10566,'BLONP',9,to_timestamp('12-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,88.4,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10567,'HUNGO',1,to_timestamp('12-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,33.97,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10568,'GALED',3,to_timestamp('13-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,6.54,'Galer?a del gastron?mo','Rambla de Catalu?a, 23','Barcelona',null,'8022','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10569,'RATTC',5,to_timestamp('16-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,58.98,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10570,'MEREP',3,to_timestamp('17-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,188.99,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10571,'ERNSH',8,to_timestamp('17-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,26.06,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10572,'BERGS',3,to_timestamp('18-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,116.43,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10573,'ANTON',7,to_timestamp('19-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,84.84,'Antonio Moreno Taquer?a','Mataderos  2312','M?xico D.F.',null,'05023','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10574,'TRAIH',4,to_timestamp('19-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,37.6,'Trail''s Head Gourmet Provisioners','722 DaVinci Blvd.','Kirkland','WA','98034','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10575,'MORGK',5,to_timestamp('20-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,127.34,'Morgenstern Gesundkost','Heerstr. 22','Leipzig',null,'04179','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10576,'TORTU',3,to_timestamp('23-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,18.56,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10577,'TRAIH',9,to_timestamp('23-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,25.41,'Trail''s Head Gourmet Provisioners','722 DaVinci Blvd.','Kirkland','WA','98034','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10578,'BSBEV',4,to_timestamp('24-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,29.6,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10579,'LETSS',1,to_timestamp('25-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,13.73,'Let''s Stop N Shop','87 Polk St. Suite 5','San Francisco','CA','94117','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10580,'OTTIK',4,to_timestamp('26-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,75.89,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10581,'FAMIA',3,to_timestamp('26-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,3.01,'Familia Arquibaldo','Rua Or?s, 92','Sao Paulo','SP','05442-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10582,'BLAUS',3,to_timestamp('27-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,27.71,'Blauer See Delikatessen','Forsterstr. 57','Mannheim',null,'68306','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10583,'WARTH',2,to_timestamp('30-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,7.28,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10584,'BLONP',4,to_timestamp('30-06-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,59.14,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10585,'WELLI',7,to_timestamp('01-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,13.41,'Wellington Importadora','Rua do Mercado, 12','Resende','SP','08737-363','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10586,'REGGC',9,to_timestamp('02-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,0.48,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10587,'QUEDE',1,to_timestamp('02-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,62.52,'Que Del?cia','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10588,'QUICK',2,to_timestamp('03-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,194.67,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10589,'GREAL',8,to_timestamp('04-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.42,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10590,'MEREP',4,to_timestamp('07-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,44.77,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10591,'VAFFE',1,to_timestamp('07-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,55.92,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10592,'LEHMS',3,to_timestamp('08-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,32.1,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10593,'LEHMS',7,to_timestamp('09-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,174.2,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10594,'OLDWO',3,to_timestamp('09-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,5.24,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10595,'ERNSH',2,to_timestamp('10-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,96.78,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10596,'WHITC',8,to_timestamp('11-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,16.34,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10597,'PICCO',7,to_timestamp('11-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,35.12,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10598,'RATTC',1,to_timestamp('14-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,44.42,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10599,'BSBEV',6,to_timestamp('15-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,29.98,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10600,'HUNGC',4,to_timestamp('16-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,45.13,'Hungry Coyote Import Store','City Center Plaza 516 Main St.','Elgin','OR','97827','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10601,'HILAA',7,to_timestamp('16-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,58.3,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10602,'VAFFE',8,to_timestamp('17-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,2.92,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10603,'SAVEA',8,to_timestamp('18-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,48.77,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10604,'FURIB',1,to_timestamp('18-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,7.46,'Furia Bacalhau e Frutos do Mar','Jardim das rosas n. 32','Lisboa',null,'1675','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10605,'MEREP',1,to_timestamp('21-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,379.13,'M?re Paillarde','43 rue St. Laurent','Montr?al','Qu?bec','H1J 1C3','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10606,'TRADH',4,to_timestamp('22-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,79.4,'Tradi?ao Hipermercados','Av. In?s de Castro, 414','Sao Paulo','SP','05634-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10607,'SAVEA',5,to_timestamp('22-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,200.24,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10608,'TOMSP',4,to_timestamp('23-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,27.79,'Toms Spezialit?ten','Luisenstr. 48','M?nster',null,'44087','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10609,'DUMON',7,to_timestamp('24-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.85,'Du monde entier','67, rue des Cinquante Otages','Nantes',null,'44000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10610,'LAMAI',8,to_timestamp('25-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,26.78,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10611,'WOLZA',6,to_timestamp('25-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,80.65,'Wolski Zajazd','ul. Filtrowa 68','Warszawa',null,'01-012','Poland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10612,'SAVEA',1,to_timestamp('28-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,544.08,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10613,'HILAA',4,to_timestamp('29-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,8.11,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10614,'BLAUS',8,to_timestamp('29-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.93,'Blauer See Delikatessen','Forsterstr. 57','Mannheim',null,'68306','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10615,'WILMK',2,to_timestamp('30-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,0.75,'Wilman Kala','Keskuskatu 45','Helsinki',null,'21240','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10616,'GREAL',1,to_timestamp('31-07-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-08-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,116.53,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10906,'WOLZA',4,to_timestamp('25-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,26.29,'Wolski Zajazd','ul. Filtrowa 68','Warszawa',null,'01-012','Poland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10907,'SPECD',6,to_timestamp('25-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,9.19,'Sp?cialit?s du monde','25, rue Lauriston','Paris',null,'75016','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10908,'REGGC',4,to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,32.96,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10909,'SANTG',1,to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,53.05,'Sant? Gourmet','Erling Skakkes gate 78','Stavern',null,'4110','Norway');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10910,'WILMK',1,to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,38.11,'Wilman Kala','Keskuskatu 45','Helsinki',null,'21240','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10911,'GODOS',3,to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,38.19,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10912,'HUNGO',2,to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,580.91,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10913,'QUEEN',4,to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,33.05,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10914,'QUEEN',6,to_timestamp('27-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,21.19,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10915,'TORTU',2,to_timestamp('27-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.51,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10916,'RANCH',1,to_timestamp('27-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,63.77,'Rancho grande','Av. del Libertador 900','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10917,'ROMEY',4,to_timestamp('02-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,8.29,'Romero y tomillo','Gran V?a, 1','Madrid',null,'28001','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10918,'BOTTM',3,to_timestamp('02-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,48.83,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10919,'LINOD',2,to_timestamp('02-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,19.8,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10920,'AROUT',4,to_timestamp('03-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,29.61,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10921,'VAFFE',1,to_timestamp('03-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,176.48,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10922,'HANAR',5,to_timestamp('03-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,62.74,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10923,'LAMAI',7,to_timestamp('03-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,68.26,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10924,'BERGS',3,to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,151.52,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10925,'HANAR',3,to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,2.27,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10926,'ANATR',4,to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,39.92,'Ana Trujillo Emparedados y helados','Avda. de la Constituci?n 2222','M?xico D.F.',null,'05021','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10927,'LACOR',4,to_timestamp('05-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,19.79,'La corne d''abondance','67, avenue de l''Europe','Versailles',null,'78000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10928,'GALED',1,to_timestamp('05-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,1.36,'Galer?a del gastron?mo','Rambla de Catalu?a, 23','Barcelona',null,'8022','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10929,'FRANK',6,to_timestamp('05-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,33.93,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10930,'SUPRD',4,to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,15.55,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10931,'RICSU',4,to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,13.6,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10932,'BONAP',8,to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,134.64,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10933,'ISLAT',6,to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,54.15,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10934,'LEHMS',3,to_timestamp('09-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,32.01,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10935,'WELLI',4,to_timestamp('09-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,47.59,'Wellington Importadora','Rua do Mercado, 12','Resende','SP','08737-363','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10936,'GREAL',3,to_timestamp('09-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,33.68,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10937,'CACTU',7,to_timestamp('10-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,31.51,'Cactus Comidas para llevar','Cerrito 333','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10938,'QUICK',3,to_timestamp('10-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,31.89,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10939,'MAGAA',2,to_timestamp('10-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,76.33,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10940,'BONAP',8,to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,19.77,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10941,'SAVEA',7,to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,400.81,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10942,'REGGC',9,to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,17.95,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10943,'BSBEV',4,to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,2.17,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10944,'BOTTM',6,to_timestamp('12-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,52.92,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10945,'MORGK',4,to_timestamp('12-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,10.22,'Morgenstern Gesundkost','Heerstr. 22','Leipzig',null,'04179','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10946,'VAFFE',1,to_timestamp('12-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,27.2,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10987,'EASTC',8,to_timestamp('31-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,185.48,'Eastern Connection','35 King George','London',null,'WX3 6FW','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10988,'RATTC',3,to_timestamp('31-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,61.14,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10989,'QUEDE',2,to_timestamp('31-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,34.76,'Que Del?cia','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10990,'ERNSH',2,to_timestamp('01-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,117.61,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10991,'QUICK',1,to_timestamp('01-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,38.51,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10992,'THEBI',1,to_timestamp('01-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.27,'The Big Cheese','89 Jefferson Way Suite 2','Portland','OR','97201','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10993,'FOLKO',7,to_timestamp('01-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,8.81,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10994,'VAFFE',2,to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,65.53,'Vaffeljernet','Smagsloget 45','?rhus',null,'8200','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10995,'PERIC',1,to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,46,'Pericles Comidas cl?sicas','Calle Dr. Jorge Cash 321','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10996,'QUICK',4,to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.12,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10997,'LILAS',8,to_timestamp('03-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,73.91,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10998,'WOLZA',8,to_timestamp('03-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,20.31,'Wolski Zajazd','ul. Filtrowa 68','Warszawa',null,'01-012','Poland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10999,'OTTIK',6,to_timestamp('03-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,96.35,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11000,'RATTC',2,to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,55.12,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11001,'FOLKO',2,to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,197.3,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11002,'SAVEA',4,to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,141.16,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11003,'THECR',3,to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,14.91,'The Cracker Box','55 Grizzly Peak Rd.','Butte','MT','59801','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11004,'MAISD',3,to_timestamp('07-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,44.84,'Maison Dewey','Rue Joseph-Bens 532','Bruxelles',null,'B-1180','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11005,'WILMK',2,to_timestamp('07-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,0.75,'Wilman Kala','Keskuskatu 45','Helsinki',null,'21240','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11006,'GREAL',3,to_timestamp('07-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,25.19,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11007,'PRINI',8,to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,202.24,'Princesa Isabel Vinhos','Estrada da sa?de n. 58','Lisboa',null,'1756','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11008,'ERNSH',7,to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,3,79.46,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11009,'GODOS',2,to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,59.11,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11010,'REGGC',2,to_timestamp('09-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,28.71,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11011,'ALFKI',3,to_timestamp('09-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,1.21,'Alfred''s Futterkiste','Obere Str. 57','Berlin',null,'12209','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11012,'FRANK',1,to_timestamp('09-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,242.95,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11013,'ROMEY',2,to_timestamp('09-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,32.99,'Romero y tomillo','Gran V?a, 1','Madrid',null,'28001','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11014,'LINOD',2,to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,23.6,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11015,'SANTG',2,to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.62,'Sant? Gourmet','Erling Skakkes gate 78','Stavern',null,'4110','Norway');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11016,'AROUT',9,to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,33.8,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11017,'ERNSH',9,to_timestamp('13-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,754.26,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11018,'LONEP',4,to_timestamp('13-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,11.65,'Lonesome Pine Restaurant','89 Chiaroscuro Rd.','Portland','OR','97219','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11019,'RANCH',6,to_timestamp('13-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,3,3.17,'Rancho grande','Av. del Libertador 900','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11020,'OTTIK',2,to_timestamp('14-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,43.3,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11021,'QUICK',3,to_timestamp('14-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,297.18,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11022,'HANAR',9,to_timestamp('14-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,6.27,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11023,'BSBEV',1,to_timestamp('14-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,123.83,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11024,'EASTC',4,to_timestamp('15-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,74.36,'Eastern Connection','35 King George','London',null,'WX3 6FW','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11025,'WARTH',6,to_timestamp('15-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,29.17,'Wartian Herkku','Torikatu 38','Oulu',null,'90110','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11026,'FRANS',4,to_timestamp('15-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,47.09,'Franchi S.p.A.','Via Monte Bianco 34','Torino',null,'10100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11027,'BOTTM',1,to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,52.52,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11028,'KOENE',2,to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,29.59,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11029,'CHOPS',4,to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,47.84,'Chop-suey Chinese','Hauptstr. 31','Bern',null,'3012','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11030,'SAVEA',7,to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,830.75,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11031,'SAVEA',6,to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,227.22,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11032,'WHITC',2,to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,606.19,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11033,'RICSU',7,to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,84.74,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11034,'OLDWO',8,to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,40.32,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11035,'SUPRD',2,to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,0.17,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11036,'DRACD',8,to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,149.47,'Drachenblut Delikatessen','Walserweg 21','Aachen',null,'52066','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11037,'GODOS',7,to_timestamp('21-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,3.2,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11038,'SUPRD',1,to_timestamp('21-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,29.59,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11039,'LINOD',1,to_timestamp('21-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,65,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11040,'GREAL',4,to_timestamp('22-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,3,18.84,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11041,'CHOPS',3,to_timestamp('22-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,48.22,'Chop-suey Chinese','Hauptstr. 31','Bern',null,'3012','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11042,'COMMI',2,to_timestamp('22-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,29.99,'Com?rcio Mineiro','Av. dos Lus?adas, 23','Sao Paulo','SP','05432-043','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11043,'SPECD',5,to_timestamp('22-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,8.8,'Sp?cialit?s du monde','25, rue Lauriston','Paris',null,'75016','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11044,'WOLZA',4,to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,8.72,'Wolski Zajazd','ul. Filtrowa 68','Warszawa',null,'01-012','Poland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11045,'BOTTM',6,to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,70.58,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11046,'WANDK',8,to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,71.64,'Die Wandernde Kuh','Adenauerallee 900','Stuttgart',null,'70563','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11047,'EASTC',7,to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,46.62,'Eastern Connection','35 King George','London',null,'WX3 6FW','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11048,'BOTTM',7,to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,24.12,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11049,'GOURL',3,to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,8.34,'Gourmet Lanchonetes','Av. Brasil, 442','Campinas','SP','04876-786','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11050,'FOLKO',8,to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,59.41,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11051,'LAMAI',7,to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,3,2.79,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11052,'HANAR',3,to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,67.26,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11053,'PICCO',2,to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,53.05,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11054,'CACTU',8,to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,1,0.33,'Cactus Comidas para llevar','Cerrito 333','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11055,'HILAA',7,to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,120.92,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11056,'EASTC',8,to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,278.96,'Eastern Connection','35 King George','London',null,'WX3 6FW','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11057,'NORTS',3,to_timestamp('29-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.13,'North/South','South House 300 Queensbridge','London',null,'SW7 1RZ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11058,'BLAUS',9,to_timestamp('29-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,3,31.14,'Blauer See Delikatessen','Forsterstr. 57','Mannheim',null,'68306','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11059,'RICAR',2,to_timestamp('29-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,85.8,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11060,'FRANS',2,to_timestamp('30-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,10.98,'Franchi S.p.A.','Via Monte Bianco 34','Torino',null,'10100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11061,'GREAL',4,to_timestamp('30-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,3,14.01,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11062,'REGGC',4,to_timestamp('30-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,29.93,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11063,'HUNGO',3,to_timestamp('30-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,81.73,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11064,'SAVEA',1,to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,30.09,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11065,'LILAS',8,to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,1,12.91,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11066,'WHITC',7,to_timestamp('01-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,44.72,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11067,'DRACD',1,to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,7.98,'Drachenblut Delikatessen','Walserweg 21','Aachen',null,'52066','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11068,'QUEEN',8,to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,81.75,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11069,'TORTU',1,to_timestamp('04-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,15.67,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11070,'LEHMS',2,to_timestamp('05-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,1,136,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10947,'BSBEV',3,to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.26,'B''s Beverages','Fauntleroy Circus','London',null,'EC2 5NT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10948,'GODOS',3,to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,23.39,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10949,'BOTTM',2,to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,74.44,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10950,'MAGAA',1,to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,2.5,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10951,'RICSU',9,to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,30.85,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10952,'ALFKI',1,to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,40.42,'Alfred''s Futterkiste','Obere Str. 57','Berlin',null,'12209','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10953,'AROUT',9,to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,23.72,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10954,'LINOD',5,to_timestamp('17-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,27.91,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10955,'FOLKO',8,to_timestamp('17-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.26,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10956,'BLAUS',6,to_timestamp('17-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,44.65,'Blauer See Delikatessen','Forsterstr. 57','Mannheim',null,'68306','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10957,'HILAA',8,to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,105.36,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10958,'OCEAN',7,to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,49.56,'Oc?ano Atl?ntico Ltda.','Ing. Gustavo Moncada 8585 Piso 20-A','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10959,'GOURL',6,to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,4.98,'Gourmet Lanchonetes','Av. Brasil, 442','Campinas','SP','04876-786','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10960,'HILAA',3,to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,2.08,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10961,'QUEEN',8,to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,104.47,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10962,'QUICK',8,to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,275.79,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10963,'FURIB',9,to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,2.7,'Furia Bacalhau e Frutos do Mar','Jardim das rosas n. 32','Lisboa',null,'1675','Portugal');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10964,'SPECD',3,to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,87.38,'Sp?cialit?s du monde','25, rue Lauriston','Paris',null,'75016','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10965,'OLDWO',6,to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,144.38,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10966,'CHOPS',4,to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,27.19,'Chop-suey Chinese','Hauptstr. 31','Bern',null,'3012','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10967,'TOMSP',2,to_timestamp('23-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,62.22,'Toms Spezialit?ten','Luisenstr. 48','M?nster',null,'44087','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10968,'ERNSH',1,to_timestamp('23-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,74.6,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10969,'COMMI',1,to_timestamp('23-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,0.21,'Com?rcio Mineiro','Av. dos Lus?adas, 23','Sao Paulo','SP','05432-043','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10970,'BOLID',9,to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,16.16,'B?lido Comidas preparadas','C/ Araquil, 67','Madrid',null,'28023','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10971,'FRANR',2,to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,121.82,'France restauration','54, rue Royale','Nantes',null,'44000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10972,'LACOR',4,to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,0.02,'La corne d''abondance','67, avenue de l''Europe','Versailles',null,'78000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10973,'LACOR',6,to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,15.17,'La corne d''abondance','67, avenue de l''Europe','Versailles',null,'78000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10974,'SPLIR',3,to_timestamp('25-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,12.96,'Split Rail Beer & Ale','P.O. Box 555','Lander','WY','82520','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10975,'BOTTM',1,to_timestamp('25-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,32.27,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10976,'HILAA',1,to_timestamp('25-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,37.97,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10977,'FOLKO',8,to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,208.5,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10978,'MAISD',9,to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,32.82,'Maison Dewey','Rue Joseph-Bens 532','Bruxelles',null,'B-1180','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10979,'ERNSH',8,to_timestamp('26-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,353.07,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10980,'FOLKO',4,to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,1.26,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10981,'HANAR',1,to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,193.37,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10982,'BOTTM',2,to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,14.01,'Bottom-Dollar Markets','23 Tsawassen Blvd.','Tsawassen','BC','T2F 8M4','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10983,'SAVEA',2,to_timestamp('27-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,657.54,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10984,'SAVEA',1,to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,211.22,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10985,'HUNGO',2,to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,91.51,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10986,'OCEAN',8,to_timestamp('30-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-04-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,217.86,'Oc?ano Atl?ntico Ltda.','Ing. Gustavo Moncada 8585 Piso 20-A','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10784,'MAGAA',4,to_timestamp('18-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,70.09,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10785,'GROSR',1,to_timestamp('18-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.51,'GROSELLA-Restaurante','5? Ave. Los Palos Grandes','Caracas','DF','1081','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10786,'QUEEN',8,to_timestamp('19-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,110.87,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10787,'LAMAI',2,to_timestamp('19-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,249.93,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10788,'QUICK',1,to_timestamp('22-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,42.7,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10789,'FOLIG',1,to_timestamp('22-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,100.6,'Folies gourmandes','184, chauss?e de Tournai','Lille',null,'59000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10790,'GOURL',6,to_timestamp('22-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,28.23,'Gourmet Lanchonetes','Av. Brasil, 442','Campinas','SP','04876-786','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10791,'FRANK',6,to_timestamp('23-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('01-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,16.85,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10792,'WOLZA',1,to_timestamp('23-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,23.79,'Wolski Zajazd','ul. Filtrowa 68','Warszawa',null,'01-012','Poland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10793,'AROUT',3,to_timestamp('24-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.52,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10794,'QUEDE',6,to_timestamp('24-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,21.49,'Que Del?cia','Rua da Panificadora, 12','Rio de Janeiro','RJ','02389-673','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10795,'ERNSH',8,to_timestamp('24-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,126.66,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10796,'HILAA',3,to_timestamp('25-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,26.52,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10797,'DRACD',7,to_timestamp('25-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,33.35,'Drachenblut Delikatessen','Walserweg 21','Aachen',null,'52066','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10798,'ISLAT',2,to_timestamp('26-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,2.33,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10799,'KOENE',9,to_timestamp('26-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,30.76,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10800,'SEVES',1,to_timestamp('26-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,137.44,'Seven Seas Imports','90 Wadhurst Rd.','London',null,'OX15 4NB','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10801,'BOLID',4,to_timestamp('29-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('31-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,97.09,'B?lido Comidas preparadas','C/ Araquil, 67','Madrid',null,'28023','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10802,'SIMOB',4,to_timestamp('29-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,257.26,'Simons bistro','Vinb?ltet 34','Kobenhavn',null,'1734','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10803,'WELLI',4,to_timestamp('30-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,55.23,'Wellington Importadora','Rua do Mercado, 12','Resende','SP','08737-363','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10804,'SEVES',6,to_timestamp('30-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,27.33,'Seven Seas Imports','90 Wadhurst Rd.','London',null,'OX15 4NB','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10805,'THEBI',2,to_timestamp('30-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,237.34,'The Big Cheese','89 Jefferson Way Suite 2','Portland','OR','97201','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10806,'VICTE',3,to_timestamp('31-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,22.11,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10807,'FRANS',4,to_timestamp('31-12-97 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('28-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,1.36,'Franchi S.p.A.','Via Monte Bianco 34','Torino',null,'10100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10808,'OLDWO',2,to_timestamp('01-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,45.53,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10809,'WELLI',7,to_timestamp('01-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,4.87,'Wellington Importadora','Rua do Mercado, 12','Resende','SP','08737-363','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10810,'LAUGB',2,to_timestamp('01-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('07-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,4.33,'Laughing Bacchus Wine Cellars','2319 Elm St.','Vancouver','BC','V3F 2K1','Canada');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10811,'LINOD',8,to_timestamp('02-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('08-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,31.22,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10812,'REGGC',5,to_timestamp('02-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,59.78,'Reggiani Caseifici','Strada Provinciale 124','Reggio Emilia',null,'42100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10813,'RICAR',1,to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,47.38,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10814,'VICTE',3,to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,130.94,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10815,'SAVEA',2,to_timestamp('05-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,14.62,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10816,'GREAL',4,to_timestamp('06-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,719.78,'Great Lakes Food Market','2732 Baker Blvd.','Eugene','OR','97403','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10817,'KOENE',3,to_timestamp('06-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,306.07,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10818,'MAGAA',7,to_timestamp('07-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,65.48,'Magazzini Alimentari Riuniti','Via Ludovico il Moro 22','Bergamo',null,'24100','Italy');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10819,'CACTU',2,to_timestamp('07-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,19.76,'Cactus Comidas para llevar','Cerrito 333','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10820,'RATTC',3,to_timestamp('07-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,37.52,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10821,'SPLIR',1,to_timestamp('08-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('15-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,36.68,'Split Rail Beer & Ale','P.O. Box 555','Lander','WY','82520','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10822,'TRAIH',6,to_timestamp('08-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,7,'Trail''s Head Gourmet Provisioners','722 DaVinci Blvd.','Kirkland','WA','98034','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10823,'LILAS',5,to_timestamp('09-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,163.97,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10824,'FOLKO',8,to_timestamp('09-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,1.23,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10825,'DRACD',1,to_timestamp('09-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('14-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,79.25,'Drachenblut Delikatessen','Walserweg 21','Aachen',null,'52066','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10826,'BLONP',6,to_timestamp('12-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,7.09,'Blondel p?re et fils','24, place Kl?ber','Strasbourg',null,'67000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10827,'BONAP',1,to_timestamp('12-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,63.54,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10828,'RANCH',9,to_timestamp('13-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,90.85,'Rancho grande','Av. del Libertador 900','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10829,'ISLAT',9,to_timestamp('13-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,154.72,'Island Trading','Garden House Crowther Way','Cowes','Isle of Wight','PO31 7PJ','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10830,'TRADH',4,to_timestamp('13-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,81.83,'Tradi?ao Hipermercados','Av. In?s de Castro, 414','Sao Paulo','SP','05634-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10831,'SANTG',3,to_timestamp('14-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,72.19,'Sant? Gourmet','Erling Skakkes gate 78','Stavern',null,'4110','Norway');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10832,'LAMAI',2,to_timestamp('14-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,43.26,'La maison d''Asie','1 rue Alsace-Lorraine','Toulouse',null,'31000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10833,'OTTIK',6,to_timestamp('15-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,71.49,'Ottilies K?seladen','Mehrheimerstr. 369','K?ln',null,'50739','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10834,'TRADH',1,to_timestamp('15-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,29.78,'Tradi?ao Hipermercados','Av. In?s de Castro, 414','Sao Paulo','SP','05634-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10835,'ALFKI',1,to_timestamp('15-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,69.53,'Alfred''s Futterkiste','Obere Str. 57','Berlin',null,'12209','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10836,'ERNSH',7,to_timestamp('16-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,411.88,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10837,'BERGS',9,to_timestamp('16-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,13.32,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10838,'LINOD',3,to_timestamp('19-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,59.28,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10839,'TRADH',3,to_timestamp('19-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('22-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,35.43,'Tradi?ao Hipermercados','Av. In?s de Castro, 414','Sao Paulo','SP','05634-030','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10840,'LINOD',4,to_timestamp('19-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,2.71,'LINO-Delicateses','Ave. 5 de Mayo Porlamar','I. de Margarita','Nueva Esparta','4980','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10841,'SUPRD',5,to_timestamp('20-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,424.3,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10842,'TORTU',1,to_timestamp('20-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,54.42,'Tortuga Restaurante','Avda. Azteca 123','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10843,'VICTE',4,to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,9.26,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10844,'PICCO',8,to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,25.22,'Piccolo und mehr','Geislweg 14','Salzburg',null,'5020','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10845,'QUICK',8,to_timestamp('21-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,212.98,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10846,'SUPRD',2,to_timestamp('22-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,56.46,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10847,'SAVEA',4,to_timestamp('22-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,487.57,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10848,'CONSH',7,to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('29-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,38.24,'Consolidated Holdings','Berkeley Gardens 12  Brewery','London',null,'WX1 6LT','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10849,'KOENE',9,to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,0.56,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10850,'VICTE',1,to_timestamp('23-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,49.19,'Victuailles en stock','2, rue du Commerce','Lyon',null,'69004','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10851,'RICAR',5,to_timestamp('26-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,160.55,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10852,'RATTC',8,to_timestamp('26-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,174.05,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10853,'BLAUS',9,to_timestamp('27-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,53.83,'Blauer See Delikatessen','Forsterstr. 57','Mannheim',null,'68306','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10854,'ERNSH',3,to_timestamp('27-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,100.22,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10855,'OLDWO',3,to_timestamp('27-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,170.97,'Old World Delicatessen','2743 Bering St.','Anchorage','AK','99508','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10856,'ANTON',3,to_timestamp('28-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,58.43,'Antonio Moreno Taquer?a','Mataderos  2312','M?xico D.F.',null,'05023','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10857,'BERGS',8,to_timestamp('28-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,188.85,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10858,'LACOR',2,to_timestamp('29-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,52.51,'La corne d''abondance','67, avenue de l''Europe','Versailles',null,'78000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10859,'FRANK',1,to_timestamp('29-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,76.1,'Frankenversand','Berliner Platz 43','M?nchen',null,'80805','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10860,'FRANR',3,to_timestamp('29-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,19.26,'France restauration','54, rue Royale','Nantes',null,'44000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10861,'WHITC',4,to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,14.93,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10862,'LEHMS',8,to_timestamp('30-01-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,53.23,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10863,'HILAA',4,to_timestamp('02-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,30.26,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10864,'AROUT',4,to_timestamp('02-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,3.04,'Around the Horn','Brook Farm Stratford St. Mary','Colchester','Essex','CO7 6JX','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10865,'QUICK',2,to_timestamp('02-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,348.14,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10866,'BERGS',5,to_timestamp('03-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,109.11,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10867,'LONEP',6,to_timestamp('03-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,1.93,'Lonesome Pine Restaurant','89 Chiaroscuro Rd.','Portland','OR','97219','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10868,'QUEEN',7,to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,191.27,'Queen Cozinha','Alameda dos Can?rios, 891','Sao Paulo','SP','05487-020','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10869,'SEVES',5,to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,143.28,'Seven Seas Imports','90 Wadhurst Rd.','London',null,'OX15 4NB','UK');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10870,'WOLZA',5,to_timestamp('04-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,12.04,'Wolski Zajazd','ul. Filtrowa 68','Warszawa',null,'01-012','Poland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10871,'BONAP',9,to_timestamp('05-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,112.27,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10872,'GODOS',5,to_timestamp('05-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('05-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,175.32,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10873,'WILMK',4,to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,0.82,'Wilman Kala','Keskuskatu 45','Helsinki',null,'21240','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10874,'GODOS',5,to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,19.58,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10875,'BERGS',4,to_timestamp('06-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,32.37,'Berglunds snabbk?p','Berguvsv?gen  8','Lule?',null,'S-958 22','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10876,'BONAP',7,to_timestamp('09-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,60.42,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10877,'RICAR',1,to_timestamp('09-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('09-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,38.06,'Ricardo Adocicados','Av. Copacabana, 267','Rio de Janeiro','RJ','02389-890','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10878,'QUICK',4,to_timestamp('10-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,46.69,'QUICK-Stop','Taucherstra?e 10','Cunewalde',null,'01307','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10879,'WILMK',3,to_timestamp('10-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('10-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,8.5,'Wilman Kala','Keskuskatu 45','Helsinki',null,'21240','Finland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10880,'FOLKO',7,to_timestamp('10-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,88.01,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10881,'CACTU',4,to_timestamp('11-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,2.84,'Cactus Comidas para llevar','Cerrito 333','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10882,'SAVEA',4,to_timestamp('11-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('11-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,23.1,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10883,'LONEP',8,to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,0.53,'Lonesome Pine Restaurant','89 Chiaroscuro Rd.','Portland','OR','97219','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10884,'LETSS',4,to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,90.97,'Let''s Stop N Shop','87 Polk St. Suite 5','San Francisco','CA','94117','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10885,'SUPRD',6,to_timestamp('12-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('12-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,5.64,'Supr?mes d?lices','Boulevard Tirou, 255','Charleroi',null,'B-6000','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10886,'HANAR',1,to_timestamp('13-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,4.99,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10887,'GALED',8,to_timestamp('13-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('13-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.25,'Galer?a del gastron?mo','Rambla de Catalu?a, 23','Barcelona',null,'8022','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10888,'GODOS',1,to_timestamp('16-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,51.87,'Godos Cocina T?pica','C/ Romero, 33','Sevilla',null,'41101','Spain');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10889,'RATTC',9,to_timestamp('16-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,280.61,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10890,'DUMON',7,to_timestamp('16-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('16-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,32.76,'Du monde entier','67, rue des Cinquante Otages','Nantes',null,'44000','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10891,'LEHMS',7,to_timestamp('17-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,20.37,'Lehmanns Marktstand','Magazinweg 7','Frankfurt a.M.',null,'60528','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10892,'MAISD',4,to_timestamp('17-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('17-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,120.27,'Maison Dewey','Rue Joseph-Bens 532','Bruxelles',null,'B-1180','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10893,'KOENE',9,to_timestamp('18-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,77.78,'K?niglich Essen','Maubelstr. 90','Brandenburg',null,'14776','Germany');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10894,'SAVEA',1,to_timestamp('18-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,116.13,'Save-a-lot Markets','187 Suffolk Ln.','Boise','ID','83720','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10895,'ERNSH',3,to_timestamp('18-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('18-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,162.75,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10896,'MAISD',7,to_timestamp('19-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,32.45,'Maison Dewey','Rue Joseph-Bens 532','Bruxelles',null,'B-1180','Belgium');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10897,'HUNGO',3,to_timestamp('19-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('19-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('25-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,603.54,'Hungry Owl All-Night Grocers','8 Johnstown Road','Cork','Co. Cork',null,'Ireland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10898,'OCEAN',4,to_timestamp('20-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.27,'Oc?ano Atl?ntico Ltda.','Ing. Gustavo Moncada 8585 Piso 20-A','Buenos Aires',null,'1010','Argentina');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10899,'LILAS',5,to_timestamp('20-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,1.21,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10900,'WELLI',1,to_timestamp('20-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('20-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,1.66,'Wellington Importadora','Rua do Mercado, 12','Resende','SP','08737-363','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10901,'HILAA',4,to_timestamp('23-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('26-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,62.09,'HILARION-Abastos','Carrera 22 con Ave. Carlos Soublette #8-35','San Crist?bal','T?chira','5022','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10902,'FOLKO',1,to_timestamp('23-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('23-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),1,44.15,'Folk och f? HB','?kergatan 24','Br?cke',null,'S-844 67','Sweden');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10903,'HANAR',3,to_timestamp('24-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('04-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,36.71,'Hanari Carnes','Rua do Pa?o, 67','Rio de Janeiro','RJ','05454-876','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10904,'WHITC',3,to_timestamp('24-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('27-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),3,162.95,'White Clover Markets','1029 - 12th Ave. S.','Seattle','WA','98124','USA');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (10905,'WELLI',9,to_timestamp('24-02-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('24-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('06-03-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),2,13.72,'Wellington Importadora','Rua do Mercado, 12','Resende','SP','08737-363','Brazil');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11071,'LILAS',1,to_timestamp('05-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,1,0.93,'LILA-Supermercado','Carrera 52 con Ave. Bol?var #65-98 Llano Largo','Barquisimeto','Lara','3508','Venezuela');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11072,'ERNSH',4,to_timestamp('05-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,258.64,'Ernst Handel','Kirchgasse 6','Graz',null,'8010','Austria');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11073,'PERIC',2,to_timestamp('05-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('02-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,24.95,'Pericles Comidas cl?sicas','Calle Dr. Jorge Cash 321','M?xico D.F.',null,'05033','Mexico');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11074,'SIMOB',7,to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,18.44,'Simons bistro','Vinb?ltet 34','Kobenhavn',null,'1734','Denmark');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11075,'RICSU',8,to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,6.19,'Richter Supermarkt','Starenweg 5','Gen?ve',null,'1204','Switzerland');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11076,'BONAP',4,to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,38.28,'Bon app''','12, rue des Bouchers','Marseille',null,'13008','France');
Insert into ORDERS (ORDERID,CUSTOMERID,EMPLOYEEID,ORDERDATE,REQUIREDDATE,SHIPPEDDATE,SHIPVIA,FREIGHT,SHIPNAME,SHIPADDRESS,SHIPCITY,SHIPREGION,SHIPPOSTALCODE,SHIPCOUNTRY) values (11077,'RATTC',1,to_timestamp('06-05-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),to_timestamp('03-06-98 12.00.00.000000000 AM','DD-MM-RR HH.MI.SS.FF AM'),null,2,8.53,'Rattlesnake Canyon Grocery','2817 Milton Dr.','Albuquerque','NM','87110','USA');

---------------------------------------------------
--   END DATA FOR TABLE ORDERS
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE ORDER_DETAILS
--   FILTER = none used
---------------------------------------------------
REM INSERTING into ORDER_DETAILS
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10495,41,7.7,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10495,77,10.4,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10496,31,10,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10497,56,30.4,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10497,72,27.8,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10497,77,10.4,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10498,24,4.5,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10498,40,18.4,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10498,42,14,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10499,28,45.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10499,49,20,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10500,15,15.5,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10500,28,45.6,8,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10501,54,7.45,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10502,45,9.5,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10502,53,32.8,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10502,67,14,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10503,14,23.25,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10503,65,21.05,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10504,2,19,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10504,21,10,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10504,53,32.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10504,61,28.5,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10505,62,49.3,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10506,25,14,18,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10506,70,15,14,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10507,43,46,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10507,48,12.75,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10508,13,6,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10508,39,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10509,28,45.6,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10510,29,123.79,36,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10510,75,7.75,36,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10511,4,22,50,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10511,7,30,50,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10511,8,40,10,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10512,24,4.5,10,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10512,46,12,9,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10512,47,9.5,6,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10512,60,34,12,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10513,21,10,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10513,32,32,50,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10513,61,28.5,15,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10514,20,81,39,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10514,28,45.6,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10514,56,38,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10514,65,21.05,39,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10514,75,7.75,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10515,9,97,16,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10515,16,17.45,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10515,27,43.9,120,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10515,33,2.5,16,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10515,60,34,84,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10516,18,62.5,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10516,41,9.65,80,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10516,42,14,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10517,52,7,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10517,59,55,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10517,70,15,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10518,24,4.5,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10518,38,263.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10518,44,19.45,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10519,10,31,16,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10519,56,38,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10519,60,34,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10520,24,4.5,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10520,53,32.8,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10521,35,18,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10521,41,9.65,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10521,68,12.5,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10522,1,18,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10522,8,40,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10522,30,25.89,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10522,40,18.4,25,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10523,17,39,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10523,20,81,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10523,37,26,18,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10523,41,9.65,6,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10524,10,31,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10524,30,25.89,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10524,43,46,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10524,54,7.45,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10525,36,19,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10525,40,18.4,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10526,1,18,8,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10526,13,6,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10526,56,38,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10527,4,22,50,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10527,36,19,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10528,11,21,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10528,33,2.5,8,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10528,72,34.8,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10529,55,24,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10529,68,12.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10529,69,36,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10530,17,39,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10530,43,46,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10530,61,28.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10530,76,18,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10531,59,55,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10532,30,25.89,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10532,66,17,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10533,4,22,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10533,72,34.8,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10533,73,15,24,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10534,30,25.89,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10534,40,18.4,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10534,54,7.45,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10535,11,21,50,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10535,40,18.4,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10535,57,19.5,5,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10535,59,55,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10536,12,38,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10536,31,12.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10536,33,2.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10536,60,34,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10537,31,12.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10537,51,53,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10537,58,13.25,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10537,72,34.8,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10537,73,15,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10538,70,15,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10538,72,34.8,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10539,13,6,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10539,21,10,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10539,33,2.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10539,49,20,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10540,3,10,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10540,26,31.23,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10540,38,263.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10540,68,12.5,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10541,24,4.5,35,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10541,38,263.5,4,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10541,65,21.05,36,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10541,71,21.5,9,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10542,11,21,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10542,54,7.45,24,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10543,12,38,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10543,23,9,70,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10544,28,45.6,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10544,67,14,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10545,11,21,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10546,7,30,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10546,35,18,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10546,62,49.3,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10547,32,32,24,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10547,36,19,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10548,34,14,10,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10548,41,9.65,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10549,31,12.5,55,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10549,45,9.5,100,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10549,51,53,48,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10550,17,39,8,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10550,19,9.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10550,21,10,6,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10550,61,28.5,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10551,16,17.45,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10551,35,18,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10551,44,19.45,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10552,69,36,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10552,75,7.75,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10553,11,21,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10553,16,17.45,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10553,22,21,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10553,31,12.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10553,35,18,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10554,16,17.45,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10554,23,9,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10554,62,49.3,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10554,77,13,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10555,14,23.25,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10555,19,9.2,35,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10555,24,4.5,18,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10555,51,53,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10555,56,38,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10556,72,34.8,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10557,64,33.25,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10557,75,7.75,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10558,47,9.5,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10558,51,53,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10558,52,7,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10558,53,32.8,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10558,73,15,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10559,41,9.65,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10559,55,24,18,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10560,30,25.89,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10560,62,49.3,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10561,44,19.45,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10561,51,53,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10562,33,2.5,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10562,62,49.3,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10563,36,19,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10563,52,7,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10564,17,39,16,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10564,31,12.5,6,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10564,55,24,25,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10565,24,4.5,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10565,64,33.25,18,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10566,11,21,35,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10566,18,62.5,18,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10566,76,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10567,31,12.5,60,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10567,51,53,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10567,59,55,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10568,10,31,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10569,31,12.5,35,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10569,76,18,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10570,11,21,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10570,56,38,60,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10571,14,23.25,11,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10571,42,14,28,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10572,16,17.45,12,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10572,32,32,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10572,40,18.4,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10572,75,7.75,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10573,17,39,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10573,34,14,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10573,53,32.8,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10574,33,2.5,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10574,40,18.4,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10574,62,49.3,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10574,64,33.25,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10575,59,55,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10575,63,43.9,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10575,72,34.8,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10575,76,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10576,1,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10576,31,12.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10576,44,19.45,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10577,39,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10577,75,7.75,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10577,77,13,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10578,35,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10578,57,19.5,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10579,15,15.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10579,75,7.75,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10580,14,23.25,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10580,41,9.65,9,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10580,65,21.05,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10581,75,7.75,50,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10582,57,19.5,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10582,76,18,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10583,29,123.79,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10583,60,34,24,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10583,69,36,10,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10584,31,12.5,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10585,47,9.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10586,52,7,4,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10587,26,31.23,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10587,35,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10587,77,13,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10588,18,62.5,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10588,42,14,100,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10589,35,18,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10590,1,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10590,77,13,60,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10591,3,10,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10591,7,30,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10591,54,7.45,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10592,15,15.5,25,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10592,26,31.23,5,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10593,20,81,21,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10593,69,36,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10593,76,18,4,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10594,52,7,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10594,58,13.25,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10595,35,18,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10595,61,28.5,120,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10595,69,36,65,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10596,56,38,5,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10596,63,43.9,24,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10596,75,7.75,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10597,24,4.5,35,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10597,57,19.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10597,65,21.05,12,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10598,27,43.9,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10598,71,21.5,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10599,62,49.3,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10600,54,7.45,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10600,73,15,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10601,13,6,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10601,59,55,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10602,77,13,5,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10603,22,21,48,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10603,49,20,25,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10604,48,12.75,6,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10604,76,18,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10605,16,17.45,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10605,59,55,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10605,60,34,70,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10605,71,21.5,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10606,4,22,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10606,55,24,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10606,62,49.3,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10607,7,30,45,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10607,17,39,100,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10607,33,2.5,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10607,40,18.4,42,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10607,72,34.8,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10608,56,38,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10609,1,18,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10609,10,31,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10609,21,10,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10610,36,19,21,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10611,1,18,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10611,2,19,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10611,60,34,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10612,10,31,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10612,36,19,55,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10612,49,20,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10612,60,34,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10612,76,18,80,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10613,13,6,8,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10613,75,7.75,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10614,11,21,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10614,21,10,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10614,39,18,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10615,55,24,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10616,38,263.5,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10616,56,38,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10616,70,15,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10616,71,21.5,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10617,59,55,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10618,6,25,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10618,56,38,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10618,68,12.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10619,21,10,42,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10619,22,21,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10620,24,4.5,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10620,52,7,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10621,19,9.2,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10621,23,9,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10621,70,15,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10621,71,21.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10622,2,19,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10622,68,12.5,18,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10623,14,23.25,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10623,19,9.2,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10623,21,10,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10623,24,4.5,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10623,35,18,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10624,28,45.6,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10624,29,123.79,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10624,44,19.45,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10625,14,23.25,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10625,42,14,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10625,60,34,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10626,53,32.8,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10626,60,34,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10626,71,21.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10627,62,49.3,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10627,73,15,35,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10628,1,18,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10629,29,123.79,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10629,64,33.25,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10630,55,24,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10630,76,18,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10631,75,7.75,8,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10632,2,19,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10632,33,2.5,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10633,12,38,36,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10633,13,6,13,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10633,26,31.23,35,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10633,62,49.3,80,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10634,7,30,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10634,18,62.5,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10634,51,53,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10634,75,7.75,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10635,4,22,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10635,5,21.35,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10635,22,21,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10636,4,22,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10636,58,13.25,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10637,11,21,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10637,50,16.25,25,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10637,56,38,60,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10638,45,9.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10638,65,21.05,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10638,72,34.8,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10639,18,62.5,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10640,69,36,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10640,70,15,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10641,2,19,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10641,40,18.4,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10642,21,10,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10642,61,28.5,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10643,28,45.6,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10643,39,18,21,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10643,46,12,2,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10644,18,62.5,4,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10644,43,46,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10644,46,12,21,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10645,18,62.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10645,36,19,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10646,1,18,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10646,10,31,18,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10646,71,21.5,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10646,77,13,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10647,19,9.2,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10647,39,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10648,22,21,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10648,24,4.5,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10649,28,45.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10649,72,34.8,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10650,30,25.89,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10650,53,32.8,25,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10650,54,7.45,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10651,19,9.2,12,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10651,22,21,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10652,30,25.89,2,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10652,42,14,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10653,16,17.45,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10653,60,34,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10654,4,22,12,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10654,39,18,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10654,54,7.45,6,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10655,41,9.65,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10656,14,23.25,3,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10656,44,19.45,28,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10656,47,9.5,6,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10657,15,15.5,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10657,41,9.65,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10657,46,12,45,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10657,47,9.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10657,56,38,45,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10657,60,34,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10658,21,10,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10658,40,18.4,70,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10658,60,34,55,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10658,77,13,70,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10659,31,12.5,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10659,40,18.4,24,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10659,70,15,40,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10660,20,81,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10661,39,18,3,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10661,58,13.25,49,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10662,68,12.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10663,40,18.4,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10663,42,14,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10663,51,53,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10664,10,31,24,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10664,56,38,12,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10664,65,21.05,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10665,51,53,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10665,59,55,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10665,76,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10666,29,123.79,36,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10666,65,21.05,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10667,69,36,45,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10667,71,21.5,14,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10668,31,12.5,8,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10668,55,24,4,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10668,64,33.25,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10669,36,19,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10670,23,9,32,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10670,46,12,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10670,67,14,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10670,73,15,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10670,75,7.75,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10671,16,17.45,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10671,62,49.3,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10671,65,21.05,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10672,38,263.5,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10672,71,21.5,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10673,16,17.45,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10673,42,14,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10673,43,46,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10674,23,9,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10675,14,23.25,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10675,53,32.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10675,58,13.25,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10676,10,31,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10676,19,9.2,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10676,44,19.45,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10677,26,31.23,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10677,33,2.5,8,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10678,12,38,100,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10678,33,2.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10678,41,9.65,120,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10678,54,7.45,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10679,59,55,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10680,16,17.45,50,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10680,31,12.5,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10680,42,14,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10681,19,9.2,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10681,21,10,12,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10681,64,33.25,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10682,33,2.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10682,66,17,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10682,75,7.75,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10683,52,7,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10684,40,18.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10684,47,9.5,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10684,60,34,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10685,10,31,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10685,41,9.65,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10685,47,9.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10686,17,39,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10686,26,31.23,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10687,9,97,50,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10687,29,123.79,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10687,36,19,6,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10688,10,31,18,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10688,28,45.6,60,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10688,34,14,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10689,1,18,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10690,56,38,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10690,77,13,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10691,1,18,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10691,29,123.79,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10691,43,46,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10691,44,19.45,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10691,62,49.3,48,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10692,63,43.9,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10693,9,97,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10693,54,7.45,60,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10693,69,36,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10693,73,15,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10694,7,30,90,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10694,59,55,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10694,70,15,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10695,8,40,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10695,12,38,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10695,24,4.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10696,17,39,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10696,46,12,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10697,19,9.2,7,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10697,35,18,9,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10697,58,13.25,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10697,70,15,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10698,11,21,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10698,17,39,8,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10698,29,123.79,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10698,65,21.05,65,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10698,70,15,8,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10699,47,9.5,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10700,1,18,5,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10700,34,14,12,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10700,68,12.5,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10700,71,21.5,60,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10701,59,55,42,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10701,71,21.5,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10701,76,18,35,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10702,3,10,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10702,76,18,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10703,2,19,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10703,59,55,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10703,73,15,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10704,4,22,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10704,24,4.5,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10704,48,12.75,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10705,31,12.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10705,32,32,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10706,16,17.45,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10706,43,46,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10706,59,55,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10707,55,24,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10707,57,19.5,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10707,70,15,28,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10708,5,21.35,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10708,36,19,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10709,8,40,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10709,51,53,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10709,60,34,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10710,19,9.2,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10710,47,9.5,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10711,19,9.2,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10711,41,9.65,42,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10711,53,32.8,120,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10712,53,32.8,3,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10712,56,38,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10713,10,31,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10713,26,31.23,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10713,45,9.5,110,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10713,46,12,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10714,2,19,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10714,17,39,27,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10714,47,9.5,50,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10714,56,38,18,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10714,58,13.25,12,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10715,10,31,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10715,71,21.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10716,21,10,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10716,51,53,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10716,61,28.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10717,21,10,32,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10717,54,7.45,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10717,69,36,25,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10718,12,38,36,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10718,16,17.45,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10718,36,19,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10718,62,49.3,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10719,18,62.5,12,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10719,30,25.89,3,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10719,54,7.45,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10720,35,18,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10720,71,21.5,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10721,44,19.45,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10722,2,19,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10722,31,12.5,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10722,68,12.5,45,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10722,75,7.75,42,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10723,26,31.23,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10724,10,31,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10724,61,28.5,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10725,41,9.65,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10725,52,7,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10725,55,24,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10726,4,22,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10726,11,21,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10727,17,39,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10727,56,38,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10727,59,55,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10728,30,25.89,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10728,40,18.4,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10728,55,24,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10728,60,34,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10729,1,18,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10729,21,10,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10729,50,16.25,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10730,16,17.45,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10730,31,12.5,3,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10730,65,21.05,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10731,21,10,40,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10731,51,53,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10732,76,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10733,14,23.25,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10733,28,45.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10733,52,7,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10734,6,25,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10734,30,25.89,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10734,76,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10735,61,28.5,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10735,77,13,2,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10736,65,21.05,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10736,75,7.75,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10737,13,6,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10737,41,9.65,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10738,16,17.45,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10739,36,19,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10739,52,7,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10740,28,45.6,5,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10740,35,18,35,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10740,45,9.5,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10740,56,38,14,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10741,2,19,15,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10742,3,10,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10742,60,34,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10742,72,34.8,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10743,46,12,28,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10744,40,18.4,50,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10745,18,62.5,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10745,44,19.45,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10745,59,55,45,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10745,72,34.8,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10746,13,6,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10746,42,14,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10746,62,49.3,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10746,69,36,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10747,31,12.5,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10747,41,9.65,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10747,63,43.9,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10747,69,36,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10748,23,9,44,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10748,40,18.4,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10748,56,38,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10749,56,38,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10749,59,55,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10749,76,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10750,14,23.25,5,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10750,45,9.5,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10750,59,55,25,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10751,26,31.23,12,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10751,30,25.89,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10751,50,16.25,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10751,73,15,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10752,1,18,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10752,69,36,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10753,45,9.5,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10753,74,10,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10754,40,18.4,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10755,47,9.5,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10755,56,38,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10755,57,19.5,14,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10755,69,36,25,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10756,18,62.5,21,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10756,36,19,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10756,68,12.5,6,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10756,69,36,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10757,34,14,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10757,59,55,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10757,62,49.3,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10757,64,33.25,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10758,26,31.23,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10758,52,7,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10758,70,15,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10759,32,32,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10760,25,14,12,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10760,27,43.9,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10760,43,46,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10761,25,14,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10761,75,7.75,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10762,39,18,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10762,47,9.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10762,51,53,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10762,56,38,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10763,21,10,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10763,22,21,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10763,24,4.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10764,3,10,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10764,39,18,130,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10765,65,21.05,80,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10766,2,19,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10766,7,30,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10766,68,12.5,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10767,42,14,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10768,22,21,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10768,31,12.5,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10768,60,34,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10768,71,21.5,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10769,41,9.65,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10769,52,7,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10769,61,28.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10769,62,49.3,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10770,11,21,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10771,71,21.5,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10772,29,123.79,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10772,59,55,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10773,17,39,33,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10773,31,12.5,70,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10773,75,7.75,7,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10774,31,12.5,2,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10774,66,17,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10775,10,31,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10775,67,14,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10776,31,12.5,16,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10776,42,14,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10776,45,9.5,27,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10776,51,53,120,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10777,42,14,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10778,41,9.65,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10779,16,17.45,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10779,62,49.3,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10780,70,15,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10780,77,13,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10781,54,7.45,3,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10781,56,38,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10781,74,10,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10782,31,12.5,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10783,31,12.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10783,38,263.5,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10784,36,19,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10784,39,18,2,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10784,72,34.8,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10785,10,31,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10785,75,7.75,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10786,8,40,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10786,30,25.89,15,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10786,75,7.75,42,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10787,2,19,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10787,29,123.79,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10788,19,9.2,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10788,75,7.75,40,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10789,18,62.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10789,35,18,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10789,63,43.9,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10789,68,12.5,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10790,7,30,3,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10790,56,38,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10791,29,123.79,14,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10791,41,9.65,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10792,2,19,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10792,54,7.45,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10792,68,12.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10793,41,9.65,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10793,52,7,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10794,14,23.25,15,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10794,54,7.45,6,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10795,16,17.45,65,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10795,17,39,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10796,26,31.23,21,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10796,44,19.45,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10796,64,33.25,35,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10796,69,36,24,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10797,11,21,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10798,62,49.3,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10798,72,34.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10799,13,6,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10799,24,4.5,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10799,59,55,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10800,11,21,50,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10800,51,53,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10800,54,7.45,7,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10801,17,39,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10801,29,123.79,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10802,30,25.89,25,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10802,51,53,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10802,55,24,60,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10802,62,49.3,5,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10803,19,9.2,24,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10803,25,14,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10803,59,55,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10804,10,31,36,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10804,28,45.6,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10804,49,20,4,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10805,34,14,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10805,38,263.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10806,2,19,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10806,65,21.05,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10806,74,10,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10807,40,18.4,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10808,56,38,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10808,76,18,50,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10809,52,7,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10810,13,6,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10810,25,14,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10810,70,15,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10811,19,9.2,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10811,23,9,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10811,40,18.4,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10812,31,12.5,16,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10812,72,34.8,40,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10812,77,13,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10813,2,19,12,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10813,46,12,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10814,41,9.65,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10814,43,46,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10814,48,12.75,8,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10814,61,28.5,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10815,33,2.5,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10816,38,263.5,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10816,62,49.3,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10817,26,31.23,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10817,38,263.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10817,40,18.4,60,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10817,62,49.3,25,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10818,32,32,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10818,41,9.65,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10819,43,46,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10819,75,7.75,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10820,56,38,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10821,35,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10821,51,53,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10822,62,49.3,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10822,70,15,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10823,11,21,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10823,57,19.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10823,59,55,40,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10823,77,13,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10824,41,9.65,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10824,70,15,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10825,26,31.23,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10825,53,32.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10826,31,12.5,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10826,57,19.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10827,10,31,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10827,39,18,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10828,20,81,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10828,38,263.5,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10829,2,19,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10829,8,40,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10829,13,6,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10829,60,34,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10830,6,25,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10830,39,18,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10830,60,34,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10830,68,12.5,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10831,19,9.2,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10831,35,18,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10831,38,263.5,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10831,43,46,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10832,13,6,3,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10832,25,14,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10832,44,19.45,16,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10832,64,33.25,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10833,7,30,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10833,31,12.5,9,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10833,53,32.8,9,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10834,29,123.79,8,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10834,30,25.89,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10835,59,55,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10835,77,13,2,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10836,22,21,52,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10836,35,18,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10836,57,19.5,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10836,60,34,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10836,64,33.25,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10837,13,6,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10837,40,18.4,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10837,47,9.5,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10837,76,18,21,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10838,1,18,4,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10838,18,62.5,25,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10838,36,19,50,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10839,58,13.25,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10839,72,34.8,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10840,25,14,6,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10840,39,18,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10841,10,31,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10841,56,38,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10841,59,55,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10841,77,13,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10842,11,21,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10842,43,46,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10842,68,12.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10842,70,15,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10843,51,53,4,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10844,22,21,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10845,23,9,70,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10845,35,18,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10845,42,14,42,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10845,58,13.25,60,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10845,64,33.25,48,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10846,4,22,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10846,70,15,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10846,74,10,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10847,1,18,80,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10847,19,9.2,12,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10847,37,26,60,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10847,45,9.5,36,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10847,60,34,45,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10847,71,21.5,55,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10848,5,21.35,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10848,9,97,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10849,3,10,49,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10849,26,31.23,18,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10850,25,14,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10850,33,2.5,4,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10850,70,15,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10851,2,19,5,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10851,25,14,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10851,57,19.5,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10851,59,55,42,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10852,2,19,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10852,17,39,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10852,62,49.3,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10853,18,62.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10854,10,31,100,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10854,13,6,65,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10855,16,17.45,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10855,31,12.5,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10855,56,38,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10855,65,21.05,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10856,2,19,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10856,42,14,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10857,3,10,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10857,26,31.23,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10857,29,123.79,10,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10858,7,30,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10858,27,43.9,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10858,70,15,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10859,24,4.5,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10859,54,7.45,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10859,64,33.25,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10860,51,53,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10860,76,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10861,17,39,42,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10861,18,62.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10861,21,10,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10861,33,2.5,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10861,62,49.3,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10862,11,21,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10862,52,7,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10863,1,18,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10863,58,13.25,12,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10864,35,18,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10864,67,14,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10865,38,263.5,60,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10865,39,18,80,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10866,2,19,21,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10866,24,4.5,6,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10866,30,25.89,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10867,53,32.8,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10868,26,31.23,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10868,35,18,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10868,49,20,42,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10869,1,18,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10869,11,21,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10869,23,9,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10869,68,12.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10870,35,18,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10870,51,53,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10871,6,25,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10871,16,17.45,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10871,17,39,16,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10872,55,24,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10872,62,49.3,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10872,64,33.25,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10872,65,21.05,21,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10873,21,10,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10873,28,45.6,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10874,10,31,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10875,19,9.2,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10248,11,14,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10248,42,9.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10248,72,34.8,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10249,14,18.6,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10249,51,42.4,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10250,41,7.7,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10250,51,42.4,35,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10250,65,16.8,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10251,22,16.8,6,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10251,57,15.6,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10251,65,16.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10252,20,64.8,40,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10252,33,2,25,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10252,60,27.2,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10253,31,10,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10253,39,14.4,42,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10253,49,16,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10254,24,3.6,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10254,55,19.2,21,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10254,74,8,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10255,2,15.2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10255,16,13.9,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10255,36,15.2,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10255,59,44,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10256,53,26.2,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10256,77,10.4,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10257,27,35.1,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10257,39,14.4,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10257,77,10.4,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10258,2,15.2,50,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10258,5,17,65,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10258,32,25.6,6,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10259,21,8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10259,37,20.8,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10260,41,7.7,16,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10260,57,15.6,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10260,62,39.4,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10260,70,12,21,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10261,21,8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10261,35,14.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10262,5,17,12,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10262,7,24,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10262,56,30.4,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10263,16,13.9,60,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10263,24,3.6,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10263,30,20.7,60,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10263,74,8,36,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10264,2,15.2,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10264,41,7.7,25,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10265,17,31.2,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10265,70,12,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10266,12,30.4,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10267,40,14.7,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10267,59,44,70,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10267,76,14.4,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10268,29,99,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10268,72,27.8,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10269,33,2,60,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10269,72,27.8,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10270,36,15.2,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10270,43,36.8,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10271,33,2,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10272,20,64.8,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10272,31,10,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10272,72,27.8,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10273,10,24.8,24,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10273,31,10,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10273,33,2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10273,40,14.7,60,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10273,76,14.4,33,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10274,71,17.2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10274,72,27.8,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10275,24,3.6,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10275,59,44,6,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10276,10,24.8,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10276,13,4.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10277,28,36.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10277,62,39.4,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10278,44,15.5,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10278,59,44,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10278,63,35.1,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10278,73,12,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10279,17,31.2,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10280,24,3.6,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10280,55,19.2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10280,75,6.2,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10281,19,7.3,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10281,24,3.6,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10281,35,14.4,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10282,30,20.7,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10282,57,15.6,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10283,15,12.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10283,19,7.3,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10283,60,27.2,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10283,72,27.8,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10284,27,35.1,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10284,44,15.5,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10284,60,27.2,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10284,67,11.2,5,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10285,1,14.4,45,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10285,40,14.7,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10285,53,26.2,36,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10286,35,14.4,100,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10286,62,39.4,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10287,16,13.9,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10287,34,11.2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10287,46,9.6,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10288,54,5.9,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10288,68,10,3,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10289,3,8,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10289,64,26.6,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10290,5,17,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10290,29,99,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10290,49,16,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10290,77,10.4,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10291,13,4.8,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10291,44,15.5,24,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10291,51,42.4,2,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10292,20,64.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10293,18,50,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10293,24,3.6,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10293,63,35.1,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10293,75,6.2,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10294,1,14.4,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10294,17,31.2,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10294,43,36.8,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10294,60,27.2,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10294,75,6.2,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10295,56,30.4,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10296,11,16.8,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10296,16,13.9,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10296,69,28.8,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10297,39,14.4,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10297,72,27.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10298,2,15.2,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10298,36,15.2,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10298,59,44,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10298,62,39.4,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10299,19,7.3,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10299,70,12,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10300,66,13.6,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10300,68,10,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10301,40,14.7,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10301,56,30.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10302,17,31.2,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10302,28,36.4,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10302,43,36.8,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10303,40,14.7,40,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10303,65,16.8,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10303,68,10,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10304,49,16,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10304,59,44,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10304,71,17.2,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10305,18,50,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10305,29,99,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10305,39,14.4,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10306,30,20.7,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10306,53,26.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10306,54,5.9,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10307,62,39.4,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10307,68,10,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10308,69,28.8,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10308,70,12,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10309,4,17.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10309,6,20,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10309,42,11.2,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10309,43,36.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10309,71,17.2,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10310,16,13.9,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10310,62,39.4,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10311,42,11.2,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10311,69,28.8,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10312,28,36.4,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10312,43,36.8,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10312,53,26.2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10312,75,6.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10313,36,15.2,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10314,32,25.6,40,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10314,58,10.6,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10314,62,39.4,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10315,34,11.2,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10315,70,12,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10316,41,7.7,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10316,62,39.4,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10317,1,14.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10318,41,7.7,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10318,76,14.4,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10319,17,31.2,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10319,28,36.4,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10319,76,14.4,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10320,71,17.2,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10321,35,14.4,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10322,52,5.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10323,15,12.4,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10323,25,11.2,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10323,39,14.4,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10324,16,13.9,21,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10324,35,14.4,70,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10324,46,9.6,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10324,59,44,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10324,63,35.1,80,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10325,6,20,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10325,13,4.8,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10325,14,18.6,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10325,31,10,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10325,72,27.8,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10326,4,17.6,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10326,57,15.6,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10326,75,6.2,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10327,2,15.2,25,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10327,11,16.8,50,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10327,30,20.7,35,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10327,58,10.6,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10328,59,44,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10328,65,16.8,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10328,68,10,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10329,19,7.3,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10329,30,20.7,8,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10329,38,210.8,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10329,56,30.4,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10330,26,24.9,50,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10330,72,27.8,25,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10331,54,5.9,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10332,18,50,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10332,42,11.2,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10332,47,7.6,16,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10333,14,18.6,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10333,21,8,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10333,71,17.2,40,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10334,52,5.6,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10334,68,10,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10335,2,15.2,7,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10335,31,10,25,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10335,32,25.6,6,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10335,51,42.4,48,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10336,4,17.6,18,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10337,23,7.2,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10337,26,24.9,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10337,36,15.2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10337,37,20.8,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10337,72,27.8,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10338,17,31.2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10338,30,20.7,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10339,4,17.6,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10339,17,31.2,70,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10339,62,39.4,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10340,18,50,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10340,41,7.7,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10340,43,36.8,40,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10341,33,2,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10341,59,44,9,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10342,2,15.2,24,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10342,31,10,56,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10342,36,15.2,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10342,55,19.2,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10343,64,26.6,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10343,68,10,4,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10343,76,14.4,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10344,4,17.6,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10344,8,32,70,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10345,8,32,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10345,19,7.3,80,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10345,42,11.2,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10346,17,31.2,36,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10346,56,30.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10347,25,11.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10347,39,14.4,50,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10347,40,14.7,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10347,75,6.2,6,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10348,1,14.4,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10348,23,7.2,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10349,54,5.9,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10350,50,13,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10350,69,28.8,18,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10351,38,210.8,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10351,41,7.7,13,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10351,44,15.5,77,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10351,65,16.8,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10352,24,3.6,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10352,54,5.9,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10353,11,16.8,12,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10353,38,210.8,50,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10354,1,14.4,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10354,29,99,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10355,24,3.6,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10355,57,15.6,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10356,31,10,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10356,55,19.2,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10356,69,28.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10357,10,24.8,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10357,26,24.9,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10357,60,27.2,8,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10358,24,3.6,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10358,34,11.2,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10358,36,15.2,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10359,16,13.9,56,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10359,31,10,70,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10359,60,27.2,80,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10360,28,36.4,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10360,29,99,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10360,38,210.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10360,49,16,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10360,54,5.9,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10361,39,14.4,54,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10361,60,27.2,55,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10362,25,11.2,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10362,51,42.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10362,54,5.9,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10363,31,10,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10363,75,6.2,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10363,76,14.4,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10364,69,28.8,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10364,71,17.2,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10365,11,16.8,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10366,65,16.8,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10366,77,10.4,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10367,34,11.2,36,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10367,54,5.9,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10367,65,16.8,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10367,77,10.4,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10368,21,8,5,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10368,28,36.4,13,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10368,57,15.6,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10368,64,26.6,35,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10369,29,99,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10369,56,30.4,18,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10370,1,14.4,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10370,64,26.6,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10370,74,8,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10371,36,15.2,6,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10372,20,64.8,12,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10372,38,210.8,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10372,60,27.2,70,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10372,72,27.8,42,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10373,58,10.6,80,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10373,71,17.2,50,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10374,31,10,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10374,58,10.6,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10375,14,18.6,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10375,54,5.9,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10376,31,10,42,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10377,28,36.4,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10377,39,14.4,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10378,71,17.2,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10379,41,7.7,8,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10379,63,35.1,16,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10379,65,16.8,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10380,30,20.7,18,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10380,53,26.2,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10380,60,27.2,6,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10380,70,12,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10381,74,8,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10382,5,17,32,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10382,18,50,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10382,29,99,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10382,33,2,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10382,74,8,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10383,13,4.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10383,50,13,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10383,56,30.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10384,20,64.8,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10384,60,27.2,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10385,7,24,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10385,60,27.2,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10385,68,10,8,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10386,24,3.6,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10386,34,11.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10387,24,3.6,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10387,28,36.4,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10387,59,44,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10387,71,17.2,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10388,45,7.6,15,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10388,52,5.6,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10388,53,26.2,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10389,10,24.8,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10389,55,19.2,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10389,62,39.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10389,70,12,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10390,31,10,60,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10390,35,14.4,40,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10390,46,9.6,45,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10390,72,27.8,24,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10391,13,4.8,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10392,69,28.8,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10393,2,15.2,25,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10393,14,18.6,42,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10393,25,11.2,7,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10393,26,24.9,70,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10393,31,10,32,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10394,13,4.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10394,62,39.4,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10395,46,9.6,28,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10395,53,26.2,70,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10395,69,28.8,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10396,23,7.2,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10396,71,17.2,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10396,72,27.8,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10397,21,8,10,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10397,51,42.4,18,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10398,35,14.4,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10398,55,19.2,120,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10399,68,10,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10399,71,17.2,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10399,76,14.4,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10399,77,10.4,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10400,29,99,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10400,35,14.4,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10400,49,16,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10401,30,20.7,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10401,56,30.4,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10401,65,16.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10401,71,17.2,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10402,23,7.2,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10402,63,35.1,65,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10403,16,13.9,21,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10403,48,10.2,70,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10404,26,24.9,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10404,42,11.2,40,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10404,49,16,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10405,3,8,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10406,1,14.4,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10406,21,8,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10406,28,36.4,42,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10406,36,15.2,5,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10406,40,14.7,2,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10407,11,16.8,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10407,69,28.8,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10407,71,17.2,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10408,37,20.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10408,54,5.9,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10408,62,39.4,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10409,14,18.6,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10409,21,8,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10410,33,2,49,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10410,59,44,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10411,41,7.7,25,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10411,44,15.5,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10411,59,44,9,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10412,14,18.6,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10413,1,14.4,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10413,62,39.4,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10413,76,14.4,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10414,19,7.3,18,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10414,33,2,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10415,17,31.2,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10415,33,2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10416,19,7.3,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10416,53,26.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10416,57,15.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10417,38,210.8,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10417,46,9.6,2,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10417,68,10,36,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10417,77,10.4,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10418,2,15.2,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10418,47,7.6,55,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10418,61,22.8,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10418,74,8,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10419,60,27.2,60,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10419,69,28.8,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10420,9,77.6,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10420,13,4.8,2,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10420,70,12,8,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10420,73,12,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10421,19,7.3,4,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10421,26,24.9,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10421,53,26.2,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10421,77,10.4,10,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10422,26,24.9,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10423,31,10,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10423,59,44,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10424,35,14.4,60,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10424,38,210.8,49,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10424,68,10,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10425,55,19.2,10,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10425,76,14.4,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10426,56,30.4,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10426,64,26.6,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10427,14,18.6,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10428,46,9.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10429,50,13,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10429,63,35.1,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10430,17,31.2,45,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10430,21,8,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10430,56,30.4,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10430,59,44,70,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10431,17,31.2,50,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10431,40,14.7,50,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10431,47,7.6,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10432,26,24.9,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10432,54,5.9,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10433,56,30.4,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10434,11,16.8,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10434,76,14.4,18,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10435,2,15.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10435,22,16.8,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10435,72,27.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10436,46,9.6,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10436,56,30.4,40,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10436,64,26.6,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10436,75,6.2,24,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10437,53,26.2,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10438,19,7.3,15,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10438,34,11.2,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10438,57,15.6,15,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10439,12,30.4,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10439,16,13.9,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10439,64,26.6,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10439,74,8,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10440,2,15.2,45,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10440,16,13.9,49,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10440,29,99,24,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10440,61,22.8,90,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10441,27,35.1,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10442,11,16.8,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10442,54,5.9,80,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10442,66,13.6,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10443,11,16.8,6,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10443,28,36.4,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10444,17,31.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10444,26,24.9,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10444,35,14.4,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10444,41,7.7,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10445,39,14.4,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10445,54,5.9,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10446,19,7.3,12,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10446,24,3.6,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10446,31,10,3,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10446,52,5.6,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10447,19,7.3,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10447,65,16.8,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10447,71,17.2,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10448,26,24.9,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10448,40,14.7,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10449,10,24.8,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10449,52,5.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10449,62,39.4,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10450,10,24.8,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10450,54,5.9,6,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10451,55,19.2,120,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10451,64,26.6,35,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10451,65,16.8,28,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10451,77,10.4,55,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10452,28,36.4,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10452,44,15.5,100,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10453,48,10.2,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10453,70,12,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10454,16,13.9,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10454,33,2,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10454,46,9.6,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10455,39,14.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10455,53,26.2,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10455,61,22.8,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10455,71,17.2,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10456,21,8,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10456,49,16,21,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10457,59,44,36,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10458,26,24.9,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10458,28,36.4,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10458,43,36.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10458,56,30.4,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10458,71,17.2,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10459,7,24,16,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10459,46,9.6,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10459,72,27.8,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10460,68,10,21,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10460,75,6.2,4,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10461,21,8,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10461,30,20.7,28,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10461,55,19.2,60,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10462,13,4.8,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10462,23,7.2,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10463,19,7.3,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10463,42,11.2,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10464,4,17.6,16,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10464,43,36.8,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10464,56,30.4,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10464,60,27.2,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10465,24,3.6,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10465,29,99,18,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10465,40,14.7,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10465,45,7.6,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10465,50,13,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10466,11,16.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10466,46,9.6,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10467,24,3.6,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10467,25,11.2,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10468,30,20.7,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10468,43,36.8,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10469,2,15.2,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10469,16,13.9,35,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10469,44,15.5,2,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10470,18,50,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10470,23,7.2,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10470,64,26.6,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10471,7,24,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10471,56,30.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10472,24,3.6,80,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10472,51,42.4,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10473,33,2,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10473,71,17.2,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10474,14,18.6,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10474,28,36.4,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10474,40,14.7,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10474,75,6.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10475,31,10,35,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10475,66,13.6,60,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10475,76,14.4,42,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10476,55,19.2,2,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10476,70,12,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10477,1,14.4,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10477,21,8,21,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10477,39,14.4,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10478,10,24.8,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10479,38,210.8,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10479,53,26.2,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10479,59,44,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10479,64,26.6,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10480,47,7.6,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10480,59,44,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10481,49,16,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10481,60,27.2,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10482,40,14.7,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10483,34,11.2,35,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10483,77,10.4,30,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10484,21,8,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10484,40,14.7,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10484,51,42.4,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10485,2,15.2,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10485,3,8,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10485,55,19.2,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10485,70,12,60,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10486,11,16.8,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10486,51,42.4,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10486,74,8,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10487,19,7.3,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10487,26,24.9,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10487,54,5.9,24,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10488,59,44,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10488,73,12,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10489,11,16.8,15,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10489,16,13.9,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10490,59,44,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10490,68,10,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10490,75,6.2,36,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10491,44,15.5,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10491,77,10.4,7,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10492,25,11.2,60,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10492,42,11.2,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10493,65,16.8,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10493,66,13.6,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10493,69,28.8,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10494,56,30.4,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10495,23,7.2,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10875,47,9.5,21,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10875,49,20,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10876,46,12,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10876,64,33.25,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10877,16,17.45,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10877,18,62.5,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10878,20,81,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10879,40,18.4,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10879,65,21.05,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10879,76,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10880,23,9,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10880,61,28.5,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10880,70,15,50,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10881,73,15,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10882,42,14,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10882,49,20,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10882,54,7.45,32,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10883,24,4.5,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10884,21,10,40,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10884,56,38,21,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10884,65,21.05,12,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10885,2,19,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10885,24,4.5,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10885,70,15,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10885,77,13,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10886,10,31,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10886,31,12.5,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10886,77,13,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10887,25,14,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10888,2,19,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10888,68,12.5,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10889,11,21,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10889,38,263.5,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10890,17,39,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10890,34,14,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10890,41,9.65,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10891,30,25.89,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10892,59,55,40,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10893,8,40,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10893,24,4.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10893,29,123.79,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10893,30,25.89,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10893,36,19,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10894,13,6,28,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10894,69,36,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10894,75,7.75,120,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10895,24,4.5,110,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10895,39,18,45,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10895,40,18.4,91,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10895,60,34,100,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10896,45,9.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10896,56,38,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10897,29,123.79,80,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10897,30,25.89,36,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10898,13,6,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10899,39,18,8,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10900,70,15,3,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10901,41,9.65,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10901,71,21.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10902,55,24,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10902,62,49.3,6,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10903,13,6,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10903,65,21.05,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10903,68,12.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10904,58,13.25,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10904,62,49.3,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10905,1,18,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10906,61,28.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10907,75,7.75,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10908,7,30,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10908,52,7,14,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10909,7,30,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10909,16,17.45,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10909,41,9.65,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10910,19,9.2,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10910,49,20,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10910,61,28.5,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10911,1,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10911,17,39,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10911,67,14,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10912,11,21,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10912,29,123.79,60,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10913,4,22,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10913,33,2.5,40,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10913,58,13.25,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10914,71,21.5,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10915,17,39,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10915,33,2.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10915,54,7.45,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10916,16,17.45,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10916,32,32,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10916,57,19.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10917,30,25.89,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10917,60,34,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10918,1,18,60,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10918,60,34,25,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10919,16,17.45,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10919,25,14,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10919,40,18.4,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10920,50,16.25,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10921,35,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10921,63,43.9,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10922,17,39,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10922,24,4.5,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10923,42,14,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10923,43,46,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10923,67,14,24,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10924,10,31,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10924,28,45.6,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10924,75,7.75,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10925,36,19,25,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10925,52,7,12,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10926,11,21,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10926,13,6,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10926,19,9.2,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10926,72,34.8,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10927,20,81,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10927,52,7,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10927,76,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10928,47,9.5,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10928,76,18,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10929,21,10,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10929,75,7.75,49,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10929,77,13,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10930,21,10,36,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10930,27,43.9,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10930,55,24,25,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10930,58,13.25,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10931,13,6,42,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10931,57,19.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10932,16,17.45,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10932,62,49.3,14,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10932,72,34.8,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10932,75,7.75,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10933,53,32.8,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10933,61,28.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10934,6,25,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10935,1,18,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10935,18,62.5,4,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10935,23,9,8,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10936,36,19,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10937,28,45.6,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10937,34,14,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10938,13,6,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10938,43,46,24,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10938,60,34,49,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10938,71,21.5,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10939,2,19,10,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10939,67,14,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10940,7,30,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10940,13,6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10941,31,12.5,44,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10941,62,49.3,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10941,68,12.5,80,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10941,72,34.8,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10942,49,20,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10943,13,6,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10943,22,21,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10943,46,12,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10944,11,21,5,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10944,44,19.45,18,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10944,56,38,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10945,13,6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10945,31,12.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10946,10,31,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10946,24,4.5,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10946,77,13,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10947,59,55,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10948,50,16.25,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10948,51,53,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10948,55,24,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10949,6,25,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10949,10,31,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10949,17,39,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10949,62,49.3,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10950,4,22,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10951,33,2.5,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10951,41,9.65,6,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10951,75,7.75,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10952,6,25,16,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10952,28,45.6,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10953,20,81,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10953,31,12.5,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10954,16,17.45,28,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10954,31,12.5,25,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10954,45,9.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10954,60,34,24,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10955,75,7.75,12,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10956,21,10,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10956,47,9.5,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10956,51,53,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10957,30,25.89,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10957,35,18,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10957,64,33.25,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10958,5,21.35,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10958,7,30,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10958,72,34.8,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10959,75,7.75,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10960,24,4.5,10,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10960,41,9.65,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10961,52,7,6,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10961,76,18,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10962,7,30,45,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10962,13,6,77,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10962,53,32.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10962,69,36,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10962,76,18,44,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10963,60,34,2,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10964,18,62.5,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10964,38,263.5,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10964,69,36,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10965,51,53,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10966,37,26,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10966,56,38,12,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10966,62,49.3,12,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10967,19,9.2,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10967,49,20,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10968,12,38,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10968,24,4.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10968,64,33.25,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10969,46,12,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10970,52,7,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10971,29,123.79,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10972,17,39,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10972,33,2.5,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10973,26,31.23,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10973,41,9.65,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10973,75,7.75,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10974,63,43.9,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10975,8,40,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10975,75,7.75,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10976,28,45.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10977,39,18,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10977,47,9.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10977,51,53,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10977,63,43.9,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10978,8,40,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10978,21,10,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10978,40,18.4,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10978,44,19.45,6,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10979,7,30,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10979,12,38,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10979,24,4.5,80,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10979,27,43.9,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10979,31,12.5,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10979,63,43.9,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10980,75,7.75,40,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10981,38,263.5,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10982,7,30,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10982,43,46,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10983,13,6,84,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10983,57,19.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10984,16,17.45,55,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10984,24,4.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10984,36,19,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10985,16,17.45,36,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10985,18,62.5,8,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10985,32,32,35,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10986,11,21,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10986,20,81,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10986,76,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10986,77,13,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10987,7,30,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10987,43,46,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10987,72,34.8,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10988,7,30,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10988,62,49.3,40,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10989,6,25,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10989,11,21,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10989,41,9.65,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10990,21,10,65,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10990,34,14,60,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10990,55,24,65,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10990,61,28.5,66,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10991,2,19,50,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10991,70,15,20,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10991,76,18,90,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10992,72,34.8,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10993,29,123.79,50,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10993,41,9.65,35,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10994,59,55,18,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10995,51,53,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10995,60,34,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10996,42,14,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10997,32,32,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10997,46,12,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10997,52,7,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10998,24,4.5,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10998,61,28.5,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10998,74,10,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10998,75,7.75,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10999,41,9.65,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10999,51,53,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (10999,77,13,21,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11000,4,22,25,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11000,24,4.5,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11000,77,13,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11001,7,30,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11001,22,21,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11001,46,12,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11001,55,24,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11002,13,6,56,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11002,35,18,15,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11002,42,14,24,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11002,55,24,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11003,1,18,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11003,40,18.4,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11003,52,7,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11004,26,31.23,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11004,76,18,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11005,1,18,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11005,59,55,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11006,1,18,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11006,29,123.79,2,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11007,8,40,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11007,29,123.79,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11007,42,14,14,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11008,28,45.6,70,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11008,34,14,90,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11008,71,21.5,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11009,24,4.5,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11009,36,19,18,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11009,60,34,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11010,7,30,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11010,24,4.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11011,58,13.25,40,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11011,71,21.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11012,19,9.2,50,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11012,60,34,36,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11012,71,21.5,60,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11013,23,9,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11013,42,14,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11013,45,9.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11013,68,12.5,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11014,41,9.65,28,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11015,30,25.89,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11015,77,13,18,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11016,31,12.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11016,36,19,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11017,3,10,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11017,59,55,110,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11017,70,15,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11018,12,38,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11018,18,62.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11018,56,38,5,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11019,46,12,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11019,49,20,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11020,10,31,24,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11021,2,19,11,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11021,20,81,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11021,26,31.23,63,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11021,51,53,44,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11021,72,34.8,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11022,19,9.2,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11022,69,36,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11023,7,30,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11023,43,46,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11024,26,31.23,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11024,33,2.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11024,65,21.05,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11024,71,21.5,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11025,1,18,10,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11025,13,6,20,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11026,18,62.5,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11026,51,53,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11027,24,4.5,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11027,62,49.3,21,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11028,55,24,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11028,59,55,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11029,56,38,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11029,63,43.9,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11030,2,19,100,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11030,5,21.35,70,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11030,29,123.79,60,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11030,59,55,100,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11031,1,18,45,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11031,13,6,80,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11031,24,4.5,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11031,64,33.25,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11031,71,21.5,16,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11032,36,19,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11032,38,263.5,25,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11032,59,55,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11033,53,32.8,70,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11033,69,36,36,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11034,21,10,15,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11034,44,19.45,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11034,61,28.5,6,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11035,1,18,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11035,35,18,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11035,42,14,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11035,54,7.45,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11036,13,6,7,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11036,59,55,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11037,70,15,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11038,40,18.4,5,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11038,52,7,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11038,71,21.5,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11039,28,45.6,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11039,35,18,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11039,49,20,60,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11039,57,19.5,28,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11040,21,10,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11041,2,19,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11041,63,43.9,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11042,44,19.45,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11042,61,28.5,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11043,11,21,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11044,62,49.3,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11045,33,2.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11045,51,53,24,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11046,12,38,20,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11046,32,32,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11046,35,18,18,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11047,1,18,25,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11047,5,21.35,30,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11048,68,12.5,42,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11049,2,19,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11049,12,38,4,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11050,76,18,50,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11051,24,4.5,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11052,43,46,30,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11052,61,28.5,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11053,18,62.5,35,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11053,32,32,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11053,64,33.25,25,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11054,33,2.5,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11054,67,14,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11055,24,4.5,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11055,25,14,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11055,51,53,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11055,57,19.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11056,7,30,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11056,55,24,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11056,60,34,50,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11057,70,15,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11058,21,10,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11058,60,34,21,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11058,61,28.5,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11059,13,6,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11059,17,39,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11059,60,34,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11060,60,34,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11060,77,13,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11061,60,34,15,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11062,53,32.8,10,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11062,70,15,12,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11063,34,14,30,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11063,40,18.4,40,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11063,41,9.65,30,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11064,17,39,77,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11064,41,9.65,12,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11064,53,32.8,25,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11064,55,24,4,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11064,68,12.5,55,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11065,30,25.89,4,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11065,54,7.45,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11066,16,17.45,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11066,19,9.2,42,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11066,34,14,35,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11067,41,9.65,9,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11068,28,45.6,8,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11068,43,46,36,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11068,77,13,28,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11069,39,18,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11070,1,18,40,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11070,2,19,20,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11070,16,17.45,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11070,31,12.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11071,7,30,15,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11071,13,6,10,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11072,2,19,8,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11072,41,9.65,40,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11072,50,16.25,22,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11072,64,33.25,130,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11073,11,21,10,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11073,24,4.5,20,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11074,16,17.45,14,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11075,2,19,10,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11075,46,12,30,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11075,76,18,2,0.15);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11076,6,25,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11076,14,23.25,20,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11076,19,9.2,10,0.25);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,2,19,24,0.2);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,3,10,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,4,22,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,6,25,1,0.02);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,7,30,1,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,8,40,2,0.1);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,10,31,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,12,38,2,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,13,6,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,14,23.25,1,0.03);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,16,17.45,2,0.03);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,20,81,1,0.04);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,23,9,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,32,32,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,39,18,2,0.05);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,41,9.65,3,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,46,12,3,0.02);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,52,7,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,55,24,2,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,60,34,2,0.06);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,64,33.25,2,0.03);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,66,17,1,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,73,15,2,0.01);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,75,7.75,4,0);
Insert into ORDER_DETAILS (ORDERID,PRODUCTID,UNITPRICE,QUANTITY,DISCOUNT) values (11077,77,13,2,0);

---------------------------------------------------
--   END DATA FOR TABLE ORDER_DETAILS
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE PRODUCTS
--   FILTER = none used
---------------------------------------------------
REM INSERTING into PRODUCTS
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (1,'Chai',1,1,'10 boxes x 20 bags',18,39,0,10,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (2,'Chang',1,1,'24 - 12 oz bottles',19,17,40,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (3,'Aniseed Syrup',1,2,'12 - 550 ml bottles',10,13,70,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (4,'Chef Anton''s Cajun Seasoning',2,2,'48 - 6 oz jars',22,53,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (5,'Chef Anton''s Gumbo Mix',2,2,'36 boxes',21.35,0,0,0,1);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (6,'Grandma''s Boysenberry Spread',3,2,'12 - 8 oz jars',25,120,0,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (7,'Uncle Bob''s Organic Dried Pears',3,7,'12 - 1 lb pkgs.',30,15,0,10,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (8,'Northwoods Cranberry Sauce',3,2,'12 - 12 oz jars',40,6,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (9,'Mishi Kobe Niku',4,6,'18 - 500 g pkgs.',97,29,0,0,1);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (10,'Ikura',4,8,'12 - 200 ml jars',31,31,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (11,'Queso Cabrales',5,4,'1 kg pkg.',21,22,30,30,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (12,'Queso Manchego La Pastora',5,4,'10 - 500 g pkgs.',38,86,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (13,'Konbu',6,8,'2 kg box',6,24,0,5,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (14,'Tofu',6,7,'40 - 100 g pkgs.',23.25,35,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (15,'Genen Shouyu',6,2,'24 - 250 ml bottles',15.5,39,0,5,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (16,'Pavlova',7,3,'32 - 500 g boxes',17.45,29,0,10,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (17,'Alice Mutton',7,6,'20 - 1 kg tins',39,0,0,0,1);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (18,'Carnarvon Tigers',7,8,'16 kg pkg.',62.5,42,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (19,'Teatime Chocolate Biscuits',8,3,'10 boxes x 12 pieces',9.2,25,0,5,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (20,'Sir Rodney''s Marmalade',8,3,'30 gift boxes',81,40,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (21,'Sir Rodney''s Scones',8,3,'24 pkgs. x 4 pieces',10,3,40,5,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (22,'Gustaf''s Knackebrod',9,5,'24 - 500 g pkgs.',21,104,0,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (23,'Tunnbrod',9,5,'12 - 250 g pkgs.',9,61,0,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (24,'Guarana Fantastica',10,1,'12 - 355 ml cans',4.5,20,0,0,1);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (25,'NuNuCa Nu?-Nougat-Creme',11,3,'20 - 450 g glasses',14,76,0,30,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (26,'Gumbar Gummibarchen',11,3,'100 - 250 g bags',31.23,15,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (27,'Schoggi Schokolade',11,3,'100 - 100 g pieces',43.9,49,0,30,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (28,'Rossle Sauerkraut',12,7,'25 - 825 g cans',45.6,26,0,0,1);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (29,'Thuringer Rostbratwurst',12,6,'50 bags x 30 sausgs.',123.79,0,0,0,1);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (30,'Nord-Ost Matjeshering',13,8,'10 - 200 g glasses',25.89,10,0,15,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (31,'Gorgonzola Telino',14,4,'12 - 100 g pkgs',12.5,0,70,20,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (32,'Mascarpone Fabioli',14,4,'24 - 200 g pkgs.',32,9,40,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (33,'Geitost',15,4,'500 g',2.5,112,0,20,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (34,'Sasquatch Ale',16,1,'24 - 12 oz bottles',14,111,0,15,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (35,'Steeleye Stout',16,1,'24 - 12 oz bottles',18,20,0,15,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (36,'Inlagd Sill',17,8,'24 - 250 g  jars',19,112,0,20,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (37,'Gravad lax',17,8,'12 - 500 g pkgs.',26,11,50,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (38,'Cote de Blaye',18,1,'12 - 75 cl bottles',263.5,17,0,15,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (39,'Chartreuse verte',18,1,'750 cc per bottle',18,69,0,5,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (40,'Boston Crab Meat',19,8,'24 - 4 oz tins',18.4,123,0,30,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (41,'Jack''s New England Clam Chowder',19,8,'12 - 12 oz cans',9.65,85,0,10,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (42,'Singaporean Hokkien Fried Mee',20,5,'32 - 1 kg pkgs.',14,26,0,0,1);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (43,'Ipoh Coffee',20,1,'16 - 500 g tins',46,17,10,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (44,'Gula Malacca',20,2,'20 - 2 kg bags',19.45,27,0,15,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (45,'Rogede sild',21,8,'1k pkg.',9.5,5,70,15,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (46,'Spegesild',21,8,'4 - 450 g glasses',12,95,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (47,'Zaanse koeken',22,3,'10 - 4 oz boxes',9.5,36,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (48,'Chocolade',22,3,'10 pkgs.',12.75,15,70,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (49,'Maxilaku',23,3,'24 - 50 g pkgs.',20,10,60,15,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (50,'Valkoinen suklaa',23,3,'12 - 100 g bars',16.25,65,0,30,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (51,'Manjimup Dried Apples',24,7,'50 - 300 g pkgs.',53,20,0,10,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (52,'Filo Mix',24,5,'16 - 2 kg boxes',7,38,0,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (53,'Perth Pasties',24,6,'48 pieces',32.8,0,0,0,1);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (54,'Tourtiere',25,6,'16 pies',7.45,21,0,10,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (55,'Pate chinois',25,6,'24 boxes x 2 pies',24,115,0,20,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (56,'Gnocchi di nonna Alice',26,5,'24 - 250 g pkgs.',38,21,10,30,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (57,'Ravioli Angelo',26,5,'24 - 250 g pkgs.',19.5,36,0,20,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (58,'Escargots de Bourgogne',27,8,'24 pieces',13.25,62,0,20,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (59,'Raclette Courdavault',28,4,'5 kg pkg.',55,79,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (60,'Camembert Pierrot',28,4,'15 - 300 g rounds',34,19,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (61,'Sirop d''erable',29,2,'24 - 500 ml bottles',28.5,113,0,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (62,'Tarte au sucre',29,3,'48 pies',49.3,17,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (63,'Vegie-spread',7,2,'15 - 625 g jars',43.9,24,0,5,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (64,'Wimmers gute Semmelknodel',12,5,'20 bags x 4 pieces',33.25,22,80,30,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (65,'Louisiana Fiery Hot Pepper Sauce',2,2,'32 - 8 oz bottles',21.05,76,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (66,'Louisiana Hot Spiced Okra',2,2,'24 - 8 oz jars',17,4,100,20,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (67,'Laughing Lumberjack Lager',16,1,'24 - 12 oz bottles',14,52,0,10,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (68,'Scottish Longbreads',8,3,'10 boxes x 8 pieces',12.5,6,10,15,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (69,'Gudbrandsdalsost',15,4,'10 kg pkg.',36,26,0,15,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (70,'Outback Lager',7,1,'24 - 355 ml bottles',15,15,10,30,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (71,'Flotemysost',15,4,'10 - 500 g pkgs.',21.5,26,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (72,'Mozzarella di Giovanni',14,4,'24 - 200 g pkgs.',34.8,14,0,0,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (73,'Rod Kaviar',17,8,'24 - 150 g jars',15,101,0,5,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (74,'Longlife Tofu',4,7,'5 kg pkg.',10,4,20,5,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (75,'Rhonbrau Klosterbier',12,1,'24 - 0.5 l bottles',7.75,125,0,25,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (76,'Lakkalikoori',23,1,'500 ml',18,57,0,20,0);
Insert into PRODUCTS (PRODUCTID,PRODUCTNAME,SUPPLIERID,CATEGORYID,QUANTITYPERUNIT,UNITPRICE,UNITSINSTOCK,UNITSONORDER,REORDERLEVEL,DISCONTINUED) values (77,'Original Frankfurter grune So?e',12,2,'12 boxes',13,32,0,15,0);

---------------------------------------------------
--   END DATA FOR TABLE PRODUCTS
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE REGION
--   FILTER = none used
---------------------------------------------------
REM INSERTING into REGION
Insert into REGION (REGIONID,REGIONDESCRIPTION) values (1,'Eastern                                           ');
Insert into REGION (REGIONID,REGIONDESCRIPTION) values (2,'Western                                           ');
Insert into REGION (REGIONID,REGIONDESCRIPTION) values (3,'Northern                                          ');
Insert into REGION (REGIONID,REGIONDESCRIPTION) values (4,'Southern                                          ');

---------------------------------------------------
--   END DATA FOR TABLE REGION
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE SHIPPERS
--   FILTER = none used
---------------------------------------------------
REM INSERTING into SHIPPERS
Insert into SHIPPERS (SHIPPERID,COMPANYNAME,PHONE) values (1,'Speedy Express','(503) 555-9831');
Insert into SHIPPERS (SHIPPERID,COMPANYNAME,PHONE) values (2,'United Package','(503) 555-3199');
Insert into SHIPPERS (SHIPPERID,COMPANYNAME,PHONE) values (3,'Federal Shipping','(503) 555-9931');

---------------------------------------------------
--   END DATA FOR TABLE SHIPPERS
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE SUPPLIERS
--   FILTER = none used
---------------------------------------------------
REM INSERTING into SUPPLIERS
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (1,'Exotic Liquids','Charlotte Cooper','Purchasing Manager','49 Gilbert St.','London',null,'EC1 4SD','UK','(171) 555-2222',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (2,'New Orleans Cajun Delights','Shelley Burke','Order Administrator','P.O. Box 78934','New Orleans','LA','70117','USA','(100) 555-4822',null,'#CAJUN.HTM#');
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (3,'Grandma Kelly''s Homestead','Regina Murphy','Sales Representative','707 Oxford Rd.','Ann Arbor','MI','48104','USA','(313) 555-5735','(313) 555-3349',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (4,'Tokyo Traders','Yoshi Nagase','Marketing Manager','9-8 Sekimai Musashino-shi','Tokyo',null,'100','Japan','(03) 3555-5011',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (5,'Cooperativa de Quesos ''Las Cabras''','Antonio del Valle Saavedra','Export Administrator','Calle del Rosal 4','Oviedo','Asturias','33007','Spain','(98) 598 76 54',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (6,'Mayumi''s','Mayumi Ohno','Marketing Representative','92 Setsuko Chuo-ku','Osaka',null,'545','Japan','(06) 431-7877',null,'Mayumi''s (on the World Wide Web)#http://www.microsoft.com/accessdev/sampleapps/mayumi.htm#');
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (7,'Pavlova, Ltd.','Ian Devling','Marketing Manager','74 Rose St. Moonie Ponds','Melbourne','Victoria','3058','Australia','(03) 444-2343','(03) 444-6588',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (8,'Specialty Biscuits, Ltd.','Peter Wilson','Sales Representative','29 King''s Way','Manchester',null,'M14 GSD','UK','(161) 555-4448',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (9,'PB Knackebrod AB','Lars Peterson','Sales Agent','Kaloadagatan 13','Goteborg',null,'S-345 67','Sweden','031-987 65 43','031-987 65 91',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (10,'Refrescos Americanas LTDA','Carlos Diaz','Marketing Manager','Av. das Americanas 12.890','Sao Paulo',null,'5442','Brazil','(11) 555 4640',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (11,'Heli Su?waren GmbH & Co. KG','Petra Winkler','Sales Manager','Tiergartenstra?e 5','Berlin',null,'10785','Germany','(010) 9984510',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (12,'Plutzer Lebensmittelgro?markte AG','Martin Bein','International Marketing Mgr.','Bogenallee 51','Frankfurt',null,'60439','Germany','(069) 992755',null,'Plutzer (on the World Wide Web)#http://www.microsoft.com/accessdev/sampleapps/plutzer.htm#');
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (13,'Nord-Ost-Fisch Handelsgesellschaft mbH','Sven Petersen','Coordinator Foreign Markets','Frahmredder 112a','Cuxhaven',null,'27478','Germany','(04721) 8713','(04721) 8714',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (14,'Formaggi Fortini s.r.l.','Elio Rossi','Sales Representative','Viale Dante, 75','Ravenna',null,'48100','Italy','(0544) 60323','(0544) 60603','#FORMAGGI.HTM#');
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (15,'Norske Meierier','Beate Vileid','Marketing Manager','Hatlevegen 5','Sandvika',null,'1320','Norway','(0)2-953010',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (16,'Bigfoot Breweries','Cheryl Saylor','Regional Account Rep.','3400 - 8th Avenue Suite 210','Bend','OR','97101','USA','(503) 555-9931',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (17,'Svensk Sjofoda AB','Michael Bjorn','Sales Representative','Brovallavagen 231','Stockholm',null,'S-123 45','Sweden','08-123 45 67',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (18,'Aux joyeux ecclesiastiques','Guylene Nodier','Sales Manager','203, Rue des Francs-Bourgeois','Paris',null,'75004','France','(1) 03.83.00.68','(1) 03.83.00.62',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (19,'New England Seafood Cannery','Robb Merchant','Wholesale Account Agent','Order Processing Dept. 2100 Paul Revere Blvd.','Boston','MA','02134','USA','(617) 555-3267','(617) 555-3389',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (20,'Leka Trading','Chandra Leka','Owner','471 Serangoon Loop, Suite #402','Singapore',null,'0512','Singapore','555-8787',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (21,'Lyngbysild','Niels Petersen','Sales Manager','Lyngbysild Fiskebakken 10','Lyngby',null,'2800','Denmark','43844108','43844115',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (22,'Zaanse Snoepfabriek','Dirk Luchte','Accounting Manager','Verkoop Rijnweg 22','Zaandam',null,'9999 ZZ','Netherlands','(12345) 1212','(12345) 1210',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (23,'Karkki Oy','Anne Heikkonen','Product Manager','Valtakatu 12','Lappeenranta',null,'53120','Finland','(953) 10956',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (24,'G''day, Mate','Wendy Mackenzie','Sales Representative','170 Prince Edward Parade Hunter''s Hill','Sydney','NSW','2042','Australia','(02) 555-5914','(02) 555-4873','G''day Mate (on the World Wide Web)#http://www.microsoft.com/accessdev/sampleapps/gdaymate.htm#');
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (25,'Ma Maison','Jean-Guy Lauzon','Marketing Manager','2960 Rue St. Laurent','Montreal','Quebec','H1J 1C3','Canada','(514) 555-9022',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (26,'Pasta Buttini s.r.l.','Giovanni Giudici','Order Administrator','Via dei Gelsomini, 153','Salerno',null,'84100','Italy','(089) 6547665','(089) 6547667',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (27,'Escargots Nouveaux','Marie Delamare','Sales Manager','22, rue H. Voiron','Montceau',null,'71300','France','85.57.00.07',null,null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (28,'Gai paturage','Eliane Noz','Sales Representative','Bat. B 3, rue des Alpes','Annecy',null,'74000','France','38.76.98.06','38.76.98.58',null);
Insert into SUPPLIERS (SUPPLIERID,COMPANYNAME,CONTACTNAME,CONTACTTITLE,ADDRESS,CITY,REGION,POSTALCODE,COUNTRY,PHONE,FAX,HOMEPAGE) values (29,'Forets d''erables','Chantal Goulet','Accounting Manager','148 rue Chasseur','Ste-Hyacinthe','Quebec','J2S 7S8','Canada','(514) 555-2955','(514) 555-2921',null);

---------------------------------------------------
--   END DATA FOR TABLE SUPPLIERS
---------------------------------------------------


---------------------------------------------------
--   DATA FOR TABLE TERRITORIES
--   FILTER = none used
---------------------------------------------------
REM INSERTING into TERRITORIES
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('01581','Westboro                                          ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('01730','Bedford                                           ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('01833','Georgetow                                         ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('02116','Boston                                            ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('02139','Cambridge                                         ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('02184','Braintree                                         ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('02903','Providence                                        ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('03049','Hollis                                            ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('03801','Portsmouth                                        ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('06897','Wilton                                            ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('07960','Morristown                                        ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('08837','Edison                                            ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('10019','New York                                          ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('10038','New York                                          ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('11747','Mellvile                                          ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('14450','Fairport                                          ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('19428','Philadelphia                                      ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('19713','Neward                                            ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('20852','Rockville                                         ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('27403','Greensboro                                        ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('27511','Cary                                              ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('29202','Columbia                                          ',4);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('30346','Atlanta                                           ',4);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('31406','Savannah                                          ',4);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('32859','Orlando                                           ',4);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('33607','Tampa                                             ',4);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('40222','Louisville                                        ',1);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('44122','Beachwood                                         ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('45839','Findlay                                           ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('48075','Southfield                                        ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('48084','Troy                                              ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('48304','Bloomfield Hills                                  ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('53404','Racine                                            ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('55113','Roseville                                         ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('55439','Minneapolis                                       ',3);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('60179','Hoffman Estates                                   ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('60601','Chicago                                           ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('72716','Bentonville                                       ',4);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('75234','Dallas                                            ',4);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('78759','Austin                                            ',4);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('80202','Denver                                            ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('80909','Colorado Springs                                  ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('85014','Phoenix                                           ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('85251','Scottsdale                                        ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('90405','Santa Monica                                      ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('94025','Menlo Park                                        ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('94105','San Francisco                                     ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('95008','Campbell                                          ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('95054','Santa Clara                                       ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('95060','Santa Cruz                                        ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('98004','Bellevue                                          ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('98052','Redmond                                           ',2);
Insert into TERRITORIES (TERRITORYID,TERRITORYDESCRIPTION,REGIONID) values ('98104','Seattle                                           ',2);

---------------------------------------------------
--   END DATA FOR TABLE TERRITORIES
---------------------------------------------------

--------------------------------------------------------
--  Constraints for Table CUSTOMERDEMOGRAPHICS
--------------------------------------------------------

  ALTER TABLE "CUSTOMERDEMOGRAPHICS" ADD CONSTRAINT "PK_CUSTOMERDEMOGRAPHICS" PRIMARY KEY ("CUSTOMERTYPEID") ENABLE;
 
  ALTER TABLE "CUSTOMERDEMOGRAPHICS" MODIFY ("CUSTOMERTYPEID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table EMPLOYEETERRITORIES
--------------------------------------------------------

  ALTER TABLE "EMPLOYEETERRITORIES" ADD CONSTRAINT "PK_EMPLOYEETERRITORIES" PRIMARY KEY ("EMPLOYEEID", "TERRITORYID") ENABLE;
 
  ALTER TABLE "EMPLOYEETERRITORIES" MODIFY ("EMPLOYEEID" NOT NULL ENABLE);
 
  ALTER TABLE "EMPLOYEETERRITORIES" MODIFY ("TERRITORYID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table PRODUCTS
--------------------------------------------------------

  ALTER TABLE "PRODUCTS" ADD CONSTRAINT "CK_PRODUCTS_UNITPRICE" CHECK (
  ( UnitPrice >= (0) )
) ENABLE;
 
  ALTER TABLE "PRODUCTS" ADD CONSTRAINT "CK_REORDERLEVEL" CHECK (
  ( ReorderLevel >= (0) )
) ENABLE;
 
  ALTER TABLE "PRODUCTS" ADD CONSTRAINT "CK_UNITSINSTOCK" CHECK (
  ( UnitsInStock >= (0) )
) ENABLE;
 
  ALTER TABLE "PRODUCTS" ADD CONSTRAINT "CK_UNITSONORDER" CHECK (
  ( UnitsOnOrder >= (0) )
) ENABLE;
 
  ALTER TABLE "PRODUCTS" ADD CONSTRAINT "PK_PRODUCTS" PRIMARY KEY ("PRODUCTID") ENABLE;
 
  ALTER TABLE "PRODUCTS" MODIFY ("PRODUCTID" NOT NULL ENABLE);
 
  ALTER TABLE "PRODUCTS" MODIFY ("PRODUCTNAME" NOT NULL ENABLE);
 
  ALTER TABLE "PRODUCTS" MODIFY ("DISCONTINUED" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table EMPLOYEES
--------------------------------------------------------

  ALTER TABLE "EMPLOYEES" ADD CONSTRAINT "PK_EMPLOYEES" PRIMARY KEY ("EMPLOYEEID") ENABLE;
 
  ALTER TABLE "EMPLOYEES" MODIFY ("EMPLOYEEID" NOT NULL ENABLE);
 
  ALTER TABLE "EMPLOYEES" MODIFY ("LASTNAME" NOT NULL ENABLE);
 
  ALTER TABLE "EMPLOYEES" MODIFY ("FIRSTNAME" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table SHIPPERS
--------------------------------------------------------

  ALTER TABLE "SHIPPERS" ADD CONSTRAINT "PK_SHIPPERS" PRIMARY KEY ("SHIPPERID") ENABLE;
 
  ALTER TABLE "SHIPPERS" MODIFY ("SHIPPERID" NOT NULL ENABLE);
 
  ALTER TABLE "SHIPPERS" MODIFY ("COMPANYNAME" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table TERRITORIES
--------------------------------------------------------

  ALTER TABLE "TERRITORIES" ADD CONSTRAINT "PK_TERRITORIES" PRIMARY KEY ("TERRITORYID") ENABLE;
 
  ALTER TABLE "TERRITORIES" MODIFY ("TERRITORYID" NOT NULL ENABLE);
 
  ALTER TABLE "TERRITORIES" MODIFY ("TERRITORYDESCRIPTION" NOT NULL ENABLE);
 
  ALTER TABLE "TERRITORIES" MODIFY ("REGIONID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table ORDER_DETAILS
--------------------------------------------------------

  ALTER TABLE "ORDER_DETAILS" ADD CONSTRAINT "CK_DISCOUNT" CHECK (
  ( Discount >= (0)
                   AND Discount <= (1) )
) ENABLE;
 
  ALTER TABLE "ORDER_DETAILS" ADD CONSTRAINT "CK_QUANTITY" CHECK (
  ( Quantity > (0) )
) ENABLE;
 
  ALTER TABLE "ORDER_DETAILS" ADD CONSTRAINT "CK_UNITPRICE" CHECK (
  ( UnitPrice >= (0) )
) ENABLE;
 
  ALTER TABLE "ORDER_DETAILS" ADD CONSTRAINT "PK_ORDER_DETAILS" PRIMARY KEY ("ORDERID", "PRODUCTID") ENABLE;
 
  ALTER TABLE "ORDER_DETAILS" MODIFY ("ORDERID" NOT NULL ENABLE);
 
  ALTER TABLE "ORDER_DETAILS" MODIFY ("PRODUCTID" NOT NULL ENABLE);
 
  ALTER TABLE "ORDER_DETAILS" MODIFY ("UNITPRICE" NOT NULL ENABLE);
 
  ALTER TABLE "ORDER_DETAILS" MODIFY ("QUANTITY" NOT NULL ENABLE);
 
  ALTER TABLE "ORDER_DETAILS" MODIFY ("DISCOUNT" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table ORDERS
--------------------------------------------------------

  ALTER TABLE "ORDERS" ADD CONSTRAINT "PK_ORDERS" PRIMARY KEY ("ORDERID") ENABLE;
 
  ALTER TABLE "ORDERS" MODIFY ("ORDERID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table SUPPLIERS
--------------------------------------------------------

  ALTER TABLE "SUPPLIERS" ADD CONSTRAINT "PK_SUPPLIERS" PRIMARY KEY ("SUPPLIERID") ENABLE;
 
  ALTER TABLE "SUPPLIERS" MODIFY ("SUPPLIERID" NOT NULL ENABLE);
 
  ALTER TABLE "SUPPLIERS" MODIFY ("COMPANYNAME" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table CUSTOMERCUSTOMERDEMO
--------------------------------------------------------

  ALTER TABLE "CUSTOMERCUSTOMERDEMO" ADD CONSTRAINT "PK_CUSTOMERCUSTOMERDEMO" PRIMARY KEY ("CUSTOMERID", "CUSTOMERTYPEID") ENABLE;
 
  ALTER TABLE "CUSTOMERCUSTOMERDEMO" MODIFY ("CUSTOMERID" NOT NULL ENABLE);
 
  ALTER TABLE "CUSTOMERCUSTOMERDEMO" MODIFY ("CUSTOMERTYPEID" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table CUSTOMERS
--------------------------------------------------------

  ALTER TABLE "CUSTOMERS" ADD CONSTRAINT "PK_CUSTOMERS" PRIMARY KEY ("CUSTOMERID") ENABLE;
 
  ALTER TABLE "CUSTOMERS" MODIFY ("CUSTOMERID" NOT NULL ENABLE);
 
  ALTER TABLE "CUSTOMERS" MODIFY ("COMPANYNAME" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table REGION
--------------------------------------------------------

  ALTER TABLE "REGION" ADD CONSTRAINT "PK_REGION" PRIMARY KEY ("REGIONID") ENABLE;
 
  ALTER TABLE "REGION" MODIFY ("REGIONID" NOT NULL ENABLE);
 
  ALTER TABLE "REGION" MODIFY ("REGIONDESCRIPTION" NOT NULL ENABLE);
--------------------------------------------------------
--  Constraints for Table CATEGORIES
--------------------------------------------------------

  ALTER TABLE "CATEGORIES" ADD CONSTRAINT "PK_CATEGORIES" PRIMARY KEY ("CATEGORYID") ENABLE;
 
  ALTER TABLE "CATEGORIES" MODIFY ("CATEGORYID" NOT NULL ENABLE);
 
  ALTER TABLE "CATEGORIES" MODIFY ("CATEGORYNAME" NOT NULL ENABLE);
--------------------------------------------------------
--  DDL for Index CUSTOMERID
--------------------------------------------------------

  CREATE INDEX "CUSTOMERID" ON "ORDERS" ("CUSTOMERID") 
  ;
--------------------------------------------------------
--  DDL for Index POSTALCODE_1
--------------------------------------------------------

  CREATE INDEX "POSTALCODE_1" ON "SUPPLIERS" ("POSTALCODE") 
  ;
--------------------------------------------------------
--  DDL for Index PRODUCTNAME
--------------------------------------------------------

  CREATE INDEX "PRODUCTNAME" ON "PRODUCTS" ("PRODUCTNAME") 
  ;
--------------------------------------------------------
--  DDL for Index POSTALCODE
--------------------------------------------------------

  CREATE INDEX "POSTALCODE" ON "CUSTOMERS" ("POSTALCODE") 
  ;
--------------------------------------------------------
--  DDL for Index SUPPLIERID
--------------------------------------------------------

  CREATE INDEX "SUPPLIERID" ON "PRODUCTS" ("SUPPLIERID") 
  ;
--------------------------------------------------------
--  DDL for Index LASTNAME
--------------------------------------------------------

  CREATE INDEX "LASTNAME" ON "EMPLOYEES" ("LASTNAME") 
  ;
--------------------------------------------------------
--  DDL for Index SHIPPOSTALCODE
--------------------------------------------------------

  CREATE INDEX "SHIPPOSTALCODE" ON "ORDERS" ("SHIPPOSTALCODE") 
  ;
--------------------------------------------------------
--  DDL for Index PRODUCTID
--------------------------------------------------------

  CREATE INDEX "PRODUCTID" ON "ORDER_DETAILS" ("PRODUCTID") 
  ;
--------------------------------------------------------
--  DDL for Index CATEGORIESPRODUCTS
--------------------------------------------------------

  CREATE INDEX "CATEGORIESPRODUCTS" ON "PRODUCTS" ("CATEGORYID") 
  ;
--------------------------------------------------------
--  DDL for Index CITY
--------------------------------------------------------

  CREATE INDEX "CITY" ON "CUSTOMERS" ("CITY") 
  ;
--------------------------------------------------------
--  DDL for Index COMPANYNAME
--------------------------------------------------------

  CREATE INDEX "COMPANYNAME" ON "CUSTOMERS" ("COMPANYNAME") 
  ;
--------------------------------------------------------
--  DDL for Index EMPLOYEEID
--------------------------------------------------------

  CREATE INDEX "EMPLOYEEID" ON "ORDERS" ("EMPLOYEEID") 
  ;
--------------------------------------------------------
--  DDL for Index ORDERDATE
--------------------------------------------------------

  CREATE INDEX "ORDERDATE" ON "ORDERS" ("ORDERDATE") 
  ;
--------------------------------------------------------
--  DDL for Index SHIPPEDDATE
--------------------------------------------------------

  CREATE INDEX "SHIPPEDDATE" ON "ORDERS" ("SHIPPEDDATE") 
  ;
--------------------------------------------------------
--  DDL for Index CATEGORYNAME
--------------------------------------------------------

  CREATE INDEX "CATEGORYNAME" ON "CATEGORIES" ("CATEGORYNAME") 
  ;
--------------------------------------------------------
--  DDL for Index COMPANYNAME_1
--------------------------------------------------------

  CREATE INDEX "COMPANYNAME_1" ON "SUPPLIERS" ("COMPANYNAME") 
  ;
--------------------------------------------------------
--  DDL for Index POSTALCODE_2
--------------------------------------------------------

  CREATE INDEX "POSTALCODE_2" ON "EMPLOYEES" ("POSTALCODE") 
  ;
--------------------------------------------------------
--  DDL for Index SHIPPERSORDERS
--------------------------------------------------------

  CREATE INDEX "SHIPPERSORDERS" ON "ORDERS" ("SHIPVIA") 
  ;
--------------------------------------------------------
--  DDL for Index ORDERID
--------------------------------------------------------

  CREATE INDEX "ORDERID" ON "ORDER_DETAILS" ("ORDERID") 
  ;
--------------------------------------------------------
--  DDL for Index REGION
--------------------------------------------------------

  CREATE INDEX "REGION" ON "CUSTOMERS" ("REGION") 
  ;

--------------------------------------------------------
--  Ref Constraints for Table CUSTOMERCUSTOMERDEMO
--------------------------------------------------------

  ALTER TABLE "CUSTOMERCUSTOMERDEMO" ADD CONSTRAINT "FK_CUSTOMERCUSTOMERDEMO" FOREIGN KEY ("CUSTOMERTYPEID")
	  REFERENCES "CUSTOMERDEMOGRAPHICS" ("CUSTOMERTYPEID") ENABLE;
 
  ALTER TABLE "CUSTOMERCUSTOMERDEMO" ADD CONSTRAINT "FK_CUSTOMERCUSTOMERDEMO_CUSTOM" FOREIGN KEY ("CUSTOMERID")
	  REFERENCES "CUSTOMERS" ("CUSTOMERID") ENABLE;


--------------------------------------------------------
--  Ref Constraints for Table EMPLOYEES
--------------------------------------------------------

  ALTER TABLE "EMPLOYEES" ADD CONSTRAINT "FK_EMPLOYEES_EMPLOYEES" FOREIGN KEY ("REPORTSTO")
	  REFERENCES "EMPLOYEES" ("EMPLOYEEID") ENABLE;
--------------------------------------------------------
--  Ref Constraints for Table EMPLOYEETERRITORIES
--------------------------------------------------------

  ALTER TABLE "EMPLOYEETERRITORIES" ADD CONSTRAINT "FK_EMPLOYEETERRITORIES_EMPLOYE" FOREIGN KEY ("EMPLOYEEID")
	  REFERENCES "EMPLOYEES" ("EMPLOYEEID") ENABLE;
 
  ALTER TABLE "EMPLOYEETERRITORIES" ADD CONSTRAINT "FK_EMPLOYEETERRITORIES_TERRITO" FOREIGN KEY ("TERRITORYID")
	  REFERENCES "TERRITORIES" ("TERRITORYID") ENABLE;
--------------------------------------------------------
--  Ref Constraints for Table ORDERS
--------------------------------------------------------

  ALTER TABLE "ORDERS" ADD CONSTRAINT "FK_ORDERS_CUSTOMERS" FOREIGN KEY ("CUSTOMERID")
	  REFERENCES "CUSTOMERS" ("CUSTOMERID") ENABLE;
 
  ALTER TABLE "ORDERS" ADD CONSTRAINT "FK_ORDERS_EMPLOYEES" FOREIGN KEY ("EMPLOYEEID")
	  REFERENCES "EMPLOYEES" ("EMPLOYEEID") ENABLE;
 
  ALTER TABLE "ORDERS" ADD CONSTRAINT "FK_ORDERS_SHIPPERS" FOREIGN KEY ("SHIPVIA")
	  REFERENCES "SHIPPERS" ("SHIPPERID") ENABLE;
--------------------------------------------------------
--  Ref Constraints for Table ORDER_DETAILS
--------------------------------------------------------

  ALTER TABLE "ORDER_DETAILS" ADD CONSTRAINT "FK_ORDER_DETAILS_ORDERS" FOREIGN KEY ("ORDERID")
	  REFERENCES "ORDERS" ("ORDERID") ENABLE;
 
  ALTER TABLE "ORDER_DETAILS" ADD CONSTRAINT "FK_ORDER_DETAILS_PRODUCTS" FOREIGN KEY ("PRODUCTID")
	  REFERENCES "PRODUCTS" ("PRODUCTID") ENABLE;
--------------------------------------------------------
--  Ref Constraints for Table PRODUCTS
--------------------------------------------------------

  ALTER TABLE "PRODUCTS" ADD CONSTRAINT "FK_PRODUCTS_CATEGORIES" FOREIGN KEY ("CATEGORYID")
	  REFERENCES "CATEGORIES" ("CATEGORYID") ENABLE;
 
  ALTER TABLE "PRODUCTS" ADD CONSTRAINT "FK_PRODUCTS_SUPPLIERS" FOREIGN KEY ("SUPPLIERID")
	  REFERENCES "SUPPLIERS" ("SUPPLIERID") ENABLE;



--------------------------------------------------------
--  Ref Constraints for Table TERRITORIES
--------------------------------------------------------

  ALTER TABLE "TERRITORIES" ADD CONSTRAINT "FK_TERRITORIES_REGION" FOREIGN KEY ("REGIONID")
	  REFERENCES "REGION" ("REGIONID") ENABLE;
--------------------------------------------------------
--  DDL for Trigger CATEGORIES_CATEGORYID_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "CATEGORIES_CATEGORYID_TRG" BEFORE INSERT OR UPDATE ON categories
FOR EACH ROW
DECLARE
v_newVal NUMBER(12) := 0;
v_incval NUMBER(12) := 0;
BEGIN
  IF INSERTING AND :new.CategoryID IS NULL THEN
    SELECT  Categories_CategoryID_SEQ.NEXTVAL INTO v_newVal FROM DUAL;
    -- If this is the first time this table have been inserted into (sequence == 1)
    IF v_newVal = 1 THEN
      --get the max indentity value from the table
      SELECT max(CategoryID) INTO v_newVal FROM Categories;
      v_newVal := v_newVal + 1;
      --set the sequence to that value
      LOOP
           EXIT WHEN v_incval>=v_newVal;
           SELECT Categories_CategoryID_SEQ.nextval INTO v_incval FROM dual;
      END LOOP;
    END IF;
   -- assign the value from the sequence to emulate the identity column
   :new.CategoryID := v_newVal;
  END IF;
END;
/
ALTER TRIGGER "CATEGORIES_CATEGORYID_TRG" ENABLE;
--------------------------------------------------------
--  DDL for Trigger CK_BIRTHDATE_SYSDTRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "CK_BIRTHDATE_SYSDTRG" 
   BEFORE INSERT OR UPDATE
   ON Employees
   FOR EACH ROW
BEGIN
   IF NOT ( ( :NEW.BirthDate < SYSDATE ) ) THEN
   BEGIN
      raise_application_error( -20002, 'CK_Birthdate_SYSDTRG failed' );

   END;
   END IF;

END;
/
ALTER TRIGGER "CK_BIRTHDATE_SYSDTRG" ENABLE;
--------------------------------------------------------
--  DDL for Trigger EMPLOYEES_EMPLOYEEID_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "EMPLOYEES_EMPLOYEEID_TRG" BEFORE INSERT OR UPDATE ON employees
FOR EACH ROW
DECLARE
v_newVal NUMBER(12) := 0;
v_incval NUMBER(12) := 0;
BEGIN
  IF INSERTING AND :new.EmployeeID IS NULL THEN
    SELECT  Employees_EmployeeID_SEQ.NEXTVAL INTO v_newVal FROM DUAL;
    -- If this is the first time this table have been inserted into (sequence == 1)
    IF v_newVal = 1 THEN
      --get the max indentity value from the table
      SELECT max(EmployeeID) INTO v_newVal FROM Employees;
      v_newVal := v_newVal + 1;
      --set the sequence to that value
      LOOP
           EXIT WHEN v_incval>=v_newVal;
           SELECT Employees_EmployeeID_SEQ.nextval INTO v_incval FROM dual;
      END LOOP;
    END IF;
   -- assign the value from the sequence to emulate the identity column
   :new.EmployeeID := v_newVal;
  END IF;
END;
/
ALTER TRIGGER "EMPLOYEES_EMPLOYEEID_TRG" ENABLE;
--------------------------------------------------------
--  DDL for Trigger ORDERS_ORDERID_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "ORDERS_ORDERID_TRG" BEFORE INSERT OR UPDATE ON orders
FOR EACH ROW
DECLARE
v_newVal NUMBER(12) := 0;
v_incval NUMBER(12) := 0;
BEGIN
  IF INSERTING AND :new.OrderID IS NULL THEN
    SELECT  Orders_OrderID_SEQ.NEXTVAL INTO v_newVal FROM DUAL;
    -- If this is the first time this table have been inserted into (sequence == 1)
    IF v_newVal = 1 THEN
      --get the max indentity value from the table
      SELECT max(OrderID) INTO v_newVal FROM Orders;
      v_newVal := v_newVal + 1;
      --set the sequence to that value
      LOOP
           EXIT WHEN v_incval>=v_newVal;
           SELECT Orders_OrderID_SEQ.nextval INTO v_incval FROM dual;
      END LOOP;
    END IF;
   -- assign the value from the sequence to emulate the identity column
   :new.OrderID := v_newVal;
  END IF;
END;
/
ALTER TRIGGER "ORDERS_ORDERID_TRG" ENABLE;
--------------------------------------------------------
--  DDL for Trigger PRODUCTS_PRODUCTID_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "PRODUCTS_PRODUCTID_TRG" BEFORE INSERT OR UPDATE ON products
FOR EACH ROW
DECLARE
v_newVal NUMBER(12) := 0;
v_incval NUMBER(12) := 0;
BEGIN
  IF INSERTING AND :new.ProductID IS NULL THEN
    SELECT  Products_ProductID_SEQ.NEXTVAL INTO v_newVal FROM DUAL;
    -- If this is the first time this table have been inserted into (sequence == 1)
    IF v_newVal = 1 THEN
      --get the max indentity value from the table
      SELECT max(ProductID) INTO v_newVal FROM Products;
      v_newVal := v_newVal + 1;
      --set the sequence to that value
      LOOP
           EXIT WHEN v_incval>=v_newVal;
           SELECT Products_ProductID_SEQ.nextval INTO v_incval FROM dual;
      END LOOP;
    END IF;
   -- assign the value from the sequence to emulate the identity column
   :new.ProductID := v_newVal;
  END IF;
END;
/
ALTER TRIGGER "PRODUCTS_PRODUCTID_TRG" ENABLE;
--------------------------------------------------------
--  DDL for Trigger SHIPPERS_SHIPPERID_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "SHIPPERS_SHIPPERID_TRG" BEFORE INSERT OR UPDATE ON shippers
FOR EACH ROW
DECLARE
v_newVal NUMBER(12) := 0;
v_incval NUMBER(12) := 0;
BEGIN
  IF INSERTING AND :new.ShipperID IS NULL THEN
    SELECT  Shippers_ShipperID_SEQ.NEXTVAL INTO v_newVal FROM DUAL;
    -- If this is the first time this table have been inserted into (sequence == 1)
    IF v_newVal = 1 THEN
      --get the max indentity value from the table
      SELECT max(ShipperID) INTO v_newVal FROM Shippers;
      v_newVal := v_newVal + 1;
      --set the sequence to that value
      LOOP
           EXIT WHEN v_incval>=v_newVal;
           SELECT Shippers_ShipperID_SEQ.nextval INTO v_incval FROM dual;
      END LOOP;
    END IF;
   -- assign the value from the sequence to emulate the identity column
   :new.ShipperID := v_newVal;
  END IF;
END;
/
ALTER TRIGGER "SHIPPERS_SHIPPERID_TRG" ENABLE;
--------------------------------------------------------
--  DDL for Trigger SUPPLIERS_SUPPLIERID_TRG
--------------------------------------------------------

  CREATE OR REPLACE TRIGGER "SUPPLIERS_SUPPLIERID_TRG" BEFORE INSERT OR UPDATE ON suppliers
FOR EACH ROW
DECLARE
v_newVal NUMBER(12) := 0;
v_incval NUMBER(12) := 0;
BEGIN
  IF INSERTING AND :new.SupplierID IS NULL THEN
    SELECT  Suppliers_SupplierID_SEQ.NEXTVAL INTO v_newVal FROM DUAL;
    -- If this is the first time this table have been inserted into (sequence == 1)
    IF v_newVal = 1 THEN
      --get the max indentity value from the table
      SELECT max(SupplierID) INTO v_newVal FROM Suppliers;
      v_newVal := v_newVal + 1;
      --set the sequence to that value
      LOOP
           EXIT WHEN v_incval>=v_newVal;
           SELECT Suppliers_SupplierID_SEQ.nextval INTO v_incval FROM dual;
      END LOOP;
    END IF;
   -- assign the value from the sequence to emulate the identity column
   :new.SupplierID := v_newVal;
  END IF;
END;
/
ALTER TRIGGER "SUPPLIERS_SUPPLIERID_TRG" ENABLE;
--------------------------------------------------------
--  DDL for View ALPHABETICAL_LIST_OF_PRODUCTS
--------------------------------------------------------

  CREATE OR REPLACE VIEW "ALPHABETICAL_LIST_OF_PRODUCTS" ("PRODUCTID", "PRODUCTNAME", "SUPPLIERID", "CATEGORYID", "QUANTITYPERUNIT", "UNITPRICE", "UNITSINSTOCK", "UNITSONORDER", "REORDERLEVEL", "DISCONTINUED", "CATEGORYNAME") AS 
  SELECT Products.*,
          Categories.CategoryName
     FROM Categories
            JOIN Products
             ON Categories.CategoryID = Products.CategoryID
      WHERE ( ( (Products.Discontinued) = 0 ) );

--------------------------------------------------------
--  DDL for View PRODUCT_SALES_FOR_1997
--------------------------------------------------------
  CREATE OR REPLACE FORCE VIEW "PRODUCT_SALES_FOR_1997" ("CATEGORYNAME", "PRODUCTNAME", "PRODUCTSALES") AS 
  SELECT Categories.CategoryName,
          Products.ProductName,
          SUM(CAST((Order_Details.UnitPrice * Quantity * (1 - Discount) / 100) AS NUMBER(19,4)) * 100) ProductSales
     FROM ( Categories
            JOIN Products
             ON Categories.CategoryID = Products.CategoryID
             )
            JOIN ( Orders
                   JOIN Order_Details
                    ON Orders.OrderID = Order_Details.OrderID
                    )
             ON Products.ProductID = Order_Details.ProductID
      WHERE ( ( (Orders.ShippedDate) BETWEEN TO_DATE('19970101','YYYYMMDD') AND TO_DATE('19971231','YYYYMMDD') ) )
     GROUP BY Categories.CategoryName,Products.ProductName;

--------------------------------------------------------
--  DDL for View CATEGORY_SALES_FOR_1997
--------------------------------------------------------

  CREATE OR REPLACE VIEW "CATEGORY_SALES_FOR_1997" ("CATEGORYNAME", "CATEGORYSALES") AS 
  SELECT Product_Sales_for_1997.CategoryName,
          SUM(Product_Sales_for_1997.ProductSales) CategorySales
     FROM Product_Sales_for_1997
     GROUP BY Product_Sales_for_1997.CategoryName;
--------------------------------------------------------
--  DDL for View CURRENT_PRODUCT_LIST
--------------------------------------------------------

  CREATE OR REPLACE VIEW "CURRENT_PRODUCT_LIST" ("PRODUCTID", "PRODUCTNAME") AS 
  SELECT Product_List.ProductID,
          Product_List.ProductName
     FROM Products Product_List
      WHERE ( ( (Product_List.Discontinued) = 0
      --ORDER BY Product_List.ProductName
 ) );
--------------------------------------------------------
--  DDL for View CUSTOMER_AND_SUPPLIERS_BY_CITY
--------------------------------------------------------

  CREATE OR REPLACE VIEW "CUSTOMER_AND_SUPPLIERS_BY_CITY" ("CITY", "COMPANYNAME", "CONTACTNAME", "RELATIONSHIP") AS 
  SELECT City,
          CompanyName,
          ContactName,
          'Customers' Relationship
     FROM Customers
   UNION
   SELECT City,
          CompanyName,
          ContactName,
          'Suppliers'
     FROM Suppliers
     --ORDER BY City, CompanyName;
--------------------------------------------------------
--  DDL for View INVOICES
--------------------------------------------------------

  CREATE OR REPLACE VIEW "INVOICES" ("SHIPNAME", "SHIPADDRESS", "SHIPCITY", "SHIPREGION", "SHIPPOSTALCODE", "SHIPCOUNTRY", "CUSTOMERID", "CUSTOMERNAME", "ADDRESS", "CITY", "REGION", "POSTALCODE", "COUNTRY", "SALESPERSON", "ORDERID", "ORDERDATE", "REQUIREDDATE", "SHIPPEDDATE", "SHIPPERNAME", "PRODUCTID", "PRODUCTNAME", "UNITPRICE", "QUANTITY", "DISCOUNT", "EXTENDEDPRICE", "FREIGHT") AS 
  SELECT Orders.ShipName,
          Orders.ShipAddress,
          Orders.ShipCity,
          Orders.ShipRegion,
          Orders.ShipPostalCode,
          Orders.ShipCountry,
          Orders.CustomerID,
          Customers.CompanyName CustomerName,
          Customers.Address,
          Customers.City,
          Customers.Region,
          Customers.PostalCode,
          Customers.Country,
          (FirstName || ' ' || LastName) Salesperson,
          Orders.OrderID,
          Orders.OrderDate,
          Orders.RequiredDate,
          Orders.ShippedDate,
          Shippers.CompanyName ShipperName,
          Order_Details.ProductID,
          Products.ProductName,
          Order_Details.UnitPrice,
          Order_Details.Quantity,
          Order_Details.Discount,
          (CAST((Order_Details.UnitPrice * Quantity * (1 - Discount) / 100) AS NUMBER(19,4)) * 100) ExtendedPrice,
          Orders.Freight
     FROM Shippers
            JOIN ( Products
                   JOIN ( ( Employees
                            JOIN ( Customers
                                   JOIN Orders
                                    ON Customers.CustomerID = Orders.CustomerID
                                    )
                             ON Employees.EmployeeID = Orders.EmployeeID
                             )
                          JOIN Order_Details
                           ON Orders.OrderID = Order_Details.OrderID
                           )
                    ON Products.ProductID = Order_Details.ProductID
                    )
             ON Shippers.ShipperID = Orders.ShipVia;
--------------------------------------------------------
--  DDL for View ORDERS_QRY
--------------------------------------------------------

  CREATE OR REPLACE VIEW "ORDERS_QRY" ("ORDERID", "CUSTOMERID", "EMPLOYEEID", "ORDERDATE", "REQUIREDDATE", "SHIPPEDDATE", "SHIPVIA", "FREIGHT", "SHIPNAME", "SHIPADDRESS", "SHIPCITY", "SHIPREGION", "SHIPPOSTALCODE", "SHIPCOUNTRY", "COMPANYNAME", "ADDRESS", "CITY", "REGION", "POSTALCODE", "COUNTRY") AS 
  SELECT Orders.OrderID,
          Orders.CustomerID,
          Orders.EmployeeID,
          Orders.OrderDate,
          Orders.RequiredDate,
          Orders.ShippedDate,
          Orders.ShipVia,
          Orders.Freight,
          Orders.ShipName,
          Orders.ShipAddress,
          Orders.ShipCity,
          Orders.ShipRegion,
          Orders.ShipPostalCode,
          Orders.ShipCountry,
          Customers.CompanyName,
          Customers.Address,
          Customers.City,
          Customers.Region,
          Customers.PostalCode,
          Customers.Country
     FROM Customers
            JOIN Orders
             ON Customers.CustomerID = Orders.CustomerID;
--------------------------------------------------------
--  DDL for View ORDER_DETAILS_EXTENDED
--------------------------------------------------------

  CREATE OR REPLACE VIEW "ORDER_DETAILS_EXTENDED" ("ORDERID", "PRODUCTID", "PRODUCTNAME", "UNITPRICE", "QUANTITY", "DISCOUNT", "EXTENDEDPRICE") AS 
  SELECT Order_Details.OrderID,
          Order_Details.ProductID,
          Products.ProductName,
          Order_Details.UnitPrice,
          Order_Details.Quantity,
          Order_Details.Discount,
          (CAST((Order_Details.UnitPrice * Quantity * (1 - Discount) / 100) AS NUMBER(19,4)) * 100) ExtendedPrice
     FROM Products
            JOIN Order_Details
             ON Products.ProductID = Order_Details.ProductID
             --ORDER BY "Order Details".OrderID;
--------------------------------------------------------
--  DDL for View ORDER_SUBTOTALS
--------------------------------------------------------

  CREATE OR REPLACE VIEW "ORDER_SUBTOTALS" ("ORDERID", "SUBTOTAL") AS 
  SELECT Order_Details.OrderID,
          SUM(CAST((Order_Details.UnitPrice * Quantity * (1 - Discount) / 100) AS NUMBER(19,4)) * 100) Subtotal
     FROM Order_Details
     GROUP BY Order_Details.OrderID;
--------------------------------------------------------
--  DDL for View PRODUCTS_ABOVE_AVERAGE_PRICE
--------------------------------------------------------

  CREATE OR REPLACE VIEW "PRODUCTS_ABOVE_AVERAGE_PRICE" ("PRODUCTNAME", "UNITPRICE") AS 
  SELECT ProductName,
          UnitPrice
     FROM Products
      WHERE UnitPrice > ( SELECT AVG(UnitPrice)
                          FROM Products
                          --ORDER BY Products.UnitPrice DESC
  );
--------------------------------------------------------
--  DDL for View PRODUCTS_BY_CATEGORY
--------------------------------------------------------

  CREATE OR REPLACE VIEW "PRODUCTS_BY_CATEGORY" ("CATEGORYNAME", "PRODUCTNAME", "QUANTITYPERUNIT", "UNITSINSTOCK", "DISCONTINUED") AS 
  SELECT Categories.CategoryName,
          Products.ProductName,
          Products.QuantityPerUnit,
          Products.UnitsInStock,
          Products.Discontinued
     FROM Categories
            JOIN Products
             ON Categories.CategoryID = Products.CategoryID
      WHERE Products.Discontinued <> 1
      --ORDER BY Categories.CategoryName, Products.ProductName;
--------------------------------------------------------
--  DDL for View PRODUCT_SALES_FOR_1997
--------------------------------------------------------

  CREATE OR REPLACE VIEW "PRODUCT_SALES_FOR_1997" ("CATEGORYNAME", "PRODUCTNAME", "PRODUCTSALES") AS 
  SELECT Categories.CategoryName,
          Products.ProductName,
          SUM(CAST((Order_Details.UnitPrice * Quantity * (1 - Discount) / 100) AS NUMBER(19,4)) * 100) ProductSales
     FROM ( Categories
            JOIN Products
             ON Categories.CategoryID = Products.CategoryID
             )
            JOIN ( Orders
                   JOIN Order_Details
                    ON Orders.OrderID = Order_Details.OrderID
                    )
             ON Products.ProductID = Order_Details.ProductID
      WHERE ( ( (Orders.ShippedDate) BETWEEN TO_DATE('19970101','YYYYMMDD') AND TO_DATE('19971231','YYYYMMDD') ) )
     GROUP BY Categories.CategoryName,Products.ProductName;
--------------------------------------------------------
--  DDL for View QUARTERLY_ORDERS
--------------------------------------------------------

  CREATE OR REPLACE VIEW "QUARTERLY_ORDERS" ("CUSTOMERID", "COMPANYNAME", "CITY", "COUNTRY") AS 
  SELECT DISTINCT Customers.CustomerID,
                   Customers.CompanyName,
                   Customers.City,
                   Customers.Country
     FROM Customers
            RIGHT JOIN Orders
             ON Customers.CustomerID = Orders.CustomerID
      WHERE Orders.OrderDate BETWEEN TO_DATE('19970101','YYYYMMDD') AND TO_DATE('19971231','YYYYMMDD');
--------------------------------------------------------
--  DDL for View SALES_BY_CATEGORY
--------------------------------------------------------

  CREATE OR REPLACE VIEW "SALES_BY_CATEGORY" ("CATEGORYID", "CATEGORYNAME", "PRODUCTNAME", "PRODUCTSALES") AS 
  SELECT Categories.CategoryID,
          Categories.CategoryName,
          Products.ProductName,
          SUM(Order_Details_Extended.ExtendedPrice) ProductSales
     FROM Categories
            JOIN ( Products
                   JOIN ( Orders
                          JOIN Order_Details_Extended
                           ON Orders.OrderID = Order_Details_Extended.OrderID
                           )
                    ON Products.ProductID = Order_Details_Extended.ProductID
                    )
             ON Categories.CategoryID = Products.CategoryID
      WHERE Orders.OrderDate BETWEEN TO_DATE('19970101','YYYYMMDD') AND TO_DATE('19971231','YYYYMMDD')
     GROUP BY Categories.CategoryID,Categories.CategoryName,Products.ProductName
     --ORDER BY Products.ProductName;
--------------------------------------------------------
--  DDL for View SALES_TOTALS_BY_AMOUNT
--------------------------------------------------------

  CREATE OR REPLACE VIEW "SALES_TOTALS_BY_AMOUNT" ("SALEAMOUNT", "ORDERID", "COMPANYNAME", "SHIPPEDDATE") AS 
  SELECT Order_Subtotals.Subtotal SaleAmount,
          Orders.OrderID,
          Customers.CompanyName,
          Orders.ShippedDate
     FROM Customers
            JOIN ( Orders
                   JOIN Order_Subtotals
                    ON Orders.OrderID = Order_Subtotals.OrderID
                    )
             ON Customers.CustomerID = Orders.CustomerID
      WHERE ( Order_Subtotals.Subtotal > 2500 )
              AND ( Orders.ShippedDate BETWEEN TO_DATE('19970101','YYYYMMDD') AND TO_DATE('19971231','YYYYMMDD') );
--------------------------------------------------------
--  DDL for View SUMMARY_OF_SALES_BY_QUARTER
--------------------------------------------------------

  CREATE OR REPLACE VIEW "SUMMARY_OF_SALES_BY_QUARTER" ("SHIPPEDDATE", "ORDERID", "SUBTOTAL") AS 
  SELECT Orders.ShippedDate,
          Orders.OrderID,
          Order_Subtotals.Subtotal
     FROM Orders
            JOIN Order_Subtotals
             ON Orders.OrderID = Order_Subtotals.OrderID
      WHERE Orders.ShippedDate IS NOT NULL
      --ORDER BY Orders.ShippedDate;
--------------------------------------------------------
--  DDL for View SUMMARY_OF_SALES_BY_YEAR
--------------------------------------------------------

  CREATE OR REPLACE VIEW "SUMMARY_OF_SALES_BY_YEAR" ("SHIPPEDDATE", "ORDERID", "SUBTOTAL") AS 
  SELECT Orders.ShippedDate,
          Orders.OrderID,
          Order_Subtotals.Subtotal
     FROM Orders
            JOIN Order_Subtotals
             ON Orders.OrderID = Order_Subtotals.OrderID
      WHERE Orders.ShippedDate IS NOT NULL
      --ORDER BY Orders.ShippedDate;
--------------------------------------------------------
--  DDL for Procedure CLOBTOBLOB_SQLDEVELOPER
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "CLOBTOBLOB_SQLDEVELOPER" 
  ( 
    tableNameL      VARCHAR2 , 
    clobColumnNameL VARCHAR2, 
    blobColumnNameL VARCHAR2 ) 
AS 
  tableName      VARCHAR2 ( 500 ) := '';--to_UPPER(tableNameL); 
  clobColumnName VARCHAR2 ( 500 ) := '';--to_UPPER(clobColumNameL); 
  blobColumnName VARCHAR2 ( 500 ) := '';--to_UPPER(blobColumNameL); 
  tmpString      VARCHAR2 ( 500 ) := ''; 
  errorOut       BOOLEAN          := false; 
  inputLength    NUMBER; -- size of input CLOB 
  offSet         NUMBER := 1; 
  pieceMaxSize   NUMBER := 500;          -- the max size of each peice 
  piece          VARCHAR2 ( 500 CHAR ) ; -- these pieces will make up the entire CLOB 
  currentPlace   NUMBER := 1;            -- this is where were up to in the CLOB 
  blobLoc BLOB;                          -- blob locator in the table 
  clobLoc CLOB;                          -- clob locator pointsthis is the value from the dat file 
  myquery VARCHAR2 ( 2000 ) ; 
  -- THIS HAS TO BE CHANGED FOR SPECIFIC CUSTOMER TABLE 
  -- AND COLUMN NAMES 
  --CURSOR cur; 
TYPE cur_typ 
IS 
  REF 
  CURSOR; 
    cur cur_typ; 
    --cur_rec cur%ROWTYPE; 
  BEGIN 
    tableName      := UPPER ( tableNameL ) ; 
    clobColumnName := UPPER ( clobColumnNameL ) ; 
    blobColumnName := UPPER ( blobColumnNameL ) ; 
    BEGIN 
      EXECUTE immediate 'select table_name from user_tables where table_name = :1 ' INTO tmpString USING tableName; 
      IF ( tmpString != tableName ) THEN 
        errorOut     := true; 
      ELSE 
        BEGIN 
          EXECUTE immediate 'select COLUMN_NAME from user_tab_columns where table_name = :1 and COLUMN_NAME = :2 ' INTO tmpString USING tableName, clobColumnName; 
          IF ( tmpString != clobColumnName ) THEN 
            errorOut     := true; 
          ELSE 
            EXECUTE immediate 'select COLUMN_NAME from user_tab_columns where table_name = :1 and COLUMN_NAME = :2 ' INTO tmpString USING tableName, blobColumnName; 
            IF ( tmpString != blobColumnName ) THEN 
              errorOut     := true; 
            END IF; 
          END IF; 
        END; 
      END IF; 
    EXCEPTION 
    WHEN OTHERS THEN 
      errorOut := true; 
    END; 
    IF ( errorOut = true ) THEN 
      raise_application_error ( -20001, 'Invalid parameters' ) ; 
    END IF; 
    EXECUTE immediate 'update ' || tableName || ' set ' || blobColumnName || '= empty_blob() ' ; 
    myquery := 'SELECT '||clobColumnName||' clob_column , '||blobColumnName||' blob_column FROM ' || tableName || ' FOR UPDATE'; 
    OPEN cur FOR myquery;-- using clobColumName, blobColumnName ; 
    FETCH cur 
       INTO clobLoc, 
      blobLoc ; 
   
  WHILE cur%FOUND 
  LOOP 
    --RETRIVE THE clobLoc and blobLoc 
    --clobLoc := cur_rec.clob_column; 
    --blobLoc := cur_rec.blob_column; 
    currentPlace := 1; -- reset evertime 
    -- find the lenght of the clob 
    inputLength := DBMS_LOB.getLength ( clobLoc ) ; 
    -- loop through each peice 
    LOOP 
      IF (inputLength > 0) 
      THEN 
      -- get the next piece and add it to the clob 
      piece := DBMS_LOB.subStr ( clobLoc,pieceMaxSize,currentPlace ) ; 
      -- append this piece to the BLOB 
      DBMS_LOB.WRITEAPPEND ( blobLoc, LENGTH ( piece ) /2, HEXTORAW ( piece ) ) ; 
      currentPlace := currentPlace                     + pieceMaxSize ; 
      END IF; 
      EXIT 
    WHEN inputLength < currentplace; 
    END LOOP; 
    FETCH cur 
       INTO clobLoc, 
      blobLoc ; 
  END LOOP; 
  EXECUTE immediate 'alter table ' || tableName || ' drop column ' || clobColumnName; 
  --unnecessary after ddl 
  COMMIT; 
END CLOBtoBLOB_sqldeveloper;

/

--------------------------------------------------------
--  DDL for Procedure CUSTORDERHIST
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "CUSTORDERHIST" 
(
  v_CustomerID IN NCHAR DEFAULT NULL ,
  cv_1 IN OUT SYS_REFCURSOR
)
AS
BEGIN

   OPEN cv_1 FOR
      SELECT P.ProductName,
             SUM(Quantity) Total
        FROM Products P,
             Order_Details OD,
             Orders O,
             Customers C
         WHERE C.CustomerID = v_CustomerID
                 AND C.CustomerID = O.CustomerID
                 AND O.OrderID = OD.OrderID
                 AND OD.ProductID = P.ProductID
        GROUP BY P.ProductName;

END;

/

--------------------------------------------------------
--  DDL for Procedure CUSTORDERSDETAIL
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "CUSTORDERSDETAIL" 
(
  v_OrderID IN NUMBER DEFAULT NULL ,
  cv_1 IN OUT SYS_REFCURSOR
)
AS
BEGIN

   OPEN cv_1 FOR
      SELECT P.ProductName,
             round(Od.UnitPrice, 2) UnitPrice,
             Quantity,
             CAST(Discount * 100 AS NUMBER(10,0)) Discount,
             round(CAST(Quantity * (1 - Discount) * Od.UnitPrice AS NUMBER(19,4)), 2) ExtendedPrice
        FROM Products P,
             Order_Details Od
         WHERE Od.ProductID = P.ProductID
                 AND Od.OrderID = v_OrderID;

END custordersdetail;

/

--------------------------------------------------------
--  DDL for Procedure CUSTORDERSORDERS
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "CUSTORDERSORDERS" 
(
  v_CustomerID IN NCHAR DEFAULT NULL ,
  cv_1 IN OUT SYS_REFCURSOR
)
AS
BEGIN

   OPEN cv_1 FOR
      SELECT OrderID,
             OrderDate,
             RequiredDate,
             ShippedDate
        FROM Orders
         WHERE CustomerID = v_CustomerID
        ORDER BY OrderID;

END;

/

--------------------------------------------------------
--  DDL for Procedure EMPLOYEE_SALES_BY_COUNTRY
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "EMPLOYEE_SALES_BY_COUNTRY" 
(
  v_Beginning_Date IN DATE DEFAULT NULL ,
  v_Ending_Date IN DATE DEFAULT NULL ,
  cv_1 IN OUT SYS_REFCURSOR
)
AS
BEGIN

   OPEN cv_1 FOR
      SELECT Employees.Country,
             Employees.LastName,
             Employees.FirstName,
             Orders.ShippedDate,
             Orders.OrderID,
             Order_Subtotals.Subtotal SaleAmount
        FROM Employees
               JOIN ( Orders
                      JOIN Order_Subtotals
                       ON Orders.OrderID = Order_Subtotals.OrderID
                       )
                ON Employees.EmployeeID = Orders.EmployeeID
         WHERE Orders.ShippedDate BETWEEN v_Beginning_Date AND v_Ending_Date;

END;

/

--------------------------------------------------------
--  DDL for Procedure SALESBYCATEGORY
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "SALESBYCATEGORY" 
(
  v_CategoryName IN NVARCHAR2 DEFAULT NULL ,
  iv_OrdYear IN NVARCHAR2 DEFAULT '1998' ,
  cv_1 IN OUT SYS_REFCURSOR
)
AS
   v_OrdYear NVARCHAR2(4) := iv_OrdYear;
BEGIN

   IF v_OrdYear != '1996'
     AND v_OrdYear != TO_DATE('1997','YYYYMMDD')
     AND v_OrdYear != '1998' THEN
   BEGIN
      v_OrdYear := '1998';

   END;
   END IF;

   OPEN cv_1 FOR
      SELECT P.ProductName,
             round(SUM(CAST(OD.Quantity * (1 - OD.Discount) * OD.UnitPrice AS NUMBER(14,2))), 0) TotalPurchase
        FROM Order_Details OD,
             Orders O,
             Products P,
             Categories C
         WHERE OD.OrderID = O.OrderID
                 AND OD.ProductID = P.ProductID
                 AND P.CategoryID = C.CategoryID
                 AND C.CategoryName = v_CategoryName
                 AND to_char(O.OrderDate, 'YYYY') = v_OrdYear
        GROUP BY P.ProductName
        ORDER BY P.ProductName;

END salesbycategory;

/

--------------------------------------------------------
--  DDL for Procedure SALES_BY_YEAR
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "SALES_BY_YEAR" 
(
  v_Beginning_Date IN DATE DEFAULT NULL ,
  v_Ending_Date IN DATE DEFAULT NULL ,
  cv_1 IN OUT SYS_REFCURSOR
)
AS
BEGIN

   OPEN cv_1 FOR
      SELECT Orders.ShippedDate,
             Orders.OrderID,
             Order_Subtotals.Subtotal,
             to_char(ShippedDate, 'YYYY') YEAR
        FROM Orders
               JOIN Order_Subtotals
                ON Orders.OrderID = Order_Subtotals.OrderID
         WHERE Orders.ShippedDate BETWEEN v_Beginning_Date AND v_Ending_Date;

END sales_by_year;

/

--------------------------------------------------------
--  DDL for Procedure TEN_MOST_EXPENSIVE_PRODUCTS
--------------------------------------------------------
set define off;

  CREATE OR REPLACE PROCEDURE "TEN_MOST_EXPENSIVE_PRODUCTS" 
(
  cv_1 IN OUT SYS_REFCURSOR
)
AS
BEGIN

   OPEN cv_1 FOR
      SELECT *
        FROM ( SELECT ProductName TenMostExpensiveProducts,
                      UnitPrice
        FROM Products
        ORDER BY UnitPrice DESC )
        WHERE ROWNUM <= 10;

END;

/

exit