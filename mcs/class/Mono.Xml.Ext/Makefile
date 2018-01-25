thisdir = class/Mono.Xml.Ext
SUBDIRS = 
include ../../build/rules.make

LIBRARY = Mono.Xml.Ext.dll

ifeq ($(PROFILE),atomic)
SCARY_LIB=
else
SCARY_LIB=/lib:$(prefix)/lib
endif

LIB_REFS = System System.Xml
KEYFILE = ../ecma.pub
LIB_MCS_FLAGS = $(SCARY_LIB) /unsafe
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS)

EXTRA_DISTFILES = \
	Mono.Xml.XPath2/XQueryParser.jay

SKELETON = $(topdir)/jay/skeleton.cs

Mono.Xml.XPath2/XPath2Parser.jay: Mono.Xml.XPath2/ParserBase.jay $(SKELETON)
	sed "s/%start Module/%start Expr/" $< | sed "/\#XQueryStart/,/\#XQueryEnd/d" | sed "/\#XPath2Start/d" | sed "/\#XPath2End/d" >$@

Mono.Xml.XPath2/XQueryParser.jay: Mono.Xml.XPath2/ParserBase.jay $(SKELETON)
	sed "/\#XPath2Start/,/\#XPath2End/d" $< | sed "/\#XQueryStart/d" | sed "/\#XQueryEnd/d" >$@

Mono.Xml.XPath2/XPath2Parser.cs: Mono.Xml.XPath2/XPath2Parser.jay
	echo "#define XPATH2_PARSER" > $@
	echo "#if NET_2_0" >> $@
	$(topdir)/jay/jay -ct $(CURDIR)/$< < $(SKELETON) >>$@
	echo "#endif" >> $@

Mono.Xml.XPath2/XQueryParser.cs: Mono.Xml.XPath2/XQueryParser.jay $(SKELETON)
	echo "#define XQUERY_PARSER" > $@
	echo "#if NET_2_0" >> $@
	$(topdir)/jay/jay -ct $(CURDIR)/$< < $(SKELETON) >>$@
	echo "#endif" >> $@

Mono.Xml.XPath2/XPath2Tokenizer.cs: Mono.Xml.XPath2/TokenizerBase.cs
	echo "#define XPATH2_PARSER" > $@
	cat $< >>$@

Mono.Xml.XPath2/XQueryTokenizer.cs: Mono.Xml.XPath2/TokenizerBase.cs
	echo "#define XQUERY_PARSER" > $@
	cat $< >>$@

BUILT_SOURCES = Mono.Xml.XPath2/XPath2Parser.cs \
	Mono.Xml.XPath2/XPath2Tokenizer.cs \
	Mono.Xml.XPath2/XQueryParser.cs \
	Mono.Xml.XPath2/XQueryTokenizer.cs

CLEAN_FILES = Mono.Xml.Ext.pdb \
	Mono.Xml.XPath2/XPath2Parser.jay \
	Mono.Xml.XPath2/XPath2Parser.cs \
	Mono.Xml.XPath2/XQueryParser.cs \
	y.output

include ../../build/library.make
