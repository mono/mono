# 
# ociglue.c - provides glue between 
#             managed C#/.NET System.Data.OracleClient.dll and 
#             unmanaged native c library oci.dll
#             to be used in Mono System.Data.OracleClient as
#             the Oracle 8i data provider.
#
# Builds unmanaged C library ociglue.dll
#
# Author: 
#      Daniel Morgan <danmorg@sc.rr.com>
#       
# Copyright (C) Daniel Morgan, 2002
#
#
# Licensed under the MIT/X11 License.
#

# builds with command-line Microsoft C 7.0 using cl and nmake
# builds using the MSVC OCI import library oci.lib (lib that exports symbols from oci.dll)

# Two environment variables need to be set: ORACLE_HOME and GLIB_PREFIX
# ORACLE_HOME set to directory above where your /oci/lib
# For example:
#       set ORACLE_HOME=F:\oracle\ora81
# Where f:\oracle\ora81 has a subdirectory oci
#
# GLIB_PREFIX set to where GLIB 2.0 prefix is installed 
# For example:
#       set GLIB_PREFIX=F:\cygwin\home\DanielMorgan\mono\install
# Which assumes your glib-2.0 libraries are in $(GLIB_PREFIX)\lib
# and your glib-2.0 include files are in $(GLIB_PREFIX)\include

# GLIB 2.0 for Win32 found at http://www.gimp.org/win32

PROJECT = ociglue.dll

GLIB_CFLAGS = -I$(GLIB_PREFIX)\include\glib-2.0 -I$(GLIB_PREFIX)\lib\glib-2.0\include
GLIB_LIBS = /LIBPATH:$(GLIB_PREFIX)\lib glib-2.0.lib intl.lib iconv.lib

#GLIB_CFLAGS = -IF:\cygwin\home\DanielMorgan\mono\install\include\glib-2.0 -IF:\cygwin\home\DanielMorgan\mono\install\lib\glib-2.0\include
#GLIB_LIBS = /LIBPATH:F:\cygwin\home\DanielMorgan\mono\install\lib glib-2.0.lib intl.lib iconv.lib

# Oracle 8i OCI
ORACLE_CFLAGS = -I%ORACLE_HOME%\oci\include
ORACLE_LIBS = /LIBPATH:%ORACLE_HOME%\oci\lib\msvc oci.lib

OCIGLUELIB_CFLAGS = -I. -D_DLL -D_MT $(ORACLE_CFLAGS) $(GLIB_CFLAGS)
OCIGLUELIB_LIBS = kernel32.lib msvcrt.lib $(ORACLE_LIBS) $(GLIB_LIBS)
OCIGLUELIB_LINKFLAGS = /link /Dll /out:ociglue.dll /nod:libc $(OCIGLUELIB_LIBS) $(OCIGLUELIB_EXPORTS)

OCIGLUELIB_EXPORTS = /export:OciGlue_Connect /export:OciGlue_PrepareAndExecuteNonQuerySimple /export:OciGlue_Disconnect /export:OciGlue_ConnectionCount /export:OciGlue_CheckError /export:Free

SOURCE_H_FILES = ociglue.h
SOURCE_C_FILES = ociglue.c

all: ociglue.dll

$(SOURCE_C_FILES) : $(SOURCE_H_FILES)

ociglue.dll : $(SOURCE_C_FILES)
	cl  $(OCIGLUELIB_CFLAGS) $(SOURCE_C_FILES) $(OCIGLUELIB_LINKFLAGS)
	
clean:
	rm -f ociglue.dll
	rm -f System.Data.OracleClient.ociglue.o
