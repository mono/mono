TEST_SOURCES = \
	test-1 test-4 test-6 test-7 test-8 test-9

TEST_NOPASS = \
	test-2 test-3 test-5

test-compiler:
	for i in $(TEST_SOURCES); do \
		if ../mcs/compiler $$i.cs > /dev/null; then \
			if ./$$i.exe; then \
				echo $$i: ok; \
			else \
				echo failed; exit; \
			fi; \
		else \
			echo compiler failed on $$i;   exit; \
		fi \
	done
