gcc -c -I. -I. -I/usr/local/include/wine  -g -O2 -Wall -I/usr/X11R6/include -o monostub.o monostub.c

gcc -c -I. -I. -I/usr/local/include/mono  -g -O2 -Wall -I/usr/X11R6/include -o monostart.o monostart.c  -I/usr/include/glib-2.0 -I/usr/lib/glib-2.0/include -I/usr/local/include/wine

ld -r  monostub.o -o monostub.exe.tmp.o  

strip --strip-unneeded monostub.exe.tmp.o

winebuild -sym monostub.exe.tmp.o -o monostub.exe.spec.c -exe monostub.exe -mgui -L/usr/local/lib/wine  -lcomdlg32 -lshell32 -luser32 -lgdi32 -lkernel32

gcc -c -I. -I. -I/usr/local/include/wine -g -O2 -I/usr/X11R6/include -o monostub.exe.spec.o monostub.exe.spec.c

winebuild -o monostub.exe.dbg.c -debug -C. monostub.c

gcc -c -I. -I. -I/usr/local/include/wine  -g -O2 -I/usr/X11R6/include -o monostub.exe.dbg.o monostub.exe.dbg.c

gcc -shared -Wl,-Bsymbolic -o monostub.exe.so monostub.exe.spec.o monostub.o monostart.o -L/usr/local/lib/wine -L/usr/lib -L. -lm -L/usr/local/lib -lglib-2.0 -L/usr/lib -lmono -lgc -lnsl -lrt -lgd -lgmodule-2.0  -lwine

gcc -shared -Wl,-Bsymbolic -o monostub.exe.so monostub.exe.spec.o monostub.o monostart.o -I/usr/local/include -I/usr/local/include/wine -I/usr/include/glib-2.0 -I/usr/lib/glib-2.0/include -L/usr/lib /usr/local/lib/libmono.a /home/john/mono-src/mono-0.13/mono/metadata/.libs/libmetadata.al /home/john/mono-src/mono-0.13/mono/metadata/.libs/libmonoruntime.al  -lwine -lntdll.dll -lglib-2.0 -lgmodule-2.0 -lm
