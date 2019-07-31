MCS=mcs -g
MONO=MONO_PATH=../../../lib/net_4_x mono
sourcefile = proxies.sources

tools: wstest.exe

proxies: proxies.dll

wstest.exe: wstest.cs
	$(MCS) wstest.cs /r:System.Web.Services.dll

clean:
	rm -f *.dll *.exe *~ *.pdb
	rm -rf proxies
	rm -rf wsdlcache
	rm error/*

proxies.dll:
	$(MCS) /r:System.Web.Services.dll /r:System.Data /target:library -out:proxies.dll @proxies.sources

tests: proxies
	$(MCS) /r:proxies.dll /r:System.Web.Services.dll /r:nunit.framework.dll /r:System.Data /target:library -out:System.Web.Services-standalone_test.dll @client.sources WebServiceTest.cs

update:
	$(MONO) --debug wstest.exe ur http://www.xmethods.net/default.disco wsdl.ignore
	$(MONO) --debug wstest.exe ur http://localhost:8080/xsp.vsdisco
	$(MONO) --debug wstest.exe dw
	rm -f wsdlfiles.tar.gz
	tar --gzip -cf wsdlfiles.tar.gz wsdl

gen:
	$(MONO) --debug wstest.exe gp
	$(MONO) --debug wstest.exe gc

regen:
	$(MONO) --debug wstest.exe gp all

cls:
	$(MONO) --debug wstest.exe clean

pack:
	tar --gzip -cf wsdlfiles.tar.gz wsdlcache
	gzip proxies.net.xml

winbase:
	csc wstest.cs
	rm -rf wsdl
	rm -rf proxies
	rm -f proxies.dll
	tar xvfz wsdlfiles.tar.gz
	wstest.exe gp
	csc /r:System.Web.Services.dll /r:System.Data /target:library -out:proxies.dll @proxies.sources
	$(MONO) ../../../../tools/corcompare/mono-api-info.exe proxies.net.dll > proxies.net.xml

test-wsdl: tools
	rm -rf error/*
	rm -rf wsdlcache
	rm -rf proxies
	rm -f proxies.dll
	tar xvfz wsdlfiles.tar.gz
	gunzip -c proxies.net.xml.gz > proxies.net.xml
	$(MONO) wstest.exe gp
	$(MONO) wstest.exe gc
	mcs /r:System.Web.Services.dll /r:System.Data /target:library -out:proxies.dll @proxies.sources
	$(MONO) ../../../../tools/corcompare/mono-api-info.exe proxies.dll > proxies.mono.xml
	$(MONO) wstest.exe msfix proxies.net.xml
	$(MONO) ../../../../tools/mono-api-diff/mono-api-diff.exe proxies.net.xml proxies.mono.xml > proxies.diff.xml
	$(MONO) wstest.exe checkdiff proxies.diff.xml

test-clients: tests
	rm -f somefile && touch somefile
	(tail -f somefile | xsp --root server)&
	sleep 2
	-harness=../../../lib/net_4_x/nunit-console.exe; \
	if test -f $$harness; then :; else harness=../../../lib/nunit-console.exe; fi; \
	$(MONO) $$harness System.Web.Services-standalone_test.dll
	echo "" >> somefile

test: test-wsdl test-clients

respage:
	$(MONO) transform.exe proxies.diff.xml ../../../../tools/corcompare/mono-api.xsl > proxies.data.html
	cat header.html proxies.data.html > proxies.html
	rm proxies.data.html

genfiles:
	$(MONO) wstest.exe genfiles templates.xml
