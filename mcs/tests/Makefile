#
# use `run-test-local'
#
# *** make sure you add multi-file tests to harness.mk

thisdir = tests
SUBDIRS = 
include ../build/rules.make

DISTFILES = README.tests harness.mk $(wildcard *.cs)

ifeq (default, $(PROFILE))
# force this, we don't case if CSC is broken. This also
# means we can use --options, yay.

MCS = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(INTERNAL_MCS)
endif

# We don't want debugging info :-)

USE_MCS_FLAGS :=

# All new tests for 'mcs' go in here
# These tests are not run by GMCS since the corresponding code is not yet in there.-
# Martin Baulig will manually move them into TEST_SOURCES_common after merging the code into GMCS.
# He may also move some to TEST_EXCLUDE_net_2_0 if some of the merges are inappropriate for GMCS.
#
NEW_TEST_SOURCES_common = \
	test-135 test-297 test-287 test-294 test-299 test-300 \
	test-301 test-302 test-303 test-304 test-305 test-306 test-307 test-259 test-309 \
	test-311 test-312 test-313 test-314 \
	a-call		a-capture5    a-instance	a-parameter2			\
	a-capture10	a-capture6    a-nested-anon2  					\
	a-capture1	a-capture7    a-nested-anon3    				\
	a-capture2	a-capture8    a-nested-anon4  					\
	a-capture3	a-capture9    a-nested-anon   	a-simple2				\
	a-capture4	a-event       a-nested		a-simple

#
# Please do _not_ add any tests here - all new tests should go into NEW_TEST_SOURCES_common
# Martin Baulig is the only one who may add tests here - after merging the latest MCS patches
# into GMCS
#
TEST_SOURCES_common = \
	test-1   test-2   test-3   test-4   test-5   test-6   test-7   test-8   test-9   test-10  \
	test-11  test-12  test-13  test-14  test-15  test-16  test-17  test-18  test-19  test-20  \
	test-21  test-22  test-23  test-24  test-25  test-26  test-27  test-28  test-29  test-30  \
	test-31  test-32  test-33  test-34  test-35  test-36  test-37  test-38  test-39  test-40  \
	test-41  test-42  test-43  test-44  test-45  test-46  test-47  test-48  test-49  	  \
	test-51  test-52  test-53  test-54  test-55  test-56  test-57  test-58  test-59  test-60  \
	test-61	 test-62  test-63  test-64  test-65  test-66           test-68  test-69  test-70  \
	test-71  test-72  test-73  test-74  test-75  test-76  test-77  test-78  test-79  test-80  \
	test-81  test-82  test-83  test-84  test-85  test-86  test-87  test-88  test-89  test-90  \
	test-91  test-92  test-93  test-94  test-95  test-96  test-97  test-98  test-99  test-100 \
	test-101 test-102 test-103 test-104          test-106 test-107 test-108 test-109 test-110 \
	test-111 test-112 test-113 test-114 test-115 test-116 test-117 test-118 test-119 	  \
	test-121 test-122 test-123          test-125 test-126 test-127 test-128 test-129 test-130 \
	test-131                   test-134	     test-136 test-137 test-138 test-139 test-140 \
	test-141 test-142 test-143 test-144 test-145 test-146 test-147 test-148 test-149 test-150 \
	test-151 test-152 test-153 test-154 test-155 test-156 test-157 test-158 test-159 test-160 \
	test-161 test-162 test-163 test-164 test-165 test-166 test-167 test-168 test-169 test-170 \
	test-171 test-172 test-173 test-174 test-175 test-176 test-177 test-178 test-179 test-180 \
	test-181 test-182 test-183 test-184 test-185 test-186 test-187 test-188 test-189 test-190 \
	test-191 test-192 test-193 test-194 test-195 test-196 test-197 test-198 test-199 test-200 \
	test-201 test-202 test-203 test-204 test-205 test-206 test-207 test-208 test-209 test-210 \
	test-211 test-212 test-213 test-214 test-215 test-216 test-217 test-218 test-219 test-220 \
	test-221 test-222 test-223 test-224 test-225 test-226 test-227          test-229 test-230 \
	test-231 test-232 test-233 test-234 test-235 test-236 test-237 test-238 test-239 test-240 \
	test-241 test-242 test-243 test-244 test-245 test-246 test-247 test-248 test-249 test-250 \
	test-251 test-252 test-253 test-254 test-255 test-256 test-257 test-258          test-260 \
	test-261 test-262 test-263 test-264 test-265 test-266 test-267 test-268 test-269 test-270 \
	test-271 test-272 test-273 test-274 test-275 test-276 test-277 test-278 test-279 test-280 \
	test-281 test-282 test-283 test-284 test-285 test-286          test-288 test-289 test-290 \
	test-291 test-292 test-293          test-295 test-296          test-298 \
	cls-test-0 cls-test-1 cls-test-2 cls-test-3 cls-test-5 cls-test-6 cls-test-7 cls-test-10  \
	cls-test-11 cls-test-14 cls-test-15 cls-test-16 \
	2test-1  2test-2  2test-3  2test-4  2test-5  2test-6  2test-7  2test-8  2test-9  2test-10 \
	2test-11 2test-12 2test-13 2test-14 \
	unsafe-1 unsafe-2 unsafe-3 unsafe-5 unsafe-6 unsafe-7 unsafe-8 unsafe-9 unsafe-10 \
	mtest-1-dll mtest-1-exe \
	dll-1 prog-1 \
	dll-2 prog-2 \
	conv-lib conv-main \
	vararg-lib vararg-exe \
	module-1 module-2 module-3 \
	ns0 ns

TEST_EXCLUDES_common = 

TEST_SOURCES_default = $(NEW_TEST_SOURCES_common)

