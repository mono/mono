thisdir = tools/moon
SUBDIRS =
include ../../build/rules.make

sn = $(topdir)/class/lib/net_1_1_bootstrap/sn.exe
SN = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(sn)
SNFLAGS = -q -R

LINKER = $(topdir)/class/lib/net_1_1/monolinker.exe

TUNER_DESCS =	\
	Descriptors/mscorlib.xml	\
	Descriptors/smcs.xml	\
	Descriptors/System.xml	\
	Descriptors/System.ServiceModel.xml

TUNER_LIBRARIES =	\
	mscorlib	\
	System	\
	System.Net	\
	System.Core	\
	System.Xml	\
	System.Xml.Linq	\
	System.Runtime.Serialization	\
	System.ServiceModel	\
	System.ServiceModel.Web	\
	Mono.CompilerServices.SymbolWriter

TUNER_MASTERS = $(TUNER_LIBRARIES:%=masterinfos/%.info)

DISTFILES = $(TUNER_DESCS) $(TUNER_MASTERS)

WORKING_DIR = $(topdir)/class/lib/net_2_1_raw
OUTPUT_DIR = $(topdir)/class/lib/net_2_1

DLLFILES = $(TUNER_LIBRARIES:%=%.dll)

FILES = smcs.exe $(DLLFILES)
WORKING_FILES = $(FILES:%=$(WORKING_DIR)/%)
OUTPUT_FILES = $(FILES:%=$(OUTPUT_DIR)/%)

all-local: $(OUTPUT_FILES)

install-local: end-install

clean-local:
	rm -f $(OUTPUT_FILES) tune.stamp tune.stampt

uninstall-local:

dist-local: dist-default

test-local run-test-local run-test-ondotnet-local:

TUNER_FLAGS = -d $(WORKING_DIR) -o $(OUTPUT_DIR)	\
	-l none		\
	-c link		\
	-a smcs		\
	-b true		\
	-g true		\
	-m display_internalized false

TUNER_STEPS = \
	Mono.Tuner.InjectAttributes,Mono.Tuner:OutputStep	\
	Mono.Tuner.AdjustVisibility,Mono.Tuner:OutputStep	\
	Mono.Tuner.PrintStatus,Mono.Tuner:OutputStep	\
	Mono.Tuner.RemoveSerialization,Mono.Tuner:OutputStep

ifdef CHECK_VISIBILITY
TUNER_STEPS += Mono.Tuner.CheckVisibility,Mono.Tuner
endif

$(OUTPUT_FILES): tune.stamp
	test -f $@ || { rm -f $<; $(MAKE) $<; }

tune.stamp: $(WORKING_FILES) $(OUTPUT_DIR)/.stamp
	touch $@t
	MONO_PATH=".$(PLATFORM_PATH_SEPARATOR)$(topdir)/class/lib/net_1_1$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) --debug $(LINKER) $(TUNER_FLAGS) $(TUNER_DESCS:%=-x %) $(TUNER_STEPS:%=-s %) $(TUNER_MASTERS:%=-i %)
	for i in $(DLLFILES); do $(SN) $(SNFLAGS) $(OUTPUT_DIR)/$$i $(topdir)/class/mono.snk; done
	mv $@t $@

gacutil = $(topdir)/class/lib/net_1_1_bootstrap/gacutil.exe
GACUTIL = MONO_PATH="$(topdir)/class/lib/net_1_1_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(RUNTIME_FLAGS) $(gacutil) -bootstrap

TUNER_INSTALL_DIR = $(DESTDIR)$(mono_libdir)/mono/2.1

end-install:	\
	inst-mscorlib.dll	\
	inst-smcs.exe	\
	gac-System	\
	gac-System.Net	\
	gac-System.Xml	\
	gac-System.Xml.Linq	\
	gac-System.Core	\
	gac-System.Runtime.Serialization	\
	gac-System.ServiceModel	\
	gac-System.ServiceModel.Web	\
	gac-Mono.CompilerServices.SymbolWriter

inst-%: $(OUTPUT_DIR)/%
	$(MKINSTALLDIRS) $(TUNER_INSTALL_DIR)
	$(INSTALL_LIB) $(OUTPUT_DIR)/$* $(TUNER_INSTALL_DIR)/$*
	test ! -f $(OUTPUT_DIR)/$*.mdb || $(INSTALL_LIB) $(OUTPUT_DIR)/$*.mdb $(TUNER_INSTALL_DIR)/$*.mdb

gac-%: $(OUTPUT_DIR)/%.dll
	$(GACUTIL) /i $(OUTPUT_DIR)/$*.dll /f /root $(DESTDIR)$(mono_libdir) /package 2.1

retune:
	rm -f tune.stamp && $(MAKE) tune.stamp
