all	= jay
c	= closure.c error.c lalr.c lr0.c main.c mkpar.c output.c reader.c \
	  symtab.c verbose.c warshall.c

CFLAGS=#-g

jay:	$(c:.c=.o)	; $(CC) -o $@ $(CFLAGS) $(c:.c=.o)

clean:
	rm -f jay *.o

windows: jay

linux: jay

install:
	make -f makefile.gnu install
