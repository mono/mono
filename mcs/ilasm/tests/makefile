ILASM=mono ../ilasm.exe
VERIFY=../../tools/verifier.exe

TEST_SOURCES = test-1 test-2

all: test-compiler

# Compile with mono, run with mono jit
test-compiler:
	@rm -f *.exe; \
	for i in $(TEST_SOURCES); do \
		echo -n "Running $$i ... "; \
		if $(ILASM) $$i.il > /dev/null; then \
			if mono ./$$i.exe > /dev/null; then \
				echo OK; \
			else \
				echo FAILED; exit 1; \
			fi; \
		else \
			echo FAILED TO COMPILE; exit 1; \
		fi \
	done
#	echo "Running NO_PASS tests ..."
#	for i in $(TEST_NOPASS); do \
#		echo -n "Running $$i ... "; \
#		if $(ILASM) $$i.il > /dev/null; then \
#			echo OK; \
#		else \
#			echo FAILED TO COMPILE; exit 1; \
#		fi \
#	done

clean:
	rm *.exe
	rm *.out
	rm casts.cs

