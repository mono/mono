CSC = csc

CSCFLAGS = /out:$(PROGRAM) \
	/noconfig \
	/r:../../class/lib/System.dll \
	/r:../../class/lib/System.Xml.dll \
	/r:../../class/lib/System.Data.dll \
	/debug+ /debug:full 
# CSCFLAGS = /out:$(PROGRAM) \
#	/noconfig \
#	/r:../../class/lib/System.dll \
#	/r:../../class/lib/System.Xml.dll \
#	/r:../../class/lib/System.Data.dll \


PROGRAM = SqlSharpCli.exe

MONO = mono

PROGRAM_FILES =  \
	SqlSharpCli.cs

all: $(PROGRAM)

$(PROGRAM) : $(PROGRAM_FILES)
	$(CSC) $(CSCFLAGS) $(PROGRAM_FILES)

test-run : $(PROGRAM)
	$(MONO) $(PROGRAM) -A $(PROGRAM) $(ARGS)

run : $(PROGRAM)
	$(MONO) $(PROGRAM) $(ARGS)

windows:
	NAnt -buildfile:SqlSharpCli.build

linux: 
	mcs @unix.args

clean:
	rm -f *.exe *.pdb *.dbg *.dll *~

linecount:
	wc -l $(PROGRAM_FILES)

