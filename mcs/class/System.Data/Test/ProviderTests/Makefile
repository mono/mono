thisdir=class/System.Data/Test/ProviderTests

include ../../../../build/rules.make

LIB_MCS_FLAGS += /r:System.Data,System.Data.OracleClient,System,System.Xml,nunit.framework,System.Configuration

DATABASE=sqlserver
ifndef CONNECTION
ifndef PROVIDER_TESTS_CONNECTION
CONNECTION=sqlserver-tds
export PROVIDER_TESTS_CONNECTION=$(CONNECTION)
endif
else
export PROVIDER_TESTS_CONNECTION=$(CONNECTION)
endif

LIBRARY = System.Data.Connected.Tests.dll
NO_INSTALL=yes
NO_SIGN_ASSEMBLY=yes

${LIBRARY}: clean-local
	cp app-$(PROFILE).config $(LIBRARY).config
	$(CSCOMPILE) $(LIB_MCS_FLAGS) /target:library /out:$@ /debug @$(LIBRARY).sources

all-local: ${LIBRARY}

run-test-local: run-test-db
run-test-db: ${LIBRARY} 
ifdef OSQLCMD
	$(OSQLCMD) sql/$(DATABASE).sql
endif
	ok=:; \
	$(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(TEST_HARNESS) $(TEST_HARNESS_FLAGS) $(LOCAL_TEST_HARNESS_FLAGS) -output:TestResult-$(PROFILE).log -include=$(DATABASE) -xml:TestResult-$(PROFILE).xml $< || ok=false; \
	sed '1,/^Tests run: /d' TestResult-$(PROFILE).log; \
	$$ok

test-local: run-test-db

CLEAN_FILES = *.exe *.dll *.log ${LIBRARY}.config *.mdb TestResult*.xml

clean-local:
	-rm -f $(CLEAN_FILES) || :
