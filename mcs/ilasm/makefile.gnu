RUNTIME = mono
topdir = ..
MCS = $(RUNTIME) $(topdir)/mcs/mcs.exe
CSFLAGS = --target exe -L ../class/lib
LIBFLAGS = /r:PEAPI.dll
INSTALL = /usr/bin/install
prefix = /usr

SOURCES = 				\
	Driver.cs			\
	AssemblyInfo.cs			\
	codegen/CodeGen.cs		\
	codegen/ClassTable.cs		\
	codegen/ExternTable.cs		\
	codegen/MethodTable.cs          \
	codegen/InstrTable.cs		\
        codegen/TypeRef.cs              \
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
	scanner/StringHelper.cs		\

all: ilasm.exe

ilasm.exe: list
	$(MCS) $(CSFLAGS) $(LIBFLAGS) @list -o ilasm.exe

install: all
	mkdir -p $(prefix)/bin
	$(INSTALL) -m 755 ilasm.exe $(prefix)/bin

parser/ILParser.cs: parser/ILParser.jay $(topdir)/jay/skeleton.cs
	$(topdir)/jay/jay -ct < $(topdir)/jay/skeleton.cs parser/ILParser.jay > parser/ILParser.cs

list: $(SOURCES)
	echo $(SOURCES) > list

clean:
	rm -f ilasm.exe parser/ILParser.cs list

