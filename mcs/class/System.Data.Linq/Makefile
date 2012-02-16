thisdir = class/System.Data.Linq
SUBDIRS = 
include ../../build/rules.make

LIBRARY = System.Data.Linq.dll
LIB_MCS_FLAGS = \
	        /r:System.dll \
	        /r:System.Core.dll \
	        /r:System.Xml.dll \
	        /r:System.Data.dll \
	        /r:System.Runtime.Serialization.dll \
	        /resource:./src/DbLinq/Schema/Dbml/DbmlSchema.xsd,DbLinq.Schema.Dbml.DbmlSchema.xsd \
		/d:MONO_STRICT /d:MONO_DEPLOY -warn:1

ifeq (2.0, $(FRAMEWORK_VERSION))
# This is a .NET 3.5 only assembly, but built during the 2.0 build
LIB_MCS_FLAGS += -d:NET_3_5 -nowarn:1720
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = src/DbLinq/Schema/Dbml/DbmlSchema.xsd

# This is a .NET 3.5+ assembly
VALID_PROFILE := $(filter net_2_0 net_4_0 net_4_5, $(PROFILE))
ifndef VALID_PROFILE
LIBRARY_NAME = dummy-System.Data.Linq.dll
NO_INSTALL = yes
NO_SIGN_ASSEMBLY = yes
NO_TEST = yes
endif

include ../../build/library.make

dist-local: dist-default dist-DbLinq

dist-DbLinq:
	find src -name '*.cs' -o -name '*.snk' -o -name '*.xml' -o -name '*.txt' > .files ; \
	tar cTf .files - | (cd $(distdir); tar xf -) ; \
	rm .files

RSYNC_EXCLUDES = \
	--exclude '*.cache'         \
	--exclude '*.csproj.*.txt'  \
	--exclude '*.dll'           \
	--exclude '*.exe'           \
	--exclude '*.force'         \
	--exclude '*.pdb'           \
	--exclude '*.pidb'          \
	--exclude '*.suo'           \
	--exclude '*.user'          \
	--exclude '*.userprefs'     \
	--exclude '*.usertasks'     \
	--exclude '.svn' 

update-dblinq:
	rsync -avz $(RSYNC_EXCLUDES) $$HOME/Development/mono-HEAD/dblinq2007/src/ src/
	rsync -avz $(RSYNC_EXCLUDES) $$HOME/Development/mono-HEAD/dblinq2007/tests/ tests/
	rsync -avz $(RSYNC_EXCLUDES) $$HOME/Development/mono-HEAD/dblinq2007/examples/ examples/

