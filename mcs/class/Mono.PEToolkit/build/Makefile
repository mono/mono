thisdir = class/Mono.PEToolkit/build
SUBDIRS =
include ../../../build/rules.make

# ok this duplication of the source lists is just wrong. But we can't
# include both library and build/executable.make in one makefile at the
# moment, so...
#
# someday there will be a better way.

PROGRAM = mddump.exe
LOCAL_MCS_FLAGS = /unsafe

EXTRA_DISTFILES = \
	ChangeLog			\
	srcgen/a.bat		\
	srcgen/bitconv-types.xml	\
	srcgen/xslt.wsf		\
	$(wildcard srcgen/*.xsl)	\
	srcgen/data/ilasm.xml	\
	srcgen/data/md-schema.xml

include ../../../build/executable.make
