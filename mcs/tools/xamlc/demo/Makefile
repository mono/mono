thisdir = tools/xaml/demo
include ../../../build/rules.make

SOURCES=TestVocab/ConsoleApp.cs TestVocab/ConsoleWriter.cs TestVocab/IConsoleAction.cs

all-local: TestVocab.dll test.xaml.out

TestVocab.dll: $(SOURCES)
	$(CSCOMPILE) -r:PresentationFramework.dll -r:WindowsBase.dll -out:TestVocab.dll -target:library $(SOURCES)

test.xaml.out:
	make TestVocab.dll
	MONO_PATH="." $(RUNTIME) ../xamlc.exe -o:test.xaml.out test.xaml
