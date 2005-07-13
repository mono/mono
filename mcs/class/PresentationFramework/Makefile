thisdir = class/PresentationFramework
include ../../build/rules.make

LIBRARY = PresentationFramework.dll

LIB_MCS_FLAGS = -r:System.Xml.dll -r:WindowsBase.dll -r:System.dll
TEST_MCS_FLAGS = $(LIB_MCS_FLAGS) -r:TestVocab.dll -r:System.dll
TEST_MONO_PATH = .

Test/XamlParser.cs: Test/TestVocab.dll

Test/TestVocab.dll: ../../tools/xamlc/demo/TestVocab/*.cs
	echo $(MONO_PATH)
	(cd ../../tools/xamlc/demo; make TestVocab.dll)
	cp ../../tools/xamlc/demo/TestVocab.dll Test/TestVocab.dll
	cp Test/TestVocab.dll TestVocab.dll

include ../../build/library.make
