#
# "make" builds tests web application
#
# "make run-xsp" runs xsp with deployed web tests
#

.SUFFIXES: .cs .exe .dll

RUNTIME = mono

nunit_MONO_PATH="../../../../../class/lib/net_4_5"
CSCOMPILE=mcs
XSPSERVER=xsp4

TEST_PROG=bin/MainsoftWebApp.dll

$(TEST_PROG) : 
	$(CSCOMPILE) -target:library -recurse:./*.cs -r:System.Web.dll -r:System.Data.dll -r:System.Drawing.dll -out:$@

run-xsp : $(TEST_PROG)
	$(XSPSERVER) --port 8088 --applications /MainsoftWebApp:./ --nonstop

clean :
	rm -f $(TEST_PROG)

