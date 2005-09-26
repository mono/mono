TESTS = t1.exe

check:	local compare

local: $(TESTS)
	@for i in $(TESTS); do \
		echo running test $$i; \
		mono $$i > $$i.result; \
	done

compare:
	@for i in $(TESTS); do \
		if cmp $$i.expected $$i.result; then echo OK; else echo FAILED; exit 1; fi; \
	done

clean:
	rm -f *.exe *.result

%.exe: %.cs
	gmcs $< -r:System.Configuration.dll
