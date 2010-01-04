thisdir = tools/sqlmetal
SUBDIRS = 
include ../../build/rules.make

dbmetal_src = ../../class/System.Data.Linq/src/DbMetal
ns          = DbMetal.Language

LOCAL_MCS_FLAGS = \
	-d:MONO_STRICT                                                      \
	-keyfile:$(dbmetal_src)/../DbLinq.snk                               \
	-resource:$(dbmetal_src)/Language/EnglishWords.txt,$(ns).EnglishWords.txt \
	-resource:$(dbmetal_src)/Language/FrenchWords.txt,$(ns).FrenchWords.txt   \
	-resource:$(dbmetal_src)/Language/GermanWords.txt,$(ns).GermanWords.txt   \
	-r:System.Configuration                                             \
	-r:System.Core                                                      \
	-r:System.Data                                                      \
	-r:System.Data.Linq                                                 \
	-r:System.Xml

SQLMETAL_RESOURCES = \
	$(dbmetal_src)/Language/EnglishWords.txt \
	$(dbmetal_src)/Language/FrenchWords.txt  \
	$(dbmetal_src)/Language/GermanWords.txt

SQLMETAL_TEST_FILES = \
	Test/AssemblyInfo.cs

EXTRA_DISTFILES = \
	$(SQLMETAL_RESOURCES) \
	$(SQLMETAL_TEST_FILES)

PROGRAM = sqlmetal.exe

$(PROGRAM) : Makefile $(SQLMETAL_RESOURCES)

HAS_NUNIT_TEST = yes

include ../../build/executable.make

