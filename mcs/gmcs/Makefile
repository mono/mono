thisdir := gmcs
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
	generic.cs			\
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

all-local: gmcs.exe

install-local: gmcs.exe
	$(MKINSTALLDIRS) $(DESTDIR)$(prefix)/bin
	$(INSTALL_BIN) gmcs.exe $(DESTDIR)$(prefix)/bin

uninstall-local:
	-rm -f $(DESTDIR)$(prefix)/bin/mcs.exe

test-local run-test-local:

clean-local:
	rm -f *.exe *.pdb cs-parser.cs y.output

dist-local: dist-default

gmcs.exe: $(all_sources)
	$(BOOT_COMPILE) /target:exe /out:$@ $^

cs-parser.cs: cs-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $< >$@

# Testing targets

TIME = 

# This used to be called test, but that conflicts with the global
# recursive target.

btest: mcs2.exe mcs3.exe
	ls -l mcs2.exe mcs3.exe

mcs2.exe: gmcs.exe
	$(TIME) $(RUNTIME) ./gmcs.exe $(USE_MCS_FLAGS) /target:exe /out:$@ $(all_sources)

mcs3.exe: mcs2.exe
	$(TIME) $(RUNTIME) ./mcs2.exe $(USE_MCS_FLAGS) /target:exe /out:$@ $(all_sources)

wc:
	wc -l $(all_sources)

ctest: 
	-rm mcs2.exe mcs3.exe
	make btest USE_MCS_FLAGS= 

# we need this because bash tries to use its own crappy timer
FRIENDLY_TIME = $(shell which time) -f'%U seconds'

do-time : mcs.exe
	@ echo -n "Run 1:   "
	 $(FRIENDLY_TIME) $(RUNTIME) ./mcs.exe $(USE_MCS_FLAGS) /target:exe /out:mcs2.exe $(all_sources) > /dev/null || (echo FAILED; exit 1)
	@ echo -n "Run 2:   "
	@ $(FRIENDLY_TIME) $(RUNTIME) ./mcs2.exe $(USE_MCS_FLAGS) /target:exe /out:mcs3.exe $(all_sources) > /dev/null || (echo FAILED; exit 1)
	@ echo -n "corlib:  "
	@ rm -f ../class/lib/mscorlib.dll
	@ (cd ../class/corlib ; make BOOTSTRAP_MCS="$(FRIENDLY_TIME) mono ../../mcs/mcs.exe" > /dev/null ) || (echo FAILED; exit 1)

do-corlib:
	@ echo -n "corlib:  "
	@ rm -f ../class/lib/mscorlib.dll
	@ (cd ../class/corlib ; make BOOTSTRAP_MCS="$(FRIENDLY_TIME) mono ../../mcs/mcs.exe" > /dev/null ) || (echo FAILED; exit 1)

PROFILER=default

profile : mcs.exe
	$(RUNTIME) --profile=$(PROFILER) ./mcs.exe $(USE_MCS_FLAGS) /target:exe /out:mcs2.exe $(all_sources)

response:
	echo $(all_sources) > res
