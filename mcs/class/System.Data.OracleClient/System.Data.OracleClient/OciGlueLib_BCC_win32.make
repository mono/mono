#
# Makefile for System.Data.OracleClient.gluelib.dll
# using Borland C++
#
# This Makefile untested for Borland C++
# a free Borland C++ 5.5 command-line 
# compiler can be downloaded
# from http://www.borland.com/
#

all: System.Data.OracleClient.ociglue.dll

System.Data.OracleClient.ociglue.dll: ociglue.c ocglue.h
	set BINC=%BORLAND_HOME%\include 
	set BLIB=%BORLAND_HOME%\lib
	%BORLAND_HOME%\bin\bcc32 -w-pro -c -a4 -DOCI_BORLAND -I. -I%BINC% -I..\include ociglue.c -I..\..\cygwin\home\DanielMorgan\mono\install\include\glib-2.0 -I..\..\cygwin\home\DanielMorgan\mono\install\lib\glib-2.0\include
	echo LIBRARY System.Data.OracleClient.ociglue.dll > System.Data.OracleClient.ociglue.def
	echo DESCRIPTION 'System.Data.OracleClient.ociglue.dll' >> System.Data.OracleClient.ociglue.def
	echo EXPORTS >> System.Data.OracleClient.ociglue.def
	echo _qxiqtbi=qxiqtbi >> System.Data.OracleClient.ociglue.def
	%BORLAND_HOME%\bin\bcc32 -tWD -L%BLIB% -L..\lib\bc System.Data.OracleClient.ociglue.obj oci.lib bidsfi.lib glib-2.0.lib intl.lib iconv.lib
	:end
