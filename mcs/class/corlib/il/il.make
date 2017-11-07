IL_REPLACE = $(topdir)/class/lib/$(PROFILE_DIRECTORY)/corlib.unsafe.dll.tmp

$(topdir)/class/lib/$(PROFILE_DIRECTORY)/corlib.unsafe.dll.tmp: $(topdir)/class/corlib/System.Runtime.CompilerServices/Unsafe.il
	$(Q) $(ILASM) $< /dll /noautoinherit /out:$@

CLEAN_FILES += $(IL_REPLACE)
