thisdir = class/I18N/CJK
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = I18N.CJK.dll
LIBRARY_USE_INTERMEDIATE_FILE = yes
LOCAL_MCS_FLAGS = /unsafe /resource:big5.table /resource:gb2312.table /resource:jis.table /resource:ks.table /resource:gb18030.table /r:$(corlib) /r:I18N.dll /define:DISABLE_UNSAFE

EXTRA_DISTFILES = big5.table jis.table gb2312.table ks.table gb18030.table \
	README.gb18030 \
	$(wildcard Test/texts/*.txt)

include ../../../build/library.make
