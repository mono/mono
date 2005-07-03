thisdir = tools/xaml/demo
include ../../../build/rules.make

SOURCES=TestVocab/ConsoleApp.cs TestVocab/ConsoleWriter.cs TestVocab/IConsoleAction.cs

run:
	make TestVocab.dll
	MONO_PATH="." $(RUNTIME) --debug ../xamlc.exe -o:test.xaml.out.cs test.xaml
	$(CSCOMPILE) -r:TestVocab.dll -o test.exe test.xaml.out.cs
	$(RUNTIME) --debug test.exe

all-local: TestVocab.dll

TestVocab.dll: $(SOURCES)
	$(CSCOMPILE) -r:PresentationFramework.dll -r:WindowsBase.dll -out:TestVocab.dll -target:library $(SOURCES)

