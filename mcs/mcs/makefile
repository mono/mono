ROOT=/cygdrive/$(subst \,/,$(subst :\,/,$(SYSTEMROOT)))
CSC=$(ROOT)/microsoft.net/framework/v1.0.2914/csc.exe
CSCFLAGS=/nologo /debug+ /debug:full  /optimize

VERSION=0.13

COMMON_SOURCES = cs-parser.cs cs-tokenizer.cs tree.cs  

COMPILER_SOURCES = \
	assign.cs			\
	driver.cs $(COMMON_SOURCES) 	\
	class.cs 			\
	delegate.cs			\
	attribute.cs			\
	codegen.cs			\
	constant.cs			\
	decl.cs				\
	enum.cs				\
	expression.cs 			\
	gen-treedump.cs			\
	genericparser.cs		\
	interface.cs			\
	literal.cs			\
	modifiers.cs 			\
	namespace.cs			\
	parameter.cs			\
	report.cs			\
	rootcontext.cs			\
	statement.cs			\
	statementCollection.cs		\
	support.cs			\
	typemanager.cs

TEST_TOKENIZER_SOURCES = test-token.cs $(COMMON_SOURCES)

all: cs-parser.cs
	$(CSC) $(CSCFLAGS) /target:exe /r:System.dll /out:compiler.exe $(COMPILER_SOURCES)

windows: all

unix: all

opt:
	$(CSC) /optimize+ /target:exe /r:System.dll /out:compiler.exe $(COMPILER_SOURCES)

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

statementCollection.cs: X-Collection.cs
	sed -e "s/@CONTAINEE@/Statement/g" -e "s/@arrayname@/statements/g" < X-Collection.cs > statementCollection.cs

parameterCollection.cs: X-Collection.cs
	sed -e "s/@CONTAINEE@/Parameter/g" -e "s/@arrayname@/parameters/g" < X-Collection.cs > parameterCollection.cs

DIST_FILES=$(COMPILER_SOURCES) cs-parser.jay makefile ChangeLog 

dist:
	rm -rf /tmp/mcs-$(VERSION)
	mkdir /tmp/mcs-$(VERSION)
	mkdir /tmp/mcs-$(VERSION)/errors
	mkdir /tmp/mcs-$(VERSION)/tests
	mkdir /tmp/mcs-$(VERSION)/docs
	cp -a $(DIST_FILES) /tmp/mcs-$(VERSION)
	cp errors/*.cs errors/*.txt /tmp/mcs-$(VERSION)/errors
	cp tests/*.cs /tmp/mcs-$(VERSION)/tests
	cp docs/*.txt /tmp/mcs-$(VERSION)/docs
	(cd /tmp; tar czvf mcs-$(VERSION).tar.gz mcs-$(VERSION))

try:
	-mkdir try-dir
	for i in $(COMPILER_SOURCES); do \
		./compiler -t tree $$i > try-dir/$$i; \
	done

clean:
	rm -f compiler.exe cs-parser.cs y.output compiler.pdb *~ .*~
