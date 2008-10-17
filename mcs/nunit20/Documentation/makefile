PREFIX=/usr
ASSEMBLER=../../../monodoc/browser/assembler.exe
UPDATER=../../../monodoc/generator/updater.exe

all: nunit-docs.zip nunit-docs.tree

nunit-docs.zip nunit-docs.tree: en/*/*.xml
	mono $(ASSEMBLER) --ecma en -o nunit-docs

install: nunit-docs.zip nunit-docs.tree
	install -m 644 nunit-docs.zip nunit-docs.tree `monodoc --get-sourcesdir`
	install -m 644 nunit-docs.source `monodoc --get-sourcesdir`

update:
	mono $(UPDATER) $(PREFIX)/lib/NUnit.Framework.dll -o ./en
	mono $(UPDATER) $(PREFIX)/lib/NUnit.Util.dll -o ./en

ignore:
	rm -rf en/NUnit.Core
	rm -f en/NUnit.Framework/AssertionFailureMessage.xml
	rm -f en/NUnit.Framework/SuiteAttribute.xml
	rm -f en/NUnit.Framework/TestFixtureSetUpAttribute.xml
	rm -f en/NUnit.Framework/TestFixtureTearDownAttribute.xml
	rm -f en/NUnit.Framework/TestCase.xml 
clean:
	rm -f *.tree
	rm -f *.zip
	rm -f tmp
	rm -f ..zip
