thisdir = tools/monkeydoc
SUBDIRS = 
include ../../build/rules.make

LIBRARY = monkeydoc.dll
LIBRARY_PACKAGE = monkeydoc
# Remove a bunch of "obsolete"-type warning for Lucene.NET
LOCAL_MCS_FLAGS = /nowarn:618,612,672,809

JAY_FLAGS = -ct

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
	Resources/mono-ecma.xsl			\
	Resources/toc-html.xsl           \
	$(IMAGES)

EXTRA_DISTFILES = \
	monkeydoc.dll.config.in   \
	$(RESOURCE_FILES)

LIB_MCS_FLAGS = \
	/unsafe                                                           \
	/codepage:utf8                                                    \
	/nowarn:169,164,162,168,219,618,612                               \
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
	/resource:Resources/toc-html.xsl,toc-html.xsl                     \
	$(IMAGE_RESOURCE_COMMAND)					                      \
	/r:ICSharpCode.SharpZipLib                                        \
	/r:$(corlib)                                                      \
	/r:System.dll                                                     \
	/r:System.Core.dll                                                \
	/r:System.Xml.dll                                                 \
	/r:System.Xml.Linq.dll                                            \
	/r:System.Configuration.dll

TEST_MCS_FLAGS = /r:System.Core.dll

DOC_SOURCE_DIRS = \
	../../docs \
	../../../docs

DOC_SOURCES = $(foreach dir,$(DOC_SOURCE_DIRS),$(wildcard $(dir)/*.source $(dir)/*.tree $(dir)/*.zip))

include ../../build/library.make

$(the_lib): Makefile $(RESOURCE_FILES)

all-local: $(the_lib).config Monkeydoc.Ecma/EcmaUrlParser.cs

test-local: setup-doc-sources

$(the_lib).config: Makefile monkeydoc.dll.config.in
	sed 's,@monodoc_refdir@,$(mono_libdir)/monodoc,g' monkeydoc.dll.config.in > $@

Monkeydoc.Ecma/EcmaUrlParser.cs: Monkeydoc.Ecma/EcmaUrlParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay $(JAY_FLAGS) < $(topdir)/jay/skeleton.cs $< > jay-tmp.out && mv jay-tmp.out $@

parser.exe: Monkeydoc.Ecma/EcmaUrlParser.cs Monkeydoc.Ecma/EcmaUrlTokenizer.cs Monkeydoc.Ecma/EcmaUrlParserDriver.cs Monkeydoc.Ecma/EcmaDesc.cs
	mcs /out:$@ /debug $^

setup-doc-sources: $(DOC_SOURCES)
	mkdir -p ./Test/monodoc/sources/
	cp $(DOC_SOURCES) ./Test/monodoc/sources/
