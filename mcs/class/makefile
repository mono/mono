all: linux

linux:
	../nant/NAnt.exe

push:
	scp lib/*.dll mono-cvs:libs

test:
	../nant/NAnt.exe test

clean:
	../nant/NAnt.exe clean
