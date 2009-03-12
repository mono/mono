thisdir = tools/sqlmetal
SUBDIRS = 
include ../../build/rules.make

dbmetal_src = ../../class/System.Data.Linq/src/DbMetal

LOCAL_MCS_FLAGS = \
	-d:MONO_STRICT                                                      \
	-keyfile:$(dbmetal_src)/../DbLinq.snk                               \
	-resource:$(dbmetal_src)/Language/EnglishWords.txt,EnglishWords.txt \
	-resource:$(dbmetal_src)/Language/FrenchWords.txt,FrenchWords.txt   \
	-resource:$(dbmetal_src)/Language/GermanWords.txt,GermanWords.txt   \
	-r:System.Configuration                                             \
	-r:System.Core                                                      \
	-r:System.Data                                                      \
	-r:System.Data.Linq                                                 \
	-r:System.Xml

SQLMETAL_RESOURCES = \
	$(dbmetal_src)/Language/EnglishWords.txt \
	$(dbmetal_src)/Language/FrenchWords.txt  \
	$(dbmetal_src)/Language/GermanWords.txt

EXTRA_DISTFILES = \
	$(SQLMETAL_RESOURCES)

PROGRAM = sqlmetal.exe

$(PROGRAM) : $(SQLMETAL_RESOURCES)

include ../../build/executable.make

$(PROGRAM) : Makefile

