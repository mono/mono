MCS=../mcs/mcs
CSC=csc

TEST_SOURCES = \
	test-1 test-2 test-3 test-4 test-6 test-7 test-8 test-9 test-10 \
	test-11 test-12 test-13 test-16 test-17 test-18 test-20 test-21 \
	test-23

TEST_NOPASS = \
	test-5

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

