thisdir = class/System.Drawing
include ../../build/rules.make
SUBDIRS = 

LIBRARY = System.Drawing.dll

LIB_MCS_FLAGS = /unsafe /r:$(corlib) /r:System.dll /nowarn:649 /nowarn:169 \
	-resource:mono.ico,mono.ico -resource:information.ico,information.ico \
	-resource:error.ico,error.ico -resource:warning.ico,warning.ico \
	-resource:question.ico,question.ico

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -define:TEST -resource:Test/resources/indexed.png,indexed.png \
	 /r:System.Drawing.dll -nowarn:0618 -nowarn:219 -nowarn:169 -nowarn:1595

ifeq (net_1_1_java, $(PROFILE))
LIB_MCS_FLAGS +=				\
	-r:System.Xml.dll			\
	-r:rt.dll				\
	-r:J2SE.Helpers.dll
endif

include ../../build/library.make

EXTRA_DISTFILES = Test/resources/indexed.png	\
	Test/System.Drawing/bitmaps/1bit.png	\
	Test/System.Drawing/bitmaps/4bit.png	\
	Test/System.Drawing/bitmaps/almogaver1bit.bmp	\
	Test/System.Drawing/bitmaps/almogaver24bits1.bmp	\
	Test/System.Drawing/bitmaps/almogaver24bits.bmp	\
	Test/System.Drawing/bitmaps/almogaver32bits.bmp	\
	Test/System.Drawing/bitmaps/almogaver32bits.tif	\
	Test/System.Drawing/bitmaps/almogaver8bits.bmp	\
	Test/System.Drawing/bitmaps/almogaver-os2.bmp	\
	Test/System.Drawing/bitmaps/maketransparent.bmp	\
	Test/System.Drawing/bitmaps/nature24bits.gif	\
	Test/System.Drawing/bitmaps/nature24bits.jpg	\
	Test/System.Drawing/bitmaps/smiley.ico	\
	Test/System.Drawing/bitmaps/VisualPng1.ico	\
	Test/System.Drawing/bitmaps/VisualPng.ico	\
	error.ico \
	information.ico	\
	mono.ico	\
	question.ico	\
	warning.ico
