CSC=mcs
CSCFLAGS=/nologo /optimize /target:exe /r:System.dll /r:Mono.GetOptions.dll /out:mbas.exe 

VERSION=0.13

COMPILER_SOURCES = \
      AssemblyInfo.cs   \
	assign.cs		\
	attribute.cs		\
	cfold.cs		\
	class.cs 		\
	codegen.cs		\
	const.cs		\
	constant.cs		\
	decl.cs			\
	delegate.cs		\
	driver.cs 	 	\
	enum.cs			\
	ecore.cs		\
	expression.cs 		\
	genericparser.cs	\
	interface.cs		\
	literal.cs		\
	location.cs 		\
	mb-parser.cs 		\
	mb-tokenizer.cs 	\
	modifiers.cs 		\
	module.cs		\
	namespace.cs		\
	parameter.cs		\
	pending.cs		\
	report.cs		\
	rootcontext.cs		\
	statement.cs		\
	statementCollection.cs	\
	support.cs		\
	tree.cs 		\
	typemanager.cs

all: mbas.exe

parser: mb-parser.cs

test: mbas.exe
	./mbas @testmbas/filelist

mbas.exe: $(COMPILER_SOURCES)
	$(CSC) $(CSCFLAGS) $(COMPILER_SOURCES)

docs: $(COMPILER_SOURCES)
	$(CSC) $(CSCFLAGS) /doc:docs.xml /nowarn:1591 $(COMPILER_SOURCES)

windows: all

linux: all

opt:
	$(CSC) /nologo /optimize+ /r:System.dll /target:exe /out:mbas.exe $(COMPILER_SOURCES)

clean:
	rm -f mbas.exe y.output mbas.pdb *~ .*~ mb-parser.cs mbas.log response

mb-parser.cs: mb-parser.jay
	../jay/jay -ctv < ../jay/skeleton.cs mb-parser.jay > mb-parser.cs

