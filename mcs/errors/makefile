#
# Compile at the maximum warning level to probe for warnings
#
MCS=../mcs/mcs.exe --wlevel 4

all:
	failed=false; \
	for i in cs*.cs; do \
		case $$i in \
			cs[0-9]*-[0-9]*cs) \
			error=`echo $$i | sed -e 's/cs*//' -e 's/.cs//' -e 's/-.*//'`; \
			;;\
			cs[0-9-]*cs)  \
			error=`echo $$i | sed -e 's/cs*//' -e 's/.cs//'`; \
			;;\
		esac; \
		echo $(MCS) --probe $$error $$i; \
		$(MCS) --probe $$error 1 $$i > /dev/null; \
		code=$$?; \
		if test $$code != 123; then \
			echo Code is: $$code; \
			if [ x$$code = x124 ]; then \
				echo "Compiler failed to flag $$i"; \
			else \
				echo "Compiler failed to compile $$i"; \
			fi; \
			flist="$$flist $$i"; \
			failed=true; \
		else \
			echo test $$i ok; \
		fi; \
	done; \
	if $$failed; then \
		echo "The following tests failed: $$flist"; \
	else \
		echo All tests passed; \
	fi