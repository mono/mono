thisdir = class/Microsoft.Web.Services
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Microsoft.Web.Services.1.dll
LIB_MCS_FLAGS = /r:System.dll /r:System.Security.dll /r:System.Web.dll /r:System.Web.Services.dll /r:System.Xml.dll /r:System.Security.dll /r:Mono.Security.dll /d:WSE1
NO_TEST = yes

LIBRARY2 = Microsoft.Web.Services.2.dll
LIB_MCS_FLAGS2 = /r:System.dll /r:System.Security.dll /r:System.Web.dll /r:System.Web.Services.dll /r:System.Xml.dll /r:System.Security.dll /r:Mono.Security.dll /d:WSE2
NO_TEST2 = yes

# We are using a custom makefile to build both WSE1 and WSE2

sourcefile = Microsoft.Web.Services.dll.sources
sourcefile2 = Microsoft.Web.Services.dll.2.sources
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
response = $(depsdir)/$(LIBRARY).response
response2 = $(depsdir)/$(LIBRARY2).response
else
response = $(sourcefile)
response2 = $(sourcefile2)
endif
makefrag = $(depsdir)/$(LIBRARY).makefrag
makefrag2 = $(depsdir)/$(LIBRARY2).makefrag
stampfile = $(depsdir)/$(LIBRARY).stamp
stampfile2 = $(depsdir)/$(LIBRARY2).stamp
the_lib = $(topdir)/class/lib/$(LIBRARY)
the_lib2 = $(topdir)/class/lib/$(LIBRARY2)
the_pdb = $(patsubst %.dll,%.pdb,$(the_lib))
the_pdb2 = $(patsubst %.dll,%.pdb,$(the_lib2))

ifndef NO_TEST
test_lib = $(patsubst %.dll,%_test.dll,$(LIBRARY))
test_lib2 = $(patsubst %.dll, %_test.dll,$(LIBRARY2))
test_pdb = $(patsubst %.dll,%.pdb,$(test_lib))
test_pdb2 = $(patsubst %.dll,%.pdb,$(test_lib2))
test_sourcefile = $(test_lib).sources
test_sourcefile2 = $(test_lib2).sources
test_response = $(depsdir)/$(test_lib).response
test_response2 = $(depsdir)/Microsoft.Web.Services.2_test.dll.response
test_makefrag = $(depsdir)/$(test_lib).makefrag
test_makefrag2 = $(depsdir)/Microsoft.Web.Services.2_test.dll.makefrag
test_stampfile = $(depsdir)/$(test_lib).stamp
test_stampfile2 = $(depsdir)/Microsoft.Web.Services.2_test.dll.stamp
test_flags = /r:$(the_lib) /r:$(topdir)/class/lib/NUnit.Framework.dll $(TEST_MCS_FLAGS)
test_flags2 = /r:$(the_lib2) /r:$(topdir)/class/lib/NUnit.Framework.dll $(TEST_MCS_FLAGS)
endif

all-local: $(the_lib) $(the_lib2)

ifdef DEFAULT_WSE2
def_lib = $(the_lib2)
else
def_lib = $(the_lib)
endif

install-local: $(the_lib) $(the_lib2)
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib
	$(INSTALL_LIB) $(the_lib) $(DESTDIR)$(prefix)/lib
	$(INSTALL_LIB) $(the_lib2) $(DESTDIR)$(prefix)/lib
	$(INSTALL_LIB) $(def_lib) $(DESTDIR)$(prefix)/lib/Microsoft.Web.Services.dll

clean-local:
	-rm -f $(the_lib) $(makefrag) $(test_lib) \
	       $(the_lib2) $(makefrag2) $(test_lib2) \
	       $(test_makefrag) $(test_response) \
	       $(test_makefrag2) $(test_response2) \
	       $(stampfile) $(test_stampfile) \
	       $(stampfile2) $(test_stampfile2) \
	       $(the_pdb) $(test_pdb) \
	       $(the_pdb2) $(test_pdb2) \
	       TestResult.xml
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	-rm -rf $(response) $(response2)
endif

ifndef NO_TEST
test-local: $(the_lib) $(test_lib) $(the_lib2) $(test_lib2) 

run-test-local:
	$(TEST_RUNTIME) $(TEST_HARNESS) $(test_lib)

else
test-local: $(the_lib) $(the_lib2)

run-test-local:
endif

DISTFILES = $(sourcefile) $(test_sourcefile) $(EXTRA_DISTFILES)

ifdef NO_TEST
TEST_FILES = 
else
TEST_FILES = `cat $(test_sourcefile) |sed -e 's,^\(.\),Test/\1,'`
TEST_FILES2 = `cat $(test_sourcefile2) |sed -e 's,^\(.\),Test/\1,'`
endif

