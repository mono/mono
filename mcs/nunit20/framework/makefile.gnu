#
# Makefile for NUnit.Framework.dll
#
# Author:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#

all:
	mcs *.cs /resource:Transform.resources,NUnit.Framework.Transform.resources /target:library /out:../NUnit.Framework.dll

clean:
	rm -f ../NUnit.Framework.dll
