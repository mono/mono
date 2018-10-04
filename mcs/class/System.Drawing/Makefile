thisdir = class/System.Drawing
include ../../build/rules.make
SUBDIRS = 

LIBRARY = System.Drawing.dll

LIB_REFS = System
KEYFILE = ../msfinal.pub
LIB_MCS_FLAGS = /unsafe \
	-resource:Assembly/Mono.ico,Mono.ico -resource:Assembly/Information.ico,Information.ico \
	-resource:Assembly/Error.ico,Error.ico -resource:Assembly/Warning.ico,Warning.ico \
	-resource:Assembly/Question.ico,Question.ico -resource:Assembly/Shield.ico,Shield.ico

RESX_RESOURCE_STRING = ../../../external/corefx/src/System.Drawing.Common/src/Resources/Strings.resx

ifndef MOBILE_PROFILE
LIB_MCS_FLAGS += -d:FEATURE_TYPECONVERTER,SUPPORTS_WINDOWS_COLORS
endif

TEST_LIB_REFS = System.Drawing System.Xml

ifneq ($(PROFILE),wasm)
TEST_LIB_REFS += System.Runtime.Serialization.Formatters.Soap
endif

TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -define:TEST -resource:Test/resources/indexed.png,indexed.png \
	-nowarn:0618 -nowarn:219 -nowarn:169

TEST_RESOURCE_FILES = \
	Test/resources/indexed.png	\
	Test/System.Drawing/bitmaps/1bit.png	\
	Test/System.Drawing/bitmaps/81674-2bpp.png	\
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
	Test/System.Drawing/bitmaps/nature24bits87.gif	\
	Test/System.Drawing/bitmaps/81773-interlaced.gif	\
	Test/System.Drawing/bitmaps/nature24bits.jpg	\
	Test/System.Drawing/bitmaps/nature-greyscale.jpg	\
	Test/System.Drawing/bitmaps/smiley.ico	\
	Test/System.Drawing/bitmaps/only256.ico	\
	Test/System.Drawing/bitmaps/VisualPng1.ico	\
	Test/System.Drawing/bitmaps/VisualPng.ico	\
	Test/System.Drawing/bitmaps/16x16x16.ico	\
	Test/System.Drawing/bitmaps/32x32x16.ico	\
	Test/System.Drawing/bitmaps/48x48x1.ico	\
	Test/System.Drawing/bitmaps/64x64x256.ico	\
	Test/System.Drawing/bitmaps/96x96x256.ico	\
	Test/System.Drawing/bitmaps/32bpp.ico	\
	Test/System.Drawing/bitmaps/80509.ico	\
	Test/System.Drawing/bitmaps/323511.ico	\
	Test/System.Drawing/bitmaps/415581.ico	\
	Test/System.Drawing/bitmaps/non-inverted.bmp	\
	Test/System.Drawing/bitmaps/milkmateya01.emf	\
	Test/System.Drawing/bitmaps/telescope_01.wmf

TEST_MCS_FLAGS += $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

XTEST_LIB_REFS = System System.Core Facades/System.Threading.Tasks

include ../../build/library.make

EXTRA_DISTFILES =	\
	$(TEST_RESOURCE_FILES)	\
	Assembly/Application.ico	\
	Assembly/Asterisk.ico		\
	Assembly/Error.ico		\
	Assembly/Exclamation.ico	\
	Assembly/Hand.ico		\
	Assembly/Information.ico	\
	Assembly/Question.ico		\
	Assembly/Warning.ico		\
	Assembly/WinLogo.ico		\
	Assembly/Mono.ico		\
	Assembly/Shield.ico		\
	nunit-host-ignore-list
