CSC=mcs
X11R6_INCLUDE=/usr/X11R6/include
WINE_INCLUDE=/usr/include/wine
WINE_LIB=/usr/lib/wine
GLIB20_INCLUDE=`pkg-config --cflags glib-2.0` `pkg-config --cflags gmodule-2.0`
GLIB20_LIB=`pkg-config --libs glib-2.0` `pkg-config --libs gmodule-2.0`
# GLIB20_LIB_INCLUDE=/usr/lib/glib-2.0/include
LIBMONO=/mono/lib/libmono.a

all:	monostub.exe.so System.Windows.Forms.dll NativeWindowTest.exe \
	FormTest.exe Test.exe

##########################################################################
# build the mono stub application
monostub.exe.so: monostub.o monostub.exe.spec.o monostub.exe.dbg.o
	gcc -shared -Wl,-Bsymbolic -D_REENTRANT -DWINELIB -o monostub.exe.so \
	monostub.exe.spec.o \
	monostub.o monostub.exe.dbg.o -I/usr/local/include \
	$(GLIB20_INCUDE) -L/usr/lib \
	$(LIBMONO)  -lgc \
	-lwine -lntdll.dll $(GLIB20_LIB) -lm -lpthread 

clean:	
	rm *.o monostub.exe.dbg.c monostub.exe.spec.c monostub.exe.so *.exe *.dll

monostub.o: monostub.c
	gcc -c -I. -I$(WINE_INCLUDE)  $(GLIB20_INCLUDE) -g -O2 -Wall -I$(X11R6_INCLUDE) -D_REENTRANT -DWINELIB -o monostub.o monostub.c

monostub.exe.tmp.o: monostub.o 
	ld -r  monostub.o -o monostub.exe.tmp.o  
	strip --strip-unneeded monostub.exe.tmp.o

monostub.exe.spec.c: monostub.exe.tmp.o
	winebuild -sym monostub.exe.tmp.o -o monostub.exe.spec.c -exe monostub.exe -mgui -L$(WINE_LIB)  -lcomdlg32 -lshell32 -luser32 -lgdi32 -lkernel32

monostub.exe.spec.o: monostub.exe.spec.c
	gcc -c -I. -I. -I$(WINE_INCLUDE) -g -O2 -I$(X11R6_INCLUDE) -D_REENTRANT -DWINELIB -o monostub.exe.spec.o monostub.exe.spec.c

monostub.exe.dbg.c: monostub.exe.spec.o
	winebuild -o monostub.exe.dbg.c -debug -C. monostub.c

monostub.exe.dbg.o: monostub.exe.dbg.c
	gcc -c -I. -I. -I$(WINE_INCLUDE)  -g -O2 -I$(X11R6_INCLUDE) -D_REENTRANT -DWINELIB -o monostub.exe.dbg.o monostub.exe.dbg.c

##########################################################################
# build System.Windows.Forms.dll and test/sample applications

Test.exe: Test.cs
	$(CSC) Test.cs

NativeWindowTest.exe: NativeWindowTest.cs System.Windows.Forms.dll
	$(CSC) -r System.Windows.Forms.dll NativeWindowTest.cs

FormTest.exe: FormTest.cs System.Windows.Forms.dll
	$(CSC) -r System.Windows.Forms.dll FormTest.cs

