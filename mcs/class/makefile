all:
	@echo "You must use 'make windows' or 'make linux'."

windows:
	../nant/NAnt.exe windows

linux:
	../nant/NAnt.exe linux

push:
	scp lib/*.dll mono-cvs:libs

clean:
	../nant/NAnt.exe clean
