RUNTIME = mono
MCS_RUNTIME =
MCS = mcs
TESTS = xsd-test-suite/suntest/SunTestsAll/xsiType1.xsd
MASTERS = xsd-test-suite/suntest/tests-all.xml
TEST_ARCHIVE = XSTC-20020116.tar.gz
MASTER_ARCHIVE = test-masters.tar.gz

xsdtest.exe : xsdtest.cs $(TESTS)
	$(MCS_RUNTIME) $(MCS) xsdtest.cs

xs-psci-compare.exe : xs-psci-compare.cs
	$(MCS_RUNTIME) $(MCS) xs-psci-compare.cs

$(MASTERS) : $(MASTER_ARCHIVE) xsd-test-suite
	cd xsd-test-suite; tar zxvf ../$(MASTER_ARCHIVE); cd ..

$(TESTS) : $(TEST_ARCHIVE) $(MASTERS) xsd-test-suite
	cd xsd-test-suite; tar zxvf ../$(TEST_ARCHIVE); cd ..

$(TEST_ARCHIVE) :
	wget http://www.w3.org/2001/05/xmlschema-test-collection/XSTC-20020116.tar.gz

xsd-test-suite:
	mkdir xsd-test-suite

run-test :
	$(RUNTIME) xsdtest.exe --xml --reportsuccess --testall --report:TestResult.xml
	@echo "SUCCESS: `grep -c \"OK\" TestResult.xml` NORMAL FAILURE: `grep -c should TestResult.xml` UNEXPECTED `grep -c unexpected TestResult.xml`"

test-psci : xs-psci-compare.exe
	$(RUNTIME) xs-psci-compare.exe xsd-test-suite/msxsdtest/tests-all.xml > psci-mono-msxsd.txt

# be careful to use it. This removes ALL files in xsd-test-suite!
# clean:
#	rm -rf xsd-test-suite
