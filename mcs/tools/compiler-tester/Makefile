thisdir = tools/compiler-tester
SUBDIRS = 
include ../../build/rules.make

all-local: compiler-tester-$(PROFILE).exe

clean-local:
	rm -f *.exe *.mdb

compiler-tester-$(PROFILE).exe: compiler-tester.cs
	$(CSCOMPILE) /out:$@ compiler-tester.cs xmldocdiff.cs
	