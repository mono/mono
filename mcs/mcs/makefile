VERSION=0.13

COMMON_SOURCES = cs-parser.cs cs-tokenizer.cs errors.cs tree.cs  

COMPILER_SOURCES = \
	assign.cs			\
	driver.cs $(COMMON_SOURCES) 	\
	class.cs 			\
	constant.cs			\
	decl.cs				\
	enum.cs				\
	expression.cs 			\
	generator.cs			\
	gen-il.cs			\
	gen-treedump.cs			\
	interface.cs			\
	literal.cs			\
	modifiers.cs 			\
	namespace.cs			\
	parameter.cs			\
	parameterCollection.cs		\
	statement.cs			\
	statementCollection.cs		\
	type.cs

TEST_TOKENIZER_SOURCES = test-token.cs $(COMMON_SOURCES)

all:
	-rm compiler.exe
	csc /target:exe /r:System.dll /debug+ /debug:full /out:compiler.exe $(COMPILER_SOURCES)

opt:
	-rm compiler.exe
	csc /optimize+ /target:exe /r:System.dll /debug+ /debug:full /out:compiler.exe $(COMPILER_SOURCES)

old: test-tokenizer.exe
test-tokenizer.exe: $(TEST_TOKENIZER_SOURCES)
	csc /target:exe /out:test-tokenizer.exe $(TEST_TOKENIZER_SOURCES)

bison:
	perl -pe 's/\015//' < cs-parser.jay > x.y
	bison --debug --verbose x.y

parser:
	./jay -tv cs-parser.jay > cs-parser.cs

migparser:
	jay -tv cs-parser.jay > cs-parser.cs

parsernoline:
	jay -tv cs-parser.jay | grep -v '^#line' > cs-parser.cs

syncunix:
	rsync -v -a /quack/Barra ~/Barra

syncquack:
	rsync -v -a ~/Barra /quack/Barra

syncnt:
	rsync -v -a ~/Barra /quack/nt

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
