thisdir = tests
SUBDIRS = 
include ../build/rules.make

DISTFILES = README.tests $(wildcard *.cs)

# We don't want debugging info :-)

USE_MCS_FLAGS :=

#
# If you add any tests here, please do also add them to README.Tests !
#

#
# Empty slot: test-189
#

TEST_SOURCES = \
	test-1   test-2   test-3   test-4   test-5   test-6   test-7   test-8   test-9   test-10 \
	test-11  test-12  test-13  test-14  test-15  test-16  test-17  test-18  test-19  test-20 \
	test-21  test-22  test-23  test-24  test-25  test-26  test-27  test-28  test-29  test-30 \
	test-31  test-32  test-33  test-34  test-35  test-36  test-37  test-38  test-39  test-40 \
	test-41  test-42  test-43  test-44           test-46  test-47  test-48  test-49  \
	test-51  test-52           test-54  test-55  test-56  test-57  	        test-59  test-60 \
	test-61	 test-62  test-63  test-64  test-65  test-66           test-68  test-69  test-70 \
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
		 test-172 test-173 test-174 test-175 test-176 test-177 test-178 test-179 test-180 \
	test-181 test-182 test-183 test-184 test-185 test-186 test-187 test-188          test-190 \
	test-191 test-192 test-193 test-194 test-195 test-196 test-197 test-198 test-199 test-200 \
	test-201 test-202 test-203 test-204


UNSAFE_SOURCES = \
	unsafe-1 unsafe-2 unsafe-3 test-58 test-171 unsafe-5 unsafe-6

WINDOWS_SOURCES = \
	test-50 test-67

# A test is a 'no pass' if it fails on either windows or linux
# Test 120 does not pass because the MS.NET runtime is buggy.

TEST_NOPASS = \
	test-45  test-53  test-91  test-106 test-107 test-120 test-122 test-132 test-133 test-66 test-177

all-local install-local:

# casts

bootstrap-cast.exe: gen-cast-test.cs
	$(BOOT_COMPILE) /target:exe /out:$@ $<

casts.cs: bootstrap-cast.exe
	$(RUNTIME) $< >$@

casts-mcs.exe: casts.cs
	$(CSCOMPILE) /target:exe /out:$@ $<

casts-boot.exe: casts.cs
	$(BOOT_COMPILE) /target:exe /out:$@ $<

boot-casts.out: casts-boot.exe
	$(RUNTIME) $< >$@

mcs-casts.out: casts-mcs.exe
	$(RUNTIME) $< >$@

test-casts: boot-casts.out mcs-casts.out
	cmp $^

# compilation is part of the testing too, so we don't actually 
# do anything in test-local.

test-local: 

run-test-local: test-compiler-jit test-unsafe-compiler-jit test-casts

clean-local:
	rm -f *.exe *.out *.pdb casts.cs

dist-local: dist-default
	rm -f $(distdir)/casts.cs

test-compiler-jit: multi
	@rm -f *.exe ; \
	echo "Flags \"$(TEST_RUNTIME)\" ... " ; \
	for i in $(TEST_SOURCES) ; do \
	    echo -n "$$i: "; \
	    if $(INTERNAL_MCS) $$i.cs >/dev/null ; then \
		true ; \
	    else \
	        echo FAILED COMPILATION ; exit 1; \
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe >/dev/null ; then \
	        echo OK ; \
	    else \
	        echo FAILED ; exit 1; \
	    fi ; \
	done

test-unsafe-compiler-jit:
	@echo "Running UNSAFE tests ..." ; \
	echo "Flags \"$(TEST_RUNTIME)\" ... " ; \
	for i in $(UNSAFE_SOURCES) ; do \
	    echo -n "$$i: "; \
	    if $(INTERNAL_MCS) /unsafe $$i.cs >/dev/null ; then \
	        true ; \
	    else \
	        echo FAILED COMPILATION ; exit 1\
	    fi ; \
	    if $(TEST_RUNTIME) ./$$i.exe >/dev/null ; then \
	        echo OK ; \
	    else \
	        echo FAILED ; exit 1\
	    fi ; \
	done


#
# Tests that require separate compilation
#
multi: multi-1 multi-2
	echo Multi-assembly test passes

multi-1:
	$(INTERNAL_MCS) /target:library dll-1.cs
	$(INTERNAL_MCS) /r:dll-1.dll prog-1.cs /out:prog-1.exe
	$(TEST_RUNTIME) prog-1.exe

#
# Tests that the order for internal/public in external 
# assemblies does not affect the outcome of a build.
#
multi-2:
	$(INTERNAL_MCS) /target:library pi.cs
	$(INTERNAL_MCS) /target:library pp.cs
	$(INTERNAL_MCS) pu.cs -r:pi.dll -r:pp.dll
	$(INTERNAL_MCS) pu.cs -r:pp.dll -r:pi.dll
