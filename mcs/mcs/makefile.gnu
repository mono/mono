MCS = mcs
MCS_FLAGS = --debug --fatal --target exe

COMMON_SOURCES = cs-parser.cs cs-tokenizer.cs tree.cs location.cs

COMPILER_SOURCES = \
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
	typemanager.cs

TEST_TOKENIZER_SOURCES = test-token.cs $(COMMON_SOURCES)

all: mcs.exe

mcs.exe: $(COMPILER_SOURCES)
	$(MCS) $(MCS_FLAGS) -o $@ $(COMPILER_SOURCES)

cs-parser.cs: cs-parser.jay
	../jay/jay -ctv < ../jay/skeleton.cs $^ > $@

clean:
	-rm -f *.exe cs-parser.cs

