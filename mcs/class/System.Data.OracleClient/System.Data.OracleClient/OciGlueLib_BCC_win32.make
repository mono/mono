#
# Makefile for System.Data.OracleClient.gluelib.dll
# using Borland C++
#
# This Makefile untested for Borland C++
# a free Borland C++ 5.5 command-line 
# compiler can be downloaded
# from http://www.borland.com/
#

all: ociglue.dll

ociglue.dll: ociglue.c ociglue.h
	set BINC=%BORLAND_HOME%\include 
	set BLIB=%BORLAND_HOME%\lib
	%BORLAND_HOME%\bin\bcc32 -w-pro -c -a4 -DOCI_BORLAND -I. -I%BINC% -I..\include ociglue.c -I..\..\cygwin\home\DanielMorgan\mono\install\include\glib-2.0 -I..\..\cygwin\home\DanielMorgan\mono\install\lib\glib-2.0\include
	echo LIBRARY ociglue.dll > ociglue.def
	echo DESCRIPTION 'ociglue.dll' >> ociglue.def
	echo EXPORTS >> ociglue.def
	echo _qxiqtbi=qxiqtbi >> ociglue.def
	%BORLAND_HOME%\bin\bcc32 -tWD -L%BLIB% -L..\lib\bc ociglue.obj oci.lib bidsfi.lib glib-2.0.lib intl.lib iconv.lib
	:end
