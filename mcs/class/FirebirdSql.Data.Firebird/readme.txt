Firebird ADO.NET Data provider for .NET and Mono 
================================================

This project is supported by:
---- ------- -- --------- ---

	Sean Leyne ( Broadview Software )


Developement list
-----------------

You can subscribe to the developement list at:

	http://lists.sourceforge.net/lists/listinfo/firebird-net-provider


You can access to the lastest developement sources through CVS, see:

	http://sourceforge.net/cvs/?group_id=9028


Reporting Bugs
--------------

Yo can report bugs using two ways:

1. Sending it to the developement list.
2. If you have a Sourceforge ID you can send it using the Bugs section of the Firebird Project web page 
(category .Net Provider):


	http://sourceforge.net/tracker/?group_id=9028&atid=109028


Requirements for build the sources on Windows
---------------------------------------------

- The Microsoft .NET Framework or Mono:: platform.


Build with Microsoft .NET Framework:

	- You need the Microsoft .NET Platform.

	- The provider sources have a build file for NAnt ( http://nant.sourceforge.net/ ), 
	FirebirdNetProvider.build.

	For build it you only need to exececute nant (0.85) on the same directory as the build file.

	- The Nant build file generates (inside framework version directory net-1.0, net-1.1, ...):

			1.- FirebirdSql.Data.Firebird.dll ( binary of the ADO .NET data provider )
			2.- FirebirdSql.Data.Firebird.UnitTest.dll ( binary of the NUnit tests. )
			3.- MSDN style documentation.


Build with mono:: platform ( www.go-mono.com ):

	- The mono platform with ICU support.

	- The provider sources have a build file, makefile, for build the sources ( this script file needs Cygwin ).

	- The makefile build file generates:

		1.- FirebirdSql.Data.Firebird.dll ( binary of the ado .net provider )

	Note : You can build it using NAnt too, for this you need to modify the NAnt script changing the build 
	file for allow it.



Requirements for build the sources on Linux
-------------------------------------------

Build with mono:: platform ( www.go-mono.com ):

	- The mono platform with ICU support.

	- The provider sources have a build file, makefile, for build the sources, you only need to execute make on the same
	directory as the script.
