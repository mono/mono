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
	-publicsign

LIB_REFS = System.Configuration System.Core System.Data System.Data.Linq System.Xml System
	
SQLMETAL_RESOURCES = \
	$(dbmetal_src)/Language/EnglishWords.txt \
	$(dbmetal_src)/Language/FrenchWords.txt  \
	$(dbmetal_src)/Language/GermanWords.txt

EXTRA_DISTFILES = \
	$(SQLMETAL_RESOURCES)

PROGRAM = sqlmetal.exe

$(PROGRAM) : Makefile $(SQLMETAL_RESOURCES)

include ../../build/executable.make

