CSC=csc
CSCFLAGS=/nologo /optimize /target:exe /r:System.dll /r:../class/lib/Mono.GetOptions.dll /out:mbas.exe 

COMPILER_SOURCES = \
    AssemblyInfo.cs   \
	assign.cs		\
	argument.cs		\
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

linux: all

all: mbas.exe

test: mbas.exe
	mono mbas.exe --main WriteOK testmbas/WriteOK.vb testmbas/WriteOK2.mbs
	mono testmbas/WriteOK.exe

verbose: mbas.exe
	mono mbas.exe --verbose --main WriteOK testmbas/WriteOK.vb testmbas/WriteOK2.mbs | less

mbas.exe: $(COMPILER_SOURCES)
	$(CSC) $(CSCFLAGS) $(COMPILER_SOURCES)

clean:
	rm -f mbas.exe y.output mbas.pdb *~ .*~ mb-parser.cs mbas.log response

mb-parser.cs: mb-parser.jay
	../jay/jay -ctv < ../jay/skeleton.cs mb-parser.jay > mb-parser.cs
