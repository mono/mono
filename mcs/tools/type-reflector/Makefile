CSC = csc

TRACE_ARGS = /out:$(PROGRAM) /d:TRACE /d:DEBUG

# CSCFLAGS = $(TRACE_ARGS) /debug+ /debug:full
CSCFLAGS = $(TRACE_ARGS)

PROGRAM = type-reflector.exe

MONO = mono

PROGRAM_FILES =  \
	ConsoleOutput.cs \
	CSharpNodeFormatter.cs \
	DefaultNodeFormatter.cs \
	ExplicitNodeFinder.cs \
	Factories.cs \
  GroupingNodeFinder.cs \
	IndentingTextWriter.cs \
	INodeFinder.cs \
	INodeFormatter.cs \
	LanguageNodeFormatter.cs \
	Node.cs \
	NodeFinder.cs \
	NodeFormatter.cs \
  NodeGrouper.cs \
	NodeInfo.cs \
	NodeTypes.cs \
	ProgramOptions.cs \
  ReflectionNodeFinder.cs \
	TestTypes.cs \
	TextFormatter.cs \
	TypeLoader.cs \
	TypeFactory.cs \
	TypeReflectorOptions.cs \

all: $(PROGRAM)

$(PROGRAM) : $(PROGRAM_FILES)
	$(CSC) $(CSCFLAGS) $(PROGRAM_FILES)

test-run : $(PROGRAM)
	$(MONO) $(PROGRAM) -A $(PROGRAM) $(ARGS)

test-type : $(PROGRAM)
	$(MONO) $(PROGRAM) -A $(PROGRAM) --max-depth=10000 $(ARGS) TestClass$$

run : $(PROGRAM)
	$(MONO) $(PROGRAM) $(ARGS)

windows:
	NAnt -buildfile:type-reflector.build

linux: 
	mcs @unix.args

clean:
	rm -f *.exe *.pdb *.dll *.dbg *~

linecount:
	wc -l $(PROGRAM_FILES)

