API_INFO = $(MONO_PATH) $(RUNTIME) $(topdir)/class/lib/$(PROFILE)/mono-api-info.exe
API_DIFF = $(MONO_PATH) $(RUNTIME) $(topdir)/tools/corcompare/mono-api-diff.exe
TRANSFORM = $(MONO_PATH) $(RUNTIME) $(topdir)/tools/corcompare/transform.exe

OBJECTS = deploy/$(LIBRARY_NAME:.dll=.html)

corcompare: $(OBJECTS)

$(OBJECTS): $(patsubst deploy/%.html,%.src, $(OBJECTS))

.PRECIOUS: deploy/%.html
deploy/%.html: %.src
	$(TRANSFORM) $< $(topdir)/build/corcompare-api.xsl source-name=$(notdir $<) > $@

.PRECIOUS: %.src
%.src: %.xml
	$(API_DIFF) masterinfos/$(PROFILE)/$(notdir $<) $< > $@ || (rm -rf $@ && exit 1)

.PRECIOUS: %.xml
%.xml: $(topdir)/class/lib/$(PROFILE)/%.dll
	$(API_INFO) $< > $@ || (rm -f $@ && exit 1)

CLEAN_FILES += deploy/*.html $(LIBRARY_NAME:.dll=.src) $(LIBRARY_NAME:.dll=.xmlsrc)


