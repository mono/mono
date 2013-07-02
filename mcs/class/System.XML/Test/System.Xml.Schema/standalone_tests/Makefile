PROFILE = net_4_5
RUNTIME = MONO_PATH=../../../../lib/$(PROFILE) mono --debug
MCS = mcs
TESTS = xsd-test-suite/suntest/SunTestsAll/xsiType1.xsd
MASTERS = xsd-test-suite/suntest/tests-all.xml
TEST_ARCHIVE = XSTC-20020116.tar.gz
MASTER_ARCHIVE = test-masters.tar.gz
MCS_OPTS += -d:NET_2_0
TEST_RUNNER = xsdtest_$(PROFILE).exe

$(TEST_RUNNER) : xsdtest.cs $(TESTS)
	$(MCS) xsdtest.cs $(MCS_OPTS) -out:$(TEST_RUNNER)

xs-psci-compare.exe : xs-psci-compare.cs
	$(MCS) xs-psci-compare.cs

$(MASTERS) : $(MASTER_ARCHIVE)
	mkdir xsd-test-suite 2>/dev/null; true
	cd xsd-test-suite && tar zxvf ../$(MASTER_ARCHIVE)
	touch $(MASTERS)

$(TESTS) : $(TEST_ARCHIVE) $(MASTERS) 
	mkdir xsd-test-suite 2>/dev/null; true
	cd xsd-test-suite && tar zxvf ../$(TEST_ARCHIVE)
	touch $(TESTS)

$(TEST_ARCHIVE) :
	wget http://www.w3.org/2001/05/xmlschema-test-collection/XSTC-20020116.tar.gz

run-test : $(TEST_RUNNER)
	$(RUNTIME) $(TEST_RUNNER) --v2 --xml --reportsuccess --testall --details --report:TestResult.xml $(XSD_TEST_FLAGS)
	@echo "SUCCESS: `grep -c \"OK\" TestResult.xml` NORMAL FAILURE: `grep -c should TestResult.xml` UNEXPECTED `grep -c unexpected TestResult.xml`"

test-psci : xs-psci-compare.exe
	$(RUNTIME) xs-psci-compare.exe xsd-test-suite/msxsdtest/tests-all.xml > psci-mono-msxsd.txt

clean:
	rm $(TEST_RUNNER)
	
# be careful to use it. This removes ALL files in xsd-test-suite!
clean-complete:
	rm $(TEST_RUNNER)
	rm -rf xsd-test-suite
