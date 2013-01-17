SOURCES= \
	DnsClass.cs \
	DnsHeader.cs \
	DnsPacket.cs \
	DnsQClass.cs \
	DnsQType.cs \
	DnsQuery.cs \
	DnsQuestion.cs \
	DnsResourceRecordA.cs \
	DnsResourceRecordCName.cs \
	DnsResourceRecordPTR.cs \
	DnsResourceRecord.cs \
	DnsResponse.cs \
	DnsType.cs \
	ResolverAsyncOperation.cs \
	Resolver.cs \
	ResolverError.cs \
	SimpleResolverEventArgs.cs

EXES = resolver.exe \
	plainolddns.exe

all: $(EXES)

resolver.exe: $(SOURCES)
	@gmcs -out:$@ -debug $(SOURCES)

plainolddns.exe: plainolddns.cs
	@gmcs -out:$@ -debug $^
clean:
	rm -f resolver.exe* plainolddns.exe* *.mdb
