#
# Makefile for nunit-console.exe
#
# Author:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#

all:
	mcs *.cs /r:../NUnit.Framework.dll /r:../NUnit.Util.dll /out:../nunit-console.exe

clean:
	rm -f ../nunit-console.exe