TEST_SOURCES = \
	test-1 test-2 test-3

test-compiler:
	for i in $(TEST_SOURCES); do \
		if ../mcs/compiler $$i.cs > /dev/null; then \
			if ./$$i.exe; then \
				echo $$i: ok; \
			else \
				echo failed; \
			fi; \
		else \
			echo compiler failed on $$i;   \
		fi \
	done
