ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.2914/csc.exe
CSCFLAGS=/nologo /debug+ /debug:full 

windows: verifier.exe CorCompare.exe EnumCheck.exe

linux: verifier.exe CorCompare.exe EnumCheck.exe

verifier.exe: verifier.cs
	$(CSC) $(CSCFLAGS) verifier.cs

CorCompare.exe: CorCompare.cs
	$(CSC) $(CSCFLAGS) CorCompare.cs
	./CorCompare.exe ../class/lib/corlib_cmp.dll > ../../mono/status/cormissing.xml

EnumCheck.exe: EnumCheck.cs
	$(CSC) $(CSCFLAGS) /out:EnumCheck.exe EnumCheck.cs

clean:
	if test -e verifier.exe; then \
		rm verifier.exe;  \
	fi; \
		\
	if test -e verifier.pdb; then \
		rm verifier.pdb;  \
	fi

	if test -e CorCompare.exe; then \
		rm CorCompare.exe;  \
	fi; \
		\
	if test -e CorCompare.pdb; then \
		rm CorCompare.pdb;  \
	fi

	if test -e ../../mono/status/cormissing.xml; then \
		rm ../../mono/status/cormissing.xml;  \
	fi; \

	rm -f EnumCheck.exe EnumCheck.pdb

# I don't understand the need for testing for existence of pdb/exe files
#               -- Gaurav Vaish