TEST_SOURCES_net_2_0 = \
	gen-13-dll gen-13-exe gen-17-dll gen-17-exe gen-47-dll gen-47-exe	       \
	gen-1   gen-2   gen-3   gen-4   gen-5   gen-6   gen-7   gen-8   gen-9   gen-10 \
	gen-11  gen-12          gen-14  gen-15  gen-16          gen-18  gen-19  gen-20 \
	gen-21  gen-22  gen-23  gen-24  gen-25  gen-26  gen-27  gen-28  gen-29  gen-30 \
	        gen-32  gen-33  gen-34  gen-35  gen-36  gen-37  gen-38  gen-39  gen-40 \
	gen-41  gen-42  gen-43  gen-44  gen-45  gen-46          gen-48  gen-49  gen-50 \
	gen-51  gen-52  gen-53  gen-54  gen-55  gen-56          gen-58  gen-59  gen-60 \
	        gen-62  gen-63  gen-64          gen-66  gen-67  gen-68  gen-69  gen-70 \
	gen-71  gen-72  gen-73  gen-74  gen-75  gen-76  gen-77  gen-78  gen-79  gen-80 \
	gen-81  gen-82  gen-83  gen-84  gen-85  gen-86  gen-87  gen-88  gen-89  gen-90 \
	gen-91  gen-92  gen-93  gen-94          gen-96  gen-97  gen-98  gen-99  gen-100

TEST_EXCLUDES_net_2_0 = test-55 test-289 gen-77

TEST_SOURCES = $(filter-out $(TEST_EXCLUDES_common) $(TEST_EXCLUDES_$(PROFILE)) $(TEST_EXCLUDES_$(PLATFORM)), \
	$(TEST_SOURCES_common) $(TEST_SOURCES_$(PROFILE)) $(TEST_SOURCES_$(PLATFORM)))

# These tests load User32.dll and/or Kernel32.dll
TEST_SOURCES_win32 = test-50 test-67

## FIXME: Need to audit.  Maybe move to 'TEST_EXCLUDES_linux' and 'TEST_EXCLUDES_win32' as approprate
# A test is a 'no pass' if it fails on either windows or linux
# Test 120 does not pass because the MS.NET runtime is buggy.

TEST_NOPASS = test-120 test-132 test-133 a-parameter4.cs 
#	test-28 test-45 test-53 test-91 test-102 test-106 test-107 test-122 test-66 test-177

all-local install-local uninstall-local:

# casts

bootstrap-cast.exe: gen-cast-test.cs
	$(BOOT_COMPILE) -target:exe /out:$@ $<

casts.cs: bootstrap-cast.exe
	$(RUNTIME) $< >$@

casts-mcs.exe: casts.cs
	$(CSCOMPILE) -target:exe /out:$@ $<

casts-boot.exe: casts.cs
	$(BOOT_COMPILE) -target:exe /out:$@ $<

boot-casts.out: casts-boot.exe
	$(RUNTIME) $< >$@

mcs-casts.out: casts-mcs.exe
	$(RUNTIME) $< >$@

test-casts: boot-casts.out mcs-casts.out
	cmp $^

# compilation is part of the testing too, so we don't actually 
# do anything in test-local.

test-local: 

run-test-local: multi test-harness test-casts

test-everything:
	$(MAKE) PROFILE=default run-test
	$(MAKE) PROFILE=net_2_0 run-test

test-generics:
	$(MAKE) PROFILE=net_2_0 run-test

test-generics-2:
	$(MAKE) PROFILE=net_2_0 TEST_SOURCES="$(TEST_SOURCES_net_2_0)" test-harness

clean-local:
	rm -f *.exe *.netmodule *.out *.pdb casts.cs

dist-local: dist-default
	rm -f $(distdir)/casts.cs

ifeq (net_2_0, $(PROFILE))
TEST_TAG = gmcs
else
TEST_TAG = mcs
endif

.PHONY: test-harness test-harness-run
test-harness:
	@$(MAKE) -s test-harness-run

test-harness-run:
	@-rm -f $(TEST_TAG).log
	@-rm -fr dir-$(TEST_TAG)
	@mkdir dir-$(TEST_TAG)
	@sed 's,@thisdir@,$(thisdir)/dir-$(TEST_TAG),' harness.mk > dir-$(TEST_TAG)/Makefile
	@echo 'Running $(TEST_TAG) tests with flags "$(TEST_RUNTIME)" ... '
	@cd dir-$(TEST_TAG) ; \
	$(MAKE) -s $(TEST_SOURCES:=.res) || failed="make " ; \
	cat $(TEST_SOURCES:=.res) > ../$(TEST_TAG).log ; \
	failed=$$failed`sed -n 's,^FAIL: ,,p' ../$(TEST_TAG).log` ; \
	if test -z "$$failed"; then :; else echo "Failing tests: $$failed"; exit 1; fi

#
# Tests that require separate compilation
#
multi: multi-2 ilasm
	echo Multi-assembly test passes

#
# Tests that the order for internal/public in external 
# assemblies does not affect the outcome of a build.
# also checks that multiple `entry points' can be declared
# in a library. (eg, that it is not confused by two Main methods)
#
multi-2:
	$(CSCOMPILE) -target:library pi.cs
	$(CSCOMPILE) -target:library pp.cs
	$(CSCOMPILE) pu.cs -r:pi.dll -r:pp.dll
	$(CSCOMPILE) pu.cs -r:pp.dll -r:pi.dll

ilasm:
	$(INTERNAL_ILASM) /dll property-il.il
	$(CSCOMPILE) /r:property-il.dll property-main.cs /out:property-main.exe
	$(TEST_RUNTIME) property-main.exe
