thisdir = class/Commons.Xml.Relaxng
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Commons.Xml.Relaxng.dll
LIB_MCS_FLAGS = /r:$(corlib) /r:System.dll /r:System.Xml.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)
EXTRA_DISTFILES = \
	Test/ChangeLog		\
	Test/XmlFiles/ChangeLog	\
	Test/XmlFiles/SimpleElementPattern1.xml	\
	Test/XmlFiles/SimpleElementPattern1.rng	\
	Test/XmlFiles/SimpleElementPattern2.xml	\
	Test/XmlFiles/SimpleElementPattern2.rng	\
	Test/XmlFiles/team.xml	\
	Test/XmlFiles/team.rng


include ../../build/library.make
