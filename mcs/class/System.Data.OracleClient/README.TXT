
README.TXT for Mono Oracle 8i data provider

Located at
mcs/class/System.Data.OracleClient

Requirements:

1. C#/CLR compiler. (only tried with Visual Studio.NET)
   to build the OracleClient assembly.

2. C compiler (only tried with the command-line c compilers
   Borland C++ 5.5 and Visual C++ 7.0) that can build
   the OracleClient glue shared library. 
   
   A free command-line
   version of the Borland C++ 5.5 compiler
   for Win32 can be downloaded
   from http://www.borland.com/

3. Oracle 8i client software via either of the following:
   a. Oracle 8i client with access to an Oracle 8i database
   b. Oracle 8i server
   
   Either one, the client software needs to include the
   oci shared library.  This is oci.dll on Win32.
   Oracle 8i can be downloaded from
   http://www.oracle.com/

4. GLIB 2.0.  You should already have this installed since
   Mono requires it.  It can be downloaded from
   http://www.gtk.org/
   or for Win32, it can be downloaded from
   http://www.gimp.org/win32

Notes:

I seriously doubt this will build within the
mcs/class/System.Data.OracleClient directory 
structure.  Maybe, moving the project files to
a simpler directory structure may help.

There are two parts:

1. assembly System.Data.OracleClient.dll which
   includes files OracleCommand.cs and OracleConnection.cs

2. unmanaged System.Data.OracleClient.ociglue.dll which
   must be built with a compiler that can link with the
   oci shared library.  On Windows, 
   the Oracle shared library oci.dll
   This shared library provides the glue between
   System.Data.OracleClient.dll and oci.dll
   
How to build the various parts:

1. System.Data.OracleClient.dll is built 
   using Visual Studio.NET 1.0 via its projects files
   I have not tried building it 
   via only the .NET Framework 1.0 command-line tools.

2. System.Data.OracleClient.ociglue.dll is built by the
   C compiler that can build a native c shared library
   that can link with the Oracle 8i oci shared libary and
   the GLIB 2.0 shared library.  A Makefile for
   command-line compilers for Borland C++ 5.5 
   and Visual C++ 7.0  I have only tried building this
   on Windows XP.
   
TODO:

1. get the OracleClient assembly to run on mono
2. get the OracleClient glue lib to build with mcs on mono
3. get the OracleClient to build with gcc
4. get the OracleClient assembly and glub lib to 
   build with mcs on mono and run on mono on GNU/Linux
5. Add transactions
6. Add a data reader and retrieve simple data types
7. Add parameters (binding variables)
8. Add a data adapter
9. add connection pooling support
10. add more complex types
11. add support for stored procedures, functions, and packages
12. add support for security permissions
13. add ability to execute queries in SQL# CLI
14. add ability to execute queries in SQL# For GTK#

