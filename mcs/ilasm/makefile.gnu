RUNTIME = mono
topdir = ..
MCS = $(RUNTIME) $(topdir)/mcs/mcs.exe
CSFLAGS = --target exe
INSTALL = /usr/bin/install
prefix = /usr

SOURCES = 				\
	Driver.cs			\
	codegen/Class.cs		\
	codegen/CodeGen.cs		\
	codegen/InstrBase.cs		\
	codegen/Instructions.cs		\
	codegen/Method.cs		\
	codegen/Types.cs		\
	parser/ILParser.cs		\
	parser/ScannerAdapter.cs	\
	scanner/ILReader.cs		\
	scanner/ILSyntaxError.cs	\
	scanner/ILTables.cs		\
	scanner/ILToken.cs		\
	scanner/ILTokenizer.cs		\
	scanner/InstrToken.cs		\
	scanner/ITokenStream.cs		\
	scanner/Location.cs		\
	scanner/NumberHelper.cs		\
	scanner/StringHelperBase.cs	\
	scanner/StringHelper.cs

all: ilasm.exe

ilasm.exe: list
	$(MCS) $(CSFLAGS) @list -o ilasm.exe

install: all
	mkdir -p $(prefix)/bin
	$(INSTALL) -m 755 ilasm.exe $(prefix)/bin

parser/ILParser.cs: parser/ILParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs parser/ILParser.jay > parser/ILParser.cs

list: $(SOURCES)
	echo $(SOURCES) > list

clean:
	rm -f ilasm.exe parser/ILParser.cs list