dist-local: dist-default
	for f in `cat $(sourcefile)` $(TEST_FILES) ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp -p $$f $$dest || exit 1 ; \
	done
	for f in `cat $sourcefile2)` $(TEST_FILES) ; do \
	    dest=`dirname $(distdir)/$$f` ; \
	    $(MKINSTALLDIRS) $$dest && cp -p $$f $$dest || exit 1; \
	done

# Fun with dependency tracking

$(the_lib): $(makefrag) $(stampfile) $(response)
	$(CSCOMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS) /target:library /out:$(topdir)/class/lib/Microsoft.Web.Services.dll @$(response)
	mv $(topdir)/class/lib/Microsoft.Web.Services.dll $(topdir)/class/lib/Microsoft.Web.Services.1.dll

# warning: embedded tab in the 'echo touch' line
$(makefrag): $(sourcefile)
	@echo Creating $@ ...
	@echo "HAVE_MAKEFRAG = yes" >$@.new
	@echo "$(stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
	@rm -rf $@.new

ifdef PLATFORM_CHANGE_SEPARATOR_CMD
$(response): $(sourcefile)
	@echo Creating $@ ...
	@cat $< |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif

-include $(makefrag)

ifndef HAVE_MAKEFRAG
$(stampfile):
	touch $@
endif

$(the_lib2): $(makefrag2) $(stampfile2) $(response2)
	$(CSCOMPILE) $(LIBRARY_FLAGS) $(LIB_MCS_FLAGS2) /target:library /out:$(topdir)/class/lib/Microsoft.Web.Services.dll @$(response2)
	mv $(topdir)/class/lib/Microsoft.Web.Services.dll $(topdir)/class/lib/Microsoft.Web.Services.2.dll

# warning: embedded tab in the 'echo touch' line
$(makefrag2): $(sourcefile2)
	@echo Creating $@ ...
	@echo "HAVE_MAKEFRAG = yes" >$@.new
	@echo "$(stampfile2): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
	@rm -rf $@.new

ifdef PLATFORM_CHANGE_SEPARATOR_CMD
$(response2): $(sourcefile2)
	@echo Creating $@ ...
	@cat $< |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
endif

-include $(makefrag2)

ifndef HAVE_MAKEFRAG
$(stampfile2):
	touch $@
endif

include ../../build/config.make

# for now, don't give any /lib flags or set MONO_PATH, since we
# give a full path to the assembly.

ifndef NO_TEST
$(test_lib): $(test_makefrag) $(the_lib) $(test_response) $(test_stampfile)
	$(CSCOMPILE) /target:library /out:$@ $(test_flags) @$(test_response)

$(test_response): $(test_sourcefile)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat $< |sed -e 's,^\(.\),Test/\1,' |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $< |sed -e 's,^\(.\),Test/\1,' >$@
endif

# warning: embedded tab in the 'echo touch' line
$(test_makefrag): $(test_response)
	@echo Creating $@ ...
	@echo "HAVE_TEST_MAKEFRAG = yes" >$@.new
	@echo "$(test_stampfile): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
	@rm -rf $@.new

-include $(test_makefrag)
endif

ifndef HAVE_TEST_MAKEFRAG
$(test_stampfile):
	touch $@
endif

ifndef NO_TEST
$(test_lib2): $(test_makefrag2) $(the_lib2) $(test_response2) $(test_stampfile2)
	$(CSCOMPILE) /target:library /out:$@ $(test_flags2) @$(test_response2)

$(test_response2): $(test_sourcefile2)
	@echo Creating $@ ...
ifdef PLATFORM_CHANGE_SEPARATOR_CMD
	@cat $< |sed -e 's,^\(.\),Test/\1,' |$(PLATFORM_CHANGE_SEPARATOR_CMD) >$@
else
	@cat $< |sed -e 's,^\(.\),Test/\1,' >$@
endif


# warning: embedded tab in the 'echo touch' line
$(test_makefrag2): $(test_response2)
	@echo Creating $@ ...
	@echo "HAVE_TEST_MAKEFRAG = yes" >$@.new
	@echo "$(test_stampfile2): \\" >>$@.new
	@cat $< |sed -e 's,\.cs[ \t]*$$,\.cs \\,' >>$@.new
	@cat $@.new |sed -e '$$s, \\$$,,' >$@
	@echo "	touch \$$@" >>$@
	@rm -rf $@.new

-include $(test_makefrag2)
endif
#
ifndef HAVE_TEST_MAKEFRAG
$(test_stampfile2):
	touch $@
endif
