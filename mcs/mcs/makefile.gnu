MCS = mcs
MONO = mono
MCS_FLAGS = /target:exe $(MCS_DEFINES)
INSTALL = /usr/bin/install
prefix = /usr

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
	convert.cs			\
	decl.cs				\
	delegate.cs			\
	enum.cs				\
	ecore.cs			\
	expression.cs 			\
	genericparser.cs		\
	interface.cs			\
	iterators.cs			\
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

all: mcs.exe

mcs.exe: $(COMPILER_SOURCES) 
	$(MCS) $(MCS_FLAGS) -o $@ $(COMPILER_SOURCES)

mcs-mono.exe: $(COMPILER_SOURCES)
	$(MONO) mcs.exe $(MCS_FLAGS) -o $@ $(COMPILER_SOURCES)

mcs-mono2.exe: $(COMPILER_SOURCES)
	$(MONO) mcs.exe $(MCS_FLAGS) --debug -o $@ $(COMPILER_SOURCES)

cs-parser.cs: cs-parser.jay
	../jay/jay -ctv < ../jay/skeleton.cs $^ > $@

clean:
	-rm -f *.exe cs-parser.cs y.output

install: all
	mkdir -p $(prefix)/bin/
	$(INSTALL) -m 755 mcs.exe $(prefix)/bin/

test:

