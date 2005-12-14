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
OTHER_LIB_MCS_FLAGS = -r:System.Configuration.dll
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
	$(OTHER_LIB_MCS_FLAGS)			\
	$(RESX_RES:%=/res:%)

OLEDBSTRINGS = System.Data.ProviderBase.jvm/System.Data.ProviderBase.jvm.OleDbStrings.resources
SQLCLIENTSTRINGS = System.Data.ProviderBase.jvm/System.Data.ProviderBase.jvm.SqlClientStrings.resources
SQLCOMMAND = System.Data.SqlClient.jvm/System.Data.SqlClient.SqlCommand.resources
SQLCONNECTION = System.Data.SqlClient.jvm/System.Data.SqlClient.SqlConnection.resources

RESX_RES = \
	$(OLEDBSTRINGS)	\
	$(SQLCLIENTSTRINGS)	\
	$(SQLCOMMAND)	\
	$(SQLCONNECTION)
else
LIB_MCS_FLAGS += \
	-r:System.EnterpriseServices.dll 	\
	-r:Mono.Data.Tds.dll \
	$(OTHER_LIB_MCS_FLAGS)
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -nowarn:618

EXTRA_DISTFILES = \
	TODO					\
	Test/System.Data/region.xml		\
	Test/System.Data/store.xsd		\
	Test/System.Data/own_schema.xsd		\
	Test/System.Xml/2books.xml		\
	Test/System.Xml/region.xml		\
	Test/System.Xml/region.xsd		\
	Test/System.Xml/store.xsd		\
	Mono.Data.SqlExpressions/Parser.jay	\
	app_test_2.0.config

BUILT_SOURCES = Mono.Data.SqlExpressions/Parser.cs

Mono.Data.SqlExpressions/Parser.cs: Mono.Data.SqlExpressions/Parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs $< >$@

include ../../build/library.make

$(the_lib) : $(OLEDBSTRINGS) $(SQLCLIENTSTRINGS) $(SQLCOMMAND) $(SQLCONNECTION)
$(OLEDBSTRINGS) : System.Data.ProviderBase.jvm/OleDbStrings.resx
	$(RESGEN) $< $@
$(SQLCLIENTSTRINGS) : System.Data.ProviderBase.jvm/SqlClientStrings.resx
	$(RESGEN) $< $@
$(SQLCOMMAND) : System.Data.SqlClient.jvm/SqlCommand.resx
	$(RESGEN) $< $@
$(SQLCONNECTION) : System.Data.SqlClient.jvm/SqlConnection.resx
	$(RESGEN) $< $@
ifeq (net_2_0, $(PROFILE))
$(test_lib): $(test_lib).config

$(test_lib).config: app_test_2.0.config
	cp $< $@
endif
