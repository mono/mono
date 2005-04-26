RUNTIME = mono #--debug
CSCOMPILE = mcs #-debug+

UCD_TABLE = UCD/UnicodeData.txt
UCA_TABLE = UCA/allkeys.txt
NORM_TABLE = UCD/DerivedNormalizationProps.txt
CB_CLASS_TABLE = UCD/extracted/DerivedCombiningClass.txt

NORM_FILENAME = create-normalization-source
COLL_ELEM_FILENAME = create-collation-element-table
COMBINING_FILENAME = create-combining-class-source
CANON_EQUIV_FILENAME = create-char-mapping-source

all : CollationElementTable.cs Normalization.cs

distclean :
	make clean
	rm $(UCA_TABLE) $(NORM_TABLE) $(CB_CLASS_TABLE)


clean : clean-collation clean-normalization

clean-collation :
	rm -f CollationElementTable.cs $(COLL_ELEM_FILENAME).exe*

clean-normalization :
	rm -f Normalization.cs $(NORM_FILENAME).exe* $(COMBINING_FILENAME).exe* $(CANON_EQUIV_FILENAME).exe*


# collation element table
CollationElementTable.cs : $(COLL_ELEM_FILENAME).exe CollationElementTable.template $(UCA_TABLE)
	cat CollationElementTable.template > CollationElementTable.cs || rm CollationElementTable.cs
	$(RUNTIME) $(COLL_ELEM_FILENAME).exe <  $(UCA_TABLE) >> CollationElementTable.cs || rm CollationElementTable.cs
	echo "	}" >> CollationElementTable.cs
	echo "}" >> CollationElementTable.cs

COLL_TABLE_GENERATOR_SOURCES = \
	$(COLL_ELEM_FILENAME).cs \
	CollationElementTableUtil.cs \
	CodePointIndexer.cs

$(COLL_ELEM_FILENAME).exe : $(COLL_TABLE_GENERATOR_SOURCES)
	$(CSCOMPILE) $(COLL_TABLE_GENERATOR_SOURCES)

# normalization (including combining class)
Normalization.cs : $(NORM_FILENAME).exe Normalization.template $(COMBINING_FILENAME).exe $(CANON_EQUIV_FILENAME).exe $(NORM_TABLE) $(UCD_TABLE) $(CB_CLASS_TABLE)
	cat Normalization.template > Normalization.cs || rm Normalization.cs
	$(RUNTIME) $(NORM_FILENAME).exe < $(NORM_TABLE) >> Normalization.cs || rm Normalization.cs
	$(RUNTIME) $(CANON_EQUIV_FILENAME).exe < $(UCD_TABLE) >> Normalization.cs || rm Normalization.cs
	$(RUNTIME) $(COMBINING_FILENAME).exe < $(CB_CLASS_TABLE) >> Normalization.cs || rm Normalization.cs
	echo "	}" >> Normalization.cs
	echo "}" >> Normalization.cs

NORM_TABLE_GENERATOR_SOURCES = \
	$(NORM_FILENAME).cs \
	NormalizationTableUtil.cs \
	CodePointIndexer.cs

$(NORM_FILENAME).exe : $(NORM_TABLE_GENERATOR_SOURCES)
	$(CSCOMPILE) $(NORM_TABLE_GENERATOR_SOURCES)

$(COMBINING_FILENAME).exe : $(COMBINING_FILENAME).cs
	$(CSCOMPILE) $(COMBINING_FILENAME).cs

CANON_EQUIV_GENERATOR_SOURCES = \
	$(CANON_EQUIV_FILENAME).cs \
	NormalizationTableUtil.cs \
	CodePointIndexer.cs

$(CANON_EQUIV_FILENAME).exe : $(CANON_EQUIV_GENERATOR_SOURCES)
	$(CSCOMPILE) $(CANON_EQUIV_GENERATOR_SOURCES)


$(UCD_TABLE) :
	if ! test -d UCD; then mkdir UCD; fi
	wget www.unicode.org/Public/UNIDATA/UnicodeData.txt
	mv UnicodeData.txt UCD/
	touch UCD/UnicodeData.txt

$(UCA_TABLE) :
	if ! test -d UCA; then mkdir UCA; fi
	wget http://www.unicode.org/Public/UCA/latest/allkeys.txt
	mv allkeys.txt UCA/
	touch UCA/allkeys.txt

$(NORM_TABLE) :
	if ! test -d UCD; then mkdir UCD; fi
	wget http://www.unicode.org/Public/UNIDATA/DerivedNormalizationProps.txt
	mv DerivedNormalizationProps.txt UCD/
	touch UCD/DerivedNormalizationProps.txt

$(CB_CLASS_TABLE) :
	if ! test -d UCD; then mkdir UCD; fi
	if ! test -d UCD/extracted; then mkdir UCD/extracted; fi
	wget http://www.unicode.org/Public/UNIDATA/extracted/DerivedCombiningClass.txt
	mv DerivedCombiningClass.txt UCD/extracted/
	touch UCD/extracted/DerivedCombiningClass.txt


sample : 
	$(CSCOMPILE) normtest.cs Normalization.cs NormalizationTableUtil.cs CodePointIndexer.cs
