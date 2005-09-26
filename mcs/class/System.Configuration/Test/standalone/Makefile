TESTS = t1.exe t2.exe t3.exe t4.exe

check:	local compare

local: $(TESTS)
	@for i in $(TESTS); do \
		echo running test $$i; \
		mono --debug $$i > $$i.result; \
	done

compare:
	@for i in $(TESTS); do \
		echo -n "$$i: "; \
		if cmp $$i.expected $$i.result; then echo "  OK"; else echo "  FAILED"; fi; \
	done

clean:
	rm -f *.exe *.mdb *.result

%.exe: %.cs
	gmcs /debug $< -r:System.Configuration.dll
