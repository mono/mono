thisdir=class/System.Data/Test/DataProviderTests/datareadertests

include ../../../../../build/rules.make

LIB_MCS_FLAGS = /r:System.Data,System.Data.OracleClient,ByteFX.Data,Npgsql,System,System.Xml

DATABASE=all

LIBRARY = DbRetrieve.dll
NO_INSTALL=yes
NO_SIGN_ASSEMBLY=yes

include $(topdir)/build/library.make

Runtests.exe: $(the_lib)
	$(MCS) /r:$(the_lib) Runtests.cs

DbRetrieve.dll: 
	$(MCS) /r:$(REFERENCES) /target:library /out:DbRetrieve.dll @$(SOURCEFILE)

run-test-local: run-test-db
run-test-db: Runtests.exe
	$(TEST_RUNTIME) Runtests.exe $(DATABASE) | tee TestResults.log

test-local: Runtests.exe

CLEAN_FILES = *.exe *.dll *.log
