topdir = ../..

LIBRARY = $(topdir)/class/lib/Mono.PEToolkit.dll

LIB_LIST = list.unix
LIB_FLAGS = --unsafe -r corlib -r System.Xml -r mscorlib

SOURCES_INCLUDE=			\
	build/mddump.cs			\
	BadImageException.cs		\
	COFFHeader.cs			\
	Characteristics.cs		\
	CheckSum.cs			\
	CorHeader.cs			\
	DOSHeader.cs			\
	DataDir.cs			\
	ExeSignature.cs			\
	Image.cs			\
	LEBitConverter.cs		\
	MachineId.cs			\
	PEHeader.cs			\
	PEUtils.cs			\
	RVA.cs				\
	Section.cs			\
	SectionCharacteristics.cs	\
	Subsystem.cs			\
	metadata/AssemblyFlags.cs	\
	metadata/BadMetaDataException.cs	\
	metadata/CodedTokenId.cs	\
	metadata/ElementType.cs		\
	metadata/GUIDHeap.cs		\
	metadata/MDHeap.cs		\
	metadata/MDStream.cs		\
	metadata/MDTable.cs		\
	metadata/MDToken.cs		\
	metadata/MDUtils.cs		\
	metadata/ManifestResourceAttributes.cs	\
	metadata/MetaDataRoot.cs	\
	metadata/MethodIL.cs		\
	metadata/MethodSemanticsAttributes.cs	\
	metadata/PInvokeAttributes.cs	\
	metadata/Row.cs			\
	metadata/Rows.cs		\
	metadata/StringsHeap.cs		\
	metadata/TableId.cs		\
	metadata/Tables.cs		\
	metadata/TablesHeap.cs		\
	metadata/TablesHeapBase.cs	\
	metadata/TabsDecoder.cs		\
	metadata/TokenType.cs		\


SOURCES_EXCLUDE=

#export MONO_PATH_PREFIX = $(topdir)/class/lib:

#include $(topdir)/class/library.make

mddump.exe:
	mcs /out:mddump.exe /unsafe $(SOURCES_INCLUDE)

clean:
	rm -f mddump.exe
