# OBSOLETE!!!! NOW USING MAKEFILE
# this file builds the stub wine application that loads a mono application
# calling WINELib functions
# 
# 
#
X11R6_INCLUDE=/usr/X11R6/include
WINE_INCLUDE=/usr/local/include/wine
WINE_LIB=/usr/local/lib/wine
GLIB20_INCLUDE=/usr/include/glib-2.0
GLIB20_LIB_INCLUDE=/usr/lib/glib-2.0/include
LIBMONO=/usr/local/lib/libmono.a

# The Mono 0.13 build used at the time this code was written is missing
# some object code in the static libs. Due to conflicts between functions
# in the shared libs of Mono and Wine this stub at this point it seems
# we will need to statically link the embedded Mono engine.
#
# These are the two object files are missing in the libmono.a so link these 
# in also:
LIBMETADATA=/home/john/mono-src/mono-0.13/mono/metadata/.libs/libmetadata.al 
LIBMONORUNTIME=/home/john/mono-src/mono-0.13/mono/metadata/.libs/libmonoruntime.al

gcc -c -I. -I$WINE_INCLUDE  -g -O2 -Wall -I$X11R6_INCLUDE -o monostub.o monostub.c

gcc -c -I. -g -O2 -Wall -I$X11R6_INCLUDE $I-o monostart.o monostart.c -I$GLIB20_INCLUDE -I$GLIB20_LIB_INCLUDE -I$WINE_INCLUDE

ld -r  monostub.o -o monostub.exe.tmp.o  

strip --strip-unneeded monostub.exe.tmp.o

winebuild -sym monostub.exe.tmp.o -o monostub.exe.spec.c -exe monostub.exe -mgui -L$WINE_LIB  -lcomdlg32 -lshell32 -luser32 -lgdi32 -lkernel32

gcc -c -I. -I. -I$WINE_INCLUDE -g -O2 -I$X11R6_INCLUDE -o monostub.exe.spec.o monostub.exe.spec.c

winebuild -o monostub.exe.dbg.c -debug -C. monostub.c

gcc -c -I. -I. -I$WINE_INCLUDE  -g -O2 -I$X11R6_INCLUDE -o monostub.exe.dbg.o monostub.exe.dbg.c

gcc -shared -Wl,-Bsymbolic -o monostub.exe.so monostub.exe.spec.o monostub.o monostart.o -L$WINE_LIB -L/usr/lib -L. -lm -L/usr/local/lib -lglib-2.0 -L/usr/lib -lmono -lgc -lnsl -lrt -lgd -lgmodule-2.0  -lwine

gcc -shared -Wl,-Bsymbolic -o monostub.exe.so monostub.exe.spec.o monostub.o monostart.o -I/usr/local/include -I -I$GLIB20_INCUDE -I$GLIB20_LIB_INCLUDE -L/usr/lib $LIBMONO -lwine -lntdll.dll -lglib-2.0 -lgmodule-2.0 -lm $LIBMETADATA $LIBMONORUNTIME
