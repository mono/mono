MCS = mcs
MCS_FLAGS = /target:exe $(MCS_DEFINES)
INSTALL = /usr/bin/install
prefix = /usr

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

test: mbas.exe
	mono ./mbas.exe @testmbas/filelist

mbas.exe: $(COMPILER_SOURCES)
	$(MCS) $(MCSFLAGS) /r:Mono.GetOptions.dll /out:mbas.exe $(COMPILER_SOURCES)

clean:
	rm -f mbas.exe y.output mbas.pdb *~ .*~ mb-parser.cs mbas.log response

mb-parser.cs: mb-parser.jay
	../jay/jay -ctv < ../jay/skeleton.cs mb-parser.jay > mb-parser.cs

install: all
	mkdir -p $(prefix)/bin/
	$(INSTALL) -m 755 mbas.exe $(prefix)/bin/

test:
	mono mbas.exe testmbas/WriteOK.vb 

test-gtk:
	mono mbas.exe testmbas/gtk.vb -r gtk-sharp

