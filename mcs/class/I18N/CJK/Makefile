thisdir = class/I18N/CJK
SUBDIRS = 
include ../../../build/rules.make

LIBRARY = I18N.CJK.dll
LIB_REFS = I18N
KEYFILE = ../../mono.pub
LOCAL_MCS_FLAGS = /unsafe /resource:big5.table /resource:gb2312.table /resource:jis.table /resource:ks.table /resource:gb18030.table /define:DISABLE_UNSAFE
TEST_LIB_REFS = I18N

TEST_RESOURCE_FILES = $(wildcard Test/texts/*.txt)
TEST_MCS_FLAGS = $(foreach r, $(TEST_RESOURCE_FILES), -resource:$(r),$(r))

EXTRA_DISTFILES = big5.table jis.table gb2312.table ks.table gb18030.table \
	README.gb18030 \
	$(TEST_RESOURCE_FILES)

include ../../../build/library.make
