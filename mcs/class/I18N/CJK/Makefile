thisdir = class/I18N/CJK
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = I18N.CJK.dll
LOCAL_MCS_FLAGS = /unsafe /resource:big5.table /resource:gb2312.table /resource:jis.table /r:$(corlib) /r:I18N.dll
NO_TEST = yes

EXTRA_DISTFILES = big5.table jis.table gb2312.table

include ../../../build/library.make
