CSC=mcs
X11R6_INCLUDE=/usr/X11R6/include
WINE_INCLUDE=/usr/include/wine
WINDOWS_INCLUDE=/usr/local/include/wine/windows
WINE_LIB=/usr/lib/wine
GLIB20_CFLAGS=`pkg-config --cflags glib-2.0` `pkg-config --cflags gmodule-2.0`
GLIB20_LIBS=`pkg-config --libs glib-2.0` `pkg-config --libs gmodule-2.0`
MONO_CFLAGS=`pkg-config --cflags mono`
MONO_LIBS=`pkg-config --libs monoold`
COMPILE_OPTIONS=-g -O2 -Wall
#COMPILE_OPTIONS=-gstabs+ -Wall

all:	monostub.exe.so monowinehooks.so
# These are no longer built here:
# 	System.Windows.Forms.dll NativeWindowTest.exe \
# 	FormTest.exe Test.exe

##########################################################################
# build the mono stub application
monostub.exe.so: monostub.o monosem.o monogc.o monostub.exe.spec.o monostub.exe.dbg.o
	gcc -shared -Wl,-Bsymbolic -D_REENTRANT -DWINELIB -o monostub.exe.so \
	monostub.exe.spec.o monostub.o monosem.o monogc.o monostub.exe.dbg.o \
	$(GLIB20_LIBS) $(MONO_LIBS) \
	-lgc -lwine -lntdll.dll -lm -lpthread

monowinehooks.so: monowinehooks.o
	gcc -shared -W1,soname,monowinehooks.so -o monowinehooks.so monowinehooks.o -lc -ldl

monowinehooks.o: monowinehooks.c
	gcc -fPIC -I. -I$(WINE_INCLUDE) -I$(X11R6_INCLUDE) -rdynamic -g -c -Wall monowinehooks.c

clean:
	rm *.o monostub.exe.dbg.c monostub.exe.spec.c monostub.exe.so monowinehooks.so

monostub.o: monostub.c
	gcc -c -I. -I$(WINE_INCLUDE) -I$(X11R6_INCLUDE) \
	$(GLIB20_CFLAGS) $(MONO_CFLAGS) -D_REENTRANT -DWINELIB \
	$(COMPILE_OPTIONS) -o monostub.o monostub.c

monosem.o: monosem.c
	gcc -c -I. -I$(WINE_INCLUDE) -I$(WINDOWS_INCLUDE) -I$(X11R6_INCLUDE) \
	$(GLIB20_CFLAGS) $(MONO_CFLAGS) -D_REENTRANT -DWINELIB \
	$(COMPILE_OPTIONS) -o monosem.o monosem.c

monogc.o: monogc.c
	gcc -c -I. -I$(WINE_INCLUDE) -I$(WINDOWS_INCLUDE) -I$(X11R6_INCLUDE) \
	$(GLIB20_CFLAGS) $(MONO_CFLAGS) -D_REENTRANT -DWINELIB \
	$(COMPILE_OPTIONS) -o monogc.o monogc.c

monostub.exe.tmp.o: monostub.o monosem.o monogc.o
	ld -r  monostub.o monosem.o monogc.o -o monostub.exe.tmp.o
	strip --strip-unneeded monostub.exe.tmp.o

monostub.exe.spec.c: monostub.exe.tmp.o
	winebuild -D -sym monostub.exe.tmp.o -o monostub.exe.spec.c \
	--exe monostub.exe -mgui -L$(WINE_LIB) -lcomdlg32 -lshell32 \
	-luser32 -lgdi32 -lkernel32

monostub.exe.spec.o: monostub.exe.spec.c
	gcc -c -I. -I. -I$(WINE_INCLUDE) -g -O2 -I$(X11R6_INCLUDE) \
	-D_REENTRANT -DWINELIB -o monostub.exe.spec.o monostub.exe.spec.c

monostub.exe.dbg.c: monostub.exe.spec.o
	winebuild -o monostub.exe.dbg.c --debug -C. monostub.c

monostub.exe.dbg.o: monostub.exe.dbg.c
	gcc -c -I. -I. -I$(WINE_INCLUDE)  -g -O2 -I$(X11R6_INCLUDE) \
	-D_REENTRANT -DWINELIB -o monostub.exe.dbg.o monostub.exe.dbg.c

##########################################################################
# build System.Windows.Forms.dll and test/sample applications

Test.exe: Test.cs
	$(CSC) Test.cs

NativeWindowTest.exe: NativeWindowTest.cs System.Windows.Forms.dll
	$(CSC) -r System.Windows.Forms.dll NativeWindowTest.cs

FormTest.exe: FormTest.cs System.Windows.Forms.dll
	$(CSC) -r System.Windows.Forms.dll FormTest.cs

