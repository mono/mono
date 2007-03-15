thisdir = class/System.Data
SUBDIRS = 
include ../../build/rules.make

# I have *no idea* why I need to do this.

ifeq ($(PROFILE),atomic)
system = System.dll
else
system = $(topdir)/class/lib/$(PROFILE)/System.dll
endif

ifeq (net_2_0, $(PROFILE))
OTHER_LIB_MCS_FLAGS = -r:System.Configuration.dll \
			-r:System.Transactions.dll
endif

LIBRARY = System.Data.dll
LIB_MCS_FLAGS = \
	-nowarn:649 -nowarn:169 -nowarn:219 -nowarn:168 -nowarn:1595 \
	-unsafe \
	-r:$(corlib)				\
	-r:$(system)			 	\
	-r:System.Xml.dll 			\


ifeq (net_1_1_java, $(PROFILE))
OTHER_RES = $(RESOURCE_FILES)
LIB_MCS_FLAGS +=	\
	-r:rt.dll				\
	-r:J2SE.Helpers.dll			\
	$(OTHER_LIB_MCS_FLAGS)

else
LIB_MCS_FLAGS += \
	-r:System.EnterpriseServices.dll 	\
	-r:Mono.Data.Tds.dll \
	$(OTHER_LIB_MCS_FLAGS)
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:618

TEST_MONO_PATH = .

EXTRA_DISTFILES = \
	TODO					\
	Test/System.Data/region.xml		\
	Test/System.Data/store.xsd		\
	Test/System.Data/own_schema.xsd		\
	Test/System.Xml/2books.xml		\
	Test/System.Xml/region.xml		\
	Test/System.Xml/region.xsd		\
	Test/System.Xml/store.xsd		\
	Test/System.Data/schemas/bug77248.xsd	\
	Test/System.Data/schemas/ChangeLog	\
	Test/System.Data/schemas/test001.xsd	\
	Test/System.Data/schemas/test002.xsd	\
	Test/System.Data/schemas/test003.xsd	\
	Test/System.Data/schemas/test004.xsd	\
	Test/System.Data/schemas/test005.xsd	\
	Test/System.Data/schemas/test006.xsd	\
	Test/System.Data/schemas/test007.xsd	\
	Test/System.Data/schemas/test008.xsd	\
	Test/System.Data/schemas/test009.xsd	\
	Test/System.Data/schemas/test010.xsd	\
	Test/System.Data/schemas/test011.xsd	\
	Test/System.Data/schemas/test012.xsd	\
	Test/System.Data/schemas/test013.xsd	\
	Test/System.Data/schemas/test014.xsd	\
	Test/System.Data/schemas/test015.xsd	\
	Test/System.Data/schemas/test016.xsd	\
	Test/System.Data/schemas/test017.xsd	\
	Test/System.Data/schemas/test101.xsd	\
	Test/System.Data/schemas/test102.xsd	\
	Test/System.Data/schemas/test103.xsd	\
	Test/System.Data/schemas/Items.xsd	\
	Test/System.Data/TypedDataSet.xml	\
	Mono.Data.SqlExpressions/Parser.jay	\
	app_test_2.0.config

BUILT_SOURCES = Mono.Data.SqlExpressions/Parser.cs

Mono.Data.SqlExpressions/Parser.cs: Mono.Data.SqlExpressions/Parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs $< >$@

include ../../build/library.make

ifeq (net_2_0, $(PROFILE))
$(test_lib): $(test_lib).config

$(test_lib).config: app_test_2.0.config
	cp $< $@
endif
