#
# Makefile for NUnit.Util.dll
#
# Author:
#   Jackson Harper (Jackson@LatitudeGeo.com)
#

all:
	mcs /target:library /r:../NUnit.Framework.dll /out:../NUnit.Util.dll CommandLineOptions.cs ConsoleOptions.cs

clean:
	rm -f ../NUnit.Util.dll