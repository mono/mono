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
	tree.cs				\
	CryptoConvert.cs		\
	$(topdir)/class/Mono.CSharp.Debugger/MonoSymbolFile.cs	\
	$(topdir)/class/Mono.CSharp.Debugger/MonoSymbolTable.cs	\
	$(topdir)/class/Mono.CSharp.Debugger/MonoSymbolWriter.cs

all_sources = $(COMPILER_SOURCES) cs-parser.cs

DISTFILES = \
	$(COMPILER_SOURCES)	\
	cs-parser.jay		\
	gmcs.exe.config
#	compiler.csproj		\
#	compiler.doc		\
#	compiler.sln		\
#	NOTES			\
#	TODO

#uncomment to enable some debug stuff
#DEBUG_FLAGS=/define:MCS_DEBUG

all-local: gmcs.exe

PROGRAM_INSTALL_DIR = $(prefix)/lib/mono/2.0

install-local: gmcs.exe
	$(MKINSTALLDIRS) $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_BIN) gmcs.exe $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_DATA) gmcs.exe.config $(DESTDIR)$(PROGRAM_INSTALL_DIR)
	$(INSTALL_DATA) gmcs.exe.mdb $(DESTDIR)$(PROGRAM_INSTALL_DIR)

uninstall-local:
	-rm -f $(DESTDIR)$(PROGRAM_INSTALL_DIR)/gmcs.exe*

test-local run-test-local:

clean-local:
	rm -f *.exe *.pdb cs-parser.cs y.output *.mdb
	cd .. && $(MAKE) PROFILE=net_2_0_bootstrap clean

dist-local: dist-default

bootstrap_libs = mscorlib.dll System.dll System.Xml.dll Mono.CompilerServices.SymbolWriter.dll
bootstrap_libfiles = $(bootstrap_libs:%=$(topdir)/class/lib/net_2_0_bootstrap/%)

$(bootstrap_libfiles): bootstrap-libs
	@:
.PHONY: bootstrap-libs
bootstrap-libs:
	cd .. && $(MAKE) PROFILE=net_2_0_bootstrap all

gmcs.exe: $(all_sources) $(bootstrap_libfiles)
	MONO_PATH="../class/lib/net_2_0_bootstrap$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" $(RUNTIME) $(topdir)/class/lib/net_2_0_bootstrap/mcs.exe $(DEBUG_FLAGS) -2 /target:exe /debug /out:$@ $(all_sources)

gmcs2.exe: $(all_sources)
	MONO_PATH="../class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" time mcs -2 /target:exe /debug /out:$@ $^ $(DEBUG_FLAGS) 

gmcs3.exe: $(all_sources)
	MONO_PATH="../class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" time $(RUNTIME) ./gmcs2.exe $(USE_MCS_FLAGS) $(DEBUG_FLAGS) -2 /target:exe /debug /out:$@ $^

gmcs4.exe: $(all_sources)
	MONO_PATH="../class/lib/net_2_0$(PLATFORM_PATH_SEPARATOR)$$MONO_PATH" time $(RUNTIME) ./gmcs3.exe $(USE_MCS_FLAGS) $(DEBUG_FLAGS) -2 /target:exe /debug /out:$@ $^

cs-parser.cs: cs-parser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ctv < $(topdir)/jay/skeleton.cs $< > jay-tmp.out && mv jay-tmp.out $@

# Testing targets

TIME = 

# This used to be called test, but that conflicts with the global
# recursive target.

btest: gmcs2.exe gmcs3.exe gmcs4.exe
	ls -l gmcs3.exe gmcs4.exe

wc:
	wc -l $(all_sources)

ctest: 
	rm -f gmcs3.exe mcs4.exe
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

profile : gmcs2.exe
	make gmcs3.exe RUNTIME="$(RUNTIME) --profile"

response:
	echo $(all_sources) > res
