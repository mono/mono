#
# Makefile for NUnit.Framework.dll
#
# Author:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#

MCS = mcs
MCS_FLAGS =

all: NUnit.Framework.dll

NUnit.Framework.dll: *.cs
	$(MCS) $(MCS_FLAGS) *.cs /resource:Transform.resources,NUnit.Framework.Transform.resources /target:library /out:NUnit.Framework.dll
	cp NUnit.Framework.dll ..

clean:
	rm -f ../NUnit.Framework.dll NUnit.Framework.dll
