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
	codegen/ExternTable.cs		\
	codegen/InstrTable.cs		\
        codegen/ITypeRef.cs             \
        codegen/IClassRef.cs            \
        codegen/FieldDef.cs             \
        codegen/ParamDef.cs             \
        codegen/MethodDef.cs            \
        codegen/TypeDef.cs              \
        codegen/DataDef.cs              \
        codegen/TypeRef.cs              \
        codegen/PeapiTypeRef.cs         \
        codegen/ExternTypeRef.cs        \
        codegen/PrimitiveTypeRef.cs     \
        codegen/TypeManager.cs          \
        codegen/IInstr.cs               \
        codegen/IntInstr.cs             \
        codegen/LdstrInstr.cs           \
        codegen/SimpInstr.cs            \
        codegen/MiscInstr.cs            \
        codegen/LdcInstr.cs             \
        codegen/BranchInstr.cs          \
        codegen/SwitchInstr.cs          \
        codegen/TypeInstr.cs            \
        codegen/MethodInstr.cs          \
        codegen/Local.cs                \
        codegen/IMethodRef.cs           \
        codegen/ExternMethodRef.cs      \
        codegen/MethodRef.cs            \
        codegen/GlobalMethodRef.cs      \
        codegen/IFieldRef.cs            \
        codegen/ExternFieldRef.cs       \
        codegen/GlobalFieldRef.cs       \
        codegen/FeatureAttr.cs          \
        codegen/EventDef.cs             \
        codegen/PropertyDef.cs          \
        codegen/FieldRef.cs             \
        codegen/FieldInstr.cs           \
        codegen/LdtokenInstr.cs         \
        codegen/CalliInstr.cs           \
        codegen/TryBlock.cs             \
        codegen/ISehClause.cs           \
        codegen/FilterBlock.cs          \
        codegen/FinallyBlock.cs         \
        codegen/HandlerBlock.cs         \
        codegen/FaultBlock.cs           \
        codegen/CatchBlock.cs           \
        codegen/CustomAttr.cs           \
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

test:

clean:
	rm -f ilasm.exe parser/ILParser.cs list

