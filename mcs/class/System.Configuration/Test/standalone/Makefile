TESTS = t1.exe t2.exe t3.exe t4.exe t5.exe t6.exe t7.exe t8.exe t9.exe t10.exe t11.exe t12.exe t13.exe t14.exe t15.exe t16.exe t17.exe t18.exe t19.exe t20.exe

check:	local compare

local: $(TESTS)
	@for i in $(TESTS); do \
		echo running test $$i; \
		mono --debug $$i > $$i.result; \
	done

compare:
	@for i in $(TESTS); do \
		echo -n "$$i: "; \
		if cmp $$i.expected $$i.result >/dev/null ; then echo "  OK"; else echo "  FAILED"; fi; \
	done

clean:
	rm -f *.exe *.mdb *.result

%.exe: %.cs
	gmcs /debug $< -r:System.Configuration.dll
