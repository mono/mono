CSC=mcs
X11R6_INCLUDE=/usr/X11R6/include
WINE_INCLUDE=/usr/local/include/wine
WINDOWS_INCLUDE=/usr/local/include/wine/windows
WINE_LIB=/usr/lib/wine
GLIB20_CFLAGS=`pkg-config --cflags glib-2.0` `pkg-config --cflags gmodule-2.0`
GLIB20_LIBS=`pkg-config --libs glib-2.0` `pkg-config --libs gmodule-2.0`
MONO_CFLAGS=`pkg-config --cflags mono`
MONO_LIBS=`pkg-config --libs mono`

all:	monostub.exe.so
# These are no longer built here:
# 	System.Windows.Forms.dll NativeWindowTest.exe \
# 	FormTest.exe Test.exe

##########################################################################
# build the mono stub application
monostub.exe.so: monostub.o monosem.o monostub.exe.spec.o monostub.exe.dbg.o
	gcc -shared -Wl,-Bsymbolic -D_REENTRANT -DWINELIB -o monostub.exe.so \
	monostub.exe.spec.o monostub.o  monosem.o monostub.exe.dbg.o \
	$(GLIB20_LIBS) $(MONO_LIBS) \
	-lgc -lwine -lntdll.dll -lm -lpthread

clean:
	rm *.o monostub.exe.dbg.c monostub.exe.spec.c monostub.exe.so

monostub.o: monostub.c
	gcc -c -I. -I$(WINE_INCLUDE) -I$(X11R6_INCLUDE) \
	$(GLIB20_CFLAGS) $(MONO_CFLAGS) -D_REENTRANT -DWINELIB \
	-g -O2 -Wall -o monostub.o monostub.c

monosem.o: monosem.c
	gcc -c -I. -I$(WINE_INCLUDE) -I$(WINDOWS_INCLUDE) -I$(X11R6_INCLUDE) \
	$(GLIB20_CFLAGS) $(MONO_CFLAGS) -D_REENTRANT -DWINELIB \
	-g -O2 -Wall -o monosem.o monosem.c

monostub.exe.tmp.o: monostub.o monosem.o
	ld -r  monostub.o monosem.o -o monostub.exe.tmp.o
	strip --strip-unneeded monostub.exe.tmp.o

monostub.exe.spec.c: monostub.exe.tmp.o
	winebuild -D -sym monostub.exe.tmp.o -o monostub.exe.spec.c \
	-exe monostub.exe -mgui -L$(WINE_LIB) -lcomdlg32 -lshell32 \
	-luser32 -lgdi32 -lkernel32

monostub.exe.spec.o: monostub.exe.spec.c
	gcc -c -I. -I. -I$(WINE_INCLUDE) -g -O2 -I$(X11R6_INCLUDE) \
	-D_REENTRANT -DWINELIB -o monostub.exe.spec.o monostub.exe.spec.c

monostub.exe.dbg.c: monostub.exe.spec.o
	winebuild -o monostub.exe.dbg.c -debug -C. monostub.c

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

