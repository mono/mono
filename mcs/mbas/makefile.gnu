topdir=..
MCS = ../mcs/mcs.exe
MCS_FLAGS = /target:exe $(MCS_DEFINES)
INSTALL = /usr/bin/install
prefix = /usr
RUNTIME=mono
MONO_PATH_PREFIX=$(topdir)/class/lib:

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

all: mbas.exe

mbas.exe: $(COMPILER_SOURCES)
	MONO_PATH=$(MONO_PATH_PREFIX)$(MONO_PATH) $(RUNTIME) $(MCS) $(MCSFLAGS) /r:Mono.GetOptions.dll /out:mbas.exe $(COMPILER_SOURCES)

clean:
	rm -f mbas.exe y.output mbas.pdb *~ .*~ mb-parser.cs mbas.log response

mb-parser.cs: mb-parser.jay
	../jay/jay -ctv < ../jay/skeleton.cs mb-parser.jay > mb-parser.cs

install: all
	mkdir -p $(prefix)/bin/
	$(INSTALL) -m 755 mbas.exe $(prefix)/bin/

test: mbas.exe
	$(RUNTIME) mbas.exe  --stacktrace  /r:Mono.GetOptions.dll /r:System.Data,System.Windows.Forms --main WriteOK testmbas/WriteOK.vb testmbas/WriteOK2.mbs
	$(RUNTIME) testmbas/WriteOK.exe

verbose: mbas.exe
	$(RUNTIME) mbas.exe  --verbosegetoptions --stacktrace --verbose --main WriteOK testmbas/WriteOK.vb testmbas/WriteOK2.mbs | less

test-gtk: mbas.exe
	$(RUNTIME) mbas.exe testmbas/gtk.vb -r gtk-sharp
	$(RUNTIME) testmbas/gtk.exe
