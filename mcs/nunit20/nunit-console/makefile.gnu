#
# Makefile for nunit-console.exe
#
# Author:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#

all: nunit-console.exe

nunit-console.exe: *.cs
	mcs *.cs /r:../NUnit.Framework.dll /r:../NUnit.Util.dll /out:nunit-console.exe
	cp nunit-console.exe ..
clean:
	rm -f ../nunit-console.exe nunit-console.exe
