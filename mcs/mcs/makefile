CSC=csc.exe
CSCFLAGS=/nologo /debug+ /debug:full  /optimize

VERSION=0.13

COMMON_SOURCES = cs-parser.cs cs-tokenizer.cs tree.cs location.cs

COMPILER_SOURCES = \
	AssemblyInfo.cs			\
	assign.cs			\
	attribute.cs			\
	driver.cs $(COMMON_SOURCES) 	\
	cfold.cs			\
	class.cs 			\
	codegen.cs			\
	const.cs			\
	constant.cs			\
	decl.cs				\
	delegate.cs			\
	enum.cs				\
	ecore.cs			\
	expression.cs 			\
	genericparser.cs		\
	interface.cs			\
	literal.cs			\
	modifiers.cs 			\
	namespace.cs			\
	parameter.cs			\
	pending.cs			\
	report.cs			\
	rootcontext.cs			\
	statement.cs			\
	support.cs			\
	typemanager.cs			\
	symbolwriter.cs

TEST_TOKENIZER_SOURCES = test-token.cs $(COMMON_SOURCES)

all: 
	if test x$(OS) = xWindows_NT; then make mcs.exe; else make -f makefile.gnu; fi

install:
	make -f makefile.gnu install

mcs.exe: cs-parser.cs $(COMPILER_SOURCES)
	$(CSC) $(CSCFLAGS) /target:exe /r:System.dll /out:mcs.exe $(COMPILER_SOURCES)

count:
	wc -l $(COMPILER_SOURCES)

res:
	echo --target exe -o mcs2.exe $(COMPILER_SOURCES) > response

dum: cs-parser.cs
	$(TIME) $(RUNTIME) ./mcs.exe --target exe -o mcs2.exe $(COMPILER_SOURCES)

mcs2.exe: mcs.exe
	$(TIME) $(RUNTIME) ./mcs.exe $(XFLAGS) --target exe -o mcs2.exe $(COMPILER_SOURCES)

mcs3.exe: mcs2.exe
	$(TIME) $(RUNTIME) ./mcs2.exe --target exe -o mcs3.exe $(COMPILER_SOURCES)

mcs-mono.exe: cs-parser.cs $(COMPILER_SOURCES) 
	mono --precompile @mcs.exe mcs.exe $(MCSFLAGS) --fatal --timestamp -o mcs-mono.exe $(COMPILER_SOURCES) 
mm:
	mint $(MINTFLAGS) ./mcs.exe $(MCSFLAGS) --target exe -o mcs4.exe $(COMPILER_SOURCES)

mcs-mono2.exe: cs-parser.cs $(COMPILER_SOURCES)
	mono mcs.exe --timestamp $(MCSFLAGS) -o mcs-mono2.exe -g $(COMPILER_SOURCES)

mcs-mono3.exe: cs-parser.cs $(COMPILER_SOURCES)
	mono --debug mcs-mono2.exe -o mcs-mono3.exe -g $(COMPILER_SOURCES)

mcs-mono4.exe: cs-parser.cs $(COMPILER_SOURCES)
	mono --debug mcs-mono3.exe -o mcs-mono4.exe -g $(COMPILER_SOURCES)

monomcs: cs-parser.cs
	mcs $(MCSFLAGS) --target exe -o mcs4.exe $(COMPILER_SOURCES)

test: mcs3.exe
	ls -l mcs2.exe mcs3.exe

docs: cs-parser.cs
	$(CSC) $(CSCFLAGS) /doc:docs.xml /nowarn:1591 /target:exe /r:System.dll /out:mcs.exe $(COMPILER_SOURCES)

windows: all

linux: all

opt:
	$(CSC) /optimize+ /target:exe /r:System.dll /out:mcs.exe $(COMPILER_SOURCES)

old: test-tokenizer.exe
test-tokenizer.exe: $(TEST_TOKENIZER_SOURCES)
	$(CSC) /target:exe /out:test-tokenizer.exe $(TEST_TOKENIZER_SOURCES)

bison:
	perl -pe 's/\015//' < cs-parser.jay > x.y
	bison --debug --verbose x.y

cs-parser.cs: cs-parser.jay
	../jay/jay -ctv < ../jay/skeleton.cs cs-parser.jay > cs-parser.cs

pn:
	../jay/jay -ctv < ../jay/skeleton.cs cs-parser.jay | grep -v '#line' > cs-parser.cs

#statementCollection.cs: X-Collection.cs
#	sed -e "s/@CONTAINEE@/Statement/g" -e "s/@arrayname@/statements/g" < X-Collection.cs > statementCollection.cs

#parameterCollection.cs: X-Collection.cs
#	sed -e "s/@CONTAINEE@/Parameter/g" -e "s/@arrayname@/parameters/g" < X-Collection.cs > parameterCollection.cs

clean:
	rm -f mcs.exe mcs2.exe mcs3.exe cs-parser.cs y.output mcs.pdb *~ .*~ mcs.log response
