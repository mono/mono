RUNTIME = mono
MCS_RUNTIME =
MCS = mcs
TESTS = xml-test-suite/xmlconf/xmltest/valid/sa/119.xml
TEST_ARCHIVE = xmlts20031210.zip
DUMP_RESULTS_ARCHIVE = dump-results.zip

xmltest.exe : xmltest.cs $(TESTS)
	$(MCS_RUNTIME) $(MCS) xmltest.cs
	$(MCS_RUNTIME) $(MCS) xrdump.cs
	$(MCS_RUNTIME) $(MCS) domdump.cs
	$(MCS_RUNTIME) $(MCS) eventdump.cs

$(TESTS) : $(TEST_ARCHIVE) xml-test-suite
	cd xml-test-suite; unzip -n ../$(TEST_ARCHIVE); cd ..

$(TEST_ARCHIVE) :
	wget http://www.w3.org/XML/Test/xmlts20031210.zip

xml-test-suite:
	mkdir xml-test-suite

test :
	$(RUNTIME) xmltest.exe

xrtest : xmlreader-result-ms.txt
	$(RUNTIME) xrdump.exe > xmlreader-result-mono.txt
	diff xmlreader-result-ms.txt xmlreader-result-mono.txt

domtest : dom-result-ms.txt
	$(RUNTIME) domdump.exe > dom-result-mono.txt
	diff dom-result-ms.txt dom-result-mono.txt

eventtest : event-result-ms.txt
	$(RUNTIME) eventdump.exe > event-result-mono.txt
	diff event-result-ms.txt event-result-mono.txt

xmlreader-result-ms.txt dom-result-ms.txt event-result-ms.txt :
	unzip -n $(DUMP_RESULTS_ARCHIVE)

# be careful to use it. This removes ALL files in xml-test-suite!
# clean:
#	rm -rf xml-test-suite
