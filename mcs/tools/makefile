ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.2914/csc.exe
CSCFLAGS=/nologo /debug+ /debug:full 

windows: verifier.exe corcompare.exe

linux: verifier.exe corcompare.exe

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs

corcompare.exe: CorCompare.cs
	$(CSC) $(CSCFLAGS) CorCompare.cs
	./CorCompare.exe ../class/lib/corlib_cmp.dll > ../../mono/status/cormissing.xml

clean:
	if test -e verifier.exe; then \
		rm verifier.exe;  \
	fi; \
		\
	if test -e verifier.pdb; then \
		rm verifier.pdb;  \
	fi

	if test -e corcompare.exe; then \
		rm corcompare.exe;  \
	fi; \
		\
	if test -e corcompare.pdb; then \
		rm corcompare.pdb;  \
	fi

	if test -e ../../mono/status/cormissing.xml; then \
		rm ../../mono/status/cormissing.xml;  \
	fi; \
