CSC = csc
CSFLAGS = /nologo /optimize /target:exe /out:ilasm.exe
LIBFLAGS = /r:../class/lib/PEAPI.dll
INSTALL = /usr/bin/install
prefix = /usr

SOURCES =				\
	Driver.cs			\
	AssemblyInfo.cs			\
	codegen/CodeGen.cs		\
	codegen/ExternTable.cs		\
	codegen/InstrTable.cs		\
        codegen/ITypeRef.cs             \
        codegen/IClassRef.cs            \
        codegen/FieldDef.cs             \
	codegen/ParamDef.cs		\
	codegen/MethodDef.cs		\
	codegen/TypeDef.cs              \
	codegen/DataDef.cs		\
	codegen/TypeRef.cs              \
        codegen/PeapiTypeRef.cs         \
        codegen/ExternTypeRef.cs        \
        codegen/PrimitiveTypeRef.cs     \
        codegen/TypeManager.cs          \
	codegen/IInstr.cs               \
        codegen/IntInstr.cs             \
        codegen/LdstrInstr.cs           \
        codegen/SimpInstr.cs            \
	codegen/MiscInstr.cs		\
	codegen/LdcInstr.cs		\
	codegen/BranchInstr.cs		\
	codegen/SwitchInstr.cs		\
	codegen/TypeInstr.cs		\
	codegen/MethodInstr.cs          \
        codegen/Local.cs                \
        codegen/IMethodRef.cs           \
        codegen/ExternMethodRef.cs      \
        codegen/MethodRef.cs            \
	codegen/GlobalMethodRef.cs      \
	codegen/IFieldRef.cs            \
        codegen/ExternFieldRef.cs       \
        codegen/GlobalFieldRef.cs       \
        codegen/FieldRef.cs             \
        codegen/FieldInstr.cs           \
	codegen/LdtokenInstr.cs         \
        codegen/CalliInstr.cs           \
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
	$(CSC) $(CSFLAGS) $(LIBFLAGS) @list

install: all
	mkdir -p $(prefix)/bin
	$(INSTALL) -m 755 ilasm.exe $(prefix)/bin

parser/ILParser.cs: parser/ILParser.jay ../jay/skeleton.cs
	../jay/jay -ct < ../jay/skeleton.cs parser/ILParser.jay > parser/ILParser.cs

list: $(SOURCES)
	echo $(SOURCES) | sed -e 's@/@\\@g' > list

all: ilasm.exe

linux: ilasm.exe

windows: ilasm.exe

clean:
	rm -f ilasm.exe parser/ILParser.cs list
