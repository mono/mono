thisdir = errors
SUBDIRS = 
include ../build/rules.make

DISTFILES = \
	CONTRIBUTORS_README 	\
	errors.txt 		\
	$(wildcard *.cs)	\
	$(wildcard *.il)	\
	$(wildcard *.inc)	\
	$(wildcard *.pub)	\
	$(wildcard *.snk)	\
	$(wildcard *.response)	\
	$(wildcard known-issues-*)	\
	$(wildcard dlls/first/*.cs)	\
	$(wildcard dlls/second/*.cs)

TEST_SUPPORT_FILES = \
	CS0012-lib.dll CS0012-2-lib.dll CS0012-3-lib.dll CS0012-4-lib.dll CS0012-5-lib.dll CS0012-6-lib.dll CS0012-9-lib.dll CS0012-10-lib.dll CS0012-11-lib.dll CS0012-12-lib.dll CS0012-13-lib.dll CS0012-14-lib.dll CS0012-15-lib.dll CS0012-16-lib.dll CS0012-17-lib.dll CS0012-18-lib.dll CS0012-21-lib.dll CS0012-22-lib.dll CS0012-23-lib.dll CS0012-24-lib.dll CS0019-71-lib.dll CS0029-26-lib.dll \
	CS0103-2-lib.dll CS0118-2-lib.dll CS0122-8-lib.dll CS0122-10-lib.dll CS0122-14-lib.dll CS0122-15-lib.dll CS0122-19-lib.dll CS0122-35-lib.dll CS0122-36-lib.dll CS0122-38-lib.dll CS0143-lib.dll CS0144-3-lib.dll CS0165-19-lib.dll CS0165-51-lib.dll \
	CS0205-3-lib.dll CS0246-29-lib.dll CS0229-3-lib.dll CS0229-4-lib.dll CS0266-25-lib.dll \
	CS0315-2-lib.dll \
	CS0425-7-lib.dll CS0433-lib.dll CS0433-2-lib.dll \
	CS0506-3-lib.dll CS0507-7-lib.dll CS0507-8-lib.dll CS0534-3-lib.dll CS0534-4-lib.dll CS0534-6-lib.dll CS0571-3-lib.dll CS0571-5-lib.dll CS0571-6-lib.dll \
	CS0612-2-lib.dll CS0618-2-lib.dll CS0619-8-lib.dll CS0619-17-lib.dll CS0619-32-lib.dll CS0619-33-lib.dll CS0619-36-lib.dll CS0619-42-lib.dll \
	CS0619-43-lib.dll CS1546-lib.dll CS0619-51-lib.dll CS1509-module.dll CS1681-2-lib.dll \
	CS0730-lib.dll CS0731-1-lib.dll CS0731-2-lib.dll \
	CS1070-lib.dll \
	CS1540-15-lib.dll CS1540-17-lib.dll CS1542-lib.dll CS1577-lib.dll \
	CS1683-lib.dll CS1685-2-lib.dll \
	dlls/first/CS1701-lib.dll dlls/second/CS1701-lib.dll CS1701-lib.dll dlls/first/CS1702-lib.dll dlls/second/CS1702-lib.dll CS1702-lib.dll dlls/first/CS1705-lib.dll dlls/second/CS1705-lib.dll CS1705-lib.dll \
	CS2015-lib.dll CS3005-16-lib.dll CS3013-module.dll CSExternAlias-lib.dll \
	CS7069-lib.dll \
	CS8009-lib.dll \
	CSFriendAssembly-lib.dll \
	dlls/first/CS1704-lib.dll \
	dlls/second/CS1704-lib.dll \
	dlls/cs1703-2/System.dll

-include $(mcs_topdir)/build/config.make

# mention all targets
all-local $(STD_TARGETS:=-local):

VALID_PROFILE := $(filter $(DEFAULT_PROFILE), $(PROFILE))
ifdef VALID_PROFILE

qcheck: run-mcs-tests 

run-test-local: test-local qcheck

check: run-test-local

# Required to test parser correctly
# -compiler-options:-v

TEST_PATTERN = 'v4'
TESTER_OPTIONS = -compiler-options:"-v --break-on-ice -d:NET_4_0;NET_4_5"

COMPILER = $(topdir)/class/lib/$(PROFILE)/mcs.exe
TESTER = MONO_RUNTIME='$(RUNTIME)' $(TEST_RUNTIME) $(TEST_RUNTIME_FLAGS) $(LOCAL_RUNTIME_FLAGS) $(topdir)/class/lib/$(PROFILE)/compiler-tester.exe

test-local: $(TEST_SUPPORT_FILES)

run-mcs-tests: test-local
	$(TESTER) -mode:neg -files:$(TEST_PATTERN) -compiler:$(COMPILER) -reference-dir:$(topdir)/class/lib/$(PROFILE) -issues:known-issues-$(PROFILE) -log:$(PROFILE).log $(TESTER_OPTIONS) $(TOPTIONS)

endif

clean-local:
	rm -f *.exe *.dll *.log *.mdb dummy.xml *.junk dlls/first/*.dll dlls/second/*.dll

dist-local: dist-default

csproj-local install-local uninstall-local:

CS0029-26-lib.dll : CS0029-26-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /publicsign /keyfile:key.snk /out:$@ $<

CS0266-25-lib.dll: CS0266-25-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /r:$(topdir)/class/lib/$(PROFILE)/System.Core.dll /out:$@ $<

dlls/first/CS1701-lib.dll: dlls/first/CS1701-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /publicsign /keyfile:key.snk /out:$@ $<

dlls/first/CS1702-lib.dll: dlls/first/CS1702-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /publicsign /keyfile:key.snk /out:$@ $<

dlls/first/CS1703-lib.dll: dlls/first/CS1703-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /publicsign /keyfile:key.snk /out:$@ $<

dlls/first/CS1705-lib.dll: dlls/first/CS1705-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /publicsign /keyfile:key.snk /out:$@ $<

dlls/second/CS1701-lib.dll: dlls/second/CS1701-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /publicsign /keyfile:key.snk /out:$@ $<

dlls/second/CS1702-lib.dll: dlls/second/CS1702-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /publicsign /keyfile:key.snk /out:$@ $<

dlls/second/CS1703-lib.dll: dlls/second/CS1703-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /publicsign /keyfile:key.snk /out:$@ $<

dlls/second/CS1705-lib.dll: dlls/second/CS1705-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /publicsign /keyfile:key.snk /out:$@ $<

dlls/cs1703-2/System.dll: $(topdir)/../external/binary-reference-assemblies/v2.0/System.dll
	mkdir -p $(dir $@)
	cp $< $@

CS1701-lib.dll : CS1701-lib.cs dlls/first/CS1701-lib.dll
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /r:dlls/first/CS1701-lib.dll /out:$@ $<

CS1702-lib.dll : CS1702-lib.cs dlls/first/CS1702-lib.dll
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /r:dlls/first/CS1702-lib.dll /out:$@ $<

CS1705-lib.dll : CS1705-lib.cs dlls/first/CS1705-lib.dll
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /r:dlls/first/CS1705-lib.dll /out:$@ $<

CSFriendAssembly-lib.dll : CSFriendAssembly-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /publicsign /keyfile:key.snk /warn:0 /out:$@ $<

%-lib.dll: %-lib.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:library /warn:0 /out:$@ $<

%-module.dll: %-module.cs
	$(CSCOMPILE) /r:$(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll /target:module /out:$@ $<

%-lib.dll: %-lib.il
	$(ILASM) /dll /out:$@ $<
