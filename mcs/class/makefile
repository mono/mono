all:
	@echo "You must use 'make windows' or 'make linux'."

windows:
	../nant/NAnt.exe windows

unix: linux

linux:
	../nant/NAnt.exe linux

push:
	scp lib/*.dll mono-cvs:libs

test:
	../nant/NAnt.exe test

clean:
	../nant/NAnt.exe clean
