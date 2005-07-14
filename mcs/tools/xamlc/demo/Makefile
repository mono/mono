thisdir = tools/xaml/demo
include ../../../build/rules.make

SOURCES=TestVocab/ConsoleApp.cs TestVocab/ConsoleWriter.cs TestVocab/ConsoleReader.cs TestVocab/IConsoleAction.cs TestVocab/ConsoleValue.cs TestVocab/ConsoleVars.cs

run:
	make clean
	make TestVocab.dll
	MONO_PATH="." $(RUNTIME) --debug ../xamlc.exe -o:test.xaml.out.cs test.xaml
	$(CSCOMPILE) -r:TestVocab.dll -o test.exe test.xaml.out.cs
	echo "INPUTINPUT" | $(RUNTIME) --debug test.exe

run2:
	make runtimetest.exe
	$(RUNTIME) --debug runtimetest.exe

clean-local:
	rm -f TestVocab.dll test.xaml.out.cs

all-local: TestVocab.dll

TestVocab.dll: $(SOURCES)
	$(CSCOMPILE) -r:PresentationFramework.dll -r:WindowsBase.dll -out:TestVocab.dll -target:library $(SOURCES)


runtimetest.exe: runtimetest.cs TestVocab.dll
	$(CSCOMPILE) -r:PresentationFramework.dll -r:WindowsBase.dll -r:TestVocab.dll -out:runtimetest.exe runtimetest.cs
