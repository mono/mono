thisdir=class/System.Data/Test/ProviderTests


include ../../../../build/rules.make

LIB_MCS_FLAGS = /r:System.Data,System.Data.OracleClient,ByteFX.Data,Npgsql,System,System.Xml,nunit.framework,Mono.Data

DATABASE=sql92

LIBRARY = System.Data.Connected.Tests.dll
NO_INSTALL=yes
NO_SIGN_ASSEMBLY=yes


${LIBRARY}:
	$(MCS) $(LIB_MCS_FLAGS) /target:library /out:$@ /debug @$(LIBRARY).sources

run-test-local: run-test-db
run-test-db: ${LIBRARY} 
ifdef OSQLCMD
	$(OSQLCMD) sql/$(DATABASE).sql
endif
	cp app-$(PROFILE).config $(LIBRARY).config
	unlink conn_string || :
	ln -s $(DATABASE).config conn_string
	ok=:; \
	$(TEST_RUNTIME) $(TEST_HARNESS) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) /output:TestResult-$(PROFILE).log /include=$(DATABASE) /xml:TestResult-$(PROFILE).xml $< || ok=false; \
	sed '1,/^Tests run: /d' TestResult-$(PROFILE).log; \
	$$ok


test-local: run-test-db

CLEAN_FILES = *.exe *.dll *.log ${LIBRARY}.config *.mdb TestResult*.xml

clean-local:
	rm $(CLEAN_FILES) || :
