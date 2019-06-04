thisdir = class/corlib/il

include ../../../build/rules.make
include il.make

clean-local:

TMP_MSCORLIB = $(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.input

all-local: $(IL_REPLACE)
	cp $(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll $(TMP_MSCORLIB)
	$(Q) $(STRING_REPLACER) --ilreplace=$(IL_REPLACE) $(TMP_MSCORLIB)
	mv $(TMP_MSCORLIB) $(topdir)/class/lib/$(PROFILE_DIRECTORY)/mscorlib.dll

install-local:
