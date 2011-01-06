thisdir = tools/monodoc
SUBDIRS = 
include ../../build/rules.make

LIBRARY = monodoc.dll
LIBRARY_PACKAGE = monodoc

IMAGES = \
	Resources/images/bc_bg.png		\
	Resources/images/bc_separator.png	\
	Resources/images/error.png		\
	Resources/images/hatch.png		\
	Resources/images/headerbg.png		\
	Resources/images/help.png		\
	Resources/images/house.png		\
	Resources/images/members.png		\
	Resources/images/namespace.png		\
	Resources/images/privclass.png		\
	Resources/images/privdelegate.png	\
	Resources/images/privenumeration.png	\
	Resources/images/privevent.png		\
	Resources/images/privextension.png	\
	Resources/images/privfield.png		\
	Resources/images/privinterface.png	\
	Resources/images/privmethod.png		\
	Resources/images/privproperty.png	\
	Resources/images/privstructure.png	\
	Resources/images/protclass.png		\
	Resources/images/protdelegate.png	\
	Resources/images/protenumeration.png	\
	Resources/images/protevent.png		\
	Resources/images/protextension.png	\
	Resources/images/protfield.png		\
	Resources/images/protinterface.png	\
	Resources/images/protmethod.png		\
	Resources/images/protproperty.png	\
	Resources/images/protstructure.png	\
	Resources/images/pubclass.png		\
	Resources/images/pubdelegate.png	\
	Resources/images/pubenumeration.png	\
	Resources/images/pubevent.png		\
	Resources/images/pubextension.png	\
	Resources/images/pubfield.png		\
	Resources/images/pubinterface.png	\
	Resources/images/pubmethod.png		\
	Resources/images/pubproperty.png	\
	Resources/images/pubstructure.png	\
	Resources/images/reference.png		\
	Resources/images/treebg.png		

IMAGE_RESOURCE_COMMAND = $(foreach file,$(IMAGES),/resource:$(file),$(notdir $(file)))

RESOURCE_FILES = \
	../../docs/monodoc.xml            \
	Resources/base.css                \
	Resources/ecmaspec-html-css.xsl   \
	Resources/ecmaspec-html.xsl       \
	Resources/ecmaspec.css            \
	Resources/helper.js               \
	Resources/home.html               \
	Resources/Lminus.gif              \
	Resources/Lplus.gif               \
	Resources/mdoc-html-format.xsl    \
	Resources/mdoc-html-utils.xsl     \
	Resources/mdoc-sections-css.xsl   \
	Resources/mdoc-sections.xsl       \
	Resources/mono-ecma-css.xsl       \
	Resources/mono-ecma-impl.xsl      \
	Resources/mono-ecma.css           \
	Resources/mono-ecma.xsl		\
	$(IMAGES)

MAN_TEST_FILES = \
	Test/man-provider-test.cs   \
	Test/man-test1.1            \
	Test/man-test1.html         \
	Test/man-test2.1            \
	Test/man-test2.html         \
	Test/man-test3.1            \
	Test/man-test3.html         \
	Test/man-test4.1            \
	Test/man-test4.html

EXTRA_DISTFILES = \
	monodoc.dll.config.in   \
	$(RESOURCE_FILES)       \
	$(MAN_TEST_FILES)

LIB_MCS_FLAGS = \
	/codepage:utf8                                                    \
	/nowarn:169,164,162,168,219                                       \
	/r:Commons.Xml.Relaxng                                            \
	/resource:../../docs/monodoc.xml,monodoc.xml                      \
	/resource:Resources/base.css,base.css                             \
	/resource:Resources/ecmaspec-html-css.xsl,ecmaspec-html-css.xsl   \
	/resource:Resources/ecmaspec-html.xsl,ecmaspec-html.xsl           \
	/resource:Resources/ecmaspec.css,ecmaspec.css                     \
	/resource:Resources/helper.js,helper.js                           \
	/resource:Resources/home.html,home.html                           \
	/resource:Resources/Lminus.gif,Lminus.gif                         \
	/resource:Resources/Lplus.gif,Lplus.gif                           \
	/resource:Resources/mdoc-html-format.xsl,mdoc-html-format.xsl     \
	/resource:Resources/mdoc-html-utils.xsl,mdoc-html-utils.xsl       \
	/resource:Resources/mdoc-sections-css.xsl,mdoc-sections-css.xsl   \
	/resource:Resources/mdoc-sections.xsl,mdoc-sections.xsl           \
	/resource:Resources/mono-ecma-css.xsl,mono-ecma-css.xsl           \
	/resource:Resources/mono-ecma-impl.xsl,mono-ecma-impl.xsl         \
	/resource:Resources/mono-ecma.css,mono-ecma.css                   \
	/resource:Resources/mono-ecma.xsl,mono-ecma.xsl                   \
	$(IMAGE_RESOURCE_COMMAND)					  \
	/r:ICSharpCode.SharpZipLib                                        \
	/r:$(corlib)                                                      \
	/r:System.dll                                                     \
	/r:System.Core.dll                                                \
	/r:System.Web                                                     \
	/r:System.Web.Services                                            \
	/r:System.Xml.dll

include ../../build/library.make

$(the_lib): Makefile $(RESOURCE_FILES)

all-local: $(the_lib).config

$(the_lib).config: Makefile
	sed 's,@monodoc_refdir@,$(mono_libdir)/monodoc,g' monodoc.dll.config.in > $@

test: Test/man-provider-test.exe

run-test-local: check-man-provider

run-test-update: check-man-provider-update

Test/man-provider-test.exe: Test/man-provider-test.cs $(the_lib)
	$(CSCOMPILE) -target:exe -out:$@ $< /r:$(the_lib)

my_mono = \
	MONO_REGISTRY_PATH="$(HOME)/.mono/registry" \
	$(TEST_RUNTIME) \
	$(RUNTIME_FLAGS)

check-man-provider: Test/man-provider-test.exe
	$(my_mono) $< Test/man-test1.1 | diff - Test/man-test1.html
	$(my_mono) $< Test/man-test2.1 | diff - Test/man-test2.html
	$(my_mono) $< Test/man-test3.1 | diff - Test/man-test3.html
	$(my_mono) $< Test/man-test4.1 | diff - Test/man-test4.html

check-man-provider-update: Test/man-provider-test.exe
	$(my_mono) $< Test/man-test1.1 > Test/man-test1.html
	$(my_mono) $< Test/man-test2.1 > Test/man-test2.html
	$(my_mono) $< Test/man-test3.1 > Test/man-test3.html
	$(my_mono) $< Test/man-test4.1 > Test/man-test4.html

clean-local: cleanup

cleanup:
	-rm -f Test/man-provider-test.exe*

