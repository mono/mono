RUNTIME = mono
MCS = mcs
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
	codegen/MethodTable.cs		\
	codegen/TypeRef.cs		\
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

ilasm.exe: list
	$(MCS) $(CSFLAGS) $(LIBFLAGS) @list -o ilasm.exe

install: all
	mkdir -p $(prefix)/bin
	$(INSTALL) -m 755 ilasm.exe $(prefix)/bin

parser/ILParser.cs: parser/ILParser.jay ../jay/skeleton.cs
	../jay/jay -ct < ../jay/skeleton.cs parser/ILParser.jay > parser/ILParser.cs

list: $(SOURCES)
	echo $(SOURCES) > list

all: ilasm.exe

linux: ilasm.exe

windows: ilasm.exe

clean:
	rm -f ilasm.exe parser/ILParser.cs list
