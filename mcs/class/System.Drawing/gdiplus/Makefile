thisdir := class/System.Drawing/gdiplus
SUBDIRS := 
include ../../../build/rules.make

MoreWarnings = -Wstrict-prototypes -Wmissing-prototypes -Wmissing-declarations -Wredundant-decls
LOCAL_CFLAGS=-g -Wall  `pkg-config --cflags cairo mono`
LOCAL_LDFLAGS=`pkg-config --libs cairo mono`
gdiplus = $(topdir)/class/lib/libgdiplus.dll.so
local_sources = bitmap.c \
	brush.c \
	general.c \
	graphics.c \
        graphics-path.c \
	image.c \
	matrix.c \
	pen.c \
	solidbrush.c \
	gdip_win32.c

local_objs = bitmap.o \
	brush.o \
	general.o \
	graphics.o \
	graphics-path.o \
	image.o \
	matrix.o \
	pen.o \
	solidbrush.o \
	gdip_win32.o


all-local: 
	(if pkg-config --modversion cairo; then \
		make guilibrary; \
	else \
		echo WARNING: Skipping gdiplus library compilation; \
		gcc -shared -Wl,-soname,libgdiplus.dll.so -o $(gdiplus) empty.c;  \
	fi)

clean-local:
	rm -f *.o
	rm -f $(gdiplus)
#	rm -f detect.h detect.libs detect-stamp 

install-local: all-local
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/lib; \
	if test -f $(gdiplus); then \
		$(INSTALL_LIB) $(gdiplus) $(DESTDIR)$(prefix)/lib; \
	fi

dist-local: dist-default

guilibrary: $(gdiplus)

$(gdiplus) : $(local_sources:.c=.o) 
	gcc -shared -Wl,-soname,libgdiplus.dll.so -o $(gdiplus) $(local_objs) $(LOCAL_LDFLAGS)

%.o: %.c
	$(CCOMPILE) -c -o $@ $^

test-local: 
	echo Done

test:
	echo DOne

#detect.h detect.libs: detect-stamp
#	sh detect
#
#detect-stamp: 
#	touch detect-stamp
#

