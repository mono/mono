all: linux

linux:
	../nant/Nant.exe

push:
	scp lib/*.dll mono-cvs:libs

test:
	../nant/NAnt.exe test

clean:
	../nant/NAnt.exe clean
