#
# Compile at the maximum warning level to probe for warnings
#
MCS=mono ../mcs/mcs.exe /nowarn:162 /nowarn:169 --wlevel 4 # for linux
#MCS=../mcs/mcs.exe --wlevel 4 # for windows


all:
	@ failed=false; \
	for i in cs*.cs; do \
		case $$i in \
			cs[0-9]*-[0-9]*cs) \
			error=`echo $$i | sed -e 's/cs*//' -e 's/.cs//' -e 's/-.*//'`; \
			;;\
			cs[0-9-]*cs)  \
			error=`echo $$i | sed -e 's/cs*//' -e 's/.cs//'`; \
			;;\
		esac; \
		echo -n "Running test $$i ... "; \
		if $(MCS) --unsafe --expect-error $$error $$i > /dev/null; \
		then echo OK; \
		else echo FAILED; \
		     flist="$$flist $$i"; \
		     failed=true; \
		fi; \
	done; \
	if $$failed; then \
		echo "The following tests failed: $$flist"; \
	else \
		echo All tests passed; \
	fi

test-error-mono:
	@ failed=false; \
	for i in error-*.cs; do \
		echo -n "Running test $$i ... "; \
		if MCS='$(MCS)' perl ./runtest.pl $$i > /dev/null; \
		then echo OK; \
		else echo FAILED; \
		     flist="$$flist $$i"; \
		     failed=true; \
		fi; \
	done; \
	if $$failed; then \
		echo "The following tests failed: $$flist"; \
	else \
		echo All tests passed; \
	fi
