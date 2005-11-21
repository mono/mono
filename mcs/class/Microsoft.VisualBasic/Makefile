thisdir = class/Microsoft.VisualBasic

SUBDIRS = Test/standalone

include ../../build/rules.make

LIBRARY = Microsoft.VisualBasic.dll
LIBRARY_NEEDS_POSTPROCESSING = yes

# UTF-8
CODEPAGE = 65001

LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Windows.Forms.dll @Microsoft.VisualBasic.dll.resources
TEST_MCS_FLAGS = -nowarn:0618 -nowarn:219 -nowarn:169

LIB_MCS_FLAGS_RESOURCES = $(LIB_MCS_FLAGS) 

TXT_RES = Microsoft.VisualBasic.VBUtils.resources
#RESX_RES = 

with_mono_path = MONO_PATH="$(topdir)/class/lib/$(PROFILE)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" 

ifeq ($(PROFILE), default)
bootstrap_profile = net_1_1_bootstrap
else
bootstrap_profile = $(PROFILE)_bootstrap
endif

ilasm = $(topdir)/class/lib/$(bootstrap_profile)/ilasm.exe
ILASM = MONO_PATH="$(topdir)/class/lib/$(bootstrap_profile)$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(ilasm)

the_il = fixup/$(PROFILE)/Microsoft.VisualBasic.il

EXTRA_DISTFILES = fixup/fixup.pl Microsoft.VisualBasic.dll.resources $(TXT_RES:.resources=.txt) #$(RESX_RES:.resources=.resx)
CLEAN_FILES = $(the_il) $(TXT_RES) #$(RESX_RES)

include ../../build/library.make

$(build_lib): $(TXT_RES) #$(RESX_RES)

$(TXT_RES): %.resources: %.txt
	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

#$(RESX_RES): %.resources: %.resx
#	$(RESGEN) `echo $< | $(PLATFORM_CHANGE_SEPARATOR_CMD)`

$(the_lib): $(the_il)
	$(ILASM) $(IL_FLAGS) /out:$@ /dll $<
	$(SN) $(SNFLAGS) $@ $(topdir)/class/mono.snk

$(the_il): $(build_lib) fixup/fixup.pl
	$(with_mono_path) $(ILDISASM) $< > fixup/$(PROFILE)/t1
	$(PERL) fixup/fixup.pl fixup/$(PROFILE)/t1 > fixup/$(PROFILE)/t2
	rm -f fixup/$(PROFILE)/t1
	mv fixup/$(PROFILE)/t2 $@
