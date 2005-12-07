TESTS = t1.aspx t2.aspx t3.aspx t4.aspx t5.aspx t6.aspx t7.aspx
TESTS2 = c1.aspx c2.aspx c3.aspx
#SERVER=quack
SERVER=lalo-office.boston.ximian.com

all: rtest.exe ctest.exe test

rtest.exe: rtest.cs
	mcs rtest.cs

ctest.exe: ctest.cs
	mcs ctest.cs

copy:
	for i in $(TESTS) $(TESTS2); do cp $$i /q/web; done

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
		mono rtest.exe localhost 8080 /$$i > $$i.result; \
		mono ctest.exe localhost 8080 /$$i > $$i.oresult; \
	done
	@for i in $(TESTS2); do \
		echo running test $$i; \
		mono rtest.exe -header Cache-Control: localhost 8080 /$$i > $$i.result; \
	done

tw:
	@for i in $(TESTS); do \
		echo running test $$i; \
		mono rtest.exe $(SERVER) 80 /test/$$i > $$i.expected; \
		mono ctest.exe $(SERVER) 80 /test/$$i > $$i.oexpected; \
	done
	@for i in $(TESTS2); do \
		echo running test $$i; \
		mono rtest.exe -header Cache-Control: $(SERVER) 80 /test/$$i > $$i.expected; \
	done

