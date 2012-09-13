thisdir = class/System.Data
SUBDIRS = 
include ../../build/rules.make

# I have *no idea* why I need to do this.

ifeq ($(PROFILE),atomic)
system = System.dll
else
system = $(topdir)/class/lib/$(PROFILE)/System.dll
endif

ifneq (1, $(FRAMEWORK_VERSION_MAJOR))
OTHER_LIB_MCS_FLAGS = -r:System.Configuration.dll \
			-r:System.Transactions.dll
endif

LIBRARY = System.Data.dll
LIB_MCS_FLAGS = \
	-nowarn:649 \
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
MOBILE := $(filter monotouch monodroid mobile, $(PROFILE))
ifdef MOBILE
LIB_MCS_FLAGS += \
	-r:Mono.Data.Tds.dll \
	-r:System.Transactions.dll
else
LIB_MCS_FLAGS += \
	-r:System.EnterpriseServices.dll 	\
	-r:Mono.Data.Tds.dll \
	$(OTHER_LIB_MCS_FLAGS)
endif
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -r:System.Core.dll -r:Mono.Data.Sqlite.dll -nowarn:618,169,612,219,168

TEST_MONO_PATH = .

EXTRA_DISTFILES = \
	TODO					\
	$(wildcard Test/System.Data/*.xml)		\
	$(wildcard Test/System.Data/*.xsd)		\
	$(wildcard Test/System.Xml/*.xml)		\
	$(wildcard Test/System.Xml/*.xsd)		\
	Test/System.Data/schemas/ChangeLog	\
	$(wildcard Test/System.Data/schemas/*.xsd)		\
	Mono.Data.SqlExpressions/Parser.jay	\
	app_test_net_2_0.config			\
	app_test_net_4_5.config			\
	Test/System.Data/binserialize/*.bin	\
	SqliteTest.db

BUILT_SOURCES = Mono.Data.SqlExpressions/Parser.cs

Mono.Data.SqlExpressions/Parser.cs: Mono.Data.SqlExpressions/Parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -vct < $(topdir)/jay/skeleton.cs $(CURDIR)/$< >$@
	rm -f y.output

include ../../build/library.make

$(test_lib): $(test_lib).config

$(test_lib).config: app_test_$(PROFILE).config
	cp $< $@
