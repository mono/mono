#
# Makefile for nunit-console.exe
#
# Author:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#

RUNTIME=mono
MCS = $(RUNTIME) ../../mcs/mcs.exe
MCS_FLAGS =

all: nunit-console.exe

nunit-console.exe: *.cs
	$(MCS) $(MCS_FLAGS) *.cs /r:../NUnit.Framework.dll /r:../NUnit.Util.dll /out:nunit-console.exe
	cp nunit-console.exe ..
clean:
	rm -f ../nunit-console.exe nunit-console.exe
