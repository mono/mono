TESTS = t1 t2 t3 t4 t5 t6 t7
TESTS2 = c1 c2 c3
#SERVER=quack
SERVER=lalo-office.boston.ximian.com

all: rtest.exe ctest.exe test

rtest.exe: rtest.cs
	mcs rtest.cs

ctest.exe: ctest.cs
	mcs ctest.cs

copy:
	for i in $(TESTS) $(TESTS2); do cp $$i.aspx /q/web; done

test: local compare

compare:
	@for i in $(TESTS) $(TESTS2); do \
		if cmp $$i.expected $$i.result; then echo OK; else echo FAILED; exit 1; fi; \
		if test -f $$i.oexpected; then \
		if cmp $$i.oexpected $$i.oresult; then echo OK; else echo FAILED; exit 1; fi; fi \
	done

local:
	@for i in $(TESTS); do \
		echo running test $$i; \
		mono rtest.exe localhost 8080 /$$i.aspx > $$i.result; \
		mono ctest.exe localhost 8080 /$$i.aspx > $$i.oresult; \
	done
	@for i in $(TESTS2); do \
		echo running test $$i; \
		mono rtest.exe -header Cache-Control: localhost 8080 /$$i.aspx > $$i.result; \
	done

tw:
	@for i in $(TESTS); do \
		echo running test $$i; \
		mono rtest.exe $(SERVER) 80 /test/$$i.aspx > $$i.expected; \
		mono ctest.exe $(SERVER) 80 /test/$$i.aspx > $$i.oexpected; \
	done
	@for i in $(TESTS2); do \
		echo running test $$i; \
		mono rtest.exe -header Cache-Control: $(SERVER) 80 /test/$$i.aspx > $$i.expected; \
	done
