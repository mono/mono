thisdir = class/monodoc
SUBDIRS = 
include ../../build/rules.make

LIBRARY = monodoc.dll
LIBRARY_PACKAGE = monodoc
KEYFILE = ../../class/mono.snk
# Remove a bunch of "obsolete"-type warning for Lucene.NET
# also activate legacy mode to compile old monodoc.dll api
LOCAL_MCS_FLAGS = /nowarn:618,612,672,809,414,649 /define:LEGACY_MODE

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

IMAGE_RESOURCE_COMMAND = $(foreach file,$(IMAGES),/resource:../../../external/api-doc-tools/monodoc/$(file),$(notdir $(file)))

RESOURCE_FILES = \
	Resources/base.css                \
	Resources/ecmaspec-html-css.xsl   \
	Resources/ecmaspec-html.xsl       \
	Resources/ecmaspec.css            \
	Resources/helper.js               \
	Resources/home.html               \
	Resources/Lminus.gif              \
	Resources/Lplus.gif               \
	Resources/creativecommons.png     \
	Resources/mdoc-html-format.xsl    \
	Resources/mdoc-html-utils.xsl     \
	Resources/mdoc-sections-css.xsl   \
	Resources/mdoc-sections.xsl       \
	Resources/mono-ecma-css.xsl       \
	Resources/mono-ecma-impl.xsl      \
	Resources/mono-ecma.css           \
	Resources/mono-ecma.xsl			\
	Resources/toc-html.xsl

RESOURCE_COMMAND = $(foreach file,$(RESOURCE_FILES),/resource:../../../external/api-doc-tools/monodoc/$(file),$(notdir $(file)))


EXTRA_DISTFILES = \
	monodoc.dll.config.in   \

LIB_REFS = ICSharpCode.SharpZipLib System System.Core System.Xml System.Xml.Linq System.Configuration
LIB_MCS_FLAGS = \
	/unsafe                                                           \
	/nowarn:169,164,162,168,219,618,612                               \
	/resource:../../docs/monodoc.xml,monodoc.xml                      \
	$(RESOURCE_COMMAND) \
	$(IMAGE_RESOURCE_COMMAND)

CLEAN_FILES += $(the_lib).config

TEST_MCS_FLAGS =
TEST_LIB_REFS = System System.Core System.Xml

include ../../build/library.make

all-local: $(the_lib).config

$(the_lib).config: monodoc.dll.config.in
	sed 's,@monodoc_refdir@,$(mono_libdir)/monodoc,g' monodoc.dll.config.in > $@

