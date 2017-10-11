all: check

.PHONY: check
check: Harness.exe test_all.sh cert_lock
	printf "Running Tls Compatibiliy Check\n\n"
	bash ./test_all.sh 8080 5

Harness.exe: Harness.cs
	mcs -debug Harness.cs

.PHONY: clean
clean:
	-rm -f *.exe
	-rm -f *.pem
	-rm -f *.key
	-rm -f *.pfx
	-rm -f *.mdb
	-rm -f *.crt
	-rm -f *.csr
	-rm -f *.srl
	-rm -f cert_lock

cert_lock: 
	-rm -rf junkca
	bash ./junkca.sh
	touch cert_lock

