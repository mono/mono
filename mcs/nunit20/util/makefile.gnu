#
# Makefile for NUnit.Util.dll
#
# Author:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#

all: NUnit.Util.dll

NUnit.Util.dll: *.cs
	mcs /target:library /r:../NUnit.Framework.dll /out:NUnit.Util.dll CommandLineOptions.cs ConsoleOptions.cs
	cp NUnit.Util.dll ..

clean:
	rm -f ../NUnit.Util.dll NUnit.Util.dll
