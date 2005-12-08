thisdir = class/System.Drawing
include ../../build/rules.make
SUBDIRS = 

LIBRARY = System.Drawing.dll

LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll /nowarn:649 /nowarn:169

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -resource:Test/resources/indexed.png,indexed.png \
	 /r:System.Drawing.dll -nowarn:0618 -nowarn:219 -nowarn:169 -nowarn:1595

ifeq (net_1_1_java, $(PROFILE))
LIB_MCS_FLAGS +=				\
	-r:System.Xml.dll			\
	-r:rt.dll				\
	-r:J2SE.Helpers.dll \
	-res:Assembly\Application.ico \
	-res:Assembly\Asterisk.ico \
	-res:Assembly\Error.ico \
	-res:Assembly\Exclamation.ico \
	-res:Assembly\Hand.ico \
	-res:Assembly\Information.ico \
	-res:Assembly\Question.ico  \
	-res:Assembly\Warning.ico \
	-res:Assembly\WinLogo.ico

EXTRA_DISTFILES = 	\
	Assembly\Application.ico \
	Assembly\Asterisk.ico \
	Assembly\Error.ico \
	Assembly\Exclamation.ico \
	Assembly\Hand.ico \
	Assembly\Information.ico \
	Assembly\Question.ico  \
	Assembly\Warning.ico \
	Assembly\WinLogo.ico
endif

include ../../build/library.make

EXTRA_DISTFILES = Test/resources/indexed.png