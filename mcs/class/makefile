NANT	= ../nant/NAnt.exe

all: 
	if test x$(OS) = xWindows_NT; then make windows; else make -f makefile.gnu; fi

windows:
	$(NANT)

push:
	scp lib/*.dll mono-cvs:libs

test:
	$(NANT) test

testcorlib:
	$(NANT) testcorlib

clean:
	$(NANT) clean
