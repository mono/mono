#
# You can change "RUNTIME" to control what runtime to use
# ie, make vs make RUNTIME=mono
# 
CSC=csc.exe

MCS=../mcs/mcs.exe
MCS2=mono ../mcs/mcs.exe
VERIFY=../tools/verifier.exe

TEST_SOURCES = \
	test-1   test-2   test-3   test-4   test-5   test-6   test-7   test-8   test-9   test-10 \
	test-11  test-12  test-13  test-14  test-15  test-16  test-17  test-18  test-19  test-20 \
	test-21  test-22  test-23  test-24  test-25  test-26  test-27  test-28  test-29  test-30 \
	test-31  test-32  test-33  test-34  test-35  test-36  test-37  test-38  test-39  test-40 \
	test-41  test-42  test-43  test-44           test-46  test-47  test-48  test-49  \
	test-51  test-52           test-54  test-55  test-56  test-57           test-59  test-60 \
	test-61	 test-62  test-63  test-64  test-65                    test-68  test-69  test-70 \
	test-71  test-72  test-73  test-74  test-75  test-76  test-77  test-78  test-79  test-80 \
	test-81  test-82  test-83  test-84  test-85  test-86  test-87  test-88  test-89  test-90 \
	         test-92  test-93  test-94  test-95  test-96  test-97  test-98  test-99  test-100\
	test-101 test-102 test-103 test-104 test-105                   test-108 test-109 test-110\
	test-111 test-112 test-113 test-114 test-115 test-116 test-117 test-118 test-119 \
	test-121          test-123          test-125 test-126 test-127 test-128 test-129 test-130 \
	test-131                   test-134 test-135 test-136 test-137 test-138 test-139 test-140 \
	test-141 test-142 test-143 test-144 test-145 test-146 test-147 test-148 test-149 test-150 \
	                  test-153 test-154 test-155 test-156 test-157 test-158 test-159 test-160 \
	test-161 test-162 test-163 test-164 test-165 test-166 test-167 test-168 test-169 test-170 \
		 test-172 test-173 test-174 test-175 test-176

UNSAFE_SOURCES = \
	unsafe-1 unsafe-2 unsafe-3 test-58.cs test-171.cs

WINDOWS_SOURCES = \
	test-50 test-67

# A test is a 'no pass' if it fails on either windows or linux
# Test 120 does not pass because the MS.NET runtime is buggy.
TEST_NOPASS = \
	test-45  test-53  test-91  test-106 test-107 test-120 test-122 test-132 test-133 test-66 test-177

all: test-compiler test-unsafe test-windows


# Compile with mono, run with MS jit
test-compiler:
	@rm -f *.exe; \
	for i in $(TEST_SOURCES); do \
		echo -n "Running $$i ... "; \
		if $(MCS) $$i.cs > /dev/null; then \
			if $(RUNTIME) ./$$i.exe > /dev/null; then \
				echo $$i: ok; \
			else \
				echo FAILED; exit 1; \
			fi; \
		else \
			echo FAILED TO COMPILE; exit 1; \
		fi \
	done

# Compile with mono, run with MS jit
test-unsafe:
	@for i in $(UNSAFE_SOURCES); do \
		echo -n "Running (unsafe) $$i ... "; \
		if $(MCS) --unsafe $$i.cs > /dev/null; then \
			if $(RUNTIME) ./$$i.exe > /dev/null; then \
				echo OK; \
			else \
				echo FAILED; exit 1; \
			fi; \
		else \
			echo FAILED WHILE COMPILING; exit 1; \
		fi \
	done

# Compiled (previously) with mono, run with mono jit
test-jit:
	@for i in $(TEST_SOURCES:.cs=.exe); do \
		echo -n "Running jit $$i ... "; \
		if mono ./$$i.exe > /dev/null; then \
			echo OK; \
		else \
			echo FAILED; exit 1; \
		fi \
	done

# Compiled with mono, run with MS jit
test-windows:
	@echo Running windows-only tests - these will fail on linux; \
	for i in $(WINDOWS_SOURCES); do \
		echo -n "Running $$i ... "; \
		if $(MCS) $$i.cs > /dev/null; then \
			if ./$$i.exe > /dev/null; then \
				echo OK; \
			else \
				echo FAILED; exit 1; \
			fi; \
		else \
			echo FAILED TO COMPILE; exit 1; \
		fi \
	done

# Compile with mono, run with mono jit
test-compiler-mono:
	@rm -f *.exe; \
	for i in $(TEST_SOURCES); do \
		echo -n "Running $$i ... "; \
		if $(MCS2) $$i.cs > /dev/null; then \
			if mono ./$$i.exe > /dev/null; then \
				echo OK; \
			else \
				echo FAILED; exit 1; \
			fi; \
		else \
			echo FAILED TO COMPILE; exit 1; \
		fi \
	done
	echo "Running NO_PASS tests ..."
	for i in $(TEST_NOPASS); do \
		echo -n "Running $$i ... "; \
		if $(MCS2) $$i.cs > /dev/null; then \
			echo OK; \
		else \
			echo FAILED TO COMPILE; exit 1; \
		fi \
	done

# Compile with mono, run with mono jit
test-unsafe-mono:
	@rm -f *.exe; \
	for i in $(UNSAFE_SOURCES); do \
		echo -n "Running $$i ... "; \
		if $(MCS2) --unsafe $$i.cs > /dev/null; then \
			if mono ./$$i.exe > /dev/null; then \
				echo OK; \
			else \
				echo FAILED; exit 1; \
			fi; \
		else \
			echo FAILED TO COMPILE; exit 1; \
		fi \
	done

verify:
	@for i in $(TEST_SOURCES); do \
		if $(MCS) -o mcs-gen-code.exe $$i.cs > /dev/null; then \
	        	if $(CSC) /out:csc-gen-code.exe $$i.cs > /dev/null; then \
				if $(VERIFY) mcs-gen-code.exe csc-gen-code.exe > /dev/null; then \
					echo $$i: identical assemblies; \
				else \
					echo $$i: unidentical assemblies; exit; \
				fi; \
			fi \
		fi \
	done; \
	echo Verification passed


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

