CSC = csc

CSCFLAGS = /out:$(PROGRAM) /debug+ /debug:full
# CSCFLAGS = /out:$(PROGRAM)

PROGRAM = type-reflector.exe

MONO = mono

PROGRAM_FILES =  \
	ConsoleOutput.cs \
	CSharpTypeDisplayer.cs \
	IndentingTextWriter.cs \
	IndentingTypeDisplayer.cs \
	ProgramOptions.cs \
	ReflectionTypeDisplayer.cs \
	TestTypes.cs \
	TextFormatter.cs \
	TypeDisplayer.cs \
	TypeDisplayerFactory.cs \
	TypeLoader.cs \
	TypeReflectorOptions.cs \
	ExplicitTypeDisplayer.cs

all: $(PROGRAM)

$(PROGRAM) : $(PROGRAM_FILES)
	$(CSC) $(CSCFLAGS) $(PROGRAM_FILES)

test-run : $(PROGRAM)
	$(MONO) $(PROGRAM) -A $(PROGRAM) $(ARGS)

run : $(PROGRAM)
	$(MONO) $(PROGRAM) $(ARGS)

windows:
	NAnt -buildfile:type-reflector.build

linux: 
	mcs @unix.args

clean:
	rm -f *.exe *.pdb *.dll *~

linecount:
	wc -l $(PROGRAM_FILES)

