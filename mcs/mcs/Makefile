thisdir := mcs
SUBDIRS := 
include ../build/rules.make

COMPILER_SOURCES = \
	AssemblyInfo.cs			\
	anonymous.cs			\
	assign.cs			\
	attribute.cs			\
	driver.cs		 	\
	cs-tokenizer.cs			\
	cfold.cs			\
	class.cs 			\
	codegen.cs			\
	const.cs			\
	constant.cs			\
	convert.cs			\
	decl.cs				\
	delegate.cs			\
	enum.cs				\
	ecore.cs			\
	expression.cs 			\
	flowanalysis.cs			\
	genericparser.cs		\
	interface.cs			\
	iterators.cs			\
	literal.cs			\
	location.cs			\
	modifiers.cs 			\
	namespace.cs			\
	parameter.cs			\
	pending.cs			\
	report.cs			\
	rootcontext.cs			\
	statement.cs			\
	support.cs			\
	typemanager.cs			\
	symbolwriter.cs			\
	tree.cs

all_sources = $(COMPILER_SOURCES) cs-parser.cs

DISTFILES = \
	$(COMPILER_SOURCES)	\
	compiler.csproj		\
	compiler.csproj.user	\
	compiler.doc		\
	compiler.sln		\
	cs-parser.jay		\
	mcs.exe.config		\
	NOTES			\
	TODO

all-local: mcs.exe

install-local: mcs.exe
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin
	$(INSTALL_BIN) mcs.exe $(DESTDIR)$(prefix)/bin

test-local run-test-local:

clean-local:
	rm -f *.exe *.pdb cs-parser.cs y.output

dist-local: dist-default

mcs.exe: $(all_sources)
	$(BOOT_COMPILE) /target:exe /out:$@ $^

cs-parser.cs: cs-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $< >$@

# Testing targets

TIME = time

# This used to be called test, but that conflicts with the global
# recursive target.

btest: mcs2.exe mcs3.exe
	ls -l mcs2.exe mcs3.exe

mcs2.exe: mcs.exe
	$(TIME) $(RUNTIME) ./mcs.exe $(USE_MCS_FLAGS) /target:exe /out:$@ $(all_sources)

mcs3.exe: mcs2.exe
	$(TIME) $(RUNTIME) ./mcs2.exe $(USE_MCS_FLAGS) /target:exe /out:$@ $(all_sources)

ctest: 
	-rm mcs2.exe mcs3.exe
	make btest USE_MCS_FLAGS= 

response:
	echo $(all_sources) > res
