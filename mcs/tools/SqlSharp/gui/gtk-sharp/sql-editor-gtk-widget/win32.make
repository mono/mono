# Makefile for gnome-db-sqleditor

PROJECT = sqleditor.dll

MODULES = gnome-db-sqleditor.c
OBJS = gnome-db-sqleditor.o

CFLAGS = `pkg-config --cflags gtk+-2.0`
LIBS = `pkg-config --libs gtk+-2.0`

CC = gcc -b i686-pc-mingw32

all: $(PROJECT)

$(PROJECT) : 
	$(CC) -shared -mms-bitfields -mno-cygwin -Wall $(CFLAGS) -o $(PROJECT) $(MODULES) $(LIBS)
clean:
	rm -f *.o
	rm -f $(PROJECT).exe
# -b i686-pc-mingw32 -mms-bitfields -mno-cygwin

