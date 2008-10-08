PROFILE = net_1_1
RUNTIME = MONO_PATH=../../../lib/$(PROFILE) mono --debug 
#XMLDSIG_EXE_OPTIONS =
XMLDSIG_EXE_OPTIONS = --decent-reader

ifeq ($(PROFILE), net_2_0)
CSCOMPILE = gmcs --debug -d:NET_2_0
else
CSCOMPILE = mcs --debug
endif

run-test: c14n.exe xmldsig.exe merlin-xmldsig-twenty-three
	$(RUNTIME) $(RUNTIME_FLAGS) c14n.exe merlin-xmldsig-twenty-three/signature-enveloped-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloped-dsa-c14n-1.txt
	$(RUNTIME) $(RUNTIME_FLAGS) c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-b64-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-b64-dsa-c14n-0.txt
	$(RUNTIME) $(RUNTIME_FLAGS) c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-dsa-c14n-1.txt
	$(RUNTIME) $(RUNTIME_FLAGS) c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-hmac-sha1-40.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-hmac-sha1-40-c14n-1.txt
	$(RUNTIME) $(RUNTIME_FLAGS) c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-hmac-sha1.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-hmac-sha1-c14n-1.txt
	$(RUNTIME) $(RUNTIME_FLAGS) c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-rsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-rsa-c14n-1.txt
	$(RUNTIME) $(RUNTIME_FLAGS) c14n.exe merlin-xmldsig-twenty-three/signature-external-b64-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-external-b64-dsa-c14n-0.txt
	$(RUNTIME) $(RUNTIME_FLAGS) c14n.exe merlin-xmldsig-twenty-three/signature-external-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-external-dsa-c14n-0.txt
	$(RUNTIME) $(RUNTIME_FLAGS) xmldsig.exe $(XMLDSIG_EXE_OPTIONS)

ms-test: c14n.exe xmldsig.exe merlin-xmldsig-twenty-three
	cp ../../../lib/$(PROFILE)/Mono.Security.dll .
	./c14n.exe merlin-xmldsig-twenty-three/signature-enveloped-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloped-dsa-c14n-1.txt
	./c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-b64-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-b64-dsa-c14n-0.txt
	./c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-dsa-c14n-1.txt
	./c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-hmac-sha1-40.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-hmac-sha1-40-c14n-1.txt
	./c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-hmac-sha1.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-hmac-sha1-c14n-1.txt
	./c14n.exe merlin-xmldsig-twenty-three/signature-enveloping-rsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-enveloping-rsa-c14n-1.txt
	./c14n.exe merlin-xmldsig-twenty-three/signature-external-b64-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-external-b64-dsa-c14n-0.txt
	./c14n.exe merlin-xmldsig-twenty-three/signature-external-dsa.xml SignedInfo | cmp merlin-xmldsig-twenty-three/signature-external-dsa-c14n-0.txt
	./xmldsig.exe $(XMLDSIG_EXE_OPTIONS)
	rm Mono.Security.dll

clean:
	rm -f *.exe *.exe.mdb

c14n.exe : c14n.cs
	$(CSCOMPILE) c14n.cs -r:System.Security.dll

xmldsig.exe: xmldsig.cs
	$(CSCOMPILE) xmldsig.cs -r:System.Security.dll -r:Mono.Security.dll

phaos-xmldsig-three: phaos-xmldsig-three.zip
	unzip -u phaos-xmldsig-three.zip

phaos-xmldsig-three.zip:
	wget http://lists.w3.org/Archives/Public/w3c-ietf-xmldsig/2003JulSep/att-0018/phaos-xmldsig-three.zip

merlin-xmldsig-twenty-three: 01-merlin-xmldsig-twenty-three.tar.gz
	tar -zxf 01-merlin-xmldsig-twenty-three.tar.gz

01-merlin-xmldsig-twenty-three.tar.gz:
	wget http://lists.w3.org/Archives/Public/w3c-ietf-xmldsig/2002AprJun/att-0016/01-merlin-xmldsig-twenty-three.tar.gz

