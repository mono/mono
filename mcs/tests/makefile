ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.2914/csc.exe

MCS=../mcs/mcs.exe
VERIFY=../tools/verifier.exe

TEST_SOURCES = \
	test-1  test-2  test-3  test-4  test-5  test-6  test-7  test-8  test-9  test-10 \
	test-11 test-12 test-13 test-14 test-15 test-16 test-17 test-18 test-19 test-20  \
	test-21 	test-23 test-24 test-25 test-26 test-27 test-28 		 \
	test-31 test-32 test-33 test-34 test-35 test-37 test-39 test-40 test-42 test-43  \
	test-44 test-45 test-46

TEST_NOPASS = \
	test-22 test-29 test-44

test-compiler:
	-rm *.exe
	for i in $(TEST_SOURCES); do \
		if $(MCS) $$i.cs > /dev/null; then \
			if ./$$i.exe; then \
				echo $$i: ok; \
			else \
				echo failed; exit; \
			fi; \
		else \
			echo compiler failed on $$i;   exit; \
		fi \
	done



verify:
	for i in $(TEST_SOURCES); do \
		if $(MCS) -o mcs-gen-code.exe $$i.cs > /dev/null; then \
	        	if $(CSC) /out:csc-gen-code.exe $$i.cs > /dev/null; then \
				if $(VERIFY) mcs-gen-code.exe csc-gen-code.exe > /dev/null; then \
					echo $$i: identical assemblies; \
				else \
					echo $$i: unidentical assemblies; exit; \
				fi; \
			fi \
		fi \
	done


casts.cs: gen-cast-test.cs
	$(CSC) /out:csc-cast.exe gen-cast-test.cs
	./csc-cast > casts.cs

casts-csc.exe: casts.cs
	$(CSC) /out:casts-csc.exe casts.cs

casts.exe: casts.cs
	$(MCS) casts.cs

csc-casts.out: casts-csc.exe
	./casts-csc.exe > csc-casts.out

msc-casts.out: casts.exe
	./casts.exe > msc-casts.out

test-casts: csc-casts.out msc-casts.out
	cmp csc-casts.out msc-casts.out

clean:
	rm *.exe
	rm *.out
	rm casts.cs

