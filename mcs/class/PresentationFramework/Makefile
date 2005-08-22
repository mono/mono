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

library_CLEAN_FILES = System.Windows.Serialization/XamlAttributeNode.cs \
System.Windows.Serialization/XamlClrEventNode.cs \
System.Windows.Serialization/XamlConstructorParametersEndNode.cs \
System.Windows.Serialization/XamlConstructorParametersStartNode.cs \
System.Windows.Serialization/XamlConstructorParameterTypeNode.cs \
System.Windows.Serialization/XamlDefAttributeKeyTypeNode.cs \
System.Windows.Serialization/XamlDefAttributeNode.cs \
System.Windows.Serialization/XamlDefTagNode.cs \
System.Windows.Serialization/XamlDocumentEndNode.cs \
System.Windows.Serialization/XamlDocumentStartNode.cs \
System.Windows.Serialization/XamlElementStartNode.cs \
System.Windows.Serialization/XamlElementEndNode.cs \
System.Windows.Serialization/XamlEndAttributesNode.cs \
System.Windows.Serialization/XamlKeyElementEndNode.cs \
System.Windows.Serialization/XamlKeyElementStartNode.cs \
System.Windows.Serialization/XamlLanguageNode.cs \
System.Windows.Serialization/XamlLiteralContentNode.cs \
System.Windows.Serialization/XamlNode.cs \
System.Windows.Serialization/XamlPIMappingNode.cs \
System.Windows.Serialization/XamlPropertyArrayEndNode.cs \
System.Windows.Serialization/XamlPropertyArrayStartNode.cs \
System.Windows.Serialization/XamlPropertyComplexEndNode.cs \
System.Windows.Serialization/XamlPropertyComplexStartNode.cs \
System.Windows.Serialization/XamlPropertyIDictionaryEndNode.cs \
System.Windows.Serialization/XamlPropertyIDictionaryStartNode.cs \
System.Windows.Serialization/XamlPropertyIListEndNode.cs \
System.Windows.Serialization/XamlPropertyIListStartNode.cs \
System.Windows.Serialization/XamlPropertyNode.cs \
System.Windows.Serialization/XamlPropertyWithTypeNode.cs \
System.Windows.Serialization/XamlRoutedEventNode.cs \
System.Windows.Serialization/XamlTextNode.cs \
System.Windows.Serialization/XamlUnknownAttributeNode.cs \
System.Windows.Serialization/XamlUnknownTagEndNode.cs \
System.Windows.Serialization/XamlUnknownTagStartNode.cs

System.Windows.Serialization/XamlAttributeNode.cs, \
System.Windows.Serialization/XamlClrEventNode.cs, \
System.Windows.Serialization/XamlConstructorParametersEndNode.cs, \
System.Windows.Serialization/XamlConstructorParametersStartNode.cs, \
System.Windows.Serialization/XamlConstructorParameterTypeNode.cs, \
System.Windows.Serialization/XamlDefAttributeKeyTypeNode.cs, \
System.Windows.Serialization/XamlDefAttributeNode.cs, \
System.Windows.Serialization/XamlDefTagNode.cs, \
System.Windows.Serialization/XamlDocumentEndNode.cs, \
System.Windows.Serialization/XamlDocumentStartNode.cs, \
System.Windows.Serialization/XamlElementStartNode.cs, \
System.Windows.Serialization/XamlElementEndNode.cs, \
System.Windows.Serialization/XamlEndAttributesNode.cs, \
System.Windows.Serialization/XamlKeyElementEndNode.cs, \
System.Windows.Serialization/XamlKeyElementStartNode.cs, \
System.Windows.Serialization/XamlLanguageNode.cs, \
System.Windows.Serialization/XamlLiteralContentNode.cs, \
System.Windows.Serialization/XamlNode.cs, \
System.Windows.Serialization/XamlPIMappingNode.cs, \
System.Windows.Serialization/XamlPropertyArrayEndNode.cs, \
System.Windows.Serialization/XamlPropertyArrayStartNode.cs, \
System.Windows.Serialization/XamlPropertyComplexEndNode.cs, \
System.Windows.Serialization/XamlPropertyComplexStartNode.cs, \
System.Windows.Serialization/XamlPropertyIDictionaryEndNode.cs, \
System.Windows.Serialization/XamlPropertyIDictionaryStartNode.cs, \
System.Windows.Serialization/XamlPropertyIListEndNode.cs, \
System.Windows.Serialization/XamlPropertyIListStartNode.cs, \
System.Windows.Serialization/XamlPropertyNode.cs, \
System.Windows.Serialization/XamlPropertyWithTypeNode.cs, \
System.Windows.Serialization/XamlRoutedEventNode.cs, \
System.Windows.Serialization/XamlTextNode.cs, \
System.Windows.Serialization/XamlUnknownAttributeNode.cs, \
System.Windows.Serialization/XamlUnknownTagEndNode.cs, \
System.Windows.Serialization/XamlUnknownTagStartNode.cs: System.Windows.Serialization/data-classes.txt System.Windows.Serialization/data-classes-builder.pl
	(cd System.Windows.Serialization && perl data-classes-builder.pl)

include ../../build/library.make
