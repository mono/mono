MCS=mcs -g
sourcefile = proxies.sources

%.dll: %.cs
	$(MCS) /r:System.Web.dll /target:library $<

all: tools update gen proxies tests

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
	$(MCS) /r:System.Web.Services.dll /target:library -out:proxies.dll @proxies.sources

tests: proxies
	$(MCS) /r:proxies.dll /r:System.Web.Services.dll /r:NUnit.Framework.dll /target:library -out:testclient.dll @client.sources WebServiceTest.cs

update:
	mono --debug wstest.exe ur http://www.xmethods.net/default.disco wsdl.ignore
	mono --debug wstest.exe ur http://localhost:8080/xsp.vsdisco
	mono --debug wstest.exe dw

gen:
	mono --debug wstest.exe gp
	mono --debug wstest.exe gc

regen:
	mono --debug wstest.exe gp all

cls:
	mono --debug wstest.exe clean
