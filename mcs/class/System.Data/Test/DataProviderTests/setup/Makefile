thisdir=class/System.Data/Test/DataProviderTests/datareadertests

include ../../../../../build/rules.make

LIB_MCS_FLAGS = /r:System.Data,System.Data.OracleClient,ByteFX.Data,Npgsql,System,System.Xml

DATABASE=all

LIBRARY = Setup.dll
NO_INSTALL=yes
NO_SIGN_ASSEMBLY=yes

include $(topdir)/build/library.make

SetupDb.exe: $(the_lib)
	$(MCS) /r:$(the_lib) SetupDb.cs

Setup.dll: 
	$(MCS) /r:$(REFERENCES) /target:library /out:Setup.dll @$(SOURCEFILE)

run-test-local: run-test-db
run-test-db: SetupDb.exe
	$(TEST_RUNTIME) SetupDb.exe $(DATABASE) | tee TestResults.log

test-local: SetupDb.exe

CLEAN_FILES = *.exe *.dll *.log
