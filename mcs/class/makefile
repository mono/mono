NANT	= ../nant/NAnt.exe

all: linux

linux:
	$(NANT)

push:
	scp lib/*.dll mono-cvs:libs

test:
	$(NANT) test

clean:
	$(NANT) clean
